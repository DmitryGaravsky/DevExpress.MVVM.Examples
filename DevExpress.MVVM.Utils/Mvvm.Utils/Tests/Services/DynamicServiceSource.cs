#if DEBUGTEST
namespace Mvvm.Utils.Tests {
    using NUnit.Framework;
    using Mvvm.Utils.Services;

    #region TestClasses
    public interface ITestService {
        string Name { get; set; }
        void ReverseName();
    }
    public class TestService {
        public string Name { get; set; }
        public void ReverseName() {
            char[] charArray = Name.ToCharArray();
            System.Array.Reverse(charArray);
            Name = new string(charArray);
        }
    }
    // boxed
    public interface ITestService2 {
        ITestService Owner { get; set; }
        bool CheckOwner(ITestService owner);
    }
    public class TestService2 {
        public object Owner { get; set; }
        public bool CheckOwner(object owner) {
            return owner != null && owner is ITestService;
        }
    }
    // optional
    public interface ITestService3 {
        ITestService Owner { get; set; }
        ITestService GetParent(ITestService owner);
    }
    public class TestService3 { }
    //multi-interface
    public class TestService4 {
        public string Name { get; set; }
        public object Owner { get; set; }
        public bool CheckOwner(object owner) {
            return owner != null && owner is ITestService;
        }
    }
    //multiple parameters
    public interface ITestService5 {
        int Sum { get; }
    }
    public class TestService5 {
        protected TestService5(int a) {
            Sum = a;
        }
        protected TestService5(int a, int b) {
            Sum = a + b;
        }
        protected TestService5(int a, int b, int c) {
            Sum = a + b + c;
        }
        public int Sum { get; private set; }
    }
    #endregion

    [TestFixture]
    public class DynamicServiceSourceTests {
        [TestFixtureTearDown]
        public void FixtureTearDown() {
            DynamicServiceSource.Reset();
        }
        [Test]
        public void Test00_SimplePropertiesAndMethods() {
            TestService service = DynamicServiceSource.Create<TestService>(typeof(ITestService));
            Assert.IsNotNull(service);
            ITestService ts = service as ITestService;
            Assert.IsNotNull(ts);
            ts.Name = "Test";
            Assert.AreEqual("Test", service.Name);
            ts.ReverseName();
            Assert.AreEqual("tseT", service.Name);
            service.Name = "AAA";
            Assert.AreEqual("AAA", ts.Name);
        }
        [Test]
        public void Test02_BoxedPropertiesAndMethodParameters() {
            ITestService serviceOwner = DynamicServiceSource.Create<TestService>(typeof(ITestService)) as ITestService;
            TestService2 service = DynamicServiceSource.Create<TestService2>(typeof(ITestService2));
            Assert.IsNotNull(service);
            ITestService2 ts2 = service as ITestService2;
            Assert.IsNotNull(ts2);
            ts2.Owner = serviceOwner;
            Assert.AreEqual(serviceOwner, service.Owner);

            Assert.IsTrue(ts2.CheckOwner(serviceOwner));
            Assert.IsTrue(service.CheckOwner(serviceOwner));
            Assert.IsFalse(service.CheckOwner(new object()));
        }
        [Test]
        public void Test03_OptionalPropertiesAndMethods() {
            TestService3 service = DynamicServiceSource.Create<TestService3>(typeof(ITestService3));
            Assert.IsNotNull(service);
            ITestService3 ts3 = service as ITestService3;
            Assert.IsNotNull(ts3);
            Assert.IsNull(ts3.Owner);
            Assert.IsNull(ts3.GetParent(null));
        }
        [Test]
        public void Test04_MultipleInterfaces() {
            TestService4 service = DynamicServiceSource.Create<TestService4>(
                new System.Type[] { typeof(ITestService), typeof(ITestService2), typeof(ITestService3) });
            Assert.IsNotNull(service);
            Assert.IsNotNull(service as ITestService);
            Assert.IsNotNull(service as ITestService2);
            Assert.IsNotNull(service as ITestService3);
        }
        [Test]
        public void Test05_MultipleCtorParameters() {
            TestService5 service1 = DynamicServiceSource.Create<TestService5, int>(typeof(ITestService5), 1);
            TestService5 service2 = DynamicServiceSource.Create<TestService5, int, int>(typeof(ITestService5), 2, 2);
            TestService5 service3 = DynamicServiceSource.Create<TestService5, int, int, int>(typeof(ITestService5), 3, 3, 3);
            Assert.AreEqual(1, (service1 as ITestService5).Sum);
            Assert.AreEqual(4, (service2 as ITestService5).Sum);
            Assert.AreEqual(9, (service3 as ITestService5).Sum);
        }
    }
}
#endif