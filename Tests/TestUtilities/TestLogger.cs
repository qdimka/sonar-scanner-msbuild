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
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarScanner.MSBuild.Common;

namespace TestUtilities
{
    public class TestLogger : ILogger
    {
        public List<string> DebugMessages { get; private set; }
        public List<string> InfoMessages { get; private set; }
        public List<string> Warnings { get; private set; }
        public List<string> Errors { get; private set; }

        public LoggerVerbosity Verbosity
        {
            get; set;
        }

        public bool IncludeTimestamp
        {
            get; set;
        }

        public TestLogger()
        {
            // Write out a separator. Many tests create more than one TestLogger.
            // This helps separate the results of the different cases.
            WriteLine("");
            WriteLine("------------------------------------------------------------- (new TestLogger created)");
            WriteLine("");

            DebugMessages = new List<string>();
            InfoMessages = new List<string>();
            Warnings = new List<string>();
            Errors = new List<string>();

            Verbosity = LoggerVerbosity.Debug;
        }

        #region Public methods

        public void AssertErrorsLogged()
        {
            Errors.Count.Should().BePositive("Expecting at least one error to be logged");
        }

        public void AssertMessagesLogged()
        {
            InfoMessages.Count.Should().BePositive("Expecting at least one message to be logged");
        }

        public void AssertErrorsLogged(int expectedCount)
        {
            Errors.Should().HaveCount(expectedCount, "Unexpected number of errors logged");
        }

        public void AssertWarningsLogged(int expectedCount)
        {
            Warnings.Should().HaveCount(expectedCount, "Unexpected number of warnings logged");
        }

        public void AssertMessagesLogged(int expectedCount)
        {
            InfoMessages.Should().HaveCount(expectedCount, "Unexpected number of messages logged");
        }

        public void AssertDebugLogged(string expected)
        {
            DebugMessages.Should().Contain(expected);
        }

        public void AssertMessageLogged(string expected)
        {
            InfoMessages.Should().Contain(expected);
        }

        public void AssertErrorLogged(string expected)
        {
            Errors.Should().Contain(expected);
        }

        public void AssertMessageNotLogged(string message)
        {
            var found = InfoMessages.Any(s => message.Equals(s, System.StringComparison.CurrentCulture));
            found.Should().BeFalse("Not expecting the message to have been logged: '{0}'", message);
        }

        public void AssertWarningNotLogged(string warning)
        {
            var found = Warnings.Any(s => warning.Equals(s, System.StringComparison.CurrentCulture));
            found.Should().BeFalse("Not expecting the warning to have been logged: '{0}'", warning);
        }

        /// <summary>
        /// Checks that a single error exists that contains all of the specified strings
        /// </summary>
        public void AssertSingleErrorExists(params string[] expected)
        {
            var matches = Errors.Where(w => expected.All(e => w.Contains(e)));
            matches.Should().ContainSingle("More than one error contains the expected strings: {0}", string.Join(",", expected));
        }

        /// <summary>
        /// Checks that a single warning exists that contains all of the specified strings
        /// </summary>
        public void AssertSingleWarningExists(params string[] expected)
        {
            var matches = Warnings.Where(w => expected.All(e => w.Contains(e)));
            matches.Should().ContainSingle("More than one warning contains the expected strings: {0}", string.Join(",", expected));
        }

        /// <summary>
        /// Checks that a single INFO message exists that contains all of the specified strings
        /// </summary>
        public string AssertSingleInfoMessageExists(params string[] expected)
        {
            var matches = InfoMessages.Where(m => expected.All(e => m.Contains(e)));
            matches.Should().ContainSingle("More than one INFO message contains the expected strings: {0}", string.Join(",", expected));
            return matches.First();
        }

        /// <summary>
        /// Checks that a single DEBUG message exists that contains all of the specified strings
        /// </summary>
        public string AssertSingleDebugMessageExists(params string[] expected)
        {
            var matches = DebugMessages.Where(m => expected.All(e => m.Contains(e)));
            matches.Should().ContainSingle("More than one DEBUG message contains the expected strings: {0}", string.Join(",", expected));
            return matches.First();
        }

        /// <summary>
        /// Checks that at least one INFO message exists that contains all of the specified strings
        /// </summary>
        public void AssertInfoMessageExists(params string[] expected)
        {
            var matches = InfoMessages.Where(m => expected.All(e => m.Contains(e)));
            matches.Should().NotBeEmpty("No INFO message contains the expected strings: {0}", string.Join(",", expected));
        }

        /// <summary>
        /// Checks that at least one DEBUG message exists that contains all of the specified strings
        /// </summary>
        public void AssertDebugMessageExists(params string[] expected)
        {
            var matches = DebugMessages.Where(m => expected.All(e => m.Contains(e)));
            matches.Should().NotBeEmpty("No DEBUG message contains the expected strings: {0}", string.Join(",", expected));
        }

        /// <summary>
        /// Checks that an error that contains all of the specified strings does not exist
        /// </summary>
        public void AssertErrorDoesNotExist(params string[] expected)
        {
            var matches = Errors.Where(w => expected.All(e => w.Contains(e)));
            matches.Should().BeEmpty("Not expecting any errors to contain the specified strings: {0}", string.Join(",", expected));
        }

        public void AssertVerbosity(LoggerVerbosity expected)
        {
            Verbosity.Should().Be(expected, "Logger verbosity mismatch");
        }

        #endregion Public methods

        #region ILogger interface

        public void LogInfo(string message, params object[] args)
        {
            InfoMessages.Add(GetFormattedMessage(message, args));
            WriteLine("INFO: " + message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            Warnings.Add(GetFormattedMessage(message, args));
            WriteLine("WARNING: " + message, args);
        }

        public void LogError(string message, params object[] args)
        {
            Errors.Add(GetFormattedMessage(message, args));
            WriteLine("ERROR: " + message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            DebugMessages.Add(GetFormattedMessage(message, args));
            WriteLine("DEBUG: " + message, args);
        }

        public void SuspendOutput()
        {
            // no-op
        }

        public void ResumeOutput()
        {
            // no-op
        }

        #endregion ILogger interface

        #region Private methods

        private static void WriteLine(string message, params object[] args)
        {
            Console.WriteLine(GetFormattedMessage(message, args));
        }

        private static string GetFormattedMessage(string message, params object[] args)
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, message, args);
        }

        #endregion Private methods
    }
}
