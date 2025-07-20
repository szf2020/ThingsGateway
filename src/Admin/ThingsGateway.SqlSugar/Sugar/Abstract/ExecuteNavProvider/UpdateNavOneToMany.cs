using System.Collections;
namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 更新导航属性提供者
    /// </summary>
    /// <typeparam name="Root">根实体类型</typeparam>
    /// <typeparam name="T">当前实体类型</typeparam>
    public partial class UpdateNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 导航类型
        /// </summary>
        public NavigateType? _NavigateType { get; set; }

        /// <summary>
        /// 更新一对多关系
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="name">导航属性名称</param>
        /// <param name="nav">导航列信息</param>
        private void UpdateOneToMany<TChild>(string name, EntityColumnInfo nav) where TChild : class, new()
        {
            if (_Options?.OneToManyInsertOrUpdate == true)
            {
                InsertOrUpdate<TChild>(name, nav);
            }
            else
            {
                DeleteInsert<TChild>(name, nav);
            }
        }

        /// <summary>
        /// 插入或更新子实体
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="name">导航属性名称</param>
        /// <param name="nav">导航列信息</param>
        private void InsertOrUpdate<TChild>(string name, EntityColumnInfo nav) where TChild : class, new()
        {
            List<TChild> children = new List<TChild>();
            var parentEntity = _ParentEntity;
            var parentList = _ParentList;
            var parentNavigateProperty = parentEntity.Columns.FirstOrDefault(it => it.PropertyName == name);
            var thisEntity = this._Context.EntityMaintenance.GetEntityInfo<TChild>();
            var thisPkColumn = GetPkColumnByNav2(thisEntity, nav);
            var thisFkColumn = GetFKColumnByNav(thisEntity, nav);
            EntityColumnInfo parentPkColumn = GetParentPkColumn();
            EntityColumnInfo parentNavColumn = GetParentPkNavColumn(nav);
            if (parentNavColumn != null)
            {
                parentPkColumn = parentNavColumn;
            }
            if (ParentIsPk(parentNavigateProperty))
            {
                parentPkColumn = this._ParentEntity.Columns.FirstOrDefault(it => it.IsPrimarykey);
            }
            var ids = new HashSet<object>();
            foreach (var item in parentList)
            {
                var parentValue = parentPkColumn.PropertyInfo.GetValue(item);
                var childs = parentNavigateProperty.PropertyInfo.GetValue(item) as List<TChild>;
                if (childs != null)
                {
                    foreach (var child in childs)
                    {
                        thisFkColumn.PropertyInfo.SetValue(child, parentValue, null);
                    }
                    children.AddRange(childs);
                }
                ids.Add(parentValue);
                if (_Options?.OneToManyNoDeleteNull == true && childs == null)
                {
                    ids.Remove(parentValue);
                }
            }
            if (NotAny(name))
            {
                DeleteMany(thisEntity, ids.ToList(), thisFkColumn.DbColumnName);
                if (this._Options?.OneToManyEnableLogicDelete == true)
                {
                    var locgicColumn = thisEntity.Columns.FirstOrDefault(it => it.PropertyName.EqualCase("IsDeleted") || it.PropertyName.EqualCase("IsDelete"));
                    Check.ExceptionEasy(
                         locgicColumn == null,
                         thisEntity.EntityName + "Logical deletion requires the entity to have the IsDeleted property",
                         thisEntity.EntityName + "假删除需要实体有IsDeleted属性");
                    List<IConditionalModel> conditionalModels = new List<IConditionalModel>();
                    conditionalModels.Add(new ConditionalModel()
                    {
                        FieldName = thisFkColumn.DbColumnName,
                        FieldValue = string.Join(",", ids.Distinct()),
                        ConditionalType = ConditionalType.In,
                        CSharpTypeName = thisFkColumn?.PropertyInfo?.PropertyType?.Name
                    });
                    var sqlObj = _Context.Queryable<object>().SqlBuilder.ConditionalModelToSql(conditionalModels);
                    this._Context.Updateable<object>()
                      .AS(thisEntity.DbTableName)
                      .Where(sqlObj.Key, sqlObj.Value)
                      .SetColumns(locgicColumn.DbColumnName, true)
                      .ExecuteCommand();
                }
                else
                {
                    var list = this._Context.Queryable<TChild>()
                        .AS(thisEntity.DbTableName)
                        .In(thisFkColumn.DbColumnName, ids.ToList())
                        .ToList();
                    List<TChild> result = GetNoExistsId(list, children, thisPkColumn.PropertyName);
                    if (result.Count != 0)
                    {
                        this._Context.Deleteable(result).ExecuteCommand();
                    }
                }
                _NavigateType = NavigateType.OneToMany;
                InsertDatas(children, thisPkColumn);
            }
            else
            {
                this._ParentList = children.Cast<object>().ToList();
            }
            _NavigateType = null;
            SetNewParent<TChild>(thisEntity, thisPkColumn);
        }

        /// <summary>
        /// 删除并插入子实体
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="name">导航属性名称</param>
        /// <param name="nav">导航列信息</param>
        private void DeleteInsert<TChild>(string name, EntityColumnInfo nav) where TChild : class, new()
        {
            List<TChild> children = new List<TChild>();
            var parentEntity = _ParentEntity;
            var parentList = _ParentList;
            var parentNavigateProperty = parentEntity.Columns.FirstOrDefault(it => it.PropertyName == name);
            var thisEntity = this._Context.EntityMaintenance.GetEntityInfo<TChild>();
            var thisPkColumn = GetPkColumnByNav2(thisEntity, nav);
            var thisFkColumn = GetFKColumnByNav(thisEntity, nav);
            EntityColumnInfo parentPkColumn = GetParentPkColumn();
            EntityColumnInfo parentNavColumn = GetParentPkNavColumn(nav);
            if (parentNavColumn != null)
            {
                parentPkColumn = parentNavColumn;
            }
            if (ParentIsPk(parentNavigateProperty))
            {
                parentPkColumn = this._ParentEntity.Columns.FirstOrDefault(it => it.IsPrimarykey);
            }
            var ids = new HashSet<object>();
            foreach (var item in parentList)
            {
                var parentValue = parentPkColumn.PropertyInfo.GetValue(item);
                var childs = parentNavigateProperty.PropertyInfo.GetValue(item) as List<TChild>;
                if (childs != null)
                {
                    foreach (var child in childs)
                    {
                        thisFkColumn.PropertyInfo.SetValue(child, parentValue, null);
                    }
                    children.AddRange(childs);
                }
                else if (childs == null && parentNavigateProperty.PropertyInfo.GetValue(item) is IList ilist && ilist?.Count > 0)
                {
                    childs = GetIChildsBylList(children, thisFkColumn, parentValue, ilist);
                }
                ids.Add(parentValue);
                if (_Options?.OneToManyNoDeleteNull == true && childs == null)
                {
                    ids.Remove(parentValue);
                }
            }
            if (NotAny(name))
            {
                DeleteMany(thisEntity, ids.ToList(), thisFkColumn.DbColumnName);
                if (this._Options?.OneToManyEnableLogicDelete == true)
                {
                    var locgicColumn = thisEntity.Columns.FirstOrDefault(it => it.PropertyName.EqualCase("IsDeleted") || it.PropertyName.EqualCase("IsDelete"));
                    Check.ExceptionEasy(
                         locgicColumn == null,
                         thisEntity.EntityName + "Logical deletion requires the entity to have the IsDeleted property",
                         thisEntity.EntityName + "假删除需要实体有IsDeleted属性");
                    List<IConditionalModel> conditionalModels = new List<IConditionalModel>();
                    conditionalModels.Add(new ConditionalModel()
                    {
                        FieldName = thisFkColumn.DbColumnName,
                        FieldValue = string.Join(",", ids.Distinct()),
                        ConditionalType = ConditionalType.In,
                        CSharpTypeName = thisFkColumn?.PropertyInfo?.PropertyType?.Name
                    });
                    var sqlObj = _Context.Queryable<object>().SqlBuilder.ConditionalModelToSql(conditionalModels);
                    this._Context.Updateable<object>()
                      .AS(thisEntity.DbTableName)
                      .Where(sqlObj.Key, sqlObj.Value)
                      .SetColumns(locgicColumn.DbColumnName, true)
                      .ExecuteCommand();
                }
                else
                {
                    if (this._Context?.CurrentConnectionConfig?.MoreSettings?.IsAutoDeleteQueryFilter == true)
                    {
                        this._Context.Deleteable<object>()
                           .AS(thisEntity.DbTableName)
                           .EnableQueryFilter(thisEntity.Type)
                           .In(thisFkColumn.DbColumnName, ids.ToList()).ExecuteCommand();
                    }
                    else
                    {
                        this._Context.Deleteable<object>()
                            .AS(thisEntity.DbTableName)
                            .In(thisFkColumn.DbColumnName, ids.ToList()).ExecuteCommand();
                    }
                }
                _NavigateType = NavigateType.OneToMany;
                InsertDatas(children, thisPkColumn);
            }
            else
            {
                this._ParentList = children.Cast<object>().ToList();
            }
            _NavigateType = null;
            SetNewParent<TChild>(thisEntity, thisPkColumn);
        }

        /// <summary>
        /// 从IList获取子实体列表
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="children">子实体列表</param>
        /// <param name="thisFkColumn">外键列信息</param>
        /// <param name="parentValue">父实体值</param>
        /// <param name="ilist">IList集合</param>
        /// <returns>子实体列表</returns>
        private static List<TChild> GetIChildsBylList<TChild>(List<TChild> children, EntityColumnInfo thisFkColumn, object parentValue, IList ilist) where TChild : class, new()
        {
            List<TChild> childs = ilist.Cast<TChild>().ToList();
            foreach (var child in childs)
            {
                thisFkColumn.PropertyInfo.SetValue(child, parentValue, null);
            }
            children.AddRange(childs);
            return childs;
        }

        /// <summary>
        /// 检查父导航属性是否为主键
        /// </summary>
        /// <param name="parentNavigateProperty">父导航属性</param>
        /// <returns>是否为主键</returns>
        private static bool ParentIsPk(EntityColumnInfo parentNavigateProperty)
        {
            return parentNavigateProperty?.Navigat != null &&
                   parentNavigateProperty.Navigat.NavigatType == NavigateType.OneToMany &&
                   parentNavigateProperty.Navigat.Name2 == null;
        }

        /// <summary>
        /// 删除多个子实体
        /// </summary>
        /// <param name="thisEntity">当前实体信息</param>
        /// <param name="ids">ID列表</param>
        /// <param name="fkName">外键名称</param>
        private void DeleteMany(EntityInfo thisEntity, List<object> ids, string fkName)
        {
            if (_Options == null || _Options.OneToManyDeleteAll == false)
            {
                return;
            }
            var oneToManys = thisEntity.Columns.Where(it => it.Navigat != null && it.Navigat.NavigatType == NavigateType.OneToMany);
            foreach (var oneToMany in oneToManys)
            {
                var fkFieldName = oneToMany.Navigat.Name2 ?? thisEntity.Columns.FirstOrDefault(it => it.IsPrimarykey).PropertyName;
                var fkDbColumnName = thisEntity.Columns.FirstOrDefault(it => it.PropertyName == fkFieldName).DbColumnName;
                var fks = this._Context.Queryable<object>()
                .AS(thisEntity.DbTableName)
                .In<object>(fkName, ids).Select(fkDbColumnName).ToDataTable().Rows.Cast<System.Data.DataRow>().Select(x => x[0]).Distinct().ToArray();

                var type = oneToMany.PropertyInfo.PropertyType.GenericTypeArguments[0];
                var entity = this._Context.EntityMaintenance.GetEntityInfo(type);
                var id = oneToMany.Navigat.Name;
                var column = entity.Columns.FirstOrDefault(it => it.PropertyName == id).DbColumnName;

                DeleteChild(fks, entity, column);

                this._Context.Deleteable<object>()
                                                .AS(entity.DbTableName)
                                                .In(column, fks).ExecuteCommand();
            }
        }

        /// <summary>
        /// 删除子实体
        /// </summary>
        /// <param name="fks">外键值数组</param>
        /// <param name="entity">实体信息</param>
        /// <param name="column">列名</param>
        private void DeleteChild(object[] fks, EntityInfo entity, string column)
        {
            var childs = entity.Columns.Where(it => it.Navigat != null && it.Navigat?.NavigatType == NavigateType.OneToMany).ToList();
            if (childs.Count != 0)
            {
                var pkColumn = entity.Columns.First(it => it.IsPrimarykey);
                var pkIds = this._Context.Queryable<object>()
                                         .AS(entity.DbTableName)
                                         .In(column, fks)
                                         .Select(pkColumn.DbColumnName).ToDataTable().Rows
                                         .Cast<System.Data.DataRow>().Select(it => it[0]).ToList();
                DeleteChildChild(pkIds, childs);
            }
        }

        /// <summary>
        /// 子实体索引
        /// </summary>
        int childIndex = 0;

        /// <summary>
        /// 递归删除子实体
        /// </summary>
        /// <param name="ids">ID列表</param>
        /// <param name="childs">子实体列信息列表</param>
        private void DeleteChildChild(List<object> ids, List<EntityColumnInfo> childs)
        {
            childIndex++;
            if (childIndex > 4)
            {
                Check.ExceptionEasy("Removing too many levels", "安全机制限制删除脏数据层级不能超过7层");
            }
            foreach (var columnInfo in childs)
            {
                var navigat = columnInfo.Navigat;
                var type = columnInfo.PropertyInfo.PropertyType.GenericTypeArguments[0];
                var thisEntity = this._Context.EntityMaintenance.GetEntityInfo(type);
                var fkColumn = thisEntity.Columns.FirstOrDefault(it => navigat.Name.EqualCase(it.PropertyName));
                var thisPkColumn = thisEntity.Columns.FirstOrDefault(it => it.IsPrimarykey);
                var childs2 = thisEntity.Columns.Where(it => it.Navigat != null && it.Navigat?.NavigatType == NavigateType.OneToMany).ToList();
                if (childs2.Count != 0)
                {
                    var pkIds = _Context.Queryable<object>().AS(thisEntity.DbTableName)
                                         .In(fkColumn.DbColumnName, ids)
                                         .Select(thisPkColumn.DbColumnName).ToDataTable().Rows
                                        .Cast<System.Data.DataRow>().Select(it => it[0]).ToList();

                    DeleteChildChild(pkIds, childs2);
                }
                _Context.Deleteable<object>().AS(thisEntity.DbTableName).In(fkColumn.DbColumnName, ids).ExecuteCommand();
            }
        }

        /// <summary>
        /// 获取父主键列
        /// </summary>
        /// <returns>父主键列信息</returns>
        private EntityColumnInfo GetParentPkColumn()
        {
            EntityColumnInfo parentPkColumn = _ParentPkColumn;
            if (_ParentPkColumn == null)
            {
                parentPkColumn = _ParentPkColumn = this._ParentEntity.Columns.FirstOrDefault(it => it.IsPrimarykey);
            }
            return parentPkColumn;
        }

        /// <summary>
        /// 获取父主键导航列
        /// </summary>
        /// <param name="nav">导航列信息</param>
        /// <returns>父主键导航列信息</returns>
        private EntityColumnInfo GetParentPkNavColumn(EntityColumnInfo nav)
        {
            EntityColumnInfo result = null;
            if (nav.Navigat.Name2.HasValue())
            {
                result = _ParentPkColumn = this._ParentEntity.Columns.FirstOrDefault(it => it.PropertyName == nav.Navigat.Name2);
            }
            return result;
        }

        /// <summary>
        /// 设置新的父实体
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="entityInfo">实体信息</param>
        /// <param name="entityColumnInfo">实体列信息</param>
        private void SetNewParent<TChild>(EntityInfo entityInfo, EntityColumnInfo entityColumnInfo) where TChild : class, new()
        {
            this._ParentEntity = entityInfo;
            this._ParentPkColumn = entityColumnInfo;
        }

        /// <summary>
        /// 获取不存在的ID列表
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="old">旧实体列表</param>
        /// <param name="newList">新实体列表</param>
        /// <param name="pkName">主键名称</param>
        /// <returns>不存在的实体列表</returns>
        public List<TChild> GetNoExistsId<TChild>(List<TChild> old, List<TChild> newList, string pkName)
        {
            List<TChild> result = new List<TChild>();

            // 将newList中的主键属性转换为字符串集合
            var newIds = newList.Select(item => GetPropertyValueAsString(item, pkName)).ToHashSet();

            // 获取在old中但不在newList中的主键属性值
            result = old.Where(item => !newIds.Contains(GetPropertyValueAsString(item, pkName)))
                        .ToList();

            return result;
        }

        /// <summary>
        /// 获取对象的属性值字符串
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="item">实体对象</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>属性值字符串</returns>
        private string GetPropertyValueAsString<TChild>(TChild item, string propertyName)
        {
            var property = item.GetType().GetProperty(propertyName);
            if (property != null)
            {
                return property.GetValue(item, null) + "";
            }
            else
            {
                throw new ArgumentException($"Property '{propertyName}' not found on type {item.GetType().Name}");
            }
        }
    }
}