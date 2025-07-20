using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 标签插入表操作类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class TagInserttable<T> where T : class, new()
    {
        /// <summary>
        /// 可插入对象
        /// </summary>
        internal IInsertable<T> thisValue;

        /// <summary>
        /// 获取子表名称的委托
        /// </summary>
        internal Func<string, T, string> getChildTableNamefunc;

        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        internal SqlSugarProvider Context;

        /// <summary>
        /// 执行插入命令
        /// </summary>
        /// <returns>影响的行数</returns>
        public int ExecuteCommand()
        {
            var provider = (InsertableProvider<T>)thisValue;
            var inserObjects = provider.InsertObjs;
            var attr = GetCommonSTableAttribute(typeof(T).GetCustomAttribute<STableAttribute>());
            Check.ExceptionEasy(attr == null || attr?.Tag1 == null, $"", $"{nameof(T)}缺少特性STableAttribute和Tag1");
            // 根据所有非空的 Tag 进行分组
            var groups = GetGroupInfos(inserObjects, attr);
            foreach (var item in groups)
            {
                var childTableName = getChildTableNamefunc(attr.STableName, item.First());
                this.Context.Utilities.PageEach(item, 500, pageItems =>
                {
                    var sTableName = provider.SqlBuilder.GetTranslationColumnName(attr.STableName);
                    var tags = new List<string>();
                    List<string> tagValues = GetTagValues(pageItems, attr);
                    var tagString = string.Join(",", tagValues.Where(v => !string.IsNullOrEmpty(v)).Select(v => $"'{v.ToSqlFilter()}'"));
                    tags.Add(tagString);
                    this.Context.Ado.ExecuteCommand($"CREATE TABLE IF NOT EXISTS {childTableName} USING {sTableName} TAGS ({tagString})");
                    this.Context.Insertable(pageItems).IgnoreColumns(GetTagNames(pageItems.First(), attr)).AS(childTableName).ExecuteCommand();
                });
            }
            return inserObjects.Count;
        }

        /// <summary>
        /// 异步执行插入命令
        /// </summary>
        /// <returns>影响的行数</returns>
        public async Task<int> ExecuteCommandAsync()
        {
            var provider = (InsertableProvider<T>)thisValue;
            var inserObjects = provider.InsertObjs;
            var attr = GetCommonSTableAttribute(typeof(T).GetCustomAttribute<STableAttribute>());
            Check.ExceptionEasy(attr == null || attr?.Tag1 == null, $"", $"{nameof(T)}缺少特性STableAttribute和Tag1");
            // 根据所有非空的 Tag 进行分组
            var groups = GetGroupInfos(inserObjects, attr);
            foreach (var item in groups)
            {
                var childTableName = getChildTableNamefunc(attr.STableName, item.First());
                await this.Context.Utilities.PageEachAsync(item, 500, async pageItems =>
                {
                    var sTableName = provider.SqlBuilder.GetTranslationColumnName(attr.STableName);
                    var tags = new List<string>();
                    List<string> tagValues = GetTagValues(pageItems, attr);
                    var tagString = string.Join(",", tagValues.Where(v => !string.IsNullOrEmpty(v)).Select(v => $"'{v.ToSqlFilter()}'"));
                    tags.Add(tagString);
                    await Context.Ado.ExecuteCommandAsync($"CREATE TABLE IF NOT EXISTS {childTableName} USING {sTableName} TAGS ({tagString})").ConfigureAwait(false);
                    await Context.Insertable(pageItems).IgnoreColumns(GetTagNames(pageItems.First(), attr)).AS(childTableName).ExecuteCommandAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            return inserObjects.Count;
        }

        /// <summary>
        /// 获取标签值列表
        /// </summary>
        /// <param name="pageItems">当前页数据</param>
        /// <param name="attr">STable特性</param>
        /// <returns>标签值列表</returns>
        private static List<string> GetTagValues(List<T> pageItems, STableAttribute attr)
        {
            var tagValues = new List<string>();
            var obj = pageItems.First();
            if (attr.Tag1 != null)
                tagValues.Add(obj.GetType().GetProperty(attr.Tag1)?.GetValue(obj)?.ToString());

            if (attr.Tag2 != null)
                tagValues.Add(obj.GetType().GetProperty(attr.Tag2)?.GetValue(obj)?.ToString());

            if (attr.Tag3 != null)
                tagValues.Add(obj.GetType().GetProperty(attr.Tag3)?.GetValue(obj)?.ToString());

            if (attr.Tag4 != null)
                tagValues.Add(obj.GetType().GetProperty(attr.Tag4)?.GetValue(obj)?.ToString());
            return tagValues;
        }

        /// <summary>
        /// 获取标签名称列表
        /// </summary>
        /// <param name="obj">实体对象</param>
        /// <param name="attr">STable特性</param>
        /// <returns>标签名称列表</returns>
        private static List<string> GetTagNames(T obj, STableAttribute attr)
        {
            var tagValues = new List<string>();
            if (attr.Tag1 != null)
                tagValues.Add(attr.Tag1);

            if (attr.Tag2 != null)
                tagValues.Add(attr.Tag2);

            if (attr.Tag3 != null)
                tagValues.Add(attr.Tag3);

            if (attr.Tag4 != null)
                tagValues.Add(attr.Tag4);
            return tagValues;
        }

        /// <summary>
        /// 获取分组信息
        /// </summary>
        /// <param name="inserObjects">插入对象数组</param>
        /// <param name="attr">STable特性</param>
        /// <returns>分组结果</returns>
        private static IEnumerable<IGrouping<string, T>> GetGroupInfos(IReadOnlyList<T> inserObjects, STableAttribute? attr)
        {
            var groups = inserObjects.GroupBy(it =>
            {
                // 动态生成分组键
                var groupKey = new List<string>();

                if (attr.Tag1 != null)
                    groupKey.Add(it.GetType().GetProperty(attr.Tag1)?.GetValue(it)?.ToString());

                if (attr.Tag2 != null)
                    groupKey.Add(it.GetType().GetProperty(attr.Tag2)?.GetValue(it)?.ToString());

                if (attr.Tag3 != null)
                    groupKey.Add(it.GetType().GetProperty(attr.Tag3)?.GetValue(it)?.ToString());

                if (attr.Tag4 != null)
                    groupKey.Add(it.GetType().GetProperty(attr.Tag4)?.GetValue(it)?.ToString());

                // 将非空的 Tag 值用下划线连接作为分组键
                return string.Join("_", groupKey.Where(k => !string.IsNullOrEmpty(k)));
            });
            return groups;
        }

        /// <summary>
        /// 获取通用STable特性
        /// </summary>
        /// <param name="sTableAttribute">STable特性</param>
        /// <returns>STable特性</returns>
        private STableAttribute GetCommonSTableAttribute(STableAttribute sTableAttribute)
        {
            return TaosUtilMethods.GetCommonSTableAttribute(this.Context, sTableAttribute);
        }
    }
}