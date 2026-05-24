using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuestManagerSharedResources.Model.Enums;
using QuestManagerSharedResources.Model.Utility;

namespace QuestManagerSharedResources.Model.Tests
{
    [TestClass()]
    public class QuestSubObjectComparisonUtilityTests
    {
        [TestMethod()]
        public void PerformComparisonTest_Equal_Numeric()
        {
            Assert.IsTrue(QuestSubObjectComparisonUtility.PerformComparison("5", SubObjectComparator.EQUAL, "5"));
        }

        [TestMethod()]
        public void PerformComparisonTest_Equal_Numeric_False()
        {
            Assert.IsFalse(QuestSubObjectComparisonUtility.PerformComparison("5", SubObjectComparator.EQUAL, "6"));
        }

        [TestMethod()]
        public void PerformComparisonTest_Greater()
        {
            Assert.IsTrue(QuestSubObjectComparisonUtility.PerformComparison("6", SubObjectComparator.GREATER, "5"));
        }

        [TestMethod()]
        public void PerformComparisonTest_Less()
        {
            Assert.IsTrue(QuestSubObjectComparisonUtility.PerformComparison("4", SubObjectComparator.LESS, "5"));
        }

        [TestMethod()]
        public void PerformComparisonTest_GreaterOrEqual()
        {
            Assert.IsTrue(QuestSubObjectComparisonUtility.PerformComparison("5", SubObjectComparator.GREATEROREQUAL, "5"));
        }

        [TestMethod()]
        public void PerformComparisonTest_LessOrEqual()
        {
            Assert.IsTrue(QuestSubObjectComparisonUtility.PerformComparison("5", SubObjectComparator.LESSOREQUAL, "6"));
        }

        [TestMethod()]
        public void PerformComparisonTest_NotEqual()
        {
            Assert.IsTrue(QuestSubObjectComparisonUtility.PerformComparison("5", SubObjectComparator.NOTEQUAL, "6"));
        }

        [TestMethod()]
        public void PerformComparisonTest_StringEquality()
        {
            Assert.IsTrue(QuestSubObjectComparisonUtility.PerformComparison("abc", SubObjectComparator.EQUAL, "abc"));
        }

        [TestMethod()]
        public void PerformComparisonTest_DateTime()
        {
            Assert.IsTrue(QuestSubObjectComparisonUtility.PerformComparison("2024-01-15", SubObjectComparator.GREATER, "2024-01-01"));
        }

        [TestMethod()]
        public void PerformComparisonTest_StringComparator()
        {
            Assert.IsTrue(QuestSubObjectComparisonUtility.PerformComparison("5", "EQUAL", "5"));
        }
    }
}
