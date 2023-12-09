using System;
using System.Collections.Generic;
using System.Reflection;
using Comman.Dapper.Linq.Extension.Dapper;

namespace Comman.Dapper.Linq.Extension.Entites
{
    /// <summary>
    /// 表示連接關聯表的類別。
    /// </summary>
    public class JoinAssTable : ICloneable
    {
        public JoinAssTable()
        {
            MapperList = new Dictionary<string, string>();
        }

        public JoinAction Action { get; set; }
        public JoinMode JoinMode { get; set; }
        public string RightTabName { get; set; }
        public string LeftTabName { get; set; }
        public string RightAssName { get; set; }
        public string LeftAssName { get; set; }
        public Type TableType { get; set; }
        public string JoinSql { get; set; }

        public string NoLockString { get; set; }
        public Type PropertyType => PropertyInfo.PropertyType;

        /// <summary>
        /// 接收導航屬性的物件。
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// 自定義查詢的字段。
        /// </summary>
        public Dictionary<string, string> SelectFieldPairs { get; set; }

        /// <summary>
        /// 表首字段。
        /// </summary>
        public string FirstFieldName => MapperList.Values?.AsList()[0];

        /// <summary>
        /// 表尾字段。
        /// </summary>
        public string LastFieldName => MapperList.Values.AsList()[MapperList.Count - 1];

        /// <summary>
        /// 映射目錄。
        /// </summary>
        public Dictionary<string, string> MapperList { get; set; }

        /// <summary>
        /// 是否手動開關映射（優先級最高）。
        /// </summary>
        public bool IsUseMapper { get; set; } = true;

        /// <summary>
        /// 是否需要映射字段。
        /// </summary>
        public bool IsMapperField { get; set; } = true;

        /// <summary>
        /// 是否是Dto。
        /// </summary>
        public bool IsDto { get; set; } = false;

        /// <summary>
        /// Dto類。
        /// </summary>
        public Type DtoType { get; set; }

        /// <summary>
        /// 克隆方法。
        /// </summary>
        /// <returns>克隆出的新物件。</returns>
        public object Clone()
        {
            return new JoinAssTable
            {
                Action = Action,
                JoinMode = JoinMode,
                RightTabName = RightTabName,
                LeftTabName = LeftTabName,
                RightAssName = RightAssName,
                LeftAssName = LeftAssName,
                TableType = TableType,
                JoinSql = JoinSql,
                SelectFieldPairs = new Dictionary<string, string>(),
                MapperList = new Dictionary<string, string>(),
                IsMapperField = false,
                PropertyInfo = PropertyInfo
            };
        }
    }

    /// <summary>
    /// 連接方式
    /// </summary>
    public enum JoinAction
    {
        Default, // 默認表達式
        Sql, // SQL查詢
        Navigation // 導航屬性
    }

    /// <summary>
    /// 連接模式
    /// </summary>
    public enum JoinMode
    {
        LEFT, // 左連接
        RIGHT, // 右連接
        INNER, // 內連接
        FULL // 全連接
    }
}
