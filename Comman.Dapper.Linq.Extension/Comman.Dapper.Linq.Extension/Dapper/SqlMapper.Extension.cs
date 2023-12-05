using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Comman.Dapper.Linq.Extension.Entites;
using Comman.Dapper.Linq.Extension.Extension;
using Comman.Dapper.Linq.Extension.Extension.From;
using Comman.Dapper.Linq.Extension.Helper.Cache;
using static Comman.Dapper.Linq.Extension.Dapper.SqlMapper;

namespace Comman.Dapper.Linq.Extension.Dapper
{
    public static class SqlMapperExtension
    {
        /// <summary>
        ///     獲取拆分字段。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <param name="provider">SQL 提供者。</param>
        /// <returns>拆分字段字符串。</returns>
        private static string GetSplitOn<T>(SqlProvider provider)
        {
            var splitOn = string.Empty;
            var navigationList = provider.JoinList.Where(x => x.Action == JoinAction.Navigation).ToList();
            if (navigationList.Any()) splitOn = string.Join(",", navigationList.Select(x => x.FirstFieldName));
            return splitOn;
        }

        /// <summary>
        ///     查詢帶聚合導航屬性的對象集合。
        /// </summary>
        /// <typeparam name="TFirst">第一個實體類型。</typeparam>
        /// <typeparam name="TSecond">第二個實體類型。</typeparam>
        /// <param name="cnn">資料庫連接。</param>
        /// <param name="provider">SQL 提供者。</param>
        /// <param name="transaction">事務處理。</param>
        /// <returns>查詢的第一個對象。</returns>
        public static TFirst QueryFirstOrDefault<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(
            this IDbConnection cnn, SqlProvider provider, IDbTransaction transaction = null)
            where TFirst : IBaseEntity
            where TSecond : IBaseEntity
            where TThird : IBaseEntity
            where TFourth : IBaseEntity
            where TFifth : IBaseEntity
            where TSixth : IBaseEntity
            where TSeventh : IBaseEntity
        {
            return cnn.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(provider, transaction)
                .FirstOrDefault();
        }

        /// <summary>
        ///     異步查詢帶聚合導航屬性的對象集合。
        /// </summary>
        /// <typeparam name="TFirst">第一個實體類型。</typeparam>
        /// <typeparam name="TSecond">第二個實體類型。</typeparam>
        /// <param name="cnn">資料庫連接。</param>
        /// <param name="provider">SQL 提供者。</param>
        /// <param name="transaction">事務處理。</param>
        /// <returns>異步操作結果。</returns>
        public static async Task<TFirst> QueryFirstOrDefaultAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth,
            TSeventh>(this IDbConnection cnn, SqlProvider provider, IDbTransaction transaction = null)
            where TFirst : IBaseEntity
            where TSecond : IBaseEntity
            where TThird : IBaseEntity
            where TFourth : IBaseEntity
            where TFifth : IBaseEntity
            where TSixth : IBaseEntity
            where TSeventh : IBaseEntity
        {
            return (await cnn.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(provider,
                transaction)).FirstOrDefault();
        }


