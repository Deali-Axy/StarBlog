using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace StarBlog.Contrib.CLRStats;

/// <summary>
/// 根据原来代码的基础魔改了一下，By DealiAxy，2022-6-11
/// https://github.com/zanders3/json/blob/master/src/JSONWriter.cs
/// </summary>
internal static class ObjectExtensions {
    public static string ToJson(this object obj) {
        StringBuilder stringBuilder = new StringBuilder();
        AppendValue(stringBuilder, obj);
        return stringBuilder.ToString();
    }

    private static void AppendValue(StringBuilder stringBuilder, object? item) {
        if (item == null) {
            stringBuilder.Append("null");
            return;
        }

        var type = item.GetType();
        if (type == typeof(string) || type == typeof(char)) {
            stringBuilder.Append('"');
            var str = item.ToString();
            for (var i = 0; i < str.Length; ++i)
                if (str[i] < ' ' || str[i] == '"' || str[i] == '\\') {
                    stringBuilder.Append('\\');
                    var j = "\"\\\n\r\t\b\f".IndexOf(str[i]);
                    if (j >= 0)
                        stringBuilder.Append("\"\\nrtbf"[j]);
                    else
                        stringBuilder.AppendFormat("u{0:X4}", (UInt32) str[i]);
                }
                else
                    stringBuilder.Append(str[i]);

            stringBuilder.Append('"');
        }
        else if (type == typeof(byte) || type == typeof(sbyte)) {
            stringBuilder.Append(item.ToString());
        }
        else if (type == typeof(short) || type == typeof(ushort)) {
            stringBuilder.Append(item.ToString());
        }
        else if (type == typeof(int) || type == typeof(uint)) {
            stringBuilder.Append(item.ToString());
        }
        else if (type == typeof(long) || type == typeof(ulong)) {
            stringBuilder.Append(item.ToString());
        }
        else if (type == typeof(float)) {
            stringBuilder.Append(((float) item).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        else if (type == typeof(double)) {
            stringBuilder.Append(((double) item).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        else if (type == typeof(decimal)) {
            stringBuilder.Append(((decimal) item).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        else if (type == typeof(bool)) {
            stringBuilder.Append(((bool) item) ? "true" : "false");
        }
        else if (type.IsEnum) {
            stringBuilder.Append('"');
            stringBuilder.Append(item.ToString());
            stringBuilder.Append('"');
        }
        else if (item is IList) {
            stringBuilder.Append('[');
            var isFirst = true;
            var list = item as IList;
            foreach (var t in list) {
                if (isFirst)
                    isFirst = false;
                else
                    stringBuilder.Append(',');
                AppendValue(stringBuilder, t);
            }

            stringBuilder.Append(']');
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
            var keyType = type.GetGenericArguments()[0];

            //Refuse to output dictionary keys that aren't of type string
            if (keyType != typeof(string)) {
                stringBuilder.Append("{}");
                return;
            }

            stringBuilder.Append('{');
            var dict = item as IDictionary;
            var isFirst = true;
            foreach (var key in dict.Keys) {
                if (isFirst)
                    isFirst = false;
                else
                    stringBuilder.Append(',');
                stringBuilder.Append('\"');
                stringBuilder.Append((string) key);
                stringBuilder.Append("\":");
                AppendValue(stringBuilder, dict[key]);
            }

            stringBuilder.Append('}');
        }
        else {
            stringBuilder.Append('{');

            var isFirst = true;
            var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (var t in fieldInfos) {
                if (t.IsDefined(typeof(IgnoreDataMemberAttribute), true))
                    continue;

                var value = t.GetValue(item);
                if (value == null) continue;

                if (isFirst)
                    isFirst = false;
                else
                    stringBuilder.Append(',');
                stringBuilder.Append('\"');
                stringBuilder.Append(GetMemberName(t));
                stringBuilder.Append("\":");
                AppendValue(stringBuilder, value);
            }

            var propertyInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (var t in propertyInfo) {
                if (!t.CanRead || t.IsDefined(typeof(IgnoreDataMemberAttribute), true))
                    continue;

                var value = t.GetValue(item, null);

                if (value == null) continue;

                if (isFirst)
                    isFirst = false;
                else
                    stringBuilder.Append(',');
                stringBuilder.Append('\"');
                stringBuilder.Append(GetMemberName(t));
                stringBuilder.Append("\":");
                AppendValue(stringBuilder, value);
            }

            stringBuilder.Append('}');
        }
    }

    private static string GetMemberName(MemberInfo member) {
        if (member.IsDefined(typeof(DataMemberAttribute), true)) {
            var dataMemberAttribute =
                (DataMemberAttribute) Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute), true)!;
            if (!string.IsNullOrEmpty(dataMemberAttribute.Name))
                return dataMemberAttribute.Name;
        }

        return member.Name;
    }
}