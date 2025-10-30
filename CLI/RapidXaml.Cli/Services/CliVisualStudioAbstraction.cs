// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using RapidXamlToolkit;
using RapidXamlToolkit.XamlAnalysis;

namespace RapidXaml.Cli.Services
{
    /// <summary>
    /// CLI implementation of Visual Studio abstraction for XAML analysis.
    /// </summary>
    public class CliVisualStudioAbstraction : IVisualStudioAbstraction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CliVisualStudioAbstraction"/> class.
        /// </summary>
        /// <param name="projectFilePath">Path to the project file.</param>
        /// <param name="projectType">Type of project.</param>
        public CliVisualStudioAbstraction(string projectFilePath, ProjectType projectType)
        {
            this.ProjectFilePath = projectFilePath;
            this.ProjectType = projectType;
        }

        /// <inheritdoc/>
        public string ProjectFilePath { get; }

        /// <inheritdoc/>
        public ProjectType ProjectType { get; }

        /// <inheritdoc/>
        public List<string> GetAccessibleAttributes(string elementName, SemanticModelAccessor semanticModel, string xmlns)
        {
            // Return empty list as we don't have IntelliSense data in CLI
            return new List<string>();
        }
    }
}
