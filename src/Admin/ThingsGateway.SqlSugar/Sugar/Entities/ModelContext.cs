using Newtonsoft.Json;

namespace ThingsGateway.SqlSugar
{
    public class ModelContext
    {
        [SugarColumn(IsIgnore = true)]
        [JsonIgnore]
        public SqlSugarProvider Context { get; set; }
        public ISugarQueryable<T> CreateMapping<T>() where T : class, new()
        {
            if (Context == null) { throw new SqlSugarException("Please use Sqlugar.ModelContext"); }
            using (Context)
            {
                return Context.Queryable<T>();
            }
        }
    }
}
