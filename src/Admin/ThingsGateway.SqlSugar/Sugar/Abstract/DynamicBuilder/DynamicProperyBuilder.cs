using System.Reflection;
using System.Reflection.Emit;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 动态属性构建器
    /// </summary>
    public class DynamicPropertyBuilder
    {
        private bool IsCache = false;
        public DynamicBuilder baseBuilder;

        /// <summary>
        /// 创建新的构建器副本
        /// </summary>
        public static DynamicPropertyBuilder CopyNew()
        {
            return new DynamicPropertyBuilder();
        }

        /// <summary>
        /// 创建属性
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="column">列配置</param>
        /// <param name="isSplitField">是否分表字段</param>
        /// <param name="navigate">导航属性配置</param>
        public DynamicPropertyBuilder CreateProperty(string propertyName, Type propertyType, SugarColumn column = null, bool isSplitField = false, Navigate navigate = null)
        {
            column ??= new SugarColumn() { ColumnName = propertyName };

            var addItem = new PropertyMetadata
            {
                Name = propertyName,
                Type = propertyType,
                CustomAttributes = new List<CustomAttributeBuilder>() { baseBuilder.GetProperty(column) }
            };

            if (navigate != null)
            {
                addItem.CustomAttributes.Add(BuildNavigateAttribute(navigate));
            }

            baseBuilder.propertyAttr.Add(addItem);

            if (isSplitField)
            {
                addItem.CustomAttributes.Add(baseBuilder.GetSplitFieldAttr(new SplitFieldAttribute()));
            }

            return this;
        }

        /// <summary>
        /// 设置是否缓存
        /// </summary>
        public DynamicPropertyBuilder WithCache(bool isCache = true)
        {
            IsCache = isCache;
            return this;
        }

        /// <summary>
        /// 构建类型
        /// </summary>
        public Type BuilderType()
        {
            if (IsCache)
            {
                var key = baseBuilder.entityName + string.Join("_", baseBuilder.propertyAttr.Select(it => it.Name + it.Type.Name));
                return ReflectionInoCacheService.Instance.GetOrCreate(key, () =>
                {
                    return DynamicBuilderHelper.CreateDynamicClass(
                        baseBuilder.entityName,
                        baseBuilder.propertyAttr,
                        TypeAttributes.Public,
                        baseBuilder.entityAttr,
                        baseBuilder.baseType,
                        baseBuilder.interfaces);
                });
            }

            return DynamicBuilderHelper.CreateDynamicClass(
                baseBuilder.entityName,
                baseBuilder.propertyAttr,
                TypeAttributes.Public,
                baseBuilder.entityAttr,
                baseBuilder.baseType,
                baseBuilder.interfaces);
        }

        /// <summary>
        /// 构建关联类型
        /// </summary>
        public Tuple<Type, Type> BuilderTypes(DynamicPropertyBuilder dynamicBuilderB)
        {
            if (IsCache)
            {
                var key1 = baseBuilder.entityName + string.Join("_", baseBuilder.propertyAttr.Select(it => it.Name + it.Type.Name));
                var key2 = dynamicBuilderB.baseBuilder.entityName + string.Join("_", dynamicBuilderB.baseBuilder.propertyAttr.Select(it => it.Name + it.Type.Name));
                return new ReflectionInoCacheService().GetOrCreate(key1 + key2, () =>
                {
                    Tuple<Type, Type> result = GetBuilderTypes(dynamicBuilderB);
                    return result;
                });
            }
            else
            {
                Tuple<Type, Type> result = GetBuilderTypes(dynamicBuilderB);
                return result;
            }
        }

        private Tuple<Type, Type> GetBuilderTypes(DynamicPropertyBuilder dynamicBuilderB)
        {
            var typeBuilderA = EmitTool.CreateTypeBuilder(
                baseBuilder.entityName,
                TypeAttributes.Public,
                baseBuilder.baseType,
                baseBuilder.interfaces);

            var typeBuilderB = EmitTool.CreateTypeBuilder(
                dynamicBuilderB.baseBuilder.entityName,
                TypeAttributes.Public,
                dynamicBuilderB.baseBuilder.baseType,
                dynamicBuilderB.baseBuilder.interfaces);

            DynamicBuilderHelper.CreateDynamicClass(
                typeBuilderA,
                typeBuilderB,
                baseBuilder.propertyAttr,
                baseBuilder.entityAttr);

            DynamicBuilderHelper.CreateDynamicClass(
                typeBuilderB,
                typeBuilderA,
                dynamicBuilderB.baseBuilder.propertyAttr,
                dynamicBuilderB.baseBuilder.entityAttr);

            return Tuple.Create(
                typeBuilderB.CreateTypeInfo().AsType(),
                typeBuilderA.CreateTypeInfo().AsType());
        }

        /// <summary>
        /// 构建导航属性特性
        /// </summary>
        public CustomAttributeBuilder BuildNavigateAttribute(Navigate navigate)
        {
            NavigateType navigatType = navigate.NavigatType;
            string name = navigate.Name;
            string name2 = navigate.Name2;
            string whereSql = navigate.WhereSql;
            Type mappingTableType = navigate.MappingType;
            string typeAiD = navigate.MappingAId;
            string typeBId = navigate.MappingBId;
            ConstructorInfo constructor;
            object[] constructorArgs;

            if (mappingTableType != null && typeAiD != null && typeBId != null)
            {
                constructor = typeof(Navigate).GetConstructor(new Type[] { typeof(Type), typeof(string), typeof(string), typeof(string) });
                constructorArgs = new object[] { mappingTableType, typeAiD, typeBId, whereSql };
            }
            else if (!string.IsNullOrEmpty(whereSql))
            {
                constructor = typeof(Navigate).GetConstructor(new Type[] { typeof(NavigateType), typeof(string), typeof(string), typeof(string) });
                constructorArgs = new object[] { navigatType, name, name2, whereSql };
            }
            else if (!string.IsNullOrEmpty(name2))
            {
                constructor = typeof(Navigate).GetConstructor(new Type[] { typeof(NavigateType), typeof(string), typeof(string) });
                constructorArgs = new object[] { navigatType, name, name2 };
            }
            else
            {
                constructor = typeof(Navigate).GetConstructor(new Type[] { typeof(NavigateType), typeof(string) });
                constructorArgs = new object[] { navigatType, name };
            }

            return new CustomAttributeBuilder(constructor, constructorArgs);
        }
    }
}