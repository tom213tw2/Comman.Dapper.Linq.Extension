using System.Data;

namespace Comman.Dapper.Linq.Extension.Attributes
{
    /// <summary>
    /// 代表實體欄位的顯示屬性。
    /// </summary>
    public class Display : BaseAttribute
    {
        /// <summary>
        /// 初始化 <see cref="Display"/> 類別的新實例。
        /// </summary>
        /// <param name="name">欄位名稱。</param>
        /// <param name="description">欄位描述。</param>
        /// <param name="rename">對應的資料庫欄位名稱。</param>
        /// <param name="schema">資料庫架構（SQL Server 預設為 'dbo'）。</param>
        /// <param name="asName">欄位的別名。</param>
        /// <param name="isField">指示該屬性是否為表相關欄位。</param>
        /// <param name="sqlDbType">欄位的 SQL 資料庫類型。</param>
        /// <param name="length">欄位長度。</param>
        /// <param name="isNull">指示該欄位是否可為 null。</param>
        /// <param name="defaultValue">欄位的預設值。</param>
        /// <param name="dbType"></param>
        public Display(string name = null, string description = null, string rename = null, string schema = null,
                       string asName = null, bool isField = true, SqlDbType sqlDbType = SqlDbType.Structured,
                       int length = 0, bool isNull = default, object defaultValue = null,DbType dbType=System.Data.DbType.Object)
        {
            Name = name;
            Description = description;
            IsField = isField;
            Rename = string.IsNullOrEmpty(rename) ? name : rename;
            Schema = schema;
            AsName = asName;
            SqlDbType = sqlDbType;
            Length = length;
            IfNull = isNull;
            DefaultValue = defaultValue;
            DbType = dbType;
        }

        /// <summary>
        /// 獲取或設置一個值，指示此屬性是否為表相關欄位。
        /// </summary>
        public bool IsField { get; set; }

        /// <summary>
        /// 獲取或設置此屬性的資料庫映射名稱。如果未指定，則預設為類別名稱。
        /// </summary>
        public string Rename { get; set; }

        /// <summary>
        /// 獲取或設置資料庫的命名空間，例如 SQL Server 中的 'dbo'。
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 獲取或設置此欄位的 'AS' 別名。
        /// </summary>
        public string AsName { get; set; }

        /// <summary>
        /// 獲取或設置此欄位的 SQL 資料庫類型。
        /// </summary>
        public SqlDbType SqlDbType { get; set; }

        /// <summary>
        /// 獲取或設置此欄位的長度。
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 獲取或設置一個值，指示此欄位是否可以為 null。
        /// </summary>
        public bool? IfNull { get; set; }

        /// <summary>
        /// 獲取或設置此欄位的預設值。
        /// </summary>
        public object DefaultValue { get; set; }

        public DbType DbType { get; set; }
    }
}
