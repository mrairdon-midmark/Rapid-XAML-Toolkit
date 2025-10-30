// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RapidXaml.Cli.Services;
using System.IO;
using System.Threading.Tasks;

namespace RapidXaml.Cli.Tests
{
    [TestClass]
    public class XamlGenerationServiceTests
    {
        [TestMethod]
        public async Task GenerateXaml_FromCSharpViewModel_ReturnsXaml()
        {
            // Arrange
            var service = new XamlGenerationService();
            var viewModelCode = @"
namespace TestApp
{
    public class MainViewModel
    {
        public string Title { get; set; }
        public int Count { get; set; }
        public bool IsEnabled { get; set; }
    }
}";

            var tempFile = Path.GetTempFileName();
            tempFile = Path.ChangeExtension(tempFile, ".cs");
            await File.WriteAllTextAsync(tempFile, viewModelCode);

            try
            {
                // Act
                var xaml = await service.GenerateXamlAsync(tempFile, "Uwp", null, null);

                // Assert
                Assert.IsNotNull(xaml);
                Assert.IsTrue(xaml.Contains("MainViewModel"));
                Assert.IsTrue(xaml.Contains("Binding Title"));
                Assert.IsTrue(xaml.Contains("Binding Count"));
                Assert.IsTrue(xaml.Contains("Binding IsEnabled"));
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [TestMethod]
        public async Task GenerateXaml_WithSpecificClass_GeneratesOnlyThatClass()
        {
            // Arrange
            var service = new XamlGenerationService();
            var viewModelCode = @"
namespace TestApp
{
    public class MainViewModel
    {
        public string Title { get; set; }
    }

    public class OtherViewModel
    {
        public string Name { get; set; }
    }
}";

            var tempFile = Path.GetTempFileName();
            tempFile = Path.ChangeExtension(tempFile, ".cs");
            await File.WriteAllTextAsync(tempFile, viewModelCode);

            try
            {
                // Act
                var xaml = await service.GenerateXamlAsync(tempFile, "Uwp", "MainViewModel", null);

                // Assert
                Assert.IsNotNull(xaml);
                Assert.IsTrue(xaml.Contains("MainViewModel"));
                Assert.IsFalse(xaml.Contains("OtherViewModel"));
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public async Task GenerateXaml_WithNonExistentFile_ThrowsException()
        {
            // Arrange
            var service = new XamlGenerationService();

            // Act
            await service.GenerateXamlAsync("NonExistent.cs", "Uwp", null, null);

            // Assert - expects exception
        }
    }
}
