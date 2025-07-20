using System.Data;
using System.Text.RegularExpressions;

namespace ThingsGateway.SqlSugar
{
    public interface IDataAdapter
    {
        void Fill(DataSet ds);
    }
    public partial class SqliteProvider : AdoProvider
    {
        public override void ExecuteBefore(string sql, IReadOnlyList<SugarParameter> parameters)
        {
            this.BeforeTime = DateTime.Now;
            if (sql.HasValue() && parameters.HasValue())
            {
                foreach (var parameter in parameters)
                {
                    //Compatible with.NET CORE parameters case
                    var name = parameter.ParameterName;
                    if (!sql.Contains(name) && Regex.IsMatch(sql, "(" + name + "$)" + "|(" + name + @"[ ,\,])", RegexOptions.IgnoreCase))
                    {
                        parameter.ParameterName = Regex.Match(sql, "(" + name + "$)" + "|(" + name + @"[ ,\,])", RegexOptions.IgnoreCase).Value.Trim();
                    }
                }
            }
            if (this.IsEnableLogEvent)
            {
                Action<string, IReadOnlyList<SugarParameter>> action = LogEventStarting;
                if (action != null)
                {
                    if (parameters == null || parameters.Count == 0)
                    {
                        action(sql, Array.Empty<SugarParameter>());
                    }
                    else
                    {
                        action(sql, parameters);
                    }
                }
            }
        }
    }
}
