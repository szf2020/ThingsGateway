using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    public class SubCount : ISubOperation
    {
        public bool HasWhere
        {
            get; set;
        }

        public string Name
        {
            get
            {
                return nameof(QueryMethodInfo.Count);
            }
        }

        public Expression Expression
        {
            get; set;
        }

        public int Sort
        {
            get
            {
                return 200;
            }
        }

        public ExpressionContext Context
        {
            get; set;
        }

        public string GetValue(Expression expression)
        {
            return "COUNT(*)";
        }
    }
}
