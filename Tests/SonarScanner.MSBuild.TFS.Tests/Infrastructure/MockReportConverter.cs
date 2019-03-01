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

using FluentAssertions;

namespace SonarScanner.MSBuild.TFS.Tests.Infrastructure
{
    internal class MockReportConverter : ICoverageReportConverter
    {
        private int convertCallCount;

        #region Test helpers

        public bool CanConvert { get; set; }

        public bool ConversionResult { get; set; }

        #endregion Test helpers

        #region Assertions

        public void AssertExpectedNumberOfConversions(int expected)
        {
            convertCallCount.Should().Be(expected, "ConvertToXml called an unexpected number of times");
        }

        public void AssertConvertNotCalled()
        {
            convertCallCount.Should().Be(0, "Not expecting ConvertToXml to have been called");
        }

        #endregion Assertions

        #region ICoverageReportConverter interface

        bool ICoverageReportConverter.Initialize() => CanConvert;

        bool ICoverageReportConverter.ConvertToXml(string inputFilePath, string outputFilePath)
        {
            convertCallCount++;

            return true;
        }

        #endregion ICoverageReportConverter interface
    }
}
