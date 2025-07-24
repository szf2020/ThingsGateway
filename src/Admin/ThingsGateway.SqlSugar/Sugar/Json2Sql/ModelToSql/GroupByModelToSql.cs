using System.Text;
namespace ThingsGateway.SqlSugar
{
    public abstract partial class SqlBuilderProvider : SqlBuilderAccessory, ISqlBuilder
    {
        public KeyValuePair<string, IReadOnlyCollection<SugarParameter>> GroupByModelToSql(List<GroupByModel> models)
        {
            StringBuilder sql = new StringBuilder("");
            var pars = new List<SugarParameter>();
            foreach (var item in models)
            {
                if (item is GroupByModel && item.FieldName is IFuncModel)
                {
                    var orderByModel = item as GroupByModel;
                    sql.Append($" {GetSqlPart(item.FieldName, pars)} ,");
                }
                else if (item is GroupByModel)
                {
                    var orderByModel = item as GroupByModel;
                    sql.Append($" {this.GetTranslationColumnName(orderByModel.FieldName.ObjToString().ToSqlFilter())} ,");
                }
            }
            return new KeyValuePair<string, IReadOnlyCollection<SugarParameter>>(sql.ToString().TrimEnd(','), pars);
        }
    }
}
