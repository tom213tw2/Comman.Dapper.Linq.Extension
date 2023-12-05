using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Comman.Dapper.Linq.Extension.Entites;
using Comman.Dapper.Linq.Extension.Expressions;
using Comman.Dapper.Linq.Extension.Extension;
using Comman.Dapper.Linq.Extension.Helper.Cache;
using DynamicParameters = Comman.Dapper.Linq.Extension.Dapper.DynamicParameters;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    public abstract class ResolveExpression
    {
        /// <summary>
        ///     字段列映射表。
        /// </summary>
        private static readonly Hashtable TableFieldMap = new Hashtable();

        private readonly SqlProvider provider;
        private readonly ProviderOption providerOption;

        protected ResolveExpression(SqlProvider provider)
        {
            this.provider = provider;
            providerOption = provider.ProviderOption;
        }

        private AbstractSet AbstractSet => provider.Context.Set;


        /// <summary>
        ///     根據反射對象獲取表字段。
        /// </summary>
        /// <param name="entityObject">實體對象。</param>
        /// <returns>表字段字符串。</returns>
        public virtual string GetTableField(EntityObject entityObject)
        {
            lock (TableFieldMap)
            {
                var fieldBuild = (string)TableFieldMap[entityObject];
                if (fieldBuild != null) return fieldBuild;

                var asName = entityObject.Name == entityObject.AsName
                    ? providerOption.CombineFieldName(entityObject.AsName)
                    : entityObject.AsName;

                fieldBuild = string.Join(",",
                    entityObject.FieldPairs.Select(field =>
                        $"{asName}.{providerOption.CombineFieldName(field.Value)}"));

                TableFieldMap.Add(entityObject, fieldBuild);

                return fieldBuild;
            }
        }

        /// <summary>
        ///     解析查詢字段。
        /// </summary>
        /// <param name="topNum">查詢頂部數量限制。</param>
        /// <returns>查詢字段的 SQL 字串。</returns>
        public abstract string ResolveSelect(int? topNum);

        /// <summary>
        ///     解析查詢條件。
        /// </summary>
        /// <param name="prefix">查詢條件的前綴。</param>
        /// <returns>查詢條件的 SQL 字串。</returns>
        public virtual string ResolveWhereList(string prefix = null)
        {
            // 添加 Linq 生成的 SQL 條件和參數
            var lambdaExpressionList = AbstractSet.WhereExpressionList;
            var builder = new StringBuilder("WHERE 1=1 ");

            for (var i = 0; i < lambdaExpressionList.Count; i++)
            {
                var wherePrefix = string.IsNullOrEmpty(prefix) ? $"{i}" : $"{prefix}{i}_";
                var whereParam = new WhereExpression(lambdaExpressionList[i], wherePrefix, provider);
                builder.Append(whereParam.SqlCmd);

                // 添加參數
                foreach (var paramKey in whereParam.Param.ParameterNames)
                    AbstractSet.Params.Add(paramKey, whereParam.Param.Get<object>(paramKey));
            }

            // 添加自定義 SQL 生成的條件和參數
            if (AbstractSet.WhereBuilder != null && AbstractSet.WhereBuilder.Length != 0)
                builder.Append(AbstractSet.WhereBuilder);

            return builder.ToString();
        }


        /// <summary>
        ///     解析分組。
        /// </summary>
        /// <returns>分組的 SQL 字串。</returns>
        public virtual string ResolveGroupBy()
        {
            var builder = new StringBuilder();
            var groupExpression = AbstractSet.GroupExpressionList;
            if (groupExpression != null && groupExpression.Any())
            {
                for (var i = 0; i < groupExpression.Count; i++)
                {
                    var groupParam = new GroupExpression(groupExpression[i], $"Group_{i}", provider);
                    if (builder.Length != 0)
                        builder.Append(",");
                    builder.Append(groupParam.SqlCmd);
                }

                builder.Insert(0, " GROUP BY ");
            }

            return builder.ToString();
        }

        /// <summary>
        ///     解析分組聚合條件。
        /// </summary>
        /// <returns>聚合條件的 SQL 字串。</returns>
        public virtual string ResolveHaving()
        {
            var builder = new StringBuilder();
            var havingExpression = AbstractSet.HavingExpressionList;
            if (havingExpression != null && havingExpression.Any())
            {
                for (var i = 0; i < havingExpression.Count; i++)
                {
                    var whereParam = new WhereExpression(havingExpression[i], $"Having_{i}", provider);
                    builder.Append(whereParam.SqlCmd);
                    // 添加參數
                    foreach (var paramKey in whereParam.Param.ParameterNames)
                        AbstractSet.Params.Add(paramKey, whereParam.Param.Get<object>(paramKey));
                }

                builder.Insert(0, " Having 1=1 ");
            }

            return builder.ToString();
        }

        /// <summary>
        ///     解析排序。
        /// </summary>
        /// <returns>排序的 SQL 字串。</returns>
        public virtual string ResolveOrderBy()
        {
            var orderByList = AbstractSet?.OrderbyExpressionList.Select(a =>
            {
                var entity = EntityCache.QueryEntity(a.Key.Type.GenericTypeArguments[0]);
                var columnName = entity.FieldPairs[a.Key.Body.GetCorrectPropertyName()];
                var orderBySql =
                    $"{entity.AsName}.{providerOption.CombineFieldName(columnName)}{(a.Value == EOrderBy.Asc ? " ASC " : " DESC ")}";
                return orderBySql;
            }) ?? new List<string>();

            if (!orderByList.Any() && (AbstractSet?.OrderbyBuilder == null || AbstractSet.OrderbyBuilder.Length == 0))
                return "";

            return $"ORDER BY {string.Join(",", orderByList)} {AbstractSet.OrderbyBuilder}";
        }


        /// <summary>
        ///     解析查詢總和。
        /// </summary>
        /// <param name="selector">選擇器表達式。</param>
        /// <returns>總和查詢的 SQL 字串。</returns>
        public abstract string ResolveSum(LambdaExpression selector);

        /// <summary>
        ///     解析查詢最小值。
        /// </summary>
        /// <param name="selector">選擇器表達式。</param>
        /// <returns>最小值查詢的 SQL 字串。</returns>
        public abstract string ResolveMax(LambdaExpression selector);

        /// <summary>
        ///     解析查詢最大值。
        /// </summary>
        /// <param name="selector">選擇器表達式。</param>
        /// <returns>最大值查詢的 SQL 字串。</returns>
        public abstract string ResolveMin(LambdaExpression selector);

        /// <summary>
        ///     解析更新表達式。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <param name="updateExpression">更新表達式。</param>
        /// <returns>更新表達式物件。</returns>
        public virtual UpdateExpression<T> ResolveUpdate<T>(Expression<Func<T, T>> updateExpression)
        {
            return new UpdateExpression<T>(updateExpression, provider);
        }

        /// <summary>
        ///     解析更新語句。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <param name="entity">更新的實體對象。</param>
        /// <param name="param">參數集合。</param>
        /// <param name="excludeFields">排除的字段名稱列表。</param>
        /// <returns>更新語句的 SQL 字串。</returns>
        public virtual string ResolveUpdate<T>(T entity, DynamicParameters param, string[] excludeFields)
        {
            var entityObject = EntityCache.QueryEntity(typeof(T));
            var builder = new StringBuilder();
            foreach (var entityField in entityObject.EntityFieldList)
            {
                var name = entityField.FieldName;
                // 是否為排除字段
                if (excludeFields != null &&
                    (excludeFields.Contains(entityField.PropertyInfo.Name) || excludeFields.Contains(name))) continue;
                if (entityField.IsIncrease) continue;
                var value = entityField.PropertyInfo.GetValue(entity);
                if (builder.Length != 0) builder.Append(",");
                var paramName = $"{providerOption.ParameterPrefix}U_{name}_{param.ParameterNames.Count()}";
                builder.Append($"{providerOption.CombineFieldName(name)}={paramName}");
                param.Add($"{paramName}", value);
            }

            builder.Insert(0, " SET ");
            return builder.ToString();
        }


        /// <summary>
        ///     批量修改。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <param name="entities">實體集合。</param>
        /// <param name="param">參數集合。</param>
        /// <param name="excludeFields">排除的字段名稱列表。</param>
        /// <returns>批量修改的 SQL 字串。</returns>
        public virtual string ResolveBulkUpdate<T>(IEnumerable<T> entities, DynamicParameters param,
            string[] excludeFields)
        {
            var entityObject = EntityCache.QueryEntity(typeof(T));
            var builder = new StringBuilder();
            foreach (var entityField in entityObject.EntityFieldList)
            {
                var name = entityField.FieldName;
                // 是否為排除字段
                if (excludeFields != null &&
                    (excludeFields.Contains(entityField.PropertyInfo.Name) || excludeFields.Contains(name))) continue;
                if (entityField.IsIncrease) continue;
                if (builder.Length != 0) builder.Append(",");
                var paramName = $"{providerOption.ParameterPrefix}{name}";
                builder.Append($"{providerOption.CombineFieldName(name)}={paramName}");
                param.Add(paramName);
            }

            builder.Insert(0, " SET ");
            return builder.ToString();
        }

        /// <summary>
        ///     解析無鎖查詢設定。
        /// </summary>
        /// <param name="nolock">是否無鎖查詢。</param>
        /// <returns>無鎖查詢設定的 SQL 字串。</returns>
        public virtual string ResolveWithNoLock(bool nolock)
        {
            return nolock ? "(NOLOCK)" : "";
        }

        /// <summary>
        ///     解析連表查詢。
        /// </summary>
        /// <param name="joinAssTables">連表輔助表列表。</param>
        /// <param name="sql">原始 SQL 字串。</param>
        /// <returns>連表查詢的 SQL 字串。</returns>
        public virtual string ResolveJoinSql(List<JoinAssTable> joinAssTables, ref string sql)
        {
            var builder = new StringBuilder(Environment.NewLine);
            if (joinAssTables.Count != 0)
            {
                sql = sql.TrimEnd();
                // 循環拼接連表對象
                for (var i = 0; i < joinAssTables.Count; i++)
                {
                    var item = joinAssTables[i];
                    if (item.IsMapperField == false) continue;
                    item.MapperList.Clear();
                    if (item.TableType != null)
                    {
                        var leftEntity = EntityCache.QueryEntity(item.TableType);
                        if (item.Action == JoinAction.Default || item.Action == JoinAction.Navigation)
                        {
                            var leftTable = providerOption.CombineFieldName(item.LeftTabName);
                            builder.Append($@" {item.JoinMode} JOIN 
                                       {leftTable} {leftEntity.AsName} ON {leftEntity.AsName}
                                      .{providerOption.CombineFieldName(item.LeftAssName)} = {providerOption.CombineFieldName(item.RightTabName)}
                                      .{providerOption.CombineFieldName(item.RightAssName)} " + Environment.NewLine);
                        }
                        else
                        {
                            builder.Append(" " + item.JoinSql);
                            if (!item.IsMapperField) continue;
                        }

                        if (provider.Context.Set.SelectExpression != null) continue;
                        FieldDetailWith(ref sql, item, leftEntity);
                    }
                    else
                    {
                        if (item.Action != JoinAction.Sql) continue;
                        builder.Append(" " + item.JoinSql);
                        if (!item.IsMapperField) continue;
                    }
                }
            }

            return builder.ToString();
        }


        /// <summary>
        ///     字段處理。
        /// </summary>
        /// <param name="masterSql">主 SQL 字串。</param>
        /// <param name="joinAssTable">連表輔助表。</param>
        /// <param name="joinEntity">連接實體。</param>
        /// <returns>處理後的 SQL 字串。</returns>
        private string FieldDetailWith(ref string masterSql, JoinAssTable joinAssTable, EntityObject joinEntity)
        {
            var sqlBuilder = new StringBuilder();
            // 表名稱
            var joinTableName = joinEntity.AsName == joinEntity.Name
                ? providerOption.CombineFieldName(joinEntity.Name)
                : joinEntity.AsName;
            // 查詢的字段
            var fieldPairs = joinAssTable.SelectFieldPairs != null && joinAssTable.SelectFieldPairs.Any()
                ? joinAssTable.SelectFieldPairs
                : joinEntity.FieldPairs;
            foreach (var fieldValue in fieldPairs.Values)
            {
                if (masterSql.LastIndexOf(',') == masterSql.Length - 1 && sqlBuilder.Length == 0)
                    sqlBuilder.Append($"{joinTableName}.");
                else
                    sqlBuilder.Append($",{joinTableName}.");
                var field = providerOption.CombineFieldName(fieldValue);
                var repeatCount = masterSql.Split(new[] { field }, StringSplitOptions.None).Length - 1;
                sqlBuilder.Append(field);
                if (repeatCount > 0)
                {
                    sqlBuilder.Append($" AS {fieldValue}_{repeatCount}");
                    joinAssTable.MapperList.Add(fieldValue, $"{fieldValue}_{repeatCount}");
                }
                else
                {
                    joinAssTable.MapperList.Add(fieldValue, fieldValue);
                }
            }

            masterSql += sqlBuilder;
            return masterSql;
        }

        /// <summary>
        ///     解析批量新增。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <param name="entities">實體集合。</param>
        /// <param name="excludeFields">排除字段列表。</param>
        /// <returns>批量新增的 SQL 字串。</returns>
        public virtual string ResolveBulkInsert<T>(IEnumerable<T> entities, string[] excludeFields)
        {
            var sqlBuilder = new StringBuilder();
            var parameters = new DynamicParameters();
            var index = 0;
            foreach (var item in entities)
            {
                var resolveInsertParamsAndValues = ResolveInsertParamsAndValues(item, excludeFields, index++);
                var tableName = resolveInsertParamsAndValues.Item1;
                var fieldStr = resolveInsertParamsAndValues.Item2;
                var paramStr = resolveInsertParamsAndValues.Item3;
                var parameter = resolveInsertParamsAndValues.Item4;

                if (sqlBuilder.Length == 0)
                {
                    sqlBuilder.Append($"INSERT INTO {tableName}({fieldStr})");
                }

                sqlBuilder.Append(index == 1 ? $"Values({paramStr})" : $",({paramStr})");
                parameters.AddDynamicParams(parameter);
            }

            provider.Params.AddDynamicParams(parameters);
            return sqlBuilder.ToString();
        }


        /// <summary>
        /// 解析插入操作的參數和值。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <param name="tValue">實體對象。</param>
        /// <param name="excludeFields">需要排除的字段列表。</param>
        /// <param name="index">當前數據索引。</param>
        /// <returns>返回包含表名、字段構建器、參數構建器和參數的元組。</returns>
        private Tuple<string, StringBuilder, StringBuilder, DynamicParameters> ResolveInsertParamsAndValues<T>(
            T tValue, string[] excludeFields = null, int index = 0)
        {
            var fieldBuilder = new StringBuilder(64);
            var paramBuilder = new StringBuilder(64);
            var parameters = new DynamicParameters();

            var entityProperties = EntityCache.QueryEntity(typeof(T));
            var tableName = provider.FormatTableName(false, false); // entityProperties.Name;
            var isAppend = false;

            foreach (var entityField in entityProperties.EntityFieldList)
            {
                var fieldName = entityField.FieldName;
                // 檢查是否為排除字段
                if (excludeFields != null && (excludeFields.Contains(fieldName) ||
                                              excludeFields.Contains(entityField.PropertyInfo.Name))) continue;
                // 檢查是否為自增字段
                if (entityField.IsIncrease) continue;
                if (isAppend)
                {
                    fieldBuilder.Append(",");
                    paramBuilder.Append(",");
                }

                // 添加字段
                fieldBuilder.Append($"{provider.ProviderOption.CombineFieldName(fieldName)}");
                // 添加參數
                paramBuilder.Append($"{provider.ProviderOption.ParameterPrefix}{fieldName}{index}");
                parameters.Add($"{provider.ProviderOption.ParameterPrefix}{fieldName}{index}",
                    entityField.PropertyInfo.GetValue(tValue));

                isAppend = true;
            }

            return new Tuple<string, StringBuilder, StringBuilder, DynamicParameters>(tableName, fieldBuilder,
                paramBuilder, parameters);
        }
    }
}