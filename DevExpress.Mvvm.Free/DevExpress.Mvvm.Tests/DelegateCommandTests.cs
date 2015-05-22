#pragma warning disable 612,618
#if SILVERLIGHT
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#elif NETFX_CORE
#else
using NUnit.Framework;
#endif
using System;
using System.Windows;
using DevExpress.Mvvm.Native;
using System.Windows.Input;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
#if !MONO
#if !NETFX_CORE
using System.Windows.Controls;
#else
using DevExpress.TestFramework.NUnit;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DevExpress.TestRunner.Utils;
#endif
#endif

namespace DevExpress.Mvvm.Tests {
    public abstract class CommandTests : BaseWpfFixture {
        protected abstract CommandBase<object> CreateCommand(Action execute);
        protected abstract CommandBase<object> CreateCommand(Action execute, bool useCommandManager);
        protected abstract CommandBase<object> CreateCommand(Action execute, Func<bool> canExecute, bool? useCommandManager = null);
        protected abstract CommandBase<T> CreateCommand<T>(Action<T> execute);
        protected abstract CommandBase<T> CreateCommand<T>(Action<T> execute, bool useCommandManager);
        protected abstract CommandBase<T> CreateCommand<T>(Action<T> execute, Func<T, bool> canExecute, bool? useCommandManager = null);
        protected abstract void Execute(ICommand command, object parameter);

