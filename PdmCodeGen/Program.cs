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
            string fileName = null;
            if (args.Length < 1)
            {
                var pdmFiles = Directory.GetFiles(CurrentPath, "*.pdm", SearchOption.TopDirectoryOnly);
                if (pdmFiles.Length == 1)
                {
                    fileName = pdmFiles[0];
                }
                else
                {
                    Console.WriteLine("Please input filename.");
                    return;
                }
            }
            else
            {
                fileName = args[0];
                if (!File.Exists(fileName))
                {
                    fileName = Path.Combine(CurrentPath, args[0]);
                }
            }
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File not exists.");
                return;
            }

            ExtractAndGen(fileName);
        }

        private static void ExtractAndGen(string fileName)
        {
            using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var tables = PdmHelper.GetTables(stream);

                string classTemplate = GetClassTemplateContent();
                string propertyTemplate = GetPropertyTemplateContent();
                var typeMapping = GetMapping();

                foreach (var table in tables)
                {
                    StringBuilder classBuilder = new StringBuilder(classTemplate);
                    classBuilder.Replace("{TableName}", table.Name).Replace("{TableCode}", table.Code).Replace("{TableComment}", table.Comment);

                    StringBuilder propertiesBuilder = new StringBuilder();
                    foreach (var col in table.Columns)
                    {
                        propertiesBuilder.AppendLine(
                            propertyTemplate.Replace("{ColName}", col.Name)
                            .Replace("{ColCode}", col.Code)
                            .Replace("{ColComment}", col.Comment)
                            .Replace("{ColDataType}", GetMapType(typeMapping, col.DataType, !col.Mandatory)));
                    }
                    classBuilder.Replace("{Cols}", propertiesBuilder.ToString());

                    SaveToFile(table.Code, classBuilder.ToString());
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
            string file = null;
            string tryCurrentPathFileName = Path.Combine(CurrentPath, $"_{fileName}");
            if (File.Exists(tryCurrentPathFileName))
            {
                file = tryCurrentPathFileName;
            }
            else
            {
                file = Path.Combine(CurrentExePath, fileName);
            }

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
