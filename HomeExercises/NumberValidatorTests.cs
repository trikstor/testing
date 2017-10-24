using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	public class NumberValidatorTests
	{
        private NumberValidator testValidator_3_2 { get; set; }
        private NumberValidator testValidator_17_2{ get; set; }
        private NumberValidator testValidator_17_2_true{ get; set; }

        [SetUp]
        public void SetUp()
        {
            testValidator_3_2 = new NumberValidator(3, 2);
            testValidator_17_2 = new NumberValidator(17, 2);
            testValidator_17_2_true = new NumberValidator(17, 2, true);
        }

	    [Test]
	    public void Constructor_ThrowException_PrecisionIsZeroOrLess()
	    {
	        Action res = () =>
	        {
                var numberValidator  = new NumberValidator(0);
	        };

            	res.ShouldThrow<ArgumentException>("zero precision");
	    }

	    [Test]
        public void Constructor_ThrowException_ScaleIsLessThenZero()
	    {
	        Action res = () =>
	        {
	            var numberValidator = new NumberValidator(0, -1);
	        };

	        res.ShouldThrow<ArgumentException>("scale is negative (-1)");
	    }

	    [Test]
        public void Constructor_ThrowException_ScaleIsMoreThenPrecision()
	    {
	        Action res = () =>
	        {
	            var numberValidator = new NumberValidator(0, 1);
	        };

	        res.ShouldThrow<ArgumentException>("scale is more then precision (0 < 1)");
	    }

	    [Test]
        public void Constructor_NotThrowException_PrecisionIsMoreThenZeroAndScaleIsLessThenPrecision()
	    {
	        Action res = () =>
	        {
	            var numberValidator = new NumberValidator(1);
	        };

	        res.ShouldNotThrow<ArgumentException>();
	    }

	    [Test]
        public void IsValidNumber_ReturnsFalse_IsNull()
	    {
            testValidator_3_2.IsValidNumber(null).Should().BeFalse();
	    }

	    [Test]
	    public void IsValidNumber_ReturnsFalse_IsEmpty()
	    {
            testValidator_3_2.IsValidNumber("").Should().BeFalse();
	    }

	    [Test]
	    public void IsValidNumber_ReturnsFalse_IsLetters()
	    {
            testValidator_3_2.IsValidNumber("a.sd").Should().BeFalse();
	    }

        [Test]
        public void IsValidNumber_ReturnsFalse_WithoutOneValue()
        {
            testValidator_3_2.IsValidNumber("0.").Should().BeFalse();
            testValidator_3_2.IsValidNumber(".0").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_ReturnsTrue_CommaBetweenValues()
        {
            testValidator_3_2.IsValidNumber("0,00").Should().BeTrue();
        }
        
	    [Test]
        public void IsValidNumber_ReturnsFalse_FracPartIsMoreThenScale()
	    {
            testValidator_17_2.IsValidNumber("0.000").Should().BeFalse();
	    }

	    [Test]
        public void IsValidNumber_ReturnsFalse_IntPartPlusFracPartIsMoreThenPrecision()
	    {
	        testValidator_3_2.IsValidNumber("+1.23").Should().BeFalse();
	    }

	    [Test]
        public void IsValidNumber_ReturnsFalse_onlyPositiveAndSignIsNegative()
	    {
            testValidator_17_2_true.IsValidNumber("-1.23").Should().BeFalse(); 
	    }

	    [Test]
	    public void IsValidNumber_ReturnsTrue_onlyPositiveAndSignIsPositive()
	    {
            testValidator_17_2_true.IsValidNumber("+1.23").Should().BeTrue();
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
