using System;

namespace Comman.Dapper.Linq.Extension.Attributes
{
    /// <summary>
    /// 表示應用程式中自定義屬性的基底屬性類別。
    /// </summary>
    public class BaseAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="BaseAttribute"/> 類別的新實例。
        /// </summary>
        /// <param name="name">與屬性相關聯的名稱。</param>
        /// <param name="description">屬性的描述。</param>
        public BaseAttribute(string name = null, string description = null)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// 獲取或設置屬性的名稱。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 獲取或設置屬性的描述。
        /// </summary>
        public string Description { get; set; }
    }
}