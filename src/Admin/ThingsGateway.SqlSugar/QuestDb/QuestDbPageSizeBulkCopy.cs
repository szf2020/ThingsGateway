namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 提供分页批量插入数据到QuestDB的功能
    /// </summary>
    public class QuestDbPageSizeBulkCopy
    {
        private QuestDbRestAPI questDbRestAPI;
        private int pageSize;
        private ISqlSugarClient db;

        /// <summary>
        /// 初始化QuestDbPageSizeBulkCopy实例
        /// </summary>
        /// <param name="questDbRestAPI">QuestDB REST API客户端</param>
        /// <param name="pageSize">每页数据量</param>
        /// <param name="db">SqlSugar客户端</param>
        public QuestDbPageSizeBulkCopy(QuestDbRestAPI questDbRestAPI, int pageSize, ISqlSugarClient db)
        {
            this.questDbRestAPI = questDbRestAPI;
            this.pageSize = pageSize;
            this.db = db;
        }

        /// <summary>
        /// 同步批量插入数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="insertDatas">要插入的数据列表</param>
        /// <param name="dateFormat">日期格式字符串</param>
        /// <returns>插入的记录数</returns>
        public int BulkCopy<T>(List<T> insertDatas, string dateFormat = "yyyy/M/d H:mm:ss") where T : class, new()
        {
            int result = 0;
            // 使用分页方式处理大数据量插入
            db.Utilities.PageEach(insertDatas, pageSize, pageItems =>
            {
                // 同步调用批量插入API并累加结果
                result += questDbRestAPI.BulkCopyAsync(pageItems, dateFormat).GetAwaiter().GetResult();
            });
            return result;
        }

        /// <summary>
        /// 异步批量插入数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="insertDatas">要插入的数据列表</param>
        /// <param name="dateFormat">日期格式字符串</param>
        /// <returns>插入的记录数</returns>
        public async Task<int> BulkCopyAsync<T>(List<T> insertDatas, string dateFormat = "yyyy/M/d H:mm:ss") where T : class, new()
        {
            int result = 0;
            // 异步分页处理大数据量插入
            await db.Utilities.PageEachAsync(insertDatas, pageSize, async pageItems =>
            {
                // 异步调用批量插入API并累加结果
                result += await questDbRestAPI.BulkCopyAsync(pageItems, dateFormat).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
        }
    }
}