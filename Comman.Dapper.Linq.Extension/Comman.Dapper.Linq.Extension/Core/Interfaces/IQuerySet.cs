using System;
using System.Linq.Expressions;
using Comman.Dapper.Linq.Extension.Entites;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    public partial interface IQuerySet<T> : IAggregation<T>, IQuery<T>
    {
        /// <summary>
        ///     根據類型重命名表名。
        ///     允許將特定類型的表名重命名為指定的表名。
        /// </summary>
        /// <param name="type">要重命名的類型。</param>
        /// <param name="tableName">新的表名。</param>
        /// <returns>返回更新後的查詢集。</returns>
        IQuerySet<T> ResetTableName(Type type, string tableName);

        /// <summary>
        ///     不鎖表查詢（此方法只支持 Mssql）。
        ///     啟用此選項可以在查詢時避免鎖定表。
        /// </summary>
        /// <returns>返回更新後的查詢集。</returns>
        IQuerySet<T> WithNoLock();

        /// <summary>
        ///     字段匹配（適用於實體類字段和數據庫字段不一致時，返回值為 Dynamic 類型時不適用）。
        ///     用於實體類和數據庫字段不一致的情況，進行字段間的匹配。
        /// </summary>
        /// <returns>返回更新後的查詢集。</returns>
        [Obsolete]
        IQuerySet<T> FieldMatch<TSource>();

        /// <summary>
        ///     返回對應行數數據。
        ///     限制查詢結果的最大行數。
        /// </summary>
        /// <param name="num">行數限制。</param>
        /// <returns>返回更新後的查詢集。</returns>
        IQuerySet<T> Top(int num);

        /// <summary>
        ///     去重。
        ///     對查詢結果進行去重處理。
        /// </summary>
        /// <returns>返回更新後的查詢集。</returns>
        IQuerySet<T> Distinct();


        /// <summary>
        ///     進行連表查詢。
        /// </summary>
        /// <typeparam name="TWhere">條件表達式型別。</typeparam>
        /// <typeparam name="TInner">內部表達式型別。</typeparam>
        /// <param name="rightField">右表欄位。</param>
        /// <param name="leftField">左表欄位。</param>
        /// <param name="joinMode">連接模式，預設為左連接（LEFT JOIN）。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Join<TWhere, TInner>(Expression<Func<TWhere, object>> rightField,
            Expression<Func<TInner, object>> leftField, JoinMode joinMode = JoinMode.LEFT);

        /// <summary>
        ///     進行連表查詢（使用自定義條件）。
        /// </summary>
        /// <typeparam name="TWhere">主表型別。</typeparam>
        /// <typeparam name="TInner">副表型別。</typeparam>
        /// <param name="exp">查詢條件表達式。</param>
        /// <param name="joinMode">連接模式，預設為左連接（LEFT JOIN）。</param>
        /// <param name="isDisField">是否顯示欄位，預設為 true。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Join<TWhere, TInner>(Expression<Func<TWhere, TInner, bool>> exp, JoinMode joinMode = JoinMode.LEFT,
            bool isDisField = true);

        /// <summary>
        ///     進行多表連表查詢。
        /// </summary>
        /// <typeparam name="TWhere">第一個條件表達式型別。</typeparam>
        /// <typeparam name="TInner">第二個內部表達式型別。</typeparam>
        /// <typeparam name="TWhere2">第三個條件表達式型別。</typeparam>
        /// <param name="expression">查詢條件表達式。</param>
        /// <param name="joinMode">連接模式，預設為左連接（LEFT JOIN）。</param>
        /// <param name="isDisField">是否顯示欄位，預設為 true。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Join<TWhere, TInner, TWhere2>(Expression<Func<TWhere, TInner, TWhere2, bool>> expression,
            JoinMode joinMode = JoinMode.LEFT, bool isDisField = true);

        /// <summary>
        ///     進行多表連表查詢。
        /// </summary>
        /// <typeparam name="TWhere">第一個條件表達式型別。</typeparam>
        /// <typeparam name="TInner">第二個內部表達式型別。</typeparam>
        /// <typeparam name="TWhere2">第三個條件表達式型別。</typeparam>
        /// <typeparam name="TWhere3">第四個條件表達式型別。</typeparam>
        /// <param name="expression">查詢條件表達式。</param>
        /// <param name="joinMode">連接模式，預設為左連接（LEFT JOIN）。</param>
        /// <param name="isDisField">是否顯示欄位，預設為 true。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Join<TWhere, TInner, TWhere2, TWhere3>(
            Expression<Func<TWhere, TInner, TWhere2, TWhere3, bool>> expression, JoinMode joinMode = JoinMode.LEFT,
            bool isDisField = true);

        /// <summary>
        ///     通過 SQL 語句進行連表查詢，不指定表實體不增加該表顯示欄位。
        /// </summary>
        /// <param name="sqlJoin">SQL 連接語句。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Join(string sqlJoin);

        /// <summary>
        ///     通過 SQL 語句進行連表查詢，指定表實體增加該表顯示欄位。
        /// </summary>
        /// <typeparam name="TInner">內部表型別。</typeparam>
        /// <param name="sqlJoin">SQL 連接語句。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Join<TInner>(string sqlJoin);


        /// <summary>
        ///     進行分組查詢。
        /// </summary>
        /// <param name="groupByExp">分組表達式。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> GroupBy(Expression<Func<T, object>> groupByExp);

        /// <summary>
        ///     進行分組查詢（指定表）。
        /// </summary>
        /// <typeparam name="TGroup">分組的型別。</typeparam>
        /// <param name="groupByExp">分組表達式。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> GroupBy<TGroup>(Expression<Func<TGroup, object>> groupByExp);

        /// <summary>
        ///     進行條件分組查詢。
        /// </summary>
        /// <typeparam name="TGroup">分組的型別。</typeparam>
        /// <param name="where">條件判斷。</param>
        /// <param name="trueGroupByExp">滿足條件時的分組表達式。</param>
        /// <param name="falseGroupByExp">不滿足條件時的分組表達式。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> GroupByIf<TGroup>(bool where, Expression<Func<TGroup, object>> trueGroupByExp,
            Expression<Func<TGroup, object>> falseGroupByExp);

        /// <summary>
        ///     設定分組後的聚合條件。
        /// </summary>
        /// <param name="havingExp">聚合條件表達式。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Having(Expression<Func<T, object>> havingExp);

        /// <summary>
        ///     設定分組後的聚合條件（指定表）。
        /// </summary>
        /// <typeparam name="THaving">聚合條件的型別。</typeparam>
        /// <param name="havingExp">聚合條件表達式。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> Having<THaving>(Expression<Func<THaving, object>> havingExp);

        /// <summary>
        ///     設定條件分組後的聚合條件。
        /// </summary>
        /// <typeparam name="THaving">聚合條件的型別。</typeparam>
        /// <param name="where">條件判斷。</param>
        /// <param name="trueHavingExp">滿足條件時的聚合條件表達式。</param>
        /// <param name="falseHavingExp">不滿足條件時的聚合條件表達式。</param>
        /// <returns>返回查詢集合。</returns>
        IQuerySet<T> HavingIf<THaving>(bool where, Expression<Func<THaving, object>> trueHavingExp,
            Expression<Func<THaving, object>> falseHavingExp);
    }
}