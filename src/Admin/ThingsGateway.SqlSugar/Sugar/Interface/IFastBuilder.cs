using System.Data;

namespace ThingsGateway.SqlSugar
{
    public interface IFastBuilder
    {
        EntityInfo FastEntityInfo { get; set; }
        bool IsActionUpdateColumns { get; set; }
        DbFastestProperties DbFastestProperties { get; set; }
        SqlSugarProvider Context { get; set; }
        string CharacterSet { get; set; }
        Task<int> UpdateByTempAsync(string tableName, string tempName, string[] updateColumns, string[] whereColumns);
        Task<int> ExecuteBulkCopyAsync(DataTable dt);
        Task<int> ExecuteBulkCopyAsync(string tableName, Dictionary<string, (Type, List<DataInfos>)> list)
        {
            return Task.FromResult(0);
        }
        Task CreateTempAsync<T>(DataTable dt) where T : class, new();
        Task<string> CreateTempAsync<T>(Dictionary<string, (Type, List<DataInfos>)> list) where T : class, new()
        {
            return Task.FromResult(string.Empty);
        }

        void CloseDb();

        Task<int> Merge<T>(string tableName, Dictionary<string, (Type, List<DataInfos>)> list, EntityInfo entityInfo, string[] whereColumns, string[] updateColumns, IEnumerable<T> datas) where T : class, new()
        {
            return Task.FromResult(0);
        }
        Task<int> Merge<T>(string tableName, DataTable dt, EntityInfo entityInfo, string[] whereColumns, string[] updateColumns, IEnumerable<T> datas) where T : class, new();
    }
}
