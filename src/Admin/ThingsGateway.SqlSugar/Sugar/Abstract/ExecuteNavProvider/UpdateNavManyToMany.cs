using Newtonsoft.Json;
namespace ThingsGateway.SqlSugar
{
    public partial class UpdateNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 更新多对多关系数据
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="name">导航属性名称</param>
        /// <param name="nav">导航属性信息</param>
        private void UpdateManyToMany<TChild>(string name, EntityColumnInfo nav) where TChild : class, new()
        {
            // 获取父实体信息和父实体列表
            var parentEntity = _ParentEntity;
            var parentList = _ParentList;

            // 获取父实体主键列和导航属性列
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
            if (mappingA == null || mappingB == null) { throw new SqlSugarLangException($"Navigate property {name} error ", $"导航属性{name}配置错误"); }

            // 获取映射表主键列(排除关联字段)
            var mappingPk = mappingEntity.Columns
                   .Where(it => it.PropertyName != mappingA.PropertyName && it.PropertyName != mappingB.PropertyName && it.IsPrimarykey && !it.IsIdentity && it.OracleSequenceName.IsNullOrEmpty())
                   .FirstOrDefault();

            // 获取映射表其他列(排除关联字段和主键)        
            var mappingOthers = mappingEntity.Columns
                   .Where(it => it.PropertyName != mappingA.PropertyName && it.PropertyName != mappingB.PropertyName && !it.IsIdentity && !it.IsPrimarykey && !it.IsOnlyIgnoreInsert && !it.IsIgnore);

            // 准备映射表数据
            List<Dictionary<string, object>> mappgingTables = new List<Dictionary<string, object>>();
            var ids = new List<object>();

            // 遍历父实体列表
            foreach (var item in parentList)
            {
                var items = parentNavigateProperty.PropertyInfo.GetValue(item);
                if (items == null)
                {
                    continue;
                }

                // 获取子实体列表
                var children = ((List<TChild>)items);

                // 根据配置决定是否更新子实体
                if (this._Options?.ManyToManyIsUpdateB == true)
                {
                    InsertDatas(children, thisPkColumn);
                }
                else
                {
                    _ParentList = children.Cast<object>().ToList();
                }

                // 获取父实体ID
                var parentId = parentPkColumn.PropertyInfo.GetValue(item);
                if (!ids.Contains(parentId))
                {
                    ids.Add(parentId);
                }

                // 为每个子实体创建映射关系
                foreach (var child in children)
                {
                    var chidId = thisPkColumn.PropertyInfo.GetValue(child);
                    Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();

                    // 添加关联字段
                    keyValuePairs.Add(mappingA.DbColumnName, parentId);
                    keyValuePairs.Add(mappingB.DbColumnName, chidId);

                    // 处理映射表其他字段的默认值
                    if (mappingOthers != null)
                    {
                        foreach (var pair in mappingOthers)
                        {
                            if (pair.UnderType == UtilConstants.DateType)
                            {
                                keyValuePairs.Add(pair.DbColumnName, DateTime.Now);
                            }
                            else if (pair.UnderType == UtilConstants.StringType)
                            {
                                keyValuePairs.Add(pair.DbColumnName, UtilConstants.Space);
                            }
                            else
                            {
                                keyValuePairs.Add(pair.DbColumnName, UtilMethods.GetDefaultValue(pair.UnderType));
                            }
                        }
                    }

                    // 处理映射表主键
                    if (mappingPk != null)
                    {
                        SetMappingTableDefaultValue(mappingPk, keyValuePairs);
                    }
                    mappgingTables.Add(keyValuePairs);
                }
            }

            // 根据配置决定如何处理原有映射关系
            if (this._Options?.ManyToManyEnableLogicDelete == true)
            {
                // 逻辑删除原有映射关系
                var locgicColumn = thisEntity.Columns.FirstOrDefault(it => it.PropertyName.EqualCase("IsDeleted") || it.PropertyName.EqualCase("IsDelete"));
                if (locgicColumn == null)
                {
                    throw new SqlSugarLangException(thisEntity.EntityName + "Logical deletion requires the entity to have the IsDeleted property",
                        thisEntity.EntityName + "假删除需要实体有IsDeleted属性");
                }

                List<IConditionalModel> conditionalModels = new List<IConditionalModel>();
                conditionalModels.Add(new ConditionalModel()
                {
                    FieldName = mappingA.DbColumnName,
                    FieldValue = string.Join(",", ids.Distinct()),
                    ConditionalType = ConditionalType.In,
                    CSharpTypeName = mappingA?.PropertyInfo?.PropertyType?.Name
                });

                var sqlObj = _Context.Queryable<object>().SqlBuilder.ConditionalModelToSql(conditionalModels);
                this._Context.Updateable<object>()
                  .AS(mappingEntity.DbTableName)
                  .Where(sqlObj.Key, sqlObj.Value)
                  .SetColumns(locgicColumn.DbColumnName, true)
                  .ExecuteCommand();
            }
            else if (_Context?.CurrentConnectionConfig?.MoreSettings?.IsAutoDeleteQueryFilter == true)
            {
                // 带查询过滤器的物理删除
                this._Context.Deleteable<object>().AS(mappingEntity.DbTableName).In(mappingA.DbColumnName, ids).EnableQueryFilter(mappingEntity.Type).ExecuteCommand();
            }
            else
            {
                // 直接物理删除
                this._Context.Deleteable<object>().AS(mappingEntity.DbTableName).In(mappingA.DbColumnName, ids).ExecuteCommand();
            }

            // 根据是否有模板决定插入方式
            if (HasMappingTemplate(mappingEntity))
            {
                InertMappingWithTemplate(mappingEntity, mappingA, mappingB, mappgingTables);
            }
            else
            {
                this._Context.Insertable(mappgingTables).AS(mappingEntity.DbTableName).ExecuteCommand();
            }

            // 更新当前处理的父实体信息
            _ParentEntity = thisEntity;
        }

