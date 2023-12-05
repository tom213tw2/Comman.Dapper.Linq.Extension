using System.Data;
using Kogel.Dapper.Extension.Entites;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    /// <summary>
    /// 同步實體到數據庫。
    /// </summary>
    public interface ICodeFirst
    {
        /// <summary>
        /// 同步字段（生成 SQL）。
        /// </summary>
        /// <param name="typeEntity">實體對象。</param>
        /// <param name="field">實體字段。</param>
        /// <returns>生成的 SQL 字串。</returns>
        string SyncField(EntityObject typeEntity, EntityField field);

        /// <summary>
        /// 同步單個表結構。
        /// </summary>
        /// <param name="typeEntity">實體對象。</param>
        void SyncTable(EntityObject typeEntity);

        /// <summary>
        /// 同步整體實體結構。
        /// </summary>
        void SyncStructure();

        /// <summary>
        /// 轉換字段類型。
        /// </summary>
        /// <param name="sqlDbType">SQL 字段類型。</param>
        /// <param name="length">字段長度。</param>
        /// <returns>轉換後的字段類型。</returns>
        string ConversionFieldType(SqlDbType sqlDbType, int length);
    }
}