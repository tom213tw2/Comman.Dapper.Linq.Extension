using System.Linq.Expressions;

namespace Comman.Dapper.Linq.Extension.Expressions
{
    internal class ExpressionModifier : ExpressionVisitor
    {
        private readonly Expression newExpression;
        private readonly Expression oldExpression;

        public ExpressionModifier(Expression newExpression, Expression oldExpression)
        {
            this.newExpression = newExpression;
            this.oldExpression = oldExpression;
        }

        public Expression Replace(Expression node)
        {
            return Visit(node == oldExpression ? newExpression : node);
        }

        public static Expression Replace(Expression node, Expression oldExpression, Expression newExpression)
        {
            return new ExpressionModifier(newExpression, oldExpression).Replace(node);
        }
    }
}