﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

#if NET || NETSTANDARD || CORECLR

namespace Aqua.TypeSystem.Emit
{
    using Aqua.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    partial class TypeEmitter
    {
        private sealed class PropertyList
        {
            private readonly ReadOnlyCollection<string> _properties;

            private readonly Lazy<int> _hash;

            internal PropertyList(IEnumerable<string> properties)
            {
                _properties = properties.ToList().AsReadOnly();
                _hash = new Lazy<int>(_properties.GetCollectionHashCode);
            }

            public IEnumerable<string> Properties => _properties;

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == typeof(PropertyList) && Equals((PropertyList)obj);
            }

            private bool Equals(PropertyList other)
            {
                if (_properties.Count != other._properties.Count)
                {
                    return false;
                }

                for (int i = 0; i < _properties.Count; i++)
                {
                    if (!string.Equals(_properties[i], other._properties[i], StringComparison.Ordinal))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override int GetHashCode() => _hash.Value;
        }

        private sealed class TypeWithPropertyList
        {
            private readonly string _typeFullName;

            private readonly ReadOnlyCollection<Tuple<string, Type>> _properties;

            private readonly Lazy<int> _hash;

            public TypeWithPropertyList(TypeInfo typeInfo)
            {
                _typeFullName = typeInfo.FullName;

                var properties = typeInfo.Properties;
                _properties = ReferenceEquals(null, properties)
                    ? new List<Tuple<string, Type>>().AsReadOnly()
                    : properties.Select(CreatePropertyInfo).ToList().AsReadOnly();

                _hash = new Lazy<int>(_properties.GetCollectionHashCode);
            }

            public string TypeFullName => _typeFullName;

            public IEnumerable<Tuple<string, Type>> Properties => _properties;

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == typeof(TypeWithPropertyList) && Equals((TypeWithPropertyList)obj);
            }

            private bool Equals(TypeWithPropertyList other)
            {
                if (!string.Equals(_typeFullName, other._typeFullName))
                {
                    return false;
                }

                if (_properties.Count != other._properties.Count)
                {
                    return false;
                }

                return _properties.CollectionEquals(other._properties);
            }

            public override int GetHashCode() => _hash.Value;

            private static Tuple<string, Type> CreatePropertyInfo(PropertyInfo propertyInfo)
            {
                if (ReferenceEquals(null, propertyInfo))
                {
                    return null;
                }

                var propertyName = propertyInfo.Name;
                if (string.IsNullOrEmpty(propertyName))
                {
                    throw new Exception("Property name missing");
                }

                var propertyTypeInfo = propertyInfo.PropertyType;
                if (ReferenceEquals(null, propertyInfo))
                {
                    throw new Exception($"Property type missing for property '{propertyInfo.Name}'");
                }

                var propertyType = propertyTypeInfo.Type;

                return new Tuple<string, Type>(propertyName, propertyType);
            }
        }

        private sealed class TypeCache : TransparentCache<object, Type>
        {
            internal Type GetOrCreate(IEnumerable<string> properties, Func<IEnumerable<string>, Type> factory)
            {
                var key = new PropertyList(properties);
                return GetOrCreate(key, x => factory(properties));
            }

            internal Type GetOrCreate(TypeInfo typeInfo, Func<TypeInfo, Type> factory)
            {
                var key = new TypeWithPropertyList(typeInfo);
                return GetOrCreate(key, x => factory(typeInfo));
            }
        }
    }
}

#endif