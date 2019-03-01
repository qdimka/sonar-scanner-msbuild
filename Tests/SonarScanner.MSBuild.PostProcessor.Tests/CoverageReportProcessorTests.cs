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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarScanner.MSBuild.Common;
using SonarScanner.MSBuild.TFS;
using SonarScanner.MSBuild.TFS.Interfaces;
using TestUtilities;

namespace SonarScanner.MSBuild.PostProcessor.Tests
{
    [TestClass]
    public class CoverageReportProcessorTests
    {
        private Mock<ILegacyTeamBuildFactory> legacyFactoryMock;
        private Mock<ICoverageReportConverter> converterMock;
        private CoverageReportProcessor processor;

        [TestInitialize]
        public void TestInitialize()
        {
            converterMock = new Mock<ICoverageReportConverter>(MockBehavior.Strict);
            legacyFactoryMock = new Mock<ILegacyTeamBuildFactory>(MockBehavior.Strict);
            processor = new CoverageReportProcessor(legacyFactoryMock.Object, converterMock.Object, new TestLogger());
        }

        public void Ctor_WhenLegacyFactoryIsNull_Throws()
        {
            // Arrange
            Action act = () => new CoverageReportProcessor(null, converterMock.Object, new TestLogger());

            // Act & Assert
            act.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("legactTeamBuildFactory");
        }

        public void Ctor_WhenConverterIsNull_Throws()
        {
            // Arrange
            Action act = () => new CoverageReportProcessor(legacyFactoryMock.Object, null, new TestLogger());

            // Act & Assert
            act.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("coverageReportConverter");
        }

        public void Ctor_WhenLoggerIsNull_Throws()
        {
            // Arrange
            Action act = () => new CoverageReportProcessor(legacyFactoryMock.Object, converterMock.Object, null);

            // Act & Assert
            act.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        [TestMethod]
        public void Initialize_Checks_Arguments_For_Null()
        {
            // Arrange
            var analysisConfig = new AnalysisConfig();
            Action act = () => processor.Initialise(analysisConfig, null);

            // Act & Assert
            act.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("settings");
        }

        [TestMethod]
        public void ProcessCoverageReports_LegacyTeamBuild_SkipCoverageIsFalse_WhenProcess_CallsLegacyFactoryThenCallsReturnedProcessor()
        {
            // Arrange
            var analysisConfig = new AnalysisConfig { LocalSettings = new AnalysisProperties() };
            var settingsMock = new MockTeamBuildSettings { BuildEnvironment = BuildEnvironment.LegacyTeamBuild };
            var logger = new TestLogger();

            // Set up the factory to return a processor that returns success
            var processorMock = new Mock<ICoverageReportProcessor>();
            processorMock.Setup(x => x.Initialise(It.IsAny<AnalysisConfig>(), It.IsAny<ITeamBuildSettings>())).Returns(true);
            processorMock.Setup(x => x.ProcessCoverageReports()).Returns(true);
            legacyFactoryMock.Setup(x => x.BuildTfsLegacyCoverageReportProcessor()).Returns(processorMock.Object);

            using (var scope = new EnvironmentVariableScope())
            {
                scope.SetVariable(TeamBuildSettings.EnvironmentVariables.SkipLegacyCodeCoverage, "false");

                var testSubject = new CoverageReportProcessor(legacyFactoryMock.Object, converterMock.Object, logger);
                testSubject.Initialise(analysisConfig, settingsMock);

                // Act
                var result = testSubject.ProcessCoverageReports();

                // Assert
                result.Should().BeTrue();
                legacyFactoryMock.Verify(x => x.BuildTfsLegacyCoverageReportProcessor(), Times.Once);
                processorMock.Verify(x => x.Initialise(It.IsAny<AnalysisConfig>(), It.IsAny<ITeamBuildSettings>()), Times.Once);
                processorMock.Verify(x => x.ProcessCoverageReports(), Times.Once);
            }
        }

        [TestMethod]
        public void ProcessCoverageReports_LegacyTeamBuild_SkipCoverageIsTrue_WhenProcess_CallsLegacyFactoryThenCallsReturnedProcessor()
        {
            // Arrange
            var analysisConfig = new AnalysisConfig { LocalSettings = new AnalysisProperties() };
            var settingsMock = new MockTeamBuildSettings { BuildEnvironment = BuildEnvironment.LegacyTeamBuild };
            var logger = new TestLogger();

            using (var scope = new EnvironmentVariableScope())
            {
                scope.SetVariable(TeamBuildSettings.EnvironmentVariables.SkipLegacyCodeCoverage, "true");

                var testSubject = new CoverageReportProcessor(legacyFactoryMock.Object, converterMock.Object, logger);
                testSubject.Initialise(analysisConfig, settingsMock);

                // Act
                var result = false;
                using (new AssertIgnoreScope())
                {
                    result = testSubject.ProcessCoverageReports();
                }

                // Assert
                result.Should().BeTrue();
                legacyFactoryMock.Verify(x => x.BuildTfsLegacyCoverageReportProcessor(), Times.Never);
            }
        }

        [TestMethod]
        public void ProcessCoverageReports_Standalone_WhenProcess_ReturnsTrue()
        {
            // Arrange
            var analysisConfig = new AnalysisConfig { LocalSettings = new AnalysisProperties() };
            var settings = new MockTeamBuildSettings { BuildEnvironment = BuildEnvironment.NotTeamBuild };
            var logger = new TestLogger();

            var testSubject = new CoverageReportProcessor(legacyFactoryMock.Object, converterMock.Object, logger);
            testSubject.Initialise(analysisConfig, settings);

            // Act
            var result = false;
            using (new AssertIgnoreScope())
            {
                result = testSubject.ProcessCoverageReports();
            }

            // Assert
            result.Should().BeTrue(); // false would cause the remaining processing to stop
            legacyFactoryMock.Verify(x => x.BuildTfsLegacyCoverageReportProcessor(), Times.Never);
        }
    }
}
