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

namespace SonarScanner.MSBuild.Tasks.IntegrationTests
{
    internal static class TargetConstants
    {
        // Target file names
        public const string AnalysisTargetFile = TestUtilities.TestUtils.AnalysisTargetFile;

        public const string ImportsBeforeFile = TestUtilities.TestUtils.ImportsBeforeFile;

        // Targets
        public const string ImportBeforeInfoTarget = "SonarQubeImportBeforeInfo";

        public const string CategoriseProjectTarget = "SonarQubeCategoriseProject";
        public const string CalculateFilesToAnalyzeTarget = "CalculateSonarQubeFilesToAnalyze";
        public const string CreateProjectSpecificDirs = "CreateProjectSpecificDirs";
        public const string WriteProjectDataTarget = "WriteSonarQubeProjectData";

        public const string DefaultBuildTarget = "Build";
        public const string CoreCompile = "CoreCompile";

        // Roslyn
        public const string OverrideRoslynAnalysisTarget = "OverrideRoslynCodeAnalysisProperties";

        public const string SetRoslynAnalysisPropertiesTarget = "SetRoslynCodeAnalysisProperties";
        public const string MergeResultSetsTask = "MergeRuleSets";

        public const string SetRoslynResultsTarget = "SetRoslynAnalysisResults";
        public const string ResolveCodeAnalysisRuleSet = "ResolveCodeAnalysisRuleSet";

        public const string MsTestProjectTypeGuid = "3AC096D0-A1C2-E12C-1390-A8335801FDAB";
    }

    internal static class TargetProperties
    {
        // SonarQube Integration constants
        public const string SonarQubeTargetFilePath = "SonarQubeTargetFilePath";

        public const string SonarQubeTargetsPath = "SonarQubeTargetsPath";

        public const string SonarQubeConfigPath = "SonarQubeConfigPath";
        public const string SonarQubeOutputPath = "SonarQubeOutputPath";
        public const string SonarQubeTempPath = "SonarQubeTempPath";
        public const string SonarBuildTasksAssemblyFile = "SonarQubeBuildTasksAssemblyFile";

        public const string SonarQubeTestProject = "SonarQubeTestProject";
        public const string SonarQubeExcludeMetadata = "SonarQubeExclude";

        public const string MergedRulesetFullName = "MergedRulesetFullName";

        public const string SonarCompileErrorLog = "SonarCompileErrorLog";
        public const string RazorSonarCompileErrorLog = "RazorSonarCompileErrorLog";

        // Non-SonarQube constants
        public const string ProjectGuid = "ProjectGuid";

        public const string ProjectTypeGuids = "ProjectTypeGuids";

        public const string RunCodeAnalysis = "RunCodeAnalysis";
        public const string CodeAnalysisRuleset = "CodeAnalysisRuleSet";
        public const string CodeAnalysisLogFile = "CodeAnalysisLogFile";

        public const string AssemblyName = "AssemblyName";

        public const string TreatWarningsAsErrors = "TreatWarningsAsErrors";
        public const string WarningsAsErrors = "WarningsAsErrors";
        public const string WarningLevel = "WarningLevel";

        public const string IsInTeamBuild = "TF_Build"; // Common to legacy and non-legacy TeamBuilds
        public const string ProjectName = "MSBuildProjectName";

        public const string BuildingInsideVS = "BuildingInsideVisualStudio";

        public const string CodePage = "CodePage";

        // Roslyn
        public const string ResolvedCodeAnalysisRuleset = "ResolvedCodeAnalysisRuleSet";

        public const string TargetDir = "TargetDir"; // bin directory into which output will be dropped
        public const string TargetFileName = "TargetFileName"; // filename and extension of the project being built
        public const string ErrorLog = "ErrorLog"; // file path to which the Roslyn error log should be written
        public const string Language = "Language"; // Language of the project: normally "C#" or "VB"
        public const string AnalyzerItemType = "Analyzer";
        public const string AdditionalFilesItemType = "AdditionalFiles";
        public const string ProjectConfFilePath = "ProjectConfFilePath";

        public const string ProjectSpecificOutDir = "ProjectSpecificOutDir";
        public const string ProjectSpecificConfDir = "ProjectSpecificConfDir";

        // Legacy TeamBuild environment variables (XAML Builds)
        public const string TfsCollectionUri_Legacy = "TF_BUILD_COLLECTIONURI";

        public const string BuildUri_Legacy = "TF_BUILD_BUILDURI";
        public const string BuildDirectory_Legacy = "TF_BUILD_BUILDDIRECTORY";

        // TFS 2015 Environment variables
        public const string TfsCollectionUri_TFS2015 = "SYSTEM_TEAMFOUNDATIONCOLLECTIONURI";

        public const string BuildUri_TFS2015 = "BUILD_BUILDURI";
        public const string BuildDirectory_TFS2015 = "AGENT_BUILDDIRECTORY";

        public const string MSBuildExtensionsPath = "MSBuildExtensionsPath";

        public const string ItemType_Compile = "Compile";
        public const string ItemType_Content = "Content";
        public const string AutoGenMetadata = "AutoGen";
    }
}
