using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Comman.Dapper.Linq.Extension.Core.Interfaces;
using Comman.Dapper.Linq.Extension.Entites;
using Comman.Dapper.Linq.Extension.Exception;
using Comman.Dapper.Linq.Extension.Expressions;
using Comman.Dapper.Linq.Extension.Helper;
using Comman.Dapper.Linq.Extension.Helper.Cache;
using DynamicParameters = Comman.Dapper.Linq.Extension.Dapper.DynamicParameters;

namespace Comman.Dapper.Linq.Extension
{
    /// <summary>
    /// 提供 SQL 查詢和命令生成的抽象基類。
    /// </summary>
    public abstract class SqlProvider
    {
        protected SqlProvider()
        {
            Params = new DynamicParameters();
            JoinList = new List<JoinAssTable>();
            AsTableNameDic = new Dictionary<Type, string>();
        }

        /// <summary>
        /// 數據庫上下文。
        /// </summary>
        public AbstractDataBaseContext Context { get; set; }

        /// <summary>
        /// 數據庫提供者選項。
        /// </summary>
        public abstract ProviderOption ProviderOption { get; set; }

        /// <summary>
        /// 表達式解析器。
        /// </summary>
        public abstract ResolveExpression ResolveExpression { get; set; }

        /// <summary>
        /// 生成的 SQL 字串。
        /// </summary>
        public string SqlString { get; set; }

        /// <summary>
        /// 連接對象集合。
        /// </summary>
        public List<JoinAssTable> JoinList { get; set; }

        /// <summary>
        /// 參數對象。
        /// </summary>
        public DynamicParameters Params { get; set; }

        /// <summary>
        /// 重命名目錄。
        /// </summary>
        public Dictionary<Type, string> AsTableNameDic { get; set; }

        /// <summary>
        /// 是否排除在工作單位外。
        /// </summary>
        public bool IsExcludeUnitOfWork { get; set; }

        /// <summary>
        /// 條件是否需要加上 as 名稱（一般修改和刪除時不需要）。
        /// </summary>
        public bool IsAppendAsName { get; set; } = true;

        public abstract SqlProvider FormatGet<T>();
        public abstract SqlProvider FormatToList<T>();
        public abstract SqlProvider FormatToPageList<T>(int pageIndex, int pageSize);
        public abstract SqlProvider FormatCount();
        public abstract SqlProvider FormatDelete();
        public abstract SqlProvider FormatInsert<T>(T entity, string[] excludeFields);
        public abstract SqlProvider FormatInsert<T>(IEnumerable<T> entitys, string[] excludeFields);
        public abstract SqlProvider FormatInsertIdentity<T>(T entity, string[] excludeFields);
        public abstract SqlProvider FormatUpdate<T>(Expression<Func<T, T>> updateExpression);
        public abstract SqlProvider FormatUpdate<T>(T entity, string[] excludeFields);
        public abstract SqlProvider FormatUpdate<T>(IEnumerable<T> entity, string[] excludeFields);
        public abstract SqlProvider FormatSum(LambdaExpression sumExpression);
        public abstract SqlProvider FormatMin(LambdaExpression MinExpression);
        public abstract SqlProvider FormatMax(LambdaExpression MaxExpression);
        public abstract SqlProvider FormatUpdateSelect<T>(Expression<Func<T, T>> updator);
        public abstract SqlProvider Create();

        public virtual SqlProvider Clear()
        {
            Params.Clear();
            ProviderOption.MappingList.Clear();
            ProviderOption.NavigationList.Clear();
            return this;
        }

        /// <summary>
        /// 獲取表名稱。
        /// </summary>
        /// <param name="isNeedFrom">是否需要加上 FROM 語句。</param>
        /// <param name="isAsName">是否需要使用別名。</param>
        /// <param name="tableType">連接查詢時會用到的表類型。</param>
        /// <returns>格式化後的表名稱字串。</returns>
        public virtual string FormatTableName(bool isNeedFrom = true, bool isAsName = true, Type tableType = null)
        {
            // 實體解析類型
            var entity = EntityCache.QueryEntity(tableType ?? Context.Set.TableType);
            var schema = string.IsNullOrEmpty(entity.Schema)
                ? ""
                : ProviderOption.CombineFieldName(entity.Schema) + ".";
            var fromName = entity.Name;
            // 函數 AsTableName 優先級大於一切
            if (AsTableNameDic.TryGetValue(entity.Type, out var asTableName))
                fromName = asTableName;
            // 是否存在實體特性中的 AsName 標記
            if (isAsName)
            {
                fromName = entity.AsName.Equals(fromName)
                    ? ProviderOption.CombineFieldName(fromName)
                    : $"{ProviderOption.CombineFieldName(fromName)} {entity.AsName}";
            }
            else
            {
                fromName = ProviderOption.CombineFieldName(fromName);
            }

            var sqlString = $" {schema}{fromName} ";
            // 是否需要 FROM
            if (isNeedFrom)
                sqlString = $" FROM {sqlString}";
            return sqlString;
        }

