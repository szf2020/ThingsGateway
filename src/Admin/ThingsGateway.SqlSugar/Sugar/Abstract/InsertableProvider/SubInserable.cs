using System.Data.SqlTypes;
using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 子表插入操作类
    /// </summary>
    /// <typeparam name="T">主表实体类型</typeparam>
    public class SubInsertable<T> : ISubInsertable<T> where T : class, new()
    {
        /// <summary>
        /// 实体信息
        /// </summary>
        internal EntityInfo Entity { get; set; }
        /// <summary>
        /// 子表插入表达式列表
        /// </summary>
        internal List<SubInsertTreeExpression> SubList { get; set; }
        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        internal SqlSugarProvider Context { get; set; }
        /// <summary>
        /// 待插入对象数组
        /// </summary>
        internal IReadOnlyCollection<T> InsertObjects { get; set; }
        /// <summary>
        /// 插入构建器
        /// </summary>
        internal InsertBuilder InsertBuilder { get; set; }
        /// <summary>
        /// 主键名称
        /// </summary>
        internal string Pk { get; set; }

        /// <summary>
        /// 添加子表列表
        /// </summary>
        public ISubInsertable<T> AddSubList(Expression<Func<T, object>> items)
        {
            if (this.SubList == null)
                this.SubList = new List<SubInsertTreeExpression>();
            this.SubList.Add(new SubInsertTreeExpression() { Expression = items });
            return this;
        }
        /// <summary>
        /// 添加子表树形结构
        /// </summary>
        public ISubInsertable<T> AddSubList(Expression<Func<T, SubInsertTree>> tree)
        {
            try
            {
                var lamda = (tree as LambdaExpression);
                var memInit = lamda.Body as MemberInitExpression;
                if (memInit.Bindings != null)
                {
                    MemberAssignment memberAssignment = (MemberAssignment)memInit.Bindings[0];
                    SubList.Add(new SubInsertTreeExpression()
                    {
                        Expression = memberAssignment.Expression,
                        Childs = GetSubInsertTree(((MemberAssignment)memInit.Bindings[1]).Expression)
                    });
                }
            }
            catch
            {
                { throw new SqlSugarException($"{tree} format error "); }
            }
            return this;
        }

        /// <summary>
        /// 获取子表插入树
        /// </summary>
        private List<SubInsertTreeExpression> GetSubInsertTree(Expression expression)
        {
            List<SubInsertTreeExpression> resul = new List<SubInsertTreeExpression>();

            if (expression is ListInitExpression)
            {
                ListInitExpression exp = expression as ListInitExpression;
                foreach (var item in exp.Initializers)
                {
                    SubInsertTreeExpression tree = new SubInsertTreeExpression();
                    var memInit = item.Arguments[0] as MemberInitExpression;
                    if (memInit.Bindings != null)
                    {
                        MemberAssignment memberAssignment = (MemberAssignment)memInit.Bindings[0];
                        tree.Expression = memberAssignment.Expression;
                        if (memInit.Bindings.Count > 1)
                        {
                            tree.Childs = GetSubInsertTree(((MemberAssignment)memInit.Bindings[1]).Expression);
                        }
                    }
                    resul.Add(tree);
                }
            }
            return resul;
        }

        /// <summary>
        /// 异步执行插入命令
        /// </summary>
        public async Task<object> ExecuteCommandAsync()
        {
            object resut = 0;
            await Task.Run(() => resut = ExecuteCommand()).ConfigureAwait(false);
            return resut;
        }
        /// <summary>
        /// 执行插入命令
        /// </summary>
        public object ExecuteCommand()
        {
            var isNoTrean = this.Context.Ado.Transaction == null;
            try
            {
                if (isNoTrean)
                    this.Context.Ado.BeginTran();

                var result = Execute();

                if (isNoTrean)
                    this.Context.Ado.CommitTran();
                return result;
            }
            catch (Exception)
            {
                if (isNoTrean)
                    this.Context.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 执行插入操作
        /// </summary>
        private int Execute()
        {
            if (InsertObjects?.Count > 0)
            {
                var isIdEntity = IsIdEntity(this.Entity);
                if (!isIdEntity)
                {
                    this.Context.Insertable(InsertObjects).ExecuteCommand();
                }
                foreach (var InsertObject in InsertObjects)
                {
                    int id = 0;
                    if (isIdEntity)
                    {
                        id = this.Context.InsertableT(InsertObject).ExecuteReturnIdentity();
                        this.Entity.Columns.First(it => it.IsIdentity || it.OracleSequenceName.HasValue()).PropertyInfo.SetValue(InsertObject, id);
                    }
                    var pk = GetPrimaryKey(this.Entity, InsertObject, id);
                    AddChildList(this.SubList, InsertObject, pk);
                }
                return InsertObjects.Count;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 检查是否是自增实体
        /// </summary>
        private bool IsIdEntity(EntityInfo entity)
        {
            return entity.Columns.Any(it => it.IsIdentity || it.OracleSequenceName.HasValue());
        }

        /// <summary>
        /// 添加子表列表数据
        /// </summary>
        private void AddChildList(List<SubInsertTreeExpression> items, object insertObject, object pkValue)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    MemberExpression subMemberException;
                    string subMemberName = GetMemberName(item, out subMemberException);
                    string childName = GetChildName(item, subMemberException);
                    var childListProperty = insertObject.GetType().GetProperty(childName);
                    if (childListProperty == null)
                    {
                        childName = subMemberName;
                        childListProperty = insertObject.GetType().GetProperty(childName);
                    }
                    var childList = childListProperty.GetValue(insertObject);
                    if (childList != null)
                    {
                        if (!(childList is IEnumerable<object>))
                        {
                            childList = new List<object>() { childList };
                        }
                        if (!string.IsNullOrEmpty(subMemberName) && subMemberName != childName)
                        {
                            foreach (var child in childList as IEnumerable<object>)
                            {
                                child.GetType().GetProperty(subMemberName).SetValue(child, pkValue);
                            }
                        }
                        if (!(childList as IEnumerable<object>).Any())
                        {
                            continue;
                        }
                        var type = (childList as IEnumerable<object>).First().GetType();
                        this.Context.InitMappingInfo(type);
                        var entityInfo = this.Context.EntityMaintenance.GetEntityInfo(type);
                        var isIdentity = IsIdEntity(entityInfo);
                        var tableName = entityInfo.DbTableName;
                        List<Dictionary<string, object>> insertList = new List<Dictionary<string, object>>();
                        var entityList = (childList as IEnumerable<object>).ToList();
                        foreach (var child in entityList)
                        {
                            insertList.Add(GetInsertDictionary(child, entityInfo));
                        }
                        if (!isIdentity)
                        {
                            this.Context.Insertable(insertList).AS(tableName).ExecuteCommand();
                        }
                        int i = 0;
                        foreach (var insert in insertList)
                        {
                            int id = 0;
                            if (isIdentity)
                            {
                                if (this.Context.CurrentConnectionConfig.DbType == DbType.PostgreSQL)
                                {
                                    var sqlobj = this.Context.InsertableT(insert).AS(tableName).ToSql();
                                    id = this.Context.Ado.GetInt(sqlobj.Key + " returning " + this.InsertBuilder.Builder.GetTranslationColumnName(entityInfo.Columns.First(it => isIdentity).DbColumnName), sqlobj.Value);
                                }
                                else
                                {
                                    id = this.Context.InsertableT(insert).AS(tableName).ExecuteReturnIdentity();
                                }
                                if (this.Context.CurrentConnectionConfig.DbType == DbType.Oracle && id == 0)
                                {
                                    var seqName = entityInfo.Columns.First(it => it.OracleSequenceName.HasValue())?.OracleSequenceName;
                                    id = this.Context.Ado.GetInt("select " + seqName + ".currval from dual");
                                }
                            }
                            var entity = entityList[i];
                            var pk = GetPrimaryKey(entityInfo, entity, id);
                            AddChildList(item.Childs, entity, pk);
                            ++i;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取插入字典
        /// </summary>
        private Dictionary<string, object> GetInsertDictionary(object insetObject, EntityInfo subEntity)
        {
            Dictionary<string, object> insertDictionary = new Dictionary<string, object>();
            foreach (var item in subEntity.Columns)
            {
                if (item.IsIdentity || item.IsIgnore)
                {
                }
                else if (!string.IsNullOrEmpty(item.OracleSequenceName) && this.Context.CurrentConnectionConfig.DbType == DbType.Oracle)
                {
                    var value = "{SugarSeq:=}" + item.OracleSequenceName + ".nextval{SugarSeq:=}";
                    insertDictionary.Add(item.DbColumnName, value);
                    continue;
                }
                else
                {
                    var value = item.PropertyInfo.GetValue(insetObject);
                    if (value == null && this.Context.CurrentConnectionConfig.DbType == DbType.PostgreSQL)
                    {
                        var underType = UtilMethods.GetUnderType(item.PropertyInfo.PropertyType);
                        if (underType == UtilConstants.DateType)
                        {
                            value = SqlDateTime.Null;
                        }
                    }
                    insertDictionary.Add(item.DbColumnName, value);
                }
            }
            return insertDictionary;
        }
        /// <summary>
        /// 获取子表名称
        /// </summary>
        private static string GetChildName(SubInsertTreeExpression item, MemberExpression subMemberException)
        {
            string childName;
            MemberExpression listMember = null;
            if (subMemberException.Expression is MethodCallExpression)
            {
                listMember = (subMemberException.Expression as MethodCallExpression).Arguments[0] as MemberExpression;
            }
            else
            {
                listMember = (subMemberException.Expression as MemberExpression);
            }
            if (listMember == null && item.Expression is LambdaExpression)
            {
                listMember = (item.Expression as LambdaExpression).Body as MemberExpression;
            }
            if (listMember == null && item.Expression is MemberExpression)
            {
                listMember = item.Expression as MemberExpression;
            }
            childName = listMember.Member.Name;
            return childName;
        }

        /// <summary>
        /// 获取成员名称
        /// </summary>
        private static string GetMemberName(SubInsertTreeExpression item, out MemberExpression subMemberException)
        {
            string subMemberName = null;
            Expression lambdaExpression;
            if (item.Expression is LambdaExpression)
            {
                lambdaExpression = (item.Expression as LambdaExpression).Body;
            }
            else
            {
                lambdaExpression = item.Expression;
            }
            if (lambdaExpression is UnaryExpression)
            {
                lambdaExpression = (lambdaExpression as UnaryExpression).Operand;
            }
            subMemberException = lambdaExpression as MemberExpression;
            subMemberName = subMemberException.Member.Name;
            return subMemberName;
        }

        /// <summary>
        /// 获取主键值
        /// </summary>
        private object GetPrimaryKey(EntityInfo entityInfo, object InsertObject, int id)
        {
            object pkValue;
            if (id.ObjToInt() == 0)
            {
                var primaryProperty = entityInfo.Columns.FirstOrDefault(it => it.IsPrimarykey);
                if (primaryProperty == null) { throw new SqlSugarException($"{entityInfo.EntityName} no primarykey"); }
                pkValue = primaryProperty.PropertyInfo.GetValue(InsertObject);
            }
            else
            {
                pkValue = id;
            }

            return pkValue;
        }
    }
}