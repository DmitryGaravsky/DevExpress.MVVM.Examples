#if DEBUGTEST
namespace Mvvm.Utils.Commands.Tests {
    using System;
    using NUnit.Framework;

    #region TestClasses
    class Button : IDisposable {
        void IDisposable.Dispose() {
            Click = null;
            GC.SuppressFinalize(this);
        }
        public bool Enabled { get; set; }
        public void PerformClick() {
            if(Click != null) Click(this, EventArgs.Empty);
        }
        public event EventHandler Click;
    }
    namespace DataAnnotations {
        [AttributeUsage(AttributeTargets.Method)]
        class CommandAttribute : Attribute {
            public string Name { get; set; }
        }
    }
    namespace Native {
        using System.Collections.Generic;
        static class MetadataHelper {
            internal static IEnumerable<Attribute> result = null;
            public static IEnumerable<Attribute> GetExtenalAndFluentAPIAttrbutes(Type componentType, string memberName) {
                return result ?? new Attribute[0];
            }
        }
    }
    class TestViewModel {
        public TestViewModel() {
            C1Command = new CommandImplicitImpl();
            C2PrivateCommand = new CommandImplicitImplPrivate();
            C3Command = new CommandExplicitImpl();
            C4PrivateCommand = new CommandExplicitImpl();
            PropertyForC5 = new CommandImplicitImpl();
            PropertyForC6 = new CommandImplicitImpl();
        }
        public CommandImplicitImpl C1Command { get; set; }
        public CommandImplicitImplPrivate C2PrivateCommand { get; set; }
        public ICommandTest C3Command { get; set; }
        public CommandExplicitImpl C4PrivateCommand { get; set; }
        public void C1() { }
        public void C2Private() { }
        public void C3() { }
        public void C4Private() { }
        //
        [DataAnnotations.Command(Name = "PropertyForC5")]
        public void C5() { }
        public CommandImplicitImpl PropertyForC5 { get; set; }
        public void C6() { }
        public CommandImplicitImpl PropertyForC6 { get; set; }
    }
    class CommandImplicitImpl {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) {
            return true;
        }
        public void Execute(object parameter) { executed = true; }
        public void Raise() {
            if(CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
        internal bool executed;
    }
    class CommandImplicitImplPrivate {
        event EventHandler CanExecuteChanged;
        bool CanExecute(object parameter) {
            return true;
        }
        void Execute(object parameter) { executed = true; }
        public void Raise() {
            if(CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
        internal bool executed;
    }
    class CommandExplicitImpl : ICommandTest {
        EventHandler CanExecuteChangedCore;
        event EventHandler ICommandTest.CanExecuteChanged {
            add { CanExecuteChangedCore += value; }
            remove { CanExecuteChangedCore -= value; }
        }
        bool ICommandTest.CanExecute(object parameter) {
            return true;
        }
        void ICommandTest.Execute(object parameter) { executed = true; }
        void ICommandTest.Raise() {
            RaiseCore();
        }
        internal void RaiseCore() {
            if(CanExecuteChangedCore != null)
                CanExecuteChangedCore(this, EventArgs.Empty);
        }
        internal bool executed;
        bool ICommandTest.executed { get { return executed; } }
    }
    interface ICommandTest {
        bool executed { get; }
        event EventHandler CanExecuteChanged;
        bool CanExecute(object parameter);
        void Execute(object parameter);
        void Raise();
    }
    #endregion TestClasses
    [TestFixture]
    public class CommandHelperTests {
        [TestFixtureSetUp]
        public void FixtureSetUp() {
            MVVMTypesResolver.SetUpMVVMAssembly("Mvvm.Utils.Commands.Tests.");
        }
        [TestFixtureTearDown]
        public void FixtureTearDown() {
            MVVMTypesResolver.Reset();
        }
        [Test]
        public void Bind_C1_Command() {
            TestViewModel viewModel = new TestViewModel();
            using(var btn = new Button()) {
                btn.Enabled = false;
                using(CommandHelper.Bind(btn,
                        (button, execute) => button.Click += (s, e) => execute(),
                        (button, canExecute) => button.Enabled = canExecute(),
                        viewModel.C1Command)) {
                    Assert.IsTrue(btn.Enabled);

                    Assert.IsFalse(viewModel.C1Command.executed);
                    btn.PerformClick();
                    Assert.IsTrue(viewModel.C1Command.executed);

                    btn.Enabled = false;
                    viewModel.C1Command.Raise();
                    Assert.IsTrue(btn.Enabled);
                }
            }
        }
        [Test]
        public void Bind_C1_Command_UnbindCanExecuteChanged() {
            TestViewModel viewModel = new TestViewModel();
            using(var btn = new Button()) {
                btn.Enabled = false;
                using(CommandHelper.Bind(btn,
                        (button, execute) => button.Click += (s, e) => execute(),
                        (button, canExecute) => button.Enabled = canExecute(),
                        viewModel.C1Command)) {
                    Assert.IsFalse(viewModel.C1Command.executed);
                    btn.PerformClick();
                    Assert.IsTrue(viewModel.C1Command.executed);
                }
                btn.Enabled = false;
                btn.PerformClick();
                Assert.IsFalse(btn.Enabled);
            }
        }
        [Test]
        public void Bind_C1_CommandSelector() {
            TestViewModel viewModel = new TestViewModel();
            using(var btn = new Button()) {
                btn.Enabled = false;
                using(CommandHelper.Bind(btn,
                        (button, execute) => button.Click += (s, e) => execute(),
                        (button, canExecute) => button.Enabled = canExecute(),
                        () => viewModel.C1(), viewModel)) {
                    Assert.IsTrue(btn.Enabled);

                    Assert.IsFalse(viewModel.C1Command.executed);
                    btn.PerformClick();
                    Assert.IsTrue(viewModel.C1Command.executed);

                    btn.Enabled = false;
                    viewModel.C1Command.Raise();
                    Assert.IsTrue(btn.Enabled);
                }
            }
        }
        [Test]
        public void Bind_C2Private_Command() {
            TestViewModel viewModel = new TestViewModel();
            using(var btn = new Button()) {
                btn.Enabled = false;
                using(CommandHelper.Bind(btn,
                        (button, execute) => button.Click += (s, e) => execute(),
                        (button, canExecute) => button.Enabled = canExecute(),
                        viewModel.C2PrivateCommand)) {
                    Assert.IsTrue(btn.Enabled);

                    Assert.IsFalse(viewModel.C2PrivateCommand.executed);
                    btn.PerformClick();
                    Assert.IsTrue(viewModel.C2PrivateCommand.executed);

                    btn.Enabled = false;
                    viewModel.C2PrivateCommand.Raise();
                    Assert.IsTrue(btn.Enabled);
                }
            }
        }
        [Test]
        public void Bind_C2Private_CommandSelector() {
            TestViewModel viewModel = new TestViewModel();
            using(var btn = new Button()) {
                btn.Enabled = false;
                using(CommandHelper.Bind(btn,
                        (button, execute) => button.Click += (s, e) => execute(),
                        (button, canExecute) => button.Enabled = canExecute(),
                        () => viewModel.C2Private(), viewModel)) {
                    Assert.IsTrue(btn.Enabled);

                    Assert.IsFalse(viewModel.C2PrivateCommand.executed);
                    btn.PerformClick();
                    Assert.IsTrue(viewModel.C2PrivateCommand.executed);

                    btn.Enabled = false;
                    viewModel.C2PrivateCommand.Raise();
                    Assert.IsTrue(btn.Enabled);
                }
            }
        }
        [Test]
        public void Bind_C3_Command() {
            TestViewModel viewModel = new TestViewModel();
            using(var btn = new Button()) {
                btn.Enabled = false;
                using(CommandHelper.Bind(btn,
                        (button, execute) => button.Click += (s, e) => execute(),
                        (button, canExecute) => button.Enabled = canExecute(),
                        viewModel.C3Command)) {
                    Assert.IsTrue(btn.Enabled);

                    Assert.IsFalse(viewModel.C3Command.executed);
                    btn.PerformClick();
                    Assert.IsTrue(viewModel.C3Command.executed);

                    btn.Enabled = false;
                    viewModel.C3Command.Raise();
                    Assert.IsTrue(btn.Enabled);
                }
            }
        }
        [Test]
        public void Bind_C3_CommandSelector() {
            TestViewModel viewModel = new TestViewModel();
            using(var btn = new Button()) {
                btn.Enabled = false;
                using(CommandHelper.Bind(btn,
                        (button, execute) => button.Click += (s, e) => execute(),
                        (button, canExecute) => button.Enabled = canExecute(),
                        () => viewModel.C3(), viewModel)) {
                    Assert.IsTrue(btn.Enabled);

                    Assert.IsFalse(viewModel.C3Command.executed);
                    btn.PerformClick();
                    Assert.IsTrue(viewModel.C3Command.executed);

                    btn.Enabled = false;
                    viewModel.C3Command.Raise();
                    Assert.IsTrue(btn.Enabled);
                }
            }
        }
        [Test]
        public void Bind_C4Private_Command() {
            TestViewModel viewModel = new TestViewModel();
            using(var btn = new Button()) {
                btn.Enabled = false;
                using(CommandHelper.Bind(btn,
                        (button, execute) => button.Click += (s, e) => execute(),
                        (button, canExecute) => button.Enabled = canExecute(),
                        viewModel.C4PrivateCommand)) {
                    Assert.IsTrue(btn.Enabled);

                    Assert.IsFalse(viewModel.C4PrivateCommand.executed);
                    btn.PerformClick();
                    Assert.IsTrue(viewModel.C4PrivateCommand.executed);

                    btn.Enabled = false;
                    viewModel.C4PrivateCommand.RaiseCore();
                    Assert.IsTrue(btn.Enabled);
                }
            }
        }
        [Test]
        public void Bind_C4Private_CommandSelector() {
            TestViewModel viewModel = new TestViewModel();
            using(var btn = new Button()) {
                btn.Enabled = false;
                using(CommandHelper.Bind(btn,
                        (button, execute) => button.Click += (s, e) => execute(),
                        (button, canExecute) => button.Enabled = canExecute(),
                        () => viewModel.C4Private(), viewModel)) {
                    Assert.IsTrue(btn.Enabled);

                    Assert.IsFalse(viewModel.C4PrivateCommand.executed);
                    btn.PerformClick();
                    Assert.IsTrue(viewModel.C4PrivateCommand.executed);

                    btn.Enabled = false;
                    viewModel.C4PrivateCommand.RaiseCore();
                    Assert.IsTrue(btn.Enabled);
                }
            }
        }
        [Test]
        public void Bind_C5_CommandSelector_Attribute() {
            TestViewModel viewModel = new TestViewModel();
            using(var btn = new Button()) {
                btn.Enabled = false;
                using(CommandHelper.Bind(btn,
                        (button, execute) => button.Click += (s, e) => execute(),
                        (button, canExecute) => button.Enabled = canExecute(),
                        () => viewModel.C5(), viewModel)) {
                    Assert.IsTrue(btn.Enabled);

                    Assert.IsFalse(viewModel.PropertyForC5.executed);
                    btn.PerformClick();
                    Assert.IsTrue(viewModel.PropertyForC5.executed);

                    btn.Enabled = false;
                    viewModel.PropertyForC5.Raise();
                    Assert.IsTrue(btn.Enabled);
                }
            }
        }
        [Test]
        public void Bind_C6_CommandSelector_Fluent() {
            Native.MetadataHelper.result = new Attribute[] {
                new DataAnnotations.CommandAttribute() { Name = "PropertyForC6" }
            };
            TestViewModel viewModel = new TestViewModel();
            using(var btn = new Button()) {
                btn.Enabled = false;
                using(CommandHelper.Bind(btn,
                        (button, execute) => button.Click += (s, e) => execute(),
                        (button, canExecute) => button.Enabled = canExecute(),
                        () => viewModel.C6(), viewModel)) {
                    Assert.IsTrue(btn.Enabled);

                    Assert.IsFalse(viewModel.PropertyForC6.executed);
                    btn.PerformClick();
                    Assert.IsTrue(viewModel.PropertyForC6.executed);

                    btn.Enabled = false;
                    viewModel.PropertyForC6.Raise();
                    Assert.IsTrue(btn.Enabled);
                }
            }
            Native.MetadataHelper.result = null;
        }
    }
}
#endif