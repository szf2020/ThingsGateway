using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    public class SubAny : ISubOperation
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
                return nameof(QueryMethodInfo.Any);
            }
        }

        public int Sort
        {
            get
            {
                return 0;
            }
        }

        public string GetValue(Expression expression)
        {
            return "EXISTS";
        }
    }
}
