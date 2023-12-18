using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Comman.Dapper.Linq.Extension.Core.Interfaces;
using Comman.Dapper.Linq.Extension.Dapper;
using Comman.Dapper.Linq.Extension.Entites;
using Comman.Dapper.Linq.Extension.Exception;
using Comman.Dapper.Linq.Extension.Extension;
using Comman.Dapper.Linq.Extension.Helper.Cache;

namespace Comman.Dapper.Linq.Extension.Expressions
{
    /// <summary>
    /// 實現表達式解析的基類。
    /// </summary>
    public class BaseExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// 提供方選項。
        /// </summary>
        protected IProviderOption providerOption;

        public BaseExpressionVisitor(SqlProvider provider)
        {
            SpliceField = new StringBuilder();
            Param = new DynamicParameters();
            Provider = provider;
            providerOption = provider.ProviderOption;
        }

        /// <summary>
        /// 字段SQL。
        /// </summary>
        internal StringBuilder SpliceField { get; set; }

        /// <summary>
        /// 參數。
        /// </summary>
        protected DynamicParameters Param { get; set; }

        /// <summary>
        /// 解析提供方。
        /// </summary>
        protected SqlProvider Provider { get; set; }

        /// <summary>
        /// 解析第n個下標。
        /// </summary>
        protected int Index { get; set; }


