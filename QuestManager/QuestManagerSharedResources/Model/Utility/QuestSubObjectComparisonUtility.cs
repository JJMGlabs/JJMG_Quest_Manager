using QuestManagerSharedResources.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuestManagerSharedResources.Model.Utility
{
    /// <summary>
    /// Evaluates a progress value against a target using a SubObjectComparator. Supports numeric (float), datetime, and string comparisons.
    /// </summary>
    public static class QuestSubObjectComparisonUtility
    {
        static Dictionary<string, SubObjectComparator> _lowerCaseStringToComparitor = new Dictionary<string, SubObjectComparator>() {
            { "==" , SubObjectComparator.EQUAL},
            { "equal" , SubObjectComparator.EQUAL},
            { "<" , SubObjectComparator.LESS},
            { "less" , SubObjectComparator.LESS},
            { "<=" , SubObjectComparator.LESSOREQUAL},
            { "lessorequal" , SubObjectComparator.LESSOREQUAL},
            { "lessequal" , SubObjectComparator.LESSOREQUAL},
            { ">" , SubObjectComparator.GREATER},
            { "greater" , SubObjectComparator.GREATER},
            { ">=" , SubObjectComparator.GREATEROREQUAL},
            { "greaterorequal" , SubObjectComparator.GREATEROREQUAL},
            { "greaterequal" , SubObjectComparator.GREATEROREQUAL},
            { "!=" , SubObjectComparator.NOTEQUAL},
            { "notequal" , SubObjectComparator.NOTEQUAL}
        };

        /// <summary>
        /// Evaluates the comparison using a string operator name (e.g., "equal", ">", "LESS"). Resolves to the enum overload.
        /// </summary>
        public static bool PerformComparison(string valueToCompare, string comparitor, string targetValue)
        {
            var compare = InterperetComparitorFromString(comparitor);
            return PerformComparison(valueToCompare, compare, targetValue);
        }

        /// <summary>
        /// Evaluates the comparison using a SubObjectComparator. Automatically parses numeric and datetime values; falls back to string equality for other types.
        /// </summary>
        public static bool PerformComparison(string valueToCompare, SubObjectComparator comparitor, string targetValue)
        {
            if (float.TryParse(valueToCompare, out float floatValueToCompare) && float.TryParse(targetValue, out float floatTargetValue))
                return Compare(floatValueToCompare, comparitor, floatTargetValue);
            if (DateTime.TryParse(valueToCompare, out DateTime dateValueToCompare) && DateTime.TryParse(targetValue, out DateTime dateTargetValue))
                return Compare(dateValueToCompare, comparitor, dateTargetValue);

            return Compare(valueToCompare, comparitor, targetValue);
        }

        static bool Compare(float valueToCompare, SubObjectComparator comparitor, float targetValue)
        {
            switch (comparitor)
            {
                case SubObjectComparator.GREATER:
                    return valueToCompare > targetValue;
                case SubObjectComparator.LESS:
                    return valueToCompare < targetValue;
                case SubObjectComparator.EQUAL:
                    return valueToCompare == targetValue;
                case SubObjectComparator.NOTEQUAL:
                    return valueToCompare != targetValue;
                case SubObjectComparator.GREATEROREQUAL:
                    return valueToCompare >= targetValue;
                case SubObjectComparator.LESSOREQUAL:
                    return valueToCompare <= targetValue;
                default:
                    return false;
            }
        }
        static bool Compare(DateTime valueToCompare, SubObjectComparator comparitor, DateTime targetValue)
        {
            switch (comparitor)
            {
                case SubObjectComparator.GREATER:
                    return valueToCompare > targetValue;
                case SubObjectComparator.LESS:
                    return valueToCompare < targetValue;
                case SubObjectComparator.EQUAL:
                    return valueToCompare == targetValue;
                case SubObjectComparator.NOTEQUAL:
                    return valueToCompare != targetValue;
                case SubObjectComparator.GREATEROREQUAL:
                    return valueToCompare >= targetValue;
                case SubObjectComparator.LESSOREQUAL:
                    return valueToCompare <= targetValue;
                default:
                    return false;
            }
        }

        static bool Compare(string valueToCompare, SubObjectComparator comparitor, string targetValue)
        {
            switch (comparitor)
            {
                case SubObjectComparator.EQUAL:
                    return valueToCompare == targetValue;
                case SubObjectComparator.NOTEQUAL:
                    return valueToCompare != targetValue;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Resolves a string operator name or symbol to a SubObjectComparator enum value. Case-insensitive.
        /// </summary>
        public static SubObjectComparator InterperetComparitorFromString(string camparitorString)
        {
            return _lowerCaseStringToComparitor[camparitorString.ToLower()];
        }
    }
}
