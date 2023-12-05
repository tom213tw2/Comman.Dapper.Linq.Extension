using System;
using System.Linq.Expressions;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    /// <summary>
    /// 定義對於類型 <typeparamref name="T"/> 的聚合操作。
    /// </summary>
    public interface IAggregation<T>
    {
        /// <summary>
        /// 獲得行數。
        /// </summary>
        /// <returns>行數。</returns>
        int Count();

        /// <summary>
        /// 計算總和。
        /// </summary>
        /// <typeparam name="TResult">結果類型。</typeparam>
        /// <param name="sumExpression">指定要計算總和的表達式。</param>
        /// <returns>總和結果。</returns>
        TResult Sum<TResult>(Expression<Func<T, TResult>> sumExpression);

        /// <summary>
        /// 獲得最小值。
        /// </summary>
        /// <typeparam name="TResult">結果類型。</typeparam>
        /// <param name="minExpression">指定要獲得最小值的表達式。</param>
        /// <returns>最小值結果。</returns>
        TResult Min<TResult>(Expression<Func<T, TResult>> minExpression);

        /// <summary>
        /// 獲得最大值。
        /// </summary>
        /// <typeparam name="TResult">結果類型。</typeparam>
        /// <param name="maxExpression">指定要獲得最大值的表達式。</param>
        /// <returns>最大值結果。</returns>
        TResult Max<TResult>(Expression<Func<T, TResult>> maxExpression);
    }
}