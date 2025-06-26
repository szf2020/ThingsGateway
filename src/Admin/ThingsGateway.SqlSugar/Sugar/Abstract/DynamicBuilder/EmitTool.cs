using System.Reflection;
using System.Reflection.Emit;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// Emit动态代码生成工具类
    /// </summary>
    internal static class EmitTool
    {
        /// <summary>
        /// 创建动态模块构建器
        /// </summary>
        /// <returns>模块构建器</returns>
        internal static ModuleBuilder CreateModuleBuilder()
        {
            AssemblyBuilder assemblyBuilder = CreateAssembly();
            // 定义动态模块，使用随机名称避免冲突
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
            return moduleBuilder;
        }

        /// <summary>
        /// 创建动态程序集构建器
        /// </summary>
        /// <returns>程序集构建器</returns>
        internal static AssemblyBuilder CreateAssembly()
        {
            // 使用GUID生成唯一程序集名称
            AssemblyName assemblyName = new AssemblyName($"DynamicAssembly_{Guid.NewGuid():N}");
            // 创建可回收的动态程序集
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                assemblyName,
                AssemblyBuilderAccess.RunAndCollect);
            return assemblyBuilder;
        }

        /// <summary>
        /// 创建类型构建器
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="attributes">类型属性</param>
        /// <param name="baseType">基类</param>
        /// <param name="interfaces">实现的接口</param>
        /// <returns>类型构建器</returns>
        internal static TypeBuilder CreateTypeBuilder(
            string className,
            TypeAttributes attributes,
            Type baseType,
            Type[] interfaces)
        {
            ModuleBuilder moduleBuilder = CreateModuleBuilder();
            // 定义类型并指定基类和接口
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                className,
                attributes,
                baseType,
                interfaces);
            return typeBuilder;
        }

        /// <summary>
        /// 创建属性定义
        /// </summary>
        /// <param name="typeBuilder">类型构建器</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="propertyCustomAttributes">自定义特性</param>
        /// <returns>属性构建器</returns>
        internal static PropertyBuilder CreateProperty(
            TypeBuilder typeBuilder,
            string propertyName,
            Type propertyType,
            IEnumerable<CustomAttributeBuilder> propertyCustomAttributes = null)
        {
            // 定义私有字段
            FieldBuilder fieldBuilder = typeBuilder.DefineField(
                $"_{propertyName}",
                propertyType,
                FieldAttributes.Private);

            // 定义属性
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                propertyName,
                PropertyAttributes.None,
                propertyType,
                null);

            // 生成get方法
            MethodBuilder getterBuilder = typeBuilder.DefineMethod(
                $"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType,
                Type.EmptyTypes);

            ILGenerator getterIL = getterBuilder.GetILGenerator();
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIL.Emit(OpCodes.Ret);

            // 生成set方法
            MethodBuilder setterBuilder = typeBuilder.DefineMethod(
                $"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new[] { propertyType });

            ILGenerator setterIL = setterBuilder.GetILGenerator();
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldarg_1);
            setterIL.Emit(OpCodes.Stfld, fieldBuilder);
            setterIL.Emit(OpCodes.Ret);

            // 关联get/set方法
            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);

            // 添加自定义特性
            if (propertyCustomAttributes != null)
            {
                foreach (var attributeBuilder in propertyCustomAttributes)
                {
                    propertyBuilder.SetCustomAttribute(attributeBuilder);
                }
            }

            return propertyBuilder;
        }
    }
}