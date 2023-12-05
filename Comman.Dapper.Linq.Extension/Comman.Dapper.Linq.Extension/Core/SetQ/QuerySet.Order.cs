using System;
using System.Linq.Expressions;
using Kogel.Dapper.Extension.Core.Interfaces;
using Kogel.Dapper.Extension.Entites;

namespace Kogel.Dapper.Extension.Core.SetQ
{
    /// <summary>
    ///     排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class QuerySet<T> : Aggregation<T>, Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T>
    {
        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> OrderBy<TProperty>(Expression<Func<TProperty, object>> field)
        {
            if (field != null)
                OrderbyExpressionList.Add(field, EOrderBy.Asc);
            return this;
        }

        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> OrderBy(Expression<Func<T, object>> field)
        {
            if (field != null)
                OrderbyExpressionList.Add(field, EOrderBy.Asc);
            return this;
        }

        /// <inheritdoc />
        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> OrderByDescing<TProperty>(Expression<Func<TProperty, object>> field)
        {
            if (field != null)
                OrderbyExpressionList.Add(field, EOrderBy.Desc);
            return this;
        }

        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> OrderByDescing(Expression<Func<T, object>> field)
        {
            if (field != null)
                OrderbyExpressionList.Add(field, EOrderBy.Desc);
            return this;
        }

        public Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T> OrderBy(string orderBy)
        {
            if (!string.IsNullOrEmpty(orderBy))
                OrderbyBuilder.Append(orderBy);
            return this;
        }
    }
}