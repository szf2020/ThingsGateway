namespace ThingsGateway.SqlSugar
{
    public partial class PostgreSQLExpressionContext
    {
    }
    public partial class DmExpressionContext
    {
    }
    public partial class OracleExpressionContext
    {
    }
    public partial class SqlServerBlukCopy
    {
    }
    public partial class MySqlBlukCopy<T>
    {
        internal SqlSugarProvider Context { get; set; }
        internal ISqlBuilder Builder { get; set; }
        internal IReadOnlyList<T> Entitys { get; set; }
        internal string Chara { get; set; }

        public MySqlBlukCopy(SqlSugarProvider context, ISqlBuilder builder, IReadOnlyList<T> entitys)
        {
            this.Context = context;
            this.Builder = builder;
            this.Entitys = entitys;
        }
    }
    public partial class OracleBlukCopy
    {
    }
}
