using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Comman.Dapper.Linq.Extension.Attributes;
using Comman.Dapper.Linq.Extension.Dapper;
using Comman.Dapper.Linq.Extension.Entites;
using Comman.Dapper.Linq.Extension.Expressions;
using Comman.Dapper.Linq.Extension.Extension;
using Comman.Dapper.Linq.Extension.Helper.Cache;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    /// <summary>
    /// 提供對表達式解析的抽象類別。
    /// 負責處理 SQL 語句的組合和格式化。
    /// </summary>
    public abstract class IResolveExpression
    {
        protected SqlProvider provider;
        protected IProviderOption providerOption;
        protected AbstractSet abstractSet => provider.Context.Set;

        public IResolveExpression(SqlProvider provider)
        {
            this.provider = provider;
            this.providerOption = provider.ProviderOption;
        }

        /// <summary>
        /// 用於緩存字段列表映射的靜態哈希表。
        /// </summary>
        private static Hashtable _tableFieldMap = new Hashtable();

        /// <summary>
        /// 根據反射對象獲取表字段，用於構建 SQL 查詢。
        /// </summary>
        /// <param name="entityObject">反射對象。</param>
        /// <returns>返回表字段的 SQL 字串。</returns>
        public virtual string GetTableField(EntityObject entityObject)
        {
            lock (_tableFieldMap)
            {
                string fieldBuild = (string)_tableFieldMap[entityObject];
                if (fieldBuild == null)
                {
                    var propertyInfos = entityObject.Properties;
                    string asName = entityObject.Name == entityObject.AsName
                        ? providerOption.CombineFieldName(entityObject.AsName)
                        : entityObject.AsName;
                    fieldBuild = string.Join(",",
                        entityObject.FieldPairs.Select(field =>
                            $"{asName}.{providerOption.CombineFieldName(field.Value)}"));
                    _tableFieldMap.Add(entityObject, fieldBuild);
                }

                return fieldBuild;
            }
        }

        /// <summary>
        /// 解析 SELECT 查詢字段。
        /// </summary>
        /// <param name="topNum">限制返回的頂部記錄數。</param>
        /// <returns>返回 SQL SELECT 字串。</returns>
        public abstract string ResolveSelect(int? topNum);


        /// <summary>
        /// 解析查詢條件（WHERE 子句）。
        /// 此方法將 LINQ 表達式轉換為 SQL WHERE 條件，並結合額外的自定義 SQL 條件。
        /// </summary>
        /// <param name="prefix">用於區分多個條件的前綴。</param>
        /// <returns>返回組合後的 SQL WHERE 條件字串。</returns>
        public virtual string ResolveWhereList(string prefix = null)
        {
            // 添加 LINQ 生成的 SQL 條件和參數
            List<LambdaExpression> lambdaExpressionList = abstractSet.WhereExpressionList;
           
            var aaa = new EntityObject(abstractSet.TableType);
           var list= GetTableField(aaa).Split(',').Select(s => s.Split('.')[0].Replace("[","").Replace("]","")+"."+ s.Split('.')[1]).ToList();
            
            StringBuilder builder = new StringBuilder("WHERE 1=1 ");
          
            for (int i = 0; i < lambdaExpressionList.Count; i++)
            {
                string wherePrefix = string.IsNullOrEmpty(prefix) ? $"{i}" : $"{prefix}{(i)}_";
                var whereParam = new WhereExpression(lambdaExpressionList[i], wherePrefix, provider);      
                
                builder.Append(whereParam.SqlCmd);
                // 處理參數
                foreach (var paramKey in whereParam.Param.ParameterNames)
                {
                    var  txt = whereParam.SqlCmd;

                   for(int j=0;j< list.Count(); j++)
                    {
                        if (IsFieldAssociatedWithParam(txt, list[j], $"@{paramKey}"))
                        {
                            var config = aaa.EntityFieldList[j];
                            int? size = 0;
                            if (config.Length == 0)
                                size = null;
                            else
                                size = config.Length;

                            abstractSet.Params.Add(paramKey, whereParam.Param.Get<object>(paramKey), config.DbType, size: size);
                        }
                    }
                }
            }

            // 添加自定義 SQL 生成的條件和參數
            if (abstractSet.WhereBuilder != null && abstractSet.WhereBuilder.Length != 0)
            {

                // 添加自定義條件 SQL
                builder.Append(abstractSet.WhereBuilder);
            }

            return builder.ToString();
        }

        public virtual string ResolveCommandWhereList(string prefix = null)
        {
            // 添加 LINQ 生成的 SQL 條件和參數
            List<LambdaExpression> lambdaExpressionList = abstractSet.WhereExpressionList;
           
            var entityFields = new EntityObject(abstractSet.TableType).EntityFieldList;
           
            
            StringBuilder builder = new StringBuilder("WHERE 1=1 ");
          
            for (int i = 0; i < lambdaExpressionList.Count; i++)
            {
                string wherePrefix = string.IsNullOrEmpty(prefix) ? $"{i}" : $"{prefix}{(i)}_";
                var whereParam = new WhereExpression(lambdaExpressionList[i], wherePrefix, provider);      
                
                builder.Append(whereParam.SqlCmd);
                // 處理參數
                foreach (var paramKey in whereParam.Param.ParameterNames)
                {
                    var sqlCmd = whereParam.SqlCmd;
                    var entityFieldList = entityFields.Where(s =>
                        IsFieldAssociatedWithParam(sqlCmd, $"[{s.FieldName}]", $"@{paramKey}"));
                   if (entityFieldList.Any())
                   {
                       var entityField = entityFieldList.Single();
                       int? size = 0;
                       if (entityField.Length == 0)
                           size = null;
                       else
                           size = entityField.Length;

                       abstractSet.Params.Add(paramKey, whereParam.Param.Get<object>(paramKey), entityField.DbType, size: size);
                   }
                }
            }

            // 添加自定義 SQL 生成的條件和參數
            if (abstractSet.WhereBuilder != null && abstractSet.WhereBuilder.Length != 0)
            {

                // 添加自定義條件 SQL
                builder.Append(abstractSet.WhereBuilder);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 解析 GROUP BY 子句。
        /// 此方法基於提供的表達式列表，生成 SQL GROUP BY 子句。
        /// </summary>
        /// <returns>返回 SQL GROUP BY 子句字串。</returns>
        public virtual string ResolveGroupBy()
        {
            StringBuilder builder = new StringBuilder();
            var groupExpression = abstractSet.GroupExpressionList;
            if (groupExpression != null && groupExpression.Any())
            {
                for (int i = 0; i < groupExpression.Count; i++)
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
        /// 解析分組聚合條件（HAVING 子句）。
        /// 用於根據指定的表達式列表生成 SQL HAVING 條件，進行分組後的數據過濾。
        /// </summary>
        /// <returns>返回 SQL HAVING 條件字串。</returns>
        public virtual string ResolveHaving()
        {
            StringBuilder builder = new StringBuilder();
            var havingExpression = abstractSet.HavingExpressionList;
            if (havingExpression != null && havingExpression.Any())
            {
                for (int i = 0; i < havingExpression.Count; i++)
                {
                    var whereParam = new WhereExpression(havingExpression[i], $"Having_{i}", provider);
                    builder.Append(whereParam.SqlCmd);
                    // 處理參數
                    foreach (var paramKey in whereParam.Param.ParameterNames)
                    {
                        abstractSet.Params.Add(paramKey, whereParam.Param.Get<object>(paramKey));
                    }
                }

                builder.Insert(0, " Having 1=1 ");
            }

            return builder.ToString();
        }

        /// <summary>
        /// 解析排序條件（ORDER BY 子句）。
        /// 根據提供的表達式列表生成 SQL ORDER BY 條件，用於指定數據的排序方式。
        /// </summary>
        /// <returns>返回 SQL ORDER BY 條件字串。</returns>
        public virtual string ResolveOrderBy()
        {
            var orderByList = abstractSet?.OrderbyExpressionList.Select(a =>
            {
                var entity = EntityCache.QueryEntity(a.Key.Type.GenericTypeArguments[0]);
                var columnName = entity.FieldPairs[a.Key.Body.GetCorrectPropertyName()];
                string orderBySql =
                    $"{entity.AsName}.{providerOption.CombineFieldName(columnName)}{(a.Value == EOrderBy.Asc ? " ASC " : " DESC ")}";
                return orderBySql;
            }) ?? new List<string>();
            if (!orderByList.Any() && (abstractSet.OrderbyBuilder == null || abstractSet.OrderbyBuilder.Length == 0))
                return "";

            return $"ORDER BY {string.Join(",", orderByList)} {abstractSet.OrderbyBuilder}";
        }

        /// <summary>
        /// 解析查詢總和（SUM 函數）。
        /// 根據提供的選擇器表達式生成 SQL SUM 聚合函數查詢。
        /// </summary>
        /// <param name="selector">指定要求和的字段。</param>
        /// <returns>返回 SQL SUM 聚合函數查詢字串。</returns>
        public abstract string ResolveSum(LambdaExpression selector);


        /// <summary>
        /// 解析查詢最大值（MAX 函數）。
        /// 此方法根據提供的選擇器表達式生成 SQL MAX 聚合函數查詢，用於獲取某個字段的最大值。
        /// </summary>
        /// <param name="selector">指定要查詢最大值的字段的表達式。</param>
        /// <returns>返回 SQL MAX 聚合函數查詢字串。</returns>
        public abstract string ResolveMax(LambdaExpression selector);

        /// <summary>
        /// 解析查詢最小值（MIN 函數）。
        /// 此方法根據提供的選擇器表達式生成 SQL MIN 聚合函數查詢，用於獲取某個字段的最小值。
        /// </summary>
        /// <param name="selector">指定要查詢最小值的字段的表達式。</param>
        /// <returns>返回 SQL MIN 聚合函數查詢字串。</returns>
        public abstract string ResolveMin(LambdaExpression selector);

        /// <summary>
        /// 解析更新表達式。
        /// 此方法根據提供的更新表達式生成 SQL 更新語句，用於更新數據庫中的記錄。
        /// </summary>
        /// <typeparam name="T">要更新的實體類型。</typeparam>
        /// <param name="updateExpression">表示更新操作的表達式。</param>
        /// <returns>返回一個表示更新操作的 UpdateExpression 物件。</returns>
        public virtual UpdateExpression<T> ResolveUpdate<T>(Expression<Func<T, T>> updateExpression)
        {
            return new UpdateExpression<T>(updateExpression, provider);
        }


        /// <summary>
        /// 解析更新語句。
        /// 此方法根據提供的實體和參數生成 SQL 更新語句，用於更新數據庫中的紀錄。可以指定排除特定字段。
        /// </summary>
        /// <typeparam name="T">要更新的實體類型。</typeparam>
        /// <param name="entity">要更新的實體物件。</param>
        /// <param name="param">用於更新操作的參數集合。</param>
        /// <param name="excludeFields">需要排除的字段名稱列表。</param>
        /// <returns>返回 SQL 更新語句字串。</returns>
        public virtual string ResolveUpdate<T>(T entity, DynamicParameters param, string[] excludeFields)
        {
            var entityObject = EntityCache.QueryEntity(typeof(T));
            StringBuilder builder = new StringBuilder();
            foreach (var entityField in entityObject.EntityFieldList)
            {
                string name = entityField.FieldName;
                // 檢查是否為排除字段
                if (excludeFields != null &&
                    (excludeFields.Contains(entityField.PropertyInfo.Name) || excludeFields.Contains(name)))
                {
                    continue;
                }

                // 檢查字段是否為自增（自增字段在更新時通常不包含）
                if (entityField.IsIncrease)
                {
                    continue;
                }

                object value = entityField.PropertyInfo.GetValue(entity);
                if (builder.Length != 0)
                {
                    builder.Append(",");
                }

                int? size = 0;
                if (entityField.Length == 0)
                    size = null;
                else
                    size = entityField.Length;
                string paramName = $"{providerOption.ParameterPrefix}U_{name}_{param.ParameterNames.Count()}";
                builder.Append($"{providerOption.CombineFieldName(name)}={paramName}");
                param.Add($"{paramName}", value,entityField.DbType,size:size);
            }

            builder.Insert(0, " SET ");
            return builder.ToString();
        }


        /// <summary>
        /// 批量修改。
        /// 此方法用於根據提供的實體集合生成 SQL 批量更新語句。可以指定排除特定字段。
        /// </summary>
        /// <typeparam name="T">要批量更新的實體類型。</typeparam>
        /// <param name="entites">要更新的實體集合。</param>
        /// <param name="param">用於更新操作的參數集合。</param>
        /// <param name="excludeFields">需要排除的字段名稱列表。</param>
        /// <returns>返回 SQL 批量更新語句字串。</returns>
        public virtual string ResolveBulkUpdate<T>(IEnumerable<T> entites, DynamicParameters param,
            string[] excludeFields)
        {
            var entityObject = EntityCache.QueryEntity(typeof(T));
            StringBuilder builder = new StringBuilder();
            foreach (var entityField in entityObject.EntityFieldList)
            {
                string name = entityField.FieldName;
                // 檢查是否為排除字段
                if (excludeFields != null &&
                    (excludeFields.Contains(entityField.PropertyInfo.Name) || excludeFields.Contains(name)))
                {
                    continue;
                }

                // 檢查字段是否為自增（自增字段在更新時通常不包含）
                if (entityField.IsIncrease)
                {
                    continue;
                }

                if (builder.Length != 0)
                {
                    builder.Append(",");
                }

                string paramName = $"{providerOption.ParameterPrefix}{name}";
                builder.Append($"{providerOption.CombineFieldName(name)}={paramName}");
                param.Add(paramName);
            }

            builder.Insert(0, " SET ");
            return builder.ToString();
        }

        /// <summary>
        /// 根據傳入的布爾值來解析 SQL NOLOCK 語句。
        /// 當 nolock 為 true 時，返回 "(NOLOCK)" 字串，否則返回空字串。
        /// </summary>
        /// <param name="nolock">指示是否需要 NOLOCK 關鍵字。</param>
        /// <returns>返回 "(NOLOCK)" 或空字串。</returns>
        public virtual string ResolveWithNoLock(bool nolock)
        {
            return nolock ? "(NOLOCK)" : "";
        }


        /// <summary>
        /// 解析連表查詢。
        /// 此方法用於根據提供的連表關係列表生成 SQL 連表查詢語句。
        /// 它支持根據實體關係或自定義 SQL 語句進行連表。
        /// </summary>
        /// <param name="joinAssTables">連表關係的列表。</param>
        /// <param name="sql">參考的 SQL 查詢字串，此方法可能會修改它。</param>
        /// <returns>返回生成的 SQL 連表查詢語句。</returns>
        public virtual string ResolveJoinSql(List<JoinAssTable> joinAssTables, ref string sql)
        {
            StringBuilder builder = new StringBuilder(Environment.NewLine);
            if (joinAssTables.Count != 0)
            {
                sql = sql.TrimEnd();
                // 循環拼接連表對象
                for (int i = 0; i < joinAssTables.Count; i++)
                {
                    // 當前連表對象
                    var item = joinAssTables[i];
                    if (item.IsMapperField == false)
                    {
                        continue;
                    }
                   
                    item.MapperList.Clear();
                    if (item.TableType != null)
                    {
                        // 連表實體
                        EntityObject leftEntity = EntityCache.QueryEntity(item.TableType);
                        // 根據連表動作類型決定連表方式
                        if (item.Action == JoinAction.Default || item.Action == JoinAction.Navigation)
                        {
                            string leftTable = providerOption.CombineFieldName(item.LeftTabName);
                            builder.Append($@" {item.JoinMode} JOIN 
                                       {leftTable} {item.NoLockString} {leftEntity.AsName} ON {leftEntity.AsName}
                                      .{providerOption.CombineFieldName(item.LeftAssName)} = {providerOption.CombineFieldName(item.RightTabName)}
                                      .{providerOption.CombineFieldName(item.RightAssName)} " + Environment.NewLine);
                        }
                        else // SQL 連表
                        {
                            builder.Append(" " + item.JoinSql);
                            // 判斷是否需要顯示連表的字段
                            if (!item.IsMapperField)
                            {
                                continue;
                            }
                        }

                        // 自定義返回
                        if (provider.Context.Set.SelectExpression != null)
                        {
                            continue;
                        }

                        FieldDetailWith(ref sql, item, leftEntity);
                    }
                    else
                    {
                        if (item.Action == JoinAction.Sql)
                        {
                            builder.Append(" " + item.JoinSql);
                            // 判斷是否需要顯示連表的字段
                            if (!item.IsMapperField)
                            {
                                continue;
                            }
                        }
                    }
                }
            }

            return builder.ToString();
        }


        /// <summary>
        /// 字段處理。
        /// 此方法用於處理連表查詢中的字段，包括字段的選擇和重命名，以避免在連表查詢中產生字段名衝突。
        /// </summary>
        /// <param name="masterSql">主 SQL 查詢字串，此方法會修改它來加入連表查詢的字段。</param>
        /// <param name="joinAssTable">連表關係對象。</param>
        /// <param name="joinEntity">連表實體對象。</param>
        /// <returns>修改後的 SQL 查詢字串。</returns>
        private string FieldDetailWith(ref string masterSql, JoinAssTable joinAssTable, EntityObject joinEntity)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            // 表名稱
            string joinTableName = joinEntity.AsName == joinEntity.Name
                ? providerOption.CombineFieldName(joinEntity.Name)
                : joinEntity.AsName;
            // 查詢的字段
            var fieldPairs = joinAssTable.SelectFieldPairs != null && joinAssTable.SelectFieldPairs.Any()
                ? joinAssTable.SelectFieldPairs
                : joinEntity.FieldPairs;
            foreach (string fieldValue in fieldPairs.Values)
            {
                if (masterSql.LastIndexOf(',') == masterSql.Length - 1 && sqlBuilder.Length == 0)
                    sqlBuilder.Append($"{joinTableName}.");
                else
                    // 首先添加表名稱
                    sqlBuilder.Append($",{joinTableName}.");
                // 字段
                string field = providerOption.CombineFieldName(fieldValue);
                // 字符出現的次數
                int repeatCount = masterSql.Split(new string[] { field }, StringSplitOptions.None).Length - 1;
                // 添加字段
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

            // 設置 SQL 字段
            masterSql += sqlBuilder;
            return masterSql;
        }


        /// <summary>
        /// 解析批量新增。
        /// 此方法用於根據提供的實體集合生成 SQL 批量插入語句。可以指定排除特定字段。
        /// </summary>
        /// <typeparam name="T">要插入的實體類型。</typeparam>
        /// <param name="entitys">要插入的實體集合。</param>
        /// <param name="excludeFields">需要排除的字段名稱列表。</param>
        /// <returns>返回 SQL 批量插入語句字串。</returns>
        public virtual string ResolveBulkInsert<T>(IEnumerable<T> entitys, string[] excludeFields)
        {
            var sqlBuilder = new StringBuilder();
            DynamicParameters parameters = new DynamicParameters();
            // 當前數據索引
            int index = 0;
            foreach (var item in entitys)
            {
                var resolveInsertParamsAndValues = ResolveInsertParamsAndValues(item, excludeFields, index++);
                var tableName = resolveInsertParamsAndValues.Item1;
                var fieldStr = resolveInsertParamsAndValues.Item2;
                var paramStr = resolveInsertParamsAndValues.Item3;
                var parameter = resolveInsertParamsAndValues.Item4;

                // 增加字段（只加一次）
                if (sqlBuilder.Length == 0)
                {
                    sqlBuilder.Append($"INSERT INTO {tableName}");
                    sqlBuilder.Append($"({fieldStr})");
                }

                // 增加參數
                if (index == 1)
                    sqlBuilder.Append($"Values({paramStr})");
                else
                    sqlBuilder.Append($",({paramStr})");
                parameters.AddDynamicParams(parameter);
            }

            provider.Params.AddDynamicParams(parameters);
            return sqlBuilder.ToString();
        }


        /// <summary>
        /// 解析插入參數和值。
        /// 此方法用於解析給定實體的字段和對應的參數，生成插入語句所需的部分。可以指定排除特定字段。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <param name="tValue">要插入的實體物件。</param>
        /// <param name="excludeFields">需要排除的字段名稱列表。</param>
        /// <param name="index">當前數據索引，用於參數命名。</param>
        /// <returns>返回一個包含表名、字段字串、參數字串和參數集合的 Tuple。</returns>
        protected Tuple<string, StringBuilder, StringBuilder, DynamicParameters> ResolveInsertParamsAndValues<T>(
            T tValue, string[] excludeFields = null, int index = 0)
        {
            var fieldBuilder = new StringBuilder(64);
            var paramBuilder = new StringBuilder(64);
            DynamicParameters parameters = new DynamicParameters();

            var entityProperties = EntityCache.QueryEntity(typeof(T));
            var tableName = provider.FormatTableName(false, false); // entityProperties.Name;
            var isAppend = false;

            int? size=0;
            foreach (EntityObject.EntityField entityField in entityProperties.EntityFieldList)
            {
                string fieldName = entityField.FieldName;
                // 檢查是否為排除字段
                if (excludeFields != null && (excludeFields.Contains(fieldName) ||
                                              excludeFields.Contains(entityField.PropertyInfo.Name)))
                {
                    continue;
                }

                // 檢查字段是否為自增（自增字段在插入時通常不包含）
                if (entityField.IsIncrease)
                {
                    continue;
                }

                if (isAppend)
                {
                    fieldBuilder.Append(",");
                    paramBuilder.Append(",");
                }

                if (entityField.Length==0)
                {
                    size = null;
                }
                else
                {
                    size = entityField.Length;
                }
                // 字段添加
                fieldBuilder.Append($"{provider.ProviderOption.CombineFieldName(fieldName)}");
                // 參數添加
                paramBuilder.Append($"{provider.ProviderOption.ParameterPrefix}{fieldName}{index}");
                parameters.Add($"{provider.ProviderOption.ParameterPrefix}{fieldName}{index}",
                    entityField.PropertyInfo.GetValue(tValue),entityField.DbType,size:size);

                isAppend = true;
            }

            return new Tuple<string, StringBuilder, StringBuilder, DynamicParameters>(tableName, fieldBuilder,
                paramBuilder, parameters);
        }

        private bool IsFieldAssociatedWithParam(string query, string fieldName, string paramName)
        {
            string pattern = $@"{Regex.Escape(fieldName)}\s*=\s*{Regex.Escape(paramName)}";
            Match match = Regex.Match(query, pattern);

            return match.Success;
        }


    }
}