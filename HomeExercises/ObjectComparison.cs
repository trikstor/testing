using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	public class ObjectComparison
	{
		[Test]
		[Description("Проверка текущего царя")]
		[Category("ToRefactor")]
		public void CheckCurrentTsar()
		{
			var actualTsar = TsarRegistry.GetCurrentTsar();
			var expectedTsar = new Person(
			    "Ivan IV The Terrible", 54, 170, 70,
			    new Person("Vasili III of Russia", 28, 170, 60, null));

            // Перепишите код на использование Fluent Assertions.
            /*
			Assert.AreEqual(actualTsar.Name, expectedTsar.Name);
			Assert.AreEqual(actualTsar.Age, expectedTsar.Age);
			Assert.AreEqual(actualTsar.Height, expectedTsar.Height);
			Assert.AreEqual(actualTsar.Weight, expectedTsar.Weight);

			Assert.AreEqual(expectedTsar.Parent.Name, actualTsar.Parent.Name);
			Assert.AreEqual(expectedTsar.Parent.Age, actualTsar.Parent.Age);
			Assert.AreEqual(expectedTsar.Parent.Height, actualTsar.Parent.Height);
			Assert.AreEqual(expectedTsar.Parent.Parent, actualTsar.Parent.Parent);
            */

            Func<FieldInfo, Person, string> TakeVal = (fieldAbout, currTsar) =>
		        fieldAbout.GetValue(currTsar) != null ? fieldAbout.GetValue(currTsar).ToString() : null;

            var fields = typeof(Person).GetFields();
		    foreach (var fieldAbout in fields.Where(field => field.Name != "Id"))
		    {
		        TakeVal(fieldAbout, actualTsar)
		            .Should().BeEquivalentTo(TakeVal(fieldAbout, expectedTsar));

		        TakeVal(fieldAbout, actualTsar.Parent)
		            .Should().BeEquivalentTo(TakeVal(fieldAbout, expectedTsar.Parent));
            }
        }

		[Test]
		[Description("Альтернативное решение. Какие у него недостатки?")]
		public void CheckCurrentTsar_WithCustomEquality()
		{
			var actualTsar = TsarRegistry.GetCurrentTsar();
			var expectedTsar = new Person(
			    "Ivan IV The Terrible", 54, 170, 70,
			    new Person("Vasili III of Russia", 28, 170, 60, null));

            // Какие недостатки у такого подхода? 
            /*
             * Данный метод не включает в себя полного набора Assert-ов, 
             * получается что производится тестирование работы стороннего метода, 
             * в котром также могут содержаться потенциальные ошибки.
             * 
             * Over specification: в методе AreEqual производится проверка 
             * экземпляров класса Person на null, хотя даже если бы его не было, тест 
             * на данных с null не был бы пройден.
             * Проверка actual == expected также лишняя, так как поля уже сопоставляются 
             * в данном методе.
             * 
             * Мое решение лучше тем, что при любом изменении количества полей 
             * не требуется изменять тест, тест стал расширяем.
             */
            Assert.True(AreEqual(actualTsar, expectedTsar));
		}

		private bool AreEqual(Person actual, Person expected)
		{
			if (actual == expected) return true;
			if (actual == null || expected == null) return false; 
			return
			actual.Name == expected.Name
			&& actual.Age == expected.Age
			&& actual.Height == expected.Height
			&& actual.Weight == expected.Weight
			&& AreEqual(actual.Parent, expected.Parent);
		}
	}

	public class TsarRegistry
	{
		public static Person GetCurrentTsar()
		{
			return new Person(
				"Ivan IV The Terrible", 54, 170, 70,
				new Person("Vasili III of Russia", 28, 170, 60, null));
		}
	}

	public class Person
	{
		public static int IdCounter = 0;
		public int Age, Height, Weight;
		public string Name;
		public Person Parent;
		public int Id;

		public Person(string name, int age, int height, int weight, Person parent)
		{
			Id = IdCounter++;
			Name = name;
			Age = age;
			Height = height;
			Weight = weight;
			Parent = parent;
		}
	}
}
