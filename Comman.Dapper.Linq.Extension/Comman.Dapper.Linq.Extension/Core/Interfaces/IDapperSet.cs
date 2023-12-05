using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Comman.Dapper.Linq.Extension.Entites;
using Kogel.Dapper.Extension;
using DynamicParameters = Comman.Dapper.Linq.Extension.Dapper.DynamicParameters;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    /// <summary>
    /// 定義抽象設置類別的基本結構。
    /// </summary>
    public abstract class AbstractSet
    {
        /// <summary>
        /// 數據解析提供方。
        /// </summary>
        public SqlProvider SqlProvider;

        /// <summary>
        /// 表類型。
        /// </summary>
        public Type TableType { get; set; }

        /// <summary>
        /// [已棄用]只用來生成對象。
        /// </summary>
        internal LambdaExpression WhereExpression { get; set; }

        /// <summary>
        /// 條件表達式對象集合。
        /// </summary>
        internal List<LambdaExpression> WhereExpressionList { get; set; }

        /// <summary>
        /// 表達式排序集合。
        /// </summary>
        internal Dictionary<LambdaExpression, EOrderBy> OrderbyExpressionList { get; set; }

        /// <summary>
        /// 字符串排序。
        /// </summary>
        internal StringBuilder OrderbyBuilder { get; set; }

        /// <summary>
        /// 字段查詢對象。
        /// </summary>
        public LambdaExpression SelectExpression { get; set; }

        /// <summary>
        /// 是否鎖表（with(nolock)）。
        /// </summary>
        public bool NoLock { get; set; }

        /// <summary>
        /// SQL 字符串對象。
        /// </summary>
        internal StringBuilder WhereBuilder { get; set; }

        /// <summary>
        /// SQL 參數對象。
        /// </summary>
        internal DynamicParameters Params
        {
            get => SqlProvider.Params;
            set => SqlProvider.Params.AddDynamicParams(value);
        }

        /// <summary>
        /// 分組表達式對象集合。
        /// </summary>
        internal List<LambdaExpression> GroupExpressionList { get; set; }

        /// <summary>
        /// 分組聚合條件。
        /// </summary>
        internal List<LambdaExpression> HavingExpressionList { get; set; }

        /// <summary>
        /// 是否去重。
        /// </summary>
        public bool IsDistinct { get; set; }

        /// <summary>
        /// 是否排除在工作單位外。
        /// </summary>
        public bool IsExcludeUnitOfWork { get; set; }

        /// <summary>
        /// 返回行數。
        /// </summary>
        public int? TopNum { get; set; }
    }
}
