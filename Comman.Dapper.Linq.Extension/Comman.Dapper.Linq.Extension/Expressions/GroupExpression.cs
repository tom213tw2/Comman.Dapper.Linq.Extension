using System;
using System.Linq.Expressions;
using System.Text;
using Comman.Dapper.Linq.Extension.Exception;
using Comman.Dapper.Linq.Extension.Extension;
using Comman.Dapper.Linq.Extension.Helper.Cache;

namespace Comman.Dapper.Linq.Extension.Expressions
{
	/// <summary>
	///     解析分组
	/// </summary>
	public class GroupExpression : BaseExpressionVisitor
    {
        public GroupExpression(LambdaExpression expression, string prefix, SqlProvider provider) : base(provider)
        {
            _sqlCmd = new StringBuilder();
            Param = new Comman.Dapper.Linq.Extension.Dapper.DynamicParameters();
            //当前定义的查询返回对象
            var entity = EntityCache.QueryEntity(expression.Body.Type);
            var newExpression = expression.Body as NewExpression;
            foreach (var argument in newExpression.Arguments)
            {
                SpliceField.Clear();
                base.Param = new Comman.Dapper.Linq.Extension.Dapper.DynamicParameters();
                if (_sqlCmd.Length != 0)
                    _sqlCmd.Append(",");
                //返回类型
                var returnProperty = entity.Properties[Index];
                //实体类型
                Type entityType;
                //验证是实体类或者是泛型
                if (ExpressionExtension.IsAnyBaseEntity(returnProperty.PropertyType, out entityType))
                {
                    throw new DapperExtensionException("GroupBy不支持导航属性!");
                }

                //值对象
                Visit(argument);
                _sqlCmd.Append($" {SpliceField} ");
                Param.AddDynamicParams(base.Param);
                Index++;
            }
        }

        #region sql指令

        private readonly StringBuilder _sqlCmd;

        /// <summary>
        ///     sql指令
        /// </summary>
        public string SqlCmd => _sqlCmd.ToString();

        /// <summary>
        ///     参数
        /// </summary>
        public new Comman.Dapper.Linq.Extension.Dapper.DynamicParameters Param;

        #endregion
    }
}