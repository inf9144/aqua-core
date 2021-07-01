﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.Dynamic.DynamicObject
{
    using Aqua.Dynamic;
    using Shouldly;
    using System;
    using Xunit;

    public class When_converting_to_serializable_object_with_private_property_setters
    {
        [Serializable]
        private class SerializableType
        {
            public int Int32Property { get; set; }

            public double DoubleProperty { get; private set; }

            public string StringProperty { get; private set; }
        }

        private const int Int32Value = 11;
        private const double DoubleValue = 12.3456789;
        private const string StringValue = "eleven";

        private readonly SerializableType obj;

        public When_converting_to_serializable_object_with_private_property_setters()
        {
            var dynamicObject = new DynamicObject
            {
                Properties = new PropertySet
                {
                    { nameof(SerializableType.Int32Property), Int32Value },
                    { nameof(SerializableType.DoubleProperty), DoubleValue },
                    { nameof(SerializableType.StringProperty), StringValue },
                },
            };

            obj = dynamicObject.CreateObject<SerializableType>();
        }

        [Fact]
        public void Should_create_an_instance()
        {
            obj.ShouldNotBeNull();
        }

        [Fact]
        public void Should_have_the_int_property_set()
        {
            obj.Int32Property.ShouldBe(Int32Value);
        }

        [Fact]
        public void Should_have_the_double_property_set()
        {
            obj.DoubleProperty.ShouldBe(DoubleValue);
        }

        [Fact]
        public void Should_have_the_string_property_set()
        {
            obj.StringProperty.ShouldBe(StringValue);
        }
    }
}