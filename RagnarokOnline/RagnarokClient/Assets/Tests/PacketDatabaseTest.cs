using NUnit.Framework;
using System;
using System.Reflection;
using UnityEngine;

namespace Tests
{
    public class PacketDatabaseTest : MonoBehaviour
    {
        [Test]
        public void CheckPacketSizes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (assembly.ManifestModule.Name != "RagnarokClient.dll")
                    continue;
                var types = assembly.GetTypes();
                var packets = Array.FindAll(types, t => t.IsClass && t.Namespace == "Client.Network" &&
                                                    (t.GetInterface("SND_Packet") != null || t.GetInterface("RCV_Packet") != null) && !t.FullName.Contains("Internal"));

                foreach (var packetType in packets)
                    AssertClassSize(packetType);
            }
        }

        private void AssertClassSize(Type t)
        {
            var fields = t.GetFields();
            fields = Array.FindAll(fields, field => !field.IsStatic);

            int size = 0;
            foreach (FieldInfo field in fields)
            {
                int s = GetTypeSize(field.FieldType);
                if (s == -1)
                {
                    if (field.Name.ToLower() == "name")
                        s = RO.Common.Constants.MAX_NAME_LEN;
                    else if (field.Name.ToLower() == "password")
                        s = RO.Common.Constants.MAX_PASSWORD_SIZE;
                    else
                    {
                        Debug.Log("No fixed size for string " + field.Name);
                        s = 0;
                    }
                }
                size += s;
            }

            var fieldInfos = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var fixedSize = Convert.ToInt32(Array.Find(fieldInfos, info => info.Name == "FIXED_SIZE").GetRawConstantValue());

            if (fixedSize == short.MaxValue)
                Debug.Log(t + " is dynamic packet");

            Assert.IsTrue(size == fixedSize || fixedSize == short.MaxValue, "Declared size " + fixedSize + " does not match actual size " + size + " for type " + t);
        }

        private int GetStructSize(Type t)
        {
            var fields = t.GetFields();
            fields = Array.FindAll(fields, field => !field.IsStatic);

            int size = 0;
            foreach (FieldInfo field in fields)
            {
                int s = GetTypeSize(field.FieldType);
                if (s == -1)
                {
                    if (field.Name.ToLower() == "name")
                        s = RO.Common.Constants.MAX_NAME_LEN;
                    else if (field.Name.ToLower() == "password")
                        s = RO.Common.Constants.MAX_PASSWORD_SIZE;
                    else
                    {
                        Debug.Log("No fixed size for string " + field.Name);
                        s = 0;
                    }
                }
                size += s;
            }

            return size;
        }

        private int GetTypeSize(Type type)
        {
            if (type.Name == "Byte" || type.Name == "Char")
                return sizeof(byte);
            else if (type.Name == "Int16" || type.Name == "UInt16")
                return sizeof(short);
            else if (type.Name == "Int32" || type.Name == "UInt32")
                return sizeof(int);
            else if (type.Name == "Double")
                return sizeof(double);
            else if (type.Name == "Single")
                return sizeof(float);
            else if (type.Name == "String")
                return -1;
            else if (type.Name == "Boolean")
                return sizeof(bool);
            else if (type.IsEnum)
            {
                if (type.Name == "Gender" || type.Name == "Jobs" || type.Name == "BlockTypes" || type.Name == "DamageType")
                    return sizeof(byte);
                return GetTypeSize(type.GetEnumUnderlyingType());
            }
            else if (type.IsArray && type.GetElementType().IsValueType && !type.GetElementType().IsPrimitive)
                return GetStructSize(type.GetElementType());
            Debug.Log("No if for type " + type.Name);
            return 0;
        }
    }
}
