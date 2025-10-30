// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

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

            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException("Project file not found.", projectPath);
            }

            var projFileLines = File.ReadAllLines(projectPath);
            var projDir = Path.GetDirectoryName(projectPath);

            // Find XAML files in project
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
                            // Perform basic XAML validation
                            var fileResults = AnalyzeXamlFile(xamlFilePath, errorsOnly);
                            results.AddRange(fileResults);
                        }
                    }
                }
            }

            return Task.FromResult(results);
        }

        private List<AnalysisResult> AnalyzeXamlFile(string xamlFilePath, bool errorsOnly)
        {
            var results = new List<AnalysisResult>();

            try
            {
                // Try to parse as XML to catch syntax errors
                var xamlContent = File.ReadAllText(xamlFilePath);
                var doc = XDocument.Parse(xamlContent);

                // Basic validation - check for common issues
                // This is a simplified version - the full RapidXaml.AnalysisCore would do more
                var lines = xamlContent.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    
                    // Example: Check for hardcoded strings (warning)
                    if (line.Contains("Text=\"") && !line.Contains("{Binding") && !line.Contains("{StaticResource"))
                    {
                        if (!errorsOnly)
                        {
                            results.Add(new AnalysisResult
                            {
                                FilePath = xamlFilePath,
                                Line = i + 1,
                                Column = line.IndexOf("Text=\"", StringComparison.Ordinal) + 1,
                                Code = "RXT101",
                                Message = "Consider using data binding or resources instead of hardcoded text",
                                IsError = false
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // XML parsing error
                results.Add(new AnalysisResult
                {
                    FilePath = xamlFilePath,
                    Line = 1,
                    Column = 1,
                    Code = "RXT001",
                    Message = $"XAML parsing error: {ex.Message}",
                    IsError = true
                });
            }

            return results;
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
