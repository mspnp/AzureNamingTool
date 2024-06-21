using AzureNamingTool.Models;
using AzureNamingTool.Components.Pages;
using AzureNamingTool.Services;
using AzureNamingTool.Components;
using Blazored.Modal;
using Blazored.Modal.Services;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AzureNamingTool.Helpers
{

    /// <summary>
    /// Helper class for general operations.
    /// </summary>
    public class GeneralHelper
    {
        /// <summary>
        /// Function to get the Property Value
        /// </summary>
        /// <param name="SourceData">The source data object</param>
        /// <param name="propName">The name of the property</param>
        /// <returns>The value of the property</returns>
        public static object? GetPropertyValue(object SourceData, string propName)
        {
            try
            {
                return SourceData!.GetType()!.GetProperty(propName)!.GetValue(SourceData, null);
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return null;
            }
        }

        /// <summary>
        /// Encrypts a string using AES encryption.
        /// </summary>
        /// <param name="text">The text to encrypt</param>
        /// <param name="keyString">The encryption key</param>
        /// <returns>The encrypted string</returns>
        public static string EncryptString(string text, string keyString)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = Encoding.UTF8.GetBytes(keyString);
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new((Stream)cryptoStream))
                {
                    streamWriter.Write(text);
                }
                array = memoryStream.ToArray();
            }
            return Convert.ToBase64String(array);
        }

        /// <summary>
        /// Decrypts a string using AES decryption.
        /// </summary>
        /// <param name="cipherText">The encrypted string</param>
        /// <param name="keyString">The decryption key</param>
        /// <returns>The decrypted string</returns>
        public static string DecryptString(string cipherText, string keyString)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = Encoding.UTF8.GetBytes(keyString);
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new((Stream)cryptoStream);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Checks if a string is base64 encoded.
        /// </summary>
        /// <param name="value">The string to check</param>
        /// <returns>True if the string is base64 encoded, otherwise false</returns>
        public static bool IsBase64Encoded(string value)
        {
            bool base64encoded = false;
            try
            {
                byte[] byteArray = Convert.FromBase64String(value);
                base64encoded = true;
            }
            catch (FormatException)
            {
                // The string is not base 64. Dismiss the error and return false
            }
            return base64encoded;
        }

        /// <summary>
        /// Downloads a string from a URL using HttpClient.
        /// </summary>
        /// <param name="url">The URL to download from</param>
        /// <returns>The downloaded string</returns>
        public static async Task<string> DownloadString(string url)
        {
            string data;
            try
            {
                HttpClient httpClient = new();
                data = await httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                data = "";
            }
            return data;
        }

        /// <summary>
        /// Normalizes a name by removing "Resource" and spaces.
        /// </summary>
        /// <param name="name">The name to normalize</param>
        /// <param name="lowercase">True to convert the name to lowercase, false otherwise</param>
        /// <returns>The normalized name</returns>
        public static string NormalizeName(string name, bool lowercase)
        {
            string newname = name.Replace("Resource", "").Replace(" ", "");
            if (lowercase)
            {
                newname = newname.ToLower();
            }
            return newname;
        }

        /// <summary>
        /// Sets the CSS class for text based on the enabled state.
        /// </summary>
        /// <param name="enabled">True if the text is enabled, false if disabled</param>
        /// <returns>The CSS class for the text</returns>
        public static string SetTextEnabledClass(bool enabled)
        {
            if (enabled)
            {
                return "";
            }
            else
            {
                return "disabled-text";
            }
        }

        /// <summary>
        /// Checks if an object is not null.
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns>True if the object is not null, otherwise false</returns>
        public static bool IsNotNull([NotNullWhen(true)] object? obj) => obj != null;

        /// <summary>
        /// Formats a resource type string.
        /// </summary>
        /// <param name="type">The resource type string</param>
        /// <returns>An array containing the formatted resource type</returns>
        public static string[] FormatResoureType(string type)
        {
            String[] returntype = new String[4];
            returntype[0] = type;
            // Make sure it is a full resource type name
            if (type.Contains('('))
            {
                returntype[0] = type[..type.IndexOf('(')].Trim();
            }
            try
            {
                if ((GeneralHelper.IsNotNull(type)) && (GeneralHelper.IsNotNull(returntype[0])))
                {
                    // trim any details out of the value
                    // Get the base resource type name
                    if (returntype[0].Contains(" -"))
                    {
                        // Get all text before the dash
                        returntype[1] = returntype[0][..returntype[0].IndexOf(" -")].Trim();
                        // Get all text after the dash
                        returntype[3] = returntype[0][(returntype[0].IndexOf('-') + 1)..].Trim();
                    }

                    // trim any details out of the value
                    if (type.Contains('(') && type.Contains(')'))
                    {
                        {
                            int intstart = type.IndexOf('(') + 1;
                            returntype[2] = String.Concat(type[intstart..].TakeWhile(x => x != ')'));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return returntype;
        }

        /// <summary>
        /// Generates a random string of specified length.
        /// </summary>
        /// <param name="maxLength">The maximum length of the string</param>
        /// <param name="alphanumeric">True to include alphanumeric characters, false for lowercase alphabetic characters only</param>
        /// <returns>The generated random string</returns>
        public static string GenerateRandomString(int maxLength, bool alphanumeric)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz";
            if (alphanumeric)
            {
                chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            }

            var Charsarr = new char[maxLength];
            var random = new Random();

            for (int i = 0; i < Charsarr.Length; i++)
            {
                Charsarr[i] = chars[random.Next(chars.Length)];
            }

            var result = new String(Charsarr);

            return result;
        }
    }
}
