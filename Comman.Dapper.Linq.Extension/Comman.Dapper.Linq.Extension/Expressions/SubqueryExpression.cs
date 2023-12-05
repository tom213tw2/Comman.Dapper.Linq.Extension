using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Comman.Dapper.Linq.Extension.Entites;
using Comman.Dapper.Linq.Extension.Exception;
using Comman.Dapper.Linq.Extension.Extension;

namespace Comman.Dapper.Linq.Extension.Expressions
{
    /// <summary>
    ///     专门处理子查询的的表达式树扩展类
    /// </summary>
    public class SubqueryExpression : ExpressionVisitor
    {
        private readonly StringBuilder _sqlCmd;
        private readonly MethodCallExpression expression;

        /// <summary>
        ///     参数
        /// </summary>
        public Comman.Dapper.Linq.Extension.Dapper.DynamicParameters Param;

        private List<ParameterExpression> parameterExpressions;

        /// <summary>
        ///     参数索引(防止命名冲突)
        /// </summary>
        private readonly int paramIndex;

        /// <summary>
        /// </summary>
        /// <param name="methodCallExpression"></param>
        /// <param name="paramIndex"></param>
        public SubqueryExpression(MethodCallExpression methodCallExpression, int paramIndex = 0)
        {
            expression = methodCallExpression;
            _sqlCmd = new StringBuilder();
            Param = new Comman.Dapper.Linq.Extension.Dapper.DynamicParameters();
            WhereExpression = new List<LambdaExpression>();
            this.paramIndex = paramIndex;
            AnalysisExpression();
        }

        /// <summary>
        ///     sql指令
        /// </summary>
        public string SqlCmd => _sqlCmd.ToString();

        /// <summary>
        ///     返回类型
        /// </summary>
        public Type ReturnType { get; set; }

        /// <summary>
        ///     解析表达式
        /// </summary>
        public void AnalysisExpression()
        {
            AnalysisKogelQuerySet(expression);
            AnalysisKogelExpression(expression);
            //动态执行，得到T类型
            typeof(SubqueryExpression)
                .GetMethod("FormatSend")
                .MakeGenericMethod(QuerySet.GetType().GenericTypeArguments[0])
                .Invoke(this, new[] { QuerySet, expression.Method.Name });
        }

        /// <summary>
        ///     递归得到QuerySet
        /// </summary>
        /// <param name="methodCallExpression"></param>
        public void AnalysisKogelQuerySet(MethodCallExpression methodCallExpression)
        {
            switch (methodCallExpression.Method.Name)
            {
                case "QuerySet":
                {
                    QuerySet = methodCallExpression.ToConvertAndGetValue();
                    break;
                }
            }

            if (methodCallExpression.Object != null)
            {
                if (methodCallExpression.Object is MethodCallExpression)
                {
                    var objectCallExpression = methodCallExpression.Object as MethodCallExpression;
                    AnalysisKogelQuerySet(objectCallExpression);
                }
                else if (methodCallExpression.Object.Type.FullName.Contains("QuerySet"))
                {
                    QuerySet = methodCallExpression.Object.ToConvertAndGetValue();
                }
            }
        }

