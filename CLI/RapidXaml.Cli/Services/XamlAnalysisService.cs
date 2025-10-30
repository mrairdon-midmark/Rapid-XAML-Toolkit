// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RapidXamlToolkit;
using RapidXamlToolkit.XamlAnalysis;
using RapidXamlToolkit.XamlAnalysis.Tags;

namespace RapidXaml.Cli.Services
{
    /// <summary>
    /// Service for analyzing XAML files.
    /// </summary>
    public class XamlAnalysisService
    {
        /// <summary>
        /// Analyzes all XAML files in a project.
        /// </summary>
        /// <param name="projectPath">Path to the project file.</param>
        /// <param name="errorsOnly">Whether to only return errors.</param>
        /// <returns>List of analysis results.</returns>
        public Task<List<AnalysisResult>> AnalyzeProjectAsync(string projectPath, bool errorsOnly)
        {
            var results = new List<AnalysisResult>();
            var logger = new CliLogger();

            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException("Project file not found.", projectPath);
            }

            var projFileLines = File.ReadAllLines(projectPath);
            var projDir = Path.GetDirectoryName(projectPath);

            // Treat project type as unknown as unable to resolve referenced projects and installed NuGet packages.
            var vsa = new CliVisualStudioAbstraction(projectPath, ProjectType.Unknown);

            foreach (var line in projFileLines)
            {
                var endPos = line.IndexOf(".xaml\"", StringComparison.OrdinalIgnoreCase);
                if (endPos > 1)
                {
                    var startPos = line.IndexOf("Include", StringComparison.OrdinalIgnoreCase);

                    if (startPos > 1)
                    {
                        var relativeFilePath = line.Substring(startPos + 9, endPos + 5 - startPos - 9);
                        var xamlFilePath = Path.Combine(projDir ?? string.Empty, relativeFilePath);

                        if (File.Exists(xamlFilePath))
                        {
                            var snapshot = new CliTextSnapshot(xamlFilePath);
                            var rxdoc = RapidXamlDocument.Create(snapshot, xamlFilePath, vsa, projectPath, logger);

                            var tagsOfInterest = rxdoc.Tags
                                .Where(t => t is RapidXamlDisplayedTag)
                                .Cast<RapidXamlDisplayedTag>()
                                .ToList();

                            foreach (var issue in tagsOfInterest)
                            {
                                bool isError = issue.ConfiguredErrorType == TagErrorType.Error;
                                bool isWarning = issue.ConfiguredErrorType == TagErrorType.Warning;

                                if ((isError || isWarning) && (!errorsOnly || isError))
                                {
                                    results.Add(new AnalysisResult
                                    {
                                        FilePath = xamlFilePath,
                                        Line = issue.Line + 1, // Add 1 to match VS line numbering
                                        Column = issue.Column,
                                        Code = issue.ErrorCode,
                                        Message = issue.Description,
                                        IsError = isError
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return Task.FromResult(results);
        }
    }

    /// <summary>
    /// Represents a single analysis result.
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>Gets or sets the file path.</summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>Gets or sets the line number.</summary>
        public int Line { get; set; }

        /// <summary>Gets or sets the column number.</summary>
        public int Column { get; set; }

        /// <summary>Gets or sets the error code.</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Gets or sets the message.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Gets or sets a value indicating whether this is an error (vs warning).</summary>
        public bool IsError { get; set; }
    }
}
