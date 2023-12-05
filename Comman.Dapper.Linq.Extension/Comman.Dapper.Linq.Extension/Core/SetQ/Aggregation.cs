using System;
using System.Data;
using System.Linq.Expressions;
using Comman.Dapper.Linq.Extension.Core.Interfaces;
using Comman.Dapper.Linq.Extension.Dapper;

namespace Comman.Dapper.Linq.Extension.Core.SetQ
{
    /// <summary>
    ///     聚合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Aggregation<T> : Query<T>, IAggregation<T>
    {
        private readonly IDbTransaction dbTransaction;

        protected Aggregation(IDbConnection conn, SqlProvider sqlProvider) : base(conn, sqlProvider)
        {
        }

        protected Aggregation(IDbConnection conn, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(conn,
            sqlProvider, dbTransaction)
        {
            this.dbTransaction = dbTransaction;
        }

        /// <inheritdoc />
        public int Count()
        {
            SqlProvider.FormatCount();
            return DbCon.QuerySingleOrDefault<int>(SqlProvider.SqlString, SqlProvider.Params, dbTransaction);
        }

        /// <inheritdoc />
        public TResult Sum<TResult>(Expression<Func<T, TResult>> sumExpression)
        {
            SqlProvider.FormatSum(sumExpression);
            return DbCon.QuerySingleOrDefault<TResult>(SqlProvider.SqlString, SqlProvider.Params, dbTransaction);
        }

        public TResult Max<TResult>(Expression<Func<T, TResult>> maxExpression)
        {
            SqlProvider.FormatMax(maxExpression);
            return DbCon.QuerySingleOrDefault<TResult>(SqlProvider.SqlString, SqlProvider.Params, dbTransaction);
        }

        public TResult Min<TResult>(Expression<Func<T, TResult>> minExpression)
        {
            SqlProvider.FormatMin(minExpression);
            return DbCon.QuerySingleOrDefault<TResult>(SqlProvider.SqlString, SqlProvider.Params, dbTransaction);
        }
    }
}