using System;
using System.Reflection;

namespace BacterioEditor
{
    public class ReflectionUtility
    {
        public static void SetStructField<T>(string name, ref T obj, object value, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public) where T : struct
        {
            object boxed = obj;
            typeof(T).GetField(name, flags).SetValue(boxed, value);
            obj = (T)boxed;
        }

        public static void SetClassField<T>(string name, T obj, object value, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public) where T : class
        {
            typeof(T).GetField(name, flags).SetValue(obj, value);
        }
    }
}
