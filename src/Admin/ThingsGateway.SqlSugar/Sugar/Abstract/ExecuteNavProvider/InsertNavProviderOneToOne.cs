namespace ThingsGateway.SqlSugar
{
    public partial class InsertNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 插入一对一关系数据
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="name">导航属性名称</param>
        /// <param name="nav">导航属性信息</param>
        private void InsertOneToOne<TChild>(string name, EntityColumnInfo nav) where TChild : class, new()
        {
            // 获取父实体信息和父实体列表
            var parentEntity = _ParentEntity;
            var parentList = _ParentList;

            // 获取父实体外键列和主键列
            var parentColumn = parentEntity.Columns.FirstOrDefault(it => it.PropertyName == nav.Navigat.Name);
            var parentPkColumn = parentEntity.Columns.FirstOrDefault(it => it.IsPrimarykey);

            // 获取子实体信息和主键列
            var thisEntity = this._Context.EntityMaintenance.GetEntityInfo<TChild>();
            EntityColumnInfo thisPkColumn = GetPkColumnByNav(thisEntity, nav);

            // 检查主键列是否存在
            Check.ExceptionEasy(thisPkColumn == null, $" Navigate {parentEntity.EntityName} : {name} is error ", $"导航实体 {parentEntity.EntityName} 属性 {name} 配置错误");

            // 检查是否配置了WhereSql(不支持插入)
            Check.ExceptionEasy(nav.Navigat.WhereSql.HasValue(), $" {name} Navigate(NavType,WhereSql) no support insert ", $"导航一对一 {name} 配置了 Sql变量 不支持插入");

            List<TChild> childList = new List<TChild>();

            // 遍历父实体列表
            foreach (var parent in parentList)
            {
                // 获取父实体外键值
                var navPropertyValue = parentColumn.PropertyInfo.GetValue(parent);

                // 获取子实体对象
                var childItem = (TChild)nav.PropertyInfo.GetValue(parent);

                if (childItem != null)
                {
                    // 处理外键值为默认值的情况
                    if (IsDefaultValue(navPropertyValue))
                    {
                        var pkValue = thisPkColumn.PropertyInfo.GetValue(childItem);
                        if (IsDefaultValue(navPropertyValue))
                        {
                            navPropertyValue = pkValue;
                        }
                    }

                    // 更新父表外键值(如果不是主键列)
                    if (!IsDefaultValue(navPropertyValue) && parentColumn.IsPrimarykey == false)
                    {
                        this._Context.Updateable<DbTableInfo>()
                           .AS(parentEntity.DbTableName)
                           .SetColumns(parentColumn.DbColumnName, navPropertyValue)
                           .Where(parentPkColumn.DbColumnName, "=", parentPkColumn.PropertyInfo.GetValue(parent))
                           .ExecuteCommand();
                    }

                    // 插入子实体并更新外键值
                    if (IsDefaultValue(navPropertyValue))
                    {
                        InsertDatas<TChild>(new List<TChild>() { childItem }, thisPkColumn);
                        navPropertyValue = thisPkColumn.PropertyInfo.GetValue(childItem);
                        parentColumn.PropertyInfo.SetValue(parent, navPropertyValue);
                        this._Context.Updateable<DbTableInfo>()
                            .AS(parentEntity.DbTableName)
                            .SetColumns(parentColumn.DbColumnName, navPropertyValue)
                            .Where(parentPkColumn.DbColumnName, "=", parentPkColumn.PropertyInfo.GetValue(parent))
                            .ExecuteCommand();
                    }

                    // 设置子实体主键值并添加到列表
                    thisPkColumn.PropertyInfo.SetValue(childItem, navPropertyValue);
                    childList.Add(childItem);
                }
            }

            // 插入子实体数据
            InsertDatas<TChild>(childList, thisPkColumn);

            // 更新当前处理的父实体列表为子实体列表
            this._ParentList = childList.Cast<object>().ToList();

            // 设置新的父实体信息
            SetNewParent<TChild>(thisEntity, thisPkColumn);
        }
    }
}
