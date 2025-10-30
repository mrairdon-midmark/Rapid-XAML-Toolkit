// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RapidXamlToolkit;
using RapidXamlToolkit.Logging;
using RapidXamlToolkit.Options;

namespace RapidXaml.Cli.Parsers
{
    /// <summary>
    /// Parser for CLI document processing.
    /// </summary>
    public class CliDocumentParser
    {
        private readonly ILogger logger;
        private readonly ProjectType projectType;
        private readonly Profile? profile;

        /// <summary>
        /// Initializes a new instance of the <see cref="CliDocumentParser"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="projectType">Type of project.</param>
        /// <param name="profile">Profile configuration (optional).</param>
        public CliDocumentParser(ILogger logger, ProjectType projectType, Profile? profile = null)
        {
            this.logger = logger;
            this.projectType = projectType;
            this.profile = profile;
        }

        /// <summary>
        /// Parses a syntax tree and generates XAML.
        /// </summary>
        /// <param name="root">Root syntax node.</param>
        /// <param name="semanticModel">Semantic model.</param>
        /// <param name="className">Specific class name to parse (optional).</param>
        /// <returns>Generated XAML.</returns>
        public string Parse(SyntaxNode root, SemanticModel semanticModel, string? className)
        {
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var output = new StringBuilder();

            foreach (var classDecl in classes)
            {
                if (!string.IsNullOrEmpty(className) && classDecl.Identifier.Text != className)
                {
                    continue;
                }

                this.logger.RecordInfo($"Processing class: {classDecl.Identifier.Text}");

                // Get all properties in the class
                var properties = classDecl.DescendantNodes()
                    .OfType<PropertyDeclarationSyntax>()
                    .ToList();

                output.AppendLine($"<!-- Generated XAML for {classDecl.Identifier.Text} -->");
                output.AppendLine("<StackPanel>");

                foreach (var prop in properties)
                {
                    var propName = prop.Identifier.Text;
                    var propType = prop.Type.ToString();

                    this.logger.RecordInfo($"  Property: {propName} ({propType})");

                    // Simple XAML generation based on property type
                    if (propType.Contains("string") || propType.Contains("String"))
                    {
                        output.AppendLine($"    <TextBox Text=\"{{Binding {propName}}}\" />");
                    }
                    else if (propType.Contains("int") || propType.Contains("Int") ||
                             propType.Contains("double") || propType.Contains("Double") ||
                             propType.Contains("decimal") || propType.Contains("Decimal"))
                    {
                        output.AppendLine($"    <TextBox Text=\"{{Binding {propName}}}\" />");
                    }
                    else if (propType.Contains("bool") || propType.Contains("Boolean"))
                    {
                        output.AppendLine($"    <CheckBox IsChecked=\"{{Binding {propName}}}\" Content=\"{propName}\" />");
                    }
                    else if (propType.Contains("DateTime"))
                    {
                        output.AppendLine($"    <DatePicker SelectedDate=\"{{Binding {propName}}}\" />");
                    }
                    else if (propType.Contains("Command") || propType.Contains("ICommand"))
                    {
                        output.AppendLine($"    <Button Command=\"{{Binding {propName}}}\" Content=\"{propName}\" />");
                    }
                    else if (propType.Contains("ObservableCollection") || propType.Contains("List"))
                    {
                        output.AppendLine($"    <ListBox ItemsSource=\"{{Binding {propName}}}\" />");
                    }
                    else
                    {
                        // Default to TextBlock for display
                        output.AppendLine($"    <TextBlock Text=\"{{Binding {propName}}}\" />");
                    }
                }

                output.AppendLine("</StackPanel>");
                output.AppendLine();
            }

            return output.ToString();
        }
    }
}
