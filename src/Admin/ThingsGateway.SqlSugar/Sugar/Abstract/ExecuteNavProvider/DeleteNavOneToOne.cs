namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 删除导航数据提供者(用于处理一对一关系的删除操作)
    /// </summary>
    public partial class DeleteNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 删除一对一关系数据
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="name">导航属性名称</param>
        /// <param name="nav">导航属性信息</param>
        private void DeleteOneToOne<TChild>(string name, EntityColumnInfo nav) where TChild : class, new()
        {
            // 获取父实体信息和父实体列表
            var parentEntity = _ParentEntity;
            var parentList = _ParentList.Cast<T>().ToList();

            // 获取父实体导航列和主键列
            var parentColumn = parentEntity.Columns.FirstOrDefault(it => it.PropertyName == nav.Navigat.Name);
            var parentPkColumn = parentEntity.Columns.FirstOrDefault(it => it.IsPrimarykey);

            // 获取子实体信息和主键列
            var thisEntity = this._Context.EntityMaintenance.GetEntityInfo<TChild>();
            EntityColumnInfo thisPkColumn = GetPkColumnByNav(thisEntity, nav);

            // 检查主键列是否存在
            Check.Exception(thisPkColumn == null, $" Navigate {parentEntity.EntityName} : {name} is error ", $"导航实体 {parentEntity.EntityName} 属性 {name} 配置错误");

            // 删除父表数据(如果尚未删除)
            if (!_IsDeletedParant)
                SetContext(() => this._Context.Deleteable(parentList)
                .EnableDiffLogEventIF(_RootOptions?.IsDiffLogEvent == true, _RootOptions?.DiffLogBizData)
                .ExecuteCommand());

            // 检查导航列是否存在
            Check.ExceptionEasy(parentColumn == null, "The one-to-one navigation configuration is incorrect", "一对一导航配置错误");

            // 获取父表导航列值列表
            var ids = _ParentList.Select(it => parentColumn.PropertyInfo.GetValue(it)).ToList();

            // 查询关联的子表数据
            List<TChild> childList = this._Context.Queryable<TChild>().In(thisPkColumn.DbColumnName, ids).ToList();

            // 更新当前处理的实体列表为子表列表
            this._ParentList = childList.Cast<object>().ToList();
            this._ParentPkColumn = thisPkColumn;
            this._IsDeletedParant = true;

            // 删除子表数据
            SetContext(() => this._Context.Deleteable(childList).ExecuteCommand());
        }
    }
}
