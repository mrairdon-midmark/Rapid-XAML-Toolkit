# Converting Rapid XAML Toolkit from IDE Extension to CLI Utility

This document explains the conversion of the Rapid XAML Toolkit from a Visual Studio IDE extension to a command-line utility that can be run in CI/CD pipelines.

## Overview

The Rapid XAML Toolkit was originally designed as a Visual Studio extension (VSIX) providing:
1. **XAML Analysis** - Find and fix issues in XAML files
2. **XAML Generation** - Generate XAML from C#/VB.NET ViewModels
3. **Editor Enhancements** - Improve the XAML editing experience
4. **Roslyn Analyzers** - Code analysis for MVVM patterns

The CLI conversion focuses on making the first two features (Analysis and Generation) available as command-line tools suitable for automation.

## Architecture Changes

### Original VSIX Architecture

```
RapidXaml.Generation (VSIX)
├── Commands (VS-specific)
├── DragDrop (VS-specific)
├── Parsers (Core logic)
└── Options (VS UI)

RapidXaml.Analysis (VSIX)
├── XamlAnalysis (Core logic)
├── Actions (VS-specific)
└── Tags (Core logic)

RapidXaml.AnalysisExe (Console App - .NET Framework 4.8)
└── Wrapper for build-time analysis
```

### New CLI Architecture

```
RapidXaml.Cli (.NET 8 Console App)
├── Commands
│   ├── AnalyzeCommand
│   └── GenerateCommand
├── Services
│   ├── XamlAnalysisService
│   ├── XamlGenerationService
│   ├── CliLogger
│   ├── CliTextSnapshot
│   └── CliVisualStudioAbstraction
├── Parsers
│   └── CliDocumentParser
└── Examples
    ├── GitHub Actions workflows
    └── Azure DevOps pipelines
```

## Key Implementation Details

### 1. CLI Framework

The CLI uses **System.CommandLine** for command parsing and execution:
- Modern, well-maintained .NET CLI framework
- Supports subcommands, options, and validation
- Good error handling and help generation

### 2. Analysis Implementation

The analysis command wraps the existing `RapidXaml.AnalysisCore` library:

```csharp
// Reuses existing analysis engine
var snapshot = new CliTextSnapshot(xamlFilePath);
var rxdoc = RapidXamlDocument.Create(snapshot, xamlFilePath, vsa, projectPath, logger);
```

**Key Adaptations:**
- `CliTextSnapshot`: Implements `ITextSnapshot` for file-based XAML reading
- `CliVisualStudioAbstraction`: Implements `IVisualStudioAbstraction` without VS dependencies
- `CliLogger`: Implements `ILogger` for console output

### 3. Generation Implementation

The generation command uses Roslyn to parse C#/VB.NET code:

```csharp
// Parse code with Roslyn
var syntaxTree = CSharpSyntaxTree.ParseText(code);
var compilation = CSharpCompilation.Create("temp")
    .AddSyntaxTrees(syntaxTree);
var semanticModel = compilation.GetSemanticModel(syntaxTree);

// Generate XAML
var parser = new CliDocumentParser(logger, projectType, profile);
var xaml = parser.Parse(syntaxTree.GetRoot(), semanticModel, className);
```

**Key Features:**
- Supports both C# and VB.NET
- Profile-based configuration
- Multiple output formats
- Batch processing capability

### 4. Output Formats

Three output formats for flexibility:

1. **MSBuild Format**: Compatible with build systems
   ```
   File.xaml(10,5): error RXT001: Message (RXT001)
   ```

2. **JSON Format**: Machine-readable for tooling
   ```json
   [{"filePath": "...", "line": 10, ...}]
   ```

3. **Console Format**: Human-readable
   ```
   [ERROR] File.xaml:10:5
     RXT001: Message
   ```

## CI/CD Integration

### GitHub Actions Example

```yaml
- name: Analyze XAML
  run: |
    dotnet tool install --global RapidXaml.Cli
    rxt analyze --project ./MyApp.csproj --format msbuild
```

### Azure DevOps Example

```yaml
- script: |
    dotnet tool install --global RapidXaml.Cli
    rxt analyze --project $(Build.SourcesDirectory)/MyApp.csproj
  displayName: 'Analyze XAML'
```