        /// <summary>
        /// 检查是否有映射表模板
        /// </summary>
        /// <param name="mappingEntity">映射表实体信息</param>
        /// <returns>是否存在模板</returns>
        private bool HasMappingTemplate(EntityInfo mappingEntity)
        {
            return this._Options?.ManyToManySaveMappingTemplate?.GetType() == mappingEntity.Type;
        }

        /// <summary>
        /// 使用模板插入映射关系
        /// </summary>
        /// <param name="mappingEntity">映射表实体信息</param>
        /// <param name="mappingA">关联字段A</param>
        /// <param name="mappingB">关联字段B</param>
        /// <param name="mappgingTables">映射表数据</param>
        private void InertMappingWithTemplate(EntityInfo mappingEntity, EntityColumnInfo mappingA, EntityColumnInfo mappingB, List<Dictionary<string, object>> mappgingTables)
        {
            var template = this._Options?.ManyToManySaveMappingTemplate;
            List<object> mappingObjects = new List<object>();

            foreach (var item in mappgingTables)
            {
                // 序列化模板对象
                var serializedTemplate = JsonConvert.SerializeObject(template);

                // 反序列化模板对象，创建新的映射对象
                var mappingObject = JsonConvert.DeserializeObject(serializedTemplate, template.GetType());

                // 获取映射对象的所有字段
                var fields = mappingEntity.Columns;

                // 遍历字典中的键值对，并将值赋给映射对象的对应字段
                foreach (var kvp in item)
                {
                    var fieldName = kvp.Key;
                    var fieldValue = kvp.Value;

                    // 查找与字段名匹配的字段
                    var field = fields.FirstOrDefault(f => f.DbColumnName.EqualCase(fieldName));

                    // 如果字段存在且是主键或关联字段，则赋值
                    if (field != null)
                    {
                        var isSetValue = field.IsPrimarykey
                        || field.DbColumnName == mappingA.DbColumnName
                        || field.DbColumnName == mappingB.DbColumnName;
                        if (isSetValue)
                            field.PropertyInfo.SetValue(mappingObject, fieldValue);
                    }
                }

                // 将映射对象添加到列表中
                mappingObjects.Add(mappingObject);
            }

            // 执行插入操作
            this._Context.InsertableByObject(mappingObjects).ExecuteCommand();
        }

        /// <summary>
        /// 设置映射表主键默认值
        /// </summary>
        /// <param name="mappingPk">映射表主键列</param>
        /// <param name="keyValuePairs">键值对字典</param>
        private void SetMappingTableDefaultValue(EntityColumnInfo mappingPk, Dictionary<string, object> keyValuePairs)
        {
            if (mappingPk.UnderType == UtilConstants.LongType)
            {
                keyValuePairs.Add(mappingPk.DbColumnName, SnowFlakeSingle.Instance.NextId());
            }
            else if (mappingPk.UnderType == UtilConstants.GuidType)
            {
                keyValuePairs.Add(mappingPk.DbColumnName, Guid.NewGuid());
            }
            else if (mappingPk.UnderType == UtilConstants.StringType)
            {
                keyValuePairs.Add(mappingPk.DbColumnName, Guid.NewGuid() + "");
            }
            else
            {
                var name = mappingPk.EntityName + " " + mappingPk.DbColumnName;
                Check.ExceptionLang($"The field {name} is not an autoassignment type and requires an assignment",
                    $" 中间表主键字段{name}不是可自动赋值类型， 可赋值类型有 自增、long、Guid、string。你也可以删掉主键 用双主键");
            }
        }
    }
}
