﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace LComplete.Framework.Common
{
    /// <summary>
    /// 反射工具类
    /// </summary>
    public sealed class ReflectUtils
    {
        public static string[] GetPropertyNames(Type type)
        {
            PropertyInfo[] propertyInfos = type.GetProperties();
            return propertyInfos.Select(property => property.Name).ToArray();
        }

        public static Object GetPropertyValue(Object entity, string propertyName)
        {
            PropertyInfo property = entity.GetType().GetProperty(propertyName);
            return property.GetValue(entity, null);
        }

        /// <summary>
        /// 获取类型上含有指定attribute的属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo SearchPropertyInfoFromAttribute<TAttribute>(Type type) where TAttribute : Attribute
        {
            foreach (var propertyInfo in type.GetProperties())
            {
                object[] attributes = propertyInfo.GetCustomAttributes(typeof(TAttribute), true);
                if (attributes.Length == 1)
                    return propertyInfo;
            }
            return null;
        }

        public static void SetPropertyValue(object entity, string propertyName, object value)
        {
            Type type = entity.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
            propertyInfo.SetValue(entity, value, null);
        }

        public static string GetDescription(Enum enumValue)
        {
            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());
            return GetFieldDescription(fi);
        }



        public static string GetFieldDescription(FieldInfo fi)
        {
            if (fi == null)
                return string.Empty;

            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return string.Empty;
        }
    }
}
