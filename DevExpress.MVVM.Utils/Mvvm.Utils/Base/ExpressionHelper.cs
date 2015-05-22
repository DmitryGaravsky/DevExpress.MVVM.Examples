using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Mvvm.Utils {
    public static class ExpressionHelper {
        public static string GetPath<T>(Expression<Func<T>> expression) {
            return GetPath(expression);
        }
        public static string GetPropertyName<T>(Expression<Func<T>> expression) {
            return GetPropertyName((LambdaExpression)expression);
        }
        public static string GetPath(LambdaExpression expression) {
            var sb = new System.Text.StringBuilder();
            MemberExpression memberExpression = GetMemberExpression(expression);
            while(IsPropertyExpression(memberExpression)) {
                if(sb.Length > 0)
                    sb.Insert(0, '.');
                sb.Insert(0, memberExpression.Member.Name);
                memberExpression = memberExpression.Expression as MemberExpression;
            }
            return sb.ToString();
        }
        public static string GetPropertyName(LambdaExpression expression) {
            MemberExpression memberExpression = GetMemberExpression(expression);
            if(IsPropertyExpression(memberExpression.Expression as MemberExpression))
                throw new ArgumentException("Expression: " + expression.ToString());
            return memberExpression.Member.Name;
        }
        static bool IsPropertyExpression(MemberExpression expression) {
            return (expression != null) && (expression.Member.MemberType == MemberTypes.Property);
        }
        static MemberExpression GetMemberExpression(LambdaExpression expression) {
            if(expression == null)
                throw new ArgumentNullException("expression");
            Expression body = expression.Body;
            if(body is UnaryExpression)
                body = ((UnaryExpression)body).Operand;
            MemberExpression memberExpression = body as MemberExpression;
            if(memberExpression == null)
                throw new ArgumentException("Expression: " + expression.ToString());
            return memberExpression;
        }
        internal static MemberInfo GetMember(LambdaExpression selector) {
            return GetMemberExpression(selector).Member;
        }
        static Expression CheckMemberType(MemberExpression accessor) {
            return accessor.Type.IsValueType ? (Expression)Expression.Convert(accessor, typeof(object)) : accessor;
        }
        internal static Expression<Func<object>> Accessor(Type sourceType, MemberInfo sourceProperty) {
            var source = Expression.Parameter(sourceType, "source");
            var accessor = Expression.MakeMemberAccess(source, sourceProperty);
            return Expression.Lambda<Func<object>>(CheckMemberType(accessor), source);
        }
        internal static Expression<Func<T, object>> Accessor<T>(Type sourceType, string sourceProperty) {
            return Accessor<T>(sourceType, sourceType.GetProperty(sourceProperty));
        }
        internal static Expression<Func<T, object>> Accessor<T>(Type sourceType, MemberInfo sourceProperty) {
            var s = Expression.Parameter(typeof(T), "s");
            var accessor = Expression.MakeMemberAccess(Expression.Convert(s, sourceType), sourceProperty);
            return Expression.Lambda<Func<T, object>>(CheckMemberType(accessor), s);
        }
        internal static Expression<Func<object, object>> Box<TSource>(Expression<Func<TSource, object>> memberExpression) {
            var source = Expression.Parameter(typeof(object), "source");
            var accessor = Expression.MakeMemberAccess(
                Expression.Convert(source, typeof(TSource)), GetMember(memberExpression));
            return Expression.Lambda<Func<object, object>>(CheckMemberType(accessor), source);
        }
        internal static Expression<Func<object, object>> Box<TSource, TValue>(Expression<Func<TSource, TValue>> memberExpression) {
            var source = Expression.Parameter(typeof(object), "source");
            var accessor = Expression.MakeMemberAccess(
                Expression.Convert(source, typeof(TSource)), GetMember(memberExpression));
            return Expression.Lambda<Func<object, object>>(CheckMemberType(accessor), source);
        }
        internal static Expression<Action> Reduce<TViewModel>(Expression<Action<TViewModel>> commandSelector, TViewModel viewModel) {
            return (commandSelector != null) ? new ReduceVisitor(commandSelector, GetInstanceExpression<TViewModel>(viewModel)).ReduceToAction() : null;
        }
        internal static Expression<Action<T>> Reduce<TViewModel, T>(Expression<Action<TViewModel, T>> commandSelector, TViewModel viewModel) {
            return (commandSelector != null) ? new ReduceVisitor<T>(commandSelector, GetInstanceExpression<TViewModel>(viewModel)).ReduceToAction() : null;
        }
        internal static Func<T> ReduceAndCompile<TViewModel, T>(Expression<Func<TViewModel, T>> commandParameterSelector, TViewModel viewModel) {
            return (commandParameterSelector != null) ? new ReduceVisitor<T>(commandParameterSelector, GetInstanceExpression<TViewModel>(viewModel)).ReduceToFunc().Compile() : null;
        }
        static UnaryExpression GetInstanceExpression<TViewModel>(TViewModel viewModel) {
            return Expression.Convert(Expression.Constant(viewModel, typeof(TViewModel)), typeof(TViewModel));
        }
        #region Visitors
        class ReduceVisitor : ExpressionVisitor {
            protected LambdaExpression root;
            Expression expression;
            public ReduceVisitor(LambdaExpression root, Expression target) {
                this.root = root;
                this.expression = target;
            }
            protected override Expression VisitParameter(ParameterExpression p) {
                return (p == root.Parameters[0]) ? expression : base.VisitParameter(p);
            }
            internal Expression<Action> ReduceToAction() {
                return Expression.Lambda<Action>(Visit(root.Body));
            }
        }
        class ReduceVisitor<T> : ReduceVisitor {
            public ReduceVisitor(LambdaExpression root, Expression target)
                : base(root, target) {
            }
            internal Expression<Func<T>> ReduceToFunc() {
                return Expression.Lambda<Func<T>>(Visit(root.Body));
            }
            internal new Expression<Action<T>> ReduceToAction() {
                return Expression.Lambda<Action<T>>(Visit(root.Body), root.Parameters[1]);
            }
        }
        #endregion Visitors
    }
}