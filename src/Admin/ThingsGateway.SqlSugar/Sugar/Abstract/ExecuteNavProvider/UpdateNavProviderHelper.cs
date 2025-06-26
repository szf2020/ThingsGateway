using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    public partial class UpdateNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 检查是否为默认值
        /// </summary>
        /// <param name="pvValue">要检查的值</param>
        /// <returns>是否为默认值</returns>
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
                _RootList = _ParentList = _Roots.Cast<object>().ToList();
                _ParentEntity = this._Context.EntityMaintenance.GetEntityInfo<Root>();
            }
            else if (_ParentList == null)
            {
                _ParentList = _RootList;
                var pkColumn = this._Context.EntityMaintenance.GetEntityInfo<T>().Columns.FirstOrDefault(it => it.IsPrimarykey);
                this._ParentPkColumn = pkColumn;
            }
        }

        /// <summary>
        /// 获取结果提供者
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <returns>更新导航提供者</returns>
        private UpdateNavProvider<Root, TChild> GetResult<TChild>() where TChild : class, new()
        {
            return new UpdateNavProvider<Root, TChild>()
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
        /// 插入带自增主键的实体
        /// </summary>
        /// <typeparam name="Type">实体类型</typeparam>
        /// <param name="datas">实体列表</param>
        private void InsertIdentity<Type>(List<Type> datas) where Type : class, new()
        {
            foreach (var item in datas)
            {
                this._Context.Insertable(item).ExecuteCommandIdentityIntoEntity();
            }
        }

        /// <summary>
        /// 根据导航属性获取主键列
        /// </summary>
        /// <param name="entity">实体信息</param>
        /// <param name="nav">导航列信息</param>
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
        /// 根据导航属性获取主键列(简化版)
        /// </summary>
        /// <param name="entity">实体信息</param>
        /// <param name="nav">导航列信息</param>
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
        /// <param name="nav">导航列信息</param>
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
            Check.ExceptionEasy(pkColumn == null, typeof(TChild).Name + " has no primary key", typeof(TChild).Name + "没有主键");
            var whereName = pkColumn.PropertyName;
            if (_Options?.OneToOneSaveByPrimaryKey == true && pkColumn.IsPrimarykey == false)
            {
                var newPkColumn = this._Context.EntityMaintenance.GetEntityInfo<TChild>().Columns.FirstOrDefault(it => it.IsPrimarykey);
                if (newPkColumn != null)
                {
                    whereName = newPkColumn.PropertyName;
                }
            }
            var x = this._Context.Storageable(children).WhereColumns(new string[] { whereName }).ToStorage();
            var insertData = x.InsertList.Select(it => it.Item).ToList();
            var updateData = x.UpdateList.Select(it => it.Item).ToList();
            Check.ExceptionEasy(pkColumn == null && NavColumn == null, $"The entity is invalid", $"实体错误无法使用导航");
            if (_Options?.CurrentFunc != null)
            {
                var updateable = x.AsUpdateable;
                var exp = _Options.CurrentFunc as Expression<Action<IUpdateable<TChild>>>;
                Check.ExceptionEasy(exp == null, "UpdateOptions.CurrentFunc is error", "UpdateOptions.CurrentFunc参数设置错误");
                var com = exp.Compile();
                com(updateable);
                if (IsDeleted)
                {
                    updateable.PageSize(1).EnableQueryFilter().ExecuteCommand();
                }
                else
                {
                    updateable.ExecuteCommand();
                }
            }
            else if (pkColumn.IsPrimarykey == false)
            {
                var pk = this._Context.EntityMaintenance.GetEntityInfo<TChild>().Columns.Where(it => it.IsPrimarykey);
                List<string> ignoreColumns = new List<string>();
                if (_Options?.IgnoreColumns != null)
                {
                    ignoreColumns.AddRange(_Options.IgnoreColumns);
                }
                if (pk.Any())
                {
                    ignoreColumns.AddRange(pk.Select(it => it.PropertyName));
                }
                if (_Options?.OneToOneSaveByPrimaryKey == true)
                {
                    ignoreColumns = ignoreColumns.Where(it => it != whereName).ToList();
                }
                if (IsDeleted)
                {
                    x.AsUpdateable.IgnoreColumns(ignoreColumns.ToArray()).PageSize(1).EnableQueryFilter().ExecuteCommand();
                }
                else
                {
                    x.AsUpdateable.IgnoreColumns(ignoreColumns.ToArray()).ExecuteCommand();
                }
            }
            else
            {
                var ignoreColumns = _Options?.IgnoreColumns;
                var isIgnoreNull = _Options?.IgnoreNullColumns == true;
                if (IsDeleted)
                {
                    x.AsUpdateable.IgnoreNullColumns(isIgnoreNull).IgnoreColumns(ignoreColumns?.ToArray()).PageSize(1).EnableQueryFilter().ExecuteCommand();
                }
                else
                {
                    x.AsUpdateable.IgnoreNullColumns(isIgnoreNull).IgnoreColumns(ignoreColumns?.ToArray()).ExecuteCommand();
                }
            }
            InitData(pkColumn, insertData);
            if (_NavigateType == NavigateType.OneToMany)
            {
                this._ParentList = children.Cast<object>().ToList();
            }
            else
            {
                this._ParentList = insertData.Union(updateData).Cast<object>().ToList();
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="pkColumn">主键列信息</param>
        /// <param name="UpdateData">要更新的数据列表</param>
        private void InitData<TChild>(EntityColumnInfo pkColumn, List<TChild> UpdateData) where TChild : class, new()
        {
            if (pkColumn.IsIdentity || pkColumn.OracleSequenceName.HasValue())
            {
                InsertIdentity(UpdateData);
            }
            else if (pkColumn.UnderType == UtilConstants.LongType)
            {
                SetValue(pkColumn, UpdateData, () => SnowFlakeSingle.Instance.NextId());
            }
            else if (pkColumn.UnderType == UtilConstants.GuidType)
            {
                SetValue(pkColumn, UpdateData, () => Guid.NewGuid());
            }
            else if (pkColumn.UnderType == UtilConstants.StringType)
            {
                SetValue(pkColumn, UpdateData, () => Guid.NewGuid().ToString());
            }
            else
            {
                SetError(pkColumn, UpdateData);
            }
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="pkColumn">主键列信息</param>
        /// <param name="UpdateData">要更新的数据列表</param>
        /// <param name="value">值生成函数</param>
        private void SetValue<TChild>(EntityColumnInfo pkColumn, List<TChild> UpdateData, Func<object> value) where TChild : class, new()
        {
            foreach (var child in UpdateData)
            {
                if (IsDefaultValue(pkColumn.PropertyInfo.GetValue(child)))
                {
                    pkColumn.PropertyInfo.SetValue(child, value());
                }
            }
            this._Context.Insertable(UpdateData).ExecuteCommand();
        }

        /// <summary>
        /// 设置错误提示
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="pkColumn">主键列信息</param>
        /// <param name="UpdateData">要更新的数据列表</param>
        private void SetError<TChild>(EntityColumnInfo pkColumn, List<TChild> UpdateData) where TChild : class, new()
        {
            foreach (var child in UpdateData)
            {
                if (IsDefaultValue(pkColumn.PropertyInfo.GetValue(child)))
                {
                    var name = pkColumn.EntityName + " " + pkColumn.DbColumnName;
                    Check.ExceptionEasy($"The field {name} is not an autoassignment type and requires an assignment", $"字段{name}不是可自动赋值类型，需要赋值 , 可赋值类型有 自增、long、Guid、string");
                }
            }
            this._Context.Insertable(UpdateData).ExecuteCommand();
        }
    }
}