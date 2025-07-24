using System.Text;
namespace ThingsGateway.SqlSugar
{
    public abstract partial class SqlBuilderProvider : SqlBuilderAccessory, ISqlBuilder
    {
        public KeyValuePair<string, IReadOnlyCollection<SugarParameter>> SelectModelToSql(List<SelectModel> models)
        {
            StringBuilder sql = new StringBuilder("");
            var pars = new List<SugarParameter>();
            foreach (var item in models)
            {
                if (item is SelectModel)
                {
                    var orderByModel = item as SelectModel;
                    orderByModel.AsName = GetAsName(orderByModel);
                    orderByModel.FieldName = GetSqlPart(orderByModel.FieldName, pars).ObjToString();
                    AppendFieldName(sql, orderByModel);
                }
            }
            return new KeyValuePair<string, IReadOnlyCollection<SugarParameter>>(sql.ToString().TrimEnd(','), pars);
        }

        private string GetAsName(SelectModel orderByModel)
        {
            if (orderByModel.AsName.IsNullOrEmpty())
            {
                orderByModel.AsName = orderByModel.FieldName.ObjToString();
            }
            if (orderByModel.AsName.StartsWith(UtilConstants.ReplaceKey))
            {
                return orderByModel.AsName.Replace(UtilConstants.ReplaceKey, string.Empty);
            }
            if (orderByModel.AsName?.Contains('[') == true)
            {
                orderByModel.AsName = orderByModel.AsName.Trim('[').Trim(']');
                return this.SqlTranslationLeft + orderByModel.AsName + this.SqlTranslationRight;
            }
            if (this.SqlTranslationLeft != null && orderByModel.AsName?.Contains(this.SqlTranslationLeft) == true)
            {
                return orderByModel.AsName;
            }
            return this.SqlTranslationLeft + orderByModel.AsName + this.SqlTranslationRight;
        }

        private void AppendFieldName(StringBuilder sql, SelectModel orderByModel)
        {
            sql.Append($" {orderByModel.FieldName} AS {orderByModel.AsName} ,");
        }
    }
}
