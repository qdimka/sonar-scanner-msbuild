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
using SonarScanner.MSBuild.TFS;

namespace SonarScanner.MSBuild
{
    public class NotSupportedLegacyTeamBuildFactory : ILegacyTeamBuildFactory
    {
        private const string message
            = "Legacy XAML builds are not supported by .NET Core version of Scanner for MSBuild. " +
              "Please use the .NET Framework executable instead.";

        public ILegacyBuildSummaryLogger BuildLegacyBuildSummaryLogger(string tfsUri, string buildUri)
            => throw new NotSupportedException(message);

        public ICoverageReportProcessor BuildTfsLegacyCoverageReportProcessor()
            => throw new NotSupportedException(message);
    }
}
