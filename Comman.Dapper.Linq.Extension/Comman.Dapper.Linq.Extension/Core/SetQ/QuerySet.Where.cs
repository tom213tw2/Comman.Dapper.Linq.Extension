using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Comman.Dapper.Linq.Extension.Entites;

namespace Comman.Dapper.Linq.Extension.Core.SetQ
{
    /// <summary>
    ///     條件篩選。
    /// </summary>
    /// <typeparam name="T">實體類型。</typeparam>
    public partial class QuerySet<T> : Aggregation<T>, Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T>
    {
        /// <summary>
        ///     添加條件表達式。
        /// </summary>
        /// <param name="predicate">條件表達式。</param>
        /// <returns>查詢集合接口。</returns>
        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> Where(Expression<Func<T, bool>> predicate)
        {
            WhereExpressionList.Add(predicate);
            return this;
        }

        /// <summary>
        ///     添加條件表達式。
        /// </summary>
        /// <typeparam name="TWhere">條件表達式類型。</typeparam>
        /// <param name="predicate">條件表達式。</param>
        /// <returns>查詢集合接口。</returns>
        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> Where<TWhere>(Expression<Func<TWhere, bool>> predicate)
        {
            WhereExpressionList.Add(predicate);
            return this;
        }

        /// <summary>
        ///     使用動態樹狀結構進行查詢（轉換成表達式樹集合），注意，int參數不會判斷為0的值。
        /// </summary>
        /// <param name="dynamicTree">動態樹狀結構。</param>
        /// <returns>查詢集合接口。</returns>
        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> Where(Dictionary<string, DynamicTree> dynamicTree)
        {
            WhereExpressionList.AddRange(SqlProvider.FormatDynamicTreeWhereExpression<T>(dynamicTree));
            return this;
        }

        /// <summary>
        ///     使用 SQL 查詢條件。
        /// </summary>
        /// <param name="sqlWhere">SQL 查詢條件。</param>
        /// <param name="param">參數。</param>
        /// <returns>查詢集合接口。</returns>
        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> Where(string sqlWhere, object param = null)
        {
            WhereBuilder.Append(" AND " + sqlWhere);
            if (param != null) Params.AddDynamicParams(param, true);
            return this;
        }

        /// <summary>
        ///     根據前置條件進行 Where 判斷。
        /// </summary>
        /// <param name="where">前置條件。</param>
        /// <param name="truePredicate">滿足條件時的表達式。</param>
        /// <param name="falsePredicate">不滿足條件時的表達式。</param>
        /// <returns>查詢集合接口。</returns>
        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> WhereIf(bool where, Expression<Func<T, bool>> truePredicate,
            Expression<Func<T, bool>> falsePredicate)
        {
            WhereExpressionList.Add(where ? truePredicate : falsePredicate);
            return this;
        }

        /// <summary>
        ///     根據前置條件進行 Where 判斷。
        /// </summary>
        /// <typeparam name="TWhere">條件表達式類型。</typeparam>
        /// <param name="where">前置條件。</param>
        /// <param name="truePredicate">滿足條件時的表達式。</param>
        /// <param name="falsePredicate">不滿足條件時的表達式。</param>
        /// <returns>查詢集合接口。</returns>
        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> WhereIf<TWhere>(bool where, Expression<Func<TWhere, bool>> truePredicate,
            Expression<Func<TWhere, bool>> falsePredicate)
        {
            WhereExpressionList.Add(where ? truePredicate : falsePredicate);
            return this;
        }

        /// <summary>
        ///     添加雙表條件表達式。
        /// </summary>
        /// <typeparam name="TWhere1">第一個表的類型。</typeparam>
        /// <typeparam name="TWhere2">第二個表的類型。</typeparam>
        /// <param name="exp">條件表達式。</param>
        /// <returns>查詢集合接口。</returns>
        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> Where<TWhere1, TWhere2>(Expression<Func<TWhere1, TWhere2, bool>> exp)
        {
            WhereExpressionList.Add(exp);
            return this;
        }
    }
}
