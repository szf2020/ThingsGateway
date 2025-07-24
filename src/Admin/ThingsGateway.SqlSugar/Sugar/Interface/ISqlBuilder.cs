using System.Data;
using System.Text;
namespace ThingsGateway.SqlSugar
{
    public partial interface ISqlBuilder
    {
        SqlSugarProvider Context { get; set; }
        CommandType CommandType { get; set; }
        String AppendWhereOrAnd(bool isWhere, string sqlString);
        string AppendHaving(string sqlString);

        SqlQueryBuilder SqlQueryBuilder { get; set; }
        QueryBuilder QueryBuilder { get; set; }
        InsertBuilder InsertBuilder { get; set; }
        DeleteBuilder DeleteBuilder { get; set; }
        UpdateBuilder UpdateBuilder { get; set; }

        string SqlParameterKeyWord { get; }
        string SqlFalse { get; }
        string SqlDateNow { get; }
        string FullSqlDateNow { get; }
        string SqlTranslationLeft { get; }
        string SqlTranslationRight { get; }
        string SqlSelectAll { get; }

        void ChangeJsonType(SugarParameter paramter);
        string GetTranslationTableName(string name);
        string GetTranslationColumnName(string entityName, string propertyName);
        string GetTranslationColumnName(string propertyName);
        string GetNoTranslationColumnName(string name);
        string GetPackTable(string sql, string shortName);
        string GetDefaultShortName();

        string GetWhere(string fieldName, string conditionalType, int? parameterIndex = null);
        string GetUnionAllSql(List<string> sqlList);
        string GetUnionSql(List<string> sqlList);
        void RepairReplicationParameters(ref string appendSql, IReadOnlyCollection<SugarParameter> parameters, int addIndex);
        KeyValuePair<string, IReadOnlyCollection<SugarParameter>> ConditionalModelToSql(List<IConditionalModel> models, int beginIndex = 0);
        string GetUnionFomatSql(string sql);
        Type GetNullType(string tableName, string columnName);
        string RemoveParentheses(string sql);
        string RemoveN(string sql);
        void FormatSaveQueueSql(StringBuilder sqlBuilder);

        bool SupportReadToken { get; set; }
        Task<bool> GetReaderByToken(IDataReader dataReader, CancellationToken cancellationToken);
    }
}
