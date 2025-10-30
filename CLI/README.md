# Rapid XAML Toolkit CLI

A command-line interface for the Rapid XAML Toolkit that enables XAML analysis and generation from C#/VB.NET code. This tool is designed to be integrated into CI/CD pipelines such as GitHub Actions and Azure DevOps.

## Installation

### As a .NET Global Tool

```bash
dotnet tool install --global RapidXaml.Cli
```

### Local Installation

```bash
dotnet tool install --local RapidXaml.Cli
```

## Usage

The CLI provides two main commands: `analyze` and `generate`.

### Analyze Command

Analyze XAML files in a project for potential issues.

```bash
rxt analyze --project <path-to-csproj> [options]
```

**Options:**
- `--project, -p` (required): Path to the project file (.csproj or .vbproj)
- `--format, -f`: Output format (msbuild, json, console) [default: msbuild]
- `--errors-only, -e`: Only output errors, not warnings [default: false]

**Examples:**

```bash
# Analyze with MSBuild format (suitable for CI/CD)
rxt analyze --project ./MyApp.csproj

# Analyze with JSON output
rxt analyze --project ./MyApp.csproj --format json

# Show only errors
rxt analyze --project ./MyApp.csproj --errors-only
```

**Exit Codes:**
- `0`: Success, no errors found
- `1`: Errors found or execution failed

### Generate Command

Generate XAML from C#/VB.NET ViewModel code.

```bash
rxt generate --input <path-to-cs-or-vb> [options]
```

**Options:**
- `--input, -i` (required): Path to the C# or VB.NET file containing ViewModel code
- `--output, -o`: Path to output file (default: stdout)
- `--profile, -p`: Path to profile configuration file (JSON)
- `--project-type, -t`: Project type (Uwp, Wpf, XamarinForms, WinUI, Maui) [default: Uwp]
- `--class, -c`: Specific class name to generate XAML for (default: all classes)

**Examples:**

```bash
# Generate XAML to stdout
rxt generate --input ./ViewModels/MainViewModel.cs

# Generate XAML to file
rxt generate --input ./ViewModels/MainViewModel.cs --output ./Views/MainView.xaml

# Generate for specific project type
rxt generate --input ./ViewModels/MainViewModel.cs --project-type Maui

# Generate for specific class
rxt generate --input ./ViewModels/MainViewModel.cs --class MainViewModel

# Use custom profile
rxt generate --input ./ViewModels/MainViewModel.cs --profile ./profiles/custom.json
```

### Version Command

Display version information.

```bash
rxt version
```

## CI/CD Integration

### GitHub Actions

```yaml
name: XAML Analysis

on: [push, pull_request]

jobs:
  analyze:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Install Rapid XAML CLI
      run: dotnet tool install --global RapidXaml.Cli
    
    - name: Analyze XAML
      run: rxt analyze --project ./src/MyApp.csproj
```

See [examples/github-actions/xaml-workflow.yml](examples/github-actions/xaml-workflow.yml) for a complete example.

### Azure DevOps

```yaml
pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- script: |
    dotnet tool install --global RapidXaml.Cli
    rxt analyze --project $(Build.SourcesDirectory)/src/MyApp.csproj
  displayName: 'Analyze XAML'
```

See [examples/azure-devops/azure-pipelines.yml](examples/azure-devops/azure-pipelines.yml) for a complete example.

## Profile Configuration

You can customize XAML generation using a JSON profile configuration file:

```json
{
  "name": "Custom Profile",
  "projectType": "Maui",
  "mappings": [
    {
      "type": "string",
      "output": "<Entry Text=\"{Binding $name$}\" />"
    },
    {
      "type": "int",
      "output": "<Entry Text=\"{Binding $name$}\" Keyboard=\"Numeric\" />"
    }
  ]
}
```

## Output Formats

### MSBuild Format

Suitable for CI/CD integration. Follows the MSBuild error format:

```
File.xaml(10,5): error RXT001: Element must have x:Name attribute (RXT001)
```

### JSON Format

Structured output for programmatic processing:

```json
[
  {
    "filePath": "MainPage.xaml",
    "line": 10,
    "column": 5,
    "code": "RXT001",
    "message": "Element must have x:Name attribute",
    "isError": true
  }
]
```

### Console Format

Human-readable output:

```
[ERROR] MainPage.xaml:10:5
  RXT001: Element must have x:Name attribute
```

## Comparison with IDE Extension

| Feature | IDE Extension | CLI Tool |
|---------|--------------|----------|
| XAML Analysis | ✅ | ✅ |
| XAML Generation | ✅ | ✅ |
| Interactive UI | ✅ | ❌ |
| CI/CD Integration | ❌ | ✅ |
| Custom Analyzers | ✅ | ✅ |
| Drag & Drop | ✅ | ❌ |
| IntelliSense | ✅ | ❌ |
| Batch Processing | ❌ | ✅ |

## Building from Source

```bash
cd CLI/RapidXaml.Cli
dotnet build
dotnet pack
dotnet tool install --global --add-source ./bin/Debug RapidXaml.Cli
```

## License

This project is licensed under the MIT License - see the [LICENSE.txt](../../LICENSE.txt) file for details.

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](../../CONTRIBUTING.md) for details.

## Support

For issues and questions, please visit:
- GitHub Issues: https://github.com/mrlacey/rapid-xaml-toolkit/issues
- Documentation: https://rapidxaml.dev
