﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Maps
{
    public class JsonPropertyMapper : IJsonPropertyMapper
    {
        private IStaticCacheManager _cacheManager;

        private IStaticCacheManager StaticCacheManager => _cacheManager ?? (_cacheManager = EngineContext.Current.Resolve<IStaticCacheManager>());

        public Dictionary<string, Tuple<string, Type>> GetMap(Type type)
        {
            if (!StaticCacheManager.IsSet(Constants.Configurations.JsonTypeMapsPattern))
            {
                StaticCacheManager.Set(Constants.Configurations.JsonTypeMapsPattern, new Dictionary<string, Dictionary<string, Tuple<string, Type>>>(),
                                       int.MaxValue);
            }

            //var typeMaps =
            //    StaticCacheManager.Get<Dictionary<string, Dictionary<string, Tuple<string, Type>>>>(Constants.Configurations.JsonTypeMapsPattern, () => null, 0);

            var typeMaps =
                StaticCacheManager.Get<Dictionary<string, Dictionary<string, Tuple<string, Type>>>>(Constants.Configurations.JsonTypeMapsPattern, () => new Dictionary<string, Dictionary<string, Tuple<string, Type>>>(), 0);


            if (!typeMaps.ContainsKey(type.Name))
            {
                Build(typeMaps, type);
            }

            return typeMaps[type.Name];
        }

        private void Build(Dictionary<string, Dictionary<string, Tuple<string, Type>>> typeMaps, Type type)
        {
            //var typeMaps =
            //    StaticCacheManager.Get<Dictionary<string, Dictionary<string, Tuple<string, Type>>>>(Constants.Configurations.JsonTypeMapsPattern, () => new Dictionary<string, Dictionary<string, Tuple<string, Type>>>(), 0);

            var mapForCurrentType = new Dictionary<string, Tuple<string, Type>>();

            var typeProps = type.GetProperties();

            foreach (var property in typeProps)
            {
                var jsonAttribute = property.GetCustomAttribute(typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;
                var doNotMapAttribute = property.GetCustomAttribute(typeof(DoNotMapAttribute)) as DoNotMapAttribute;

                // If it has json attribute set and is not marked as doNotMap
                if (jsonAttribute != null && doNotMapAttribute == null)
                {
                    if (!mapForCurrentType.ContainsKey(jsonAttribute.PropertyName))
                    {
                        var value = new Tuple<string, Type>(property.Name, property.PropertyType);
                        mapForCurrentType.Add(jsonAttribute.PropertyName, value);
                    }
                }
            }

            if (!typeMaps.ContainsKey(type.Name))
            {
                typeMaps.Add(type.Name, mapForCurrentType);
            }
        }
    }
}
