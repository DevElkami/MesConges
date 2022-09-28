using System;
using System.Collections.Generic;

namespace WebApplicationConges
{
    public static class Cache
    {
        private static Dictionary<String, Object> cache = new Dictionary<String, Object>();

        public static void Set(String key, Object data)
        {
            if (cache.ContainsKey(key))
                cache[key] = data;
            else
                cache.Add(key, data);
        }

        public static Object Get(String key)
        {
            if (cache.ContainsKey(key))
                return cache[key];
            return null;
        }

        public static void Clear(String key)
        {
            if (cache.ContainsKey(key))
                cache.Remove(key);
        }

        public static void ClearAll()
        {
            cache.Clear();
        }
    }
}