        /// <summary>
        ///     递归解析导航查询表达式
        /// </summary>
        /// <param name="methodCallExpression"></param>
        /// <returns></returns>
        public void AnalysisKogelExpression(MethodCallExpression methodCallExpression)
        {
            switch (methodCallExpression.Method.Name)
            {
                case "Where":
                {
                    foreach (UnaryExpression exp in methodCallExpression.Arguments)
                    {
                        parameterExpressions = new List<ParameterExpression>();
                        Visit(exp);
                        var lambda = Expression.Lambda(exp, parameterExpressions.ToList());
                        WhereExpression.Add(lambda);
                    }

                    break;
                }
                case "WhereIf":
                {
                    parameterExpressions = new List<ParameterExpression>();
                    if (methodCallExpression.Arguments[0].ToConvertAndGetValue().Equals(true))
                    {
                        var trueExp = methodCallExpression.Arguments[1];
                        Visit(trueExp);
                        var lambda = Expression.Lambda(trueExp, parameterExpressions.ToList());
                        WhereExpression.Add(lambda);
                    }
                    else
                    {
                        var falseExp = methodCallExpression.Arguments[2];
                        Visit(falseExp);
                        var lambda = Expression.Lambda(falseExp, parameterExpressions.ToList());
                        WhereExpression.Add(lambda);
                    }

                    break;
                }
                default:
                {
                    if (methodCallExpression.Method.Name == "OrderBy" ||
                        methodCallExpression.Method.Name == "OrderByDescing")
                    {
                        //设置参数和参数类型
                        var method = methodCallExpression.Method;
                        if (methodCallExpression.Arguments.Count != 0)
                        {
                            var paramType = (methodCallExpression.Arguments.First() as UnaryExpression).Operand.Type
                                .GenericTypeArguments[0];
                            if (paramType != typeof(string))
                            {
                                var lambda = methodCallExpression.Arguments[0].GetLambdaExpression();
                                typeof(SubqueryExpression)
                                    .GetMethod("FormatSendOrder")
                                    .MakeGenericMethod(QuerySet.GetType().GenericTypeArguments[0])
                                    .Invoke(this, new[] { QuerySet, lambda, methodCallExpression.Method.Name });
                                break;
                            }
                        }
                    }

                    //正常方法
                    string[] methodArr = { "Count", "Sum", "Min", "Max", "Get", "ToList", "Page", "PageList" };
                    if (!methodArr.Contains(methodCallExpression.Method.Name))
                    {
                        var parameters = methodCallExpression.Arguments.Select(x => x.ToConvertAndGetValue()).ToArray();
                        methodCallExpression.Method.Invoke(QuerySet, parameters);
                    }

                    break;
                }
            }

            if (methodCallExpression.Object != null)
                if (methodCallExpression.Object is MethodCallExpression)
                {
                    var objectCallExpression = methodCallExpression.Object as MethodCallExpression;
                    AnalysisKogelExpression(objectCallExpression);
                }
        }

        /// <summary>
        ///     解析参数
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (!parameterExpressions.Exists(x => x.Name == node.Name))
            {
                var param = Expression.Parameter(node.Type, node.Name);
                parameterExpressions.Add(param);
            }

            return node;
        }

        /// <summary>
        ///     替换成新的参数名，防止命名冲突
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        private Comman.Dapper.Linq.Extension.Dapper.DynamicParameters ToSubqueryParam(Comman.Dapper.Linq.Extension.Dapper.DynamicParameters param, ref string sql)
        {
            var newParam = new Comman.Dapper.Linq.Extension.Dapper.DynamicParameters();
            foreach (var paramName in param.ParameterNames)
            {
                var newName = $"{paramName}_Subquery_{paramIndex}";
                var value = param.Get<object>(paramName);
                newParam.Add(newName, value);
                sql = sql.Replace(paramName, newName);
            }

            return newParam;
        }

