using System;

namespace Comman.Dapper.Linq.Extension.Attributes
{
    /// <summary>
    /// 代表導航關聯屬性。
    /// </summary>
    public class ForeignKey : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="ForeignKey"/> 屬性類別的新實例。
        /// </summary>
        /// <param name="indexField">用於導航關聯的索引字段。</param>
        /// <param name="associatedField">導航表中的關聯字段。</param>
        public ForeignKey(string indexField, string associatedField)
        {
            IndexField = indexField;
            AssociatedField = associatedField;
        }

        /// <summary>
        /// 獲取或設置用於導航關聯的索引字段。
        /// </summary>
        public string IndexField { get; set; }

        /// <summary>
        /// 獲取或設置導航表中的關聯字段。
        /// </summary>
        public string AssociatedField { get; set; }
    }
}