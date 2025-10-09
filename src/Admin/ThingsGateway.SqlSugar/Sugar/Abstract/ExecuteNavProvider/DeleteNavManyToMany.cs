namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 删除导航数据提供者(用于处理多对多关系的删除操作)
    /// </summary>
    /// <typeparam name="Root">根实体类型</typeparam>
    /// <typeparam name="T">当前实体类型</typeparam>
    public partial class DeleteNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 删除多对多关系数据
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="name">导航属性名称</param>
        /// <param name="nav">导航属性信息</param>
        private void DeleteManyToMany<TChild>(string name, EntityColumnInfo nav) where TChild : class, new()
        {
            // 获取父实体信息和父实体列表
            var parentEntity = _ParentEntity;
            var parentList = _ParentList.Cast<T>().ToList();

            // 获取父实体主键列和导航属性
            var parentPkColumn = parentEntity.Columns.FirstOrDefault(it => it.IsPrimarykey == true);
            var parentNavigateProperty = parentEntity.Columns.FirstOrDefault(it => it.PropertyName == name);

            // 获取子实体信息和主键列
            var thisEntity = this._Context.EntityMaintenance.GetEntityInfo<TChild>();
            var thisPkColumn = thisEntity.Columns.FirstOrDefault(it => it.IsPrimarykey == true);

            // 检查主键是否存在
            if (thisPkColumn == null) { throw new SqlSugarLangException($"{thisPkColumn.EntityName} need primary key", $"{thisPkColumn.EntityName}需要主键"); }
            if (parentPkColumn == null) { throw new SqlSugarLangException($"{parentPkColumn.EntityName} need primary key", $"{parentPkColumn.EntityName}需要主键"); }

            // 获取映射表信息
            var mappingType = parentNavigateProperty.Navigat.MappingType;
            var mappingEntity = this._Context.EntityMaintenance.GetEntityInfo(mappingType);
            var mappingA = mappingEntity.Columns.FirstOrDefault(x => x.PropertyName == parentNavigateProperty.Navigat.MappingAId);
            var mappingB = mappingEntity.Columns.FirstOrDefault(x => x.PropertyName == parentNavigateProperty.Navigat.MappingBId);

            // 检查映射配置是否正确
            if (mappingA == null || mappingB == null) { throw new SqlSugarLangException($"Navigate property {name} error ", $"导航属性{name}配置错误"); }

            // 获取映射表主键列(排除关联字段)
            var mappingPk = mappingEntity.Columns
                   .Where(it => it.PropertyName != mappingA.PropertyName && it.PropertyName != mappingB.PropertyName)
                   .FirstOrDefault(it => it.IsPrimarykey && !it.IsIdentity && it.OracleSequenceName.IsNullOrEmpty());

            // 如果需要删除父表数据
            if (IsDeleteA())
            {
                if (!_IsDeletedParant)
                    SetContext(() => this._Context.Deleteable(parentList)
                    .EnableDiffLogEventIF(_RootOptions?.IsDiffLogEvent == true, _RootOptions?.DiffLogBizData)
                    .ExecuteCommand());
            }

            // 获取父表主键值列表
            var aids = _ParentList.Select(it => parentPkColumn.PropertyInfo.GetValue(it)).ToList();

            // 查询关联的子表主键值列表
            var bids = _Context.Queryable<object>().Filter(mappingEntity.Type).AS(mappingEntity.DbTableName).In(mappingA.DbColumnName, aids)
                .Select(mappingB.DbColumnName).ToDataTable()
                .Rows.Cast<System.Data.DataRow>().Select(it => it[0]).ToList();

            // 获取子表数据列表
            var childList = GetChildList<TChild>().In(thisPkColumn.DbColumnName, bids).ToList();

            // 如果有条件过滤，则更新bids为过滤后的子表主键值
            if (_WhereList.HasValue())
            {
                bids = childList.Select(it => thisPkColumn.PropertyInfo.GetValue(it)).ToList();
            }

            // 如果需要删除子表数据
            if (IsDeleteB())
            {
                SetContext(() => _Context.Deleteable(childList).ExecuteCommand());
            }

            // 更新当前处理的实体列表为主键列表
            this._ParentList = childList.Cast<object>().ToList();
            this._ParentPkColumn = thisPkColumn;
            this._IsDeletedParant = true;

            // 删除关联表数据
            if (_WhereList.HasValue())
            {
                // 带条件删除关联表数据
                SetContext(() => _Context.Deleteable<object>().AS(mappingEntity.DbTableName)
                .In(mappingA.DbColumnName, aids)
                .In(mappingB.DbColumnName, bids)
                .ExecuteCommand());
            }
            else
            {
                // 无条件删除关联表数据
                SetContext(() => _Context.Deleteable<object>().AS(mappingEntity.DbTableName).In(
                    mappingA.DbColumnName, aids
                    ).ExecuteCommand());
            }
        }

        /// <summary>
        /// 是否删除A表(父表)数据
        /// </summary>
        /// <returns></returns>
        private bool IsDeleteA()
        {
            return deleteNavOptions?.ManyToManyIsDeleteA == true;
        }

        /// <summary>
        /// 是否删除B表(子表)数据
        /// </summary>
        /// <returns></returns>
        private bool IsDeleteB()
        {
            return deleteNavOptions?.ManyToManyIsDeleteB == true;
        }
    }
}
