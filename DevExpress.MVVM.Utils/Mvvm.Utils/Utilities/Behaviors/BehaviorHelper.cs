namespace Mvvm.Utils.Behaviors {
    using System;
    using System.Collections.Generic;

    public static class BehaviorHelper {
        static IMVVMViewModelSource GetViewModelSource() {
            return MVVMViewModelSource.Instance;
        }
        static IMVVMInterfaces GetMVVMInterfaces() {
            return MVVMInterfaces.Instance;
        }
        static object GetParentViewModel() {
            return null;
        }
        static IMVVMViewModelSource GetViewModelSource(MVVMContext context) {
            return (context != null) ? context.GetViewModelSource() : GetViewModelSource();
        }
        static IMVVMInterfaces GetMVVMInterfaces(MVVMContext context) {
            return (context != null) ? context.GetMVVMInterfaces() : GetMVVMInterfaces();
        }
        static object GetParentViewModel(MVVMContext context) {
            return (context != null) ? context.ViewModel : GetParentViewModel();
        }
        public static IDisposable Attach<TBehavior>(this object source, MVVMContext context)
            where TBehavior : BehaviorBase {
            return (source != null) ? AttachCore<TBehavior>(source, GetParentViewModel(context), GetViewModelSource(context), GetMVVMInterfaces(context)) : null;
        }
        public static IDisposable Attach<TBehavior>(this object source)
            where TBehavior : BehaviorBase {
            return (source != null) ? AttachCore<TBehavior>(source, GetParentViewModel(), GetViewModelSource(), GetMVVMInterfaces()) : null;
        }
        public static void Detach(object source) {
            List<BehaviorRefCounter> list;
            if((source != null) && behaviors.TryGetValue(source, out list)) {
                foreach(IDisposable command in list)
                    command.Dispose();
                behaviors.Remove(source);
            }
        }
        public static void Detach<TBehavior>(this object source) {
            List<BehaviorRefCounter> bList;
            if((source != null) && behaviors.TryGetValue(source, out bList)) {
                var list = bList.FindAll((b) => b.Match(typeof(TBehavior)));
                foreach(IDisposable command in list)
                    command.Dispose();
                if(bList.Count == 0)
                    behaviors.Remove(source);
            }
        }
        static IDictionary<object, List<BehaviorRefCounter>> behaviors = new Dictionary<object, List<BehaviorRefCounter>>();
        internal static IDisposable AttachCore<TBehavior>(object source, object parent, IMVVMViewModelSource viewModelSource, IMVVMInterfaces mvvmInterfaces, params object[] parameters)
            where TBehavior : BehaviorBase {
            return AttachCore<TBehavior>(source, parent, viewModelSource, mvvmInterfaces, null, parameters);
        }
        internal static IDisposable AttachCore<TBehavior>(object source, object parent, IMVVMViewModelSource viewModelSource, IMVVMInterfaces mvvmInterfaces,
            Action<TBehavior> behaviorSettings, params object[] parameters)
            where TBehavior : BehaviorBase {
            List<BehaviorRefCounter> bList;
            if(!behaviors.TryGetValue(source, out bList)) {
                bList = new List<BehaviorRefCounter>();
                behaviors.Add(source, bList);
            }
            TBehavior behavior = CreateBehavior<TBehavior>(parent, viewModelSource, mvvmInterfaces, parameters);
            if(behaviorSettings != null)
                behaviorSettings(behavior);
            return new BehaviorRefCounter(bList, behavior, source);
        }
        static TBehavior CreateBehavior<TBehavior>(object parent, IMVVMViewModelSource viewModelSource, IMVVMInterfaces mvvmInterfaces, params object[] parameters)
            where TBehavior : BehaviorBase {
            TBehavior behavior = viewModelSource.Create(typeof(TBehavior), parameters) as TBehavior;
            behavior.MVVMInterfaces = mvvmInterfaces;
            if(mvvmInterfaces != null && parent != null)
                mvvmInterfaces.SetParentViewModel(behavior, parent);
            return behavior;
        }
        sealed class BehaviorRefCounter : IDisposable {
            WeakReference wRef;
            List<BehaviorRefCounter> list;
            public BehaviorRefCounter(List<BehaviorRefCounter> list, BehaviorBase behavior, object source) {
                if(behavior != null) {
                    this.list = list;
                    this.wRef = new WeakReference(behavior);
                    list.Add(this);
                    behavior.Source = source;
                }
            }
            void IDisposable.Dispose() {
                BehaviorBase behavior = wRef.Target as BehaviorBase;
                if(behavior != null) behavior.Source = null;
                list.Remove(this);
            }
            public bool Match(Type type) {
                BehaviorBase behavior = wRef.Target as BehaviorBase;
                return (behavior == null) || type.IsAssignableFrom(behavior.GetType());
            }
        }
    }
}

