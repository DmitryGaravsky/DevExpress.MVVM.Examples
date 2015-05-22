namespace Mvvm.Utils {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    class HandlerExpressionBuilder {
        Type handlerType;
        Action<object, object> addEvent;
        Action<object, object> removeEvent;
        public HandlerExpressionBuilder(EventInfo eventInfo) {
            this.handlerType = eventInfo.EventHandlerType;
            var handler = Expression.Parameter(typeof(object));
            var source = Expression.Parameter(typeof(object));
            var typedHandler = Expression.TypeAs(handler, handlerType);
            var typedSource = Expression.TypeAs(source, eventInfo.DeclaringType);
            var addEventMethod = eventInfo.GetAddMethod(true);
            this.addEvent = GetEventMethod(source, handler, typedSource, addEventMethod, typedHandler);
            var removeEventMethod = eventInfo.GetRemoveMethod(true);
            this.removeEvent = GetEventMethod(source, handler, typedSource, removeEventMethod, typedHandler);
        }
        public void Subscribe(object source, Delegate handlerDelegate) {
            addEvent(source, handlerDelegate);
        }
        public void Unsubscribe(object source, Delegate handlerDelegate) {
            removeEvent(source, handlerDelegate);
        }
        public Delegate GetHandler(MethodCallExpression triggerExpression) {
            return GetHandler(handlerType, triggerExpression, GetEventHandlerParameters(handlerType));
        }
        public Delegate GetHandler(MethodCallExpression beforeExpression, MethodCallExpression triggerExpression, MethodCallExpression afterExpression) {
            var handlerParameters = GetEventHandlerParameters(handlerType);
            if(handlerParameters.Length > 0) {
                var args = handlerParameters[handlerParameters.Length - 1];
                var beforeExpressionWithArgs = Expression<Action<object>>.Call(
                    beforeExpression.Object, beforeExpression.Method, args);
                return GetHandler(handlerType, beforeExpressionWithArgs, triggerExpression, afterExpression, handlerParameters);
            }
            return GetHandler(handlerType, triggerExpression, handlerParameters);
        }
        static Delegate GetHandler(Type handlerType, MethodCallExpression beforeExpressionWithArgs, MethodCallExpression triggerExpression, MethodCallExpression afterExpression, ParameterExpression[] handlerParameters) {
            return Expression.Lambda(handlerType,
                        Expression.Block(
                            beforeExpressionWithArgs,
                            triggerExpression,
                            afterExpression),
                        handlerParameters
                    ).Compile();
        }
        static Delegate GetHandler(Type handlerType, MethodCallExpression triggerExpression, ParameterExpression[] handlerParameters) {
            return Expression.Lambda(handlerType,
                triggerExpression,
                handlerParameters
            ).Compile();
        }
        static Action<object, object> GetEventMethod(ParameterExpression source, ParameterExpression handler, UnaryExpression typedSource, MethodInfo eventMethod, UnaryExpression typedHandler) {
            return Expression.Lambda<Action<object, object>>(
                    Expression.Call(typedSource, eventMethod, typedHandler),
                    source, handler
                ).Compile();
        }
        static ParameterExpression[] GetEventHandlerParameters(Type handlerType) {
            var invokeParameters = handlerType.GetMethod("Invoke").GetParameters();
            var parameters = new ParameterExpression[invokeParameters.Length];
            for(int i = 0; i < parameters.Length; i++) {
                var p = invokeParameters[i];
                parameters[i] = Expression.Parameter(p.ParameterType, p.Name);
            }
            return parameters;
        }
    }
}