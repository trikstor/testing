using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    [TestFixture]
    public class NumberValidatorTests
    {
        [TestCase(0, 0, false, "precision must be a positive number", 
            TestName = "When precision is zero or less")]
        [TestCase(1, -1, false, "precision must be a non-negative number less or equal than precision", 
            TestName = "When scale is less then zero")]
        [TestCase(1, 1, false, "precision must be a non-negative number less or equal than precision", 
            TestName = "When scale is more then precision")]

        public void Constructor_ThrowException(int precision, int scale, bool onlyPositive, string exMessage)
        {
            Action res = () => { new NumberValidator(precision, scale, onlyPositive); };
            res.ShouldThrow<ArgumentException>().WithMessage(exMessage);
        }


        [TestCase(1, 0, false, TestName = 
            "When precision is more then zero and scale is less then precision")]
        public void Constructor_NotThrowException(int precision, int scale, bool onlyPositive)
        {
            Action res = () => { new NumberValidator(precision, scale, onlyPositive); };
            res.ShouldNotThrow<ArgumentException>();
        }


        [TestCase(3, 2, false, null, ExpectedResult = false, TestName = "When value is null")]
        [TestCase(3, 2, false, "", ExpectedResult = false, TestName = "When value string is empty")]
        [TestCase(3, 2, false, "a.sd", ExpectedResult = false, TestName = "When value from letters")]
        [TestCase(3, 2, false, "0.", ExpectedResult = false, TestName = "When value without one signification (integer part)")]
        [TestCase(3, 2, false, ".0", ExpectedResult = false, TestName = 
            "When value without one signification (fractional part)")]
        [TestCase(3, 2, false, "0.000", ExpectedResult = false, TestName = "When fractional part is more then scale")]
        [TestCase(3, 2, false, "+1.23", ExpectedResult = false, TestName = 
            "When integer part plus fractional part is more then precision")]
        [TestCase(17, 2, true, "-1.23", ExpectedResult = false, TestName = "When only positive values and sign is negative")]

        [TestCase(17, 2, true, "+1.23", ExpectedResult = true, TestName = "When only positive values and sign is positive")]
        [TestCase(3, 2, false, "0,00", ExpectedResult = true, TestName = "When comma is between values")]

        public bool IsValidNumber(int precision, int scale, bool positiveOnly, string paramValue)
        {
            return new NumberValidator(precision, scale, positiveOnly).IsValidNumber(paramValue);
        }
    }

    public class NumberValidator
    {
        private readonly Regex numberRegex;
        private readonly bool onlyPositive;
        private readonly int precision;
        private readonly int scale;

        public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
        {
            this.precision = precision;
            this.scale = scale;
            this.onlyPositive = onlyPositive;
            if (precision <= 0)
                throw new ArgumentException("precision must be a positive number");
            if (scale < 0 || scale >= precision)
                throw new ArgumentException("precision must be a non-negative number less or equal than precision");
            numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
        }

        public bool IsValidNumber(string value)
        {
            // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
            // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
            // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
            // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
            // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

            if (string.IsNullOrEmpty(value))
                return false;

            var match = numberRegex.Match(value);
            if (!match.Success)
                return false;

            // Знак и целая часть
            var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
            // Дробная часть
            var fracPart = match.Groups[4].Value.Length;

            if (intPart + fracPart > precision || fracPart > scale)
                return false;

            if (onlyPositive && match.Groups[1].Value == "-")
                return false;
            return true;
        }
    }
}
