using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Comman.Dapper.Linq.Extension.Entites;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    public partial interface IQuerySet<T>
    {
        /// <summary>
        ///     指定查詢條件。
        /// </summary>
        /// <param name="predicate">查詢條件表達式。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        ///     指定查詢條件。
        /// </summary>
        /// <typeparam name="TWhere">條件表達式的型別。</typeparam>
        /// <param name="predicate">查詢條件表達式。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Where<TWhere>(Expression<Func<TWhere, bool>> predicate);

        /// <summary>
        ///     使用動態樹結構指定查詢條件。
        /// </summary>
        /// <param name="dynamicTree">動態樹結構條件。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Where(Dictionary<string, DynamicTree> dynamicTree);

        /// <summary>
        ///     使用 SQL 語句指定查詢條件。
        /// </summary>
        /// <param name="sqlWhere">SQL 查詢條件。</param>
        /// <param name="param">參數。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Where(string sqlWhere, object param = null);

        /// <summary>
        ///     根據條件決定是否應用查詢條件。
        /// </summary>
        /// <param name="where">條件判斷。</param>
        /// <param name="truePredicate">當條件為真時應用的查詢條件。</param>
        /// <param name="falsePredicate">當條件為假時應用的查詢條件。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> WhereIf(bool where, Expression<Func<T, bool>> truePredicate,
            Expression<Func<T, bool>> falsePredicate);

        /// <summary>
        ///     根據條件決定是否應用查詢條件。
        /// </summary>
        /// <typeparam name="TWhere">條件表達式的型別。</typeparam>
        /// <param name="where">條件判斷。</param>
        /// <param name="truePredicate">當條件為真時應用的查詢條件。</param>
        /// <param name="falsePredicate">當條件為假時應用的查詢條件。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> WhereIf<TWhere>(bool where, Expression<Func<TWhere, bool>> truePredicate,
            Expression<Func<TWhere, bool>> falsePredicate);
    }
}
