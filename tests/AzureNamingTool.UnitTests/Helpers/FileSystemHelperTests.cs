using AzureNamingTool.Helpers;
using FluentAssertions;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AzureNamingTool.UnitTests.Helpers
{
    /// <summary>
    /// Unit tests for FileSystemHelper
    /// </summary>
    public class FileSystemHelperTests : IDisposable
    {
        private readonly string _testFolder = "test_settings";
        private readonly string _testFile = "test_config.json";

        public FileSystemHelperTests()
        {
            // Ensure test folder exists
            var testPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolder);
            if (!Directory.Exists(testPath))
            {
                Directory.CreateDirectory(testPath);
            }
        }

        public void Dispose()
        {
            // Cleanup test files
            try
            {
                var testPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolder);
                if (Directory.Exists(testPath))
                {
                    Directory.Delete(testPath, true);
                }

                // Clean up any test files in settings folder
                var settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings", _testFile);
                if (File.Exists(settingsFile))
                {
                    File.Delete(settingsFile);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        [Fact]
        public async Task CheckFile_ShouldCreateFile_WhenFileDoesNotExist()
        {
            // Arrange
            var fileName = $"{_testFolder}/new_file.json";
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            // Ensure file doesn't exist
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            // Act
            await FileSystemHelper.CheckFile(fileName);

            // Assert
            File.Exists(fullPath).Should().BeTrue("file should be created");
            var content = await File.ReadAllTextAsync(fullPath);
            content.Should().Be("[]", "file should be initialized with empty array");

            // Cleanup
            File.Delete(fullPath);
        }

        [Fact]
        public async Task CheckFile_ShouldNotOverwriteExistingFile()
        {
            // Arrange
            var fileName = $"{_testFolder}/existing_file.json";
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            var existingContent = "[{\"id\": 1}]";

            await File.WriteAllTextAsync(fullPath, existingContent);

            // Act
            await FileSystemHelper.CheckFile(fileName);

            // Assert
            var content = await File.ReadAllTextAsync(fullPath);
            content.Should().Be(existingContent, "existing file should not be overwritten");

            // Cleanup
            File.Delete(fullPath);
        }

        [Fact]
        public async Task WriteFile_ShouldWriteContent_ToSpecifiedFile()
        {
            // Arrange
            var fileName = "write_test.json";
            var folderName = $"{_testFolder}/";
            var testContent = "{\"test\": \"data\"}";

            // Act
            await FileSystemHelper.WriteFile(fileName, testContent, folderName);

            // Assert
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName + fileName);
            File.Exists(fullPath).Should().BeTrue("file should exist");
            var content = File.ReadAllText(fullPath);
            content.Should().Be(testContent, "content should match written data");

            // Cleanup
            File.Delete(fullPath);
        }

        [Fact]
        public async Task WriteFile_ShouldOverwriteExistingContent()
        {
            // Arrange
            var fileName = "overwrite_test.json";
            var folderName = $"{_testFolder}/";
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName + fileName);
            
            await FileSystemHelper.WriteFile(fileName, "old content", folderName);

            // Act
            await FileSystemHelper.WriteFile(fileName, "new content", folderName);

            // Assert
            var content = File.ReadAllText(fullPath);
            content.Should().Be("new content", "file should contain new content");

            // Cleanup
            File.Delete(fullPath);
        }

        [Fact]
        public async Task ReadFile_ShouldReturnContent_WhenFileExists()
        {
            // Arrange
            var fileName = "read_test.json";
            var folderName = $"{_testFolder}/";
            var testContent = "{\"name\": \"test\"}";
            
            await FileSystemHelper.WriteFile(fileName, testContent, folderName);

            // Act
            var result = await FileSystemHelper.ReadFile(fileName, folderName);

            // Assert
            result.Should().Be(testContent, "should read the same content that was written");

            // Cleanup
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName + fileName);
            File.Delete(fullPath);
        }

        [Fact]
        public async Task WriteConfiguration_ShouldSerializeAndWriteObject()
        {
            // Arrange
            var configData = new { Name = "TestConfig", Value = 123 };
            var configFileName = "config_test.json";

            // Act
            var result = await FileSystemHelper.WriteConfiguation(configData, configFileName);

            // Assert
            result.Should().Be("Config updated.", "should return success message");

            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + configFileName);
            File.Exists(fullPath).Should().BeTrue("config file should exist");

            var content = await File.ReadAllTextAsync(fullPath);
            content.Should().Contain("name", "should contain the name property");
            content.Should().Contain("TestConfig", "should contain the name value");
            content.Should().Contain("123", "should contain the value");

            // Cleanup
            File.Delete(fullPath);
        }

        [Fact]
        public async Task WriteConfiguration_ShouldHandleComplexObjects()
        {
            // Arrange
            var configData = new
            {
                Settings = new[]
                {
                    new { Id = 1, Name = "Setting1" },
                    new { Id = 2, Name = "Setting2" }
                },
                Enabled = true
            };
            var configFileName = "complex_config.json";

            // Act
            var result = await FileSystemHelper.WriteConfiguation(configData, configFileName);

            // Assert
            result.Should().Be("Config updated.");

            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + configFileName);
            var content = await File.ReadAllTextAsync(fullPath);
            
            content.Should().Contain("settings", "should serialize nested objects");
            content.Should().Contain("Setting1");
            content.Should().Contain("Setting2");

            // Cleanup
            File.Delete(fullPath);
        }

        [Fact]
        public void ResetConfiguration_ShouldReturnFalse_WhenFileNotInRepository()
        {
            // Arrange
            var nonExistentFile = "nonexistent_config.json";

            // Act
            var result = FileSystemHelper.ResetConfiguration(nonExistentFile);

            // Assert
            result.Should().BeFalse("should return false when file doesn't exist in repository");
        }

        [Fact]
        public void ResetConfiguration_ShouldCopyFromRepository_WhenFileExists()
        {
            // Arrange
            var fileName = "reset_test.json";
            var repoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "repository");
            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings");

            // Create repository directory if it doesn't exist
            if (!Directory.Exists(repoPath))
            {
                Directory.CreateDirectory(repoPath);
            }

            if (!Directory.Exists(settingsPath))
            {
                Directory.CreateDirectory(settingsPath);
            }

            // Create a test file in repository
            var repoFile = Path.Combine(repoPath, fileName);
            File.WriteAllText(repoFile, "repository content");

            // Act
            var result = FileSystemHelper.ResetConfiguration(fileName);

            // Assert
            result.Should().BeTrue("should return true when file is copied successfully");

            var settingsFile = Path.Combine(settingsPath, fileName);
            File.Exists(settingsFile).Should().BeTrue("file should be copied to settings");
            var content = File.ReadAllText(settingsFile);
            content.Should().Be("repository content", "content should match repository file");

            // Cleanup
            File.Delete(repoFile);
            File.Delete(settingsFile);
        }

        [Fact]
        public async Task MigrateDataToFile_ShouldCopyData_BetweenFiles()
        {
            // Arrange
            var sourceFileName = "source_migrate.json";
            var sourceFolder = $"{_testFolder}/";
            var destFileName = "dest_migrate.json";
            var destFolder = $"{_testFolder}/";
            var testData = "{\"migrated\": true}";

            await FileSystemHelper.WriteFile(sourceFileName, testData, sourceFolder);

            // Act
            await FileSystemHelper.MigrateDataToFile(sourceFileName, sourceFolder, destFileName, destFolder, false);

            // Assert
            var destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destFolder + destFileName);
            File.Exists(destPath).Should().BeTrue("destination file should exist");
            var content = File.ReadAllText(destPath);
            content.Should().Be(testData, "data should be copied correctly");

            // Cleanup
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sourceFolder + sourceFileName));
            File.Delete(destPath);
        }

        [Fact]
        public async Task MigrateDataToFile_ShouldDeleteSource_WhenDeleteFlagIsTrue()
        {
            // Arrange
            var sourceFileName = "delete_source.json";
            var sourceFolder = "repository/";
            var destFileName = "delete_dest.json";
            var destFolder = "settings/";
            var testData = "{\"delete\": true}";

            // Create repository and settings directories
            var repoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "repository");
            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings");
            
            if (!Directory.Exists(repoPath))
            {
                Directory.CreateDirectory(repoPath);
            }
            if (!Directory.Exists(settingsPath))
            {
                Directory.CreateDirectory(settingsPath);
            }

            await FileSystemHelper.WriteFile(sourceFileName, testData, sourceFolder);

            // Act
            await FileSystemHelper.MigrateDataToFile(sourceFileName, sourceFolder, destFileName, destFolder, true);

            // Assert
            var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sourceFolder + sourceFileName);
            var settingsSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + sourceFileName);
            
            File.Exists(sourcePath).Should().BeFalse("source file in repository should be deleted");
            File.Exists(settingsSourcePath).Should().BeFalse("source file in settings should be deleted");

            var destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destFolder + destFileName);
            File.Exists(destPath).Should().BeTrue("destination file should exist");

            // Cleanup
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
        }

        [Fact]
        public async Task WriteFile_ShouldUseDefaultSettingsFolder_WhenNoFolderSpecified()
        {
            // Arrange
            var fileName = "default_folder_test.json";
            var content = "{\"useDefault\": true}";

            // Ensure settings folder exists
            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings");
            if (!Directory.Exists(settingsPath))
            {
                Directory.CreateDirectory(settingsPath);
            }

            // Act
            await FileSystemHelper.WriteFile(fileName, content);

            // Assert
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + fileName);
            File.Exists(fullPath).Should().BeTrue("file should be in settings folder");

            // Cleanup
            File.Delete(fullPath);
        }

        [Fact]
        public async Task ReadFile_ShouldUseDefaultSettingsFolder_WhenNoFolderSpecified()
        {
            // Arrange
            var fileName = "default_read_test.json";
            var content = "{\"defaultRead\": true}";
            
            await FileSystemHelper.WriteFile(fileName, content);

            // Act
            var result = await FileSystemHelper.ReadFile(fileName);

            // Assert
            result.Should().Be(content, "should read from settings folder by default");

            // Cleanup
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + fileName);
            File.Delete(fullPath);
        }
    }
}
