#if DEBUGTEST
namespace Mvvm.Utils.Bindings.Tests {
    using System;
    using System.ComponentModel;
    using Mvvm.Utils.Behaviors.Tests;
    using NUnit.Framework;

    #region Test classes
    class TestBindingSource : TestEventSource, INotifyPropertyChanged {
        string nameCore;
        public string Name {
            get { return nameCore; }
            set {
                if(nameCore == value) return;
                nameCore = value;
                OnNameChanged();
            }
        }
        void OnNameChanged() {
            if(NameChanged != null)
                NameChanged(this, EventArgs.Empty);
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Name"));
        }
        public event EventHandler NameChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }
    class TestTarget {
        public virtual bool IsLoaded { get; set; }
        string nameCore;
        public string TargetName {
            get { return nameCore; }
            set {
                if(nameCore == value) return;
                nameCore = value;
                OnTargetNameChanged();
            }
        }
        protected virtual void OnTargetNameChanged() { }
    }
    class TestTarget_CLR : TestTarget {
        public event EventHandler TargetNameChanged;
        protected override void OnTargetNameChanged() {
            if(TargetNameChanged != null)
                TargetNameChanged(this, EventArgs.Empty);
        }
    }
    class TestTarget_NPC : TestTarget, INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        protected override void OnTargetNameChanged() {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("TargetName"));
        }
    }
    class TestTarget_Event : TestTarget {
        bool loaded;
        public override bool IsLoaded {
            get { return loaded; }
            set {
                if(loaded == value) return;
                loaded = value;
                OnIsLoadedChanged();
            }
        }
        public event EventHandler IsLoadedChanged;
        protected virtual void OnIsLoadedChanged() {
            loadedChangedCount++;
            if(IsLoadedChanged != null)
                IsLoadedChanged(this, EventArgs.Empty);
        }
        internal int loadedChangedCount;
    }
    #endregion Test classes
    [TestFixture]
    public class BindingHelperTests {
        [Test]
        public void Test00_PropertyChangedEventTrigger_SetTrigger() {
            TestBindingSource source = new TestBindingSource();
            TestTarget target = new TestTarget();
            Assert.IsFalse(target.IsLoaded);
            using(BindingHelper.SetPCETriggerCore<EventArgs, bool>(source, "Loaded", (e) => true, BindingHelper.CreateTriggerAction(target, x => x.IsLoaded))) {
                source.RaiseLoaded();
                Assert.IsTrue(target.IsLoaded);
            }
        }
        [Test]
        public void Test00_CLRPropertyChangedTrigger_SetTrigger() {
            TestBindingSource source = new TestBindingSource();
            TestTarget target = new TestTarget();
            using(BindingHelper.SetTrigger(source, (s) => s.Name, BindingHelper.CreateTriggerAction(target, x => x.TargetName), true)) {
                source.Name = "Test";
                Assert.AreEqual(source.Name, target.TargetName);
            }
        }
        [Test]
        public void Test00_INPCPropertyChangedTrigger_SetTrigger() {
            TestBindingSource source = new TestBindingSource();
            TestTarget target = new TestTarget();
            using(BindingHelper.SetTrigger(source, (s) => s.Name, BindingHelper.CreateTriggerAction(target, x => x.TargetName), false)) {
                source.Name = "Test";
                Assert.AreEqual(source.Name, target.TargetName);
            }
        }
        [Test]
        public void Test01_SetBinding_OneWay() {
            TestBindingSource source = new TestBindingSource() { Name = "Start" };
            TestTarget target = new TestTarget();
            using(BindingHelper.SetBinding(target, (t) => t.TargetName, source, typeof(TestBindingSource), "Name")) {
                Assert.AreEqual(source.Name, target.TargetName);
                source.Name = "Test";
                Assert.AreEqual(source.Name, target.TargetName);
                target.TargetName = "Name";
                Assert.AreNotEqual(source.Name, target.TargetName);
            }
        }
        [Test]
        public void Test01_SetBinding_OneWay_Event() {
            TestBindingSource source = new TestBindingSource();
            TestTarget target = new TestTarget();
            Assert.IsFalse(target.IsLoaded);
            using(BindingHelper.SetBinding<EventArgs, TestTarget, bool>(target, x => x.IsLoaded, source, "Loaded", (e) => true)) {
                Assert.IsFalse(target.IsLoaded);
                source.RaiseLoaded();
                Assert.IsTrue(target.IsLoaded);
            }
        }
        [Test]
        public void Test01_SetBinding_TwoWay_NPC() {
            TestBindingSource source = new TestBindingSource() { Name = "Start" };
            TestTarget target = new TestTarget_NPC();
            using(BindingHelper.SetBinding(target, (t) => t.TargetName, source, typeof(TestBindingSource), "Name")) {
                Assert.AreEqual(source.Name, target.TargetName);
                source.Name = "Test";
                Assert.AreEqual(source.Name, target.TargetName);
                target.TargetName = "Name";
                Assert.AreEqual(target.TargetName, source.Name);
            }
        }
        [Test]
        public void Test01_SetBinding_TwoWay_CLR() {
            TestBindingSource source = new TestBindingSource() { Name = "Start" };
            TestTarget target = new TestTarget_CLR();
            using(BindingHelper.SetBinding(target, (t) => t.TargetName, source, typeof(TestBindingSource), "Name")) {
                Assert.AreEqual(source.Name, target.TargetName);
                source.Name = "Test";
                Assert.AreEqual(source.Name, target.TargetName);
                target.TargetName = "Name";
                Assert.AreEqual(target.TargetName, source.Name);
            }
        }
        [Test]
        public void Test01_SetBinding_TwoWay() {
            TestBindingSource source = new TestBindingSource();
            TestTarget_Event target = new TestTarget_Event();
            int callbackCount = 0;
            using(BindingHelper.SetBinding<EventArgs, TestBindingSource, TestTarget_Event, bool>(target, x => x.IsLoaded, source, "Loaded",
                (e) => true, (s, v) => callbackCount++)) {
                Assert.IsFalse(target.IsLoaded);

                source.RaiseLoaded();
                Assert.IsTrue(target.IsLoaded);
                Assert.AreEqual(0, callbackCount);
                Assert.AreEqual(1, source.eventCount);
                Assert.AreEqual(1, target.loadedChangedCount);
                target.IsLoaded = false;
                Assert.AreEqual(1, callbackCount);
                Assert.AreEqual(1, source.eventCount);
                Assert.AreEqual(2, target.loadedChangedCount);
            }
        }
    }
    [TestFixture]
    public class BindingHelperTests_MultiTargetBinding {
        IPropertyBinding binding1;
        IPropertyBinding binding2;
        [TearDown]
        public void TearDown() {
            Ref.Dispose(ref binding1);
            Ref.Dispose(ref binding2);
        }
        [Test]
        public void Test01_SetBinding_OneWay_MultiTarget() {
            TestBindingSource source = new TestBindingSource() { Name = "Start" };
            TestTarget target1 = new TestTarget();
            TestTarget target2 = new TestTarget();
            binding1 = BindingHelper.SetBinding(target1, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");
            binding2 = BindingHelper.SetBinding(target2, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");

            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);
            source.Name = "Test";
            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);
            target1.TargetName = "Name1";
            Assert.AreNotEqual(source.Name, target1.TargetName);
            target2.TargetName = "Name2";
            Assert.AreNotEqual(source.Name, target2.TargetName);
        }
        [Test]
        public void Test01_SetBinding_OneWay_MultiTarget_Unbind() {
            TestBindingSource source = new TestBindingSource() { Name = "Start" };
            TestTarget target1 = new TestTarget();
            TestTarget target2 = new TestTarget();
            binding1 = BindingHelper.SetBinding(target1, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");
            binding2 = BindingHelper.SetBinding(target2, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");

            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);
            source.Name = "Test";
            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);

            Ref.Dispose(ref binding2);
            source.Name = "Test2";
            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreNotEqual(source.Name, target2.TargetName);
        }
        [Test]
        public void Test02_SetBinding_OneWay_Event_MultiTarget() {
            TestBindingSource source = new TestBindingSource();
            TestTarget target1 = new TestTarget();
            TestTarget target2 = new TestTarget();
            Assert.IsFalse(target1.IsLoaded);
            Assert.IsFalse(target2.IsLoaded);
            binding1 = BindingHelper.SetBinding<EventArgs, TestTarget, bool>(target1, x => x.IsLoaded, source, "Loaded", (e) => true);
            binding2 = BindingHelper.SetBinding<EventArgs, TestTarget, bool>(target2, x => x.IsLoaded, source, "Loaded", (e) => true);

            Assert.IsFalse(target1.IsLoaded);
            Assert.IsFalse(target2.IsLoaded);
            source.RaiseLoaded();
            Assert.IsTrue(target1.IsLoaded);
            Assert.IsTrue(target2.IsLoaded);
        }
        [Test]
        public void Test02_SetBinding_OneWay_Event_MultiTarget_Unbind() {
            TestBindingSource source = new TestBindingSource();
            TestTarget target1 = new TestTarget();
            TestTarget target2 = new TestTarget();
            Assert.IsFalse(target1.IsLoaded);
            Assert.IsFalse(target2.IsLoaded);
            binding1 = BindingHelper.SetBinding<EventArgs, TestTarget, bool>(target1, x => x.IsLoaded, source, "Loaded", (e) => true);
            binding2 = BindingHelper.SetBinding<EventArgs, TestTarget, bool>(target2, x => x.IsLoaded, source, "Loaded", (e) => true);

            Assert.IsFalse(target1.IsLoaded);
            Assert.IsFalse(target2.IsLoaded);
            source.RaiseLoaded();
            Assert.IsTrue(target1.IsLoaded);
            Assert.IsTrue(target2.IsLoaded);

            target1.IsLoaded = false;
            target2.IsLoaded = false;
            Ref.Dispose(ref binding2);
            source.RaiseLoaded();
            Assert.IsTrue(target1.IsLoaded);
            Assert.IsFalse(target2.IsLoaded);
        }
        [Test]
        public void Test02_SetBinding_TwoWay_NPC_MultiTarget() {
            TestBindingSource source = new TestBindingSource() { Name = "Start" };
            TestTarget target1 = new TestTarget_NPC();
            TestTarget target2 = new TestTarget_NPC();
            binding1 = BindingHelper.SetBinding(target1, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");
            binding2 = BindingHelper.SetBinding(target2, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");

            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);

            source.Name = "Test";
            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);

            target1.TargetName = "Name";
            Assert.AreEqual(target1.TargetName, source.Name);
            Assert.AreEqual(target1.TargetName, target2.TargetName);
        }
        [Test]
        public void Test02_SetBinding_TwoWay_NPC_MultiTarget_Unbing() {
            TestBindingSource source = new TestBindingSource() { Name = "Start" };
            TestTarget target1 = new TestTarget_NPC();
            TestTarget target2 = new TestTarget_NPC();
            binding1 = BindingHelper.SetBinding(target1, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");
            binding2 = BindingHelper.SetBinding(target2, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");

            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);

            source.Name = "Test";
            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);

            target1.TargetName = "Name";
            Assert.AreEqual(target1.TargetName, source.Name);
            Assert.AreEqual(target1.TargetName, target2.TargetName);

            Ref.Dispose(ref binding2);
            source.Name = "Test2";
            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreNotEqual(source.Name, target2.TargetName);
            Assert.AreNotEqual(target1.TargetName, target2.TargetName);
        }
        [Test]
        public void Test02_SetBinding_TwoWay_CLR_MultiTarget() {
            TestBindingSource source = new TestBindingSource() { Name = "Start" };
            TestTarget target1 = new TestTarget_CLR();
            TestTarget target2 = new TestTarget_CLR();
            binding1 = BindingHelper.SetBinding(target1, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");
            binding2 = BindingHelper.SetBinding(target2, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");

            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);

            source.Name = "Test";
            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);

            target1.TargetName = "Name";
            Assert.AreEqual(target1.TargetName, source.Name);
            Assert.AreEqual(target1.TargetName, target2.TargetName);
        }
        [Test]
        public void Test02_SetBinding_TwoWay_CLR_MultiTarget_Unbind() {
            TestBindingSource source = new TestBindingSource() { Name = "Start" };
            TestTarget target1 = new TestTarget_CLR();
            TestTarget target2 = new TestTarget_CLR();
            binding1 = BindingHelper.SetBinding(target1, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");
            binding2 = BindingHelper.SetBinding(target2, (t) => t.TargetName, source, typeof(TestBindingSource), "Name");

            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);

            source.Name = "Test";
            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreEqual(source.Name, target2.TargetName);

            target1.TargetName = "Name";
            Assert.AreEqual(target1.TargetName, source.Name);
            Assert.AreEqual(target1.TargetName, target2.TargetName);

            Ref.Dispose(ref binding2);
            source.Name = "Test2";
            Assert.AreEqual(source.Name, target1.TargetName);
            Assert.AreNotEqual(source.Name, target2.TargetName);
            Assert.AreNotEqual(target1.TargetName, target2.TargetName);
        }
    }
    [TestFixture]
    public class BindingHelperTests_Dependencies {
        #region TestClasses
        class Control {
            public Control() {
                isEnabled = true;
            }
            bool isEnabled;
            public bool Enabled {
                get { return isEnabled; }
                set {
                    if(isEnabled == value) return;
                    isEnabled = value;
                    if(EnabledChanged != null)
                        EnabledChanged(this, EventArgs.Empty);
                }
            }
            public event EventHandler EnabledChanged;
        }
        class TextEditor : Control {
            string editValue;
            public string EditValue {
                get { return editValue; }
                set {
                    if(editValue == value) return;
                    editValue = value;
                    if(EditValueChanged != null)
                        EditValueChanged(this, EventArgs.Empty);
                }
            }
            public event EventHandler EditValueChanged;
        }
        class CheckEditor : Control {
            bool isChecked;
            public bool Checked {
                get { return isChecked; }
                set {
                    if(isChecked == value) return;
                    isChecked = value;
                    if(CheckedChanged != null)
                        CheckedChanged(this, EventArgs.Empty);
                }
            }
            public event EventHandler CheckedChanged;
        }
        class ViewModel : INotifyPropertyChanged {
            string textCore;
            public string Text {
                get { return textCore; }
                set {
                    if(textCore == value) return;
                    textCore = value;
                    RaisePropertyChanged("Text");
                }
            }
            bool isActiveCore;
            public bool IsActive {
                get { return isActiveCore; }
                set {
                    if(isActiveCore == value) return;
                    isActiveCore = value;
                    RaisePropertyChanged("IsActive");
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
            void RaisePropertyChanged(string name) {
                if(PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion TestClasses
        DisposableObjectsContainer c;
        [SetUp]
        public void SetUp() {
            c = new DisposableObjectsContainer();
        }
        [TearDown]
        public void TearDown() {
            Ref.Dispose(ref c);
        }
        [Test]
        public void Test00_T224520() {
            CheckEditor cEdit = new CheckEditor();
            TextEditor tEdit = new TextEditor();
            ViewModel vm = new ViewModel();
            c.Register(BindingHelper.SetBinding(cEdit, ce => ce.Checked, vm, typeof(ViewModel), "IsActive"));
            c.Register(BindingHelper.SetBinding(tEdit, t => t.Enabled, vm, typeof(ViewModel), "IsActive"));
            c.Register(BindingHelper.SetBinding(tEdit, t => t.EditValue, vm, typeof(ViewModel), "Text"));
            Assert.IsFalse(cEdit.Checked);
            Assert.IsFalse(tEdit.Enabled);
            Assert.IsNull(tEdit.EditValue);

            vm.IsActive = true;
            Assert.IsTrue(cEdit.Checked);
            Assert.IsTrue(tEdit.Enabled);
            Assert.IsNull(tEdit.EditValue);

            tEdit.EditValue = "!!!";
            Assert.AreEqual("!!!", vm.Text);
        }
    }
    [TestFixture]
    public class BindingHelperTests_NestedBinding {
        #region TestClasses
        class ViewModelBase : INotifyPropertyChanged {
            protected void Set<T>(System.Linq.Expressions.Expression<Func<T>> selector, ref T field, T value) {
                if(object.Equals(field, value)) return;
                field = value;
                RaisePropertyChanged(ExpressionHelper.GetPropertyName(selector));
            }
            protected void RaisePropertyChanged(string propertyName) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if(handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }
        class ChildViewModel : ViewModelBase {
            string nameCore;
            public string Name {
                get { return nameCore; }
                set { Set(() => Name, ref nameCore, value); }
            }
        }
        class ParentViewModel : ViewModelBase {
            ChildViewModel childCore;
            public ChildViewModel Child {
                get { return childCore; }
                set { Set(() => Child, ref childCore, value); }
            }
        }
        class ScopeViewModel : ViewModelBase {
            ParentViewModel nodeCore;
            public ParentViewModel Scope {
                get { return nodeCore; }
                set { Set(() => Scope, ref nodeCore, value); }
            }
        }
        class Target {
            public string Name { get; set; }
        }
        #endregion TestClasses
        [Test]
        public void Test00_NestedUpdateProperty() {
            Target target = new Target();
            ParentViewModel parent = new ParentViewModel()
            {
                Child = new ChildViewModel() { Name = "Child" }
            };
            using(BindingHelper.SetBinding(target, (t) => t.Name, parent, typeof(ParentViewModel), "Child.Name")) {
                Assert.AreEqual("Child", target.Name);
                parent.Child.Name = "Test";
                Assert.AreEqual("Test", target.Name);
            }
        }
        [Test]
        public void Test00_NestedUpdateSource() {
            Target target = new Target();
            ParentViewModel parent = new ParentViewModel()
            {
                Child = new ChildViewModel() { Name = "Child" }
            };
            using(var b = BindingHelper.SetBinding(target, (t) => t.Name, parent, typeof(ParentViewModel), "Child.Name")) {
                Assert.AreEqual("Child", target.Name);
                parent.Child = new ChildViewModel() { Name = "Test" };
                Assert.AreEqual("Test", target.Name);
            }
        }
        [Test]
        public void Test00_NestedUpdateSource_MultiLevel() {
            Target target = new Target();
            ScopeViewModel root = new ScopeViewModel()
            {
                Scope = new ParentViewModel()
                {
                    Child = new ChildViewModel() { Name = "Child" },
                }
            };
            using(var b = BindingHelper.SetBinding(target, (t) => t.Name, root, typeof(ScopeViewModel), "Scope.Child.Name")) {
                Assert.AreEqual("Child", target.Name);
                root.Scope.Child.Name = "Test";
                Assert.AreEqual("Test", target.Name);
                //
                root.Scope.Child = new ChildViewModel() { Name = "Child2" };
                Assert.AreEqual("Child2", target.Name);
                root.Scope = new ParentViewModel() { Child = new ChildViewModel() { Name = "Child3" } };
                Assert.AreEqual("Child3", target.Name);
            }
        }
    }
}
#endif