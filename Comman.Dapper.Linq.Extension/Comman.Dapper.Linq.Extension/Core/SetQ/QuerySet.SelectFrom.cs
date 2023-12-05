using System;
using System.Linq.Expressions;
using Comman.Dapper.Linq.Extension.Core.Interfaces;
using Comman.Dapper.Linq.Extension.Extension.From;

namespace Comman.Dapper.Linq.Extension.Core.SetQ
{
    /// <summary>
    ///     多表查詢擴展。
    /// </summary>
    /// <typeparam name="T">主表類型。</typeparam>
    public partial class QuerySet<T>
    {
        /// <summary>
        ///     選擇指定類型的欄位。
        /// </summary>
        /// <typeparam name="TReturn">返回類型。</typeparam>
        /// <param name="select">選擇表達式。</param>
        /// <returns>查詢接口。</returns>
        public IQuery<T, TReturn> Select<TReturn>(Expression<Func<T, TReturn>> select)
        {
            var query = new Query<T, TReturn>(DbCon, SqlProvider, DbTransaction);
            query.SqlProvider.Context.Set.SelectExpression = select;
            return query;
        }

        /// <summary>
        ///     進行兩表連接查詢。
        /// </summary>
        /// <typeparam name="T1">第一個連接表類型。</typeparam>
        /// <typeparam name="T2">第二個連接表類型。</typeparam>
        /// <returns>連接查詢接口。</returns>
        public ISelectFrom<T, T1, T2> From<T1, T2>()
        {
            return new ISelectFrom<T, T1, T2>(this);
        }

        /// <summary>
        ///     進行三表連接查詢。
        /// </summary>
        /// <typeparam name="T1">第一個連接表類型。</typeparam>
        /// <typeparam name="T2">第二個連接表類型。</typeparam>
        /// <typeparam name="T3">第三個連接表類型。</typeparam>
        /// <returns>連接查詢接口。</returns>
        public ISelectFrom<T, T1, T2, T3> From<T1, T2, T3>()
        {
            return new ISelectFrom<T, T1, T2, T3>(this);
        }

        /// <summary>
        ///     進行四表連接查詢。
        /// </summary>
        /// <typeparam name="T1">第一個連接表類型。</typeparam>
        /// <typeparam name="T2">第二個連接表類型。</typeparam>
        /// <typeparam name="T3">第三個連接表類型。</typeparam>
        /// <typeparam name="T4">第四個連接表類型。</typeparam>
        /// <returns>連接查詢接口。</returns>
        public ISelectFrom<T, T1, T2, T3, T4> From<T1, T2, T3, T4>()
        {
            return new ISelectFrom<T, T1, T2, T3, T4>(this);
        }
    }
}
