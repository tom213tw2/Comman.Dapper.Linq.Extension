using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Comman.Dapper.Linq.Extension.Attributes;
using Comman.Dapper.Linq.Extension.Helper.Cache;

namespace Comman.Dapper.Linq.Extension.Entites
{
    /// <summary>
    /// 實體物件類別，用於反映實體類的屬性和表關聯。
    /// </summary>
    public class EntityObject
    {
        /// <summary>
        /// 透過實體類型初始化 EntityObject。
        /// </summary>
        /// <param name="type">實體的 Type。</param>
        public EntityObject(Type type)
        {
            // 反射獲取表名稱
            Name = type.Name;
            // 指定別名
            AsName = type.Name;
            // 獲取是否有 Display 屬性
            var typeAttribute =
                type.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(Display));
            if (typeAttribute != null)
            {
                var display = typeAttribute as Display;
                // 是否有重命名
                var rename = display.Rename;
                if (!string.IsNullOrEmpty(rename)) Name = rename;
                // 是否有命名空間
                var schema = display.Schema;
                if (!string.IsNullOrEmpty(schema)) Schema = schema;
                // 是否有指定別名
                var asName = display.AsName;
                if (!string.IsNullOrEmpty(asName))
                    AsName = asName;
                else
                    // 防止 rename 有值時產生錯誤
                    AsName = Name;
            }

            Type = type;
            AssemblyString = type.FullName;
            // 反射獲取實體類屬性
            Properties = type.GetProperties();
            var propertyInfoList = new List<PropertyInfo>();
            // 字段字典
            FieldPairs = new Dictionary<string, string>();
            // 導航列表
            Navigations = new List<JoinAssTable>();
            // 字段列表
            EntityFieldList = new List<EntityField>();
            // 反射獲取實體類字段
            foreach (var item in Properties)
            {
                // 排除子父類相同的字段
                if (FieldPairs.Any(x => x.Key == item.Name)) continue;
                // 判斷當前字段是否為導航屬性
                var foreignKey = item.GetCustomAttributes(true)
                    .FirstOrDefault(x => x.GetType().Equals(typeof(ForeignKey)));
                if (foreignKey != null)
                {
                    var foreign = foreignKey as ForeignKey;
                    // 導航屬性表
                    var navigationTable = !item.PropertyType.FullName.Contains("System.Collections.Generic")
                        ? item.PropertyType
                        : item.PropertyType.GenericTypeArguments[0];
                    var leftTab = EntityCache.QueryEntity(navigationTable);
                    Navigations.Add(new JoinAssTable
                    {
                        Action = JoinAction.Navigation,
                        JoinMode = JoinMode.LEFT,
                        RightTabName = AsName,
                        RightAssName = foreign.IndexField,
                        LeftTabName = leftTab.Name,
                        LeftAssName = foreign.AssociatedField,
                        TableType = navigationTable,
                        PropertyInfo = item
                    });
                    propertyInfoList.Add(item);
                    continue;
                }

                // 設置當前字段屬性
                var fieldAttribute = item.GetCustomAttributes(true)
                    .FirstOrDefault(x => x.GetType().Equals(typeof(Display)));
                if (fieldAttribute != null)
                {
                    var display = fieldAttribute as Display;
                    // 判斷是否為表關聯映射字段
                    if (display.IsField)
                    {
                        FieldPairs.Add(item.Name, item.Name);
                        // 獲取是否有重命名
                        if (!string.IsNullOrEmpty(display.Rename)) FieldPairs[item.Name] = display.Rename;
                        propertyInfoList.Add(item);
                        var sqlDbType = GetSqlDbType(item.PropertyType, out var ifNull);
                        // 設置詳細屬性
                        EntityFieldList.Add(new EntityField
                        {
                            FieldName = FieldPairs[item.Name],
                            PropertyInfo = item,
                            SqlDbType = display.SqlDbType != SqlDbType.Structured ? display.SqlDbType : sqlDbType,
                            Length = display.Length,
                            Description = display.Description,
                            IfNull = display.IfNull.HasValue ? display.IfNull.Value : ifNull,
                            DefaultValue = display.DefaultValue
                        });
                    }
                }
                else
                {
                    FieldPairs.Add(item.Name, item.Name);
                    propertyInfoList.Add(item);

                    // 設置詳細屬性
                    EntityFieldList.Add(new EntityField
                    {
                        FieldName = item.Name,
                        PropertyInfo = item,
                        SqlDbType = GetSqlDbType(item.PropertyType, out var ifNull),
                        Length = 0,
                        IfNull = ifNull
                    });
                }

                // 獲取主鍵
                if (string.IsNullOrEmpty(Identitys))
                {
                    // 判斷當前字段是否為主鍵
                    var identityAttribute = item.GetCustomAttributes(true)
                        .FirstOrDefault(x => x.GetType().Equals(typeof(Identity)));
                    if (identityAttribute != null)
                    {
                        Identitys = FieldPairs[item.Name];
                        EntityFieldList[EntityFieldList.Count - 1].IsIdentity = true;
                        EntityFieldList[EntityFieldList.Count - 1].IsIncrease =
                            (identityAttribute as Identity).IsIncrease;
                    }
                }
            }

