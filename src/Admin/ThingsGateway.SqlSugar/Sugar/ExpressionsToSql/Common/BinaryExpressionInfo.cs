namespace ThingsGateway.SqlSugar
{
    public class BinaryExpressionInfo
    {
        public bool IsLeft { get; set; }
        public Type ExpressionType { get; set; }
        public object Value { get; set; }
    }
}
