using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Dapper;

namespace ProjectTemplate
{
    public static class Utility
    {
        public static void MakeDapperMapping(string namspace)
        {
            foreach (var type in Assembly.GetEntryAssembly().GetTypes().Where(t => t.FullName.Contains(namspace)))
            {
                var map = new CustomPropertyTypeMap(type, (t, columnName) => t.GetProperties().FirstOrDefault(
                    prop => GetDescriptionFromAttribute(prop) == columnName || prop.Name.ToLower().Equals(columnName.ToLower())));
                Dapper.SqlMapper.SetTypeMap(type, map);
            }
        }

        private static string GetDescriptionFromAttribute(MemberInfo member)
        {
            if (member == null) return null;

            var attrib = (DescriptionAttribute)Attribute.GetCustomAttribute(member, typeof(DescriptionAttribute), false);
            return attrib == null ? null : attrib.Description;
        }
    }
}