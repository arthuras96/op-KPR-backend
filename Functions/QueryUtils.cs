using MySql;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Functions
{
    public class QueryUtils
    {
        public string InsertSingleQuotes(string text)
        {
            return "'" + text + "'";
        }

        public dynamic SQLHandler(object genericObject)
        {
			foreach (var genericProperty in genericObject.GetType()
														 .GetProperties()
														 .Where(genericProperty => !genericProperty.GetGetMethod().GetParameters().Any()))
			{
				if (genericProperty.PropertyType.ToString() == "System.String" && genericProperty.GetValue(genericObject, null) != null)
					genericProperty.SetValue(genericObject, SecureStr(genericProperty.GetValue(genericObject, null)
                                                                                     .ToString()));
				else if (genericProperty.PropertyType.ToString().Contains("+") && genericProperty.GetValue(genericObject, null) != null)
				{
					var genericObjectInProperty = SQLHandler(genericObject.GetType()
																			  .GetProperty(genericProperty.Name)
																			  .GetValue(genericObject, null));

					genericProperty.SetValue(genericObject, genericObjectInProperty);
				}
			}

			return genericObject;
		}

        public string ReturnId(MySqlDataReader Reader, string Parameter)
        {
            string id = "";
            if (Reader.HasRows)
            {
                while (Reader.Read())
                {
                    id = Reader[Parameter].ToString();
                }
            }
            return id;
        }

        public string SecureStr(string S)
        { // Retorna string sem tags HTML ou comandos para SQL injection
            if (S != null)
            {
                if (S.Length > 0)
                {
                    // Retira Tags HTML
                    S = Regex.Replace(S, @"<[^>]*>", "", RegexOptions.IgnorePatternWhitespace);

                    // Troca aspas simples por duas aspas simples evitando SQL injection
                    S = S.Replace("'", "''");

                    string[] eliminar = { "select", "drop", "insert", "delete", "xp_", "update", "having", "_", "--", ";", "truncate", "=" };
                    foreach (string sEliminar in eliminar)
                    {
                        //S = S.Replace(sEliminar, ""); // não é case insensitive
                        S = ReplaceEx(S, sEliminar, "");
                    }
                }
            }

            return S;
        }

        private static string ReplaceEx(string original, string pattern, string replacement)
        {
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int inc = (original.Length / pattern.Length) *
                      (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern,
                                 position0)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (int i = position0; i < original.Length; ++i)
                chars[count++] = original[i];

            return new string(chars, 0, count);
        }

        public List<T> DataTableToList<T>(DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        public dynamic DataTableToObject<T>(DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                if (list.Count > 0)
                    return list[0];
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public string DateBrToMySql(string text)
        {
            string[] auxDateTime = text.Split(' ');

            if(auxDateTime.Length < 2)
                return text;

            string[] auxDate = auxDateTime[0].Split('/');

            if (auxDate.Length < 3)
                return text;

            return auxDate[2] + "-" + auxDate[1] + "-" + auxDate[0] + " " + auxDateTime[1];
        }
    }
}