        #region Convert Parameter
        [Test]
        public void BoolParameter() {
            bool? param = null;
            ICommand command = CreateCommand<bool>(x => param = x, x => x);
            Assert.IsTrue(command.CanExecute("True"));
            Assert.IsTrue(command.CanExecute("true"));
            Assert.IsFalse(command.CanExecute("False"));
            Assert.IsFalse(command.CanExecute("false"));
            Assert.IsTrue(command.CanExecute(true));
            Assert.IsFalse(command.CanExecute(false));
            param = null;
            Execute(command, "True");
            Assert.IsTrue(param.Value);
            param = null;
            Execute(command, "true");
            Assert.IsTrue(param.Value);
            param = null;
            Execute(command, true);
            Assert.IsTrue(param.Value);
            param = null;
            Execute(command, "False");
            Assert.IsNull(param);
            param = null;
            Execute(command, "false");
            Assert.IsNull(param);

            command = CreateCommand<bool>(x => param = x);
            param = null;
            Execute(command, new Nullable<bool>(true));
            Assert.IsTrue(param.Value);
            param = null;
            Execute(command, null);
            Assert.AreEqual(default(bool), param);
        }
        [Test, ExpectedException(typeof(FormatException))]
        public void BoolParameterException() {
            ICommand command = CreateCommand<bool>(x => { });
            command.CanExecute("True1");
        }
        [Test]
        public void NullableBoolParameter() {
            bool? param = null;
            ICommand command = CreateCommand<bool?>(x => param = x);
            Assert.IsTrue(command.CanExecute("True"));
            Assert.IsTrue(command.CanExecute("true"));
            Assert.IsTrue(command.CanExecute(true));
            Assert.IsTrue(command.CanExecute("False"));
            Assert.IsTrue(command.CanExecute("false"));
            Assert.IsTrue(command.CanExecute(false));
            Assert.IsTrue(command.CanExecute(null));
            param = null;
            Execute(command, "True");
            Assert.IsTrue(param.Value);
            param = null;
            Execute(command, "true");
            Assert.IsTrue(param.Value);
            param = null;
            Execute(command, true);
            Assert.IsTrue(param.Value);
            param = null;
            Execute(command, "False");
            Assert.IsFalse(param.Value);
            param = null;
            Execute(command, "false");
            Assert.IsFalse(param.Value);
            param = null;
            Execute(command, false);
            Assert.IsFalse(param.Value);
            param = null;
            Execute(command, new Nullable<bool>(false));
            Assert.IsFalse(param.Value);
            param = null;
            Execute(command, null);
            Assert.IsNull(param);
        }
        [Test]
        public void IntParameter() {
            int param = 0;
            ICommand command = CreateCommand<int>(x => param = x, x => x > 0);
            Assert.IsFalse(command.CanExecute("-1"));
            Assert.IsTrue(command.CanExecute("1"));
            Execute(command, "1");
            Assert.AreEqual(1, param);
            Execute(command, new Nullable<int>(2));
            Assert.AreEqual(2, param);
        }
        [Test, ExpectedException(typeof(FormatException))]
        public void IntParameterException() {
            ICommand command = CreateCommand<int>(x => { });
            command.CanExecute("1.0");
        }
        [Test]
        public void NullableIntParameter() {
            int? param = 0;
            ICommand command = CreateCommand<int?>(x => param = x);
            Assert.IsTrue(command.CanExecute("-1"));
            Assert.IsTrue(command.CanExecute("1"));
            Assert.IsTrue(command.CanExecute(null));
            Execute(command, "1");
            Assert.AreEqual(1, param);
            Execute(command, new Nullable<int>(2));
            Assert.AreEqual(2, param);
            Execute(command, null);
            Assert.IsNull(param);
        }
        [Test]
        public void DoubleParameter() {
            double param = 0;
            ICommand command = CreateCommand<double>(x => param = x);
            Execute(command, "1.11");
            Assert.AreEqual(1.11, param);
            Execute(command, "1,11");
            Assert.AreEqual(111, param);
            Execute(command, new Nullable<double>(1.11));
            Assert.AreEqual(1.11, param);

            var culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-ru");
            try {
                Assert.AreEqual(1.11, Convert.ChangeType("1,11", typeof(double), null));
                Execute(command, "1.11");
                Assert.AreEqual(1.11, param);
                Execute(command, "1,11");
                Assert.AreEqual(111, param);
            } finally {
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }
        [Test, ExpectedException(typeof(FormatException))]
        public void DoubleParameterException() {
            ICommand command = CreateCommand<double>(x => { });
            Execute(command, "one");
        }

        [Test]
        public void NullableDoubleParameter() {
            double? param = 0;
            ICommand command = CreateCommand<double?>(x => param = x);
            Execute(command, "1.11");
            Assert.AreEqual(1.11, param);
            Execute(command, "1,11");
            Assert.AreEqual(111, param);
            Execute(command, new Nullable<double>(1.11));
            Assert.AreEqual(1.11, param);
            Execute(command, null);
            Assert.IsNull(param);
        }
        [Test]
        public void EnumParameter() {
            Visibility param = Visibility.Collapsed;
            ICommand command = CreateCommand<Visibility>(x => param = x);
            Execute(command, "Visible");
            Assert.AreEqual(Visibility.Visible, param);
            Execute(command, Visibility.Collapsed);
            Assert.AreEqual(Visibility.Collapsed, param);
            Execute(command, null);
            Assert.AreEqual(default(Visibility), param);
            Execute(command, new Nullable<Visibility>(Visibility.Collapsed));
            Assert.AreEqual(Visibility.Collapsed, param);
        }
        [Test, ExpectedException(typeof(ArgumentException))]
        public void EnumParameterException() {
            ICommand command = CreateCommand<Visibility>(x => { });
            Execute(command, "visible");
        }
        [Test]
        public void NullableEnumParameter() {
            Visibility? param = Visibility.Collapsed;
            ICommand command = CreateCommand<Visibility?>(x => param = x);
            Execute(command, "Visible");
            Assert.AreEqual(Visibility.Visible, param);
            Execute(command, Visibility.Collapsed);
            Assert.AreEqual(Visibility.Collapsed, param);
            Execute(command, null);
            Assert.IsNull(param);
            Execute(command, new Nullable<Visibility>(Visibility.Collapsed));
            Assert.AreEqual(Visibility.Collapsed, param);
        }

        public class ParamClass1 { }
        public class ParamClass1_1 : ParamClass1 { }
        public class ParamClass2 { }
        [Test]
        public void CustomParameter() {
            ParamClass1 param = null;
            ICommand command = CreateCommand<ParamClass1>(x => param = x);
            Assert.IsTrue(command.CanExecute(new ParamClass1()));
            ParamClass1 pp = new ParamClass1();
            Execute(command, pp);
            Assert.AreSame(pp, param);
            Execute(command, null);
            Assert.IsNull(param);
            pp = new ParamClass1_1();
            Execute(command, pp);
            Assert.AreSame(pp, param);
        }
        [Test, ExpectedException(typeof(InvalidCastException))]
        public void CustomParameterException1() {
            ICommand command = CreateCommand<ParamClass1>(x => { });
            command.CanExecute("a");
        }
        [Test]
        public void CustomParameterException2_T120478() {
            ICommand command = CreateCommand<ParamClass1>(x => { });
            command.CanExecute(new ParamClass2());
        }
        [Test, ExpectedException(typeof(InvalidCastException))]
        public void CustomParameterException3_T120478() {
            ICommand command = CreateCommand<ParamClass1>(x => { });
            command.Execute(new ParamClass2());
        }
        [Test]
        public void DateTimeToStringParameter() {
            string parameter = null;
            ICommand command = CreateCommand<string>(o => parameter = o);
            DateTime today = DateTime.Today;
            Execute(command, today);
            Assert.AreEqual(Convert.ToString(today, CultureInfo.InvariantCulture), parameter);
        }
        #endregion
        #region Execute
        [Test]
        public void Execute1() {
            bool executed = false;
            var command = CreateCommand(() => { executed = true; });
            Execute(command, null);
            Assert.IsTrue(executed);
        }
        [Test]
        public void Execute2() {
            string param = null;
            var command = CreateCommand<string>(x => param = x);
            Execute(command, "param");
            Assert.AreEqual("param", param);
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteNull1() {
            CreateCommand(null);
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteNull2() {
            CreateCommand(null, null);
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteNull3() {
            CreateCommand<string>(null);
        }
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteNull4() {
            CreateCommand<string>(null, null);
        }
        [Test]
        public void ExecuteWhenCanExecuteIsFalse() {
            var counter = 0;
            bool canExecute = true;
            var command = CreateCommand(() => counter++, () => canExecute);
            Execute(command, null);
            Assert.AreEqual(1, counter);
            canExecute = false;
            Execute(command, null);
            Assert.AreEqual(1, counter);
            canExecute = true;
            Execute(command, null);
            Assert.AreEqual(2, counter);
        }
        #endregion
        #region CanExecute
        [Test]
        public void CanExecute() {
            bool canExecute = true;
            var command1 = CreateCommand(() => { }, () => canExecute);
            Assert.IsTrue(command1.CanExecute(null));
            Assert.IsTrue(command1.CanExecute(1));
            canExecute = false;
            Assert.IsFalse(command1.CanExecute(null));
            Assert.IsFalse(command1.CanExecute(1));

            var command2 = CreateCommand<string>(x => { }, x => x == "test");
            Assert.IsTrue(command2.CanExecute("test"));
            Assert.IsFalse(command2.CanExecute("test2"));

            var command3 = CreateCommand<string>(x => { });
            Assert.IsTrue(command3.CanExecute(string.Empty));
            Assert.IsTrue(((ICommand)command3).CanExecute(1));
        }
#if !MONO
        [Test]
        public void CanExecuteIsEnabledWithoutCommandManager() {
            Button button = new Button();
            bool canExecute = false;
            CommandBase<object> command = CreateCommand<object>(x => { }, x => canExecute, false);
            Assert.IsTrue(button.IsEnabled);
            button.Command = command;
            Assert.IsFalse(button.IsEnabled);
            canExecute = true;
            Assert.IsFalse(button.IsEnabled);
            command.RaiseCanExecuteChanged();
            Assert.IsTrue(button.IsEnabled);
        }
#endif
#if !SILVERLIGHT && !NETFX_CORE &&!NETFX_CORE &&!MONO
        [Test]
        public void CanExecuteIsEnabledWithCommandManager() {
            Button button = new Button();
            bool canExecute = false;
            CommandBase<object> command = CreateCommand<object>(x => { }, x => canExecute);
            Assert.IsTrue(button.IsEnabled);
            button.Command = command;
            Assert.IsFalse(button.IsEnabled);
            canExecute = true;
            command.RaiseCanExecuteChanged();
            DispatcherHelper.DoEvents();
            Assert.IsTrue(button.IsEnabled);
        }
#endif
        #endregion
        #region RaiseCanExecuteChanged
        void RaiseCanExecuteChangedTest<T>(CommandBase<T> command) {
            var counter = new CanExecuteChangedCounter(command);
            command.RaiseCanExecuteChanged();
            Assert.AreEqual(1, counter.FireCount);
            counter.Unsubscribe();
            command.RaiseCanExecuteChanged();
            Assert.AreEqual(1, counter.FireCount);
        }
#if !SILVERLIGHT && !NETFX_CORE && !MONO
        void RaiseCanExecuteChangedWithCommandManagerTest<T>(CommandBase<T> command) {
            var counter = new CanExecuteChangedCounter(command);
            command.RaiseCanExecuteChanged();
            Assert.AreEqual(0, counter.FireCount);
            DispatcherHelper.DoEvents();
            Assert.AreEqual(1, counter.FireCount);
            counter.Unsubscribe();
            command.RaiseCanExecuteChanged();
            DispatcherHelper.DoEvents();
            Assert.AreEqual(1, counter.FireCount);
        }
#endif
        [Test]
        public void RaiseCanExecuteChanged() {
            CommandBase<object> command1 = CreateCommand(() => { }, () => true, false);
            CommandBase<string> command2 = CreateCommand<string>(x => { }, x => true, false);
            CommandBase<object> command3 = CreateCommand(() => { }, false);
            CommandBase<string> command4 = CreateCommand<string>(x => { }, false);
            RaiseCanExecuteChangedTest(command1);
            RaiseCanExecuteChangedTest(command2);
            RaiseCanExecuteChangedTest(command3);
            RaiseCanExecuteChangedTest(command4);
        }
#if !SILVERLIGHT && !NETFX_CORE && !MONO
        [Test]
        public void RaiseCanExecuteChangedWithCommandManager() {
            CommandBase<object> command1 = CreateCommand(() => { }, () => true);
            CommandBase<string> command2 = CreateCommand<string>(x => { }, x => true);
            CommandBase<object> command3 = CreateCommand(() => { });
            CommandBase<string> command4 = CreateCommand<string>(x => { });
            RaiseCanExecuteChangedWithCommandManagerTest(command1);
            RaiseCanExecuteChangedWithCommandManagerTest(command2);
            RaiseCanExecuteChangedWithCommandManagerTest(command3);
            RaiseCanExecuteChangedWithCommandManagerTest(command4);
        }
#endif
        #endregion
        #region References and Memory
        public class ViewModel {
            public static int ExecuteCount;
            public ICommand Command { get; set; }
            public static ViewModel Create1(Func<Action, CommandBase<object>> commandCreatingMethod, out WeakReference targetWeakReference) {
                ViewModel res = new ViewModel();
                int x = 0;
                Action target = () => { ExecuteCount = (++x); };
                targetWeakReference = new WeakReference(target);
                res.Command = commandCreatingMethod(target);
                return res;
            }
            public static ViewModel Create2(Func<Action<object>, CommandBase<object>> commandCreatingMethod, out WeakReference targetWeakReference) {
                ViewModel res = new ViewModel();
                int x = 0;
                Action<object> target = xx => ExecuteCount = (++x);
                targetWeakReference = new WeakReference(target);
                res.Command = commandCreatingMethod(target);
                return res;
            }
            ViewModel() { }
        }
        [Test]
        public void KeepTargetReference() {
            WeakReference t1;
            var vm = ViewModel.Create1(x => CreateCommand(x), out t1);
            MemoryLeaksHelper.CollectOptional(t1);
            Execute(vm.Command, null);
            Assert.AreEqual(1, ViewModel.ExecuteCount);

            WeakReference t2;
            vm = ViewModel.Create2(x => CreateCommand(x), out t2);
            MemoryLeaksHelper.CollectOptional(t2);
            Execute(vm.Command, null);
            Assert.AreEqual(1, ViewModel.ExecuteCount);
        }

        public class MemoryViewModel {
            private ICommand PrivateCommand { get; set; }
            private string PrivateContent { get; set; }
            internal ICommand InternalCommand { get; set; }
            internal string InternalContent { get; set; }

            public void CreatePrivateCommand(Func<Action<string>, Func<string, bool>, CommandBase<string>> commandCreatingMethod) {
                PrivateCommand = commandCreatingMethod(SetPrivateContent, CheckPrivateContent);
            }
            public void CreateInternalCommand(Func<Action<string>, Func<string, bool>, CommandBase<string>> commandCreatingMethod) {
                InternalCommand = commandCreatingMethod(SetInternalContent, CheckInternalContent);
            }

            private void SetPrivateContent(string newContent) {
                PrivateContent = newContent;
            }
            private bool CheckPrivateContent(string p) {
                return string.IsNullOrEmpty(PrivateContent);
            }
            internal void SetInternalContent(string newContent) {
                InternalContent = newContent;
            }
            internal bool CheckInternalContent(string p) {
                return string.IsNullOrEmpty(InternalContent);
            }
        }
        [Test]
        public void ReleaseTarget_CanExecutePrivate() {
            MemoryViewModel commandContainer = new MemoryViewModel();
            WeakReference reference = new WeakReference(commandContainer);
            commandContainer.CreatePrivateCommand((x1, x2) => CreateCommand(x1, x2));
            commandContainer.CreateInternalCommand((x1, x2) => CreateCommand(x1, x2));
            Assert.IsTrue(reference.IsAlive);
            commandContainer = null;
            MemoryLeaksHelper.EnsureCollected(reference);
        }
#if !MONO
        [Test]
        public void CommandConsumerMemoryLeakTest() {
            Func<ICommand, WeakReference> createButton = x => {
                return new WeakReference(new Button() { Command = x });
            };
            CommandBase<object> command = CreateCommand<object>(null, x => true);
            WeakReference wrButton = createButton(command);
            MemoryLeaksHelper.EnsureCollected(wrButton);
        }
#endif
#if !SILVERLIGHT && !NETFX_CORE &&!MONO
        [Test]
        public void CommandMultithreading() {
            Window mainWindow = new Window();
            mainWindow.Show();
            Thread thread = new Thread(() => {
                Button button = new Button();
                CommandBase<object> command = CreateCommand(() => { }, () => button.IsEnabled);
                button.Command = command;
                Window window = new Window() {
                    Content = button,
                };
                window.ContentRendered += (s, e) => {
                    command.RaiseCanExecuteChanged();
                    window.Close();
                };
                window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();
                window.Show();
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();
            mainWindow.Close();
        }
#endif
        #endregion
    }
    [TestFixture]
    public class DelegateCommandTests : CommandTests {
        protected override CommandBase<object> CreateCommand(Action execute) {
            return new DelegateCommand(execute);
        }
        protected override CommandBase<object> CreateCommand(Action execute, bool useCommandManager) {
#if !SILVERLIGHT && !NETFX_CORE &&!MONO
            return new DelegateCommand(execute, useCommandManager);
#else
            return new DelegateCommand(execute);
#endif
        }
        protected override CommandBase<object> CreateCommand(Action execute, Func<bool> canExecute, bool? useCommandManager = null) {
#if !SILVERLIGHT && !NETFX_CORE &&!MONO
            return new DelegateCommand(execute, canExecute, useCommandManager);
#else
            return new DelegateCommand(execute, canExecute);
#endif
        }
        protected override CommandBase<T> CreateCommand<T>(Action<T> execute) {
            return new DelegateCommand<T>(execute);
        }
        protected override CommandBase<T> CreateCommand<T>(Action<T> execute, bool useCommandManager) {
#if !SILVERLIGHT && !NETFX_CORE &&!MONO
            return new DelegateCommand<T>(execute, useCommandManager);
#else
            return new DelegateCommand<T>(execute);
#endif
        }
        protected override CommandBase<T> CreateCommand<T>(Action<T> execute, Func<T, bool> canExecute, bool? useCommandManager = null) {
#if !SILVERLIGHT && !NETFX_CORE &&!MONO
            return new DelegateCommand<T>(execute, canExecute, useCommandManager);
#else
            return new DelegateCommand<T>(execute, canExecute);
#endif
        }
        protected override void Execute(ICommand command, object parameter) {
            command.Execute(parameter);
        }
    }
    [TestFixture]
    public class AsyncCommandTests : CommandTests {
        protected override CommandBase<object> CreateCommand(Action execute) {
            if(execute == null)
                return new AsyncCommand(null) { AllowMultipleExecution = true };
            return new AsyncCommand(() => Task.Factory.StartNew(() => execute())) { AllowMultipleExecution = true };
        }
        protected override CommandBase<object> CreateCommand(Action execute, bool useCommandManager) {
#if !SILVERLIGHT && !NETFX_CORE &&!MONO
            if(execute == null)
                return new AsyncCommand(null, useCommandManager) { AllowMultipleExecution = true };
            return new AsyncCommand(() => Task.Factory.StartNew(() => execute()), useCommandManager) { AllowMultipleExecution = true };
#else
            if(execute == null)
                return new AsyncCommand(null) { AllowMultipleExecution = true };
            return new AsyncCommand(() => Task.Factory.StartNew(() => execute())) { AllowMultipleExecution = true };
#endif
        }
        protected override CommandBase<object> CreateCommand(Action execute, Func<bool> canExecute, bool? useCommandManager = null) {
#if !SILVERLIGHT && !NETFX_CORE &&!MONO
            if(execute == null)
                return new AsyncCommand(null, canExecute, useCommandManager) { AllowMultipleExecution = true };
            return new AsyncCommand(() => Task.Factory.StartNew(() => execute()), canExecute, useCommandManager) { AllowMultipleExecution = true };
#else
            if(execute == null)
                return new AsyncCommand(null, canExecute) { AllowMultipleExecution = true };
            return new AsyncCommand(() => Task.Factory.StartNew(() => execute()), canExecute) { AllowMultipleExecution = true };
#endif
        }
        protected override CommandBase<T> CreateCommand<T>(Action<T> execute) {
            if(execute == null)
                return new AsyncCommand<T>(null) { AllowMultipleExecution = true };
            return new AsyncCommand<T>(x => Task.Factory.StartNew(() => execute(x))) { AllowMultipleExecution = true };
        }
        protected override CommandBase<T> CreateCommand<T>(Action<T> execute, bool useCommandManager) {
#if !SILVERLIGHT && !NETFX_CORE &&!MONO
            if(execute == null)
                return new AsyncCommand<T>(null, useCommandManager) { AllowMultipleExecution = true };
            return new AsyncCommand<T>(x => Task.Factory.StartNew(() => execute(x)), useCommandManager) { AllowMultipleExecution = true };
#else
            if(execute == null)
                return new AsyncCommand<T>(null) { AllowMultipleExecution = true };
            return new AsyncCommand<T>(x => Task.Factory.StartNew(() => execute(x))) { AllowMultipleExecution = true };
#endif
        }
        protected override CommandBase<T> CreateCommand<T>(Action<T> execute, Func<T, bool> canExecute, bool? useCommandManager = null) {
#if !SILVERLIGHT && !NETFX_CORE &&!MONO
            if(execute == null)
                return new AsyncCommand<T>(null, canExecute, useCommandManager) { AllowMultipleExecution = true };
            return new AsyncCommand<T>(x => Task.Factory.StartNew(() => execute(x)), canExecute, useCommandManager) { AllowMultipleExecution = true };
#else
            if(execute == null)
                return new AsyncCommand<T>(null, canExecute) { AllowMultipleExecution = true };
            return new AsyncCommand<T>(x => Task.Factory.StartNew(() => execute(x)), canExecute) { AllowMultipleExecution = true };
#endif
        }
        protected override void Execute(ICommand command, object parameter) {
            command.Execute(parameter);
            Wait(command);
        }

        const int latencyTime = 1;
        void Wait(ICommand command) {
            Type t = command.GetType();
#if !NETFX_CORE
            if(t.IsGenericType) {
#else
            if(t.IsGenericType()) {
#endif
                Type genericType = t.GetGenericArguments()[0];
#if !NETFX_CORE
                GetType().GetMethod("Wait2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).
#else
                GetType().GetMethod("Wait2").
#endif
                    MakeGenericMethod(genericType).Invoke(this, new object[] { command });
            } else {
                Wait1(command);
            }
        }
        void Wait1(ICommand command) {
            ((AsyncCommand)command).executeTask.Wait(TimeSpan.FromSeconds(latencyTime));
        }
        void Wait2<T>(ICommand command) {
            ((AsyncCommand<T>)command).executeTask.Wait(TimeSpan.FromSeconds(latencyTime));
        }
#if !NETFX_CORE && !MONO
        bool executingAsyncMethod = false;
        AsyncCommand<int> asyncTestCommand;

        Task AsyncExecuteMethod(int timeout) {
            return Task.Factory.StartNew(() => {
                for(int i = 0; i < 10; i++) {
                    if(asyncTestCommand.ShouldCancel) {
                        break;
                    }
                    if(timeout == 0) Thread.Sleep(100);
                    else
                        Thread.Sleep(timeout);
                }
                executingAsyncMethod = false;
            });
        }
        Task AsyncExecuteMethod2(int timeout) {
            return Task.Factory.StartNew(() => {
                for(int i = 0; i < 10; i++) {
                    if(asyncTestCommand.IsCancellationRequested) {
                        break;
                    }
                    if(timeout == 0) Thread.Sleep(100);
                    else
                        Thread.Sleep(timeout);
                }
                executingAsyncMethod = false;
            });
        }

        [Test, Asynchronous]
        public void AsyncIsExecuting() {
            asyncTestCommand = new AsyncCommand<int>(a => AsyncExecuteMethod(a));
            Assert.IsFalse(asyncTestCommand.IsExecuting);
            Assert.IsTrue(asyncTestCommand.CanExecute(100));
            executingAsyncMethod = true;
            EnqueueCallback(() => {
                asyncTestCommand.Execute(100);
                Assert.IsTrue(asyncTestCommand.IsExecuting);
                Assert.IsFalse(asyncTestCommand.CanExecute(100));
            });
            EnqueWaitForAsync(asyncTestCommand.executeTask);
            EnqueueWait(() => !asyncTestCommand.IsExecuting);
            EnqueueWindowUpdateLayout();
            Assert.IsFalse(asyncTestCommand.IsExecuting);
            Assert.IsTrue(asyncTestCommand.CanExecute(100));
            EnqueueConditional(() => !executingAsyncMethod);
            EnqueueTestComplete();
        }

        [Test, Asynchronous]
        public void AsyncCancelTest1() {
            AsyncCancelTestCore((a) => AsyncExecuteMethod(a));
        }
        [Test, Asynchronous]
        public void AsyncCancelTest2() {
            AsyncCancelTestCore((a) => AsyncExecuteMethod2(a));
        }
        public void AsyncCancelTestCore(Func<int, Task> a) {
            asyncTestCommand = new AsyncCommand<int>(a);
            Assert.IsNull(asyncTestCommand.CancellationTokenSource);
            EnqueueCallback(() => {
                asyncTestCommand.Execute(100);
                Assert.IsNotNull(asyncTestCommand.CancellationTokenSource);
                Assert.IsTrue(asyncTestCommand.IsExecuting);
                asyncTestCommand.Cancel();
                Assert.IsTrue(asyncTestCommand.ShouldCancel);
                Assert.IsTrue(asyncTestCommand.IsCancellationRequested);
                Assert.IsTrue(asyncTestCommand.IsExecuting);
            });
            EnqueueWait(() => asyncTestCommand.executeTask.IsCompleted);
            EnqueueWait(() => !asyncTestCommand.IsExecuting);
            EnqueueCallback(() => {
                Assert.IsFalse(asyncTestCommand.IsExecuting);
                Assert.IsFalse(asyncTestCommand.ShouldCancel);
                Assert.IsTrue(asyncTestCommand.IsCancellationRequested);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void AsyncCanExecuteTest1() {
            AsyncCanExecuteTestCore(a => AsyncExecuteMethod(a));
        }
        [Test, Asynchronous]
        public void AsyncCanExecuteTest2() {
            AsyncCanExecuteTestCore(a => AsyncExecuteMethod2(a));
        }
        public void AsyncCanExecuteTestCore(Func<int, Task> a) {
            asyncTestCommand = new AsyncCommand<int>(a, o => true);
            EnqueueCallback(() => {
                asyncTestCommand.Execute(100);
                Assert.IsTrue(asyncTestCommand.IsExecuting);
                Assert.IsFalse(asyncTestCommand.CanExecute(100));
                asyncTestCommand.Cancel();
                Assert.IsTrue(asyncTestCommand.IsExecuting);
                Assert.IsTrue(asyncTestCommand.ShouldCancel);
                Assert.IsTrue(asyncTestCommand.IsCancellationRequested);
            });
            EnqueueWait(() => asyncTestCommand.executeTask.IsCompleted);
            EnqueueWait(() => !asyncTestCommand.IsExecuting);
            EnqueueCallback(() => {
                Assert.IsFalse(asyncTestCommand.IsExecuting);
                Assert.IsTrue(asyncTestCommand.CanExecute(100));
                asyncTestCommand = new AsyncCommand<int>(a, o => false);
                asyncTestCommand.Execute(100);
                Assert.IsFalse(asyncTestCommand.IsExecuting);
                Assert.IsFalse(asyncTestCommand.CanExecute(100));
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void AllowMultipleExecutionTest1() {
            AllowMultipleExecutionTestCore(a => AsyncExecuteMethod(a));
        }
        [Test, Asynchronous]
        public void AllowMultipleExecutionTest2() {
            AllowMultipleExecutionTestCore(a => AsyncExecuteMethod2(a));
        }
        public void AllowMultipleExecutionTestCore(Func<int, Task> a) {
            asyncTestCommand = new AsyncCommand<int>(a, o => true) { AllowMultipleExecution = true };
            EnqueueCallback(() => {
                asyncTestCommand.Execute(100);
                Assert.IsTrue(asyncTestCommand.IsExecuting);
                Assert.IsTrue(asyncTestCommand.CanExecute(100));
                asyncTestCommand.Cancel();
                Assert.IsTrue(asyncTestCommand.IsExecuting);
                Assert.IsTrue(asyncTestCommand.CanExecute(100));
            });
            EnqueueWait(() => asyncTestCommand.executeTask.IsCompleted);
            EnqueueWait(() => !asyncTestCommand.IsExecuting);
            EnqueueCallback(() => {
                Assert.IsFalse(asyncTestCommand.IsExecuting);
                Assert.IsTrue(asyncTestCommand.CanExecute(100));
                asyncTestCommand = new AsyncCommand<int>(a, o => false) { AllowMultipleExecution = true };
                asyncTestCommand.Execute(100);
                Assert.IsFalse(asyncTestCommand.IsExecuting);
                Assert.IsFalse(asyncTestCommand.CanExecute(100));
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void IsExecutingPropertyChangedTest() {
            asyncTestCommand = new AsyncCommand<int>((a) => AsyncExecuteMethod(a));
            bool isExecutingChanged = false;

            ((INotifyPropertyChanged)asyncTestCommand).PropertyChanged += (s, e) => {
                if(e.PropertyName == "IsExecuting")
                    isExecutingChanged = true;
            };
            EnqueueCallback(() => {
                asyncTestCommand.Execute(100);
                Assert.IsTrue(isExecutingChanged);
                isExecutingChanged = false;
            });
            EnqueueWait(() => asyncTestCommand.executeTask.IsCompleted);
            EnqueueWait(() => !asyncTestCommand.IsExecuting);
            EnqueueWindowUpdateLayout();
            EnqueueConditional(() => isExecutingChanged);
            EnqueueTestComplete();
        }
#endif
    }
}
#pragma warning restore 612,618