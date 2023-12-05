using System;
using System.Linq.Expressions;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    /// <summary>
    /// 定義一個查詢集合接口，用於實現從 T 到 TReturn 的查詢。
    /// </summary>
    /// <typeparam name="T">源類型。</typeparam>
    /// <typeparam name="TReturn">返回類型。</typeparam>
    public interface IQuerySet<T, TReturn> : IQuerySet<T>
    {
        /// <summary>
        /// 定義一個選擇操作，用於將 T 轉換為 TReturn。
        /// </summary>
        /// <param name="select">選擇表達式，描述如何從 T 轉換為 TReturn。</param>
        /// <returns>返回一個 IQuery 實例，它包含從 T 到 TReturn 的轉換邏輯。</returns>
        IQuery<T, TReturn> Select(Expression<Func<T, TReturn>> select);
    }
}