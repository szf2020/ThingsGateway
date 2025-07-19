using System.Linq.Expressions;
namespace ThingsGateway.SqlSugar
{
    public class LambdaExpressionResolve : BaseResolve
    {
        public LambdaExpressionResolve(ExpressionParameter parameter) : base(parameter)
        {
            LambdaExpression lambda = base.Expression as LambdaExpression;
            var expression = ExpressionTool.RemoveConvert(lambda.Body);
            base.Expression = expression;
            if (parameter.Context.ResolveType.IsIn(ResolveExpressType.FieldMultiple, ResolveExpressType.FieldSingle))
            {
                parameter.CommonTempData = CommonTempDataType.Append;
            }
            base.Start();
        }
    }
}