## Differences from VSIX

| Aspect | VSIX Extension | CLI Tool |
|--------|----------------|----------|
| **Target Framework** | .NET Framework 4.8 | .NET 8.0 |
| **Platform** | Windows only | Cross-platform |
| **Packaging** | VSIX installer | NuGet global tool |
| **UI** | Visual Studio integration | Command-line only |
| **Analysis** | Real-time in editor | On-demand |
| **Generation** | Interactive with IntelliSense | Batch processing |
| **Configuration** | VS settings UI | JSON files |
| **Output** | VS Error List/Toolbox | Console/Files |

## Benefits of CLI Approach

1. **CI/CD Integration**: Run analysis and generation in automated pipelines
2. **Cross-platform**: Works on Windows, Linux, macOS
3. **Batch Processing**: Process multiple files efficiently
4. **Scriptable**: Easy to integrate into build scripts
5. **Version Control**: Configuration files can be committed
6. **No IDE Required**: Can run in containerized environments

## Limitations of CLI

1. **No IntelliSense**: Cannot leverage VS IntelliSense for attribute suggestions
2. **No Interactive UI**: Cannot show interactive prompts or dialogs
3. **No Real-time Feedback**: Requires explicit invocation
4. **Limited Context**: Cannot access full VS project system information

## Migration Path

Users can use both the VSIX and CLI together:

1. **Development**: Use VSIX for interactive development and real-time feedback
2. **CI/CD**: Use CLI for automated checks and generation in pipelines
3. **Configuration**: Share the same profile JSON files between both tools

## Installation and Distribution

### As .NET Global Tool

```bash
# Install globally
dotnet tool install --global RapidXaml.Cli

# Update
dotnet tool update --global RapidXaml.Cli

# Uninstall
dotnet tool uninstall --global RapidXaml.Cli
```

### As Local Tool

```bash
# Install locally in project
dotnet tool install --local RapidXaml.Cli

# Run with dotnet tool
dotnet rxt analyze --project MyApp.csproj
```

## Building from Source

```bash
cd CLI/RapidXaml.Cli
dotnet build
dotnet pack
dotnet tool install --global --add-source ./bin/Debug RapidXaml.Cli
```

## Testing

The CLI can be tested using:
1. Unit tests for individual components
2. Integration tests for end-to-end scenarios
3. CI/CD pipeline testing

Example test structure:
```csharp
[TestMethod]
public async Task Analyze_ValidProject_ReturnsResults()
{
    var service = new XamlAnalysisService();
    var results = await service.AnalyzeProjectAsync(testProjectPath, false);
    Assert.IsNotNull(results);
}
```

## Future Enhancements

Potential improvements for the CLI:

1. **Watch Mode**: Monitor files for changes and auto-analyze
2. **Interactive Mode**: Prompt user for options
3. **Report Generation**: HTML/Markdown reports of analysis results
4. **Custom Analyzers**: Load custom analyzers from DLLs
5. **Parallel Processing**: Analyze multiple projects concurrently
6. **Caching**: Cache analysis results for faster subsequent runs
7. **Configuration Validation**: Validate profile JSON files
8. **Auto-fix**: Automatically fix certain issues

## Performance Considerations

### Analysis Performance
- Cold start: ~2-5 seconds per project
- Warm start: ~1-2 seconds per project
- Depends on: Project size, number of XAML files, complexity of analyzers

### Generation Performance
- Parsing: Fast with Roslyn (~100ms per file)
- Generation: Depends on number of properties and profile complexity

### Optimization Strategies
1. Cache parsed syntax trees
2. Parallel file processing
3. Incremental analysis (only changed files)
4. Profile pre-compilation

## Conclusion

The CLI conversion makes Rapid XAML Toolkit accessible to CI/CD pipelines while maintaining compatibility with the existing VSIX extension. This dual approach provides the best of both worlds: interactive development with the VSIX and automated quality checks with the CLI.

## References

- [System.CommandLine Documentation](https://docs.microsoft.com/en-us/dotnet/standard/commandline/)
- [Roslyn API Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)
- [.NET Global Tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure DevOps Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/)
