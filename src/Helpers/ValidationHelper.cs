using AzureNamingTool.Models;
using AzureNamingTool.Services;
using System.Text.RegularExpressions;
using System.Text;
using System.Configuration;

namespace AzureNamingTool.Helpers
{
    /// <summary>
    /// Helper class for validation operations.
    /// </summary>
    public class ValidationHelper
    {
        /// <summary>
        /// Validates a password.
        /// </summary>
        /// <param name="text">The password to validate.</param>
        /// <returns>True if the password is valid, otherwise false.</returns>
        public static bool ValidatePassword(string text)
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMinimum8Chars = new Regex(@".{8,}");

            var isValidated = hasNumber.IsMatch(text) && hasUpperChar.IsMatch(text) && hasMinimum8Chars.IsMatch(text);

            return isValidated;
        }

        /// <summary>
        /// Validates a short name.
        /// </summary>
        /// <param name="type">The type of the short name.</param>
        /// <param name="value">The value of the short name.</param>
        /// <param name="parentcomponent">The parent component of the short name (optional).</param>
        /// <returns>True if the short name is valid, otherwise false.</returns>
        public static async Task<bool> ValidateShortName(string type, string value, string? parentcomponent = null)
        {
            bool valid = false;
            try
            {
                ResourceComponent resourceComponent = new();
                List<ResourceComponent> resourceComponents = [];
                ServiceResponse serviceResponse = new();

                // Get the current components
                serviceResponse = await ResourceComponentService.GetItems(true);
                if (serviceResponse.Success)
                {
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        resourceComponents = (List<ResourceComponent>)serviceResponse.ResponseObject!;

                        if (GeneralHelper.IsNotNull(resourceComponents))
                        {
                            // Check if it's a custom component
                            if (type == "CustomComponent")
                            {
                                if (GeneralHelper.IsNotNull(parentcomponent))
                                {
                                    resourceComponent = resourceComponents.Find(x => GeneralHelper.NormalizeName(x.Name, true) == GeneralHelper.NormalizeName(parentcomponent, true))!;
                                }
                            }
                            else
                            {
                                resourceComponent = resourceComponents.Find(x => x.Name == type)!;
                            }

                            if (GeneralHelper.IsNotNull(resourceComponent))
                            {
                                // Check if the name mathces the length requirements for the component
                                if ((value.Length >= (Convert.ToInt32(resourceComponent.MinLength)) && (value.Length <= Convert.ToInt32(resourceComponent.MaxLength))))
                                {
                                    valid = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return valid;
        }

        /// <summary>
        /// Validates a generated name.
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        /// <param name="name">The generated name.</param>
        /// <param name="delimiter">The delimiter used in the generated name.</param>
        /// <returns>A ValidateNameResponse object containing the validation result.</returns>
        public static ValidateNameResponse ValidateGeneratedName(Models.ResourceType resourceType, string name, string delimiter)
        {
            ValidateNameResponse response = new();
            try
            {
                bool valid = true;
                StringBuilder sbMessage = new();

                // Check if the resource type only allows lowercase
                if (!resourceType.Regx.Contains("A-Z"))
                {
                    sbMessage.Append("This resource type only allows lowercase names. The generated name has been updated to lowercase characters.");
                    name = name.ToLower();                
                }

                // Check regex
                // Validate the name against the resource type regex
                Regex regx = new(resourceType.Regx);
                Match match = regx.Match(name);
                bool delimitervalid = false;
                // Check to see if the delimiter has been set
                if (!String.IsNullOrEmpty(delimiter))
                {
                    delimitervalid = true;
                }
                if (!match.Success)
                {
                    if (delimitervalid)
                    {
                        // Strip the delimiter in case that is causing the issue
                        name = name.Replace(delimiter, "");

                        Match match2 = regx.Match(name);
                        if (!match2.Success)
                        {
                            sbMessage.Append("Regex failed - Please review the Resource Type Naming Guidelines.");
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                        else
                        {
                            sbMessage.Append("The specified delimiter was removed. This is often caused by the length of the name exceeding the max length and the delimiter removed to shorten the value or the delimiter is not an allowed character for the resoure type.");
                            sbMessage.Append(Environment.NewLine);
                        }
                    }
                    else
                    {
                        sbMessage.Append("Regex failed - Please review the Resource Type Naming Guidelines.");
                        sbMessage.Append(Environment.NewLine);
                        valid = false;
                    }
                }


                // Check min length
                if (int.TryParse(resourceType.LengthMin, out _))
                {
                    if (name.Length < int.Parse(resourceType.LengthMin))
                    {
                        sbMessage.Append("Generated name is less than the minimum length for the selected resource type.");
                        sbMessage.Append(Environment.NewLine);
                        valid = false;
                    }
                }

                // Check max length
                if (int.TryParse(resourceType.LengthMax, out _))
                {
                    if (name.Length > int.Parse(resourceType.LengthMax))
                    {
                        // Strip the delimiter in case that is causing the issue
                        name = name.Replace(delimiter, "");
                        if (name.Length > int.Parse(resourceType.LengthMax))
                        {
                            sbMessage.Append("Generated name is more than the maximum length for the selected resource type.");
                            sbMessage.Append(Environment.NewLine);
                            sbMessage.Append("Please remove any optional components or contact your admin to update the required components for this resource type.");
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                        else
                        {
                            sbMessage.Append("Generated name with the selected delimiter is more than the maximum length for the selected resource type. The delimiter has been removed.");
                            sbMessage.Append(Environment.NewLine);
                        }
                    }
                }

                // Check invalid characters
                if (!String.IsNullOrEmpty(resourceType.InvalidCharacters))
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharacters)
                    {
                        // Check if the name contains the character
                        if (name.Contains(c))
                        {
                            sbMessage.Append("Name cannot contain the following character: " + c);
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Check start character
                if (!String.IsNullOrEmpty(resourceType.InvalidCharactersStart))
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharactersStart)
                    {
                        // Check if the name contains the character
                        if (name.StartsWith(c))
                        {
                            sbMessage.Append("Name cannot start with the following character: " + c);
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Check start character
                if (!String.IsNullOrEmpty(resourceType.InvalidCharactersEnd))
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharactersEnd)
                    {
                        // Check if the name contains the character
                        if (name.EndsWith(c))
                        {
                            sbMessage.Append("Name cannot end with the following character: " + c);
                            sbMessage.Append(Environment.NewLine);
                            valid = false;
                        }
                    }
                }

                // Check consecutive character
                if (!String.IsNullOrEmpty(resourceType.InvalidCharactersConsecutive))
                {
                    // Loop through each character
                    foreach (char c in resourceType.InvalidCharactersConsecutive)
                    {
                        // Check if the name contains the character
                        char current = name[0];
                        for (int i = 1; i < name.Length; i++)
                        {
                            char next = name[i];
                            if ((current == next) && (current == c))
                            {
                                sbMessage.Append("Name cannot contain the following consecutive character: " + next);
                                sbMessage.Append(Environment.NewLine);
                                valid = false;
                                break;
                            }
                            current = next;
                        }
                    }
                }


                response.Valid = valid;
                response.Name = name;
                response.Message = sbMessage.ToString();
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                response.Valid = false;
                response.Name = name;
                response.Message = "There was a problem validating the name.";
            }

            return response;
        }

        /// <summary>
        /// Checks if a string contains only numeric characters.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string contains only numeric characters, otherwise false.</returns>
        public static bool CheckNumeric(string value)
        {
            Regex regx = new("^[0-9]+$");
            Match match = regx.Match(value);
            return match.Success;
        }

        /// <summary>
        /// Checks if a string contains only alphanumeric characters.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string contains only alphanumeric characters, otherwise false.</returns>
        public static bool CheckAlphanumeric(string value)
        {
            Regex regx = new("^[a-zA-Z0-9]+$");
            Match match = regx.Match(value);
            return match.Success;
        }

        /// <summary>
        /// Checks if a component value length is valid.
        /// </summary>
        /// <param name="component">The resource component.</param>
        /// <param name="value">The component value.</param>
        /// <returns>True if the component value length is valid, otherwise false.</returns>
        public static bool CheckComponentLength(ResourceComponent component, string value)
        {
            // Check if the component value length is valid
            if ((value.Length < int.Parse(component.MinLength)) || (value.Length > int.Parse(component.MaxLength)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
