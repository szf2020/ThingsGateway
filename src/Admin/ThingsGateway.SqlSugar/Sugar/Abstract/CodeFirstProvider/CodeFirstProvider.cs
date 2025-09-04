using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 提供代码优先的数据库操作实现
    /// </summary>
    public partial class CodeFirstProvider : ICodeFirst
    {
        #region Properties
        /// <summary>
        /// 用于同步操作的锁对象
        /// </summary>
        internal static object LockObject = new object();
        /// <summary>
        /// SqlSugar提供者实例
        /// </summary>
        public virtual SqlSugarProvider Context { get; set; }
        /// <summary>
        /// 是否备份表
        /// </summary>
        protected bool IsBackupTable { get; set; }
        /// <summary>
        /// 最大备份数据行数
        /// </summary>
        protected int MaxBackupDataRows { get; set; }
        /// <summary>
        /// 默认字符串长度
        /// </summary>
        protected virtual int DefaultLength { get; set; }
        /// <summary>
        /// 类型与表名的映射字典
        /// </summary>
        protected Dictionary<Type, string> MappingTables = new Dictionary<Type, string>();
        /// <summary>
        /// 构造函数
        /// </summary>
        public CodeFirstProvider()
        {
            if (DefaultLength == 0)
            {
                DefaultLength = 255;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// 分表操作
        /// </summary>
        /// <returns>分表代码优先提供者</returns>
        public SplitCodeFirstProvider SplitTables()
        {
            var result = new SplitCodeFirstProvider();
            result.Context = this.Context;
            result.DefaultLength = this.DefaultLength;
            return result;
        }

        /// <summary>
        /// 设置备份表
        /// </summary>
        /// <param name="maxBackupDataRows">最大备份数据行数</param>
        /// <returns>代码优先接口</returns>
        public virtual ICodeFirst BackupTable(int maxBackupDataRows = int.MaxValue)
        {
            this.IsBackupTable = true;
            this.MaxBackupDataRows = maxBackupDataRows;
            return this;
        }

        /// <summary>
        /// 设置字符串默认长度
        /// </summary>
        /// <param name="length">长度值</param>
        /// <returns>代码优先接口</returns>
        public virtual ICodeFirst SetStringDefaultLength(int length)
        {
            DefaultLength = length;
            return this;
        }
        /// <summary>
        /// 初始化带有租户属性的表
        /// </summary>
        /// <param name="entityTypes">实体类型数组</param>
        public void InitTablesWithAttr(params Type[] entityTypes)
        {
            foreach (var item in entityTypes)
            {
                var attr = item.GetCustomAttribute<TenantAttribute>();
                if (attr == null || this.Context?.Root == null)
                {
                    this.Context.CodeFirst.InitTables(item);
                }
                else
                {
                    var newDb = this.Context.Root.GetConnectionWithAttr(item);
                    newDb.CodeFirst.InitTables(item);
                }
            }
        }
        /// <summary>
        /// 初始化表
        /// </summary>
        /// <param name="entityType">实体类型</param>
        public virtual void InitTables(Type entityType)
        {
            var oldSlave = this.Context.CurrentConnectionConfig.SlaveConnectionConfigs;
            this.Context.CurrentConnectionConfig.SlaveConnectionConfigs = null;
            var splitTableAttribute = entityType.GetCustomAttribute<SplitTableAttribute>();
            if (splitTableAttribute != null)
            {
                var mappingInfo = this.Context.MappingTables.FirstOrDefault(it => it.EntityName == entityType.Name);
                if (mappingInfo == null)
                {
                    UtilMethods.StartCustomSplitTable(this.Context, entityType);
                    this.Context.CodeFirst.SplitTables().InitTables(entityType);
                    this.Context.MappingTables.RemoveAll(it => it.EntityName == entityType.Name);
                    UtilMethods.EndCustomSplitTable(this.Context, entityType);
                    return;
                }
            }
            // 防止程序中使用时并发请求
            lock (CodeFirstProvider.LockObject)
            {
                MappingTableList oldTableList = CopyMappingTable();
                //this.Context.Utilities.RemoveCacheAll();
                var entityInfo = this.Context.GetEntityNoCacheInitMappingInfo(entityType);
                if (!this.Context.DbMaintenance.IsAnySystemTablePermissions())
                {
                    Check.Exception(true, "Dbfirst and  Codefirst requires system table permissions");
                }

                if (this.Context.Ado.Transaction == null)
                {
                    var executeResult = Context.Ado.UseTran(() => Execute(entityType, entityInfo));
                    Check.Exception(!executeResult.IsSuccess, executeResult.ErrorMessage);
                }
                else
                {
                    Execute(entityType, entityInfo);
                }

                RestMappingTables(oldTableList);
            }
            this.Context.CurrentConnectionConfig.SlaveConnectionConfigs = oldSlave;
        }

        /// <summary>
        /// 初始化表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        public void InitTables<T>()
        {
            InitTables(typeof(T));
        }
        /// <summary>
        /// 初始化表
        /// </summary>
        /// <typeparam name="T">实体类型1</typeparam>
        /// <typeparam name="T2">实体类型2</typeparam>
        public void InitTables<T, T2>()
        {
            InitTables(typeof(T), typeof(T2));
        }
        /// <summary>
        /// 初始化表
        /// </summary>
        /// <typeparam name="T">实体类型1</typeparam>
        /// <typeparam name="T2">实体类型2</typeparam>
        /// <typeparam name="T3">实体类型3</typeparam>
        public void InitTables<T, T2, T3>()
        {
            InitTables(typeof(T), typeof(T2), typeof(T3));
        }
        /// <summary>
        /// 初始化表
        /// </summary>
        /// <typeparam name="T">实体类型1</typeparam>
        /// <typeparam name="T2">实体类型2</typeparam>
        /// <typeparam name="T3">实体类型3</typeparam>
        /// <typeparam name="T4">实体类型4</typeparam>
        public void InitTables<T, T2, T3, T4>()
        {
            InitTables(typeof(T), typeof(T2), typeof(T3), typeof(T4));
        }
        /// <summary>
        /// 初始化表
        /// </summary>
        /// <typeparam name="T">实体类型1</typeparam>
        /// <typeparam name="T2">实体类型2</typeparam>
        /// <typeparam name="T3">实体类型3</typeparam>
        /// <typeparam name="T4">实体类型4</typeparam>
        /// <typeparam name="T5">实体类型5</typeparam>
        public void InitTables<T, T2, T3, T4, T5>()
        {
            InitTables(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }
        /// <summary>
        /// 初始化表
        /// </summary>
        /// <param name="entityTypes">实体类型数组</param>
        public virtual void InitTables(params Type[] entityTypes)
        {
            if (entityTypes.HasValue())
            {
                foreach (var item in entityTypes)
                {
                    try
                    {
                        InitTables(item);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(item.Name + " 创建失败,请认真检查 1、属性需要get set 2、特殊类型需要加Ignore 具体错误内容： " + ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 设置类型与表名的映射
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <param name="newTableName">新表名</param>
        /// <returns>代码优先接口</returns>
        public ICodeFirst AS(Type type, string newTableName)
        {
            if (!MappingTables.TryAdd(type, newTableName))
            {
                MappingTables[type] = newTableName;
            }
            return this;
        }
        /// <summary>
        /// 设置类型与表名的映射
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="newTableName">新表名</param>
        /// <returns>代码优先接口</returns>
        public ICodeFirst AS<T>(string newTableName)
        {
            return AS(typeof(T), newTableName);
        }

        /// <summary>
        /// 初始化命名空间下的所有表
        /// </summary>
        /// <param name="entitiesNamespace">实体命名空间</param>
        public virtual void InitTables(string entitiesNamespace)
        {
            var types = Assembly.Load(entitiesNamespace).GetTypes();
            InitTables(types);
        }
        /// <summary>
        /// 初始化多个命名空间下的所有表
        /// </summary>
        /// <param name="entitiesNamespaces">实体命名空间数组</param>
        public virtual void InitTables(params string[] entitiesNamespaces)
        {
            if (entitiesNamespaces.HasValue())
            {
                foreach (var item in entitiesNamespaces)
                {
                    InitTables(item);
                }
            }
        }
        /// <summary>
        /// 获取表差异
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>表差异提供者</returns>
        public TableDifferenceProvider GetDifferenceTables<T>()
        {
            var type = typeof(T);
            return GetDifferenceTables(type);
        }

        /// <summary>
        /// 获取表差异
        /// </summary>
        /// <param name="types">实体类型数组</param>
        /// <returns>表差异提供者</returns>
        public TableDifferenceProvider GetDifferenceTables(params Type[] types)
        {
            TableDifferenceProvider result = new TableDifferenceProvider();
            foreach (var type in types)
            {
                try
                {
                    GetDifferenceTables(result, type);
                }
                catch (Exception ex)
                {
                    Check.ExceptionEasy($"实体{type.Name} 出错,具体错误:" + ex.Message, $" {type.Name} error." + ex.Message);
                }
            }
            return result;
        }
        #endregion

        #region Core Logic
        /// <summary>
        /// 获取表差异
        /// </summary>
        /// <param name="result">表差异提供者</param>
        /// <param name="type">实体类型</param>
        private void GetDifferenceTables(TableDifferenceProvider result, Type type)
        {
            var tempTableName = "TempDiff" + DateTime.Now.ToString("yyMMssHHmmssfff");
            var oldTableName = this.Context.EntityMaintenance.GetEntityInfo(type).DbTableName;
            var db = new SqlSugarProvider(UtilMethods.CopyConfig(this.Context.CurrentConnectionConfig));
            db.CurrentConnectionConfig.SlaveConnectionConfigs = null;
            db.CurrentConnectionConfig.ConfigureExternalServices = UtilMethods.IsNullReturnNew(db.CurrentConnectionConfig.ConfigureExternalServices);
            db.CurrentConnectionConfig.ConfigureExternalServices.EntityNameService += (x, p) =>
            {
                p.IsDisabledUpdateAll = true;//禁用更新
            };
            db.MappingTables = new MappingTableList();
            db.MappingTables.Add(type.Name, tempTableName);
            try
            {
                var codeFirst = db.CodeFirst;
                codeFirst.SetStringDefaultLength(this.DefaultLength);
                codeFirst.InitTables(type);
                var tables = db.DbMaintenance.GetTableInfoList(false);
                var oldTableInfo = tables.FirstOrDefault(it => it.Name.EqualCase(oldTableName));
                var newTableInfo = tables.FirstOrDefault(it => it.Name.EqualCase(oldTableName));
                var oldTable = db.DbMaintenance.GetColumnInfosByTableName(oldTableName, false);
                var tempTable = db.DbMaintenance.GetColumnInfosByTableName(tempTableName, false);
                if (oldTableInfo == null)
                {
                    oldTableInfo = new DbTableInfo() { Name = "还未创建:" + oldTableName };
                    newTableInfo = new DbTableInfo() { Name = "还未创建:" + oldTableName };
                }
                result.tableInfos.Add(new DiffTableInfo()
                {
                    OldTableInfo = oldTableInfo,
                    NewTableInfo = newTableInfo,
                    OldColumnInfos = oldTable,
                    NewColumnInfos = tempTable
                });
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                db.DbMaintenance.DropTable(tempTableName);
            }
        }
        /// <summary>
        /// 执行表初始化
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="entityInfo">实体信息</param>
        protected virtual void Execute(Type entityType, EntityInfo entityInfo)
        {
            //var entityInfo = this.Context.EntityMaintenance.GetEntityInfoNoCache(entityType);
            if (entityInfo.Discrimator.HasValue())
            {
                Check.ExceptionEasy(!Regex.IsMatch(entityInfo.Discrimator, @"^(?:\w+:\w+)(?:,\w+:\w+)*$"), "The format should be type:cat for this type, and if there are multiple, it can be FieldName:cat,FieldName2:dog ", "格式错误应该是type:cat这种格式，如果是多个可以FieldName:cat,FieldName2:dog，不要有空格");
                var array = entityInfo.Discrimator.Split(',');
                foreach (var disItem in array)
                {
                    var name = disItem.Split(':').First();
                    var value = disItem.Split(':').Last();
                    entityInfo.Columns.Add(new EntityColumnInfo() { PropertyInfo = typeof(DiscriminatorObject).GetProperty(nameof(DiscriminatorObject.FieldName)), IsOnlyIgnoreUpdate = true, DbColumnName = name, UnderType = typeof(string), PropertyName = name, Length = 50 });
                }
            }
            if (this.MappingTables.TryGetValue(entityType, out string? v))
            {
                entityInfo.DbTableName = v;
                this.Context.MappingTables.Add(entityInfo.EntityName, entityInfo.DbTableName);
            }
            if (this.DefaultLength > 0)
            {
                foreach (var item in entityInfo.Columns)
                {
                    if (item.PropertyInfo.PropertyType == UtilConstants.StringType && item.DataType.IsNullOrEmpty() && item.Length == 0)
                    {
                        item.Length = DefaultLength;
                    }
                    if (item.DataType?.Contains(',') == true && !Regex.IsMatch(item.DataType, @"\d\,\d"))
                    {
                        var types = item.DataType.Split(',').ToHashSet(StringComparer.OrdinalIgnoreCase);
                        var mapingTypes = this.Context.Ado.DbBind.MappingTypes.Select(it => it.Key.ToLower()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                        var mappingType = types.FirstOrDefault(it => mapingTypes.Contains(it));
                        if (mappingType != null)
                        {
                            item.DataType = mappingType;
                        }
                        if (item.DataType == "varcharmax")
                        {
                            item.DataType = "nvarchar(max)";
                        }
                    }
                }
            }
            var tableName = GetTableName(entityInfo);
            this.Context.MappingTables.Add(entityInfo.EntityName, tableName);
            entityInfo.DbTableName = tableName;
            entityInfo.Columns.ForEach(it =>
            {
                it.DbTableName = tableName;
                if (it.UnderType?.Name == "DateOnly" && it.DataType == null)
                {
                    it.DataType = "Date";
                }
                if (it.UnderType?.Name == "TimeOnly" && it.DataType == null)
                {
                    it.DataType = "Time";
                }
            });
            var isAny = this.Context.DbMaintenance.IsAnyTable(tableName, false);
            if (isAny && entityInfo.IsDisabledUpdateAll)
            {
                return;
            }
            if (isAny)
                ExistLogic(entityInfo);
            else
                NoExistLogic(entityInfo);

            this.Context.DbMaintenance.AddRemark(entityInfo);
            this.Context.DbMaintenance.AddIndex(entityInfo);
            CreateIndex(entityInfo);
            this.Context.DbMaintenance.AddDefaultValue(entityInfo);
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="entityInfo">实体信息</param>
        private void CreateIndex(EntityInfo entityInfo)
        {
            if (entityInfo.Indexs.HasValue())
            {
                foreach (var item in entityInfo.Indexs)
                {
                    if (entityInfo.Type.GetCustomAttribute<SplitTableAttribute>() != null)
                    {
                        if (item.IndexName?.Contains("{split_table}") == true)
                        {
                            item.IndexName = item.IndexName.Replace("{split_table}", entityInfo.DbTableName);
                        }
                        else
                        {
                            item.IndexName = item.IndexName + entityInfo.DbTableName;
                        }
                    }
                    if (this.Context.CurrentConnectionConfig.IndexSuffix.HasValue())
                    {
                        item.IndexName = (this.Context.CurrentConnectionConfig.IndexSuffix + item.IndexName);
                    }
                    var include = "";
                    if (item.IndexName != null)
                    {
                        var database = "{db}";
                        if (item.IndexName.Contains(database))
                        {
                            item.IndexName = item.IndexName.Replace(database, this.Context.Ado.Connection.Database);
                        }
                        var table = "{table}";
                        if (item.IndexName.Contains(table))
                        {
                            item.IndexName = item.IndexName.Replace(table, entityInfo.DbTableName);
                        }
                        if (item.IndexName.Contains("{include:", StringComparison.CurrentCultureIgnoreCase))
                        {
                            include = Regex.Match(item.IndexName, @"\{include\:.+$").Value;
                            item.IndexName = item.IndexName.Replace(include, "");
                        }
                        if (item.IndexName.Contains('.') && item.IndexName.Contains('['))
                        {
                            item.IndexName = item.IndexName.Replace(".", "_");
                            item.IndexName = item.IndexName.Replace("[", "").Replace("]", "");
                        }
                    }
                    if (!this.Context.DbMaintenance.IsAnyIndex(item.IndexName))
                    {
                        var querybulder = InstanceFactory.GetSqlbuilder(this.Context.CurrentConnectionConfig);
                        querybulder.Context = this.Context;
                        var fields = item.IndexFields
                            .Select(it =>
                            {
                                var dbColumn = entityInfo.Columns.FirstOrDefault(z => z.PropertyName == it.Key);
                                if (dbColumn == null && entityInfo.Discrimator == null)
                                {
                                    Check.ExceptionEasy($"{entityInfo.EntityName} no   SugarIndex[ {it.Key} ]  found", $"类{entityInfo.EntityName} 索引特性没找到列 ：{it.Key}");
                                }
                                return new KeyValuePair<string, OrderByType>(dbColumn.DbColumnName, it.Value);
                            })
                            .Select(it => querybulder.GetTranslationColumnName(it.Key) + " " + it.Value).ToArray();
                        this.Context.DbMaintenance.CreateIndex(entityInfo.DbTableName, fields, item.IndexName + include, item.IsUnique);
                    }
                }
            }
        }

        /// <summary>
        /// 表不存在时的逻辑
        /// </summary>
        /// <param name="entityInfo">实体信息</param>
        public virtual void NoExistLogic(EntityInfo entityInfo)
        {
            var tableName = GetTableName(entityInfo);
            //Check.Exception(entityInfo.Columns.Where(it => it.IsPrimarykey).Count() > 1, "Use Code First ,The primary key must not exceed 1");
            List<DbColumnInfo> columns = new List<DbColumnInfo>();
            if (entityInfo.Columns.HasValue())
            {
                foreach (var item in entityInfo.Columns.OrderBy(it => it.IsPrimarykey ? 0 : 1).Where(it => it.IsIgnore == false))
                {
                    DbColumnInfo dbColumnInfo = EntityColumnToDbColumn(entityInfo, tableName, item);
                    columns.Add(dbColumnInfo);
                }
                if (entityInfo.IsCreateTableFieldSort)
                {
                    columns = columns.OrderBy(c => c.CreateTableFieldSort).ToList();
                    columns = columns.OrderBy(it => it.IsPrimarykey ? 0 : 1).ToList();
                }
            }
            this.Context.DbMaintenance.CreateTable(tableName, columns, true);
        }
        /// <summary>
        /// 表存在时的逻辑
        /// </summary>
        /// <param name="entityInfo">实体信息</param>
        public virtual void ExistLogic(EntityInfo entityInfo)
        {
            if (entityInfo.Columns.HasValue() && entityInfo.IsDisabledUpdateAll == false)
            {
                //Check.Exception(entityInfo.Columns.Where(it => it.IsPrimarykey).Count() > 1, "Multiple primary keys do not support modifications");

                var tableName = GetTableName(entityInfo);
                var dbColumns = this.Context.DbMaintenance.GetColumnInfosByTableName(tableName, false);
                ConvertColumns(dbColumns);
                var entityColumns = entityInfo.Columns.Where(it => it.IsIgnore == false).ToList();
                var dropColumns = dbColumns
                                          .Where(dc => !entityColumns.Any(ec => dc.DbColumnName.Equals(ec.OldDbColumnName, StringComparison.CurrentCultureIgnoreCase)))
                                          .Where(dc => !entityColumns.Any(ec => dc.DbColumnName.Equals(ec.DbColumnName, StringComparison.CurrentCultureIgnoreCase)))
                                          .ToList();
                var addColumns = entityColumns
                                          .Where(ec => ec.OldDbColumnName.IsNullOrEmpty() || !dbColumns.Any(dc => dc.DbColumnName.Equals(ec.OldDbColumnName, StringComparison.CurrentCultureIgnoreCase)))
                                          .Where(ec => !dbColumns.Any(dc => ec.DbColumnName.Equals(dc.DbColumnName, StringComparison.CurrentCultureIgnoreCase))).ToList();
                var alterColumns = entityColumns
                                           .Where(it => it.IsDisabledAlterColumn == false)
                                           .Where(ec => !dbColumns.Any(dc => dc.DbColumnName.Equals(ec.OldDbColumnName, StringComparison.CurrentCultureIgnoreCase)))
                                           .Where(ec =>
                                                          dbColumns.Any(dc => dc.DbColumnName.EqualCase(ec.DbColumnName)
                                                               && ((ec.Length != dc.Length && !UtilMethods.GetUnderType(ec.PropertyInfo).IsEnum() && UtilMethods.GetUnderType(ec.PropertyInfo).IsIn(UtilConstants.StringType)) ||
                                                                    ec.IsNullable != dc.IsNullable ||
                                                                    IsNoSamePrecision(ec, dc) ||
                                                                    IsNotSameType(ec, dc)))).ToList();

                alterColumns.RemoveAll(entityColumnInfo =>
                {
                    var bigStringArray = StaticConfig.CodeFirst_BigString.Replace("varcharmax", "nvarchar(max)").Split(',');
                    var dbColumnInfo = dbColumns.FirstOrDefault(dc => dc.DbColumnName.EqualCase(entityColumnInfo.DbColumnName));
                    var isMaxString = (dbColumnInfo?.Length == -1 && dbColumnInfo?.DataType?.EqualCase("nvarchar") == true);
                    var isRemove =
                           dbColumnInfo != null
                           && bigStringArray.Contains(entityColumnInfo.DataType)
                           && isMaxString;
                    return isRemove;
                });
                var renameColumns = entityColumns
                    .Where(it => !string.IsNullOrEmpty(it.OldDbColumnName))
                    .Where(entityColumn => dbColumns.Any(dbColumn => entityColumn.OldDbColumnName.Equals(dbColumn.DbColumnName, StringComparison.CurrentCultureIgnoreCase)))
                    .ToList();

                var isMultiplePrimaryKey = dbColumns.Where(it => it.IsPrimarykey).Count() > 1 || entityColumns.Where(it => it.IsPrimarykey).Count() > 1;

                var isChange = false;
                foreach (var item in addColumns)
                {
                    this.Context.DbMaintenance.AddColumn(tableName, EntityColumnToDbColumn(entityInfo, tableName, item));
                    isChange = true;
                }
                if (entityInfo.IsDisabledDelete == false)
                {
                    foreach (var item in dropColumns)
                    {
                        this.Context.DbMaintenance.DropColumn(tableName, item.DbColumnName);
                        isChange = true;
                    }
                }
                foreach (var item in alterColumns)
                {
                    if (this.Context.CurrentConnectionConfig.DbType == DbType.Oracle)
                    {
                        var entityColumnItem = entityColumns.FirstOrDefault(y => y.DbColumnName == item.DbColumnName);
                        if (entityColumnItem != null && !string.IsNullOrEmpty(entityColumnItem.DataType))
                        {
                            continue;
                        }
                    }

                    this.Context.DbMaintenance.UpdateColumn(tableName, EntityColumnToDbColumn(entityInfo, tableName, item));
                    isChange = true;
                }
                foreach (var item in renameColumns)
                {
                    this.Context.DbMaintenance.RenameColumn(tableName, item.OldDbColumnName, item.DbColumnName);
                    isChange = true;
                }
                var isAddPrimaryKey = false;
                foreach (var item in entityColumns)
                {
                    var dbColumn = dbColumns.FirstOrDefault(dc => dc.DbColumnName.Equals(item.DbColumnName, StringComparison.CurrentCultureIgnoreCase));
                    if (dbColumn == null) continue;
                    bool pkDiff, idEntityDiff;
                    KeyAction(item, dbColumn, out pkDiff, out idEntityDiff);
                    if (dbColumn != null && pkDiff && !idEntityDiff && isMultiplePrimaryKey == false)
                    {
                        var isAdd = item.IsPrimarykey;
                        if (isAdd)
                        {
                            isAddPrimaryKey = true;
                            this.Context.DbMaintenance.AddPrimaryKey(tableName, item.DbColumnName);
                        }
                        else
                        {
                            this.Context.DbMaintenance.DropConstraint(tableName, string.Format("PK_{0}_{1}", tableName, item.DbColumnName));
                        }
                    }
                    else if ((pkDiff || idEntityDiff) && isMultiplePrimaryKey == false)
                    {
                        ChangeKey(entityInfo, tableName, item);
                    }
                }
                if (isAddPrimaryKey == false && entityColumns.Count(it => it.IsPrimarykey) == 1 && !dbColumns.Any(it => it.IsPrimarykey))
                {
                    var addPk = entityColumns.First(it => it.IsPrimarykey);
                    this.Context.DbMaintenance.AddPrimaryKey(tableName, addPk.DbColumnName);
                }
                if (isMultiplePrimaryKey)
                {
                    var oldPkNames = dbColumns.Where(it => it.IsPrimarykey).Select(it => it.DbColumnName.ToLower()).OrderBy(it => it).ToList();
                    var newPkNames = entityColumns.Where(it => it.IsPrimarykey).Select(it => it.DbColumnName.ToLower()).OrderBy(it => it).ToList();
                    if (oldPkNames.Count == 0 && newPkNames.Count > 1)
                    {
                        try
                        {
                            this.Context.DbMaintenance.AddPrimaryKeys(tableName, newPkNames);
                        }
                        catch (Exception ex)
                        {
                            Check.Exception(true, ErrorMessage.GetThrowMessage("The current database does not support changing multiple primary keys. " + ex.Message, "当前数据库不支持修改多主键," + ex.Message));
                            throw;
                        }
                    }
                    else if (!Enumerable.SequenceEqual(oldPkNames, newPkNames))
                    {
                        Check.Exception(true, ErrorMessage.GetThrowMessage("Modification of multiple primary key tables is not supported. Delete tables while creating", "不支持修改多主键表，请删除表在创建"));
                    }
                }
                if (isChange && IsBackupTable)
                {
                    this.Context.DbMaintenance.BackupTable(tableName, tableName + DateTime.Now.ToString("yyyyMMddHHmmss"), MaxBackupDataRows);
                }
                ExistLogicEnd(entityColumns);
            }
        }

        /// <summary>
        /// 检查精度是否相同
        /// </summary>
        /// <param name="ec">实体列信息</param>
        /// <param name="dc">数据库列信息</param>
        /// <returns>是否不同</returns>
        private bool IsNoSamePrecision(EntityColumnInfo ec, DbColumnInfo dc)
        {
            if (this.Context.CurrentConnectionConfig.MoreSettings?.EnableCodeFirstUpdatePrecision == true)
            {
                return ec.DecimalDigits != dc.DecimalDigits && ec.UnderType.IsIn(UtilConstants.DobType, UtilConstants.DecType);
            }
            return false;
        }

        /// <summary>
        /// 主键操作
        /// </summary>
        /// <param name="item">实体列信息</param>
        /// <param name="dbColumn">数据库列信息</param>
        /// <param name="pkDiff">主键差异</param>
        /// <param name="idEntityDiff">自增差异</param>
        protected virtual void KeyAction(EntityColumnInfo item, DbColumnInfo dbColumn, out bool pkDiff, out bool idEntityDiff)
        {
            pkDiff = item.IsPrimarykey != dbColumn.IsPrimarykey;
            idEntityDiff = item.IsIdentity != dbColumn.IsIdentity;
        }

        /// <summary>
        /// 修改键
        /// </summary>
        /// <param name="entityInfo">实体信息</param>
        /// <param name="tableName">表名</param>
        /// <param name="item">实体列信息</param>
        protected virtual void ChangeKey(EntityInfo entityInfo, string tableName, EntityColumnInfo item)
        {
            string constraintName = string.Format("PK_{0}_{1}", tableName, item.DbColumnName);
            if (this.Context.DbMaintenance.IsAnyConstraint(constraintName))
                this.Context.DbMaintenance.DropConstraint(tableName, constraintName);
            this.Context.DbMaintenance.DropColumn(tableName, item.DbColumnName);
            this.Context.DbMaintenance.AddColumn(tableName, EntityColumnToDbColumn(entityInfo, tableName, item));
            if (item.IsPrimarykey)
                this.Context.DbMaintenance.AddPrimaryKey(tableName, item.DbColumnName);
        }

        /// <summary>
        /// 表存在逻辑结束
        /// </summary>
        /// <param name="dbColumns">数据库列集合</param>
        protected virtual void ExistLogicEnd(List<EntityColumnInfo> dbColumns)
        {
        }
        /// <summary>
        /// 转换列
        /// </summary>
        /// <param name="dbColumns">数据库列集合</param>
        protected virtual void ConvertColumns(List<DbColumnInfo> dbColumns)
        {
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// 恢复映射表
        /// </summary>
        /// <param name="oldTableList">旧表列表</param>
        private void RestMappingTables(MappingTableList oldTableList)
        {
            this.Context.MappingTables.Clear();
            foreach (var table in oldTableList)
            {
                this.Context.MappingTables.Add(table.EntityName, table.DbTableName);
            }
        }
        /// <summary>
        /// 复制映射表
        /// </summary>
        /// <returns>映射表列表</returns>
        private MappingTableList CopyMappingTable()
        {
            MappingTableList oldTableList = new MappingTableList();
            if (this.Context.MappingTables == null)
            {
                this.Context.MappingTables = new MappingTableList();
            }
            foreach (var table in this.Context.MappingTables)
            {
                oldTableList.Add(table.EntityName, table.DbTableName);
            }
            return oldTableList;
        }

        /// <summary>
        /// 获取创建表SQL字符串
        /// </summary>
        /// <param name="entityInfo">实体信息</param>
        /// <returns>SQL字符串</returns>
        public virtual string GetCreateTableString(EntityInfo entityInfo)
        {
            StringBuilder result = new StringBuilder();
            var tableName = GetTableName(entityInfo);
            return result.ToString();
        }
        /// <summary>
        /// 获取创建列SQL字符串
        /// </summary>
        /// <param name="entityInfo">实体信息</param>
        /// <returns>SQL字符串</returns>
        public virtual string GetCreateColumnsString(EntityInfo entityInfo)
        {
            StringBuilder result = new StringBuilder();
            var tableName = GetTableName(entityInfo);
            return result.ToString();
        }
        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="entityInfo">实体信息</param>
        /// <returns>表名</returns>
        protected virtual string GetTableName(EntityInfo entityInfo)
        {
            return this.Context.EntityMaintenance.GetTableName(entityInfo.EntityName);
        }
        /// <summary>
        /// 实体列转换为数据库列
        /// </summary>
        /// <param name="entityInfo">实体信息</param>
        /// <param name="tableName">表名</param>
        /// <param name="item">实体列信息</param>
        /// <returns>数据库列信息</returns>
        protected virtual DbColumnInfo EntityColumnToDbColumn(EntityInfo entityInfo, string tableName, EntityColumnInfo item)
        {
            var propertyType = UtilMethods.GetUnderType(item.PropertyInfo);
            var result = new DbColumnInfo()
            {
                TableId = entityInfo.Columns.IndexOf(item),
                DbColumnName = item.DbColumnName.HasValue() ? item.DbColumnName : item.PropertyName,
                IsPrimarykey = item.IsPrimarykey,
                IsIdentity = item.IsIdentity,
                TableName = tableName,
                IsNullable = item.IsNullable,
                DefaultValue = item.DefaultValue,
                ColumnDescription = item.ColumnDescription,
                Length = item.Length,
                DecimalDigits = item.DecimalDigits,
                CreateTableFieldSort = item.CreateTableFieldSort
            };
            GetDbType(item, propertyType, result);
            return result;
        }

        /// <summary>
        /// 获取数据库类型
        /// </summary>
        /// <param name="item">实体列信息</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="result">数据库列信息</param>
        protected virtual void GetDbType(EntityColumnInfo item, Type propertyType, DbColumnInfo result)
        {
            if (!string.IsNullOrEmpty(item.DataType))
            {
                result.DataType = item.DataType;
            }
            else if (propertyType.IsEnum())
            {
                result.DataType = this.Context.Ado.DbBind.GetDbTypeName(item.Length > 9 ? UtilConstants.LongType.Name : UtilConstants.IntType.Name);
            }
            else
            {
                var name = GetType(propertyType.Name);
                result.DataType = this.Context.Ado.DbBind.GetDbTypeName(name);
            }
        }

        /// <summary>
        /// 检查类型是否相同
        /// </summary>
        /// <param name="ec">实体列信息</param>
        /// <param name="dc">数据库列信息</param>
        /// <returns>是否不同</returns>
        protected virtual bool IsNotSameType(EntityColumnInfo ec, DbColumnInfo dc)
        {
            if (!string.IsNullOrEmpty(ec.DataType))
            {
                if (ec.IsIdentity && dc.IsIdentity)
                    return false;
                else
                    return !ec.DataType.Equals(dc.DataType, StringComparison.OrdinalIgnoreCase);
            }

            var propertyType = UtilMethods.GetUnderType(ec.PropertyInfo);
            string propertyTypeName;

            if (propertyType.IsEnum())
            {
                propertyTypeName = this.Context.Ado.DbBind.GetDbTypeName(ec.Length > 9 ? UtilConstants.LongType.Name : UtilConstants.IntType.Name);
            }
            else
            {
                var name = GetType(propertyType.Name);
                propertyTypeName = this.Context.Ado.DbBind.GetDbTypeName(name);
            }

            var dataType = dc.DataType;

            if (propertyTypeName.Equals("boolean", StringComparison.OrdinalIgnoreCase) && dataType.Equals("bool", StringComparison.OrdinalIgnoreCase))
                return false;

            if (propertyTypeName.Equals("varchar", StringComparison.OrdinalIgnoreCase) &&
                (dataType.Equals("string", StringComparison.OrdinalIgnoreCase) || dataType.Equals("nvarchar", StringComparison.OrdinalIgnoreCase)))
                return false;

            if (propertyTypeName.Equals("number", StringComparison.OrdinalIgnoreCase) && dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
                return false;

            if (this.Context.CurrentConnectionConfig?.MoreSettings?.EnableOracleIdentity == true &&
                propertyTypeName.Equals("int", StringComparison.OrdinalIgnoreCase) && dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
                return false;

            if (propertyTypeName.Equals("int", StringComparison.OrdinalIgnoreCase) &&
                dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase) &&
                dc.Length == 22 && dc.Scale == 0 &&
                this.Context.CurrentConnectionConfig.DbType == DbType.Oracle)
                return false;

            if (propertyTypeName.Equals("int", StringComparison.OrdinalIgnoreCase) && dataType.Equals("int32", StringComparison.OrdinalIgnoreCase))
                return false;

            if (propertyTypeName.Equals("date", StringComparison.OrdinalIgnoreCase) && dataType.Equals("datetime", StringComparison.OrdinalIgnoreCase))
                return false;

            if (propertyTypeName.Equals("bigint", StringComparison.OrdinalIgnoreCase) && dataType.Equals("int64", StringComparison.OrdinalIgnoreCase))
                return false;

            if (propertyTypeName.Equals("blob", StringComparison.OrdinalIgnoreCase) && dataType.Equals("byte[]", StringComparison.OrdinalIgnoreCase))
                return false;

            if (propertyTypeName == null || dataType == null)
            {
                return propertyTypeName != dataType;
            }

            if (this.Context.CurrentConnectionConfig.DbType == DbType.SqlServer &&
                dataType.Equals("timestamp", StringComparison.OrdinalIgnoreCase) &&
                propertyTypeName.Equals("varbinary", StringComparison.OrdinalIgnoreCase))
                return false;

            if (propertyTypeName.IsIn("int", "long") && dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase) &&
                dc.Length == 38 && dc.DecimalDigits == 127)
                return false;

            if (dataType.Equals("numeric", StringComparison.OrdinalIgnoreCase) && propertyTypeName.Equals("decimal", StringComparison.OrdinalIgnoreCase))
                return false;

            if (ec.UnderType == UtilConstants.BoolType && dc.OracleDataType?.Equals("number", StringComparison.OrdinalIgnoreCase) == true)
                return false;

            if (ec.UnderType == UtilConstants.LongType && dc.Length == 19 && dc.DecimalDigits == 0 &&
                dc.OracleDataType?.Equals("number", StringComparison.OrdinalIgnoreCase) == true)
                return false;

            if (dataType.EqualCase("timestamp") && propertyTypeName.EqualCase("timestamptz"))
            {
                return false;
            }

            return !propertyTypeName.Equals(dataType, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取类型名称
        /// </summary>
        /// <param name="name">类型名称</param>
        /// <returns>处理后的类型名称</returns>
        protected string GetType(string name)
        {
            switch (name)
            {
                case "UInt32":
                case "UInt16":
                case "UInt64":
                    return name.TrimStart('U');
                case "char":
                    return "string";
                default:
                    return name;
            }
        }

        #endregion
    }
}