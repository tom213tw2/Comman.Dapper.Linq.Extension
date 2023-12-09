using System.Linq.Expressions;
using System.Text;

namespace Comman.Dapper.Linq.Extension.Expressions
{
    /// <summary>
    ///     解析查询条件
    /// </summary>
    public sealed class WhereExpression : BaseExpressionVisitor.WhereExpressionVisitor
    {
        /// <summary>
        ///     解析条件对象
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="prefix">参数标记</param>
        /// <param name="providerOption"></param>
        public WhereExpression(LambdaExpression expression, string prefix, SqlProvider provider) : base(provider)
        {
            _sqlCmd = new StringBuilder(100);
            Param = new Comman.Dapper.Linq.Extension.Dapper.DynamicParameters();
            providerOption = provider.ProviderOption;
            Prefix = prefix;
            //开始解析对象
            Visit(expression);
            //开始拼接成条件
            _sqlCmd.Append(SpliceField);
            SqlCmd = " AND " + _sqlCmd;
            Param.AddDynamicParams(base.Param);
        }

        /// <summary>
        ///     解析二元表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var binaryWhere = new BinaryExpressionVisitor(node, Provider, 0, Prefix);
            _sqlCmd.Append(binaryWhere.SpliceField);
            base.Param.AddDynamicParams(binaryWhere.Param);
            return node;
        }

        /// <summary>
        ///     解析!不等于(!里只能包含一个条件)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not)
            {
                SpliceField.Append(" Not (");
                Visit(node.Operand);
                SpliceField.Append(") ");
            }
            else
            {
                Visit(node.Operand);
            }

            return node;
        }

        #region sql指令

        private readonly StringBuilder _sqlCmd;

        /// <summary>
        ///     sql指令
        /// </summary>
        public string SqlCmd { get; }

        /// <summary>
        ///     参数
        /// </summary>
        public new Comman.Dapper.Linq.Extension.Dapper.DynamicParameters Param { get; }

        #endregion
    }
}