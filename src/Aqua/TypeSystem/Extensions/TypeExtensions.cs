﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.TypeSystem.Extensions
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class TypeExtensions
    {
        public static bool IsAnonymousType(this Type type)
            => (type.CheckNotNull(nameof(type)).Name.Contains("AnonymousType", StringComparison.Ordinal)
            && type.IsDefined<CompilerGeneratedAttribute>())
            || type.IsEmittedType();

#if NETSTANDARD2_0
        [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Extra parameter added on purpose")]
        private static bool Contains(this string text, string value, StringComparison stringComparison) => text.Contains(value);
#endif // NETSTANDARD2_0

        public static bool IsEmittedType(this Type type) => type.IsDefined<Emit.EmittedTypeAttribute>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsDefined<T>(this Type type)
            where T : Attribute
            => type.IsDefined(typeof(T));

        internal static Type AsNonNullableType(this Type type)
        {
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            return isNullable ? type.GetGenericArguments()[0] : type;
        }

        public static bool Implements(this Type type, Type interfaceType)
            => type.Implements(interfaceType, new Type[1][]);

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
        public static bool Implements(this Type type, Type interfaceType, [NotNullWhen(true)] out Type[]? genericTypeArguments)
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
        {
            var typeArgs = new Type[1][];
            if (type.Implements(interfaceType, typeArgs))
            {
                genericTypeArguments = typeArgs[0];
                return true;
            }

            genericTypeArguments = null;
            return false;
        }

        private static bool Implements(this Type type, Type interfaceType, Type[][] typeArgs)
        {
            var isAssignableFromSpecifiedInterface = interfaceType.CheckNotNull(nameof(interfaceType)).IsGenericTypeDefinition
                ? IsAssignableToGenericTypeDefinition(interfaceType, typeArgs)
                : interfaceType.IsGenericType
                ? IsAssignableToGenericType(interfaceType, typeArgs)
                : interfaceType.IsAssignableFrom;

            return isAssignableFromSpecifiedInterface(type)
                || type.GetInterfaces().Any(isAssignableFromSpecifiedInterface);
        }

        private static Func<Type, bool> IsAssignableToGenericTypeDefinition(Type interfaceTypeInfo, Type[][] typeArgs)
        {
            var genericArgumentsCount = interfaceTypeInfo.GetGenericArguments().Length;

            return i =>
            {
                var genericArguments = i.GenericTypeArguments;
                var isAssignable = i.IsGenericType
                    && genericArguments.Length == genericArgumentsCount
                    && interfaceTypeInfo.MakeGenericType(genericArguments).IsAssignableFrom(i);
                if (isAssignable)
                {
                    typeArgs[0] = genericArguments;
                }

                return isAssignable;
            };
        }

        private static Func<Type, bool> IsAssignableToGenericType(Type interfaceTypeInfo, Type[][] typeArgs)
        {
            var interfaceTypeDefinition = interfaceTypeInfo.GetGenericTypeDefinition();
            var interfaceGenericArguments = interfaceTypeInfo.GetGenericArguments();

            return i =>
            {
                if (i.IsGenericType && !i.IsGenericTypeDefinition)
                {
                    var typeDefinition = i.GetGenericTypeDefinition();
                    if (typeDefinition == interfaceTypeDefinition)
                    {
                        var genericArguments = i.GetGenericArguments();
                        var allArgumentsAreAssignable = Enumerable.Range(0, genericArguments.Length - 1)
                            .All(index => Implements(genericArguments[index], interfaceGenericArguments[index], typeArgs));
                        if (allArgumentsAreAssignable)
                        {
                            return true;
                        }
                    }
                }

                return false;
            };
        }

        public static bool IsEnum(this Type type)
            => type.CheckNotNull(nameof(type)).AsNonNullableType().IsEnum;
    }
}
