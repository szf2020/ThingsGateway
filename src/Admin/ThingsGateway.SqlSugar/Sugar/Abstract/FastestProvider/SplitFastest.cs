using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    /// <summary>分表快速操作类</summary>
    public class SplitFastest<T> where T : class, new()
    {
        /// <summary>快速操作提供者</summary>
        public FastestProvider<T> FastestProvider { get; set; }
        /// <summary>SqlSugar上下文</summary>
        public SqlSugarProvider Context { get { return this.FastestProvider.context; } }
        /// <summary>实体信息</summary>
        public EntityInfo EntityInfo { get { return this.Context.EntityMaintenance.GetEntityInfo<T>(); } }

        /// <summary>批量插入数据(分表)</summary>
        public int BulkCopy(List<T> datas)
        {
            if (StaticConfig.SplitTableCreateTableFunc != null)
            {
                StaticConfig.SplitTableCreateTableFunc(typeof(T), datas);
            }
            List<GroupModel> groupModels;
            int result;
            GroupDataList(datas, out groupModels, out result);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                CreateTable(item.Key);
                var addList = item.Select(it => it.Item).ToList();
                result += FastestProvider.AS(item.Key).BulkCopy(addList);
                this.Context.MappingTables.Add(EntityInfo.EntityName, EntityInfo.DbTableName);
            }
            return result;
        }

        /// <summary>异步批量插入数据(分表)</summary>
        public async Task<int> BulkCopyAsync(List<T> datas)
        {
            if (StaticConfig.SplitTableCreateTableFunc != null)
            {
                StaticConfig.SplitTableCreateTableFunc(typeof(T), datas);
            }
            List<GroupModel> groupModels;
            int result;
            GroupDataList(datas, out groupModels, out result);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                CreateTable(item.Key);
                var addList = item.Select(it => it.Item).ToList();
                result += await FastestProvider.AS(item.Key).BulkCopyAsync(addList).ConfigureAwait(false);
                this.Context.MappingTables.Add(EntityInfo.EntityName, EntityInfo.DbTableName);
            }
            return result;
        }

        /// <summary>批量更新数据(分表)</summary>
        public int BulkUpdate(List<T> datas)
        {
            List<GroupModel> groupModels;
            int result;
            GroupDataList(datas, out groupModels, out result);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                CreateTable(item.Key);
                var addList = item.Select(it => it.Item).ToList();
                result += FastestProvider.AS(item.Key).BulkUpdate(addList);
                this.Context.MappingTables.Add(EntityInfo.EntityName, EntityInfo.DbTableName);
            }
            return result;
        }

        /// <summary>异步批量更新数据(分表)</summary>
        public async Task<int> BulkUpdateAsync(List<T> datas)
        {
            List<GroupModel> groupModels;
            int result;
            GroupDataList(datas, out groupModels, out result);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                CreateTable(item.Key);
                var addList = item.Select(it => it.Item).ToList();
                result += await FastestProvider.AS(item.Key).BulkUpdateAsync(addList).ConfigureAwait(false);
                this.Context.MappingTables.Add(EntityInfo.EntityName, EntityInfo.DbTableName);
            }
            return result;
        }

        /// <summary>批量更新数据(分表，指定条件列和更新列)</summary>
        public int BulkUpdate(List<T> datas, string[] wherColumns, string[] updateColumns)
        {
            List<GroupModel> groupModels;
            int result;
            GroupDataList(datas, out groupModels, out result);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                var addList = item.Select(it => it.Item).ToList();
                result += FastestProvider.AS(item.Key).BulkUpdate(addList, wherColumns, updateColumns);
            }
            return result;
        }

        /// <summary>异步批量更新数据(分表，指定条件列和更新列)</summary>
        public async Task<int> BulkUpdateAsync(List<T> datas, string[] wherColumns, string[] updateColumns)
        {
            List<GroupModel> groupModels;
            int result;
            GroupDataList(datas, out groupModels, out result);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                var addList = item.Select(it => it.Item).ToList();
                result += await FastestProvider.AS(item.Key).BulkUpdateAsync(addList, wherColumns, updateColumns).ConfigureAwait(false);
            }
            return result;
        }

        /// <summary>创建分表</summary>
        private void CreateTable(string tableName)
        {
            var isLog = this.Context.Ado.IsEnableLogEvent;
            this.Context.Ado.IsEnableLogEvent = false;
            if (!this.Context.DbMaintenance.IsAnyTable(tableName, false))
            {
                this.Context.MappingTables.Add(EntityInfo.EntityName, tableName);
                this.Context.CodeFirst.InitTables<T>();
            }
            this.Context.Ado.IsEnableLogEvent = isLog;
        }

        /// <summary>分组数据列表</summary>
        private void GroupDataList(List<T> datas, out List<GroupModel> groupModels, out int result)
        {
            var attribute = typeof(T).GetCustomAttribute<SplitTableAttribute>() as SplitTableAttribute;
            Check.Exception(attribute == null, $"{typeof(T).Name} need SplitTableAttribute");
            groupModels = new List<GroupModel>();
            var db = FastestProvider.context;
            var hasSplitField = typeof(T).GetProperties().Any(it => it.GetCustomAttribute<SplitFieldAttribute>() != null);
            foreach (var item in datas)
            {
                if (groupModels.Count > 0 && !hasSplitField)
                    groupModels.Add(new GroupModel() { GroupName = groupModels[0].GroupName, Item = item });
                else
                {
                    var value = db.SplitHelper<T>().GetValue(attribute.SplitType, item);
                    var tableName = db.SplitHelper<T>().GetTableName(attribute.SplitType, value);
                    groupModels.Add(new GroupModel() { GroupName = tableName, Item = item });
                }
            }
            result = 0;
        }

        /// <summary>分组模型</summary>
        internal class GroupModel
        {
            /// <summary>分组名称</summary>
            public string GroupName { get; set; }
            /// <summary>数据项</summary>
            public T Item { get; set; }
        }
    }
}