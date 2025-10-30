// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.CommandLine;
using System.Threading.Tasks;
using RapidXaml.Cli.Commands;

namespace RapidXaml.Cli
{
    /// <summary>
    /// Main entry point for the Rapid XAML Toolkit CLI.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Exit code.</returns>
        public static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Rapid XAML Toolkit - Analyze and generate XAML from C#/VB.NET code")
            {
                Name = "rxt"
            };

            // Add analyze command
            var analyzeCommand = AnalyzeCommand.Create();
            rootCommand.AddCommand(analyzeCommand);

            // Add generate command
            var generateCommand = GenerateCommand.Create();
            rootCommand.AddCommand(generateCommand);

            // Add version command
            var versionCommand = new Command("version", "Display version information");
            versionCommand.SetHandler(() =>
            {
                var version = typeof(Program).Assembly.GetName().Version;
                Console.WriteLine($"Rapid XAML Toolkit CLI v{version}");
            });
            rootCommand.AddCommand(versionCommand);

            try
            {
                return await rootCommand.InvokeAsync(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }
    }
}
