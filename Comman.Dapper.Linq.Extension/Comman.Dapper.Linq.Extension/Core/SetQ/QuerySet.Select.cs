using System;
using System.Data;
using System.Linq.Expressions;
using Comman.Dapper.Linq.Extension;
using Comman.Dapper.Linq.Extension.Core.Interfaces;
using Kogel.Dapper.Extension.Core.Interfaces;

namespace Kogel.Dapper.Extension.Core.SetQ
{
    public class QuerySet<T, TReturn> : QuerySet<T>, IQuerySet<T, TReturn>
    {
        public QuerySet(IDbConnection conn, SqlProvider sqlProvider) : base(conn, sqlProvider)
        {
        }

        public QuerySet(IDbConnection conn, SqlProvider sqlProvider, IDbTransaction dbTransaction) : base(conn,
            sqlProvider, dbTransaction)
        {
        }

        public IQuery<T, TReturn> Select(Expression<Func<T, TReturn>> select)
        {
            SqlProvider.Context.Set.SelectExpression = select;
            return this as IQuery<T, TReturn>;
        }
    }
}