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

namespace SonarScanner.MSBuild.PreProcessor.Roslyn
{
    /// <summary>
    /// Data class for a single SonarQube plugin containing an analyzer
    /// </summary>
    public class Plugin
    {
        public Plugin()
        {
        }

        public Plugin(string key, string version, string staticResourceName)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentNullException(nameof(version));
            }
            if (string.IsNullOrWhiteSpace(staticResourceName))
            {
                throw new ArgumentNullException(nameof(staticResourceName));
            }
            Key = key;
            Version = version;
            StaticResourceName = staticResourceName;
        }

        public string Key { get; set; }

        public string Version { get; set; }

        /// <summary>
        /// Name of the static resource in the plugin that contains the analyzer artifacts
        /// </summary>
        public string StaticResourceName { get; set; }
    }
}
