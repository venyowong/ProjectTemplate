using System;
using System.ComponentModel.DataAnnotations.Schema;
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

            var attrib = (ColumnAttribute)Attribute.GetCustomAttribute(member, typeof(ColumnAttribute), false);
            return attrib == null ? null : attrib.Name;
        }
    }
}