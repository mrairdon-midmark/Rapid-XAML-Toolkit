# Architecture Diagrams

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Rapid XAML Toolkit Ecosystem                  │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────────────┐    ┌──────────────────────────────┐
│     Visual Studio VSIX       │    │      Command-Line Tool       │
│                              │    │                              │
│  • Real-time analysis        │    │  • Batch analysis            │
│  • Interactive generation    │    │  • Automated generation      │
│  • Drag & drop               │    │  • CI/CD integration         │
│  • IntelliSense              │    │  • Cross-platform            │
│  • Windows only              │    │  • Scriptable                │
└──────────────┬───────────────┘    └──────────────┬───────────────┘
               │                                    │
               │                                    │
               └────────────┬───────────────────────┘
                            │
                            ▼
               ┌────────────────────────────┐
               │   Shared Core Libraries    │
               │                            │
               │  • RapidXaml.AnalysisCore  │
               │  • RapidXaml.Resources     │
               │  • RapidXaml.Utils         │
               │  • RapidXaml.Shared        │
               └────────────────────────────┘
```

## CLI Tool Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        rxt (CLI Tool)                            │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│                          Program.cs                               │
│                    (System.CommandLine)                           │
└────────────┬─────────────────────────────────────┬───────────────┘
             │                                     │
             ▼                                     ▼
┌────────────────────────┐            ┌────────────────────────┐
│   AnalyzeCommand       │            │   GenerateCommand      │
│                        │            │                        │
│  Options:              │            │  Options:              │
│  • --project           │            │  • --input             │
│  • --format            │            │  • --output            │
│  • --errors-only       │            │  • --profile           │
└───────────┬────────────┘            └───────────┬────────────┘
            │                                     │
            ▼                                     ▼
┌────────────────────────┐            ┌────────────────────────┐
│ XamlAnalysisService    │            │ XamlGenerationService  │
│                        │            │                        │
│  • Parse project file  │            │  • Parse C#/VB code    │
│  • Find XAML files     │            │  • Generate XAML       │
│  • Run analyzers       │            │  • Apply profiles      │
│  • Format output       │            │  • Write output        │
└───────────┬────────────┘            └───────────┬────────────┘
            │                                     │
            ▼                                     ▼
┌────────────────────────┐            ┌────────────────────────┐
│   Support Services     │            │   Roslyn Parser        │
│                        │            │                        │
│  • CliLogger           │            │  • CSharpSyntaxTree    │
│  • CliTextSnapshot     │            │  • VBSyntaxTree        │
│  • CliVisualStudio     │            │  • SemanticModel       │
│    Abstraction         │            │  • CliDocumentParser   │
└────────────────────────┘            └────────────────────────┘
```

## CI/CD Integration Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                      GitHub Actions / Azure DevOps               │
└─────────────────────────────────────────────────────────────────┘

1. Code Push/PR
   │
   ▼
2. Trigger Pipeline
   │
   ▼
3. Setup .NET 8
   │
   ▼
4. Install RXT CLI
   │  dotnet tool install --global RapidXaml.Cli
   │
   ▼
5. Run Analysis
   │  rxt analyze --project MyApp.csproj --format msbuild
   │
   ▼
6. Parse Results
   │  • MSBuild format → Build logs
   │  • JSON format → Custom processing
   │  • Console format → Terminal output
   │
   ▼
7. Decision Point
   │
   ├─ Errors Found? → Fail Build
   │                   │
   │                   ▼
   │               Show Errors
   │               Block Merge
   │
   └─ No Errors? → Continue Build
                    │
                    ▼
                Deploy/Merge
```

## Output Format Flow

```
┌────────────────────────────────────────────────────────────────┐
│                    Analysis Results                             │
└────────────────────────────────────────────────────────────────┘

Analysis Engine
│
├─ MSBuild Format
│  │  File.xaml(10,5): error RXT001: Message (RXT001)
│  │
│  └─ Used by: Build systems, VS, IDEs
│
├─ JSON Format
│  │  [{"filePath": "...", "line": 10, ...}]
│  │
│  └─ Used by: Custom tools, scripts, dashboards
│
└─ Console Format
   │  [ERROR] File.xaml:10:5
   │    RXT001: Message
   │
   └─ Used by: Human reading, terminal output
