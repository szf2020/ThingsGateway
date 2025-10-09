using System.Collections;
namespace ThingsGateway.SqlSugar
{
    public partial class InsertNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 插入一对多关系数据
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="name">导航属性名称</param>
        /// <param name="nav">导航属性信息</param>
        private void InsertOneToMany<TChild>(string name, EntityColumnInfo nav) where TChild : class, new()
        {
            // 初始化子实体列表
            List<TChild> children = new List<TChild>();

            // 获取父实体信息和父实体列表
            var parentEntity = _ParentEntity;
            var parentList = _ParentList;

            // 获取导航属性信息
            var parentNavigateProperty = parentEntity.Columns.FirstOrDefault(it => it.PropertyName == name);

            // 获取子实体信息、主键列和外键列
            var thisEntity = this._Context.EntityMaintenance.GetEntityInfo<TChild>();
            var thisPkColumn = GetPkColumnByNav2(thisEntity, nav);
            var thisFkColumn = GetFKColumnByNav(thisEntity, nav);

            // 获取父实体主键列
            EntityColumnInfo parentPkColumn = GetParentPkColumn();

            // 检查是否有自定义导航主键列
            EntityColumnInfo parentNavColumn = GetParentPkNavColumn(nav);
            if (parentNavColumn != null)
            {
                parentPkColumn = parentNavColumn;
            }

            // 处理父实体主键就是导航主键的情况
            if (ParentIsPk(parentNavigateProperty))
            {
                parentPkColumn = this._ParentEntity.Columns.FirstOrDefault(it => it.IsPrimarykey);
            }

            // 遍历父实体列表
            foreach (var item in parentList)
            {
                // 获取父实体主键值
                var parentValue = parentPkColumn.PropertyInfo.GetValue(item);

                // 获取子实体列表
                var childs = parentNavigateProperty.PropertyInfo.GetValue(item) as List<TChild>;

                if (childs != null)
                {
                    // 设置子实体的外键值
                    foreach (var child in childs)
                    {
                        thisFkColumn.PropertyInfo.SetValue(child, parentValue, null);
                    }
                    children.AddRange(childs);
                }
                else if (childs == null && parentNavigateProperty.PropertyInfo.GetValue(item) is IList ilist && ilist?.Count > 0)
                {
                    // 处理IList类型的子实体
                    childs = GetIChildsBylList(children, thisFkColumn, parentValue, ilist);
                }
            }

            // 检查是否是树形结构的子节点
            var isTreeChild = GetIsTreeChild(parentEntity, thisEntity);

            // 检查子实体是否有主键
            if (thisPkColumn == null) { throw new SqlSugarLangException($"{thisEntity.EntityName}need primary key", $"实体{thisEntity.EntityName}需要主键"); }

            // 根据条件决定是否插入数据
            if (NotAny(name) || isTreeChild)
            {
                InsertDatas(children, thisPkColumn);
            }
            else
            {
                this._ParentList = children.Cast<object>().ToList();
            }

            // 更新当前处理的父实体信息
            SetNewParent<TChild>(thisEntity, thisPkColumn);
        }

        /// <summary>
        /// 从IList中获取子实体列表
        /// </summary>
        private static List<TChild> GetIChildsBylList<TChild>(List<TChild> children, EntityColumnInfo thisFkColumn, object parentValue, IList ilist) where TChild : class, new()
        {
            // 转换IList为具体类型列表
            List<TChild> childs = ilist.Cast<TChild>().ToList();

            // 设置子实体的外键值
            foreach (var child in childs)
            {
                thisFkColumn.PropertyInfo.SetValue(child, parentValue, null);
            }
            children.AddRange(childs);
            return childs;
        }

        /// <summary>
        /// 检查是否是树形结构的子节点
        /// </summary>
        private bool GetIsTreeChild(EntityInfo parentEntity, EntityInfo thisEntity)
        {
            return this.NavContext?.Items?.Count > 0 && parentEntity.Type == thisEntity.Type;
        }

        /// <summary>
        /// 检查父实体主键是否就是导航主键
        /// </summary>
        private static bool ParentIsPk(EntityColumnInfo parentNavigateProperty)
        {
            return parentNavigateProperty?.Navigat != null &&
                   parentNavigateProperty.Navigat.NavigatType == NavigateType.OneToMany &&
                   parentNavigateProperty.Navigat.Name2 == null;
        }

        /// <summary>
        /// 获取父实体主键列
        /// </summary>
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
        /// 设置新的父实体信息
        /// </summary>
        private void SetNewParent<TChild>(EntityInfo entityInfo, EntityColumnInfo entityColumnInfo) where TChild : class, new()
        {
            this._ParentEntity = entityInfo;
            this._ParentPkColumn = entityColumnInfo;
        }
    }
}
