using System;
using System.Linq;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a policy rule.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PolicyRule"/> class.
    /// </remarks>
    /// <param name="fullName">The full name of the policy rule.</param>
    /// <param name="delimeter">The delimiter character used in the full name.</param>
    public class PolicyRule(String fullName, Char delimeter = '-')
    {

        /// <summary>
        /// Gets or sets the delimiter character used in the full name.
        /// </summary>
        public Char Delimeter { get; set; } = delimeter;

        /// <summary>
        /// Gets or sets the full name of the policy rule.
        /// </summary>
        public String FullName { get; set; } = fullName;

        /// <summary>
        /// Gets the level of the policy rule based on the number of delimiters in the full name.
        /// </summary>
        public int Level { get { return FullName.Count(p => p == Delimeter); } }

        /// <summary>
        /// Gets the name of the policy rule extracted from the full name.
        /// </summary>
        public String Name { get { return FullName[StartIndex..LastIndex]; } }

        /// <summary>
        /// Gets the length of the policy rule name.
        /// </summary>
        public int Length { get { return Name.Length; } }

        /// <summary>
        /// Gets the full length of the policy rule name including the delimiter.
        /// </summary>
        public int FullLength { get { return FullName.Length - 1; } }

        /// <summary>
        /// Gets the start index of the policy rule name in the full name.
        /// </summary>
        public int StartIndex { get { return BeforeLastIndexOf(FullName, Delimeter); } }

        /// <summary>
        /// Gets the last index of the delimiter in the full name.
        /// </summary>
        public int LastIndex { get { return FullName.LastIndexOf(Delimeter); } }

        /// <summary>
        /// Gets the index before the last occurrence of the delimiter in the full name.
        /// </summary>
        /// <param name="value">The full name.</param>
        /// <param name="toFind">The delimiter character.</param>
        /// <returns>The index before the last occurrence of the delimiter.</returns>
        static int BeforeLastIndexOf(String value, Char toFind)
        {
            int result = 0;
            for (int i = value.LastIndexOf(toFind) - 1; i > 0; i--)
            {
                if (value[i] == toFind)
                {
                    result = i + 1;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets an array containing the level, start index, and length of the policy rule.
        /// </summary>
        public int[] Group { get { return [Level, StartIndex, Length]; } }

        /// <summary>
        /// Returns the hash code for the policy rule.
        /// </summary>
        /// <returns>The hash code for the policy rule.</returns>
        public override int GetHashCode()
        {
            return (String.Join(',', Group) + Name).GetHashCode();
        }
    }
  }