namespace ThingsGateway.SqlSugar
{
    public partial class UpdateNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 是否已删除标记
        /// </summary>
        protected bool IsDeleted { get; set; }

        /// <summary>
        /// 更新一对一关系
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="name">导航属性名称</param>
        /// <param name="nav">导航列信息</param>
        private void UpdateOneToOne<TChild>(string name, EntityColumnInfo nav) where TChild : class, new()
        {
            var parentEntity = _ParentEntity;
            var parentList = _ParentList;
            var isManyPk = parentEntity.Columns.Count(it => it.IsPrimarykey) > 1;
            var parentColumn = parentEntity.Columns.FirstOrDefault(it => it.PropertyName == nav.Navigat.Name);
            var parentPkColumn = parentEntity.Columns.FirstOrDefault(it => it.IsPrimarykey);
            var thisEntity = this._Context.EntityMaintenance.GetEntityInfo<TChild>();
            IsDeleted = thisEntity.Columns.Any(it => it.PropertyName.EqualCase("isdeleted") || it.PropertyName.EqualCase("isdelete"));
            EntityColumnInfo thisPkColumn = GetPkColumnByNav(thisEntity, nav);
            Check.ExceptionEasy(thisPkColumn == null, $" Navigate {parentEntity.EntityName} : {name} is error ", $"导航实体 {parentEntity.EntityName} 属性 {name} 配置错误");
            Check.ExceptionEasy(nav.Navigat.WhereSql.HasValue(), $" {name} Navigate(NavType,WhereSql)  no support update ", $"导航一对一 {name} 配置了 Sql变量 不支持更新");
            List<TChild> childList = new List<TChild>();
            foreach (var parent in parentList)
            {
                var navPropertyValue = parentColumn.PropertyInfo.GetValue(parent);
                var childItem = (TChild)nav.PropertyInfo.GetValue(parent);
                if (childItem != null)
                {
                    if (IsDefaultValue(navPropertyValue))
                    {
                        var pkValue = thisPkColumn.PropertyInfo.GetValue(childItem);
                        if (IsDefaultValue(navPropertyValue))
                        {
                            if (parentColumn.IsPrimarykey == false && isManyPk == false && parentColumn.IsIdentity == false)
                            {
                                this._Context.Updateable<DbTableInfo>().AS(parentEntity.DbTableName)
                               .SetColumns(parentColumn.DbColumnName, pkValue)
                               .Where(parentPkColumn.DbColumnName, "=", parentPkColumn.PropertyInfo.GetValue(parent)).ExecuteCommand();
                            }
                            navPropertyValue = pkValue;
                        }
                    }
                    if (!IsDefaultValue(navPropertyValue) && isManyPk == false && parentPkColumn.IsIdentity == false)
                    {
                        this._Context.Updateable<DbTableInfo>
                           ().AS(parentEntity.DbTableName)
                           .SetColumns(parentColumn.DbColumnName, navPropertyValue)
                           .Where(parentPkColumn.DbColumnName, "=", parentPkColumn.PropertyInfo.GetValue(parent)).ExecuteCommand();
                    }
                    if (IsDefaultValue(navPropertyValue))
                    {
                        InsertDatas<TChild>(new List<TChild>() { childItem }, thisPkColumn);
                        navPropertyValue = thisPkColumn.PropertyInfo.GetValue(childItem);
                        parentColumn.PropertyInfo.SetValue(parent, navPropertyValue);
                        this._Context.Updateable<DbTableInfo>
                            ().AS(parentEntity.DbTableName)
                            .SetColumns(parentColumn.DbColumnName, navPropertyValue)
                            .Where(parentPkColumn.DbColumnName, "=", parentPkColumn.PropertyInfo.GetValue(parent)).ExecuteCommand();
                    }

                    thisPkColumn.PropertyInfo.SetValue(childItem, navPropertyValue);
                    childList.Add(childItem);
                }
            }
            InsertDatas<TChild>(childList, thisPkColumn);
            this._ParentList = childList.Cast<object>().ToList();
            SetNewParent<TChild>(thisEntity, thisPkColumn);
        }
    }
}