        /// <summary>
        ///     查詢帶聚合導航屬性的對象集合。
        /// </summary>
        /// <typeparam name="TFirst">第一個實體類型。</typeparam>
        /// <typeparam name="TSecond">第二個實體類型。</typeparam>
        /// <typeparam name="TThird">第三個實體類型。</typeparam>
        /// <typeparam name="TFourth">第四個實體類型。</typeparam>
        /// <typeparam name="TFifth">第五個實體類型。</typeparam>
        /// <typeparam name="TSixth">第六個實體類型。</typeparam>
        /// <typeparam name="TSeventh">第七個實體類型。</typeparam>
        /// <param name="cnn">資料庫連接。</param>
        /// <param name="provider">SQL 提供者。</param>
        /// <param name="transaction">事務處理。</param>
        /// <returns>對象集合。</returns>
        public static IEnumerable<TFirst> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(
            this IDbConnection cnn, SqlProvider provider, IDbTransaction transaction = null)
            where TFirst : IBaseEntity
            where TSecond : IBaseEntity
            where TThird : IBaseEntity
            where TFourth : IBaseEntity
            where TFifth : IBaseEntity
            where TSixth : IBaseEntity
            where TSeventh : IBaseEntity
        {
            var firstEntity = EntityCache.QueryEntity(typeof(TFirst));
            var splitOn = GetSplitOn<TFirst>(provider);
            if (!string.IsNullOrEmpty(splitOn))
            {
                var navigationList = provider.JoinList
                    .Where(x => x.Action == JoinAction.Navigation && x.IsMapperField)
                    .ToList();

                var firsts = new List<TFirst>();
                var seconds = new List<TSecond>();
                var thirds = new List<TThird>();
                var fourths = new List<TFourth>();
                var fifths = new List<TFifth>();
                var sixths = new List<TSixth>();
                var sevenths = new List<TSeventh>();

                cnn.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TFirst>(
                    provider.SqlString,
                    (first, second, third, fourth, fifth, sixth, seventh) =>
                    {
                        firsts.Add(first);
                        if (second != null) seconds.Add(second);
                        if (third != null) thirds.Add(third);
                        if (fourth != null) fourths.Add(fourth);
                        if (fifth != null) fifths.Add(fifth);
                        if (sixth != null) sixths.Add(sixth);
                        if (seventh != null) sevenths.Add(seventh);

                        return default;
                    },
                    provider.Params,
                    transaction,
                    true,
                    splitOn
                );

                firsts = ExcisionData(firsts, seconds, thirds, fourths, fifths, sixths, sevenths, navigationList);
                return firsts;
            }

            return cnn.Query<TFirst>(provider.SqlString, provider.Params, transaction);
        }


