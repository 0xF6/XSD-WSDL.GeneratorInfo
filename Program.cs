using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RTDLetoGen
{
    public class BaseType
    {
        public class Field
        {
            public string Name;
            public string Type;
        }


        public List<Field> Response = new List<Field>();
        public List<Field> Request = new List<Field>();
    }


    

    class Program
    {
        static void Main(string[] args)
        {

            var leto = Assembly.Load(File.ReadAllBytes("Leto.dll"));

            List<string> exceptions = new List<string>();
            Dictionary<string, BaseType> bsTypes = new Dictionary<string, BaseType>();
            Dictionary<string, List<BaseType.Field>> others = new Dictionary<string, List<BaseType.Field>>();

            foreach (var type in leto.GetTypes())
            {
                if (type.Name.Contains("Exception"))
                {
                    exceptions.Add(type.Name);
                    continue;
                }

                if (type.Name.Contains("RequestType") || type.Name.Contains("ResponseType"))
                {
                    if (!bsTypes.ContainsKey(type.Name.ToName()))
                    bsTypes.Add(type.Name.ToName(), new BaseType());

                    if (type.Name.Contains("RequestType"))
                    {
                        foreach (var propertyInfo in type.GetProperties())
                        {
                            BaseType.Field fil = new BaseType.Field();

                            fil.Name = propertyInfo.Name;
                            fil.Type = propertyInfo.PropertyType.Name.IsTypePrimitive();

                            bsTypes[type.Name.ToName()].Request.Add(fil);
                        }
                    }
                    if (type.Name.Contains("ResponseType"))
                    {
                        foreach (var propertyInfo in type.GetProperties())
                        {
                            BaseType.Field fil = new BaseType.Field();

                            fil.Name = propertyInfo.Name;
                            fil.Type = propertyInfo.PropertyType.Name.IsTypePrimitive();

                            bsTypes[type.Name.ToName()].Response.Add(fil);
                        }
                    }
                    continue;
                }
                if (!others.ContainsKey(type.Name))
                others.Add(type.Name, new List<BaseType.Field>());

                foreach (var propertyInfo in type.GetProperties())
                {
                    BaseType.Field fil = new BaseType.Field();

                    fil.Name = propertyInfo.Name;
                    fil.Type = propertyInfo.PropertyType.Name.IsTypePrimitive();

                    others[type.Name].Add(fil);
                }
            }



            ProcessData(exceptions, others, bsTypes);

        }

        private static void ProcessData(List<string> exception, Dictionary<string, List<BaseType.Field>> other, Dictionary<string, BaseType> bsTypes)
        {
            StringBuilder builder = new StringBuilder();


            foreach (var type in bsTypes)
            {
                builder.AppendLine($" {type.Key}: ");
                builder.AppendLine($"    Response>");
                foreach (var res in type.Value.Response)
                {
                    builder.AppendLine($"     Type: {res.Type}");
                    builder.AppendLine($"     Name: {res.Name}");
                }
                builder.AppendLine($"    Request>");
                foreach (var res in type.Value.Request)
                {
                    builder.AppendLine($"     Type: {res.Type}");
                    builder.AppendLine($"     Name: {res.Name}");
                }
                builder.AppendLine($"");
                builder.AppendLine($"");
            }

            builder.AppendLine($"   -[ EXCEPTIONS ]-    ");

            foreach (string s in exception)
            {
                builder.AppendLine($" {s}");
            }

            builder.AppendLine($"   -[ OTHER TYPE ]-    ");

            foreach (var s in other)
            {
                builder.AppendLine($"  Name: {s.Key}");
                foreach (var field in s.Value)
                {
                    builder.AppendLine($" --======================");
                    builder.AppendLine($"     Name: {field.Name}");
                    builder.AppendLine($"     Type: {field.Type}");
                    builder.AppendLine($" --======================");
                }
                builder.AppendLine($"");
                builder.AppendLine($"");
            }
            File.WriteAllText("Leto-info.txt", builder.ToString());
        }
    }

    public static class LetoEx
    {
        public static string ToName(this string s)
        {
            return s.Replace("RequestType", "").Replace("ResponseType", "");
        }

        private static readonly List<string> listOfPrimitive = new List<string>
        {
            typeof(string).Name,
            typeof(int).Name,
            typeof(long).Name,
            typeof(short).Name,
            typeof(object).Name
        };

        public static string IsTypePrimitive(this string s)
        {
            if (listOfPrimitive.Contains(s))
                return s.ToLower();
            return s;
        }
    }
}
