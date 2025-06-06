using System.Linq.Expressions;

namespace SqlSugar
{
    public class SubWithNolock : ISubOperation
    {
        public ExpressionContext Context
        {
            get; set;
        }

        public Expression Expression
        {
            get; set;
        }

        public bool HasWhere
        {
            get; set;
        }

        public string Name
        {
            get
            {
                return "WithNoLock";
            }
        }

        public int Sort
        {
            get
            {
                return 301;
            }
        }

        public string GetValue(Expression expression)
        {
            if (Context is SqlServerExpressionContext)
            {
                return SqlWith.NoLock;
            }
            else
            {
                return "";
            }
        }
    }
}
