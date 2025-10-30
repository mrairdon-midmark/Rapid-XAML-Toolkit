// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RapidXaml.Cli.Services;

namespace RapidXaml.Cli.Commands
{
    /// <summary>
    /// Command to analyze XAML files in a project.
    /// </summary>
    public static class AnalyzeCommand
    {
        /// <summary>
        /// Creates the analyze command.
        /// </summary>
        /// <returns>The analyze command.</returns>
        public static Command Create()
        {
            var command = new Command("analyze", "Analyze XAML files in a project for issues");

            var projectOption = new Option<FileInfo>(
                aliases: new[] { "--project", "-p" },
                description: "Path to the project file (.csproj or .vbproj)")
            {
                IsRequired = true
            };
            projectOption.AddValidator(result =>
            {
                var filePath = result.GetValueOrDefault<FileInfo>();
                if (filePath == null || !filePath.Exists)
                {
                    result.ErrorMessage = "Project file does not exist.";
                }
                else if (!filePath.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase) &&
                         !filePath.Extension.Equals(".vbproj", StringComparison.OrdinalIgnoreCase))
                {
                    result.ErrorMessage = "Project file must be a .csproj or .vbproj file.";
                }
            });
            command.AddOption(projectOption);

            var formatOption = new Option<OutputFormat>(
                aliases: new[] { "--format", "-f" },
                getDefaultValue: () => OutputFormat.MsBuild,
                description: "Output format (msbuild, json, console)");
            command.AddOption(formatOption);

            var errorOnlyOption = new Option<bool>(
                aliases: new[] { "--errors-only", "-e" },
                getDefaultValue: () => false,
                description: "Only output errors, not warnings");
            command.AddOption(errorOnlyOption);

            command.SetHandler(async (projectFile, format, errorsOnly) =>
            {
                await ExecuteAsync(projectFile, format, errorsOnly);
            }, projectOption, formatOption, errorOnlyOption);

            return command;
        }

        private static async Task<int> ExecuteAsync(FileInfo projectFile, OutputFormat format, bool errorsOnly)
        {
            try
            {
                var analyzer = new XamlAnalysisService();
                var results = await analyzer.AnalyzeProjectAsync(projectFile.FullName, errorsOnly);

                switch (format)
                {
                    case OutputFormat.MsBuild:
                        OutputMsBuildFormat(results);
                        break;
                    case OutputFormat.Json:
                        OutputJsonFormat(results);
                        break;
                    case OutputFormat.Console:
                        OutputConsoleFormat(results);
                        break;
                }

                return results.Any(r => r.IsError) ? 1 : 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error analyzing project: {ex.Message}");
                return 1;
            }
        }

        private static void OutputMsBuildFormat(IEnumerable<AnalysisResult> results)
        {
            foreach (var result in results)
            {
                var messageType = result.IsError ? "error" : "warning";
                Console.WriteLine($"{result.FilePath}({result.Line},{result.Column}): {messageType} {result.Code}: {result.Message} ({result.Code})");
            }
        }

        private static void OutputJsonFormat(IEnumerable<AnalysisResult> results)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(results, Newtonsoft.Json.Formatting.Indented);
            Console.WriteLine(json);
        }

        private static void OutputConsoleFormat(IEnumerable<AnalysisResult> results)
        {
            foreach (var result in results)
            {
                var messageType = result.IsError ? "ERROR" : "WARN";
                Console.WriteLine($"[{messageType}] {result.FilePath}:{result.Line}:{result.Column}");
                Console.WriteLine($"  {result.Code}: {result.Message}");
                Console.WriteLine();
            }
        }
    }

    /// <summary>
    /// Output format for analysis results.
    /// </summary>
    public enum OutputFormat
    {
        /// <summary>MSBuild-compatible format.</summary>
        MsBuild,

        /// <summary>JSON format.</summary>
        Json,

        /// <summary>Human-readable console format.</summary>
        Console
    }
}