        /// <summary>
        /// 獲取資料庫上下文。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <returns>資料庫上下文實例。</returns>
        protected DataBaseContext<T> DataBaseContext<T>()
        {
            return (DataBaseContext<T>)Context;
        }

        /// <summary>
        /// 根據主鍵獲取條件。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <param name="entity">實體對象。</param>
        /// <param name="param">動態參數。</param>
        /// <returns>構造的 WHERE 條件字串。</returns>
        public virtual string GetIdentityWhere<T>(T entity, DynamicParameters param = null)
        {
            var entityObject = EntityCache.QueryEntity(typeof(T));
            if (string.IsNullOrEmpty(entityObject.Identitys))
                throw new DapperExtensionException("主鍵不存在!請前往實體類使用 [Identity] 特性設置主鍵。");

            // 設置參數
            if (param == null)
                return $" AND {entityObject.Identitys}={ProviderOption.ParameterPrefix}{entityObject.Identitys} ";
            // 獲取主鍵數據
            var id = entityObject.EntityFieldList
                .FirstOrDefault(x => x.FieldName == entityObject.Identitys)
                ?.PropertyInfo
                .GetValue(entity);
            param.Add(entityObject.Identitys, id);

            return $" AND {entityObject.Identitys}={ProviderOption.ParameterPrefix}{entityObject.Identitys} ";
        }


        /// <summary>
        /// 根據主鍵獲取查詢條件。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <param name="id">主鍵的值。</param>
        /// <param name="param">動態參數，用於添加查詢條件。</param>
        /// <returns>構造的 WHERE 條件字串。</returns>
        public virtual string GetIdentityWhere<T>(object id, DynamicParameters param = null)
        {
            var entityObject = EntityCache.QueryEntity(typeof(T));
            if (string.IsNullOrEmpty(entityObject.Identitys))
                throw new DapperExtensionException("主鍵不存在!請前往實體類使用 [Identity] 特性設置主鍵。");

            //设置参数
            if (param != null) param.Add(entityObject.Identitys, id);
            return $" AND {entityObject.Identitys}={ProviderOption.ParameterPrefix}{entityObject.Identitys} ";
        }

        /// <summary>
        /// 自定義條件生成表達式。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <param name="dynamicTree">動態樹字典，包含用於構造查詢條件的資訊。</param>
        /// <returns>包含構造好的 Lambda 表達式的集合。</returns>
        public virtual IEnumerable<LambdaExpression> FormatDynamicTreeWhereExpression<T>(
            Dictionary<string, DynamicTree> dynamicTree)
        {
            foreach (var key in dynamicTree.Keys)
            {
                var tree = dynamicTree[key];
                if (tree != null && !string.IsNullOrEmpty(tree.Value))
                {
                    var tableType = typeof(T);
                    if (!string.IsNullOrEmpty(tree.Table)) tableType = EntityCache.QueryEntity(tree.Table).Type;
                    //如果不存在對應表就使用預設表
                    var param = Expression.Parameter(tableType, "param");
                    object value = tree.Value;
                    if (value == null)
                    {
                        continue;
                    }

                    if (tree.ValueType == DbType.DateTime)
                    {
                        value = Convert.ToDateTime(value);
                    }
                    else if (tree.ValueType == DbType.String)
                    {
                        value = Convert.ToString(value);
                        if ("" == value.ToString()) continue;
                    }
                    else if (tree.ValueType == DbType.Int32)
                    {
                        var number = Convert.ToInt32(value);
                        value = number;
                        if (0 == number) continue;
                    }
                    else if (tree.ValueType == DbType.Boolean)
                    {
                        if (value.ToString() == "") continue;
                        value = Convert.ToBoolean(value);
                    }

                    Expression whereExpress = null;
                    switch (tree.Operators)
                    {
                        case ExpressionType.Equal: //等于
                            whereExpress = Expression.Equal(Expression.Property(param, tree.Field),
                                Expression.Constant(value));
                            break;
                        case ExpressionType.GreaterThanOrEqual: //大于等于
                            whereExpress = Expression.GreaterThanOrEqual(Expression.Property(param, tree.Field),
                                Expression.Constant(value));
                            break;
                        case ExpressionType.LessThanOrEqual: //小于等于
                            whereExpress = Expression.LessThanOrEqual(Expression.Property(param, tree.Field),
                                Expression.Constant(value));
                            break;
                        case ExpressionType.Call: //模糊查询
                            var method = typeof(string).GetMethodss().FirstOrDefault(x => x.Name.Equals("Contains"));
                            whereExpress = Expression.Call(Expression.Property(param, tree.Field), method,
                                Expression.Constant(value));
                            break;
                        default:
                            whereExpress = Expression.Equal(Expression.Property(param, tree.Field),
                                Expression.Constant(value));
                            break;
                    }

                    var lambdaExp = Expression.Lambda(TrimExpression.Trim(whereExpress), param);
                    //WhereExpressionList.Add();
                    yield return lambdaExp;
                }
            }
        }
    }
}