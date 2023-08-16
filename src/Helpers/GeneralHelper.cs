using AzureNamingTool.Models;
using AzureNamingTool.Pages;
using AzureNamingTool.Services;
using AzureNamingTool.Shared;
using Blazored.Modal;
using Blazored.Modal.Services;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AzureNamingTool.Helpers
{
    public class GeneralHelper
    {
        //Function to get the Property Value
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

        public static string NormalizeName(string name, bool lowercase)
        {
            string newname = name.Replace("Resource", "").Replace(" ", "");
            if (lowercase)
            {
                newname = newname.ToLower();
            }
            return newname;
        }

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

        public static bool IsNotNull([NotNullWhen(true)] object? obj) => obj != null;

        public static string[] FormatResoureType(string type)
        {
            String[] returntype = new String[3];
            // Make sure it is a full resource type name
            if (type.Contains("("))
            {
                returntype[0] = type.Substring(0, type.IndexOf("(")).Trim();
            }
            try
            {
                if ((GeneralHelper.IsNotNull(type)) && (GeneralHelper.IsNotNull(returntype[0])))
                {
                    // trim any details out of the value
                    if (returntype[0].Contains(" -"))
                    {
                        returntype[1] = returntype[0].Substring(0, returntype[0].IndexOf(" -")).Trim();
                    }

                    // trim any details out of the value
                    if ((type.Contains("(")) && (type.Contains(")")))
                    {
                        {
                            int intstart = type.IndexOf("(") + 1;
                            returntype[2] = String.Concat(type.Substring(intstart).TakeWhile(x => x != ')'));
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
    }
}
