// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RapidXaml.Cli.Services;
using System.IO;
using System.Threading.Tasks;

namespace RapidXaml.Cli.Tests
{
    [TestClass]
    public class XamlAnalysisServiceTests
    {
        [TestMethod]
        public async Task AnalyzeProject_WithValidProject_ReturnsResults()
        {
            // Arrange
            var service = new XamlAnalysisService();
            var projectContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <Page Include=""MainPage.xaml"" />
  </ItemGroup>
</Project>";
            
            var xamlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Page xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
      xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <StackPanel>
        <TextBlock Text=""Hello World"" />
    </StackPanel>
</Page>";

            var tempDir = Path.Combine(Path.GetTempPath(), "RxtTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            
            var projectPath = Path.Combine(tempDir, "TestProject.csproj");
            var xamlPath = Path.Combine(tempDir, "MainPage.xaml");
            
            await File.WriteAllTextAsync(projectPath, projectContent);
            await File.WriteAllTextAsync(xamlPath, xamlContent);

            try
            {
                // Act
                var results = await service.AnalyzeProjectAsync(projectPath, false);

                // Assert
                Assert.IsNotNull(results);
                // Results may be empty if no issues found, which is valid
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public async Task AnalyzeProject_WithNonExistentProject_ThrowsException()
        {
            // Arrange
            var service = new XamlAnalysisService();
            var projectPath = "NonExistent.csproj";

            // Act
            await service.AnalyzeProjectAsync(projectPath, false);

            // Assert - expects exception
        }
    }
}
