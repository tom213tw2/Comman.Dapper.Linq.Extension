﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Comman.Dapper.Linq.Extension.Attributes;
using Comman.Dapper.Linq.Extension.Exception;
using Comman.Dapper.Linq.Extension.Helper.Cache;

namespace Comman.Dapper.Linq.Extension.Extension
{
    public static class ReflectExtension
    {
        public static PropertyInfo GetKeyPropertity(this object obj)
        {
            var properties = EntityCache.QueryEntity(obj.GetType()).Properties
                .Where(a => a.GetCustomAttribute<Identity>() != null).ToArray();

            if (!properties.Any())
                throw new DapperExtensionException($"the {nameof(obj)} entity with no KeyAttribute Propertity");

            if (properties.Length > 1)
                throw new DapperExtensionException(
                    $"the {nameof(obj)} entity with greater than one KeyAttribute Propertity");

            return properties.First();
        }

        public static PropertyInfo GetKeyPropertity(this Type typeInfo)
        {
            var properties = EntityCache.QueryEntity(typeInfo).Properties
                .Where(a => a.GetCustomAttribute<Identity>() != null).ToArray();

            if (!properties.Any())
                throw new DapperExtensionException(
                    $"the type {nameof(typeInfo.FullName)} entity with no KeyAttribute Propertity");

            if (properties.Length > 1)
                throw new DapperExtensionException(
                    $"the type {nameof(typeInfo.FullName)} entity with greater than one KeyAttribute Propertity");

            return properties.First();
        }

        /// <summary>
        ///     动态创建类
        /// </summary>
        /// <param name="namespaces">命名空间</param>
        /// <param name="fullName">类的完全限定名</param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static object CreateInstance(string namespaces, string fullName, object[] param)
        {
            var assembly = Assembly.Load(namespaces);
            return assembly.CreateInstance(fullName, false, BindingFlags.CreateInstance, null, param, null, null);
        }

        /// <summary>
        ///     获取匿名类型中字段的实际类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetAnonymousFieldType(this Type type)
        {
            if (type.FullName.Contains("System.Nullable"))
            {
                if (type.GenericTypeArguments.Count() != 0)
                    return type.GenericTypeArguments[0];
                return type;
            }

            return type;
        }

        /// <summary>
        ///     list 转dataset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entites"></param>
        /// <returns></returns>
        public static DataSet ToDataSet<T>(this IEnumerable<T> entites, string[] excludeFields = null)
        {
            var entityObject = EntityCache.QueryEntity(typeof(T));
            var dataSet = new DataSet();
            var dataTable = new DataTable(entityObject.Name);

            foreach (var field in entityObject.EntityFieldList)
            {
                //排除字段
                if (excludeFields != null && excludeFields.Contains(field.FieldName)) continue;
                dataTable.Columns.Add(field.FieldName);
            }

            foreach (var item in entites)
            {
                var dataRow = dataTable.NewRow();
                foreach (var field in entityObject.EntityFieldList)
                {
                    //排除字段
                    if (excludeFields != null && excludeFields.Contains(field.FieldName)) continue;
                    dataRow[field.FieldName] = field.PropertyInfo.GetValue(item);
                }

                dataTable.Rows.Add(dataRow);
            }

            dataSet.Tables.Add(dataTable);
            return dataSet;
        }

        /// <summary>
        ///     通过list改变dataset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSet"></param>
        /// <param name="entites"></param>
        /// <returns></returns>
        public static void UpdateDataSet<T>(this DataSet dataSet, IEnumerable<T> entites)
        {
            var entityObject = EntityCache.QueryEntity(typeof(T));
            var index = 0;
            var table = dataSet.Tables[0];
            foreach (var item in entites)
            {
                //防止db数据中途发生了变化
                if (index == table.Rows.Count)
                    break;
                var dataRow = table.Rows[index++];
                foreach (DataColumn column in table.Columns)
                {
                    var value = entityObject.EntityFieldList.FirstOrDefault(x => x.FieldName.Equals(column.ColumnName))
                        .PropertyInfo
                        .GetValue(item);
                    dataRow[column.ColumnName] = value;
                }
            }
        }
    }
}