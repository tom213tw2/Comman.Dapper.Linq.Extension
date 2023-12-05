using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Comman.Dapper.Linq.Extension.Core.Interfaces;
using Comman.Dapper.Linq.Extension.Dapper;
using Comman.Dapper.Linq.Extension.Entites;
using DynamicParameters = Comman.Dapper.Linq.Extension.Dapper.DynamicParameters;

namespace Comman.Dapper.Linq.Extension.Core.SetC
{
    /// <summary>
    ///     指令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Command<T> : AbstractSet, ICommand<T>
    {
        private readonly IDbConnection dbCon;

        protected Command(IDbConnection conn, SqlProvider sqlProvider)
        {
            SqlProvider = sqlProvider;
            dbCon = conn;
        }

        protected Command(IDbConnection conn, SqlProvider sqlProvider, IDbTransaction dbTransaction)
        {
            SqlProvider = sqlProvider;
            dbCon = conn;
            DbTransaction = dbTransaction;
        }

        private IDbTransaction DbTransaction { get; }

        protected DataBaseContext<T> SetContext { get; set; }

        public int Update(T entity, string[] excludeFields = null, int timeout = 120)
        {
            SqlProvider.FormatUpdate(entity, excludeFields);
            return dbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction, timeout,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public int Update(Expression<Func<T, T>> updateExpression)
        {
            SqlProvider.FormatUpdate(updateExpression);
            return dbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public int Update(IEnumerable<T> entities, IDbDataAdapter adapter, string[] excludeFields = null,
            int timeout = 120)
        {
            var enumerable = entities.ToList();
            SqlProvider.FormatUpdate(enumerable, excludeFields);
            return dbCon.Update(SqlProvider.SqlString, SqlProvider.Params, adapter, enumerable, SqlProvider,
                DbTransaction, isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public async Task<int> UpdateAsync(T entity, string[] excludeFields = null, int timeout = 120)
        {
            SqlProvider.FormatUpdate(entity, excludeFields);
            return await dbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public async Task<int> UpdateAsync(Expression<Func<T, T>> updateExpression)
        {
            SqlProvider.FormatUpdate(updateExpression);
            return await dbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public async Task<int> UpdateAsync(IEnumerable<T> entities, string[] excludeFields = null, int timeout = 120)
        {
            var result = 0;
            foreach (var entity in entities)
            {
                result += await UpdateAsync(entity, excludeFields, timeout);
            }

            return result;
        }

        public int Delete()
        {
            SqlProvider.FormatDelete();
            return dbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public int Delete(T model)
        {
            SqlProvider.FormatDelete();
            //设置参数
            var param = new DynamicParameters();
            SqlProvider.SqlString = $"{SqlProvider.SqlString} {SqlProvider.GetIdentityWhere(model, param)}";
            return dbCon.Execute(SqlProvider.SqlString, param, isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public async Task<int> DeleteAsync()
        {
            SqlProvider.FormatDelete();
            return await dbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public async Task<int> DeleteAsync(T model)
        {
            SqlProvider.FormatDelete();
            //设置参数
            var param = new DynamicParameters();
            SqlProvider.SqlString = $"{SqlProvider.SqlString} {SqlProvider.GetIdentityWhere(model, param)}";
            return await dbCon.ExecuteAsync(SqlProvider.SqlString, param,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public int Insert(T entity, string[] excludeFields = null)
        {
            SqlProvider.FormatInsert(entity, excludeFields);
            return dbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public long InsertIdentity(T entity, string[] excludeFields = null)
        {
            SqlProvider.FormatInsertIdentity(entity, excludeFields);
            var result = dbCon.ExecuteScalar(SqlProvider.SqlString, SqlProvider.Params, DbTransaction,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
            return result != null ? Convert.ToInt64(result) : 0;
        }

        public int Insert(IEnumerable<T> entities, string[] excludeFields = null, int timeout = 120)
        {
            SqlProvider.FormatInsert(entities, excludeFields);
            return dbCon.Execute(SqlProvider.SqlString, SqlProvider.Params, DbTransaction, timeout,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public async Task<int> InsertAsync(T entity, string[] excludeFields = null)
        {
            SqlProvider.FormatInsert(entity, excludeFields);
            return await dbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }

        public async Task<long> InsertIdentityAsync(T entity, string[] excludeFields = null)
        {
            SqlProvider.FormatInsertIdentity(entity, excludeFields);
            var result = await dbCon.ExecuteScalarAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
            return result != null ? Convert.ToInt64(result) : 0;
        }

        public async Task<int> InsertAsync(IEnumerable<T> entities, string[] excludeFields = null, int timeout = 120)
        {
            SqlProvider.FormatInsert(entities, excludeFields);
            return await dbCon.ExecuteAsync(SqlProvider.SqlString, SqlProvider.Params, DbTransaction, timeout,
                isExcludeUnitOfWork: SqlProvider.IsExcludeUnitOfWork);
        }
    }
}