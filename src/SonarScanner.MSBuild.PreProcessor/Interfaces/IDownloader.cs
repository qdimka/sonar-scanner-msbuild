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

namespace SonarScanner.MSBuild.PreProcessor
{
    /// <summary>
    /// Interface introduced for testability
    /// </summary>
    public interface IDownloader : IDisposable
    {
        /// <summary>
        /// Attempts to download the specified page
        /// </summary>
        /// <returns>False if the url does not exist, true if the contents were downloaded successfully.
        /// Exceptions are thrown for other web failures.</returns>
        bool TryDownloadIfExists(string url, out string contents);

        /// <summary>
        /// Attempts to download the specified file
        /// </summary>
        /// <param name="targetFilePath">The file to which the downloaded data should be saved</param>
        /// <returns>False if the url does not exist, true if the data was downloaded successfully.
        /// Exceptions are thrown for other web failures.</returns>
        bool TryDownloadFileIfExists(string url, string targetFilePath);

        string Download(string url);
    }
}
