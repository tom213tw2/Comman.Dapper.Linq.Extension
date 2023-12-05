using System;
using System.Linq.Expressions;

namespace Comman.Dapper.Linq.Extension.Core.Interfaces
{
    public interface IQuerySet<T, TReturn> : IQuerySet<T>
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="select"></param>
        /// <returns></returns>
        IQuery<T, TReturn> Select(Expression<Func<T, TReturn>> select);
    }
}