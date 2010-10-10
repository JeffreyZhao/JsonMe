using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonMe
{
    public class JsonContract<T>
    {
        private Dictionary<PropertyInfo, IJsonProperty> m_properties = new Dictionary<PropertyInfo, IJsonProperty>();

        public JsonSimpleProperty SimpleProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpr)
        {
            var propertyInfo = (PropertyInfo)(propertyExpr.Body as MemberExpression).Member;
            var property = new JsonSimpleProperty(propertyInfo);
            this.m_properties.Add(propertyInfo, property);

            return property;
        }

        public JsonComplexProperty<TProperty> ComplexProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpr)
            where TProperty : class, new()
        {
            var propertyInfo = (PropertyInfo)(propertyExpr.Body as MemberExpression).Member;
            var property = new JsonComplexProperty<TProperty>(propertyInfo);
            this.m_properties.Add(propertyInfo, property);

            return property;
        }

        public JsonArrayProperty<TElement> ArrayProperty<TElement>(Expression<Func<T, IEnumerable<TElement>>> propertyExpr)
            where TElement : class, new()
        {
            var propertyInfo = (PropertyInfo)(propertyExpr.Body as MemberExpression).Member;
            var property = new JsonArrayProperty<TElement>(propertyInfo);
            this.m_properties.Add(propertyInfo, property);

            return property;
        }

        internal IEnumerable<IJsonProperty> Properties
        {
            get
            {
                return this.m_properties.Values;
            }
        }
    }
}
