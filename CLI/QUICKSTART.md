# Quick Start Guide: CLI Tool

This guide shows how to quickly get started with the Rapid XAML Toolkit CLI for CI/CD integration.

## Installation

Install the CLI as a .NET Global Tool:

```bash
dotnet tool install --global RapidXaml.Cli
```

Verify installation:

```bash
rxt version
```

## Quick Examples

### 1. Analyze XAML Files in a Project

```bash
rxt analyze --project ./MyApp/MyApp.csproj
```

This will analyze all XAML files in your project and output any issues in MSBuild format.

### 2. Generate XAML from a ViewModel

Given a C# ViewModel file:

```csharp
// MainViewModel.cs
public class MainViewModel
{
    public string Title { get; set; }
    public int Count { get; set; }
    public bool IsEnabled { get; set; }
    public ICommand IncrementCommand { get; set; }
}
```

Generate XAML:

```bash
rxt generate --input MainViewModel.cs --project-type Maui
```

Output:

```xml
<!-- Generated XAML for MainViewModel -->
<StackPanel>
    <TextBox Text="{Binding Title}" />
    <TextBox Text="{Binding Count}" />
    <CheckBox IsChecked="{Binding IsEnabled}" Content="IsEnabled" />
    <Button Command="{Binding IncrementCommand}" Content="IncrementCommand" />
</StackPanel>
```

### 3. Save Generated XAML to File

```bash
rxt generate \
  --input ./ViewModels/MainViewModel.cs \
  --output ./Views/MainView.xaml \
  --project-type Maui
```

### 4. Use in GitHub Actions

Create `.github/workflows/xaml-check.yml`:

```yaml
name: XAML Quality Check

on: [push, pull_request]

jobs:
  check:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Install RXT CLI
      run: dotnet tool install --global RapidXaml.Cli
    
    - name: Analyze XAML
      run: rxt analyze --project src/MyApp.csproj
```

### 5. Use in Azure DevOps

Create `azure-pipelines.yml`:

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

## Output Formats

### MSBuild Format (Default)

Perfect for CI/CD - compatible with build log parsers:

```bash
rxt analyze --project MyApp.csproj --format msbuild
```

Output:
```
MainPage.xaml(15,5): error RXT001: Element must have x:Name attribute (RXT001)
```

### JSON Format

Machine-readable for custom processing:

```bash
rxt analyze --project MyApp.csproj --format json > results.json
```

### Console Format

Human-readable for terminal viewing:

```bash
rxt analyze --project MyApp.csproj --format console
```

## Configuration

### Using Profile Files

Create a profile JSON file (`my-profile.json`):

```json
{
  "name": "My Custom Profile",
  "projectType": "Maui",
  "mappings": [
    {
      "type": "string",
      "output": "<Entry Text=\"{Binding $name$}\" />"
    }
  ]
}
```

Use it:

```bash
rxt generate \
  --input MainViewModel.cs \
  --profile my-profile.json
```

## Common Use Cases

### Pre-commit Hook

Add to `.git/hooks/pre-commit`:

```bash
#!/bin/bash
rxt analyze --project MyApp.csproj --errors-only
if [ $? -ne 0 ]; then
  echo "XAML analysis failed. Fix errors before committing."
  exit 1
fi
```

### Build Script Integration

In your build script:

```bash
# Analyze XAML
rxt analyze --project MyApp.csproj --format msbuild

# If analysis succeeds, continue with build
if [ $? -eq 0 ]; then
  dotnet build MyApp.csproj
fi
```

### Batch Processing Multiple Projects

```bash
for project in src/**/*.csproj; do
  echo "Analyzing $project"
  rxt analyze --project "$project"
done
```

## Troubleshooting

### Command not found

If `rxt` is not found after installation:

```bash
# Ensure .NET tools are in PATH
export PATH="$PATH:$HOME/.dotnet/tools"
```

### Project references not resolved

The CLI treats projects as "Unknown" type since it can't resolve all project references. This is expected and won't affect analysis accuracy for most scenarios.

## Next Steps

- Read the [full CLI documentation](./README.md)
- Learn about [profile configuration](./CONVERSION.md#profile-configuration)
- See [complete CI/CD examples](./examples/)
- Explore [advanced features](./README.md#output-formats)

## Getting Help

- GitHub Issues: https://github.com/mrlacey/rapid-xaml-toolkit/issues
- Documentation: https://rapidxaml.dev
- CLI README: [README.md](./README.md)