```

## Component Dependencies

```
┌────────────────────────────────────────────────────────────────┐
│                       RapidXaml.Cli                             │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
│  │   Commands   │  │   Services   │  │   Parsers    │        │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘        │
│         │                  │                  │                 │
│         └──────────────────┼──────────────────┘                 │
│                            │                                    │
└────────────────────────────┼────────────────────────────────────┘
                             │
                ┌────────────┴────────────┐
                │                         │
        ┌───────▼────────┐       ┌───────▼────────┐
        │ RapidXaml      │       │ Microsoft      │
        │ Core Libraries │       │ Packages       │
        │                │       │                │
        │ • AnalysisCore │       │ • CodeAnalysis │
        │ • Resources    │       │ • Roslyn       │
        │ • Utils        │       │ • CommandLine  │
        │ • Shared       │       │ • JSON         │
        └────────────────┘       └────────────────┘
```

## Development vs Production Flow

```
┌────────────────────────────────────────────────────────────────┐
│                      Development Time                           │
└────────────────────────────────────────────────────────────────┘

Developer writes code
   │
   ▼
Visual Studio with VSIX
   │
   ├─ Real-time analysis
   ├─ Interactive generation
   ├─ IntelliSense support
   └─ Drag & drop
   │
   ▼
Commit to repository

┌────────────────────────────────────────────────────────────────┐
│                         Build Time                              │
└────────────────────────────────────────────────────────────────┘

CI/CD Pipeline
   │
   ▼
CLI Tool (rxt)
   │
   ├─ Batch analysis
   ├─ Automated generation
   ├─ Quality gates
   └─ Report generation
   │
   ▼
Deploy or Block
```

## Profile Configuration Flow

```
┌────────────────────────────────────────────────────────────────┐
│                    Profile Configuration                        │
└────────────────────────────────────────────────────────────────┘

my-profile.json
│
│  {
│    "name": "MAUI Profile",
│    "projectType": "Maui",
│    "mappings": [
│      {"type": "string", "output": "<Entry ... />"},
│      {"type": "int", "output": "<Entry ... />"}
│    ]
│  }
│
▼
rxt generate --input ViewModel.cs --profile my-profile.json
│
▼
Parser reads profile
│
│  • Match property types
│  • Apply templates
│  • Generate XAML
│
▼
Output XAML with custom templates
```

## Error Handling Flow

```
┌────────────────────────────────────────────────────────────────┐
│                      Error Handling                             │
└────────────────────────────────────────────────────────────────┘

rxt analyze --project MyApp.csproj
│
├─ Project not found?
│  │  Exit code: 1
│  │  Error message: "Project file not found"
│  │
├─ XAML parse error?
│  │  Continue processing
│  │  Log error
│  │
├─ Analysis errors found?
│  │  Output errors
│  │  Exit code: 1
│  │
└─ No errors?
   │  Output success message
   │  Exit code: 0
```

## Data Flow: Analysis

```
Project File (.csproj)
    │
    ▼
Parse for XAML files
    │
    ▼
For each XAML file:
    │
    ├─ Read file content
    │     │
    │     ▼
    ├─ Create TextSnapshot
    │     │
    │     ▼
    ├─ Run RapidXamlDocument.Create()
    │     │
    │     ▼
    ├─ Apply analyzers
    │     │
    │     ▼
    ├─ Collect issues (errors/warnings)
    │     │
    │     ▼
    └─ Format output
          │
          ▼
Output results
```

## Data Flow: Generation

```
ViewModel File (.cs/.vb)
    │
    ▼
Read file content
    │
    ▼
Parse with Roslyn
    │
    ├─ CSharpSyntaxTree
    │   or
    └─ VBSyntaxTree
    │
    ▼
Create SemanticModel
    │
    ▼
Find class declarations
    │
    ▼
For each class:
    │
    ├─ Get properties
    │     │
    │     ▼
    ├─ Match types to templates
    │     │
    │     ▼
    ├─ Apply profile mappings
    │     │
    │     ▼
    └─ Generate XAML elements
          │
          ▼
Output XAML
```
