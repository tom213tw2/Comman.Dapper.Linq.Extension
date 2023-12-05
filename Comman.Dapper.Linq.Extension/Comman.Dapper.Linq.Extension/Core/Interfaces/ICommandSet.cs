using System;
using System.Linq.Expressions;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    /// <summary>
    /// 定義針對類型 <typeparamref name="T"/> 的命令集操作接口。
    /// </summary>
    public interface ICommandSet<T> : ICommand<T>
    {
        /// <summary>
        /// 添加基於表達式的條件。
        /// </summary>
        /// <param name="predicate">條件表達式。</param>
        /// <returns>命令集介面。</returns>
        ICommandSet<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 添加基於 SQL 的條件。
        /// </summary>
        /// <param name="sqlWhere">SQL 條件字串。</param>
        /// <param name="param">SQL 條件參數。</param>
        /// <returns>命令集介面。</returns>
        ICommandSet<T> Where(string sqlWhere, object param = null);

        /// <summary>
        /// 根據條件添加不同的條件表達式。
        /// </summary>
        /// <param name="condition">決定使用哪個表達式的條件。</param>
        /// <param name="truePredicate">條件為 true 時使用的表達式。</param>
        /// <param name="falsePredicate">條件為 false 時使用的表達式。</param>
        /// <returns>命令集介面。</returns>
        ICommandSet<T> WhereIf(bool condition, Expression<Func<T, bool>> truePredicate, Expression<Func<T, bool>> falsePredicate);

        /// <summary>
        /// 重設表名。
        /// </summary>
        /// <param name="type">實體類型。</param>
        /// <param name="tableName">新的表名。</param>
        /// <returns>命令集介面。</returns>
        ICommandSet<T> ResetTableName(Type type, string tableName);
    }
}