using DevExpress.Mvvm.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;

namespace DevExpress.Mvvm.Native {
    [SecuritySafeCritical]
    public static class DataAnnotationsAttributeHelper {
        internal static bool HasRequiredAttribute(MemberInfo member) {
            return MetadataHelper.GetAttribute<RequiredAttribute>(member) != null;
        }
        internal static Type GetMetadataClassType(Type componentType) {
            Type metadataTypeAttributeType = componentType.IsEnum ? typeof(EnumMetadataTypeAttribute) : typeof(MetadataTypeAttribute);
            object[] metadataTypeAttributes = componentType.GetCustomAttributes(metadataTypeAttributeType, false);
            if(metadataTypeAttributes != null && metadataTypeAttributes.Any()) {
                return (Type)metadataTypeAttributes[0].GetType().GetProperty("MetadataClassType", BindingFlags.Instance | BindingFlags.Public).GetValue(metadataTypeAttributes[0], null);
            }
            return null;
        }
        #region scaffolding
#if !SILVERLIGHT
        internal static Type GetScaffoldColumnAttributeType() {
            return typeof(ScaffoldColumnAttribute);
        }
        internal static LambdaExpression GetScaffoldColumnAttributeCreateExpression() {
            Expression<Func<ScaffoldColumnAttribute>> expression = () => new ScaffoldColumnAttribute(default(bool));
            return expression;
        }
        internal static IEnumerable<object> GetScaffoldColumnAttributeConstructorParameters(Attribute attribute) {
            return new object[] { ((ScaffoldColumnAttribute)attribute).Scaffold };
        }

        internal static TBuilder DoNotScaffoldCore<TBuilder>(TBuilder builder) where TBuilder : IAttributeBuilderInternal<TBuilder> {
            return builder.AddOrReplaceAttribute(new ScaffoldColumnAttribute(false));
        }
#endif
        #endregion

        internal static Type GetDisplayAttributeType() {
            return typeof(DisplayAttribute);
        }
        internal static LambdaExpression GetDisplayAttributeCreateExpression() {
            Expression<Func<DisplayAttribute>> expression = () => new DisplayAttribute();
            return expression;
        }
        internal static IEnumerable<Tuple<PropertyInfo, object>> GetDisplayAttributePropertyValuePairs(Attribute attributeBase) {
            DisplayAttribute attribute = (DisplayAttribute)attributeBase;
            List<Tuple<PropertyInfo, object>> result = new List<Tuple<PropertyInfo, object>>();
            if(attribute.GetOrder() != null)
                result.Add(GetPropertyValuePair(attribute, x => x.Order));
            if(attribute.GetAutoGenerateField() != null)
                result.Add(GetPropertyValuePair(attribute, x => x.AutoGenerateField));
            result.Add(GetPropertyValuePair(attribute, x => x.Name));
            result.Add(GetPropertyValuePair(attribute, x => x.ShortName));
            result.Add(GetPropertyValuePair(attribute, x => x.Description));
            return result;
        }
        internal static Tuple<PropertyInfo, object> GetPropertyValuePair<TAttribute, TProperty>(TAttribute attribute, Expression<Func<TAttribute, TProperty>> propertyExpression) {
            PropertyInfo property = ExpressionHelper.GetArgumentPropertyStrict(propertyExpression);
            return new Tuple<PropertyInfo, object>(property, property.GetValue(attribute, null));
        }
        internal static TBuilder DisplayNameCore<TBuilder>(TBuilder builder, string name) where TBuilder : IAttributeBuilderInternal<TBuilder> {
            return builder.AddOrModifyAttribute<DisplayAttribute>(x => x.Name = name);
        }
        internal static TBuilder DisplayShortNameCore<TBuilder>(TBuilder builder, string shortName) where TBuilder : IAttributeBuilderInternal<TBuilder> {
            return builder.AddOrModifyAttribute<DisplayAttribute>(x => x.ShortName = shortName);
        }
        internal static TBuilder DescriptionCore<TBuilder>(TBuilder builder, string description) where TBuilder : IAttributeBuilderInternal<TBuilder> {
            return builder.AddOrModifyAttribute<DisplayAttribute>(x => x.Description = description);
        }
        internal static TBuilder NotAutoGeneratedCore<TBuilder>(TBuilder builder) where TBuilder : IAttributeBuilderInternal<TBuilder> {
            return builder.AddOrModifyAttribute<DisplayAttribute>(x => x.AutoGenerateField = false);
        }
        public static bool GetAutoGenerateField(FieldInfo field) {
            return GetFieldDisplayAttribute(field).Return(x => x.GetAutoGenerateField().GetValueOrDefault(true), () => true);
        }
        public static string GetFieldDisplayName(FieldInfo field) {
            return GetFieldDisplayAttribute(field).With(x => x.GetName() ?? x.GetShortName());
        }
        public static string GetFieldDescription(FieldInfo field) {
            return GetFieldDisplayAttribute(field).With(x => x.GetDescription());
        }
        static DisplayAttribute GetFieldDisplayAttribute(FieldInfo field) {
            return MetadataHelper.GetAttribute<DisplayAttribute>(field);
        }
        internal static TBuilder SetDataTypeCore<TBuilder>(TBuilder builder, PropertyDataType dataType) where TBuilder : IAttributeBuilderInternal<TBuilder> {
            return builder.AddOrReplaceAttribute(new DataTypeAttribute(ToDataType(dataType)));
        }
        #region data type conversion
        static DataType ToDataType(PropertyDataType dataType) {
            switch(dataType) {
                case PropertyDataType.Currency:
                    return DataType.Currency;
                case PropertyDataType.Password:
                    return DataType.Password;
                case PropertyDataType.MultilineText:
                    return DataType.MultilineText;
                case PropertyDataType.PhoneNumber:
                    return DataType.PhoneNumber;
                case PropertyDataType.ImageUrl:
                    return DataType.ImageUrl;
                case PropertyDataType.Time:
                    return DataType.Time;
                case PropertyDataType.DateTime:
                    return DataType.DateTime;
                case PropertyDataType.Date:
                    return DataType.Date;
                default:
                    return DataType.Custom;
            }
        }
        #endregion
    }
}