            Properties = propertyInfoList.ToArray();
        }

        /// <summary>
        /// 主鍵名稱。
        /// </summary>
        public string Identitys { get; set; }

        /// <summary>
        /// 類名（表名稱）。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 命名空間。
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 指定別名。
        /// </summary>
        public string AsName { get; set; }

        /// <summary>
        /// 類型。
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 命名空間字串。
        /// </summary>
        public string AssemblyString { get; set; }

        /// <summary>
        /// 類反映的屬性實例。
        /// </summary>
        public PropertyInfo[] Properties { get; set; }

        /// <summary>
        /// 字段目錄（屬性名稱和實體名稱）。
        /// </summary>
        public Dictionary<string, string> FieldPairs { get; set; }

        /// <summary>
        /// 導航屬性列表。
        /// </summary>
        public List<JoinAssTable> Navigations { get; set; }

        /// <summary>
        /// 字段列表。
        /// </summary>
        public List<EntityField> EntityFieldList { get; set; }


        /// <summary>
        /// 獲取預設的資料類型。
        /// </summary>
        /// <param name="type">類型</param>
        /// <param name="ifNull">是否可以為空</param>
        /// <returns>對應的 SqlDbType</returns>
        private SqlDbType GetSqlDbType(Type type, out bool ifNull)
        {
            ifNull = false;
            // 設置資料庫欄位類型
            var sqlDbType = SqlDbType.VarChar;
            switch (type)
            {
                case Type t when t == typeof(int):
                    sqlDbType = SqlDbType.Int;
                    break;
                case Type t when t == typeof(long):
                    sqlDbType = SqlDbType.BigInt;
                    break;
                case Type t when t == typeof(Guid):
                    sqlDbType = SqlDbType.UniqueIdentifier;
                    break;
                case Type t when t == typeof(DateTime):
                    sqlDbType = SqlDbType.DateTime;
                    break;
                case Type t when t == typeof(decimal):
                    sqlDbType = SqlDbType.Decimal;
                    break;
                case Type t when t == typeof(bool):
                    sqlDbType = SqlDbType.Bit;
                    break;
                default:
                    // 處理可空類型
                    if (type.FullName?.Contains("System.Nullable") == true)
                    {
                        ifNull = true;
                        return GetSqlDbType(type.GenericTypeArguments[0], out ifNull);
                    }

                    break;
            }

            return sqlDbType;
        }

        /// <summary>
        /// 實體欄位。
        /// </summary>
        public class EntityField
        {
            /// <summary>
            /// 是否為主鍵。
            /// </summary>
            public bool IsIdentity { get; set; }

            /// <summary>
            /// 是否自增。
            /// </summary>
            public bool IsIncrease { get; set; }

            /// <summary>
            /// 屬性資訊。
            /// </summary>
            public PropertyInfo PropertyInfo { get; set; }

            /// <summary>
            /// 欄位名稱。
            /// </summary>
            public string FieldName { get; set; }

            /// <summary>
            /// 欄位類型。
            /// </summary>
            public SqlDbType SqlDbType { get; set; }

            /// <summary>
            /// 欄位長度。
            /// </summary>
            public int Length { get; set; }

            /// <summary>
            /// 欄位描述。
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// 是否允許為空。
            /// </summary>
            public bool IfNull { get; set; }

            /// <summary>
            /// 預設值。
            /// </summary>
            public object DefaultValue { get; set; }
        }

    }
}