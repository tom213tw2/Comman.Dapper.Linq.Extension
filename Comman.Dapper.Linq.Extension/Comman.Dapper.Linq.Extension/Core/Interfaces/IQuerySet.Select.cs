using System;
using System.Linq.Expressions;
using Comman.Dapper.Linq.Extension.Core.Interfaces;

namespace Kogel.Dapper.Extension.Core.Interfaces
{
    public interface IQuerySet<T, TReturn> : Comman.Dapper.Linq.Extension.Core.Interfaces.IQuerySet<T>
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="select"></param>
        /// <returns></returns>
        IQuery<T, TReturn> Select(Expression<Func<T, TReturn>> select);
    }
}