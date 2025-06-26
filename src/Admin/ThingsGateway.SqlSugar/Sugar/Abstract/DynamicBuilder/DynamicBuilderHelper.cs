using System.Reflection;
using System.Reflection.Emit;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 动态构建帮助类
    /// </summary>
    public static class DynamicBuilderHelper
    {
        /// <summary>
        /// 创建动态类
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="properties">属性元数据列表</param>
        /// <param name="attributes">类型属性</param>
        /// <param name="classCustomAttributes">类自定义特性构建器列表</param>
        /// <param name="baseType">基类</param>
        /// <param name="interfaces">接口数组</param>
        /// <returns>动态创建的类类型</returns>
        public static Type CreateDynamicClass(string className, List<PropertyMetadata> properties, TypeAttributes attributes = TypeAttributes.Public, List<CustomAttributeBuilder> classCustomAttributes = null, Type baseType = null, Type[] interfaces = null)
        {
            // 创建类型构建器
            TypeBuilder typeBuilder = EmitTool.CreateTypeBuilder(className, attributes, baseType, interfaces);

            // 添加类级别的自定义特性
            if (classCustomAttributes != null)
            {
                foreach (var attributeBuilder in classCustomAttributes)
                {
                    typeBuilder.SetCustomAttribute(attributeBuilder);
                }
            }

            // 为每个属性创建属性定义
            foreach (PropertyMetadata property in properties)
            {
                var type = property.Type;
                // 处理自引用类型
                if (type == typeof(DynamicOneselfType))
                {
                    type = typeBuilder;
                }
                // 处理自引用类型列表
                else if (type == typeof(DynamicOneselfTypeList))
                {
                    type = typeof(List<>).MakeGenericType(typeBuilder);
                }
                // 创建属性
                EmitTool.CreateProperty(typeBuilder, property.Name, type, property.CustomAttributes);
            }

            // 创建最终类型
            Type dynamicType = typeBuilder.CreateTypeInfo().AsType();

            return dynamicType;
        }

        /// <summary>
        /// 创建动态类(包含嵌套类型)
        /// </summary>
        /// <param name="typeBuilder">类型构建器</param>
        /// <param name="typeBuilderChild">子类型构建器</param>
        /// <param name="properties">属性元数据列表</param>
        /// <param name="classCustomAttributes">类自定义特性构建器列表</param>
        /// <returns>动态创建的类类型</returns>
        public static Type CreateDynamicClass(TypeBuilder typeBuilder, TypeBuilder typeBuilderChild, List<PropertyMetadata> properties, List<CustomAttributeBuilder> classCustomAttributes = null)
        {
            // 添加类级别的自定义特性
            if (classCustomAttributes != null)
            {
                foreach (var attributeBuilder in classCustomAttributes)
                {
                    typeBuilder.SetCustomAttribute(attributeBuilder);
                }
            }

            // 为每个属性创建属性定义
            foreach (PropertyMetadata property in properties)
            {
                var type = property.Type;
                // 处理自引用类型
                if (type == typeof(DynamicOneselfType))
                {
                    type = typeBuilder;
                }
                // 处理自引用类型列表
                else if (type == typeof(DynamicOneselfTypeList))
                {
                    type = typeof(List<>).MakeGenericType(typeBuilder);
                }
                // 处理嵌套对象类型
                else if (type == typeof(NestedObjectType))
                {
                    type = typeBuilderChild;
                }
                // 处理嵌套对象类型列表
                else if (type == typeof(NestedObjectTypeList))
                {
                    type = typeof(List<>).MakeGenericType(typeBuilderChild);
                }
                // 创建属性
                EmitTool.CreateProperty(typeBuilder, property.Name, type, property.CustomAttributes);
            }

            // 创建最终类型
            Type dynamicType = typeBuilder.CreateTypeInfo().AsType();

            return dynamicType;
        }
    }
}