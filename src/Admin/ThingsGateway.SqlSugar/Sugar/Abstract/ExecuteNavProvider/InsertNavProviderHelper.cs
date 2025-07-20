namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 插入导航数据提供者(用于处理级联插入操作)
    /// </summary>
    /// <typeparam name="Root">根实体类型</typeparam>
    /// <typeparam name="T">当前实体类型</typeparam>
    public partial class InsertNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 检查是否是默认值
        /// </summary>
        /// <param name="pvValue">要检查的值</param>
        /// <returns>是否是默认值</returns>
        private static bool IsDefaultValue(object pvValue)
        {
            return pvValue?.Equals(UtilMethods.GetDefaultValue(pvValue.GetType())) != false;
        }

        /// <summary>
        /// 初始化父实体列表
        /// </summary>
        private void InitParentList()
        {
            if (_RootList == null)
            {
                _RootList = _ParentList = GetRootList(_Roots).Cast<object>().ToList();
            }
            else if (_ParentList == null)
            {
                _ParentList = _RootList;
                var pkColumn = this._Context.EntityMaintenance.GetEntityInfo<T>().Columns.FirstOrDefault(it => it.IsPrimarykey);
                this._ParentPkColumn = pkColumn;
            }
            IsFirst = false;
        }

        /// <summary>
        /// 获取处理结果
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <returns>插入导航提供者实例</returns>
        private InsertNavProvider<Root, TChild> GetResult<TChild>() where TChild : class, new()
        {
            return new InsertNavProvider<Root, TChild>()
            {
                _Context = this._Context,
                _ParentEntity = this._ParentEntity,
                _ParentList = this._ParentList,
                _Roots = this._Roots,
                _ParentPkColumn = this._ParentPkColumn,
                _RootList = this._RootList
            };
        }

        /// <summary>
        /// 获取根实体列表
        /// </summary>
        /// <typeparam name="Type">实体类型</typeparam>
        /// <param name="datas">实体数据列表</param>
        /// <returns>处理后的实体列表</returns>
        private List<Type> GetRootList<Type>(List<Type> datas) where Type : class, new()
        {
            List<Type> result = new List<Type>();
            this._Context.InitMappingInfo<Type>();
            var entity = this._Context.EntityMaintenance.GetEntityInfo<Type>();
            var pkColumn = entity.Columns.FirstOrDefault(it => it.IsPrimarykey);
            InsertDatas(datas, pkColumn);
            this._ParentEntity = entity;
            result = datas;
            return result;
        }

        /// <summary>
        /// 插入数据并获取自增ID
        /// </summary>
        /// <typeparam name="Type">实体类型</typeparam>
        /// <param name="datas">要插入的数据列表</param>
        private void InsertIdentity<Type>(List<Type> datas) where Type : class, new()
        {
            foreach (var item in datas)
            {
                if (IsFirst && _RootOptions != null)
                {
                    this._Context.InsertableT(item)
                        .IgnoreColumns(_RootOptions.IgnoreColumns)
                        .InsertColumns(_RootOptions.InsertColumns)
                        .EnableDiffLogEventIF(_RootOptions.IsDiffLogEvent, _RootOptions.DiffLogBizData)
                        .ExecuteCommandIdentityIntoEntity();
                }
                else
                {
                    this._Context.InsertableT(item).ExecuteCommandIdentityIntoEntity();
                }
            }
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
        /// 获取主键列(不带导航配置)
        /// </summary>
        /// <param name="entity">实体信息</param>
        /// <param name="nav">导航属性信息</param>
        /// <returns>主键列信息</returns>
        private EntityColumnInfo GetPkColumnByNav2(EntityInfo entity, EntityColumnInfo nav)
        {
            var pkColumn = entity.Columns.FirstOrDefault(it => it.IsPrimarykey == true);
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

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="children">子实体列表</param>
        /// <param name="pkColumn">主键列信息</param>
        /// <param name="NavColumn">导航列信息</param>
        private void InsertDatas<TChild>(List<TChild> children, EntityColumnInfo pkColumn, EntityColumnInfo NavColumn = null) where TChild : class, new()
        {
            children = children.Distinct().ToList();
            if (pkColumn == null)
            {
                Check.ExceptionEasy($"{typeof(TChild).Name} need primary key ", $"{typeof(TChild).Name}需要主键");
            }
            var x = this._Context.Storageable(children).WhereColumns(new string[] { pkColumn.PropertyName }).GetStorageableResult();
            var insertData = children = x.InsertList.Select(it => it.Item).ToList();
            var IsNoExistsNoInsert = _navOptions?.OneToManyIfExistsNoInsert == true;
            if (_NavigateType == NavigateType.OneToMany && IsFirst == false && IsNoExistsNoInsert == false)
            {
                var updateData = x.UpdateList.Select(it => it.Item).ToList();
                ClearPk(updateData, pkColumn);
                insertData.AddRange(updateData);
            }
            else if (_NavigateType == NavigateType.OneToMany && IsNoExistsNoInsert == true)
            {
                children = new List<TChild>();
                children.AddRange(x.InsertList.Select(it => it.Item).ToList());
                var updateData = x.UpdateList.Select(it => it.Item).ToList();
                children.AddRange(updateData);
            }
            Check.ExceptionEasy(pkColumn == null && NavColumn == null, $"The entity is invalid", $"实体错误无法使用导航");
            InitData(pkColumn, insertData);
            this._ParentList = children.Cast<object>().ToList();
        }

        /// <summary>
        /// 清空主键值
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="updateData">要更新的数据列表</param>
        /// <param name="pkColumn">主键列信息</param>
        private void ClearPk<TChild>(List<TChild> updateData, EntityColumnInfo pkColumn) where TChild : class, new()
        {
            foreach (var child in updateData)
            {
                var defaultValue = UtilMethods.DefaultForType(pkColumn.PropertyInfo.PropertyType);
                pkColumn.PropertyInfo.SetValue(child, defaultValue);
            }
        }

        /// <summary>
        /// 初始化数据(处理主键值)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="pkColumn">主键列信息</param>
        /// <param name="insertData">要插入的数据列表</param>
        private void InitData<TChild>(EntityColumnInfo pkColumn, List<TChild> insertData) where TChild : class, new()
        {
            if (pkColumn.IsIdentity || pkColumn.OracleSequenceName.HasValue())
            {
                InsertIdentity(insertData);
            }
            else if (pkColumn.UnderType == UtilConstants.LongType)
            {
                SetValue(pkColumn, insertData, () => SnowFlakeSingle.Instance.NextId());
            }
            else if (pkColumn.UnderType == UtilConstants.GuidType)
            {
                SetValue(pkColumn, insertData, () => Guid.NewGuid());
            }
            else if (pkColumn.UnderType == UtilConstants.StringType)
            {
                SetValue(pkColumn, insertData, () => Guid.NewGuid().ToString());
            }
            else
            {
                SetError(pkColumn, insertData);
            }
        }

        /// <summary>
        /// 设置主键值并插入数据
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="pkColumn">主键列信息</param>
        /// <param name="insertData">要插入的数据列表</param>
        /// <param name="value">主键值生成函数</param>
        private void SetValue<TChild>(EntityColumnInfo pkColumn, List<TChild> insertData, Func<object> value) where TChild : class, new()
        {
            foreach (var child in insertData)
            {
                if (IsDefaultValue(pkColumn.PropertyInfo.GetValue(child)))
                {
                    pkColumn.PropertyInfo.SetValue(child, value());
                }
            }
            if (IsFirst && _RootOptions != null)
            {
                this._Context.Insertable(insertData)
                    .IgnoreColumns(_RootOptions.IgnoreColumns)
                    .InsertColumns(_RootOptions.InsertColumns)
                    .EnableDiffLogEventIF(_RootOptions.IsDiffLogEvent, _RootOptions.DiffLogBizData)
                    .ExecuteCommand();
            }
            else
            {
                this._Context.Insertable(insertData).ExecuteCommand();
            }
        }

        /// <summary>
        /// 处理不支持自动生成主键的情况
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="pkColumn">主键列信息</param>
        /// <param name="insertData">要插入的数据列表</param>
        private void SetError<TChild>(EntityColumnInfo pkColumn, List<TChild> insertData) where TChild : class, new()
        {
            foreach (var child in insertData)
            {
                if (IsDefaultValue(pkColumn.PropertyInfo.GetValue(child)))
                {
                    var name = pkColumn.EntityName + " " + pkColumn.DbColumnName;
                    Check.ExceptionEasy($"The field {name} is not an autoassignment type and requires an assignment", $"字段{name}不是可自动赋值类型需要赋值（并且不能是已存在值） , 可赋值类型有 自增、long、Guid、string");
                }
            }
            if (IsFirst && _RootOptions != null)
            {
                this._Context.Insertable(insertData)
                    .IgnoreColumns(_RootOptions.IgnoreColumns)
                    .InsertColumns(_RootOptions.InsertColumns)
                    .EnableDiffLogEventIF(_RootOptions.IsDiffLogEvent, _RootOptions.DiffLogBizData)
                    .ExecuteCommand();
            }
            else
            {
                var isIdentity = this._Context.EntityMaintenance.GetEntityInfo(typeof(TChild)).Columns.Any(it => it.IsIdentity);
                if (isIdentity)
                {
                    foreach (var item in insertData)
                    {
                        this._Context.Insertable(insertData).ExecuteCommandIdentityIntoEntity();
                    }
                }
                else
                {
                    this._Context.Insertable(insertData).ExecuteCommand();
                }
            }
        }
    }
}
