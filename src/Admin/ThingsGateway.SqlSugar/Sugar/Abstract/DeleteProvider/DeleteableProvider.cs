using System.Collections;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 删除操作提供者
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class DeleteableProvider<T> : IDeleteable<T> where T : class, new()
    {
        /// <summary>
        /// SqlSugar客户端
        /// </summary>
        public ISqlSugarClient Context { get; set; }
        /// <summary>
        /// 数据库访问对象
        /// </summary>
        public IAdo Db { get { return Context.Ado; } }
        /// <summary>
        /// SQL构建器
        /// </summary>
        public ISqlBuilder SqlBuilder { get; set; }
        /// <summary>
        /// 删除构建器
        /// </summary>
        public DeleteBuilder DeleteBuilder { get; set; }
        /// <summary>
        /// 旧映射表列表
        /// </summary>
        public MappingTableList OldMappingTableList { get; set; }
        /// <summary>
        /// 是否使用AS语法
        /// </summary>
        public bool IsAs { get; set; }
        /// <summary>
        /// 是否启用差异日志
        /// </summary>
        public bool IsEnableDiffLogEvent { get; set; }
        /// <summary>
        /// 差异日志模型
        /// </summary>
        public DiffLogModel DiffModel { get; set; }
        /// <summary>
        /// 临时主键列表
        /// </summary>
        public List<string> TempPrimaryKeys { get; set; }
        /// <summary>
        /// 移除缓存函数
        /// </summary>
        internal Action RemoveCacheFunc { get; set; }
        /// <summary>
        /// 删除对象列表
        /// </summary>
        public IReadOnlyCollection<T> DeleteObjects { get; set; }
        /// <summary>
        /// 实体信息
        /// </summary>
        public EntityInfo EntityInfo
        {
            get
            {
                return this.Context.EntityMaintenance.GetEntityInfo<T>();
            }
        }

        /// <summary>
        /// 添加到队列
        /// </summary>
        public void AddQueue()
        {
            var sqlObj = this.ToSql();
            this.Context.Queues.Add(sqlObj.Key, sqlObj.Value);
        }

        /// <summary>
        /// 执行删除命令
        /// </summary>
        public int ExecuteCommand()
        {
            string sql;
            IReadOnlyCollection<SugarParameter> paramters;
            _ExecuteCommand(out sql, out paramters);
            var result = Db.ExecuteCommand(sql, paramters);
            After(sql);
            return result;
        }

        /// <summary>
        /// 检查是否有数据变更
        /// </summary>
        public bool ExecuteCommandHasChange()
        {
            return ExecuteCommand() > 0;
        }

        /// <summary>
        /// 异步执行删除命令
        /// </summary>
        public Task<int> ExecuteCommandAsync(CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return ExecuteCommandAsync();
        }

        /// <summary>
        /// 异步执行删除命令
        /// </summary>
        public async Task<int> ExecuteCommandAsync()
        {
            string sql;
            IReadOnlyCollection<SugarParameter> paramters;
            _ExecuteCommand(out sql, out paramters);
            var result = await Db.ExecuteCommandAsync(sql, paramters).ConfigureAwait(false);
            After(sql);
            return result;
        }

        /// <summary>
        /// 异步检查是否有数据变更
        /// </summary>
        public async Task<bool> ExecuteCommandHasChangeAsync()
        {
            return await ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }

        /// <summary>
        /// 指定表名类型
        /// </summary>
        public IDeleteable<T> AsType(Type tableNameType)
        {
            return AS(this.Context.EntityMaintenance.GetEntityInfo(tableNameType).DbTableName);
        }

        /// <summary>
        /// 指定表名
        /// </summary>
        public IDeleteable<T> AS(string tableName)
        {
            if (tableName == null) return this;
            this.DeleteBuilder.AsName = tableName;
            return this;
        }

        /// <summary>
        /// 条件启用差异日志
        /// </summary>
        public IDeleteable<T> EnableDiffLogEventIF(bool isEnableDiffLogEvent, object businessData = null)
        {
            if (isEnableDiffLogEvent)
            {
                return EnableDiffLogEvent(businessData);
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// 启用差异日志
        /// </summary>
        public IDeleteable<T> EnableDiffLogEvent(object businessData = null)
        {
            DiffModel = new DiffLogModel();
            this.IsEnableDiffLogEvent = true;
            DiffModel.BusinessData = businessData;
            DiffModel.DiffType = DiffType.delete;
            return this;
        }

        /// <summary>
        /// 根据对象列表设置删除条件
        /// </summary>
        public IDeleteable<T> Where(IReadOnlyCollection<T> deleteObjs)
        {
            this.DeleteObjects = deleteObjs;
            if (deleteObjs == null || deleteObjs.Count == 0)
            {
                Where(SqlBuilder.SqlFalse);
                return this;
            }
            DataAop(deleteObjs);
            string tableName = this.Context.EntityMaintenance.GetTableName<T>();
            var primaryFields = this.GetPrimaryKeys();
            var isSinglePrimaryKey = primaryFields.Count == 1;
            var isNvarchar = false;
            Check.Exception(primaryFields.IsNullOrEmpty(), string.Format("Table {0} with no primarykey", tableName));
            if (isSinglePrimaryKey)
            {
                List<object> primaryKeyValues = new List<object>();
                var primaryField = primaryFields.Single();
                foreach (var deleteObj in deleteObjs)
                {
                    var entityPropertyName = this.Context.EntityMaintenance.GetPropertyName<T>(primaryField);
                    var columnInfo = EntityInfo.Columns.Single(it => it.PropertyName.Equals(entityPropertyName, StringComparison.CurrentCultureIgnoreCase));
                    isNvarchar = columnInfo.SqlParameterDbType is System.Data.DbType dbtype && dbtype == System.Data.DbType.String;
                    var value = columnInfo.PropertyInfo.GetValue(deleteObj, null);
                    value = UtilMethods.GetConvertValue(value);
                    if (this.Context.CurrentConnectionConfig?.MoreSettings?.TableEnumIsString != true &&
                        columnInfo.SqlParameterDbType == null &&
                        columnInfo.PropertyInfo.PropertyType.IsEnum())
                    {
                        value = Convert.ToInt64(value);
                    }
                    primaryKeyValues.Add(value);
                }
                if (this.Context.CurrentConnectionConfig.DbType == DbType.Oracle && primaryKeyValues.Count >= 1000)
                {
                    List<string> inItems = new List<string>();
                    this.Context.Utilities.PageEach(primaryKeyValues, 999, pageItems =>
                    {
                        var inValueString = pageItems.ToJoinSqlInVals();
                        var whereItem = string.Format(DeleteBuilder.WhereInTemplate, SqlBuilder.GetTranslationColumnName(primaryFields.Single()), inValueString);
                        inItems.Add(whereItem);
                    });
                    Where($"({string.Join(" OR ", inItems)})");
                }
                else if (primaryKeyValues.Count < 10000)
                {
                    var inValueString = string.Empty;
                    if (isNvarchar)
                    {
                        inValueString = primaryKeyValues.ToJoinSqlInValsByVarchar();
                    }
                    else
                    {
                        inValueString = primaryKeyValues.ToJoinSqlInVals();
                    }
                }
                else
                {
                    if (DeleteBuilder.BigDataInValues == null)
                        DeleteBuilder.BigDataInValues = new List<object>();
                    DeleteBuilder.BigDataInValues.AddRange(primaryKeyValues);
                    DeleteBuilder.BigDataField = primaryField;
                }
            }
            else
            {
                StringBuilder whereInSql = new StringBuilder();
                bool first = true;
                foreach (var deleteObj in deleteObjs)
                {
                    StringBuilder orString = new StringBuilder();
                    if (!first)
                    {
                        orString.Append(DeleteBuilder.WhereInOrTemplate + UtilConstants.Space);
                    }
                    first = false;
                    int i = 0;
                    StringBuilder andString = new StringBuilder();
                    foreach (var primaryField in primaryFields)
                    {
                        if (i != 0)
                            andString.Append(DeleteBuilder.WhereInAndTemplate + UtilConstants.Space);
                        var columnInfo = EntityInfo.Columns.Single(t => t.PropertyName.EqualCase(primaryField) || t.DbColumnName.EqualCase(primaryField));
                        var entityValue = columnInfo.PropertyInfo.GetValue(deleteObj, null);
                        if (this.Context.CurrentConnectionConfig?.MoreSettings?.TableEnumIsString != true &&
                        columnInfo.SqlParameterDbType == null &&
                        columnInfo.PropertyInfo.PropertyType.IsEnum())
                        {
                            entityValue = Convert.ToInt64(entityValue);
                        }
                        var tempequals = DeleteBuilder.WhereInEqualTemplate;
                        if (this.Context.CurrentConnectionConfig.MoreSettings?.DisableNvarchar == true)
                        {
                            tempequals = $"{SqlBuilder.SqlTranslationLeft}{{0}}{SqlBuilder.SqlTranslationRight}='{{1}}' ";
                        }
                        if (SqlBuilder.SqlParameterKeyWord == ":")
                        {
                            var isAutoToUpper = this.Context.CurrentConnectionConfig?.MoreSettings?.IsAutoToUpper ?? true;
                            if (entityValue != null && UtilMethods.GetUnderType(entityValue.GetType()) == UtilConstants.DateType)
                            {
                                andString.AppendFormat("\"{0}\"={1} ", primaryField.ToUpper(isAutoToUpper), "to_date('" + entityValue.ObjToDate().ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS') ");
                            }
                            else
                            {
                                andString.AppendFormat(tempequals.Replace("N", "") + " ", primaryField.ToUpper(isAutoToUpper), entityValue);
                            }
                        }
                        else if (this.Context.CurrentConnectionConfig.DbType == DbType.PostgreSQL && (this.Context.CurrentConnectionConfig.MoreSettings == null || this.Context.CurrentConnectionConfig.MoreSettings?.PgSqlIsAutoToLower == true))
                        {
                            andString.AppendFormat("\"{0}\"={1} ", primaryField.ToLower(), new PostgreSQLExpressionContext().GetValue(entityValue));
                        }
                        else if (entityValue != null && UtilMethods.IsNumber(UtilMethods.GetUnderType(entityValue.GetType()).Name))
                        {
                            andString.AppendFormat("{0}={1} ", this.SqlBuilder.GetTranslationColumnName(primaryField), $"{entityValue}");
                        }
                        else if (entityValue != null && UtilMethods.GetUnderType(entityValue.GetType()) == UtilConstants.DateType)
                        {
                            andString.AppendFormat("{0}={1} ", this.SqlBuilder.GetTranslationColumnName(primaryField), this.DeleteBuilder.LambdaExpressions.DbMehtods.ToDate(new MethodCallExpressionModel()
                            {
                                Args = new List<MethodCallExpressionArgs>()
                             {
                                 new MethodCallExpressionArgs()
                                 {
                                      IsMember=false,
                                      MemberName="'"+entityValue.ObjToDate().ToString("yyyy-MM-dd HH:mm:ss.fff")+"'"
                                 }
                             }
                            }));
                        }
                        else
                        {
                            if ((columnInfo.SqlParameterDbType.ObjToString() == System.Data.DbType.AnsiString.ObjToString()) || !(entityValue is string) || this.Context.CurrentConnectionConfig?.MoreSettings?.DisableNvarchar == true)
                            {
                                tempequals = tempequals.Replace("=N'", "='");
                            }
                            else
                            {
                                tempequals = SqlBuilder.RemoveN(tempequals);
                            }
                            entityValue = UtilMethods.GetConvertValue(entityValue);
                            andString.AppendFormat(tempequals, primaryField, entityValue);
                        }
                        ++i;
                    }
                    orString.AppendFormat(DeleteBuilder.WhereInAreaTemplate, andString);
                    whereInSql.Append(orString);
                }
                Where(string.Format(DeleteBuilder.WhereInAreaTemplate, whereInSql.ToString()));
            }
            return this;
        }

        /// <summary>
        /// 条件Where
        /// </summary>
        public IDeleteable<T> WhereIF(bool isWhere, Expression<Func<T, bool>> expression)
        {
            if (DeleteBuilder.WhereInfos.Count != 0 != true)
            {
                Check.ExceptionEasy(!StaticConfig.EnableAllWhereIF, "Need to program startup configuration StaticConfig. EnableAllWhereIF = true; Tip: This operation is very risky if there are no conditions it is easy to update the entire table", " 需要程序启动时配置StaticConfig.EnableAllWhereIF=true; 提示：该操作存在很大的风险如果没有条件很容易将整个表全部更新");
            }
            if (isWhere)
            {
                return Where(expression);
            }
            return this;
        }

        /// <summary>
        /// 设置Where条件
        /// </summary>
        public IDeleteable<T> Where(Expression<Func<T, bool>> expression)
        {
            var expResult = DeleteBuilder.GetExpressionValue(expression, ResolveExpressType.WhereSingle);
            var whereString = expResult.GetResultString();
            if (expression.ToString().Contains("Subqueryable()"))
            {
                var entityTableName = this.EntityInfo.DbTableName;
                if (this.DeleteBuilder.AsName.HasValue())
                {
                    entityTableName = this.DeleteBuilder.AsName;
                }
                if (ExpressionTool.GetParameters(expression).First().Type == typeof(T))
                {
                    var tableName = this.SqlBuilder.GetTranslationColumnName(entityTableName);
                    whereString = whereString.Replace(tableName, $"( SELECT * FROM {tableName})  ");
                }
                whereString = whereString.Replace(this.SqlBuilder.GetTranslationColumnName(expression.Parameters[0].Name) + ".", this.SqlBuilder.GetTranslationTableName(entityTableName) + ".");
            }
            else if (expResult.IsNavicate)
            {
                var entityTableName2 = this.EntityInfo.DbTableName;
                if (this.DeleteBuilder.AsName.HasValue())
                {
                    entityTableName2 = this.DeleteBuilder.AsName;
                }
                whereString = whereString.Replace(expression.Parameters[0].Name + ".", this.SqlBuilder.GetTranslationTableName(entityTableName2) + ".");
                whereString = whereString.Replace(this.SqlBuilder.GetTranslationColumnName(expression.Parameters[0].Name) + ".", this.SqlBuilder.GetTranslationTableName(entityTableName2) + ".");
            }
            DeleteBuilder.WhereInfos.Add(whereString);
            return this;
        }

        /// <summary>
        /// 根据对象设置删除条件
        /// </summary>
        public IDeleteable<T> WhereT(T deleteObj)
        {
            Check.Exception(GetPrimaryKeys().IsNullOrEmpty(), "Where(entity) Primary key required");
            Where([deleteObj]);
            return this;
        }

        /// <summary>
        /// 设置Where条件
        /// </summary>
        public IDeleteable<T> Where(string whereString, object parameters = null)
        {
            DeleteBuilder.WhereInfos.Add(whereString);
            if (parameters != null)
            {
                if (DeleteBuilder.Parameters == null)
                {
                    DeleteBuilder.Parameters = new List<SugarParameter>();
                }
                DeleteBuilder.Parameters.AddRange(Context.Ado.GetParameters(parameters));
            }
            return this;
        }

        /// <summary>
        /// 设置Where条件
        /// </summary>
        public IDeleteable<T> Where(string whereString, SugarParameter parameter)
        {
            DeleteBuilder.WhereInfos.Add(whereString);
            if (DeleteBuilder.Parameters == null)
            {
                DeleteBuilder.Parameters = new List<SugarParameter>();
            }
            DeleteBuilder.Parameters.Add(parameter);
            return this;
        }

        /// <summary>
        /// 设置Where条件
        /// </summary>
        public IDeleteable<T> Where(string whereString, IReadOnlyCollection<SugarParameter> parameters)
        {
            DeleteBuilder.WhereInfos.Add(whereString);
            if (DeleteBuilder.Parameters == null)
            {
                DeleteBuilder.Parameters = new List<SugarParameter>();
            }
            DeleteBuilder.Parameters.AddRange(parameters);
            return this;
        }

        /// <summary>
        /// 设置Where条件
        /// </summary>
        public IDeleteable<T> Where(string whereString, List<SugarParameter> parameters)
        {
            DeleteBuilder.WhereInfos.Add(whereString);
            if (DeleteBuilder.Parameters == null)
            {
                DeleteBuilder.Parameters = new List<SugarParameter>();
            }
            DeleteBuilder.Parameters.AddRange(parameters);
            return this;
        }

        /// <summary>
        /// 设置条件模型
        /// </summary>
        public IDeleteable<T> Where(List<IConditionalModel> conditionalModels, bool isWrap)
        {
            if (conditionalModels.Count == 0)
            {
                return Where("1=2");
            }
            var sql = this.Context.Queryable<T>().SqlBuilder.ConditionalModelToSql(conditionalModels);
            var result = this;
            if (isWrap)
            {
                result.Where($"({sql.Key})", sql.Value);
            }
            else
            {
                result.Where(sql.Key, sql.Value);
            }
            return result;
        }

        /// <summary>
        /// 设置条件模型
        /// </summary>
        public IDeleteable<T> Where(List<IConditionalModel> conditionalModels)
        {
            if (conditionalModels.Count == 0)
            {
                return Where("1=2");
            }
            var sql = this.Context.Queryable<T>().SqlBuilder.ConditionalModelToSql(conditionalModels);
            var result = this;
            result.Where(sql.Key, sql.Value);
            return result;
        }

        /// <summary>
        /// 设置条件列
        /// </summary>
        public IDeleteable<T> WhereColumns(T data, Expression<Func<T, object>> columns)
        {
            return WhereColumns(new List<T>() { data }, columns);
        }

        /// <summary>
        /// 设置条件列
        /// </summary>
        public IDeleteable<T> WhereColumns(List<T> list, Expression<Func<T, object>> columns)
        {
            if (columns != null)
            {
                TempPrimaryKeys = DeleteBuilder.GetExpressionValue(columns, ResolveExpressType.ArraySingle).GetResultArray().Select(it => this.SqlBuilder.GetNoTranslationColumnName(it)).ToList();
            }
            this.Where(list);
            return this;
        }

        /// <summary>
        /// 设置条件列
        /// </summary>
        public IDeleteable<T> WhereColumns(List<Dictionary<string, object>> list)
        {
            List<IConditionalModel> conditionalModels = new List<IConditionalModel>();
            foreach (var model in list)
            {
                int i = 0;
                var clist = new List<KeyValuePair<WhereType, ConditionalModel>>();
                foreach (var item in model.Keys)
                {
                    clist.Add(new KeyValuePair<WhereType, ConditionalModel>(i == 0 ? WhereType.Or : WhereType.And, new ConditionalModel()
                    {
                        FieldName = item,
                        ConditionalType = ConditionalType.Equal,
                        FieldValue = model[item].ObjToStringNoTrim(),
                        CSharpTypeName = model[item] == null ? null : model[item].GetType().Name
                    }));
                    i++;
                }
                conditionalModels.Add(new ConditionalCollections()
                {
                    ConditionalList = clist
                });
            }
            return this.Where(conditionalModels);
        }

        /// <summary>
        /// 移除数据缓存
        /// </summary>
        public IDeleteable<T> RemoveDataCache()
        {
            this.RemoveCacheFunc = () =>
            {
                var cacheService = this.Context.CurrentConnectionConfig.ConfigureExternalServices.DataInfoCacheService;
                CacheSchemeMain.RemoveCache(cacheService, this.Context.EntityMaintenance.GetTableName<T>());
            };
            return this;
        }

        /// <summary>
        /// 启用查询过滤器
        /// </summary>
        public IDeleteable<T> EnableQueryFilter()
        {
            var queryable = this.Context.Queryable<T>();
            queryable.QueryBuilder.LambdaExpressions.ParameterIndex = 1000;
            var sqlable = queryable.ToSql();
            var whereInfos = Regex.Split(sqlable.Key, " Where ", RegexOptions.IgnoreCase);
            if (whereInfos.Length > 1)
            {
                this.Where(whereInfos.Last(), sqlable.Value);
            }
            return this;
        }

        /// <summary>
        /// 启用指定类型的查询过滤器
        /// </summary>
        public IDeleteable<T> EnableQueryFilter(Type type)
        {
            var queryable = this.Context.Queryable<T>().Filter(type);
            queryable.QueryBuilder.LambdaExpressions.ParameterIndex = 1000;
            var sqlable = queryable.ToSql();
            var whereInfos = Regex.Split(sqlable.Key, " Where ", RegexOptions.IgnoreCase);
            if (whereInfos.Length > 1)
            {
                this.Where(whereInfos.Last(), sqlable.Value);
            }
            return this;
        }

        /// <summary>
        /// 分表删除
        /// </summary>
        public SplitTableDeleteProvider<T> SplitTable(Func<List<SplitTableInfo>, IEnumerable<SplitTableInfo>> getTableNamesFunc)
        {
            UtilMethods.StartCustomSplitTable(this.Context, typeof(T));
            this.Context.MappingTables.Add(this.EntityInfo.EntityName, this.EntityInfo.DbTableName);
            SplitTableDeleteProvider<T> result = new SplitTableDeleteProvider<T>();
            result.Context = this.Context;
            SplitTableContext helper = new SplitTableContext((SqlSugarProvider)Context)
            {
                EntityInfo = this.EntityInfo
            };
            var tables = getTableNamesFunc(helper.GetTables());
            result.Tables = tables;
            result.deleteobj = this;
            return result;
        }

        /// <summary>
        /// 分表删除
        /// </summary>
        public SplitTableDeleteByObjectProvider<T> SplitTable()
        {
            UtilMethods.StartCustomSplitTable(this.Context, typeof(T));
            SplitTableDeleteByObjectProvider<T> result = new SplitTableDeleteByObjectProvider<T>();
            result.Context = this.Context;
            Check.ExceptionEasy(this.DeleteObjects == null, "SplitTable() +0  only List<T> can be deleted", "SplitTable()无参数重载只支持根据实体集合删除");
            result.deleteObjects = this.DeleteObjects;
            SplitTableContext helper = new SplitTableContext((SqlSugarProvider)Context)
            {
                EntityInfo = this.EntityInfo
            };
            result.deleteobj = this;
            return result;
        }

        /// <summary>
        /// 逻辑删除
        /// </summary>
        public LogicDeleteProvider<T> IsLogic()
        {
            LogicDeleteProvider<T> result = new LogicDeleteProvider<T>();
            result.DeleteBuilder = this.DeleteBuilder;
            result.Deleteable = this;
            return result;
        }

        /// <summary>
        /// 按条件移除数据缓存
        /// </summary>
        public IDeleteable<T> RemoveDataCache(string likeString)
        {
            this.RemoveCacheFunc = () =>
            {
                var cacheService = this.Context.CurrentConnectionConfig.ConfigureExternalServices.DataInfoCacheService;
                CacheSchemeMain.RemoveCacheByLike(cacheService, likeString);
            };
            return this;
        }

        /// <summary>
        /// IN条件
        /// </summary>
        public IDeleteable<T> In<PkType>(IReadOnlyCollection<PkType> primaryKeyValues)
        {
            if (primaryKeyValues == null || primaryKeyValues.Count == 0)
            {
                Where(SqlBuilder.SqlFalse);
                return this;
            }
            string tableName = this.Context.EntityMaintenance.GetTableName<T>();
            string primaryField = null;
            primaryField = GetPrimaryKeys().FirstOrDefault();
            Check.ArgumentNullException(primaryField, "Table " + tableName + " with no primarykey");
            if (primaryKeyValues.Count < 10000)
            {
                Where(string.Format(DeleteBuilder.WhereInTemplate, SqlBuilder.GetTranslationColumnName(primaryField), primaryKeyValues.ToJoinSqlInVals()));
            }
            else
            {
                if (DeleteBuilder.BigDataInValues == null)
                    DeleteBuilder.BigDataInValues = new List<object>();
                DeleteBuilder.BigDataInValues.AddRange(primaryKeyValues.Select(it => (object)it));
                DeleteBuilder.BigDataField = primaryField;
            }
            return this;
        }

        /// <summary>
        /// IN条件
        /// </summary>
        public IDeleteable<T> InT<PkType>(PkType primaryKeyValue)
        {
            if (typeof(PkType).FullName.IsCollectionsList())
            {
                var newValues = new List<object>();
                foreach (var item in primaryKeyValue as IEnumerable)
                {
                    newValues.Add(item);
                }
                return In(newValues);
            }

            In(new PkType[] { primaryKeyValue });
            return this;
        }

        /// <summary>
        /// IN条件
        /// </summary>
        public IDeleteable<T> InT<PkType>(Expression<Func<T, object>> inField, PkType primaryKeyValue)
        {
            var lamResult = DeleteBuilder.GetExpressionValue(inField, ResolveExpressType.FieldSingle);
            var fieldName = lamResult.GetResultString();
            TempPrimaryKeys = new List<string>() { fieldName };
            var result = In([primaryKeyValue]);
            TempPrimaryKeys = null;
            return this;
        }

        /// <summary>
        /// IN条件
        /// </summary>
        public IDeleteable<T> In<PkType>(Expression<Func<T, object>> inField, IReadOnlyCollection<PkType> primaryKeyValues)
        {
            var lamResult = DeleteBuilder.GetExpressionValue(inField, ResolveExpressType.FieldSingle);
            var fieldName = lamResult.GetResultString();
            TempPrimaryKeys = new List<string>() { fieldName };
            var result = In(primaryKeyValues);
            TempPrimaryKeys = null;
            return this;
        }

        /// <summary>
        /// IN条件(子查询)
        /// </summary>
        public IDeleteable<T> In<PkType>(Expression<Func<T, object>> inField, ISugarQueryable<PkType> childQueryExpression)
        {
            var lamResult = DeleteBuilder.GetExpressionValue(inField, ResolveExpressType.FieldSingle);
            var fieldName = lamResult.GetResultString();
            var sql = childQueryExpression.ToSql();
            Where($" {fieldName} IN ( SELECT {fieldName} FROM ( {sql.Key} ) SUBDEL) ", sql.Value);
            return this;
        }

        /// <summary>
        /// IN条件
        /// </summary>
        public IDeleteable<T> In<PkType>(string inField, IReadOnlyCollection<PkType> primaryKeyValues)
        {
            TempPrimaryKeys = new List<string>() { inField };
            var result = In(primaryKeyValues);
            TempPrimaryKeys = null;
            return this;
        }

        /// <summary>
        /// 分页删除
        /// </summary>
        public DeleteablePage<T> PageSize(int pageSize)
        {
            Check.ExceptionEasy(this.DeleteObjects == null, "PageSize can only be deleted as a List<Class> entity collection", "Deleteable.PageSize()只能是List<Class>实体集合方式删除,并且集合不能为null");
            DeleteablePage<T> result = new DeleteablePage<T>();
            result.DataList = this.DeleteObjects;
            result.Context = this.Context;
            result.DiffModel = this.DiffModel;
            result.IsEnableDiffLogEvent = this.IsEnableDiffLogEvent;
            result.TableName = this.DeleteBuilder.AsName;
            result.PageSize = pageSize;
            return result;
        }

        /// <summary>
        /// 设置锁
        /// </summary>
        public IDeleteable<T> With(string lockString)
        {
            if (this.Context.CurrentConnectionConfig.DbType == DbType.SqlServer)
                DeleteBuilder.TableWithString = lockString;
            return this;
        }

        /// <summary>
        /// 获取SQL语句
        /// </summary>
        public virtual string ToSqlString()
        {
            var sqlObj = this.ToSql();
            var result = sqlObj.Key;
            if (result == null) return null;
            result = UtilMethods.GetSqlString(this.Context.CurrentConnectionConfig, sqlObj);
            return result;
        }

        /// <summary>
        /// 获取SQL和参数
        /// </summary>
        public KeyValuePair<string, IReadOnlyCollection<SugarParameter>> ToSql()
        {
            DeleteBuilder.EntityInfo = this.Context.EntityMaintenance.GetEntityInfo<T>();
            string sql = DeleteBuilder.ToSqlString();
            var paramters = DeleteBuilder.Parameters == null ? null : DeleteBuilder.Parameters;
            RestoreMapping();
            return new KeyValuePair<string, IReadOnlyCollection<SugarParameter>>(sql, paramters);
        }

        /// <summary>
        /// 获取主键列表
        /// </summary>
        private List<string> GetPrimaryKeys()
        {
            if (TempPrimaryKeys.HasValue())
            {
                return TempPrimaryKeys;
            }
            else
            {
                return this.EntityInfo.Columns.Where(it => it.IsPrimarykey).Select(it => it.DbColumnName).ToList();
            }
        }

        /// <summary>
        /// 执行删除命令
        /// </summary>
        private void _ExecuteCommand(out string sql, out IReadOnlyCollection<SugarParameter> paramters)
        {
            DeleteBuilder.EntityInfo = this.Context.EntityMaintenance.GetEntityInfo<T>();
            sql = DeleteBuilder.ToSqlString();
            paramters = DeleteBuilder.Parameters == null ? null : DeleteBuilder.Parameters;
            RestoreMapping();
            AutoRemoveDataCache();
            Before(sql);
        }

        /// <summary>
        /// 获取自增键列表
        /// </summary>
        protected virtual List<string> GetIdentityKeys()
        {
            return this.EntityInfo.Columns.Where(it => it.IsIdentity).Select(it => it.DbColumnName).ToList();
        }

        /// <summary>
        /// 恢复映射
        /// </summary>
        private void RestoreMapping()
        {
            if (IsAs)
            {
                this.Context.MappingTables = OldMappingTableList;
            }
        }

        /// <summary>
        /// 自动移除数据缓存
        /// </summary>
        private void AutoRemoveDataCache()
        {
            var moreSetts = this.Context.CurrentConnectionConfig.MoreSettings;
            var extService = this.Context.CurrentConnectionConfig.ConfigureExternalServices;
            if (moreSetts?.IsAutoRemoveDataCache == true && extService?.DataInfoCacheService != null)
            {
                this.RemoveDataCache();
            }
        }

        /// <summary>
        /// 执行后操作
        /// </summary>
        protected virtual void After(string sql)
        {
            if (this.IsEnableDiffLogEvent)
            {
                var isDisableMasterSlaveSeparation = this.Context.Ado.IsDisableMasterSlaveSeparation;
                this.Context.Ado.IsDisableMasterSlaveSeparation = true;
                var parameters = DeleteBuilder.Parameters;
                if (parameters == null)
                    parameters = new List<SugarParameter>();
                DiffModel.AfterData = null;
                DiffModel.Time = this.Context.Ado.SqlExecutionTime;
                if (this.Context.CurrentConnectionConfig.AopEvents.OnDiffLogEvent != null)
                    this.Context.CurrentConnectionConfig.AopEvents.OnDiffLogEvent(DiffModel);
                this.Context.Ado.IsDisableMasterSlaveSeparation = isDisableMasterSlaveSeparation;
            }
            if (this.RemoveCacheFunc != null)
            {
                this.RemoveCacheFunc();
            }
            DataChangesAop(this.DeleteObjects);
        }

        /// <summary>
        /// 执行前操作
        /// </summary>
        protected virtual void Before(string sql)
        {
            if (this.IsEnableDiffLogEvent)
            {
                var isDisableMasterSlaveSeparation = this.Context.Ado.IsDisableMasterSlaveSeparation;
                this.Context.Ado.IsDisableMasterSlaveSeparation = true;
                var parameters = DeleteBuilder.Parameters;
                if (parameters == null)
                    parameters = new List<SugarParameter>();
                DiffModel.BeforeData = GetDiffTable(sql, parameters);
                DiffModel.Sql = sql;
                DiffModel.Parameters = parameters;
                this.Context.Ado.IsDisableMasterSlaveSeparation = isDisableMasterSlaveSeparation;
            }
        }

        /// <summary>
        /// 获取差异表
        /// </summary>
        protected virtual List<DiffLogTableInfo> GetDiffTable(string sql, List<SugarParameter> parameters)
        {
            List<DiffLogTableInfo> result = new List<DiffLogTableInfo>();
            var whereSql = Regex.Replace(sql, ".* WHERE ", "", RegexOptions.Singleline);
            var dt = this.Context.Queryable<T>().AS(this.DeleteBuilder.AsName).Filter(null, true).Where(whereSql).AddParameters(parameters).ToDataTable();
            if (dt.Rows?.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    DiffLogTableInfo item = new DiffLogTableInfo();
                    item.TableDescription = this.EntityInfo.TableDescription;
                    item.TableName = this.EntityInfo.DbTableName;
                    item.Columns = new List<DiffLogColumnInfo>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        var sugarColumn = this.EntityInfo.Columns.Where(it => it.DbColumnName != null).First(it =>
                            it.DbColumnName.Equals(col.ColumnName, StringComparison.CurrentCultureIgnoreCase));
                        DiffLogColumnInfo addItem = new DiffLogColumnInfo();
                        addItem.Value = row[col.ColumnName];
                        addItem.ColumnName = col.ColumnName;
                        addItem.IsPrimaryKey = sugarColumn.IsPrimarykey;
                        addItem.ColumnDescription = sugarColumn.ColumnDescription;
                        item.Columns.Add(addItem);
                    }
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// 数据AOP
        /// </summary>
        protected virtual void DataAop(object deleteObj)
        {
            var dataEvent = this.Context.CurrentConnectionConfig.AopEvents?.DataExecuting;
            if (deleteObj != null && dataEvent != null)
            {
                var model = new DataFilterModel()
                {
                    OperationType = DataFilterType.DeleteByObject,
                    EntityValue = deleteObj,
                    EntityColumnInfo = this.EntityInfo.Columns.FirstOrDefault()
                };
                dataEvent(deleteObj, model);
            }
        }

        /// <summary>
        /// 数据变更AOP
        /// </summary>
        protected virtual void DataChangesAop(IReadOnlyCollection<T> deleteObjs)
        {
            var dataEvent = this.Context.CurrentConnectionConfig.AopEvents?.DataChangesExecuted;
            if (dataEvent != null && deleteObjs != null)
            {
                foreach (var deleteObj in deleteObjs)
                {
                    if (deleteObj != null)
                    {
                        var model = new DataFilterModel()
                        {
                            OperationType = DataFilterType.DeleteByObject,
                            EntityValue = deleteObj,
                            EntityColumnInfo = this.EntityInfo.Columns.FirstOrDefault()
                        };
                        dataEvent(deleteObj, model);
                    }
                }
            }
        }
    }
}