using System;
using System.Linq.Expressions;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    public partial interface IQuerySet<T>
    {
        /// <summary>
        ///     按指定字段進行順序排序。
        /// </summary>
        /// <param name="field">排序依據的字段。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> OrderBy(Expression<Func<T, object>> field);

        /// <summary>
        ///     按指定字段進行倒序排序。
        /// </summary>
        /// <param name="field">排序依據的字段。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> OrderByDescending(Expression<Func<T, object>> field);

        /// <summary>
        ///     按指定泛型字段進行順序排序。
        /// </summary>
        /// <typeparam name="TProperty">泛型字段類型。</typeparam>
        /// <param name="field">排序依據的字段。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> OrderBy<TProperty>(Expression<Func<TProperty, object>> field);

        /// <summary>
        ///     按指定泛型字段進行倒序排序。
        /// </summary>
        /// <typeparam name="TProperty">泛型字段類型。</typeparam>
        /// <param name="field">排序依據的字段。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> OrderByDescending<TProperty>(Expression<Func<TProperty, object>> field);

        /// <summary>
        ///     使用字符串表達式進行排序。
        /// </summary>
        /// <param name="orderBy">排序的字符串表達式。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> OrderBy(string orderBy);
    }
}