/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ruikuan. All rights reserved.
 *  Licensed under the Apache-2.0 License. See License in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace PdmCodeGen
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please input filename.");
                return;
            }
            string fileName = args[0];
            if (!File.Exists(fileName))
            {
                fileName = Path.Combine(CurrentPath, args[0]);
            }
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File not exists.");
                return;
            }

            using (FileStream stream = File.OpenRead(fileName))
            {
                XDocument doc = XDocument.Load(stream);
                XNamespace o = "object";
                XNamespace a = "attribute";

                string classTemplate = GetClassTemplateContent();
                string propertyTemplate = GetPropertyTemplateContent();
                var typeMapping = GetMapping();

                var list = doc.Descendants(o + "Table");
                foreach (var table in list)
                {
                    var code = table.Descendants(a + "Code").FirstOrDefault()?.Value;
                    if (code == null) continue; // not actual table

                    var name = table.Descendants(a + "Name").FirstOrDefault()?.Value;
                    var comment = table.Descendants(a + "Comment").FirstOrDefault()?.Value;

                    StringBuilder classBuilder = new StringBuilder(classTemplate);
                    classBuilder.Replace("{TableName}", name).Replace("{TableCode}", code).Replace("{TableComment}", comment);

                    StringBuilder propertiesBuilder = new StringBuilder();
                    foreach (var col in table.Descendants(o + "Column"))
                    {
                        var colCode = col.Descendants(a + "Code").FirstOrDefault()?.Value;
                        if (colCode == null) continue; // not actual column

                        var colName = col.Descendants(a + "Name").FirstOrDefault()?.Value;
                        var dataType = col.Descendants(a + "DataType").FirstOrDefault()?.Value;

                        var colComment = col.Descendants(a + "Comment").FirstOrDefault()?.Value;

                        var colMandatory = col.Descendants(a + "Mandatory").FirstOrDefault()?.Value;
                        // if mandatory, contains <Mandatory>1</Mandatory>. if not, no Mandatory element.
                        bool nullable = colMandatory != "1"; 

                        propertiesBuilder.AppendLine(
                            propertyTemplate.Replace("{ColName}", colName)
                            .Replace("{ColCode}", colCode).Replace("{ColComment}", colComment)
                            .Replace("{ColDataType}", GetMapType(typeMapping, dataType, nullable)));
                    }
                    classBuilder.Replace("{Cols}", propertiesBuilder.ToString());

                    SaveToFile(code, classBuilder.ToString());
                }
            }
        }

        private static void SaveToFile(string code, string content)
        {
            string outputPath = Path.Combine(CurrentPath, "Code");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            string outputFile = Path.Combine(outputPath, $"{code}.cs");
            File.WriteAllText(outputFile, content, Encoding.UTF8);
        }

        static string CurrentPath => Directory.GetCurrentDirectory();
        static string GetClassTemplateContent() => GetFileContent("ClassTemplate.cs");
        static string GetPropertyTemplateContent() => GetFileContent("PropertyTemplate.cs");
        static string GetTypeMappingContent() => GetFileContent("TypeMapping.txt");

        static string CurrentExePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        static string GetFileContent(string fileName)
        {
            var file = Path.Combine(CurrentExePath, fileName);
            return File.ReadAllText(file, Encoding.UTF8);
        }

        static Dictionary<string, string> GetMapping()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            using (StringReader reader = new StringReader(GetTypeMappingContent()))
            {
                string line = null;
                while (true)
                {
                    line = reader.ReadLine();
                    if (line == null) break;

                    line = line.Trim();
                    // mapping file may contain comment
                    if (line.StartsWith("#")) continue; 

                    string[] parts = line.Replace('\t', ' ').Split(' ');
                    if (parts.Length < 2) continue;

                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    if (!map.ContainsKey(key))
                    {
                        map.Add(key, value);
                    }
                }
            }
            return map;
        }

        static string GetMapType(Dictionary<string, string> mapping, string colType, bool nullable)
        {
            string mapType = colType;
            if (mapping.ContainsKey(colType))
            {
                mapType = mapping[colType];
            }
            foreach (var key in mapping.Keys)
            {
                if (colType.StartsWith(key))
                {
                    mapType = mapping[key];
                    break;
                }
            }

            if (!nullable)
            {
                //is not nullable, remove "?" for nullable valuetype.
                mapType = mapType.Replace("?", string.Empty); 
            }
            return mapType;
        }
    }
}
