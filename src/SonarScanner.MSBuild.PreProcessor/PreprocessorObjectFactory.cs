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
using SonarScanner.MSBuild.Common;

namespace SonarScanner.MSBuild.PreProcessor
{
    /// <summary>
    /// Default implementation of the object factory interface that returns the
    /// product implementations of the required classes
    /// </summary>
    /// <remarks>
    /// Note: the factory is stateful and expects objects to be requested in the
    /// order they are used
    /// </remarks>
    public class PreprocessorObjectFactory : IPreprocessorObjectFactory
    {
        /// <summary>
        /// Reference to the SonarQube server to query
        /// </summary>
        /// <remarks>Cannot be constructed at runtime until the command line arguments have been processed.
        /// Once it has been created, it is stored so the factory can use the same instance when
        /// constructing the analyzer provider</remarks>
        private ISonarQubeServer server;
        private readonly ILogger logger;

        public PreprocessorObjectFactory(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region IPreprocessorObjectFactory methods

        public ISonarQubeServer CreateSonarQubeServer(ProcessedArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var username = args.GetSetting(SonarProperties.SonarUserName, null);
            var password = args.GetSetting(SonarProperties.SonarPassword, null);
            var hostUrl = args.SonarQubeUrl;

            this.server = new SonarWebService(new WebClientDownloader(username, password, this.logger), hostUrl, this.logger);
            return this.server;
        }

        public ITargetsInstaller CreateTargetInstaller()
        {
            return new TargetsInstaller(this.logger);
        }

        public IAnalyzerProvider CreateRoslynAnalyzerProvider()
        {
            if (this.server == null)
            {
                throw new InvalidOperationException(Resources.FACTORY_InternalError_MissingServer);
            }

            return new Roslyn.RoslynAnalyzerProvider(new Roslyn.EmbeddedAnalyzerInstaller(this.server, this.logger), this.logger);
        }

        #endregion IPreprocessorObjectFactory methods
    }
}
