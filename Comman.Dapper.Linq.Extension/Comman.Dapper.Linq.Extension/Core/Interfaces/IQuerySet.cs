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
        ///     连表查询
        /// </summary>
        /// <typeparam name="TWhere"></typeparam>
        /// <typeparam name="TInner"></typeparam>
        /// <param name="rightField"></param>
        /// <param name="leftField"></param>
        /// <param name="joinMode"></param>
        /// <returns></returns>
        IQuerySet<T> Join<TWhere, TInner>(Expression<Func<TWhere, object>> rightField,
            Expression<Func<TInner, object>> leftField, JoinMode joinMode = JoinMode.LEFT);

        /// <summary>
        ///     连表查询(任意查)
        /// </summary>
        /// <typeparam name="TWhere">主表</typeparam>
        /// <typeparam name="TInner">副表</typeparam>
        /// <param name="exp"></param>
        /// <param name="joinMode">连表方式</param>
        /// <param name="isDisField">是否显示字段</param>
        /// <returns></returns>
        IQuerySet<T> Join<TWhere, TInner>(Expression<Func<TWhere, TInner, bool>> exp, JoinMode joinMode = JoinMode.LEFT,
            bool isDisField = true);

        /// <summary>
        ///     连表查询
        /// </summary>
        /// <typeparam name="TWhere"></typeparam>
        /// <typeparam name="TInner"></typeparam>
        /// <typeparam name="TWhere2"></typeparam>
        /// <param name="expression"></param>
        /// <param name="joinMode"></param>
        /// <param name="isDisField"></param>
        /// <returns></returns>
        IQuerySet<T> Join<TWhere, TInner, TWhere2>(Expression<Func<TWhere, TInner, TWhere2, bool>> expression,
            JoinMode joinMode = JoinMode.LEFT, bool isDisField = true);

        /// <summary>
        /// </summary>
        /// <typeparam name="TWhere"></typeparam>
        /// <typeparam name="TInner"></typeparam>
        /// <typeparam name="TWhere2"></typeparam>
        /// <typeparam name="TWhere3"></typeparam>
        /// <param name="expression"></param>
        /// <param name="joinMode"></param>
        /// <param name="isDisField"></param>
        /// <returns></returns>
        IQuerySet<T> Join<TWhere, TInner, TWhere2, TWhere3>(
            Expression<Func<TWhere, TInner, TWhere2, TWhere3, bool>> expression, JoinMode joinMode = JoinMode.LEFT,
            bool isDisField = true);

        /// <summary>
        ///     连表查询(通过sql连接，不指定表实体不增加该表显示字段)
        /// </summary>
        /// <returns></returns>
        IQuerySet<T> Join(string sqlJoin);

        /// <summary>
        ///     连接(通过sql连接，指定表实体增加该表显示字段)
        /// </summary>
        /// <typeparam name="TInner"></typeparam>
        /// <param name="sqlJoin"></param>
        /// <returns></returns>
        IQuerySet<T> Join<TInner>(string sqlJoin);

        /// <summary>
        ///     分组
        /// </summary>
        /// <param name="groupByExp"></param>
        /// <returns></returns>
        IQuerySet<T> GroupBy(Expression<Func<T, object>> groupByExp);

        /// <summary>
        ///     分组(根据指定表)
        /// </summary>
        /// <typeparam name="TGroup"></typeparam>
        /// <param name="groupByExp"></param>
        /// <returns></returns>
        IQuerySet<T> GroupBy<TGroup>(Expression<Func<TGroup, object>> groupByExp);

        /// <summary>
        ///     分组(带判断)
        /// </summary>
        /// <typeparam name="TGroup"></typeparam>
        /// <param name="where"></param>
        /// <param name="trueGroupByExp"></param>
        /// <param name="falseGroupByExp"></param>
        /// <returns></returns>
        IQuerySet<T> GroupByIf<TGroup>(bool where, Expression<Func<TGroup, object>> trueGroupByExp,
            Expression<Func<TGroup, object>> falseGroupByExp);

        /// <summary>
        ///     分组聚合条件
        /// </summary>
        /// <param name="havingExp"></param>
        /// <returns></returns>
        IQuerySet<T> Having(Expression<Func<T, object>> havingExp);

        /// <summary>
        ///     分组聚合条件(根据指定表)
        /// </summary>
        /// <typeparam name="THaving"></typeparam>
        /// <param name="havingExp"></param>
        /// <returns></returns>
        IQuerySet<T> Having<THaving>(Expression<Func<THaving, object>> havingExp);

        /// <summary>
        ///     分组聚合条件(带判断)
        /// </summary>
        /// <typeparam name="THaving"></typeparam>
        /// <param name="where"></param>
        /// <param name="trueHavingExp"></param>
        /// <param name="falseHavingExp"></param>
        /// <returns></returns>
        IQuerySet<T> HavingIf<THaving>(bool where, Expression<Func<THaving, object>> trueHavingExp,
            Expression<Func<THaving, object>> falseHavingExp);
    }
}