using System;
using System.Linq.Expressions;
using Comman.Dapper.Linq.Extension.Extension.From;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    /// <summary>
    /// 多表查詢擴展介面。
    /// 提供用於多表查詢的方法和功能。
    /// </summary>
    public partial interface IQuerySet<T>
    {
        /// <summary>
        /// 定義一個選擇查詢。
        /// </summary>
        /// <typeparam name="TReturn">選擇查詢的返回類型。</typeparam>
        /// <param name="select">選擇查詢的表達式。</param>
        /// <returns>返回一個包含選擇查詢結果的查詢對象。</returns>
        IQuery<T, TReturn> Select<TReturn>(Expression<Func<T, TReturn>> select);

        /// <summary>
        /// 定義從兩個表進行查詢的方法。
        /// </summary>
        /// <typeparam name="T1">第一個表的類型。</typeparam>
        /// <typeparam name="T2">第二個表的類型。</typeparam>
        /// <returns>返回一個從兩個表進行查詢的查詢對象。</returns>
        ISelectFrom<T, T1, T2> From<T1, T2>();

        /// <summary>
        /// 定義從三個表進行查詢的方法。
        /// </summary>
        /// <typeparam name="T1">第一個表的類型。</typeparam>
        /// <typeparam name="T2">第二個表的類型。</typeparam>
        /// <typeparam name="T3">第三個表的類型。</typeparam>
        /// <returns>返回一個從三個表進行查詢的查詢對象。</returns>
        ISelectFrom<T, T1, T2, T3> From<T1, T2, T3>();

        /// <summary>
        /// 定義從四個表進行查詢的方法。
        /// </summary>
        /// <typeparam name="T1">第一個表的類型。</typeparam>
        /// <typeparam name="T2">第二個表的類型。</typeparam>
        /// <typeparam name="T3">第三個表的類型。</typeparam>
        /// <typeparam name="T4">第四個表的類型。</typeparam>
        /// <returns>返回一個從四個表進行查詢的查詢對象。</returns>
        ISelectFrom<T, T1, T2, T3, T4> From<T1, T2, T3, T4>();
    }
}