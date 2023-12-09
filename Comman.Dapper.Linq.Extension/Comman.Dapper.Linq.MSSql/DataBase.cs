using System.Data;
using Comman.Dapper.Linq.Extension.Core.Interfaces;
using Comman.Dapper.Linq.Extension.Core.SetC;
using Comman.Dapper.Linq.Extension.Core.SetQ;

namespace Comman.Dapper.Linq.MSSql
{
	public static class DataBase
	{
        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public static IQuerySet<T> QuerySet<T>(this IDbConnection sqlConnection)
        {
            return new QuerySet<T>(sqlConnection, new MsSqlProvider());
        }

        /// <summary>
        /// 查询(带事务)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlConnection"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public static IQuerySet<T> QuerySet<T>(this IDbConnection sqlConnection, IDbTransaction dbTransaction)
        {
            return new QuerySet<T>(sqlConnection, new MsSqlProvider(), dbTransaction);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public static ICommandSet<T> CommandSet<T>(this IDbConnection sqlConnection)
        {
            return new CommandSet<T>(sqlConnection, new MsSqlProvider());
        }

        /// <summary>
        /// 编辑(带事务)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlConnection"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        public static ICommandSet<T> CommandSet<T>(this IDbConnection sqlConnection, IDbTransaction dbTransaction)
        {
           
            return new CommandSet<T>(sqlConnection, new MsSqlProvider(), dbTransaction);
        }
    }
}
