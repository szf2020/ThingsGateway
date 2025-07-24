using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 分表删除提供者(按对象删除)
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class SplitTableDeleteByObjectProvider<T> where T : class, new()
    {
        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        public ISqlSugarClient Context;
        /// <summary>
        /// 可删除提供者
        /// </summary>
        public DeleteableProvider<T> deleteobj;
        /// <summary>
        /// 待删除对象数组
        /// </summary>
        public IReadOnlyCollection<T> deleteObjects { get; set; }

        /// <summary>
        /// 执行删除命令
        /// </summary>
        /// <returns>影响的行数</returns>
        public int ExecuteCommand()
        {
            List<GroupModel> groupModels;
            int result;
            GroupDataList(deleteObjects, out groupModels, out result);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                var addList = item.Select(it => it.Item).ToList();
                result += this.Context.Deleteable<T>().Where(addList).AS(item.Key).ExecuteCommand();
            }
            return result;
        }

        /// <summary>
        /// 异步执行删除命令
        /// </summary>
        /// <returns>影响的行数任务</returns>
        public async Task<int> ExecuteCommandAsync()
        {
            List<GroupModel> groupModels;
            int result;
            GroupDataList(deleteObjects, out groupModels, out result);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                var addList = item.Select(it => it.Item).ToList();
                result += await Context.Deleteable<T>().Where(addList).AS(item.Key).ExecuteCommandAsync().ConfigureAwait(false);
            }
            return result;
        }

        /// <summary>
        /// 分组数据列表
        /// </summary>
        /// <param name="datas">数据数组</param>
        /// <param name="groupModels">分组模型列表</param>
        /// <param name="result">结果值</param>
        private void GroupDataList(IEnumerable<T> datas, out List<GroupModel> groupModels, out int result)
        {
            var attribute = typeof(T).GetCustomAttribute<SplitTableAttribute>() as SplitTableAttribute;
            Check.Exception(attribute == null, $"{typeof(T).Name} need SplitTableAttribute");
            groupModels = new List<GroupModel>();
            var db = this.Context;
            var context = db.SplitHelper<T>();
            foreach (var item in datas)
            {
                var value = context.GetValue(attribute.SplitType, item);
                var tableName = context.GetTableName(attribute.SplitType, value);
                groupModels.Add(new GroupModel() { GroupName = tableName, Item = item });
            }
            result = 0;
        }

        /// <summary>
        /// 分组模型
        /// </summary>
        internal class GroupModel
        {
            /// <summary>
            /// 分组名称(表名)
            /// </summary>
            public string GroupName { get; set; }
            /// <summary>
            /// 数据项
            /// </summary>
            public T Item { get; set; }
        }
    }
}