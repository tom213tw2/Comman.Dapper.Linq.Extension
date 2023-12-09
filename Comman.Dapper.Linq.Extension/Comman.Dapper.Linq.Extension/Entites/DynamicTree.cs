using System.Data;
using System.Linq.Expressions;

namespace Comman.Dapper.Linq.Extension.Entites
{
    /// <summary>
    /// 動態樹結構，用於表示資料庫查詢的動態條件。
    /// </summary>
    public class DynamicTree
    {
        /// <summary>
        /// 初始化一個 <see cref="DynamicTree"/> 的新實例。
        /// 預設操作符為等於（Equal），值類型為字串（String）。
        /// </summary>
        public DynamicTree()
        {
            Operators = ExpressionType.Equal;
            ValueType = DbType.String;
        }

        /// <summary>
        /// 對應字段所在的表名稱（如果沒有指定，則預設為空）。
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// 用於查詢的字段名稱。
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 判斷條件的操作符（例如：13 等於（Equal），6 模糊查詢（Like），16 大於等於（GreaterThanOrEqual），21 小於等於（LessThanOrEqual））。
        /// </summary>
        public ExpressionType Operators { get; set; }

        /// <summary>
        /// 判斷條件中用於比較的值。
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 比較值的資料類型（例如：6 DateTime，11 Int32，16 String）。
        /// </summary>
        public DbType ValueType { get; set; }
    }
}