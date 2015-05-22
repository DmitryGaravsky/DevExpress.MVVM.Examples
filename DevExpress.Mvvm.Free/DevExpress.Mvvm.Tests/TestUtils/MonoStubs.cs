namespace DevExpress.Mvvm.Tests {
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using DevExpress.Mvvm.Native;
    using NUnit.Framework;

#if MONO
    public class BaseWpfFixture {
        class TestContext : System.Threading.SynchronizationContext { 
        }
        [TestFixtureSetUp]
        public void FixtureSetUp() {
            System.Threading.SynchronizationContext.SetSynchronizationContext(new TestContext());
        }
        [TestFixtureTearDown]
        public void FixtureTearDown() {
            System.Threading.SynchronizationContext.SetSynchronizationContext(null);
        }
        [SetUp]
        public void SetUp() {
            SetUpCore();
        }
        [TearDown]
        public void TearDown() {
            TearDownCore();
        }
        protected virtual void SetUpCore() { }
        protected virtual void TearDownCore() { }
    }
    public class AsynchronousAttribute : System.Attribute { }
    //
    public struct Point {
        public Point(int x, int y)
            : this() {
            X = x;
            Y = y;
        }
        public int X { get; set; }
        public int Y { get; set; }
    }
    public interface IValueConverter {
        object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
    public class AssertHelper {
        public static void AssertAllPropertiesAreEqual(object expected, object actual, bool compareTypes = true) {
            AssertAllPropertiesAreEqual(expected, actual, new string[] { }, compareTypes);
        }
        public static void AssertAllPropertiesAreEqual(object expected, object actual, IEnumerable<string> skipProperties, bool compareTypes = true) {
            if(expected == null || actual == null)
                Assert.AreEqual(expected, actual);
            if(compareTypes)
                Assert.AreEqual(expected.GetType(), actual.GetType());
            foreach(PropertyInfo expectedProperty in expected.GetType().GetProperties()) {
                if(skipProperties.Contains(expectedProperty.Name)) continue;
                MethodInfo setter = expectedProperty.GetSetMethod();
                if(setter == null || !setter.IsPublic) continue;
                PropertyInfo actualProperty = actual.GetType().GetProperty(expectedProperty.Name);
                Assert.AreEqual(expectedProperty.Name, actualProperty.With(a => a.Name));
                Assert.AreEqual(expectedProperty.GetValue(expected, null), actualProperty.GetValue(actual, null)); //new List<string> (){"one", "two"}
            }
        }
        public static void AssertEnumerablesAreEqual(IEnumerable expected, IEnumerable actual) {
            AssertEnumerablesAreEqual(expected, actual, false, null, true);
        }
        public static void AssertEnumerablesAreEqual(IEnumerable expected, IEnumerable actual, bool compareByProperties, bool compareTypes = true) {
            AssertEnumerablesAreEqual(expected, actual, compareByProperties, new string[] { }, compareTypes);
        }
        public static void AssertEnumerablesAreEqual(IEnumerable expected, IEnumerable actual, bool compareByProperties, IEnumerable<string> skipProperties, bool compareTypes = true) {
            object[] expectedArray = expected.Cast<object>().ToArray();
            object[] actualArray = actual.Cast<object>().ToArray();
            Assert.AreEqual(expectedArray.Length, actual.Cast<object>().Count());
            for(int i = 0; i < expectedArray.Length; i++) {
                if(compareByProperties)
                    AssertAllPropertiesAreEqual(expectedArray[i], actualArray[i], skipProperties, compareTypes);
                else
                    Assert.AreEqual(expectedArray[i], actualArray[i]);
            }
        }
        public static void AssertSetsAreEqual(IEnumerable expected, IEnumerable actual, bool compareByProperties = false, bool compareTypes = true) {
            List<object> expectedArray = expected.Cast<object>().ToList();
            List<object> actualArray = actual.Cast<object>().ToList();
            Assert.AreEqual(expectedArray.Count, actualArray.Count, "Length of sets does not equal");
            for(int expectedIndex = expectedArray.Count; --expectedIndex >= 0; ) {
                object expectedItem = expectedArray[expectedIndex];
                int? actualIndex = actualArray
                    .Select((x, i) => new { value = x, index = (int?)i })
                    .Where(x => AreEqual(expectedItem, x.value, compareByProperties, compareTypes))
                    .Select(x => x.index)
                    .FirstOrDefault();
                if(actualIndex == null) continue;
                actualArray.RemoveAt(actualIndex.Value);
                expectedArray.RemoveAt(expectedIndex);
            }
            if(actualArray.Count == 0 && expectedArray.Count == 0) return;
            string message = "";
            if(actualArray.Count != 0) {
                message += "Does not expected: ";
                foreach(object item in actualArray) {
                    message += "{" + item + "} ";
                }
                message += "\n";
            }
            if(expectedArray.Count != 0) {
                message += "Expected, but not found: ";
                foreach(object item in expectedArray) {
                    message += "{" + item + "} ";
                }
                message += "\n";
            }
#if !SILVERLIGHT && !NETFX_CORE && !MONO
            throw new SetNotEqualsException(message);
#else
            throw new Exception(message);
#endif
        }
        public static void AssertThrows<T>(Action action, Action<T> checkException = null) where T : Exception {
            try {
                action();
            }
            catch(T e) {
                if(checkException != null)
                    checkException(e);
                return;
            }
            catch(Exception e) {
                Assert.Fail(string.Format("A wrong type of exception was thrown: {0} ({1} was expected)",
                    e.GetType().Name, typeof(T).Name));
            }
            Assert.Fail(string.Format("No exception was thrown ({0} was expected)", typeof(T).Name));
        }
        static bool AreEqual(object a, object b, bool compareByProperties, bool compareTypes) {
            if(!compareByProperties) return object.Equals(a, b);
            if(a == null && b == null) return true;
            if(a == null || b == null) return false;
            if(compareTypes && a.GetType() != b.GetType()) return false;
            foreach(PropertyInfo aProperty in a.GetType().GetProperties()) {
                PropertyInfo bProperty = b.GetType().GetProperty(aProperty.Name);
                if(bProperty == null) return false;
                if(!object.Equals(aProperty.GetValue(a, null), bProperty.GetValue(b, null))) return false;
            }
            return true;
        }
    }
#endif
}
