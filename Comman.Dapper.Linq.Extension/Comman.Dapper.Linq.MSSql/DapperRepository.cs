using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Comman.Dapper.Linq.Extension.Core.Interfaces;
using Comman.Dapper.Linq.Extension.Dapper;
using Comman.Dapper.Linq.Extension.Extension.From;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Comman.Dapper.Linq.MSSql
{
    public class DapperRepository<T> where T:IBaseEntity
    {
        private readonly IDbTransaction _transaction;
        private readonly IDbConnection _connection;
        public IQuerySet<T> Query { get; private set; }
        public ICommandSet<T> Command { get;private set; }

        public DapperRepository(IDbConnection connection)
        {
            this._connection = connection;
            Query = _connection.QuerySet<T>();
            Command = _connection.CommandSet<T>();
        }

        public DapperRepository(IDbConnection connection, IDbTransaction transaction)
        {
            this._transaction = transaction;
            this._connection = connection;
            Query = _connection.QuerySet<T>(_transaction);
            Command = _connection.CommandSet<T>(_transaction);
        }
        
        public string QuerySqlString()
        {
            return GetSqlString(Query);
        }



        public string CommandSqlString()
        {
            return GetSqlString(Command);
        }

        private string GetSqlString(object queryOrCommand)
        {
            try
            {
                var sqlProviderField = queryOrCommand.GetType().GetField("SqlProvider");
                if (sqlProviderField == null)
                {
                    throw new InvalidOperationException("Cannot find the SqlProvider field.");
                }

                var sqlProvider = (MsSqlProvider)sqlProviderField.GetValue(queryOrCommand);
                return GetParametersWithValues(sqlProvider.Params, sqlProvider.SqlString);
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        private string GetParametersWithValues(DynamicParameters sqlProviderParams, string sqlProviderSqlString)
        {
            StringBuilder sb = new StringBuilder(sqlProviderSqlString + ";");

            var dic = ObjectToDictionary(sqlProviderParams.GetType().GetField("parameters", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(sqlProviderParams));
            foreach (var item in dic.Values)
            {
                sb.AppendLine();
                sb.AppendFormat("{0} : {1}", item["Name"], item["Value"]);
            }

            return sb.ToString();
        }

        private Dictionary<string, JObject> ObjectToDictionary(object obj) => JsonConvert.DeserializeObject<Dictionary<string, JObject>>(JsonConvert.SerializeObject(obj));
    }
}