namespace Mvvm.Utils.Behaviors {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public abstract class EventTriggerBase : BehaviorBase {
        protected readonly string EventName;
        public EventTriggerBase(string eventName) {
            if(string.IsNullOrEmpty(eventName))
                throw new System.NotSupportedException("Event name should not be empty");
            this.EventName = eventName;
        }
        protected sealed override void OnAttach() {
            var eventInfo = GetEventInfo();
            if(eventInfo != null)
                Subscribe(eventInfo);
        }
        protected sealed override void OnDetach() {
            var eventInfo = GetEventInfo();
            if(eventInfo != null)
                Unsubscribe(eventInfo);
        }
        object argsCore;
        protected object Args {
            get { return argsCore; }
        }
        void SetArgs(object args) {
            this.argsCore = args;
        }
        void ResetArgs() {
            this.argsCore = null;
        }
        protected abstract void OnEvent();
        static IDictionary<HandlerKey, HandlerExpressionBuilder> handlersCache = new Dictionary<HandlerKey, HandlerExpressionBuilder>();
        Delegate handlerDelegate;
        void Subscribe(EventInfo eventInfo) {
            HandlerKey key = new HandlerKey(GetType(), eventInfo.DeclaringType);
            HandlerExpressionBuilder builder;
            if(!handlersCache.TryGetValue(key, out builder)) {
                builder = new HandlerExpressionBuilder(eventInfo);
                handlersCache.Add(key, builder);
            }
            if(handlerDelegate == null) {
                handlerDelegate = builder.GetHandler(GetBeforeEventExpression(), GetOnEventExpression(), GetAfterEventExpression());
                builder.Subscribe(Source, handlerDelegate);
            }
        }
        void Unsubscribe(EventInfo eventInfo) {
            HandlerKey key = new HandlerKey(GetType(), eventInfo.DeclaringType);
            HandlerExpressionBuilder builder;
            if(handlersCache.TryGetValue(key, out builder))
                builder.Unsubscribe(Source, handlerDelegate);
            this.handlerDelegate = null;
        }
        MethodCallExpression GetBeforeEventExpression() {
            return ((Expression<Action<object>>)((e) => SetArgs(e))).Body as MethodCallExpression;
        }
        MethodCallExpression GetOnEventExpression() {
            return ((Expression<Action>)(() => OnEvent())).Body as MethodCallExpression;
        }
        MethodCallExpression GetAfterEventExpression() {
            return ((Expression<Action>)(() => ResetArgs())).Body as MethodCallExpression;
        }
        EventInfo GetEventInfo() {
            return MemberInfoHelper.GetEventInfo(SourceType, EventName, BindingFlags.Public | BindingFlags.Instance);
        }
    }
    //
    public abstract class EventTriggerBase<TEventArgs> : EventTriggerBase
        where TEventArgs : EventArgs {
        protected EventTriggerBase(string eventName)
            : base(eventName) {
        }
        protected new TEventArgs Args { get { return base.Args as TEventArgs; } }
    }
}
