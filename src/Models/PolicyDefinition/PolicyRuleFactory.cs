using AzureNamingTool.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a factory for generating policy rules.
    /// </summary>
    public class PolicyRuleFactory
    {
        /// <summary>
        /// Gets the name validation rules for the specified policies.
        /// </summary>
        /// <param name="policies">The list of policy rules.</param>
        /// <param name="delimiter">The delimiter character.</param>
        /// <param name="effect">The policy effect.</param>
        /// <returns>The JSON representation of the policy rule.</returns>
        internal static string GetNameValidationRules(List<PolicyRule> policies, Char delimiter, PolicyEffects effect = PolicyEffects.Deny)
        {
            var ifHeader = "\"if\": {\"allOf\": [";
            var policyGroups = policies.GroupBy(x => String.Join(',', x.Group));
            var ifContent = GenerateConditions(policyGroups);
            var ifFooter = "]}";
            var thenContent = ", \"then\": {\"effect\":\"" + effect.ToString().ToLower() + "\"}";
            var thisdelimiter = delimiter.ToString();

            return "\"policyRule\": {" + ifHeader + ifContent + ifFooter + thenContent + "}";
        }

        /// <summary>
        /// Gets the main condition for the specified list of policy rules.
        /// </summary>
        /// <param name="conditions">The list of policy rules.</param>
        /// <returns>The JSON representation of the main condition.</returns>
        static string GetMainCondition(List<PolicyRule> conditions)
        {
            return "{\"not\": { \"value\": \"[substring(field('name'), " + conditions.First().StartIndex + ", " + conditions.First().Length + ")]\",\"in\": [" + String.Join(',', conditions.Select(x => "\"" + x.Name + "\"").Distinct()) + "]}}";
        }

        /// <summary>
        /// Generates the conditions for the specified policy groups.
        /// </summary>
        /// <param name="policyGroups">The policy groups.</param>
        /// <param name="level">The level of the conditions.</param>
        /// <param name="startIndex">The start index of the conditions.</param>
        /// <returns>The JSON representation of the conditions.</returns>
        private static string GenerateConditions(IEnumerable<IGrouping<string, PolicyRule>> policyGroups, int level = 1, int startIndex = 0)
        {
            String result = String.Empty;
            var list = policyGroups.Where(x => x.Key.StartsWith($"{level},{startIndex}")).ToList();
            foreach (var levelConditions in list)
            {
                if (GeneralHelper.IsNotNull(levelConditions))
                {
                    var header = "{\"allOf\": [";
                    var mainCondition = GetMainCondition([.. levelConditions]);
                    var insideConditions = String.Empty;
                    var fullLength = levelConditions.FirstOrDefault()!.FullLength;
                    var startIndexes = policyGroups.Where(x => x.Key.StartsWith($"{level + 1}")).Select(x => Convert.ToInt32(x.Key.Split(',')[1])).Where(x => x == fullLength).Distinct().ToList();
                    foreach (var nextStartIndex in startIndexes)
                    {
                        insideConditions += GenerateConditions(policyGroups, level + 1, nextStartIndex);
                        if (startIndexes.Last() != nextStartIndex)
                            insideConditions += ",";
                    }

                    var footer = "]}";

                    if (list.Last().Key != levelConditions.Key)
                        footer += ",";

                    result += header + mainCondition + (insideConditions == String.Empty ? String.Empty : "," + insideConditions) + footer;
                }
            }

            return result;
        }
    }
}
