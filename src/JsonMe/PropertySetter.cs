using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace JsonMe
{
    internal class PropertySetter
    {
        private struct CacheKey
        {
            public PropertyInfo PropertyInfo;
            public Type PropertyValueType;

            public override bool Equals(object obj)
            {
                var that = (CacheKey)obj;
                return this.PropertyInfo == that.PropertyInfo && this.PropertyValueType == that.PropertyValueType;
            }

            public override int GetHashCode()
            {
                var hashCode = this.PropertyInfo.GetHashCode();
                if (this.PropertyValueType != null)
                {
                    hashCode ^= this.PropertyValueType.GetHashCode();
                }

                return hashCode;
            }
        }

        private class SetterCache : ConcurrentCache<CacheKey, Action<object, object>>
        {
            protected override Action<object, object> Create(CacheKey key)
            {
                return Create(key.PropertyInfo, key.PropertyValueType);
            }

            private static Action<object, object> Create(PropertyInfo propertyInfo, Type propertyValueType)
            {
                var setterInfo = propertyInfo.GetSetMethod();
                var entityExpr = Expression.Parameter(typeof(object), "entity");
                var propertyValueExpr = Expression.Parameter(typeof(object), "propertyValue");
                var strongTypedEntityExpr = Expression.Convert(entityExpr, propertyInfo.DeclaringType);

                MethodCallExpression callExpr;

                if (propertyValueType == null)
                {
                    var nullExpr = Expression.Constant(null, propertyInfo.PropertyType);
                    callExpr = Expression.Call(strongTypedEntityExpr, setterInfo, nullExpr);
                }
                else
                {
                    UnaryExpression strongTypedPropertyValueExpr = null;

                    var mediateType = propertyValueType;
                    while (mediateType != null)
                    {
                        try
                        {
                            strongTypedPropertyValueExpr = Expression.Convert(propertyValueExpr, mediateType);
                            if (propertyValueType != propertyInfo.PropertyType)
                            {
                                strongTypedPropertyValueExpr = Expression.Convert(strongTypedPropertyValueExpr, propertyInfo.PropertyType);
                            }

                            break;
                        }
                        catch
                        {
                            mediateType = mediateType.BaseType;
                            strongTypedPropertyValueExpr = null;
                        }
                    }

                    if (strongTypedEntityExpr == null) return null;

                    callExpr = Expression.Call(strongTypedEntityExpr, setterInfo, strongTypedPropertyValueExpr);
                }

                var lambdaExpr = Expression.Lambda<Action<object, object>>(callExpr, entityExpr, propertyValueExpr);
                return lambdaExpr.Compile();
            }
        }

        private static SetterCache s_setterCache = new SetterCache();

        public static void Set(object entity, PropertyInfo propertyInfo, object propertyValue)        {
            var cacheKey = new CacheKey
            {
                PropertyInfo = propertyInfo,
                PropertyValueType = propertyValue == null ? null : propertyValue.GetType()
            };
            
            var setter = s_setterCache.Get(cacheKey);
            if (setter == null)
            {
                throw new ConversionException(propertyInfo, propertyValue, null);
            }

            try
            {
                setter(entity, propertyValue);
            }
            catch (Exception ex)
            {
                throw new MappingException("Error occurred when setting " + propertyInfo, ex);
            }
        }   }
}
