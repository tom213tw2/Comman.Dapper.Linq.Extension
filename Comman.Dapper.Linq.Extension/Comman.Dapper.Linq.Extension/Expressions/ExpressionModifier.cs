using System.Linq.Expressions;

namespace Comman.Dapper.Linq.Extension.Expressions
{
    internal class ExpressionModifier : ExpressionVisitor
    {
        private readonly Expression _newExpression;
        private readonly Expression _oldExpression;

        public ExpressionModifier(Expression newExpression, Expression oldExpression)
        {
            _newExpression = newExpression;
            _oldExpression = oldExpression;
        }

        public Expression Replace(Expression node)
        {
            return Visit(node == _oldExpression ? _newExpression : node);
        }

        public static Expression Replace(Expression node, Expression oldExpression, Expression newExpression)
        {
            return new ExpressionModifier(newExpression, oldExpression).Replace(node);
        }
    }
}