        /// <summary>
        ///     分割導航屬性數據。
        /// </summary>
        /// <typeparam name="TFirst">第一個實體類型。</typeparam>
        /// <typeparam name="TSecond">第二個實體類型。</typeparam>
        /// <typeparam name="TThird">第三個實體類型。</typeparam>
        /// <typeparam name="TFourth">第四個實體類型。</typeparam>
        /// <typeparam name="TFifth">第五個實體類型。</typeparam>
        /// <typeparam name="TSixth">第六個實體類型。</typeparam>
        /// <typeparam name="TSeventh">第七個實體類型。</typeparam>
        /// <param name="firsts">第一個實體列表。</param>
        /// <param name="seconds">第二個實體列表。</param>
        /// <param name="thirds">第三個實體列表。</param>
        /// <param name="fourths">第四個實體列表。</param>
        /// <param name="fifths">第五個實體列表。</param>
        /// <param name="sixths">第六個實體列表。</param>
        /// <param name="sevenths">第七個實體列表。</param>
        /// <param name="joinAssTables">連接輔助表列表。</param>
        /// <returns>處理後的第一個實體列表。</returns>
        private static List<TFirst> ExcisionData<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(
            List<TFirst> firsts, List<TSecond> seconds, List<TThird> thirds,
            List<TFourth> fourths, List<TFifth> fifths, List<TSixth> sixths, List<TSeventh> sevenths,
            List<JoinAssTable> joinAssTables)
            where TFirst : IBaseEntity
            where TSecond : IBaseEntity
            where TThird : IBaseEntity
            where TFourth : IBaseEntity
            where TFifth : IBaseEntity
            where TSixth : IBaseEntity
            where TSeventh : IBaseEntity
        {
            var firstList = new List<TFirst>();
            var secondList = new List<TSecond>();
            var thirdList = new List<TThird>();
            var fourthList = new List<TFourth>();
            var fifthList = new List<TFifth>();
            var sixthList = new List<TSixth>();
            var seventhList = new List<TSeventh>();
            for (var i = 0; i < firsts.Count; i++)
            {
                var first = firstList.Find(x => x.GetId().Equals(firsts[i].GetId()));
                if (first == null)
                {
                    first = firsts[i];
                    firstList.Add(first);
                    //重置关联数据
                    secondList.Clear();
                    thirdList.Clear();
                    fourthList.Clear();
                    fifthList.Clear();
                    sixthList.Clear();
                    seventhList.Clear();
                }


                if (seconds.Count > i)
                {
                    var second = secondList.Find(x => x.GetId().Equals(seconds[i].GetId()));
                    if (second == null)
                    {
                        second = seconds[i];
                        secondList.Add(second);
                        ExpressionExtension.SetProperValue(first, second, joinAssTables[0].PropertyInfo);

                        thirdList.Clear();
                        fourthList.Clear();
                        fifthList.Clear();
                        sixthList.Clear();
                        seventhList.Clear();
                    }


                    if (thirds.Count > i)
                    {
                        var third = thirdList.Find(x => x.GetId().Equals(thirds[i].GetId()));
                        if (third == null)
                        {
                            third = thirds[i];
                            thirdList.Add(third);
                            //然后查找导航属性父级实体信息位置
                            var parentIndex = joinAssTables.FindIndex(x =>
                                x.TableType == joinAssTables[1].PropertyInfo.DeclaringType) + 1;
                            switch (parentIndex)
                            {
                                case 0:
                                    ExpressionExtension.SetProperValue(first, third, joinAssTables[1].PropertyInfo);
                                    break;
                                case 1:
                                    ExpressionExtension.SetProperValue(second, third, joinAssTables[1].PropertyInfo);
                                    break;
                            }
                            //重置关联数据
                            fourthList.Clear();
                            fifthList.Clear();
                            sixthList.Clear();
                            seventhList.Clear();
                        }

                        //设置第三个导航属性
                        if (fourths.Count > i)
                        {
                            var fourth = fourthList.Find(x => x.GetId().Equals(fourths[i].GetId()));
                            if (fourth == null)
                            {
                                fourth = fourths[i];
                                fourthList.Add(fourth);

                                var parentIndex = joinAssTables.FindIndex(x =>
                                    x.TableType == joinAssTables[2].PropertyInfo.DeclaringType) + 1;
                                switch (parentIndex)
                                {
                                    case 0:
                                        ExpressionExtension.SetProperValue(first, fourth, joinAssTables[2].PropertyInfo);
                                        break;
                                    case 1:
                                        ExpressionExtension.SetProperValue(second, fourth, joinAssTables[2].PropertyInfo);
                                        break;
                                    case 2:
                                        ExpressionExtension.SetProperValue(third, fourth, joinAssTables[2].PropertyInfo);
                                        break;
                                }

                                fifthList.Clear();
                                sixthList.Clear();
                                seventhList.Clear();
                            }


                            if (fifths.Count > i)
                            {
                                var fifth = fifthList.Find(x => x.GetId().Equals(fifths[i].GetId()));
                                if (fifth == null)
                                {
                                    fifth = fifths[i];
                                    fifthList.Add(fifth);

                                    var parentIndex = joinAssTables.FindIndex(x =>
                                        x.TableType == joinAssTables[3].PropertyInfo.DeclaringType) + 1;
                                    switch (parentIndex)
                                    {
                                        case 0:
                                            ExpressionExtension.SetProperValue(first, fifth, joinAssTables[3].PropertyInfo);
                                            break;
                                        case 1:
                                            ExpressionExtension.SetProperValue(second, fifth,
                                                joinAssTables[3].PropertyInfo);
                                            break;
                                        case 2:
                                            ExpressionExtension.SetProperValue(third, fifth, joinAssTables[3].PropertyInfo);
                                            break;
                                        case 3:
                                            ExpressionExtension.SetProperValue(fourth, fifth,
                                                joinAssTables[3].PropertyInfo);
                                            break;
                                    }

                                    sixthList.Clear();
                                    seventhList.Clear();
                                }


                                if (sixths.Count > i)
                                {
                                    var sixth = sixthList.Find(x => x.GetId().Equals(sixths[i].GetId()));
                                    if (sixth == null)
                                    {
                                        sixth = sixths[i];
                                        sixthList.Add(sixth);

                                        var parentIndex = joinAssTables.FindIndex(x =>
                                            x.TableType == joinAssTables[4].PropertyInfo.DeclaringType) + 1;
                                        switch (parentIndex)
                                        {
                                            case 0:
                                                ExpressionExtension.SetProperValue(first, sixth,
                                                    joinAssTables[4].PropertyInfo);
                                                break;
                                            case 1:
                                                ExpressionExtension.SetProperValue(second, sixth,
                                                    joinAssTables[4].PropertyInfo);
                                                break;
                                            case 2:
                                                ExpressionExtension.SetProperValue(third, sixth,
                                                    joinAssTables[4].PropertyInfo);
                                                break;
                                            case 3:
                                                ExpressionExtension.SetProperValue(fourth, sixth,
                                                    joinAssTables[4].PropertyInfo);
                                                break;
                                            case 4:
                                                ExpressionExtension.SetProperValue(fifth, sixth,
                                                    joinAssTables[4].PropertyInfo);
                                                break;
                                        }

                                        seventhList.Clear();
                                    }


                                    if (sevenths.Count > i)
                                    {
                                        var seventh = seventhList.Find(x => x.GetId().Equals(sevenths[i].GetId()));
                                        if (seventh == null)
                                        {
                                            seventh = sevenths[i];
                                            seventhList.Add(seventh);
                                            //然后查找导航属性父级实体信息位置
                                            var parentIndex = joinAssTables.FindIndex(x =>
                                                x.TableType == joinAssTables[5].PropertyInfo.DeclaringType) + 1;
                                            switch (parentIndex)
                                            {
                                                case 0:
                                                    ExpressionExtension.SetProperValue(first, seventh,
                                                        joinAssTables[5].PropertyInfo);
                                                    break;
                                                case 1:
                                                    ExpressionExtension.SetProperValue(second, seventh,
                                                        joinAssTables[5].PropertyInfo);
                                                    break;
                                                case 2:
                                                    ExpressionExtension.SetProperValue(third, seventh,
                                                        joinAssTables[5].PropertyInfo);
                                                    break;
                                                case 3:
                                                    ExpressionExtension.SetProperValue(fourth, seventh,
                                                        joinAssTables[5].PropertyInfo);
                                                    break;
                                                case 4:
                                                    ExpressionExtension.SetProperValue(fifth, seventh,
                                                        joinAssTables[5].PropertyInfo);
                                                    break;
                                                case 5:
                                                    ExpressionExtension.SetProperValue(sixth, seventh,
                                                        joinAssTables[5].PropertyInfo);
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return firstList;
        }

        /// <summary>
        ///     异步查询带聚合导航属性的对象集合
        /// </summary>
        /// <typeparam name="TFirst"></typeparam>
        /// <typeparam name="TSecond"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="provider"></param>
        /// <param name="transaction"></param>
        /// <param name="splitOn"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<TFirst>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth,
            TSeventh>(this IDbConnection cnn, SqlProvider provider, IDbTransaction transaction = null)
            where TFirst : IBaseEntity
            where TSecond : IBaseEntity
            where TThird : IBaseEntity
            where TFourth : IBaseEntity
            where TFifth : IBaseEntity
            where TSixth : IBaseEntity
            where TSeventh : IBaseEntity
        {
            var firstEntity = EntityCache.QueryEntity(typeof(TFirst));
            var splitOn = GetSplitOn<TFirst>(provider);
            if (!string.IsNullOrEmpty(splitOn))
            {
                var navigationList = provider.JoinList.Where(x => x.Action == JoinAction.Navigation && x.IsMapperField)
                    .ToList();

                var firsts = new List<TFirst>();
                var seconds = new List<TSecond>();
                var thirds = new List<TThird>();
                var fourths = new List<TFourth>();
                var fifths = new List<TFifth>();
                var sixths = new List<TSixth>();
                var sevenths = new List<TSeventh>();
                await cnn.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TFirst>(
                    provider.SqlString, (first, second, third, fourth, fifth, sixth, seventh) =>
                    {
                        firsts.Add(first);
                        if (second != null)
                        {
                            seconds.Add(second);
                            if (third != null)
                            {
                                thirds.Add(third);
                                if (fourth != null)
                                {
                                    fourths.Add(fourth);
                                    if (fifth != null)
                                    {
                                        fifths.Add(fifth);
                                        if (sixth != null)
                                        {
                                            sixths.Add(sixth);
                                            if (seventh != null)
                                                sevenths.Add(seventh);
                                        }
                                    }
                                }
                            }
                        }

                        return default;
                    }, provider.Params, transaction, true, splitOn);
                //分割导航属性数据
                firsts = ExcisionData(firsts, seconds, thirds, fourths, fifths, sixths, sevenths, navigationList);
                return firsts;
            }

            return await cnn.QueryAsync<TFirst>(provider.SqlString, provider.Params, transaction);
        }

        /// <summary>
        ///     查詢返回 DataSet。
        /// </summary>
        /// <param name="cnn">資料庫連接。</param>
        /// <param name="adapter">數據適配器。</param>
        /// <param name="sql">SQL 查詢語句。</param>
        /// <param name="param">參數。</param>
        /// <param name="transaction">事務處理。</param>
        /// <param name="buffered">是否緩存。</param>
        /// <param name="commandTimeout">命令超時時間。</param>
        /// <param name="commandType">命令類型。</param>
        /// <param name="isExcludeUnitOfWork">是否排除單元工作。</param>
        /// <returns>DataSet。</returns>
        public static DataSet QueryDataSet(this IDbConnection cnn, IDbDataAdapter adapter, string sql,
            object param = null, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null,
            bool isExcludeUnitOfWork = false)
        {
            var ds = new DataSet();
            var command = new CommandDefinition(cnn, sql, param, transaction, commandTimeout, commandType,
                buffered ? CommandFlags.Buffered : CommandFlags.None, isExcludeUnitOfWork: isExcludeUnitOfWork);
            try
            {
                Aop.InvokeExecuting(ref command);
                var identity = new Identity(command.CommandText, command.CommandType, cnn, null,
                    param == null ? null : param.GetType(), null);
                var info = GetCacheInfo(identity, param, command.AddToCache);
                var wasClosed = cnn.State == ConnectionState.Closed;
                if (wasClosed) cnn.Open();
                adapter.SelectCommand = command.SetupCommand(cnn, info.ParamReader);
                adapter.Fill(ds);
                if (wasClosed) cnn.Close();
                return ds;
            }
            finally
            {
                Aop.InvokeExecuted(ref command);
            }
        }

        /// <summary>
        ///     異步查詢返回 DataSet。
        /// </summary>
        /// <param name="cnn">資料庫連接。</param>
        /// <param name="adapter">數據適配器。</param>
        /// <param name="sql">SQL 查詢語句。</param>
        /// <param name="param">參數。</param>
        /// <param name="transaction">事務處理。</param>
        /// <param name="buffered">是否緩存。</param>
        /// <param name="commandTimeout">命令超時時間。</param>
        /// <param name="commandType">命令類型。</param>
        /// <param name="isExcludeUnitOfWork">是否排除單元工作。</param>
        /// <returns>異步操作結果。</returns>
        public static async Task<DataSet> QueryDataSetAsync(this IDbConnection cnn, IDbDataAdapter adapter, string sql,
            object param = null,
            IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null,
            CommandType? commandType = null, bool isExcludeUnitOfWork = false)
        {
            var ds = new DataSet();
            var command = new CommandDefinition(cnn, sql, param, transaction, commandTimeout, commandType,
                buffered ? CommandFlags.Buffered : CommandFlags.None, isExcludeUnitOfWork: isExcludeUnitOfWork);
            try
            {
                Aop.InvokeExecuting(ref command);
                var identity = new Identity(command.CommandText, command.CommandType, cnn, null,
                    param == null ? null : param.GetType(), null);
                var info = GetCacheInfo(identity, param, command.AddToCache);
                var wasClosed = cnn.State == ConnectionState.Closed;
                var cancel = command.CancellationToken;
                if (wasClosed) await cnn.TryOpenAsync(cancel).ConfigureAwait(false);
                adapter.SelectCommand = command.SetupCommand(cnn, info.ParamReader);
                adapter.Fill(ds);
                if (wasClosed) cnn.Close();
                return ds;
            }
            finally
            {
                Aop.InvokeExecuted(ref command);
            }
        }

        /// <summary>
        ///     更新操作。
        /// </summary>
        /// <typeparam name="T">實體類型。</typeparam>
        /// <param name="cnn">資料庫連接。</param>
        /// <param name="sql">更新 SQL 語句。</param>
        /// <param name="parameters">參數集合。</param>
        /// <param name="adapter">數據適配器。</param>
        /// <param name="entites">實體集合。</param>
        /// <param name="provider">SQL 提供者。</param>
        /// <param name="transaction">事務處理。</param>
        /// <param name="buffered">是否緩存。</param>
        /// <param name="commandTimeout">命令超時時間。</param>
        /// <param name="commandType">命令類型。</param>
        /// <param name="isExcludeUnitOfWork">是否排除單元工作。</param>
        /// <returns>影響的行數。</returns>
        public static int Update<T>(this IDbConnection cnn, string sql,
            DynamicParameters parameters, IDbDataAdapter adapter, IEnumerable<T> entites,
            SqlProvider provider, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null,
            bool isExcludeUnitOfWork = false)
        {
            var command = new CommandDefinition(cnn, sql, parameters, transaction, commandTimeout, commandType,
                buffered ? CommandFlags.Buffered : CommandFlags.None, isExcludeUnitOfWork: isExcludeUnitOfWork);
            try
            {
                Aop.InvokeExecuting(ref command);
                var entityObject = EntityCache.QueryEntity(typeof(T));
                var wasClosed = command.Connection.State == ConnectionState.Closed;
                if (wasClosed) command.Connection.Open();

                var selectSqlBuild = new StringBuilder();
                var selectParam = new DynamicParameters();
                var updateCommand = command.Connection.CreateCommand();
                updateCommand.CommandText = command.CommandText;
                updateCommand.Transaction = command.Transaction;
                adapter.UpdateCommand = updateCommand;
                var updateParam = (DynamicParameters)command.Parameters;
                foreach (var paramName in updateParam.ParameterNames)
                {
                    var parameter = updateCommand.CreateParameter();
                    parameter.ParameterName = $"@{paramName}";
                    parameter.SourceColumn = paramName;
                    updateCommand.Parameters.Add(parameter);

                    if (selectSqlBuild.Length != 0) selectSqlBuild.Append(",");
                    selectSqlBuild.Append(paramName);

                    if (updateCommand.Parameters.Count == updateParam.ParameterNames.Count())
                    {
                        var selectWhereSql = provider.GetIdentityWhere(entites, selectParam);
                        var tableName = provider.ProviderOption.CombineFieldName(entityObject.Name);
                        selectSqlBuild.Append($" FROM {tableName} WHERE 1=1 {selectWhereSql} ");
                    }
                }

                var ds = QueryDataSet(command.Connection, adapter, $"SELECT {selectSqlBuild}", selectParam,
                    command.Transaction, command.Buffered, command.CommandTimeout, command.CommandType,
                    command.IsExcludeUnitOfWork);
                ds.UpdateDataSet(entites);
                adapter.Update(ds);

                if (wasClosed) command.Connection.Close();
                return ds.Tables[0].Rows.Count;
            }
            finally
            {
                Aop.InvokeExecuted(ref command);
            }
        }
    }

    public class GuidTypeHandler : TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Size = 10;
            parameter.DbType = DbType.String;
            //parameter.Value = value.ToString();
            parameter.Value = value;
        }

        public override Guid Parse(object value)
        {
            //return Guid.Parse((string)value);
            return Convert(value);
        }

        internal static Guid Convert(object value)
        {
            if (value.GetType().Name.Contains("Guid"))
                return (Guid)value;
            return Guid.Parse(value.ToString());
        }
    }

    public class GuidArrTypeHanlder : TypeHandler<Guid[]>
    {
        public override void SetValue(IDbDataParameter parameter, Guid[] value)
        {
            parameter.Size = 10;
            parameter.DbType = DbType.String;
            parameter.Value = value;
        }

        public override Guid[] Parse(object value)
        {
            var guids = new List<Guid>();
            foreach (var item in (string[])value)
            {
                guids.Add(GuidTypeHandler.Convert(value));
            }

            return guids.ToArray();
        }
    }
}