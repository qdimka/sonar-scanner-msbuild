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

using SonarScanner.MSBuild.TFS;
using SonarScanner.MSBuild.TFS.Interfaces;

namespace SonarScanner.MSBuild.PostProcessor.Tests
{
    internal class MockTeamBuildSettings : ITeamBuildSettings
    {
        public BuildEnvironment BuildEnvironment { get; set; }

        public string TfsUri { get; set; }

        public string BuildUri { get; set; }

        public string SourcesDirectory { get; set; }

        public string AnalysisBaseDirectory { get; set; }

        public string BuildDirectory { get; set; }

        public string SonarConfigDirectory { get; set; }

        public string SonarOutputDirectory { get; set; }

        public string SonarBinDirectory { get; set; }

        public string AnalysisConfigFilePath { get; set; }
    }
}
