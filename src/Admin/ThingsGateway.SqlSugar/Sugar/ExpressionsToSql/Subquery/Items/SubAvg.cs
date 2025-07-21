using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    public class SubAvg : ISubOperation
    {
        public bool HasWhere
        {
            get; set;
        }

        public string Name
        {
            get
            {
                return "Avg";
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

        public string GetValue(Expression expression = null)
        {
            var exp = expression as MethodCallExpression;
            var argExp = exp.Arguments[0];
            var parameters = (argExp as LambdaExpression).Parameters;
            if ((argExp as LambdaExpression).Body is UnaryExpression)
            {
                argExp = ((argExp as LambdaExpression).Body as UnaryExpression).Operand;
            }
            var argLambda = argExp as LambdaExpression;
            if (this.Context.InitMappingInfo != null && argLambda?.Parameters.Count > 0)
            {
                foreach (var item in argLambda.Parameters)
                {
                    this.Context.InitMappingInfo(item.Type);
                }
                this.Context.RefreshMapping();
            }
            var result = "AVG(" + SubTools.GetMethodValue(Context, argExp, ResolveExpressType.WhereMultiple) + ")";
            var selfParameterName = Context.GetTranslationColumnName(parameters[0].Name) + UtilConstants.Dot;
            if (this.Context.JoinIndex == 0)
                result = result.Replace(selfParameterName, SubTools.GetSubReplace(this.Context));
            return result;
        }
    }
}
