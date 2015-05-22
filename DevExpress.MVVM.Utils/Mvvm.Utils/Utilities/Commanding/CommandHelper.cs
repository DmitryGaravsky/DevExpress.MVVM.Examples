namespace Mvvm.Utils.Commands {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class CommandHelper {
        public static IDisposable Bind<T>(T target, Action<T, Action> subscribe, Action<T, Func<bool>> updateState,
            Expression<Action> commandSelector, object source,
            Func<object> queryCommandParameter = null) {
            var commandProperty = GetCommandProperty(commandSelector, source);
            object command = commandProperty.GetValue(source, null);
            return BindCore<T>(target, subscribe, updateState, command, commandProperty.PropertyType, queryCommandParameter);
        }
        public static IDisposable Bind<T, TParameter>(T target, Action<T, Action> subscribe, Action<T, Func<bool>> updateState,
            Expression<Action<TParameter>> commandSelector, object source,
            Func<object> queryCommandParameter = null) {
            var commandProperty = GetCommandProperty(commandSelector, source);
            object command = commandProperty.GetValue(source, null);
            return BindCore<T>(target, subscribe, updateState, command, commandProperty.PropertyType, queryCommandParameter);
        }
        public static IDisposable Bind<T>(T target, Action<T, Action> subscribe, Action<T, Func<bool>> updateState,
            object command,
            Func<object> queryCommandParameter = null) {
            return BindCore<T>(target, subscribe, updateState, command, command.GetType(), queryCommandParameter);
        }
        static IDisposable BindCore<T>(T target, Action<T, Action> subscribe, Action<T, Func<bool>> updateState, object command, Type commandType, Func<object> queryCommandParameter) {
            if(queryCommandParameter == null)
                queryCommandParameter = () => null;
            CommandExpressionBuilder builder = GetCommandExpressionBuilder(commandType);
            updateState(target, builder.GetCanExecute(command, queryCommandParameter));
            subscribe(target, builder.GetExecute(command, queryCommandParameter));
            return builder.SubscribeCommandState(command, target, queryCommandParameter, updateState);
        }
        static CommandExpressionBuilder GetCommandExpressionBuilder(Type commandType) {
            CommandExpressionBuilder builder;
            if(!cache.TryGetValue(commandType, out builder)) {
                builder = new CommandExpressionBuilder(commandType);
                cache.Add(commandType, builder);
            }
            return builder;
        }
        internal static PropertyInfo GetCommandProperty(Expression<Action> commandSelector, object source) {
            MethodCallExpression callExpression = commandSelector.Body as MethodCallExpression;
            return MemberInfoHelper.GetCommandProperty(source, MVVMTypesResolver.Instance, callExpression.Method);
        }
        internal static PropertyInfo GetCommandProperty<T>(Expression<Action<T>> commandSelector, object source) {
            MethodCallExpression callExpression = commandSelector.Body as MethodCallExpression;
            return MemberInfoHelper.GetCommandProperty(source, MVVMTypesResolver.Instance, callExpression.Method);
        }
        #region CommandExpressionBuilder
        #region EventToCommand
        internal static object GetCommand(Expression<Action> commandSelector, object source, out Type commandType) {
            var commandProperty = GetCommandProperty(commandSelector, source);
            commandType = commandProperty.PropertyType;
            return commandProperty.GetValue(source, null);
        }
        internal static object GetCommand<T>(Expression<Action<T>> commandSelector, object source, out Type commandType) {
            var commandProperty = GetCommandProperty(commandSelector, source);
            commandType = commandProperty.PropertyType;
            return commandProperty.GetValue(source, null);
        }
        internal static Func<object> GetQueryCommandParameter<T, TValue>(Expression<Func<T, TValue>> parameterSelector, T source) {
            var instance = Expression.Convert(Expression.Constant(source, typeof(T)), typeof(T));
            var memberAccessExpression = Expression.MakeMemberAccess(
                instance, MemberInfoHelper.GetMember(parameterSelector));
            return Expression.Lambda<Func<object>>(memberAccessExpression).Compile();
        }
        internal static Func<bool> GetCanExecute(object command, Type commandType, Func<object> queryParameters) {
            CommandExpressionBuilder builder = GetCommandExpressionBuilder(commandType);
            return builder.GetCanExecute(command, queryParameters);
        }
        internal static Action GetExecute(object command, Type commandType, Func<object> queryParameters) {
            CommandExpressionBuilder builder = GetCommandExpressionBuilder(commandType);
            return builder.GetExecute(command, queryParameters);
        }
        #endregion
        static IDictionary<Type, CommandExpressionBuilder> cache = new Dictionary<Type, CommandExpressionBuilder>();
        class CommandExpressionBuilder {
            readonly Func<object, object, bool> canExecute = (c, p) => true;
            readonly Action<object, object> execute;
            readonly EventInfo canExecuteChangedEvent;
            ParameterExpression commandObject;
            ParameterExpression parameter;
            public CommandExpressionBuilder(Type commandType) {
                var executeMethod = MemberInfoHelper.GetMethodInfo(commandType, "Execute", MemberInfoHelper.SingleObjectParameterTypes);
                if(executeMethod == null)
                    throw new NotSupportedException(commandType.ToString() + ": Missing Execute() method");

                commandObject = Expression.Parameter(typeof(object), "command");
                parameter = Expression.Parameter(typeof(object), "parameter");
                var command = Expression.TypeAs(commandObject, commandType);

                this.execute = Expression.Lambda<Action<object, object>>(
                            Expression.Call(command, executeMethod, parameter),
                            commandObject, parameter
                        ).Compile();
                var canExecuteMethod = MemberInfoHelper.GetMethodInfo(commandType, "CanExecute", MemberInfoHelper.SingleObjectParameterTypes);
                if(canExecuteMethod != null) {
                    this.canExecute = Expression.Lambda<Func<object, object, bool>>(
                            Expression.Call(command, canExecuteMethod, parameter),
                            commandObject, parameter
                        ).Compile();
                    this.canExecuteChangedEvent = MemberInfoHelper.GetEventInfo(commandType, "CanExecuteChanged");
                }
            }
            public Func<bool> GetCanExecute(object command, Func<object> queryParameter) {
                return () => canExecute(command, queryParameter());
            }
            public Action GetExecute(object command, Func<object> queryParameter) {
                return () => execute(command, queryParameter());
            }
            static IDictionary<HandlerKey, HandlerExpressionBuilder> handlersCache = new Dictionary<HandlerKey, HandlerExpressionBuilder>();
            public IDisposable SubscribeCommandState<T>(object command, T target, Func<object> queryParameter, Action<T, Func<bool>> updateState) {
                if(canExecuteChangedEvent == null)
                    return null;
                HandlerKey key = new HandlerKey(typeof(T), canExecuteChangedEvent.DeclaringType);
                HandlerExpressionBuilder builder;
                if(!handlersCache.TryGetValue(key, out builder)) {
                    builder = new HandlerExpressionBuilder(canExecuteChangedEvent);
                    handlersCache.Add(key, builder);
                }
                Action updateStateFunc = () => updateState(target, GetCanExecute(command, queryParameter));
                var handlerDelegate = builder.GetHandler(Expression.Call(
                        Expression.Constant(updateStateFunc.Target), updateStateFunc.Method));
                builder.Subscribe(command, handlerDelegate);
                return new CommandStateSubscription(key, command, handlerDelegate);
            }
            class CommandStateSubscription : IDisposable {
                HandlerKey key;
                object command;
                Delegate handlerDelegate;
                public CommandStateSubscription(HandlerKey key, object command, Delegate handlerDelegate) {
                    this.key = key;
                    this.command = command;
                    this.handlerDelegate = handlerDelegate;
                }
                void IDisposable.Dispose() {
                    UnSubscribe();
                    GC.SuppressFinalize(this);
                }
                void UnSubscribe() {
                    HandlerExpressionBuilder builder;
                    if(handlersCache.TryGetValue(key, out builder))
                        builder.Unsubscribe(command, handlerDelegate);
                }
            }
        }
        #endregion CommandExpressionBuilder
    }
}