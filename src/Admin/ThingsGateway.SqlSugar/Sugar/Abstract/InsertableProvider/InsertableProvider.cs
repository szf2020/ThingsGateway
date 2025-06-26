using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 可插入数据提供者泛型类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public partial class InsertableProvider<T> : IInsertable<T> where T : class, new()
    {
        /// <summary>
        /// SqlSugar上下文对象
        /// </summary>
        public SqlSugarProvider Context { get; set; }
        /// <summary>
        /// 数据库访问对象
        /// </summary>
        public IAdo Ado { get { return Context.Ado; } }
        /// <summary>
        /// SQL构建器
        /// </summary>
        public ISqlBuilder SqlBuilder { get; set; }
        /// <summary>
        /// 插入构建器
        /// </summary>
        public InsertBuilder InsertBuilder { get; set; }

        /// <summary>
        /// 是否映射表
        /// </summary>
        public bool IsMappingTable { get { return this.Context.MappingTables?.Any() == true; } }
        /// <summary>
        /// 是否映射列
        /// </summary>
        public bool IsMappingColumns { get { return this.Context.MappingColumns?.Any() == true; } }
        /// <summary>
        /// 是否单条数据
        /// </summary>
        public bool IsSingle { get { return this.InsertObjs.Length == 1; } }

        /// <summary>
        /// 实体信息
        /// </summary>
        public EntityInfo EntityInfo { get; set; }
        /// <summary>
        /// 映射列列表
        /// </summary>
        public List<MappingColumn> MappingColumnList { get; set; }
        /// <summary>
        /// 忽略列名列表
        /// </summary>
        private List<string> IgnoreColumnNameList { get; set; }
        /// <summary>
        /// 是否关闭自增
        /// </summary>
        internal bool IsOffIdentity { get; set; }
        /// <summary>
        /// 插入对象数组
        /// </summary>
        public T[] InsertObjs { get; set; }

        /// <summary>
        /// 旧映射表列表
        /// </summary>
        public MappingTableList OldMappingTableList { get; set; }
        /// <summary>
        /// 是否使用AS别名
        /// </summary>
        public bool IsAs { get; set; }
        /// <summary>
        /// 是否启用差异日志事件
        /// </summary>
        public bool IsEnableDiffLogEvent { get; set; }
        /// <summary>
        /// 差异日志模型
        /// </summary>
        public DiffLogModel diffModel { get; set; }
        /// <summary>
        /// 移除缓存函数
        /// </summary>
        internal Action RemoveCacheFunc { get; set; }


        #region Core
        /// <summary>
        /// 添加到队列
        /// </summary>
        public void AddQueue()
        {
            if (this.InsertObjs?.Length > 0 && this.InsertObjs[0] != null)
            {
                var sqlObj = this.ToSql();
                this.Context.Queues.Add(sqlObj.Key, sqlObj.Value);
            }
        }
        /// <summary>
        /// 执行插入命令
        /// </summary>
        /// <returns>影响行数</returns>
        public virtual int ExecuteCommand()
        {
            if (this.InsertObjs.Length == 1 && this.InsertObjs.First() == null)
            {
                return 0;
            }
            string sql = _ExecuteCommand();
            var result = Ado.ExecuteCommand(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray());
            After(sql, null);
            if (result == -1) return this.InsertObjs.Length;
            return result;
        }
        /// <summary>
        /// 获取SQL字符串
        /// </summary>
        /// <returns>SQL语句</returns>
        public virtual string ToSqlString()
        {
            var sqlObj = this.ToSql();
            var result = sqlObj.Key;
            if (result == null) return null;
            result = UtilMethods.GetSqlString(this.Context.CurrentConnectionConfig, sqlObj);
            return result;
        }
        /// <summary>
        /// 转换为SQL
        /// </summary>
        /// <returns>键值对形式的SQL和参数</returns>
        public virtual KeyValuePair<string, List<SugarParameter>> ToSql()
        {
            InsertBuilder.IsReturnIdentity = true;
            if (this.SqlBuilder.SqlParameterKeyWord == ":" && !this.EntityInfo.Columns.Any(it => it.IsIdentity))
            {
                InsertBuilder.IsReturnIdentity = false;
            }
            PreToSql();
            AutoRemoveDataCache();
            string sql = InsertBuilder.ToSqlString();
            RestoreMapping();
            return new KeyValuePair<string, List<SugarParameter>>(sql, InsertBuilder.Parameters);
        }
        /// <summary>
        /// 异步执行并返回主键列表
        /// </summary>
        /// <typeparam name="Type">主键类型</typeparam>
        /// <returns>主键列表</returns>
        public async Task<List<Type>> ExecuteReturnPkListAsync<Type>()
        {
            return await Task.Run(() => ExecuteReturnPkList<Type>()).ConfigureAwait(false);
        }
        /// <summary>
        /// 执行并返回主键列表
        /// </summary>
        /// <typeparam name="Type">主键类型</typeparam>
        /// <returns>主键列表</returns>
        public virtual List<Type> ExecuteReturnPkList<Type>()
        {
            var pkInfo = this.EntityInfo.Columns.FirstOrDefault(it => it.IsPrimarykey == true);
            Check.ExceptionEasy(pkInfo == null, "ExecuteReturnPkList need primary key", "ExecuteReturnPkList需要主键");
            Check.ExceptionEasy(this.EntityInfo.Columns.Count(it => it.IsPrimarykey == true) > 1, "ExecuteReturnPkList ，Only support technology single primary key", "ExecuteReturnPkList只支技单主键");
            var isIdEntity = pkInfo.IsIdentity || (pkInfo.OracleSequenceName.HasValue() && this.Context.CurrentConnectionConfig.DbType == DbType.Oracle);
            if (isIdEntity && this.InsertObjs.Length == 1)
            {
                return InsertPkListIdentityCount1<Type>(pkInfo);
            }
            else if (isIdEntity && this.InsertBuilder.ConvertInsertReturnIdFunc == null)
            {
                return InsertPkListNoFunc<Type>(pkInfo);
            }
            else if (isIdEntity && this.InsertBuilder.ConvertInsertReturnIdFunc != null)
            {
                return InsertPkListWithFunc<Type>(pkInfo);
            }
            else if (pkInfo.UnderType == UtilConstants.LongType)
            {
                return InsertPkListLong<Type>();
            }
            else
            {
                return InsertPkListGuid<Type>(pkInfo);
            }
        }

        /// <summary>
        /// 执行并返回自增ID
        /// </summary>
        /// <returns>自增ID</returns>
        public virtual int ExecuteReturnIdentity()
        {
            if (this.InsertObjs.Length == 1 && this.InsertObjs.First() == null)
            {
                return 0;
            }
            string sql = _ExecuteReturnIdentity();
            var result = 0;
            if (InsertBuilder.IsOleDb)
            {
                var isAuto = false;
                if (this.Context.Ado.IsAnyTran() == false && this.Context.CurrentConnectionConfig.IsAutoCloseConnection)
                {
                    isAuto = this.Context.CurrentConnectionConfig.IsAutoCloseConnection;
                    this.Context.CurrentConnectionConfig.IsAutoCloseConnection = false;
                }
                result = Ado.GetInt(sql.Split(';').First(), InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray());
                result = Ado.GetInt(sql.Split(';').Last(), InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray());
                if (this.Context.Ado.IsAnyTran() == false && isAuto)
                {
                    this.Ado.Close();
                    this.Context.CurrentConnectionConfig.IsAutoCloseConnection = isAuto;
                }
            }
            else
            {
                result = Ado.GetInt(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray());
            }
            After(sql, result);
            return result;
        }

        /// <summary>
        /// 执行并返回大数自增ID
        /// </summary>
        /// <returns>自增ID</returns>
        public virtual long ExecuteReturnBigIdentity()
        {
            if (this.InsertObjs.Length == 1 && this.InsertObjs.First() == null)
            {
                return 0;
            }
            string sql = _ExecuteReturnBigIdentity();
            long result = 0;
            if (InsertBuilder.IsOleDb)
            {
                var isAuto = false;
                if (this.Context.Ado.IsAnyTran() == false && this.Context.CurrentConnectionConfig.IsAutoCloseConnection)
                {
                    isAuto = this.Context.CurrentConnectionConfig.IsAutoCloseConnection;
                    this.Context.CurrentConnectionConfig.IsAutoCloseConnection = false;
                }
                result = Convert.ToInt64(Ado.GetScalar(sql.Split(';').First(), InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray()));
                result = Convert.ToInt64(Ado.GetScalar(sql.Split(';').Last(), InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray()));
                if (this.Context.Ado.IsAnyTran() == false && isAuto)
                {
                    this.Ado.Close();
                    this.Context.CurrentConnectionConfig.IsAutoCloseConnection = isAuto;
                }
            }
            else
            {
                result = (Ado.GetScalar(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray())).ObjToLong();
            }
            After(sql, result);
            return result;
        }

        /// <summary>
        /// 执行并返回雪花ID
        /// </summary>
        /// <returns>雪花ID</returns>
        public virtual long ExecuteReturnSnowflakeId()
        {
            if (this.InsertObjs.Length > 1)
            {
                return this.ExecuteReturnSnowflakeIdList().First();
            }

            var id = SnowFlakeSingle.instance.getID();
            var entity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            var snowProperty = entity.Columns.FirstOrDefault(it => it.IsPrimarykey && it.PropertyInfo.PropertyType == UtilConstants.LongType);
            Check.Exception(snowProperty == null, "The entity sets the primary key and is long");
            Check.Exception(snowProperty.IsIdentity == true, "SnowflakeId IsIdentity can't true");
            foreach (var item in this.InsertBuilder.DbColumnInfoList.Where(it => it.PropertyName == snowProperty.PropertyName))
            {
                item.Value = id;
                snowProperty?.PropertyInfo.SetValue(this.InsertObjs.First(), id);
            }
            this.ExecuteCommand();
            return id;
        }
        /// <summary>
        /// 执行并返回雪花ID列表
        /// </summary>
        /// <returns>雪花ID列表</returns>
        public List<long> ExecuteReturnSnowflakeIdList()
        {
            List<long> result = new List<long>();
            var entity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            var snowProperty = entity.Columns.FirstOrDefault(it => it.IsPrimarykey && it.PropertyInfo.PropertyType == UtilConstants.LongType);
            Check.Exception(snowProperty == null, "The entity sets the primary key and is long");
            Check.Exception(snowProperty.IsIdentity == true, "SnowflakeId IsIdentity can't true");
            foreach (var item in this.InsertBuilder.DbColumnInfoList.Where(it => it.PropertyName == snowProperty.PropertyName))
            {
                var id = SnowFlakeSingle.instance.getID();
                item.Value = id;
                result.Add(id);
                var obj = this.InsertObjs.ElementAtOrDefault(item.TableId);
                if (obj != null)
                {
                    snowProperty?.PropertyInfo.SetValue(obj, id);
                }
            }
            this.ExecuteCommand();
            return result;
        }
        /// <summary>
        /// 异步执行并返回雪花ID
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <returns>雪花ID</returns>
        public Task<long> ExecuteReturnSnowflakeIdAsync(CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return ExecuteReturnSnowflakeIdAsync();
        }
        /// <summary>
        /// 异步执行并返回雪花ID
        /// </summary>
        /// <returns>雪花ID</returns>
        public async Task<long> ExecuteReturnSnowflakeIdAsync()
        {
            var id = SnowFlakeSingle.instance.getID();
            var entity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            var snowProperty = entity.Columns.FirstOrDefault(it => it.IsPrimarykey && it.PropertyInfo.PropertyType == UtilConstants.LongType);
            Check.Exception(snowProperty == null, "The entity sets the primary key and is long");
            Check.Exception(snowProperty.IsIdentity == true, "SnowflakeId IsIdentity can't true");
            foreach (var item in this.InsertBuilder.DbColumnInfoList.Where(it => it.PropertyName == snowProperty.PropertyName))
            {
                item.Value = id;
                snowProperty?.PropertyInfo.SetValue(this.InsertObjs.First(), id);
            }
            await ExecuteCommandAsync().ConfigureAwait(false);
            return id;
        }
        /// <summary>
        /// 异步执行并返回雪花ID列表
        /// </summary>
        /// <returns>雪花ID列表</returns>
        public async Task<List<long>> ExecuteReturnSnowflakeIdListAsync()
        {
            List<long> result = new List<long>();
            var entity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            var snowProperty = entity.Columns.FirstOrDefault(it => it.IsPrimarykey && it.PropertyInfo.PropertyType == UtilConstants.LongType);
            Check.Exception(snowProperty == null, "The entity sets the primary key and is long");
            Check.Exception(snowProperty.IsIdentity == true, "SnowflakeId IsIdentity can't true");
            foreach (var item in this.InsertBuilder.DbColumnInfoList.Where(it => it.PropertyName == snowProperty.PropertyName))
            {
                var id = SnowFlakeSingle.instance.getID();
                item.Value = id;
                result.Add(id);
                var obj = this.InsertObjs.ElementAtOrDefault(item.TableId);
                if (obj != null)
                {
                    snowProperty?.PropertyInfo.SetValue(obj, id);
                }
            }
            await ExecuteCommandAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// 异步执行并返回雪花ID列表
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <returns>雪花ID列表</returns>
        public Task<List<long>> ExecuteReturnSnowflakeIdListAsync(CancellationToken token)
        {
            this.Ado.CancellationToken = token;
            return ExecuteReturnSnowflakeIdListAsync();
        }

        /// <summary>
        /// 执行并返回实体
        /// </summary>
        /// <returns>插入的实体</returns>
        public virtual T ExecuteReturnEntity()
        {
            ExecuteCommandIdentityIntoEntity();
            return InsertObjs.First();
        }
        /// <summary>
        /// 执行命令并将自增ID设置到实体
        /// </summary>
        /// <returns>是否成功</returns>
        public virtual bool ExecuteCommandIdentityIntoEntity()
        {
            var result = InsertObjs.First();
            var identityKeys = GetIdentityKeys();
            if (this.Context?.CurrentConnectionConfig?.MoreSettings?.EnableOracleIdentity == true)
            {
                var identity = this.EntityInfo.Columns.FirstOrDefault(it => it.IsIdentity);
                if (identity != null)
                {
                    identityKeys = new List<string>() { identity.DbColumnName };
                }
            }
            if (identityKeys.Count == 0)
            {
                var snowColumn = this.EntityInfo.Columns.FirstOrDefault(it => it.IsPrimarykey && it.UnderType == UtilConstants.LongType);
                if (snowColumn != null)
                {
                    if (Convert.ToInt64(snowColumn.PropertyInfo.GetValue(result)) == 0)
                    {
                        var id = this.ExecuteReturnSnowflakeId();
                        snowColumn.PropertyInfo.SetValue(result, id);
                    }
                    else
                    {
                        ExecuteCommand();
                    }
                    return true;
                }
                else
                {
                    return this.ExecuteCommand() > 0;
                }
            }
            var idValue = ExecuteReturnBigIdentity();
            Check.Exception(identityKeys.Count > 1, "ExecuteCommandIdentityIntoEntity does not support multiple identity keys");
            var identityKey = identityKeys.First();
            object setValue = 0;
            if (idValue > int.MaxValue)
                setValue = idValue;
            else if (this.EntityInfo.Columns.Any(it => it.IsIdentity && it.PropertyInfo.PropertyType == typeof(uint)))
            {
                setValue = Convert.ToUInt32(idValue);
            }
            else if (this.EntityInfo.Columns.Any(it => it.IsIdentity && it.PropertyInfo.PropertyType == typeof(ulong)))
            {
                setValue = Convert.ToUInt64(idValue);
            }
            else if (this.EntityInfo.Columns.Any(it => it.IsIdentity && it.PropertyInfo.PropertyType == typeof(ushort)))
            {
                setValue = Convert.ToUInt16(idValue);
            }
            else if (this.EntityInfo.Columns.Any(it => it.IsIdentity && it.PropertyInfo.PropertyType == typeof(short)))
            {
                setValue = Convert.ToInt16(idValue);
            }
            else
                setValue = Convert.ToInt32(idValue);
            this.Context.EntityMaintenance.GetProperty<T>(identityKey).SetValue(result, setValue, null);
            return idValue > 0;
        }
        /// <summary>
        /// 异步执行命令
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <returns>影响行数</returns>
        public Task<int> ExecuteCommandAsync(CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return ExecuteCommandAsync();
        }
        /// <summary>
        /// 异步执行命令
        /// </summary>
        /// <returns>影响行数</returns>
        public async Task<int> ExecuteCommandAsync()
        {
            if (this.InsertObjs.Length == 1 && this.InsertObjs.First() == null)
            {
                return 0;
            }
            string sql = _ExecuteCommand();
            var result = await Ado.ExecuteCommandAsync(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray()).ConfigureAwait(false);
            After(sql, null);
            if (result == -1) return this.InsertObjs.Length;
            return result;
        }
        /// <summary>
        /// 异步执行并返回自增ID
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <returns>自增ID</returns>
        public Task<int> ExecuteReturnIdentityAsync(CancellationToken token)
        {
            this.Ado.CancellationToken = token;
            return ExecuteReturnIdentityAsync();
        }
        /// <summary>
        /// 异步执行并返回自增ID
        /// </summary>
        /// <returns>自增ID</returns>
        public virtual async Task<int> ExecuteReturnIdentityAsync()
        {
            if (this.InsertObjs.Length == 1 && this.InsertObjs.First() == null)
            {
                return 0;
            }
            string sql = _ExecuteReturnIdentity();
            var result = 0;
            if (InsertBuilder.IsOleDb)
            {
                var isAuto = false;
                if (this.Context.Ado.IsAnyTran() == false && this.Context.CurrentConnectionConfig.IsAutoCloseConnection)
                {
                    isAuto = this.Context.CurrentConnectionConfig.IsAutoCloseConnection;
                    this.Context.CurrentConnectionConfig.IsAutoCloseConnection = false;
                }
                result = Ado.GetInt(sql.Split(';').First(), InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray());
                result = Ado.GetInt(sql.Split(';').Last(), InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray());
                if (this.Context.Ado.IsAnyTran() == false && isAuto)
                {
                    this.Ado.Close();
                    this.Context.CurrentConnectionConfig.IsAutoCloseConnection = isAuto;
                }
            }
            else
            {
                result = await Ado.GetIntAsync(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray()).ConfigureAwait(false);
            }
            After(sql, result);
            return result;
        }
        /// <summary>
        /// 执行并返回包含所有第一层导航属性的实体
        /// </summary>
        /// <param name="isIncludesAllFirstLayer">是否包含所有第一层导航属性</param>
        /// <returns>插入的实体</returns>
        public T ExecuteReturnEntity(bool isIncludesAllFirstLayer)
        {
            var data = ExecuteReturnEntity();
            if (this.InsertBuilder.IsWithAttr)
            {
                return this.Context.Root.QueryableWithAttr<T>().WhereClassByPrimaryKey(data).IncludesAllFirstLayer().First();
            }
            else
            {
                return this.Context.Queryable<T>().WhereClassByPrimaryKey(data).IncludesAllFirstLayer().First();
            }
        }
        /// <summary>
        /// 异步执行并返回实体
        /// </summary>
        /// <returns>插入的实体</returns>
        public async Task<T> ExecuteReturnEntityAsync()
        {
            await ExecuteCommandIdentityIntoEntityAsync().ConfigureAwait(false);
            return InsertObjs.First();
        }
        /// <summary>
        /// 异步执行并返回包含所有第一层导航属性的实体
        /// </summary>
        /// <param name="isIncludesAllFirstLayer">是否包含所有第一层导航属性</param>
        /// <returns>插入的实体</returns>
        public async Task<T> ExecuteReturnEntityAsync(bool isIncludesAllFirstLayer)
        {
            var data = await ExecuteReturnEntityAsync().ConfigureAwait(false);
            if (this.InsertBuilder.IsWithAttr)
            {
                return await Context.Root.QueryableWithAttr<T>().WhereClassByPrimaryKey(data).IncludesAllFirstLayer().FirstAsync().ConfigureAwait(false);
            }
            else
            {
                return await Context.Queryable<T>().WhereClassByPrimaryKey(data).IncludesAllFirstLayer().FirstAsync().ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 异步执行命令并将自增ID设置到实体
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> ExecuteCommandIdentityIntoEntityAsync()
        {
            var result = InsertObjs.First();
            var identityKeys = GetIdentityKeys();
            if (identityKeys.Count == 0)
            {
                var snowColumn = this.EntityInfo.Columns.FirstOrDefault(it => it.IsPrimarykey && it.UnderType == UtilConstants.LongType);
                if (snowColumn != null)
                {

                    if (Convert.ToInt64(snowColumn.PropertyInfo.GetValue(result)) == 0)
                    {
                        var id = await ExecuteReturnSnowflakeIdAsync().ConfigureAwait(false);
                        snowColumn.PropertyInfo.SetValue(result, id);
                    }
                    else
                    {
                        await ExecuteCommandAsync().ConfigureAwait(false);
                    }
                    return true;
                }
                else
                {
                    return await ExecuteCommandAsync().ConfigureAwait(false) > 0;
                }
            }
            var idValue = await ExecuteReturnBigIdentityAsync().ConfigureAwait(false);
            Check.Exception(identityKeys.Count > 1, "ExecuteCommandIdentityIntoEntity does not support multiple identity keys");
            var identityKey = identityKeys.First();
            object setValue = 0;
            if (idValue > int.MaxValue)
                setValue = idValue;
            else if (this.EntityInfo.Columns.Any(it => it.IsIdentity && it.PropertyInfo.PropertyType == typeof(uint)))
            {
                setValue = Convert.ToUInt32(idValue);
            }
            else if (this.EntityInfo.Columns.Any(it => it.IsIdentity && it.PropertyInfo.PropertyType == typeof(ulong)))
            {
                setValue = Convert.ToUInt64(idValue);
            }
            else if (this.EntityInfo.Columns.Any(it => it.IsIdentity && it.PropertyInfo.PropertyType == typeof(ushort)))
            {
                setValue = Convert.ToUInt16(idValue);
            }
            else if (this.EntityInfo.Columns.Any(it => it.IsIdentity && it.PropertyInfo.PropertyType == typeof(short)))
            {
                setValue = Convert.ToInt16(idValue);
            }
            else
                setValue = Convert.ToInt32(idValue);
            this.Context.EntityMaintenance.GetProperty<T>(identityKey).SetValue(result, setValue, null);
            return idValue > 0;
        }
        /// <summary>
        /// 异步执行并返回大数自增ID
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <returns>自增ID</returns>
        public Task<long> ExecuteReturnBigIdentityAsync(CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return ExecuteReturnBigIdentityAsync();
        }
        /// <summary>
        /// 异步执行并返回大数自增ID
        /// </summary>
        /// <returns>自增ID</returns>
        public virtual async Task<long> ExecuteReturnBigIdentityAsync()
        {
            if (this.InsertObjs.Length == 1 && this.InsertObjs.First() == null)
            {
                return 0;
            }
            string sql = _ExecuteReturnBigIdentity();
            long result = 0;
            if (InsertBuilder.IsOleDb)
            {
                var isAuto = false;
                if (this.Context.Ado.IsAnyTran() == false && this.Context.CurrentConnectionConfig.IsAutoCloseConnection)
                {
                    isAuto = this.Context.CurrentConnectionConfig.IsAutoCloseConnection;
                    this.Context.CurrentConnectionConfig.IsAutoCloseConnection = false;
                }
                result = Ado.GetInt(sql.Split(';').First(), InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray());
                result = Ado.GetInt(sql.Split(';').Last(), InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray());
                if (this.Context.Ado.IsAnyTran() == false && isAuto)
                {
                    this.Ado.Close();
                    this.Context.CurrentConnectionConfig.IsAutoCloseConnection = isAuto;
                }
            }
            else
            {
                result = (await Ado.GetScalarAsync(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray()).ConfigureAwait(false)).ObjToLong();
            }
            After(sql, result);
            return result;
        }

        #endregion

        #region Setting
        /// <summary>
        /// 设置分页大小
        /// </summary>
        /// <param name="pageSize">分页大小</param>
        /// <returns>可分页插入对象</returns>
        public InsertablePage<T> PageSize(int pageSize)
        {
            InsertablePage<T> result = new InsertablePage<T>();
            result.PageSize = pageSize;
            result.Context = this.Context;
            result.DataList = this.InsertObjs;
            result.TableName = this.InsertBuilder.AsName;
            result.IsEnableDiffLogEvent = this.IsEnableDiffLogEvent;
            result.DiffModel = this.diffModel;
            result.IsMySqlIgnore = this.InsertBuilder.MySqlIgnore;
            result.IsOffIdentity = this.InsertBuilder.IsOffIdentity;
            if (this.InsertBuilder.DbColumnInfoList.Count != 0)
                result.InsertColumns = this.InsertBuilder.DbColumnInfoList.GroupBy(it => it.TableId).First().Select(it => it.DbColumnName).ToList();
            return result;
        }
        /// <summary>
        /// 使用参数化
        /// </summary>
        /// <returns>可参数化插入对象</returns>
        public IParameterInsertable<T> UseParameter()
        {
            var result = new ParameterInsertable<T>();
            result.Context = this.Context;
            result.Inserable = this;
            return result;
        }
        /// <summary>
        /// 指定表名类型
        /// </summary>
        /// <param name="tableNameType">表名类型</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> AsType(Type tableNameType)
        {
            return AS(this.Context.EntityMaintenance.GetEntityInfo(tableNameType).DbTableName);
        }
        /// <summary>
        /// 指定表名
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> AS(string tableName)
        {
            this.InsertBuilder.AsName = tableName;
            return this; ;
        }
        /// <summary>
        /// 忽略指定列
        /// </summary>
        /// <param name="columns">列表达式</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> IgnoreColumns(Expression<Func<T, object>> columns)
        {
            if (columns == null) return this;
            var ignoreColumns = InsertBuilder.GetExpressionValue(columns, ResolveExpressType.ArraySingle).GetResultArray().Select(it => this.SqlBuilder.GetNoTranslationColumnName(it)).ToList();
            this.InsertBuilder.DbColumnInfoList = this.InsertBuilder.DbColumnInfoList.Where(it => !ignoreColumns.Any(ig => ig.Equals(it.PropertyName, StringComparison.CurrentCultureIgnoreCase))).ToList();
            this.InsertBuilder.DbColumnInfoList = this.InsertBuilder.DbColumnInfoList.Where(it => !ignoreColumns.Any(ig => ig.Equals(it.DbColumnName, StringComparison.CurrentCultureIgnoreCase))).ToList();
            return this;
        }
        /// <summary>
        /// 忽略指定列
        /// </summary>
        /// <param name="columns">列名数组</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> IgnoreColumns(params string[] columns)
        {
            if (columns == null)
                columns = Array.Empty<string>();
            this.InsertBuilder.DbColumnInfoList = this.InsertBuilder.DbColumnInfoList.Where(it => !columns.Any(ig => ig.Equals(it.PropertyName, StringComparison.CurrentCultureIgnoreCase))).ToList();
            this.InsertBuilder.DbColumnInfoList = this.InsertBuilder.DbColumnInfoList.Where(it => !columns.Any(ig => ig.Equals(it.DbColumnName, StringComparison.CurrentCultureIgnoreCase))).ToList();
            return this;
        }

        /// <summary>
        /// 忽略空值列
        /// </summary>
        /// <param name="isIgnoreNull">是否忽略空值</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> IgnoreColumnsNull(bool isIgnoreNull = true)
        {
            if (isIgnoreNull)
            {
                Check.Exception(this.InsertObjs.Length > 1, ErrorMessage.GetThrowMessage("ignoreNullColumn NoSupport batch insert, use .PageSize(1).IgnoreColumnsNull().ExecuteCommand()", "ignoreNullColumn 不支持批量操作,你可以用PageSzie(1).IgnoreColumnsNull().ExecuteCommand()"));
                this.InsertBuilder.IsNoInsertNull = true;
            }
            return this;
        }
        /// <summary>
        /// PostgreSQL冲突时不执行任何操作
        /// </summary>
        /// <param name="columns">列名数组</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> PostgreSQLConflictNothing(string[] columns)
        {
            this.InsertBuilder.ConflictNothing = columns;
            return this;
        }
        /// <summary>
        /// MySQL忽略错误
        /// </summary>
        /// <returns>可插入对象</returns>
        public IInsertable<T> MySqlIgnore()
        {
            this.InsertBuilder.MySqlIgnore = true;
            return this;
        }
        /// <summary>
        /// MySQL忽略错误
        /// </summary>
        /// <param name="isIgnore">是否忽略</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> MySqlIgnore(bool isIgnore)
        {
            if (isIgnore)
            {
                return MySqlIgnore();
            }
            else
            {
                return this;
            }
        }
        /// <summary>
        /// 指定插入列
        /// </summary>
        /// <param name="columns">列表达式</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> InsertColumns(Expression<Func<T, object>> columns)
        {
            if (columns == null) return this;
            var ignoreColumns = InsertBuilder.GetExpressionValue(columns, ResolveExpressType.ArraySingle).GetResultArray().Select(it => this.SqlBuilder.GetNoTranslationColumnName(it)).ToList();
            this.InsertBuilder.DbColumnInfoList = this.InsertBuilder.DbColumnInfoList.Where(it => ignoreColumns.Any(ig => ig.Equals(it.PropertyName, StringComparison.CurrentCultureIgnoreCase)) || ignoreColumns.Any(ig => ig.Equals(it.DbColumnName, StringComparison.CurrentCultureIgnoreCase))).ToList();
            return this;
        }

        /// <summary>
        /// 指定插入列
        /// </summary>
        /// <param name="columns">列名数组</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> InsertColumns(string[] columns)
        {
            if (columns == null) return this;
            this.InsertBuilder.DbColumnInfoList = this.InsertBuilder.DbColumnInfoList.Where(it => columns.Any(ig => ig.Equals(it.PropertyName, StringComparison.CurrentCultureIgnoreCase)) || columns.Any(ig => ig.Equals(it.DbColumnName, StringComparison.CurrentCultureIgnoreCase))).ToList();
            return this;
        }

        /// <summary>
        /// 指定表锁
        /// </summary>
        /// <param name="lockString">锁字符串</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> With(string lockString)
        {
            if (this.Context.CurrentConnectionConfig.DbType == DbType.SqlServer)
                this.InsertBuilder.TableWithString = lockString;
            return this;
        }
        /// <summary>
        /// 关闭自增
        /// </summary>
        /// <param name="isSetOn">是否设置</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> OffIdentity(bool isSetOn)
        {
            if (isSetOn)
            {
                return this.OffIdentity();
            }
            else
            {
                return this;
            }
        }
        /// <summary>
        /// 关闭自增
        /// </summary>
        /// <returns>可插入对象</returns>
        public IInsertable<T> OffIdentity()
        {
            this.IsOffIdentity = true;
            this.InsertBuilder.IsOffIdentity = true;
            return this;
        }
        /// <summary>
        /// 忽略列设置
        /// </summary>
        /// <param name="ignoreNullColumn">是否忽略空列</param>
        /// <param name="isOffIdentity">是否关闭自增</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> IgnoreColumns(bool ignoreNullColumn, bool isOffIdentity = false)
        {
            Check.Exception(this.InsertObjs.Length > 1 && ignoreNullColumn, ErrorMessage.GetThrowMessage("ignoreNullColumn NoSupport batch insert, use .PageSize(1).IgnoreColumnsNull().ExecuteCommand()", "ignoreNullColumn 不支持批量操作, 你可以使用 .PageSize(1).IgnoreColumnsNull().ExecuteCommand()"));
            this.IsOffIdentity = isOffIdentity;
            this.InsertBuilder.IsOffIdentity = isOffIdentity;
            if (this.InsertBuilder.LambdaExpressions == null)
                this.InsertBuilder.LambdaExpressions = InstanceFactory.GetLambdaExpressions(this.Context.CurrentConnectionConfig);
            this.InsertBuilder.IsNoInsertNull = ignoreNullColumn;
            return this;
        }

        /// <summary>
        /// 移除数据缓存
        /// </summary>
        /// <returns>可插入对象</returns>
        public IInsertable<T> RemoveDataCache()
        {
            this.RemoveCacheFunc = () =>
            {
                var cacheService = this.Context.CurrentConnectionConfig.ConfigureExternalServices.DataInfoCacheService;
                CacheSchemeMain.RemoveCache(cacheService, this.Context.EntityMaintenance.GetTableName<T>());
            };
            return this;
        }
        /// <summary>
        /// 移除指定模式的数据缓存
        /// </summary>
        /// <param name="likeString">匹配字符串</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> RemoveDataCache(string likeString)
        {
            this.RemoveCacheFunc = () =>
            {
                var cacheService = this.Context.CurrentConnectionConfig.ConfigureExternalServices.DataInfoCacheService;
                CacheSchemeMain.RemoveCacheByLike(cacheService, likeString);
            };
            return this;
        }
        /// <summary>
        /// 使用MySQL批量插入
        /// </summary>
        /// <returns>MySQL批量插入对象</returns>
        public MySqlBlukCopy<T> UseMySql()
        {
            return new MySqlBlukCopy<T>(this.Context, this.SqlBuilder, InsertObjs);
        }
        /// <summary>
        /// 使用SQL Server批量插入
        /// </summary>
        /// <returns>SQL Server批量插入对象</returns>
        public SqlServerBlukCopy UseSqlServer()
        {
            PreToSql();
            var currentType = this.Context.CurrentConnectionConfig.DbType;
            Check.Exception(currentType != DbType.SqlServer, "UseSqlServer no support " + currentType);
            SqlServerBlukCopy result = new SqlServerBlukCopy();
            result.DbColumnInfoList = this.InsertBuilder.DbColumnInfoList.GroupBy(it => it.TableId).ToList();
            result.InsertBuilder = this.InsertBuilder;
            result.Builder = this.SqlBuilder;
            result.Context = this.Context;
            result.Inserts = this.InsertObjs;
            return result;
        }
        /// <summary>
        /// 使用Oracle批量插入
        /// </summary>
        /// <returns>Oracle批量插入对象</returns>
        public OracleBlukCopy UseOracle()

        {

            PreToSql();

            var currentType = this.Context.CurrentConnectionConfig.DbType;

            Check.Exception(currentType != DbType.Oracle, "UseSqlServer no support " + currentType);

            OracleBlukCopy result = new OracleBlukCopy();

            result.DbColumnInfoList = this.InsertBuilder.DbColumnInfoList.GroupBy(it => it.TableId).ToList();

            result.InsertBuilder = this.InsertBuilder;

            result.Builder = this.SqlBuilder;

            result.Context = this.Context;

            result.Inserts = this.InsertObjs;
            InsertBuilder.IsBlukCopy = true;

            return result;

        }


        /// <summary>
        /// 条件启用差异日志事件
        /// </summary>
        /// <param name="isDiffLogEvent">是否启用差异日志</param>
        /// <param name="diffLogBizData">差异日志业务数据</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> EnableDiffLogEventIF(bool isDiffLogEvent, object diffLogBizData)
        {
            if (isDiffLogEvent)
            {
                return EnableDiffLogEvent(diffLogBizData);
            }
            return this;
        }
        /// <summary>
        /// 启用差异日志事件
        /// </summary>
        /// <param name="businessData">业务数据</param>
        /// <returns>可插入对象</returns>
        public IInsertable<T> EnableDiffLogEvent(object businessData = null)
        {
            //Check.Exception(this.InsertObjs.HasValue() && this.InsertObjs.Count() > 1, "DiffLog does not support batch operations");
            diffModel = new DiffLogModel();
            this.IsEnableDiffLogEvent = true;
            diffModel.BusinessData = businessData;
            diffModel.DiffType = DiffType.insert;
            return this;
        }

        /// <summary>
        /// 添加子列表
        /// </summary>
        /// <param name="items">子列表表达式</param>
        /// <returns>可子插入对象</returns>
        public ISubInsertable<T> AddSubList(Expression<Func<T, object>> items)
        {
            Check.Exception(GetPrimaryKeys().Count == 0, typeof(T).Name + " need Primary key");
            Check.Exception(GetPrimaryKeys().Count > 1, typeof(T).Name + "Multiple primary keys are not supported");
            //Check.Exception(this.InsertObjs.Count() > 1, "SubInserable No Support Insertable(List<T>)");
            //Check.Exception(items.ToString().Contains(".First().")==false, items.ToString()+ " not supported ");
            if (this.InsertObjs == null || this.InsertObjs.Length == 0)
            {
                return new SubInsertable<T>();
            }
            SubInsertable<T> result = new SubInsertable<T>();
            result.InsertObjects = this.InsertObjs;
            result.Context = this.Context;
            result.SubList = new List<SubInsertTreeExpression>();
            result.SubList.Add(new SubInsertTreeExpression() { Expression = items });
            result.InsertBuilder = this.InsertBuilder;
            result.Pk = GetPrimaryKeys().First();
            result.Entity = this.EntityInfo;
            return result;
        }
        /// <summary>
        /// 添加子列表树
        /// </summary>
        /// <param name="tree">子列表树表达式</param>
        /// <returns>可子插入对象</returns>
        public ISubInsertable<T> AddSubList(Expression<Func<T, SubInsertTree>> tree)
        {
            Check.Exception(GetPrimaryKeys().Count == 0, typeof(T).Name + " need Primary key");
            Check.Exception(GetPrimaryKeys().Count > 1, typeof(T).Name + "Multiple primary keys are not supported");
            //Check.Exception(this.InsertObjs.Count() > 1, "SubInserable No Support Insertable(List<T>)");
            //Check.Exception(items.ToString().Contains(".First().")==false, items.ToString()+ " not supported ");
            if (this.InsertObjs == null || this.InsertObjs.Length == 0)
            {
                return new SubInsertable<T>();
            }
            SubInsertable<T> result = new SubInsertable<T>();
            result.InsertObjects = this.InsertObjs;
            result.Context = this.Context;
            result.SubList = new List<SubInsertTreeExpression>();
            result.InsertBuilder = this.InsertBuilder;
            result.Pk = GetPrimaryKeys().First();
            result.Entity = this.EntityInfo;
            result.AddSubList(tree);
            return result;
        }
        /// <summary>
        /// 分表插入
        /// </summary>
        /// <param name="splitType">分表类型</param>
        /// <returns>可分表插入对象</returns>
        public SplitInsertable<T> SplitTable(SplitType splitType)
        {
            UtilMethods.StartCustomSplitTable(this.Context, typeof(T));
            SplitTableContext helper = new SplitTableContext(Context)
            {
                EntityInfo = this.EntityInfo
            };
            helper.CheckPrimaryKey();
            SplitInsertable<T> result = new SplitInsertable<T>();
            result.Context = this.Context;
            result.EntityInfo = this.EntityInfo;
            result.Helper = helper;
            result.SplitType = splitType;
            result.TableNames = new List<KeyValuePair<string, object>>();
            result.MySqlIgnore = this.InsertBuilder.MySqlIgnore;
            foreach (var item in this.InsertObjs)
            {
                var splitFieldValue = helper.GetValue(splitType, item);
                var tableName = helper.GetTableName(splitType, splitFieldValue);
                result.TableNames.Add(new KeyValuePair<string, object>(tableName, item));
            }
            result.Inserable = this;
            return result;
        }

        /// <summary>
        /// 分表插入
        /// </summary>
        /// <returns>可分表插入对象</returns>
        public SplitInsertable<T> SplitTable()
        {
            if (StaticConfig.SplitTableCreateTableFunc != null)
            {
                StaticConfig.SplitTableCreateTableFunc(typeof(T), this.InsertObjs);
            }
            UtilMethods.StartCustomSplitTable(this.Context, typeof(T));
            var splitTableAttribute = typeof(T).GetCustomAttribute<SplitTableAttribute>();
            if (splitTableAttribute != null)
            {
                return SplitTable((splitTableAttribute as SplitTableAttribute).SplitType);
            }
            else
            {
                Check.Exception(true, $" {typeof(T).Name} need SplitTableAttribute");
                return null;
            }
        }

        #endregion

    }
}