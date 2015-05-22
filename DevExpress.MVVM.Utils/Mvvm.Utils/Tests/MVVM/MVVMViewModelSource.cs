#if DEBUGTEST
namespace Mvvm.Utils.Tests {
    using System;
    using System.Linq.Expressions;
    using NUnit.Framework;

    #region Test Classes
    public class Foo { }
    public class Bar {
        public Bar() : this(null) { }
        public Bar(Foo foo) { this.Foo = foo; }
        public Foo Foo { get; private set; }
    }
    public class FooBar {
        protected FooBar(Bar bar = null) {
            this.Bar = bar;
        }
        public Bar Bar { get; private set; }
    }
    public class Zoo {
        protected Zoo() { }
    }
    public class TestViewModelSource {
        public static T Create<T>() where T : class, new() {
            return new T();
        }
        public static T Create<T>(Expression<Func<T>> constructorExpression) where T : class {
            return constructorExpression.Compile()();
        }
    }
    #endregion
    [TestFixture]
    public class ViewModelSourceProxyTests : IMVVMViewModelSource {
        IMVVMViewModelSource ViewModelSource;
        [TestFixtureSetUp]
        public void FixtureSetUp() {
            ViewModelSource = this;
        }
        [TestFixtureTearDown]
        public void FixtureTearDown() {
            ViewModelSource = null;
            MVVMViewModelSourceProxy.Reset();
        }
        [Test]
        public void Test00() {
            var foo = ViewModelSource.Create(typeof(Foo));
            Assert.IsNotNull(foo);
            Assert.IsTrue(foo is Foo);

            var bar1 = ViewModelSource.Create(typeof(Bar));
            Assert.IsNotNull(bar1);
            Assert.IsTrue(bar1 is Bar);
            Assert.IsNull(((Bar)bar1).Foo);

            var bar2 = ViewModelSource.Create(typeof(Bar), foo);
            Assert.IsNotNull(bar2);
            Assert.IsTrue(bar2 is Bar);
            Assert.AreEqual(foo, ((Bar)bar2).Foo);
        }
        [Test]
        public void Test01() {
            var foo = ViewModelSource.Create(typeof(Foo), 5);
            Assert.IsNotNull(foo);
            Assert.IsTrue(foo is Foo);

            var bar1 = ViewModelSource.Create(typeof(Bar), 5);
            Assert.IsNotNull(bar1);
            Assert.IsTrue(bar1 is Bar);
            Assert.IsNull(((Bar)bar1).Foo);

            var bar2 = ViewModelSource.Create(typeof(Bar), foo, 5);
            Assert.IsNotNull(bar2);
            Assert.IsTrue(bar2 is Bar);
            Assert.AreEqual(foo, ((Bar)bar2).Foo);
        }
        [Test]
        public void Test02_ProtectedCtor() {
            Bar b = new Bar();
            var fooBar = ViewModelSource.Create(typeof(FooBar), b);
            Assert.IsNotNull(fooBar);
            Assert.IsTrue(fooBar is FooBar);
            Assert.AreEqual(b, ((FooBar)fooBar).Bar);
        }
        [Test]
        public void Test02_CtorDefaultParameters() {
            var fooBar = ViewModelSource.Create(typeof(FooBar));
            Assert.IsNotNull(fooBar);
            Assert.IsTrue(fooBar is FooBar);
            Assert.IsNull(((FooBar)fooBar).Bar);
        }
        [Test]
        public void Test03_ProtectedCtorNewConstraint() {
            var zoo = ViewModelSource.Create(typeof(Zoo));
            Assert.IsNotNull(zoo);
            Assert.IsTrue(zoo is Zoo);
        }
        object IMVVMViewModelSource.Create(Type viewModelType, params object[] parameters) {
            return MVVMViewModelSourceProxy.Create(typeof(TestViewModelSource), viewModelType, parameters);
        }
    }
}
#endif