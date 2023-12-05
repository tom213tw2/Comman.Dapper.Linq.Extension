using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    /// <summary>
    /// 定義針對類型 <typeparamref name="T"/> 的命令操作接口。
    /// </summary>
    public interface ICommand<T>
    {
        /// <summary>
        /// 更新實體。
        /// </summary>
        /// <param name="entity">要更新的實體。</param>
        /// <param name="excludeFields">排除的字段。</param>
        /// <param name="timeout">超時時間（秒）。</param>
        /// <returns>影響的行數。</returns>
        int Update(T entity, string[] excludeFields = null, int timeout = 120);

        /// <summary>
        /// 批量更新實體。
        /// </summary>
        /// <param name="entities">要更新的實體集合。</param>
        /// <param name="adapter">數據庫適配器。</param>
        /// <param name="excludeFields">排除的字段。</param>
        /// <param name="timeout">超時時間（秒）。</param>
        /// <returns>影響的行數。</returns>
        int Update(IEnumerable<T> entities, IDbDataAdapter adapter, string[] excludeFields = null, int timeout = 120);

        /// <summary>
        /// 根據表達式更新實體。
        /// </summary>
        /// <param name="updateExpression">更新表達式。</param>
        /// <returns>影響的行數。</returns>
        int Update(Expression<Func<T, T>> updateExpression);


        /// <summary>
        /// 異步更新實體。
        /// </summary>
        /// <param name="entity">要更新的實體。</param>
        /// <param name="excludeFields">排除的字段。</param>
        /// <param name="timeout">超時時間（秒）。</param>
        /// <returns>影響的行數。</returns>
        Task<int> UpdateAsync(T entity, string[] excludeFields = null, int timeout = 120);

        /// <summary>
        /// 異步批量更新實體。
        /// </summary>
        /// <param name="entities">要更新的實體集合。</param>
        /// <param name="excludeFields">排除的字段。</param>
        /// <param name="timeout">超時時間（秒）。</param>
        /// <returns>影響的行數。</returns>
        Task<int> UpdateAsync(IEnumerable<T> entities, string[] excludeFields = null, int timeout = 120);

        /// <summary>
        /// 異步根據表達式更新實體。
        /// </summary>
        /// <param name="updateExpression">更新表達式。</param>
        /// <returns>影響的行數。</returns>
        Task<int> UpdateAsync(Expression<Func<T, T>> updateExpression);

        /// <summary>
        /// 刪除所有實體。
        /// </summary>
        /// <returns>影響的行數。</returns>
        int Delete();

        /// <summary>
        /// 刪除指定實體。
        /// </summary>
        /// <param name="model">要刪除的實體。</param>
        /// <returns>影響的行數。</returns>
        int Delete(T model);

        /// <summary>
        /// 異步刪除所有實體。
        /// </summary>
        /// <returns>影響的行數。</returns>
        Task<int> DeleteAsync();

        /// <summary>
        /// 異步刪除指定實體。
        /// </summary>
        /// <param name="model">要刪除的實體。</param>
        /// <returns>影響的行數。</returns>
        Task<int> DeleteAsync(T model);


        /// <summary>
        /// 插入實體。
        /// </summary>
        /// <param name="entity">要插入的實體。</param>
        /// <param name="excludeFields">排除的字段。</param>
        /// <returns>影響的行數。</returns>
        int Insert(T entity, string[] excludeFields = null);

        /// <summary>
        /// 插入實體並返回插入的身份（自增）ID。
        /// </summary>
        /// <param name="entity">要插入的實體。</param>
        /// <param name="excludeFields">排除的字段。</param>
        /// <returns>插入的身份ID。</returns>
        long InsertIdentity(T entity, string[] excludeFields = null);

        /// <summary>
        /// 批量插入實體。
        /// </summary>
        /// <param name="entities">要插入的實體集合。</param>
        /// <param name="excludeFields">排除的字段。</param>
        /// <param name="timeout">超時時間（秒）。</param>
        /// <returns>影響的行數。</returns>
        int Insert(IEnumerable<T> entities, string[] excludeFields = null, int timeout = 120);

        /// <summary>
        /// 異步插入實體。
        /// </summary>
        /// <param name="entity">要插入的實體。</param>
        /// <param name="excludeFields">排除的字段。</param>
        /// <returns>影響的行數。</returns>
        Task<int> InsertAsync(T entity, string[] excludeFields = null);

        /// <summary>
        /// 異步插入實體並返回插入的身份（自增）ID。
        /// </summary>
        /// <param name="entity">要插入的實體。</param>
        /// <param name="excludeFields">排除的字段。</param>
        /// <returns>插入的身份ID。</returns>
        Task<long> InsertIdentityAsync(T entity, string[] excludeFields = null);

        /// <summary>
        /// 異步批量插入實體。
        /// </summary>
        /// <param name="entities">要插入的實體集合。</param>
        /// <param name="excludeFields">排除的字段。</param>
        /// <param name="timeout">超時時間（秒）。</param>
        /// <returns>影響的行數。</returns>
        Task<int> InsertAsync(IEnumerable<T> entities, string[] excludeFields = null, int timeout = 120);

    }
}