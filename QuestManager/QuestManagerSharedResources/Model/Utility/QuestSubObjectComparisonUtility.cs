using QuestManagerSharedResources.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuestManagerSharedResources.Model.Utility
{
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

        public static bool PerformComparison(string valueToCompare, string comparitor, string targetValue)
        {
            var compare = InterperetComparitorFromString(comparitor);
            return PerformComparison(valueToCompare, compare, targetValue);
        }

        public static bool PerformComparison(string valueToCompare, SubObjectComparator comparitor, string targetValue)
        {
            if (float.TryParse(targetValue, out float floatValue))
                return Compare(float.Parse(valueToCompare), comparitor, floatValue);
            if (DateTime.TryParse(targetValue, out DateTime datetimeValue))
                return Compare(DateTime.Parse(targetValue), comparitor, datetimeValue);

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

        public static SubObjectComparator InterperetComparitorFromString(string camparitorString)
        {
            return _lowerCaseStringToComparitor[camparitorString.ToLower()];
        }
    }
}
