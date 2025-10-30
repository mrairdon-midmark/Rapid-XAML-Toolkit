// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using RapidXaml.Cli.Parsers;
using RapidXamlToolkit;
using RapidXamlToolkit.Options;

namespace RapidXaml.Cli.Services
{
    /// <summary>
    /// Service for generating XAML from C#/VB.NET code.
    /// </summary>
    public class XamlGenerationService
    {
        /// <summary>
        /// Generates XAML from a C# or VB.NET file.
        /// </summary>
        /// <param name="inputPath">Path to the input file.</param>
        /// <param name="projectType">Type of project (Uwp, Wpf, etc.).</param>
        /// <param name="className">Specific class name to generate for (optional).</param>
        /// <param name="profilePath">Path to profile configuration (optional).</param>
        /// <returns>Generated XAML.</returns>
        public async Task<string> GenerateXamlAsync(
            string inputPath,
            string projectType,
            string? className,
            string? profilePath)
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException("Input file not found.", inputPath);
            }

            var code = await File.ReadAllTextAsync(inputPath);
            var extension = Path.GetExtension(inputPath).ToLowerInvariant();

            // Parse project type
            if (!Enum.TryParse<ProjectType>(projectType, true, out var projType))
            {
                projType = ProjectType.Uwp;
            }

            // Load profile if specified
            Profile? profile = null;
            if (!string.IsNullOrEmpty(profilePath) && File.Exists(profilePath))
            {
                var profileJson = await File.ReadAllTextAsync(profilePath);
                profile = Newtonsoft.Json.JsonConvert.DeserializeObject<Profile>(profileJson);
            }

            // Create parser based on file extension
            SyntaxTree? syntaxTree;
            SemanticModel? semanticModel;

            if (extension == ".cs")
            {
                syntaxTree = CSharpSyntaxTree.ParseText(code);
                var compilation = CSharpCompilation.Create("temp")
                    .AddSyntaxTrees(syntaxTree)
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
                semanticModel = compilation.GetSemanticModel(syntaxTree);
            }
            else if (extension == ".vb")
            {
                syntaxTree = VisualBasicSyntaxTree.ParseText(code);
                var compilation = VisualBasicCompilation.Create("temp")
                    .AddSyntaxTrees(syntaxTree)
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
                semanticModel = compilation.GetSemanticModel(syntaxTree);
            }
            else
            {
                throw new NotSupportedException($"File extension '{extension}' is not supported.");
            }

            var logger = new CliLogger();
            var parser = new CliDocumentParser(logger, projType, profile);
            var output = parser.Parse(syntaxTree.GetRoot(), semanticModel, className);

            return output;
        }
    }
}
