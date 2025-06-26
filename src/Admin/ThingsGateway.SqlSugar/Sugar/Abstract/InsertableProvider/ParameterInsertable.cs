using System.Text;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 参数化可插入实体类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class ParameterInsertable<T> : IParameterInsertable<T> where T : class, new()
    {
        /// <summary>
        /// 内部可插入对象
        /// </summary>
        internal IInsertable<T> Inserable { get; set; }
        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        internal SqlSugarProvider Context { get; set; }
        /// <summary>
        /// 执行插入命令
        /// </summary>
        /// <returns>影响行数</returns>
        public int ExecuteCommand()
        {
            if (this.Context.CurrentConnectionConfig.DbType.IsIn(DbType.Oracle, DbType.Dm))
            {
                return DefaultExecuteCommand();
            }
            else
            {
                return ValuesExecuteCommand();
            }
        }
        /// <summary>
        /// 异步执行插入命令
        /// </summary>
        /// <returns>影响行数</returns>
        public async Task<int> ExecuteCommandAsync()
        {
            if (this.Context.CurrentConnectionConfig.DbType.IsIn(DbType.Oracle, DbType.Dm))
            {
                return await DefaultExecuteCommandAsync().ConfigureAwait(false);
            }
            else
            {
                return await ValuesExecuteCommandAsync().ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 默认执行插入命令
        /// </summary>
        /// <returns>影响行数</returns>
        public int DefaultExecuteCommand()
        {
            int result = 0;
            var inserable = Inserable as InsertableProvider<T>;
            var columns = inserable.InsertBuilder.DbColumnInfoList.GroupBy(it => it.DbColumnName).Select(it => it.Key).ToHashSet();
            var tableWithString = inserable.InsertBuilder.TableWithString;
            var removeCacheFunc = inserable.RemoveCacheFunc;
            var objects = inserable.InsertObjs;
            this.Context.Utilities.PageEach(objects, 60, pagelist =>
            {
                if (this.Context.CurrentConnectionConfig.DbType == DbType.Oracle)
                    this.Context.AddQueue("begin");
                foreach (var item in pagelist)
                {
                    var itemable = this.Context.Insertable(item);
                    itemable.InsertBuilder.DbColumnInfoList = itemable.InsertBuilder.DbColumnInfoList.Where(it => columns.Contains(it.DbColumnName)).ToList();
                    itemable.InsertBuilder.TableWithString = tableWithString;
                    itemable.InsertBuilder.AsName = Inserable.InsertBuilder.AsName;
                    (itemable as InsertableProvider<T>).RemoveCacheFunc = removeCacheFunc;
                    itemable.AddQueue();
                }
                if (this.Context.CurrentConnectionConfig.DbType == DbType.Oracle)
                    this.Context.AddQueue("end \r\n");
                result += this.Context.SaveQueues(false);
            });
            //if (this.Context.CurrentConnectionConfig.DbType == DbType.Oracle)
            //    result=objects.Length;
            if (result == -1)
            {
                result = objects.Length;
            }
            return result;
        }
        /// <summary>
        /// 异步默认执行插入命令
        /// </summary>
        /// <returns>影响行数</returns>
        public async Task<int> DefaultExecuteCommandAsync()
        {
            int result = 0;
            var inserable = Inserable as InsertableProvider<T>;
            var columns = inserable.InsertBuilder.DbColumnInfoList.GroupBy(it => it.DbColumnName).Select(it => it.Key).ToHashSet();
            var tableWithString = inserable.InsertBuilder.TableWithString;
            var removeCacheFunc = inserable.RemoveCacheFunc;
            var objects = inserable.InsertObjs;
            await Context.Utilities.PageEachAsync(objects, 60, async pagelist =>
            {
                if (Context.CurrentConnectionConfig.DbType == DbType.Oracle)
                    Context.AddQueue("begin");
                foreach (var item in pagelist)
                {
                    var itemable = Context.Insertable(item);
                    itemable.InsertBuilder.DbColumnInfoList = itemable.InsertBuilder.DbColumnInfoList.Where(it => columns.Contains(it.DbColumnName)).ToList();
                    itemable.InsertBuilder.TableWithString = tableWithString;
                    itemable.InsertBuilder.AsName = Inserable.InsertBuilder.AsName;
                    (itemable as InsertableProvider<T>).RemoveCacheFunc = removeCacheFunc;
                    itemable.AddQueue();
                }
                if (Context.CurrentConnectionConfig.DbType == DbType.Oracle)
                    Context.AddQueue("end");
                result += await Context.SaveQueuesAsync(false).ConfigureAwait(false);
                if (Context.CurrentConnectionConfig.DbType == DbType.Oracle)
                    result = objects.Length;
                return result;
            }).ConfigureAwait(false);
            return result;
        }
        /// <summary>
        /// 值执行插入命令
        /// </summary>
        /// <returns>影响行数</returns>
        public int ValuesExecuteCommand()
        {

            int result = 0;
            var inserable = Inserable as InsertableProvider<T>;
            var columns = inserable.InsertBuilder.DbColumnInfoList.GroupBy(it => it.DbColumnName).Select(it => it.Key).Distinct().ToList();
            var tableWithString = inserable.InsertBuilder.AsName;
            var removeCacheFunc = inserable.RemoveCacheFunc;
            var objects = inserable.InsertObjs;
            if (objects == null || objects.Length == 0 || (objects.Length == 1 && objects.First() == null))
            {
                return result;
            }
            var identityList = inserable.EntityInfo.Columns.Where(it => it.IsIdentity).Select(it => it.PropertyName).ToArray();
            if (inserable.IsOffIdentity)
            {
                identityList = Array.Empty<string>();
            }
            var pageSize = 100;
            var count = inserable.EntityInfo.Columns.Count;
            pageSize = GetPageSize(pageSize, count);
            this.Context.Utilities.PageEach(objects, pageSize, pagelist =>
            {

                StringBuilder batchInsetrSql;
                List<SugarParameter> allParamter = new List<SugarParameter>();
                GetInsertValues(identityList, columns, tableWithString, removeCacheFunc, pagelist, out batchInsetrSql, allParamter);
                result += this.Context.Ado.ExecuteCommand(batchInsetrSql.ToString(), allParamter);

            });
            return result;

        }

        /// <summary>
        /// 异步值执行插入命令
        /// </summary>
        /// <returns>影响行数</returns>
        public async Task<int> ValuesExecuteCommandAsync()
        {
            int result = 0;
            var inserable = Inserable as InsertableProvider<T>;
            var columns = inserable.InsertBuilder.DbColumnInfoList.GroupBy(it => it.DbColumnName).Select(it => it.Key).Distinct().ToList();
            var tableWithString = inserable.InsertBuilder.AsName;
            var removeCacheFunc = inserable.RemoveCacheFunc;
            var objects = inserable.InsertObjs;
            var identityList = inserable.EntityInfo.Columns.Where(it => it.IsIdentity).Select(it => it.PropertyName).ToArray();
            if (inserable.IsOffIdentity)
            {
                identityList = Array.Empty<string>();
            }
            await Context.Utilities.PageEachAsync(objects, 100, async pagelist =>
            {

                StringBuilder batchInsetrSql;
                List<SugarParameter> allParamter = new List<SugarParameter>();
                GetInsertValues(identityList, columns, tableWithString, removeCacheFunc, pagelist, out batchInsetrSql, allParamter);
                result += await Context.Ado.ExecuteCommandAsync(batchInsetrSql.ToString(), allParamter).ConfigureAwait(false);

            }).ConfigureAwait(false);
            return result;
        }
        #region Values Helper

        /// <summary>
        /// 获取分页大小
        /// </summary>
        private static int GetPageSize(int pageSize, int count)
        {
            if (pageSize * count > 2100)
            {
                pageSize = 50;
            }
            if (pageSize * count > 2100)
            {
                pageSize = 20;
            }
            if (pageSize * count > 2100)
            {
                pageSize = 10;
            }

            return pageSize;
        }
        /// <summary>
        /// 获取插入值
        /// </summary>
        private void GetInsertValues(string[] identitys, List<string> columns, string tableWithString, Action removeCacheFunc, List<T> items, out StringBuilder batchInsetrSql, List<SugarParameter> allParamter)
        {
            var itemable = this.Context.Insertable(items);
            itemable.InsertBuilder.DbColumnInfoList = itemable.InsertBuilder.DbColumnInfoList.Where(it => columns.Contains(it.DbColumnName)).ToList();
            itemable.InsertBuilder.TableWithString = tableWithString;
            (itemable as InsertableProvider<T>).RemoveCacheFunc = removeCacheFunc;
            batchInsetrSql = new StringBuilder();
            batchInsetrSql.Append("INSERT INTO " + itemable.InsertBuilder.GetTableNameString + " ");
            batchInsetrSql.Append('(');
            var groupList = itemable.InsertBuilder.DbColumnInfoList.Where(it => !identitys.Contains(it.PropertyName)).GroupBy(it => it.TableId).ToList();
            string columnsString = string.Join(",", groupList.First().Select(it => itemable.InsertBuilder.Builder.GetTranslationColumnName(it.DbColumnName)));
            batchInsetrSql.Append(columnsString);
            batchInsetrSql.Append(") VALUES");
            string insertColumns = "";
            foreach (var gitem in groupList)
            {
                batchInsetrSql.Append('(');
                insertColumns = string.Join(",", gitem.Select(it => FormatValue(it.PropertyType, it.DbColumnName, it.Value, allParamter, itemable.InsertBuilder.Builder.SqlParameterKeyWord)));
                batchInsetrSql.Append(insertColumns);
                if (groupList.Last() == gitem)
                {
                    batchInsetrSql.Append(") ");
                }
                else
                {
                    batchInsetrSql.Append("),  ");
                }
            }
        }
        /// <summary>
        /// 格式化值
        /// </summary>
        private string FormatValue(Type type, string name, object value, List<SugarParameter> allParamter, string keyword)
        {
            var result = keyword + name + allParamter.Count;
            var addParameter = new SugarParameter(result, value, type);
            allParamter.Add(addParameter);
            return result;
        }
        #endregion
    }
}