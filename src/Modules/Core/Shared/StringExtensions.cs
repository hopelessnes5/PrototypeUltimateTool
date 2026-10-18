using System.Text.RegularExpressions;

namespace Nixson.Common
{
    /// <summary>
    /// String formatting helpers used by Nixson.Common's property-grid converters.
    /// Kept in this source project so the utility does not depend on the unrelated
    /// FightData code or on an extension method from a different Gibbed snapshot.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Inserts spaces between a lowercase/digit character and the following
        /// uppercase character: "VertexBuffer" becomes "Vertex Buffer".
        /// Matches the behavior used by EditP3D's FightDataStringExtensions.
        /// </summary>
        internal static string SeparateCamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return Regex.Replace(value, "([a-z0-9])([A-Z])", "$1 $2");
        }
    }
}
