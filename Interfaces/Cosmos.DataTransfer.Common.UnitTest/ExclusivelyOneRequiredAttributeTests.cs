using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using Cosmos.DataTransfer.Common;

namespace Cosmos.DataTransfer.Common.UnitTest;

[TestClass]
public class ExclusivelyOneRequiredAttributeTests {

    // Test helper
    private static IEnumerable<string?> GetValidationErrors(object x) {
        ExclusivelyOneRequiredAttribute[] attrs = (ExclusivelyOneRequiredAttribute[]) 
            Attribute.GetCustomAttributes(x.GetType(), 
                typeof(ExclusivelyOneRequiredAttribute), inherit: true);
        foreach (var attr in attrs) {
            foreach (var s in attr.ValidateObject(x)) {
                yield return s.ErrorMessage;
            }
        }
    }

    private class DummyTestClass {
        [Required]
        public string? Name { get; set; }
    }

    [TestMethod]
    public void TestDummyClass() {
        var x = new DummyTestClass();

        var context = new ValidationContext(x, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(x, context, results, true);
        
        Assert.AreEqual(results.Count, 1);
        Assert.AreEqual(results[0].ErrorMessage, "The Name field is required.");

        x.Name = "foobar";
        results = new List<ValidationResult>();
        Validator.TryValidateObject(x, context, results, true);
        Assert.AreEqual(results.Count, 0);
    }

    [ExclusivelyOneRequired("Name", "FilePath", "FooBar")]
    private class TestDummyClass1 : DummyTestClass {
        public string? FilePath { get; set; }
        public string? FooBar { get; set; }
    }

    [TestMethod]
    public void TestClassAttributeCanBeFound() {
        var x = new TestDummyClass1();

        ExclusivelyOneRequiredAttribute[] attrs = (ExclusivelyOneRequiredAttribute[]) 
            Attribute.GetCustomAttributes(x.GetType(), 
                typeof(ExclusivelyOneRequiredAttribute), inherit: true);
        Assert.IsTrue(Attribute.IsDefined(x.GetType(), typeof(ExclusivelyOneRequiredAttribute)));
    }

    [TestMethod]
    [DataRow(null, null, null, "None of the required properties were provided. TestDummyClass1 requires exactly one of the properties: Name, FilePath, FooBar")]
    [DataRow("", null, "", "None of the required properties were provided. TestDummyClass1 requires exactly one of the properties: Name, FilePath, FooBar")]
    [DataRow("foo", "bar", null, "Multiple properties were given where exactly one is required. Offending properties (reduce to one): Name, FilePath")]
    [DataRow("foo", "", "bar", "Multiple properties were given where exactly one is required. Offending properties (reduce to one): Name, FooBar")]
    [DataRow("foo", null, null, null)]
    public void TestClassAttributeFails(string? Name, string? FilePath, 
        string? FooBar, string ErrorMessage) {
        var x = new TestDummyClass1() {
            Name = Name,
            FilePath = FilePath,
            FooBar = FooBar
        };

        var errors = GetValidationErrors(x).ToList();
        if (ErrorMessage == null) {
            Assert.Equals(errors.Count, 0);
        } else {
            Assert.Equals(ErrorMessage, errors[0]);
        }
    }

    [ExclusivelyOneRequired("nonexisting")]
    private class TestDummyClass2 : DummyTestClass {
        public string? FilePath { get; set; }
    }

    [TestMethod]
    public void TestFailureOnNonexistingProperty() {
        var x = new TestDummyClass2() { FilePath = "foo" };
        var e = Assert.ThrowsException<ArgumentException>(() => 
            ExclusivelyOneRequiredAttributeTests.GetValidationErrors(x).ToList());
        Assert.AreEqual(e.ParamName, "nonexisting");
    }
}
