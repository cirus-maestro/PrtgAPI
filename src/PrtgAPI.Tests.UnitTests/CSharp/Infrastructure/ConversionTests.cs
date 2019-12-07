﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Infrastructure
{
    [TestClass]
    public class ConversionTests
    {
        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_Integer() =>
            TestDouble("19", 19.0000, 19);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_DecimalBelowZero() =>
            TestDouble("0.1", 0.1000, 0.1);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_DecimalBelowZero() =>
            TestDouble("0,1", 0.1000, 0.1);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_DecimalAboveZero_TwoDecimalPlaces() =>
            TestDouble("60.95", 60.9500, 60.95);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_DecimalAboveZero_TwoDecimalPlaces() =>
            TestDouble("60,95", 60.9500, 60.95);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_DecimalAboveZero_ThreeDecimalPlaces() =>
            TestDouble("60.953", 60.9530, 60.953);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_DecimalAboveZero_ThreeDecimalPlaces() =>
            TestDouble("60,953", 60.9530, 60.953);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Thousand() =>
            TestDouble("3,726", 4000762036224.0000, 3726);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Thousand() =>
            TestDouble("3.726", 4000762036224.0000, 3726);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Thousand_WithDecimal_TwoDecimalPlaces() =>
            TestDouble("3,726.21", 4000762036224.2100, 3726.21);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Thousand_WithDecimal_TwoDecimalPlaces() =>
            TestDouble("3.726,21", 4000762036224.2100, 3726.21);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Thousand_WithDecimal_ThreeDecimalPlaces() =>
            TestDouble("3,726.213", 4000762036224.2130, 3726.213);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Thousand_WithDecimal_ThreeDecimalPlaces() =>
            TestDouble("3.726,213", 4000762036224.2130, 3726.213);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Million_WithDecimal() =>
            TestDouble("3,726,432.21", 4000762036224.2100, 3726432.21);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Million_WithDecimal() =>
            TestDouble("3.726.432,21", 4000762036224.2100, 3726432.21);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_SameInteger_DifferentDecimal() =>
            TestDouble("851,337.123", 851337.1429, 851337.123);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_SameInteger_DifferentDecimal() =>
            TestDouble("851.337,123", 851337.1429, 851337.123);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_SameInteger_NoDecimal() =>
            TestDouble("851,337", 851337.1429, 851337);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_SameInteger_NoDecimal() =>
            TestDouble("851.337", 851337.1429, 851337);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_RoundUp_ToInteger() =>
            TestDouble("16,237", 16236.8736, 16237);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_RoundUp_ToInteger() =>
            TestDouble("16.237", 16236.8736, 16237);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_RoundUp_StringHasMoreDecimalPlaces() =>
            TestDouble("1.666667", 1.6667, 1.666667);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_RoundUp_StringHasMoreDecimalPlaces() =>
            TestDouble("1,666667", 1.6667, 1.666667);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_RoundUp_RawHasMoreDecimalPlaces() =>
            TestDouble("1.6667", 1.666667, 1.6667);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_RoundUp_RawHasMoreDecimalPlaces() =>
            TestDouble("1,6667", 1.666667, 1.6667);

        private void TestDouble(string s, double d, double expected)
        {
            var result = ConvertUtilities.ToDynamicDouble(s, d);

            Assert.AreEqual(expected, result);
        }
    }
}