        /// <summary>
        /// 需要執行加、減、乘、除運算時進行的拼接操作。
        /// </summary>
        /// <param name="node">二元表達式節點</param>
        /// <returns>表達式訪問結果</returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var binary = new BinaryExpressionVisitor(node, Provider, Index);
            SpliceField.Append(binary.SpliceField);
            Param.AddDynamicParams(binary.Param);
            return node;
        }

        /// <summary>
        /// 訪問值表達式節點。
        /// </summary>
        /// <param name="node">常數表達式節點</param>
        /// <returns>表達式訪問結果</returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            // 參數名稱
            var paramName = $"{providerOption.ParameterPrefix}Member_Param_{Index}_{Param.ParameterNames.Count()}";
            // 獲取值
            var nodeValue = node.ToConvertAndGetValue();
            // 設置SQL參數
            SpliceField.Append(paramName);
            Param.Add(paramName, nodeValue);
            return node;
        }

        /// <summary>
        /// 訪問成員表達式節點。
        /// </summary>
        /// <param name="node">成員表達式節點</param>
        /// <returns>表達式訪問結果</returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            // 獲取需要計算的字段值
            var expTypeName = node.Expression?.GetType().FullName ?? "";
            if (expTypeName == "System.Linq.Expressions.TypedParameterExpression" ||
                expTypeName == "System.Linq.Expressions.PropertyExpression")
            {
                // 檢查是否為可空類型
                if (!node.Expression.Type.FullName.Contains("System.Nullable"))
                {
                    // 檢查是否為成員值對象
                    if (expTypeName == "System.Linq.Expressions.PropertyExpression" && node.IsConstantExpression())
                    {
                        // 參數
                        var paramName =
                            $"{providerOption.ParameterPrefix}Member_Param_{Index}_{Param.ParameterNames.Count()}";
                        // 獲取值
                        var nodeValue = node.ToConvertAndGetValue();
                        
                        // 設置SQL
                        SpliceField.Append(paramName);
                        Param.Add(paramName, nodeValue);
                        return node;
                    }

                    var member = EntityCache.QueryEntity(node.Expression.Type);
                    var fieldName = member.FieldPairs[node.Member.Name];
                    // 字段全稱
                    var fieldStr = Provider.IsAppendAsName
                        ? $"{member.AsName}.{providerOption.CombineFieldName(fieldName)}"
                        : providerOption.CombineFieldName(member.FieldPairs[node.Member.Name]);
                    SpliceField.Append(fieldStr);
                }
                else
                {
                    // 訪問可空函數
                    Visit(node.Expression);
                    if (node.Member.Name == "HasValue")
                    {
                        SpliceField.Append(" IS NOT NULL");
                    }
                }
            }
            else
            {
                // 參數
                var paramName = $"{providerOption.ParameterPrefix}Member_Param_{Index}_{Param.ParameterNames.Count()}";
                // 獲取值
                var nodeValue = node.ToConvertAndGetValue();
                // 設置SQL
                SpliceField.Append(paramName);
                Param.Add(paramName, nodeValue);
            }

            return node;
        }


        /// <summary>
        /// 訪問方法調用表達式節點。
        /// </summary>
        /// <param name="node">方法調用表達式節點</param>
        /// <returns>表達式訪問結果</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType.FullName.Contains("Kogel.Dapper.Extension.Function")) // 系統函數
            {
                Operation(node);
            }
            else if (node.Method.DeclaringType.FullName.Contains("Kogel.Dapper.Extension"))
            {
                var parameters = new DynamicParameters();
                SpliceField.Append($"({node.MethodCallExpressionToSql(ref parameters, Index)})");
                Param.AddDynamicParams(parameters);
            }
            else
            {
                Operation(node);
            }

            return node;
        }


        /// <summary>
        /// 解析方法調用表達式。
        /// </summary>
        /// <param name="node">方法調用表達式節點</param>
        private void Operation(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                #region 轉換計算

                case "ToInt32":
                case "ToString":
                case "ToDecimal":
                case "ToDouble":
                case "ToBoolean":
                case "ToDateTime":
                {
                    var convertOption = (ConvertOption)Enum.Parse(typeof(ConvertOption), node.Method.Name);
                    providerOption.CombineConvert(convertOption, SpliceField,
                        () => { Visit(node.Object != null ? node.Object : node.Arguments[0]); });
                }
                    break;

                #endregion

                #region 時間計算

                case "AddYears":
                case "AddMonths":
                case "AddDays":
                case "AddHours":
                case "AddMinutes":
                case "AddSeconds":
                {
                    var dateOption = (DateOption)Enum.Parse(typeof(DateOption), node.Method.Name);
                    providerOption.CombineDate(dateOption, SpliceField,
                        () => { Visit(node.Object); },
                        () => { Visit(node.Arguments); });
                }
                    break;

                #endregion

                #region 字符處理

                case "ToLower":
                {
                    providerOption.ToLower(SpliceField,
                        () =>
                        {
                            if (node.Object != null)
                                Visit(node.Object);
                            else
                                Visit(node.Arguments);
                        });
                }
                    break;
                case "ToUpper":
                {
                    providerOption.ToUpper(SpliceField,
                        () =>
                        {
                            if (node.Object != null)
                                Visit(node.Object);
                            else
                                Visit(node.Arguments);
                        });
                }
                    break;
                case "Replace":
                {
                    SpliceField.Append("Replace(");
                    Visit(node.Object);
                    SpliceField.Append(",");
                    Visit(node.Arguments[0]);
                    SpliceField.Append(",");
                    Visit(node.Arguments[1]);
                    SpliceField.Append(")");
                }
                    break;
                case "Trim":
                {
                    SpliceField.Append("Trim(");
                    Visit(node.Object);
                    SpliceField.Append(")");
                }
                    break;
                case "Concat":
                {
                    SpliceField.Append("Concat(");
                    Visit(node.Arguments[0]);
                    SpliceField.Append(",");
                    Visit(node.Arguments[1]);
                    SpliceField.Append(")");
                }
                    break;
                case "IfNull":
                {
                    SpliceField.Append($"{providerOption.IfNull()}(");
                    Visit(node.Arguments[0]);
                    SpliceField.Append(",");
                    Visit(node.Arguments[1]);
                    SpliceField.Append(")");
                }
                    break;
                case "ConcatSql":
                {
                    SpliceField.Append(node.Arguments[0].ToConvertAndGetValue());
                    // 如果有參數，則添加
                    if (node.Arguments.Count > 1) Param.AddDynamicParams(node.Arguments[1].ToConvertAndGetValue());
                }
                    break;

                #endregion

                #region 聚合函數

                case "Count":
                {
                    providerOption.Count(SpliceField, () => { Visit(node.Arguments); });
                }
                    break;
                case "Sum":
                {
                    providerOption.Sum(SpliceField, () => { Visit(node.Arguments); });
                }
                    break;
                case "Max":
                {
                    providerOption.Max(SpliceField, () => { Visit(node.Arguments); });
                }
                    break;
                case "Min":
                {
                    providerOption.Min(SpliceField, () => { Visit(node.Arguments); });
                }
                    break;
                case "Avg":
                {
                    providerOption.Avg(SpliceField, () => { Visit(node.Arguments); });
                }
                    break;

                #endregion

                #region 導航屬性

                case "Select":
                {
                    Visit(node.Arguments[1]);
                    break;
                }

                #endregion

                default:
                    SpliceField.Append(node.ToConvertAndGetValue());
                    break;
            }
        }


        /// <summary>
        /// 用於解析條件表達式。
        /// </summary>
        public class WhereExpressionVisitor : BaseExpressionVisitor
        {
            public WhereExpressionVisitor(SqlProvider provider) : base(provider)
            {
                SpliceField = new StringBuilder();
                Param = new DynamicParameters();
            }

            /// <summary>
            /// 參數標記。
            /// </summary>
            internal string Prefix { get; set; }

            /// <summary>
            /// 字段。
            /// </summary>
            private string FieldName { get; set; } = "";

            /// <summary>
            /// 帶參數標識的參數名稱。
            /// </summary>
            private string ParamName => $"{GetParamName()}{Prefix}";

            /// <summary>
            /// 拼接SQL。
            /// </summary>
            internal new StringBuilder SpliceField { get; set; }

            /// <summary>
            /// 參數目錄。
            /// </summary>
            internal new DynamicParameters Param { get; set; }

            /// <summary>
            /// 獲取參數名稱。
            /// </summary>
            /// <returns>參數名稱。</returns>
            private string GetParamName()
            {
                var builder = new StringBuilder();
                builder.Append(providerOption.ParameterPrefix);
                if (!string.IsNullOrEmpty(FieldName))
                    builder.Append("Param");
                builder.Append($"_{Param.ParameterNames.Count()}{Index}");
                return builder.ToString();
            }


            /// <summary>
            /// 訪問方法調用表達式節點。
            /// </summary>
            /// <param name="node">方法調用表達式節點</param>
            /// <returns>表達式訪問結果</returns>
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var callName = node.Method.DeclaringType.FullName;
                // 使用convert函數裡待執行的SQL數據
                if (callName.Equals("Comman.Dapper.Linq.Extension.Extension.ExpressExpansion")) // 自定義擴展方法
                {
                    Operation(node);
                }
                else if (callName.Contains("Comman.Dapper.Linq.Extension.Helper.Function")) // 系統函數
                {
                    Operation(node);
                }
                else if (callName.Contains("Comman.Dapper.Linq.Extension"))
                {
                    base.VisitMethodCall(node);
                    SpliceField.Append(base.SpliceField);
                    Param.AddDynamicParams(base.Param);
                }
                else
                {
                    Operation(node);
                }

                return node;
            }

            /// <summary>
            /// 處理一元表達式節點。
            /// </summary>
            /// <param name="node">一元表達式節點</param>
            /// <returns>表達式訪問結果</returns>
            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (node.NodeType == ExpressionType.Not)
                {
                    SpliceField.Append("NOT(");
                    Visit(node.Operand);
                    SpliceField.Append(")");
                }
                else
                {
                    Visit(node.Operand);
                }

                return node;
            }


            /// <summary>
