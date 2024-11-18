using AzureNamingTool.Models;
using AzureNamingTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AzureNamingTool.Helpers
{
    /// <summary>
    /// Helper class for file system operations.
    /// </summary>
    public class FileSystemHelper
    {
        /// <summary>
        /// Reads the content of a file.
        /// </summary>
        /// <param name="fileName">The name of the file to read.</param>
        /// <param name="folderName">The folder name where the file is located. Default is "settings/".</param>
        /// <returns>The content of the file.</returns>
        public static async Task<string> ReadFile(string fileName, string folderName = "settings/")
        {
            await CheckFile(folderName + fileName);
            string data = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName + fileName));
            return data;
        }

        /// <summary>
        /// Writes content to a file.
        /// </summary>
        /// <param name="fileName">The name of the file to write.</param>
        /// <param name="content">The content to write to the file.</param>
        /// <param name="folderName">The folder name where the file should be located. Default is "settings/".</param>
        public static async Task WriteFile(string fileName, string content, string folderName = "settings/")
        {
            await CheckFile(folderName + fileName);
            int retries = 0;
            while (retries < 10)
            {
                try
                {
                    using FileStream fstr = File.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName + fileName), FileMode.Truncate, FileAccess.Write);
                    StreamWriter sw = new(fstr);
                    sw.Write(content);
                    sw.Flush();
                    sw.Dispose();
                    return;
                }
                catch (Exception)
                {
                    Thread.Sleep(50);
                    retries++;
                }
            }
        }

        /// <summary>
        /// Checks if a file exists. If not, creates an empty file.
        /// </summary>
        /// <param name="fileName">The name of the file to check.</param>
        public static async Task CheckFile(string fileName)
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName)))
            {
                var file = File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
                file.Close();

                for (int numTries = 0; numTries < 10; numTries++)
                {
                    try
                    {
                        await File.WriteAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), "[]");
                        return;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(50);
                    }
                }
            }
        }

        /// <summary>
        /// Writes configuration data to a file.
        /// </summary>
        /// <param name="configdata">The configuration data to write.</param>
        /// <param name="configFileName">The name of the configuration file.</param>
        /// <returns>A message indicating the status of the operation.</returns>
        public static async Task<object> WriteConfiguation(object configdata, string configFileName)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                await FileSystemHelper.WriteFile(configFileName, JsonSerializer.Serialize(configdata, options));
                return "Config updated.";
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return ex;
            }
        }

        /// <summary>
        /// Resets the configuration file by copying it from the repository folder to the settings folder.
        /// </summary>
        /// <param name="filename">The name of the configuration file to reset.</param>
        /// <returns>True if the configuration file was reset successfully, otherwise false.</returns>
        public static bool ResetConfiguration(string filename)
        {
            bool result = false;
            try
            {
                DirectoryInfo dirRepository = new("repository");
                foreach (FileInfo file in dirRepository.GetFiles())
                {
                    if (file.Name == filename)
                    {
                        file.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + file.Name), true);
                        result = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return result;
        }

        /// <summary>
        /// Migrates data from one file to another.
        /// </summary>
        /// <param name="sourcefileName">The name of the source file.</param>
        /// <param name="sourcefolderName">The folder name where the source file is located.</param>
        /// <param name="destinationfilename">The name of the destination file.</param>
        /// <param name="destinationfolderName">The folder name where the destination file should be located.</param>
        /// <param name="delete">Indicates whether to delete the source file after migration.</param>
        public static async Task MigrateDataToFile(string sourcefileName, string sourcefolderName, string destinationfilename, string destinationfolderName, bool delete)
        {
            string data = await ReadFile(sourcefileName, sourcefolderName);
            await WriteFile(destinationfilename, data, destinationfolderName);

            if (delete)
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "repository/" + sourcefileName));
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/" + sourcefileName));
            }
        }
    }
}
