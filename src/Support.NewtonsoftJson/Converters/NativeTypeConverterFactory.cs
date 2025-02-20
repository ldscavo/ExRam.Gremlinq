﻿using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using ExRam.Gremlinq.Core.Transformation;
using ExRam.Gremlinq.Core;

namespace ExRam.Gremlinq.Support.NewtonsoftJson
{
    internal sealed class NativeTypeConverterFactory : IConverterFactory
    {
        public sealed class NativeTypeConverter<TTarget> : IConverter<JValue, TTarget>
        {
            private readonly IGremlinQueryEnvironment _environment;

            public NativeTypeConverter(IGremlinQueryEnvironment environment)
            {
                _environment = environment;
            }

            public bool TryConvert(JValue serialized, ITransformer recurse, [NotNullWhen(true)] out TTarget? value)
            {
                if (serialized.Value is TTarget serializedValue)
                {
                    value = serializedValue;
                    return true;
                }

                if (_environment.SupportsType(typeof(TTarget)))
                {
                    if (serialized.ToObject<TTarget>() is { } convertedSerializedValue)
                    {
                        value = convertedSerializedValue;
                        return true;
                    }
                }

                value = default;
                return false;
            }
        }

        public IConverter<TSource, TTarget>? TryCreate<TSource, TTarget>(IGremlinQueryEnvironment environment)
        {
            return typeof(JValue).IsAssignableFrom(typeof(TSource))
                ? (IConverter<TSource, TTarget>)(object)new NativeTypeConverter<TTarget>(environment)
                : default;
        }
    }
}