/// 重寫成員對象，獲得字段名稱。
/// </summary>
/// <param name="node">成員表達式節點</param>
/// <returns>表達式訪問結果</returns>
protected override Expression VisitMember(MemberExpression node)
{
    var expType = node.Expression?.GetType();
    var expTypeName = expType?.FullName ?? "";
    if (expTypeName == "System.Linq.Expressions.TypedParameterExpression" ||
        expTypeName == "System.Linq.Expressions.PropertyExpression")
    {
        // 驗證是否是可空對象
        if (!node.Expression.Type.FullName.Contains("System.Nullable"))
        {
            // 是否是成員值對象
            if (expTypeName == "System.Linq.Expressions.PropertyExpression" && node.IsConstantExpression())
            {
                SpliceField.Append(ParamName);
                var nodeValue = node.ToConvertAndGetValue();
                Param.Add(ParamName, nodeValue);
                return node;
            }

            var member = EntityCache.QueryEntity(node.Expression.Type);
            FieldName = member.FieldPairs[node.Member.Name];
            // 字段全稱
            var fieldStr = Provider.IsAppendAsName
                ? $"{member.AsName}.{providerOption.CombineFieldName(member.FieldPairs[node.Member.Name])}"
                : providerOption.CombineFieldName(member.FieldPairs[node.Member.Name]);
            SpliceField.Append(fieldStr);
            // 導航屬性允許顯示字段
            if (expTypeName == "System.Linq.Expressions.PropertyExpression")
            {
                // 設置查詢該導航屬性的條件
                var joinTable = Provider.JoinList.Find(x => x.TableType.IsTypeEquals(member.Type));
                if (joinTable != null && !joinTable.IsMapperField)
                {
                    joinTable.IsMapperField = true;
                }
                else
                {
                    // 不存在於第一層中，可能在後幾層嵌套使用導航屬性
                    var parentExpression = (node.Expression as MemberExpression)?.Expression;
                    var parentEntity = EntityCache.QueryEntity(parentExpression.Type);
                    joinTable = parentEntity.Navigations.Find(x => x.TableType == member.Type);
                    if (joinTable != null)
                    {
                        joinTable = (JoinAssTable)joinTable.Clone();
                        joinTable.IsMapperField = true;
                        // 加入導航連表到提供方
                        Provider.JoinList.Add(joinTable);
                    }
                }
            }
        }
        else
        {
            // 可空函數
            Visit(node.Expression);
            if (node.Member.Name == "HasValue")
            {
                SpliceField.Append(" IS NOT NULL");
            }
        }
    }
    else
    {
        SpliceField.Append(ParamName);
        var nodeValue = node.ToConvertAndGetValue();
        Param.Add(ParamName, nodeValue);
    }

    return node;
}


            /// <summary>
            /// 重寫值對象，記錄參數。
            /// </summary>
            /// <param name="node">常數表達式節點</param>
            /// <returns>表達式訪問結果</returns>
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (!string.IsNullOrEmpty(FieldName))
                {
                    SpliceField.Append(ParamName);
                    Param.Add(ParamName, node.ToConvertAndGetValue());
                }
                else
                {
                    var nodeValue = node.ToConvertAndGetValue();
                    switch (nodeValue)
                    {
                        case true:
                            SpliceField.Append("1=1");
                            break;
                        case false:
                            SpliceField.Append("1!=1");
                            break;
                        default:
                            SpliceField.Append(ParamName);
                            Param.Add(ParamName, nodeValue);
                            break;
                    }
                }

                return node;
            }


            /// <summary>
            ///     解析函数
            /// </summary>
            /// <param name="node"></param>
            private void Operation(MethodCallExpression node)
            {
                switch (node.Method.Name)
                {
                    case "Contains":
                    {
                        if (node.Object != null && !node.Object.Type.FullName.Contains("System.Collections.Generic"))
                        {
                            Visit(node.Object);
                            var value = node.Arguments[0].ToConvertAndGetValue();
                            var param = ParamName;
                            value = providerOption.FuzzyEscaping(value, ref param);
                            SpliceField.Append($" LIKE {param}");
                            Param.Add(ParamName, value);
                        }
                        else
                        {
                            if (node.Object != null)
                            {
                                Visit(node.Arguments[0]);
                                SpliceField.Append(" IN ");
                                Visit(node.Object);
                            }
                            else
                            {
                                Visit(node.Arguments[1]);
                                SpliceField.Append($" IN {ParamName}");
                                //这里只能手动记录参数
                                var nodeValue = node.Arguments[0].ToConvertAndGetValue();
                                Param.Add(ParamName, nodeValue);
                            }
                        }
                    }
                        break;
                    case "StartsWith":
                    {
                        Visit(node.Object);
                        var value = node.Arguments[0].ToConvertAndGetValue();
                        var param = ParamName;
                        value = providerOption.FuzzyEscaping(value, ref param, EFuzzyLocation.Right);
                        SpliceField.Append($" LIKE {param}");
                        Param.Add(ParamName, value);
                    }
                        break;
                    case "EndsWith":
                    {
                        Visit(node.Object);
                        var value = node.Arguments[0].ToConvertAndGetValue();
                        var param = ParamName;
                        value = providerOption.FuzzyEscaping(value, ref param, EFuzzyLocation.Left);
                        SpliceField.Append($" LIKE {param}");
                        Param.Add(ParamName, value);
                    }
                        break;
                    case "Equals":
                    {
                        if (node.Object != null)
                        {
                            Visit(node.Object);
                            SpliceField.Append(" = ");
                            Visit(node.Arguments[0]);
                        }
                        else
                        {
                            Visit(node.Arguments[0]);
                            SpliceField.Append(" = ");
                            Visit(node.Arguments[1]);
                        }
                    }
                        break;
                    case "In":
                    {
                        Visit(node.Arguments[0]);
                        SpliceField.Append($" IN {ParamName}");
                        var value = node.Arguments[1].ToConvertAndGetValue();
                        Param.Add(ParamName, value);
                    }
                        break;
                    case "NotIn":
                    {
                        Visit(node.Arguments[0]);
                        SpliceField.Append($" NOT IN {ParamName}");
                        var value = node.Arguments[1].ToConvertAndGetValue();
                        Param.Add(ParamName, value);
                    }
                        break;
                    case "IsNull":
                    {
                        Visit(node.Arguments[0]);
                        SpliceField.Append(" IS NULL");
                    }
                        break;
                    case "IsNotNull":
                    {
                        Visit(node.Arguments[0]);
                        SpliceField.Append(" IS NOT NULL");
                    }
                        break;
                    case "IsNullOrEmpty":
                    {
                        SpliceField.Append("(");
                        Visit(node.Arguments[0]);
                        SpliceField.Append(" IS NULL OR ");
                        Visit(node.Arguments[0]);
                        SpliceField.Append(" =''");
                        SpliceField.Append(")");
                    }
                        break;
                    case "Between":
                    {
                        if (node.Object != null)
                        {
                            Visit(node.Object);
                            SpliceField.Append(" BETWEEN ");
                            Visit(node.Arguments[0]);
                            SpliceField.Append(" AND ");
                            Visit(node.Arguments[1]);
                        }
                        else
                        {
                            Visit(node.Arguments[0]);
                            SpliceField.Append(" BETWEEN ");
                            Visit(node.Arguments[1]);
                            SpliceField.Append(" AND ");
                            Visit(node.Arguments[2]);
                        }
                    }
                        break;
                    case "Any":
                    {
                        Type entityType;
                        if (ExpressionExtension.IsAnyBaseEntity(node.Arguments[0].Type, out entityType))
                        {
                            //导航属性有条件时设置查询该导航属性
                            var navigationTable = Provider.JoinList.Find(x => x.TableType.IsTypeEquals(entityType));
                            if (navigationTable != null)
                            {
                                navigationTable.IsMapperField = true;
                            }
                            else
                            {
                                //不存在第一层中，可能在后几层嵌套使用导航属性
                                //获取调用者表达式
                                var parentExpression = (node.Arguments[0] as MemberExpression).Expression;
                                var parentEntity = EntityCache.QueryEntity(parentExpression.Type);
                                navigationTable = parentEntity.Navigations.Find(x => x.TableType == entityType);
                                if (navigationTable != null)
                                {
                                    navigationTable = (JoinAssTable)navigationTable.Clone();
                                    navigationTable.IsMapperField = true;
                                    //加入导航连表到提供方
                                    Provider.JoinList.Add(navigationTable);
                                }
                            }

                            //解析导航属性条件
                            var navigationExpression = new WhereExpression(node.Arguments[1] as LambdaExpression,
                                $"_Navi_{navigationTable.PropertyInfo.Name}", Provider);
                            //添加sql和参数
                            SpliceField.Append($" 1=1 {navigationExpression.SqlCmd}");
                            foreach (var paramName in navigationExpression.Param.ParameterNames)
                                //相同的key会直接顶掉
                                Param.Add(paramName, navigationExpression.Param.Get<object>(paramName));
                            //this.Param.AddDynamicParams(navigationExpression.Param);
                        }
                        else
                        {
                            throw new DapperExtensionException("导航属性类需要继承IBaseEntity");
                        }
                    }
                        break;

                    #region Convert转换计算

                    case "ToInt32":
                    case "ToString":
                    case "ToDecimal":
                    case "ToDouble":
                    case "ToBoolean":
                    case "ToDateTime":
                    {
                        var convertOption = (ConvertOption)Enum.Parse(typeof(ConvertOption), node.Method.Name);
                        providerOption.CombineConvert(convertOption, SpliceField,
                            () => { Visit(node.Object != null ? node.Object : node.Arguments[0]); });
                    }
                        break;

                    #endregion

                    #region 时间计算

                    case "AddYears":
                    case "AddMonths":
                    case "AddDays":
                    case "AddHours":
                    case "AddMinutes":
                    case "AddSeconds":
                    {
                        var dateOption = (DateOption)Enum.Parse(typeof(DateOption), node.Method.Name);
                        providerOption.CombineDate(dateOption, SpliceField,
                            () => { Visit(node.Object); },
                            () => { Visit(node.Arguments); });
                    }
                        break;

                    #endregion

                    #region 字符处理

                    case "ToLower":
                    {
                        providerOption.ToLower(SpliceField,
                            () =>
                            {
                                if (node.Object != null)
                                    Visit(node.Object);
                                else
                                    Visit(node.Arguments);
                            });
                    }
                        break;
                    case "ToUpper":
                    {
                        providerOption.ToUpper(SpliceField,
                            () =>
                            {
                                if (node.Object != null)
                                    Visit(node.Object);
                                else
                                    Visit(node.Arguments);
                            });
                    }
                        break;
                    case "Replace":
                    {
                        SpliceField.Append("Replace(");
                        Visit(node.Object);
                        SpliceField.Append(",");
                        Visit(node.Arguments[0]);
                        SpliceField.Append(",");
                        Visit(node.Arguments[1]);
                        SpliceField.Append(")");
                    }
                        break;
                    case "Substring" :
                    {
                        SpliceField.Append("SUBSTRING(");
                        Visit(node.Object);
                        SpliceField.Append(",");
                        var aa = Convert.ToInt32(node.Arguments[0].ToString()) + 1;
                        Visit(Expression.Constant(aa, typeof(int)));
                        SpliceField.Append(",");
                        Visit(node.Arguments[1]);
                        SpliceField.Append(")");
                    }
                        break;
                    case "Trim":
                    {
                        SpliceField.Append("Trim(");
                        Visit(node.Object);
                        SpliceField.Append(")");
                    }
                        break;
                    case "Concat":
                    {
                        SpliceField.Append("Concat(");
                        Visit(node.Arguments[0]);
                        SpliceField.Append(",");
                        Visit(node.Arguments[1]);
                        SpliceField.Append(")");
                    }
                        break;
                    case "IfNull":
                    {
                        SpliceField.Append($"{providerOption.IfNull()}(");
                        Visit(node.Arguments[0]);
                        SpliceField.Append(",");
                        Visit(node.Arguments[1]);
                        SpliceField.Append(")");
                    }
                        break;
                    case "ConcatSql":
                    {
                        SpliceField.Append(node.Arguments[0].ToConvertAndGetValue());
                        // Param
                        if (node.Arguments.Count > 1) Param.AddDynamicParams(node.Arguments[1].ToConvertAndGetValue());
                    }
                        break;

                    #endregion

                    #region 聚合函数

                    case "Count":
                    {
                        providerOption.Count(SpliceField, () => { Visit(node.Arguments); });
                    }
                        break;
                    case "Sum":
                    {
                        providerOption.Sum(SpliceField, () => { Visit(node.Arguments); });
                    }
                        break;
                    case "Max":
                    {
                        providerOption.Max(SpliceField, () => { Visit(node.Arguments); });
                    }
                        break;
                    case "Min":
                    {
                        providerOption.Min(SpliceField, () => { Visit(node.Arguments); });
                    }
                        break;
                    case "Avg":
                    {
                        providerOption.Avg(SpliceField, () => { Visit(node.Arguments); });
                    }
                        break;

                    #endregion

                    #region lambda函数

                    case "FirstOrDefault":
                    {
                        var paramName = ParamName;
                        SpliceField.Append(paramName);
                        Param.Add(paramName, node.ToConvertAndGetValue());
                    }
                        break;

                    #endregion

                    default:
                    {
                        if (node.Object != null)
                            Visit(node.Object);
                        else
                            Visit(node.Arguments);
                    }
                        break;
                }
            }
        }

        /// <summary>
        ///     用于解析二元表达式
        /// </summary>
        public class BinaryExpressionVisitor : WhereExpressionVisitor
        {
            public BinaryExpressionVisitor(BinaryExpression expression, SqlProvider provider, int index = 0,
                string prefix = null) : base(provider)
            {
                SpliceField = new StringBuilder();
                Param = new DynamicParameters();
                Index = index;
                Prefix = prefix;
                SpliceField.Append("(");
                Visit(expression);
                SpliceField.Append(")");
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                SpliceField.Append("(");
                Visit(node.Left);
                var expressionType = node.GetExpressionType();
                SpliceField.Append(expressionType);
                if (expressionType == " AND " || expressionType == " OR ")
                    switch (node.Right.ToString())
                    {
                        case "True":
                            SpliceField.Append("1=1");
                            break;
                        case "False":
                            SpliceField.Append("1!=1");
                            break;
                        default:
                            Visit(node.Right);
                            break;
                    }
                else
                    Visit(node.Right);

                SpliceField.Append(")");
                return node;
            }
        }
    }
}