#if DEBUGTEST
namespace Mvvm.Utils.Behaviors.Tests {
    using System;
    using NUnit.Framework;

    #region Test classes
    class TestEventSource {
        public event EventHandler Loaded;
        public void RaiseLoaded() {
            if(Loaded != null)
                Loaded(this, EventArgs.Empty);
            eventCount++;
        }
        internal int eventCount;
    }
    class TestEventTrigger : EventTriggerBase<EventArgs> {
        public TestEventTrigger()
            : base("Loaded") {
        }
        public int eventCount;
        protected override void OnEvent() {
            eventCount++;
        }
    }
    class EventTriggerTests_ViewModelSource<T> : IMVVMViewModelSource
        where T : new() {
        public T instance;
        object IMVVMViewModelSource.Create(Type viewModelType, params object[] parameters) {
            instance = new T();
            return instance;
        }
    }
    #endregion Test classes
    [TestFixture]
    public class EventTriggerTests {
        [Test]
        public void Bind_Loaded_Command() {
            TestEventSource source = new TestEventSource();
            TestEventTrigger trigger = new TestEventTrigger();
            trigger.Source = source;
            Assert.IsTrue(trigger.IsAttached);
            Assert.AreEqual(source, trigger.Source);
            Assert.AreEqual(typeof(TestEventSource), trigger.SourceType);
            Assert.AreEqual(0, trigger.eventCount);
            source.RaiseLoaded();
            Assert.AreEqual(1, trigger.eventCount);
            trigger.Source = null;
            Assert.IsNull(trigger.Source);
            Assert.IsNull(trigger.SourceType);
            Assert.IsFalse(trigger.IsAttached);
            source.RaiseLoaded();
            Assert.AreEqual(1, trigger.eventCount);
        }
        [Test]
        public void Bind_Behavior_Attach() {
            var vms = new EventTriggerTests_ViewModelSource<TestEventTrigger>();
            TestEventSource source = new TestEventSource();
            var detach = BehaviorHelper.AttachCore<TestEventTrigger>(source, null, vms, null);
            var trigger = vms.instance;
            Assert.IsTrue(trigger.IsAttached);
            Assert.AreEqual(source, trigger.Source);
            Assert.AreEqual(typeof(TestEventSource), trigger.SourceType);
            Assert.AreEqual(0, trigger.eventCount);
            source.RaiseLoaded();
            Assert.AreEqual(1, trigger.eventCount);
            detach.Dispose();
            Assert.IsNull(trigger.Source);
            Assert.IsNull(trigger.SourceType);
            Assert.IsFalse(trigger.IsAttached);
            source.RaiseLoaded();
            Assert.AreEqual(1, trigger.eventCount);
        }
        [Test]
        public void Bind_Behavior_Detach() {
            var vms = new EventTriggerTests_ViewModelSource<TestEventTrigger>();
            TestEventSource source = new TestEventSource();
            BehaviorHelper.AttachCore<TestEventTrigger>(source, null, vms, null);
            var trigger = vms.instance;
            Assert.IsTrue(trigger.IsAttached);
            Assert.AreEqual(source, trigger.Source);
            Assert.AreEqual(typeof(TestEventSource), trigger.SourceType);
            Assert.AreEqual(0, trigger.eventCount);
            source.RaiseLoaded();
            Assert.AreEqual(1, trigger.eventCount);
            BehaviorHelper.Detach<TestEventTrigger>(source);
            Assert.IsNull(trigger.Source);
            Assert.IsNull(trigger.SourceType);
            Assert.IsFalse(trigger.IsAttached);
            source.RaiseLoaded();
            Assert.AreEqual(1, trigger.eventCount);
        }
    }
}
#endif