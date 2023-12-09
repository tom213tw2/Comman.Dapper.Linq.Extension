using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Comman.Dapper.Linq.Extension.Entites;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    /// <summary>
    /// 提供 SQL 語句的配置選項的抽象類。
    /// </summary>
    public abstract class IProviderOption
    {
        protected IProviderOption(string openQuote, string closeQuote, char parameterPrefix)
        {
            OpenQuote = openQuote;
            CloseQuote = closeQuote;
            ParameterPrefix = parameterPrefix;
            NavigationList = new List<NavigationMemberAssign>();
            MappingList = new Dictionary<string, string>();
            IsAsName = true;
        }

        /// <summary>
        /// 開引號字符。
        /// </summary>
        private string OpenQuote { get; set; }

        /// <summary>
        /// 閉引號字符。
        /// </summary>
        private string CloseQuote { get; set; }

        /// <summary>
        /// 參數前綴符號。
        /// </summary>
        public char ParameterPrefix { get; set; }

        /// <summary>
        /// 處理字段。
        /// </summary>
        /// <param name="field">字段名稱。</param>
        /// <returns>加上引號的字段名稱。</returns>
        public string CombineFieldName(string field)
        {
            return $"{OpenQuote}{field}{CloseQuote}";
        }

        /// <summary>
        /// 獲取當前時間。
        /// </summary>
        /// <returns>表示當前時間的字符串。</returns>
        public abstract string GetDate();

        /// <summary>
        /// 結合轉換處理。
        /// </summary>
        /// <param name="convertOption">轉換選項。</param>
        /// <param name="spliceField">拼接字段。</param>
        /// <param name="fieldInkoField">字段處理動作。</param>
        public abstract void CombineConvert(ConvertOption convertOption, StringBuilder spliceField,
            Action fieldInkoField);

        /// <summary>
        /// 結合時間處理。
        /// </summary>
        /// <param name="dateOption">日期選項。</param>
        /// <param name="spliceField">拼接字段。</param>
        /// <param name="fieldInvoke">字段處理動作。</param>
        /// <param name="valueInvoke">值處理動作。</param>
        public abstract void CombineDate(DateOption dateOption, StringBuilder spliceField, Action fieldInvoke,
            Action valueInvoke);


        /// <summary>
        /// 模糊轉義。
        /// </summary>
        /// <param name="value">要進行模糊轉義的對象值。</param>
        /// <param name="param">參考參數，用於進行附加操作或修改。</param>
        /// <param name="eFuzzyLocation">模糊位置設定，決定模糊匹配的方向（左側、右側或兩側）。</param>
        /// <returns>經過模糊轉義處理後的對象值。</returns>
        public virtual object FuzzyEscaping(object value, ref string param,
            EFuzzyLocation eFuzzyLocation = EFuzzyLocation.Both)
        {
            switch (eFuzzyLocation)
            {
                case EFuzzyLocation.Right:
                    value = $"{value}%";
                    return value;
                case EFuzzyLocation.Left:
                    value = $"%{value}";
                    return value;
                case EFuzzyLocation.Both:
                default:
                    value = $"%{value}%";
                    return value;
            }
        }

        /// <summary>
        /// 轉換為小寫。
        /// </summary>
        /// <param name="spliceField">用於拼接字符串的 StringBuilder 對象。</param>
        /// <param name="fieldInkove">一個動作代理，用於執行特定操作。</param>
        public virtual void ToLower(StringBuilder spliceField, Action fieldInkove)
        {
            spliceField.Append(" lower(");
            fieldInkove.Invoke();
            spliceField.Append(") ");
        }

        /// <summary>
        ///  轉換為大寫。
        /// </summary>
        /// <param name="spliceField">用於拼接字符串的 StringBuilder 對象。</param>
        /// <param name="fieldInvoke">一個動作代理，用於執行特定操作。</param>
        public virtual void ToUpper(StringBuilder spliceField, Action fieldInvoke)
        {
            spliceField.Append(" upper(");
            fieldInvoke.Invoke();
            spliceField.Append(") ");
        }


        /// <summary>
        /// 判斷是否為 null 的函數。
        /// </summary>
        /// <returns>若為 null 則返回特定的字符串。</returns>
        public abstract string IfNull();

        #region 临时属性

        /// <summary>
        /// 子查詢導航的集合。
        /// 用於儲存子查詢對應的導航成員分配。
        /// </summary>
        public List<NavigationMemberAssign> NavigationList { get; set; }

        /// <summary>
        /// 記錄映射對象的字典。
        /// 用於存儲字段名與其映射名之間的對應關係。
        /// </summary>
        public Dictionary<string, string> MappingList { get; set; }

        /// <summary>
        /// 標記是否重命名。
        /// 指示查詢中的表是否使用別名（例如 "table AS newName"）。
        /// </summary>
        public bool IsAsName { get; set; }

        #endregion

        #region 聚合函數

        /// <summary>
        ///     計數函數。
        ///     將計數操作（Count）添加到 SQL 查詢中。
        /// </summary>
        /// <param name="spliceField">用於拼接 SQL 查詢的 StringBuilder 對象。</param>
        /// <param name="fieldInvoke">執行特定字段操作的委託方法。</param>
        public virtual void Count(StringBuilder spliceField, Action fieldInvoke)
        {
            spliceField.Append(" Count(");
            fieldInvoke.Invoke();
            spliceField.Append(") ");
        }

        /// <summary>
        ///     計總函數。
        ///     將求和操作（Sum）添加到 SQL 查詢中。
        /// </summary>
        /// <param name="spliceField">用於拼接 SQL 查詢的 StringBuilder 對象。</param>
        /// <param name="fieldInvoke">執行特定字段操作的委託方法。</param>
        public virtual void Sum(StringBuilder spliceField, Action fieldInvoke)
        {
            spliceField.Append(" Sum(");
            fieldInvoke.Invoke();
            spliceField.Append(") ");
        }

        /// <summary>
        ///     最大值函數。
        ///     將求最大值操作（Max）添加到 SQL 查詢中。
        /// </summary>
        /// <param name="spliceField">用於拼接 SQL 查詢的 StringBuilder 對象。</param>
        /// <param name="fieldInvoke">執行特定字段操作的委託方法。</param>
        public virtual void Max(StringBuilder spliceField, Action fieldInvoke)
        {
            spliceField.Append(" Max(");
            fieldInvoke.Invoke();
            spliceField.Append(") ");
        }

        /// <summary>
        ///     最小值函數。
        ///     將求最小值操作（Min）添加到 SQL 查詢中。
        /// </summary>
        /// <param name="spliceField">用於拼接 SQL 查詢的 StringBuilder 對象。</param>
        /// <param name="fieldInvoke">執行特定字段操作的委託方法。</param>
        public virtual void Min(StringBuilder spliceField, Action fieldInvoke)
        {
            spliceField.Append(" Min(");
            fieldInvoke.Invoke();
            spliceField.Append(") ");
        }

        /// <summary>
        ///     平均值函數。
        ///     將求平均值操作（Avg）添加到 SQL 查詢中。
        /// </summary>
        /// <param name="spliceField">用於拼接 SQL 查詢的 StringBuilder 對象。</param>
        /// <param name="fieldInvoke">執行特定字段操作的委託方法。</param>
        public virtual void Avg(StringBuilder spliceField, Action fieldInvoke)
        {
            spliceField.Append(" Avg(");
            fieldInvoke.Invoke();
            spliceField.Append(") ");
        }

        #endregion
    }

    /// <summary>
    ///     轉換處理選項。
    ///     定義了一系列的數據類型轉換選項。
    /// </summary>
    public enum ConvertOption
    {
        ToInt32,
        ToString,
        ToDecimal,
        ToDouble,
        ToBoolean,
        ToDateTime
    }

    /// <summary>
    ///     時間格式處理選項。
    ///     定義了一系列的時間操作，如增加年份、月份、天數等。
    /// </summary>
    public enum DateOption
    {
        AddYears,
        AddMonths,
        AddDays,
        AddHours,
        AddMinutes,
        AddSeconds
    }

    /// <summary>
    ///     子查詢導航成員。
    ///     表示一個子查詢中的導航成員，包括其表達式、名稱和類型。
    /// </summary>
    public class NavigationMemberAssign
    {
        /// <summary>
        ///     導航查詢的表達式成員。
        ///     定義了導航查詢時使用的表達式。
        /// </summary>
        public MemberAssignment MemberAssign { get; set; }

        /// <summary>
        ///     導航查詢的對象名稱。
        ///     存儲導航查詢對象的名稱。
        /// </summary>
        public string MemberAssignName { get; set; }

        /// <summary>
        ///     導航查詢對象的類型。
        ///     表示導航查詢對象的 .NET 類型。
        /// </summary>
        public Type MemberAssignType { get; set; }
    }
}