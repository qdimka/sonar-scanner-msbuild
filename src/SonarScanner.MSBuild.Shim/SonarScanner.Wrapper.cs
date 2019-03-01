﻿/*
 * SonarScanner for MSBuild
 * Copyright (C) 2016-2019 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SonarScanner.MSBuild.Common;

namespace SonarScanner.MSBuild.Shim
{
    public class SonarScannerWrapper : ISonarScanner
    {
        /// <summary>
        /// Env variable that controls the amount of memory the JVM can use for the sonar-scanner.
        /// </summary>
        /// <remarks>Large projects error out with OutOfMemoryException if not set</remarks>
        private const string SonarScannerOptsVariableName = "SONAR_SCANNER_OPTS";

        /// <summary>
        /// Env variable that locates the sonar-scanner
        /// </summary>
        /// <remarks>Existing values set by the user might cause failures/remarks>
        public const string SonarScannerHomeVariableName = "SONAR_SCANNER_HOME";

        /// <summary>
        /// Name of the command line argument used to specify the generated project settings file to use
        /// </summary>
        public const string ProjectSettingsFileArgName = "project.settings";

        private const string CmdLineArgPrefix = "-D";

        // This version needs to be in sync with version in src/Packaging/Directory.Build.props.
        private const string SonarScannerVersion = "3.3.0.1492";

        private readonly ILogger logger;

        public SonarScannerWrapper(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region ISonarScanner interface

        public ProjectInfoAnalysisResult Execute(AnalysisConfig config, IEnumerable<string> userCmdLineArguments)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (userCmdLineArguments == null)
            {
                throw new ArgumentNullException(nameof(userCmdLineArguments));
            }

            var result = new PropertiesFileGenerator(config, logger).GenerateFile();
            Debug.Assert(result != null, "Not expecting the file generator to return null");
            result.RanToCompletion = false;

            SonarProjectPropertiesValidator.Validate(
                config.SonarScannerWorkingDirectory,
                result.Projects,
                onValid: () =>
                {
                    ProjectInfoReportBuilder.WriteSummaryReport(config, result, logger);

                    result.RanToCompletion = InternalExecute(config, userCmdLineArguments, logger, result.FullPropertiesFilePath);
                },
                onInvalid: (invalidFolders) =>
                {
                    // LOG error message
                    logger.LogError(Resources.ERR_ConflictingSonarProjectProperties, string.Join(", ", invalidFolders));
                });

            return result;
        }

        #endregion ISonarScanner interface

        #region Private methods

        private static bool InternalExecute(AnalysisConfig config, IEnumerable<string> userCmdLineArguments, ILogger logger, string fullPropertiesFilePath)
        {
            if (fullPropertiesFilePath == null)
            {
                // We expect a detailed error message to have been logged explaining
                // why the properties file generation could not be performed
                logger.LogInfo(Resources.MSG_PropertiesGenerationFailed);
                return false;
            }

            var exeFileName = FindScannerExe();
            return ExecuteJavaRunner(config, userCmdLineArguments, logger, exeFileName, fullPropertiesFilePath, new ProcessRunner(logger));
        }

        private static string FindScannerExe()
        {
            var binFolder = Path.GetDirectoryName(typeof(SonarScannerWrapper).Assembly.Location);
            var fileExtension = PlatformHelper.IsWindows() ? ".bat" : "";
            return Path.Combine(binFolder, $"sonar-scanner-{SonarScannerVersion}", "bin", $"sonar-scanner{fileExtension}");
        }

        public /* for test purposes */ static bool ExecuteJavaRunner(AnalysisConfig config, IEnumerable<string> userCmdLineArguments, ILogger logger, string exeFileName, string propertiesFileName, IProcessRunner runner)
        {
            Debug.Assert(File.Exists(exeFileName), "The specified exe file does not exist: " + exeFileName);
            Debug.Assert(File.Exists(propertiesFileName), "The specified properties file does not exist: " + propertiesFileName);

            IgnoreSonarScannerHome(logger);

            var allCmdLineArgs = GetAllCmdLineArgs(propertiesFileName, userCmdLineArguments, config, logger);

            var envVarsDictionary = GetAdditionalEnvVariables(logger);
            Debug.Assert(envVarsDictionary != null);

            logger.LogInfo(Resources.MSG_SonarScannerCalling);

            Debug.Assert(!string.IsNullOrWhiteSpace(config.SonarScannerWorkingDirectory), "The working dir should have been set in the analysis config");
            Debug.Assert(Directory.Exists(config.SonarScannerWorkingDirectory), "The working dir should exist");

            var scannerArgs = new ProcessRunnerArguments(exeFileName, PlatformHelper.IsWindows())
            {
                CmdLineArgs = allCmdLineArgs,
                WorkingDirectory = config.SonarScannerWorkingDirectory,
                EnvironmentVariables = envVarsDictionary
            };

            // SONARMSBRU-202 Note that the Sonar Scanner may write warnings to stderr so
            // we should only rely on the exit code when deciding if it ran successfully
            var success = runner.Execute(scannerArgs);

            if (success)
            {
                logger.LogInfo(Resources.MSG_SonarScannerCompleted);
            }
            else
            {
                logger.LogError(Resources.ERR_SonarScannerExecutionFailed);
            }
            return success;
        }

        private static void IgnoreSonarScannerHome(ILogger logger)
        {
            if (!string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(SonarScannerHomeVariableName)))
            {
                logger.LogInfo(Resources.MSG_SonarScannerHomeIsSet);
                Environment.SetEnvironmentVariable(SonarScannerHomeVariableName, string.Empty);
            }
        }

        /// <summary>
        /// Returns any additional environment variables that need to be passed to
        /// the sonar-scanner
        /// </summary>
        private static IDictionary<string, string> GetAdditionalEnvVariables(ILogger logger)
        {
            IDictionary<string, string> envVarsDictionary = new Dictionary<string, string>();

            // If there is a value for SONAR_SCANNER_OPTS then pass it through explicitly just in case it is
            // set at process-level (which wouldn't otherwise be inherited by the child sonar-scanner process)
            var sonarScannerOptsValue = Environment.GetEnvironmentVariable(SonarScannerOptsVariableName);
            if (sonarScannerOptsValue != null)
            {
                envVarsDictionary.Add(SonarScannerOptsVariableName, sonarScannerOptsValue);
                logger.LogInfo(Resources.MSG_UsingSuppliedSonarScannerOptsValue, SonarScannerOptsVariableName, sonarScannerOptsValue);
            }

            return envVarsDictionary;
        }

        /// <summary>
        /// Returns all of the command line arguments to pass to sonar-scanner
        /// </summary>
        private static IEnumerable<string> GetAllCmdLineArgs(string projectSettingsFilePath,
            IEnumerable<string> userCmdLineArguments, AnalysisConfig config, ILogger logger)
        {
            // We don't know what all of the valid command line arguments are so we'll
            // just pass them on for the sonar-scanner to validate.
            var args = new List<string>(userCmdLineArguments);

            // Add any sensitive arguments supplied in the config should be passed on the command line
            args.AddRange(GetSensitiveFileSettings(config, userCmdLineArguments));

            // Add the project settings file and the standard options.
            // Experimentation suggests that the sonar-scanner won't error if duplicate arguments
            // are supplied - it will just use the last argument.
            // So we'll set our additional properties last to make sure they take precedence.
            args.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}={2}", CmdLineArgPrefix,
                ProjectSettingsFileArgName, projectSettingsFilePath));

            // Let the scanner cli know it is run as an embedded tool (allows to tweak the behavior)
            // See https://jira.sonarsource.com/browse/SQSCANNER-49
            args.Add("--embedded");

            // For debug mode, we need to pass the debug option to the scanner cli in order to see correctly stack traces.
            // Note that in addition to this change, the sonar.verbose=true was removed from the config file.
            // See: https://github.com/SonarSource/sonar-scanner-msbuild/issues/543
            if (logger.Verbosity == LoggerVerbosity.Debug)
            {
                args.Add("--debug");
            }

            return args;
        }

        private static IEnumerable<string> GetSensitiveFileSettings(AnalysisConfig config, IEnumerable<string> userCmdLineArguments)
        {
            var allPropertiesFromConfig = config.GetAnalysisSettings(false).GetAllProperties();

            return allPropertiesFromConfig.Where(p => p.ContainsSensitiveData() && !UserSettingExists(p, userCmdLineArguments))
                .Select(p => p.AsSonarScannerArg());
        }

        private static bool UserSettingExists(Property fileProperty, IEnumerable<string> userArgs)
        {
            return userArgs.Any(userArg => userArg.IndexOf(CmdLineArgPrefix + fileProperty.Id, StringComparison.Ordinal) == 0);
        }

        #endregion Private methods
    }
}
