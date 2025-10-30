// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using RapidXaml.Cli.Services;

namespace RapidXaml.Cli.Commands
{
    /// <summary>
    /// Command to generate XAML from C#/VB.NET code.
    /// </summary>
    public static class GenerateCommand
    {
        /// <summary>
        /// Creates the generate command.
        /// </summary>
        /// <returns>The generate command.</returns>
        public static Command Create()
        {
            var command = new Command("generate", "Generate XAML from C#/VB.NET ViewModel code");

            var inputOption = new Option<FileInfo>(
                aliases: new[] { "--input", "-i" },
                description: "Path to the C# or VB.NET file containing ViewModel code")
            {
                IsRequired = true
            };
            inputOption.AddValidator(result =>
            {
                var filePath = result.GetValueOrDefault<FileInfo>();
                if (filePath == null || !filePath.Exists)
                {
                    result.ErrorMessage = "Input file does not exist.";
                }
                else if (!filePath.Extension.Equals(".cs", StringComparison.OrdinalIgnoreCase) &&
                         !filePath.Extension.Equals(".vb", StringComparison.OrdinalIgnoreCase))
                {
                    result.ErrorMessage = "Input file must be a .cs or .vb file.";
                }
            });
            command.AddOption(inputOption);

            var outputOption = new Option<FileInfo?>(
                aliases: new[] { "--output", "-o" },
                description: "Path to output file (default: stdout)");
            command.AddOption(outputOption);

            var profileOption = new Option<FileInfo?>(
                aliases: new[] { "--profile", "-p" },
                description: "Path to profile configuration file (JSON)");
            profileOption.AddValidator(result =>
            {
                var filePath = result.GetValueOrDefault<FileInfo>();
                if (filePath != null && !filePath.Exists)
                {
                    result.ErrorMessage = "Profile file does not exist.";
                }
            });
            command.AddOption(profileOption);

            var projectTypeOption = new Option<string>(
                aliases: new[] { "--project-type", "-t" },
                getDefaultValue: () => "Uwp",
                description: "Project type (Uwp, Wpf, XamarinForms, WinUI, Maui)");
            command.AddOption(projectTypeOption);

            var classNameOption = new Option<string?>(
                aliases: new[] { "--class", "-c" },
                description: "Specific class name to generate XAML for (default: all classes)");
            command.AddOption(classNameOption);

            command.SetHandler(async (inputFile, outputFile, profileFile, projectType, className) =>
            {
                await ExecuteAsync(inputFile, outputFile, profileFile, projectType, className);
            }, inputOption, outputOption, profileOption, projectTypeOption, classNameOption);

            return command;
        }

        private static async Task<int> ExecuteAsync(
            FileInfo inputFile,
            FileInfo? outputFile,
            FileInfo? profileFile,
            string projectType,
            string? className)
        {
            try
            {
                var generator = new XamlGenerationService();
                var xaml = await generator.GenerateXamlAsync(
                    inputFile.FullName,
                    projectType,
                    className,
                    profileFile?.FullName);

                if (outputFile != null)
                {
                    await File.WriteAllTextAsync(outputFile.FullName, xaml);
                    Console.WriteLine($"XAML generated successfully: {outputFile.FullName}");
                }
                else
                {
                    Console.WriteLine(xaml);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error generating XAML: {ex.Message}");
                return 1;
            }
        }
    }
}
