namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 删除导航数据提供者(用于处理一对多关系的删除操作)
    /// </summary>
    public partial class DeleteNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 删除一对多关系数据
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="name">导航属性名称</param>
        /// <param name="nav">导航属性信息</param>
        private void DeleteOneToMany<TChild>(string name, EntityColumnInfo nav) where TChild : class, new()
        {
            // 获取父实体信息和父实体列表
            var parentEntity = _ParentEntity;
            var prentList = _ParentList.Cast<T>().ToList();

            // 获取导航属性信息
            var parentNavigateProperty = parentEntity.Columns.FirstOrDefault(it => it.PropertyName == name);

            // 获取子实体信息、主键列和外键列
            var thisEntity = this._Context.EntityMaintenance.GetEntityInfo<TChild>();
            var thisPkColumn = GetPkColumnByNav(thisEntity, nav);
            var thisFkColumn = GetFKColumnByNav(thisEntity, nav);

            // 获取父实体主键列
            EntityColumnInfo parentPkColumn = GetParentPkColumn();

            // 检查是否有自定义导航主键列
            EntityColumnInfo parentNavColumn = GetParentPkNavColumn(nav);
            if (parentNavColumn != null)
            {
                parentPkColumn = parentNavColumn;
            }

            // 删除父表数据(如果尚未删除)
            if (!_IsDeletedParant)
                SetContext(() => this._Context.Deleteable(prentList)
                .EnableDiffLogEventIF(_RootOptions?.IsDiffLogEvent == true, _RootOptions?.DiffLogBizData)
                .ExecuteCommand());

            // 获取父表主键值列表
            var ids = _ParentList.Select(it => parentPkColumn.PropertyInfo.GetValue(it)).ToList();

            // 查询关联的子表数据
            var childList = GetChildList<TChild>().In(thisFkColumn.DbColumnName, ids).ToList();

            // 更新当前处理的实体列表为子表列表
            this._ParentList = childList.Cast<object>().ToList();
            this._ParentPkColumn = thisPkColumn;
            this._IsDeletedParant = true;

            // 删除子表数据
            SetContext(() => this._Context.Deleteable(childList).ExecuteCommand());
        }

        /// <summary>
        /// 获取子实体查询对象
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <returns>可查询对象</returns>
        private ISugarQueryable<TChild> GetChildList<TChild>() where TChild : class, new()
        {
            var queryable = this._Context.Queryable<TChild>();

            // 添加查询条件(如果有)
            if (_WhereList.HasValue())
            {
                foreach (var item in _WhereList)
                {
                    queryable.Where(item);
                }
                queryable.AddParameters(_Parameters);
            }
            return queryable;
        }

        /// <summary>
        /// 设置删除任务上下文
        /// </summary>
        /// <param name="action">删除操作</param>
        private void SetContext(Action action)
        {
            var key = "_DeleteNavTask";

            // 初始化临时存储字典
            if (this._Context.TempItems == null)
            {
                this._Context.TempItems = new Dictionary<string, object>();
            }

            // 获取或创建任务列表
            if (!this._Context.TempItems.TryGetValue(key, out object? oldTask))
            {
                oldTask = null;
                this._Context.TempItems.Add(key, oldTask);
            }

            var newTask = new List<Action>();
            if (oldTask != null)
            {
                newTask = (List<Action>)oldTask;
            }

            // 添加新任务
            newTask.Add(action);
            this._Context.TempItems[key] = newTask;
        }

        /// <summary>
        /// 获取父实体主键列
        /// </summary>
        /// <returns>主键列信息</returns>
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
        /// 获取父实体导航主键列
        /// </summary>
        /// <param name="nav">导航属性信息</param>
        /// <returns>主键列信息</returns>
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
        /// 根据导航属性获取主键列
        /// </summary>
        /// <param name="entity">实体信息</param>
        /// <param name="nav">导航属性信息</param>
        /// <returns>主键列信息</returns>
        private EntityColumnInfo GetPkColumnByNav(EntityInfo entity, EntityColumnInfo nav)
        {
            var pkColumn = entity.Columns.FirstOrDefault(it => it.IsPrimarykey == true);
            if (nav.Navigat.Name2.HasValue())
            {
                pkColumn = entity.Columns.FirstOrDefault(it => it.PropertyName == nav.Navigat.Name2);
            }
            return pkColumn;
        }

        /// <summary>
        /// 根据导航属性获取外键列
        /// </summary>
        /// <param name="entity">实体信息</param>
        /// <param name="nav">导航属性信息</param>
        /// <returns>外键列信息</returns>
        private EntityColumnInfo GetFKColumnByNav(EntityInfo entity, EntityColumnInfo nav)
        {
            var fkColumn = entity.Columns.FirstOrDefault(it => it.PropertyName == nav.Navigat.Name);
            return fkColumn;
        }
    }
}
