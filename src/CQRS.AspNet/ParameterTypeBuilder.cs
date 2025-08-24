namespace CQRS.AspNet;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

public static class ParameterTypeBuilder
{
    private static readonly AssemblyBuilder AssemblyBuilder =
        AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicParameterAssembly"), AssemblyBuilderAccess.Run);

    private static readonly ModuleBuilder ModuleBuilder =
        AssemblyBuilder.DefineDynamicModule("DynamicParameterModule");

    public static Type CreateParameterType(string typeName, List<ParameterInfo> parameters)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            throw new ArgumentException("Type name must be provided.", nameof(typeName));

        // var typeBuilder = ModuleBuilder.DefineType(
        //     typeName,
        //     TypeAttributes.Public | TypeAttributes.Class
        // );

        var typeBuilder = GetTypeBuilder();


        // NullableContext(2): Enables nullable annotations for reference types
        var nullableContextAttrCtor = typeof(NullableContextAttribute).GetConstructor(new[] { typeof(byte) })!;
        var nullableContextAttr = new CustomAttributeBuilder(nullableContextAttrCtor, new object[] { (byte)2 });
        typeBuilder.SetCustomAttribute(nullableContextAttr);

        foreach (var param in parameters)
        {
            AddPropertyWithAttributes(typeBuilder, param);
        }

        return typeBuilder.CreateTypeInfo()!;
    }

    private static TypeBuilder GetTypeBuilder()
    {
        AssemblyName dynamicAssemblyName = new AssemblyName("DynamicTypeAssembly");
        AssemblyBuilder dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("DynamicTypeModule");
        TypeBuilder dynamicAnonymousType = dynamicModule.DefineType("DynamicType", TypeAttributes.Public | TypeAttributes.Class);
        return dynamicAnonymousType;
    }


    private static void AddPropertyWithAttributes(TypeBuilder typeBuilder, ParameterInfo param)
    {
        string name = param.Name;
        Type type = param.Type;
        string description = param.Description;

        var fieldBuilder = typeBuilder.DefineField($"_{char.ToLowerInvariant(name[0])}{name[1..]}", type, FieldAttributes.Private);

        var propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.HasDefault, type, null);

        if (!String.IsNullOrWhiteSpace(description))
        {

        }

        // [Description("...")]
        var descriptionCtor = typeof(DescriptionAttribute).GetConstructor(new[] { typeof(string) })!;
        var descriptionAttr = new CustomAttributeBuilder(descriptionCtor, new object[] { description });
        propertyBuilder.SetCustomAttribute(descriptionAttr);

        // [Nullable(2)] for nullable reference types or nullable value types
        if (param.IsOptional && IsNullable(type))
        {
            var nullableAttrCtor = typeof(NullableAttribute).GetConstructor(new[] { typeof(byte[]) })!;
            var nullableAttr = new CustomAttributeBuilder(nullableAttrCtor, new object[] { new byte[] { 2 } });
            propertyBuilder.SetCustomAttribute(nullableAttr);
        }

        // Define Getter
        var getter = typeBuilder.DefineMethod($"get_{name}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            type,
            Type.EmptyTypes);

        var getIL = getter.GetILGenerator();
        getIL.Emit(OpCodes.Ldarg_0);
        getIL.Emit(OpCodes.Ldfld, fieldBuilder);
        getIL.Emit(OpCodes.Ret);
        propertyBuilder.SetGetMethod(getter);

        // Define Setter
        var setter = typeBuilder.DefineMethod($"set_{name}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            null,
            new[] { type });

        var setIL = setter.GetILGenerator();
        setIL.Emit(OpCodes.Ldarg_0);
        setIL.Emit(OpCodes.Ldarg_1);
        setIL.Emit(OpCodes.Stfld, fieldBuilder);
        setIL.Emit(OpCodes.Ret);
        propertyBuilder.SetSetMethod(setter);
    }

    private static bool IsNullable(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }
}
