using System;

namespace Comman.Dapper.Linq.Extension.Entites
{
    /// <summary>
    /// 代表用於解析導航屬性關聯的實體類別。
    /// </summary>
    public class Navigation
    {
        /// <summary>
        /// 定義的屬性字段，表示當前實體與關聯實體之間的關聯。
        /// </summary>
        public string AssoField { get; set; }

        /// <summary>
        /// 用於建立關聯的條件語句，指明如何與關聯表進行連接。
        /// </summary>
        public string JoinWhere { get; set; }

        /// <summary>
        /// 連接表的實體類型，用於定義與當前實體關聯的表。
        /// </summary>
        public Type AssociatedTableType { get; set; }

        /// <summary>
        /// 導航類型，指示導航屬性是單一實體（0）還是實體集合（1）。
        /// </summary>
        public NavigationType NavigationType { get; set; }
    }

    /// <summary>
    /// 表示導航屬性的類型，包括單一實體和實體集合。
    /// </summary>
    public enum NavigationType
    {
        SingleEntity = 0,  // 單一實體
        EntityCollection = 1  // 實體集合
    }
}