        /// <summary>
        ///     反射执行需要指向T类型的函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlProvider"></param>
        /// <param name="methodName"></param>
        public void FormatSend<T>(Comman.Dapper.Linq.Extension.Core.SetQ.QuerySet<T> querySet, string methodName)
        {
            var sqlProvider = querySet.SqlProvider;
            //写入重新生成后的条件
            if (WhereExpression != null && WhereExpression.Any())
                querySet.WhereExpressionList.AddRange(WhereExpression);
            //因为表达式的原因，递归获取连表默认会倒序
            if (sqlProvider.JoinList.Any()) sqlProvider.JoinList.Reverse();
            switch (methodName)
            {
                case "Count":
                {
                    sqlProvider.FormatCount();
                }
                    break;
                case "Sum":
                {
                    var lambda = expression.Arguments[0].GetLambdaExpression();
                    sqlProvider.FormatSum(lambda);
                }
                    break;
                case "Min":
                {
                    var lambda = expression.Arguments[0].GetLambdaExpression();
                    sqlProvider.FormatMin(lambda);
                }
                    break;
                case "Max":
                {
                    var lambda = expression.Arguments[0].GetLambdaExpression();
                    sqlProvider.FormatMax(lambda);
                }
                    break;
                case "Get":
                {
                    var lambda = default(LambdaExpression);
                    if (expression.Arguments.Count == 1)
                    {
                        lambda = expression.Arguments[0].GetLambdaExpression();
                        ReturnType = lambda.ReturnType;
                    }
                    else if (expression.Arguments.Count == 0) //无自定义列表返回
                    {
                        lambda = null;
                        ReturnType = expression.Method.ReturnType;
                    }
                    else
                    {
                        //带if判断
                        if (expression.Arguments[0].ToConvertAndGetValue().Equals(true))
                            lambda = expression.Arguments[1].GetLambdaExpression();
                        else
                            lambda = expression.Arguments[2].GetLambdaExpression();
                        ReturnType = lambda.ReturnType;
                    }

                    sqlProvider.Context.Set.SelectExpression = lambda;
                    sqlProvider.FormatGet<T>();
                }
                    break;
                case "ToList":
                {
                    var lambda = default(LambdaExpression);
                    if (expression.Arguments.Count == 1)
                    {
                        lambda = expression.Arguments[0].GetLambdaExpression();
                        ReturnType = lambda.ReturnType;
                    }
                    else if (expression.Arguments.Count == 0) //无自定义列表返回
                    {
                        lambda = null;
                        ReturnType = expression.Method.ReturnType.GenericTypeArguments[0];
                    }
                    else
                    {
                        //带if判断
                        if (expression.Arguments[0].ToConvertAndGetValue().Equals(true))
                            lambda = expression.Arguments[1].GetLambdaExpression();
                        else
                            lambda = expression.Arguments[2].GetLambdaExpression();
                        ReturnType = lambda.ReturnType;
                    }

                    sqlProvider.Context.Set.SelectExpression = lambda;
                    sqlProvider.FormatToList<T>();
                }
                    break;
                case "Page":
                {
                    var pageIndex = Convert.ToInt32(expression.Arguments[0].ToConvertAndGetValue());
                    var pageSize = Convert.ToInt32(expression.Arguments[1].ToConvertAndGetValue());
                    var lambda = default(LambdaExpression);
                    if (expression.Arguments.Count == 3)
                    {
                        lambda = expression.Arguments[2].GetLambdaExpression();
                        ReturnType = lambda.ReturnType;
                    }
                    else if (expression.Arguments.Count == 2) //无自定义列表返回
                    {
                        lambda = null;
                        ReturnType = expression.Method.ReturnType.GenericTypeArguments[0];
                    }
                    else
                    {
                        //带if判断
                        if (expression.Arguments[2].ToConvertAndGetValue().Equals(true))
                            lambda = expression.Arguments[3].GetLambdaExpression();
                        else
                            lambda = expression.Arguments[4].GetLambdaExpression();
                        ReturnType = lambda.ReturnType;
                    }

                    sqlProvider.Context.Set.SelectExpression = lambda;
                    sqlProvider.FormatToPageList<T>(pageIndex, pageSize);
                }
                    break;
                default:
                    throw new DapperExtensionException("Kogel.Dapper.Extension中子查询不支持的扩展函数");
            }

            //得到解析的sql和param对象
            var sql = sqlProvider.SqlString;
            var param = ToSubqueryParam(sqlProvider.Params, ref sql);
            _sqlCmd.Append(sql);
            Param.AddDynamicParams(param);
        }

        /// <summary>
        ///     反射执行多表联查的排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="querySet"></param>
        /// <param name="orderExpression"></param>
        /// <param name="methodName"></param>
        public void FormatSendOrder<T>(Comman.Dapper.Linq.Extension.Core.SetQ.QuerySet<T> querySet, LambdaExpression orderExpression, string methodName)
        {
            if (methodName == "OrderBy")
                querySet.OrderbyExpressionList.Add(orderExpression, EOrderBy.Asc);
            else
                querySet.OrderbyExpressionList.Add(orderExpression, EOrderBy.Desc);
        }

        #region Kogel对象

        /// <summary>
        ///     查询对象
        /// </summary>
        public object QuerySet { get; set; }

        /// <summary>
        ///     条件表达式
        /// </summary>
        public List<LambdaExpression> WhereExpression { get; set; }

        #endregion
    }
}