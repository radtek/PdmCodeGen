using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PdmCodeGen
{
    public static class PdmHelper
    {
        public static List<PdmTable> GetTables(Stream stream)
        {
            XDocument doc = XDocument.Load(stream);
            XNamespace o = "object";
            XNamespace a = "attribute";

            List<PdmTable> tableList = new List<PdmTable>();
            var list = doc.Descendants(o + "Table");

            foreach (var table in list)
            {
                var code = table.Descendants(a + "Code").FirstOrDefault()?.Value;
                if (code == null) continue; // not actual table

                var name = table.Descendants(a + "Name").FirstOrDefault()?.Value;
                var comment = table.Descendants(a + "Comment").FirstOrDefault()?.Value;
                var id = table.Attribute("Id").Value;

                PdmTable pdmTable = new PdmTable()
                {
                    Code = code,
                    Comment = comment,
                    Id = id,
                    Name = name
                };
                tableList.Add(pdmTable);

                foreach (var col in table.Descendants(o + "Column"))
                {
                    var colCode = col.Descendants(a + "Code").FirstOrDefault()?.Value;
                    if (colCode == null) continue; // not actual column

                    var colName = col.Descendants(a + "Name").FirstOrDefault()?.Value;
                    var dataType = col.Descendants(a + "DataType").FirstOrDefault()?.Value;

                    var colComment = col.Descendants(a + "Comment").FirstOrDefault()?.Value;

                    var colMandatory = col.Descendants(a + "Mandatory").FirstOrDefault()?.Value;
                    // if mandatory, contains <Mandatory>1</Mandatory>. if not, no Mandatory element.
                    bool mandatory = colMandatory == "1";
                    var colId = col.Attribute("Id").Value;

                    PdmTableColumn pdmCol = new PdmTableColumn()
                    {
                        Code = colCode,
                        Comment = colComment,
                        DataType = dataType,
                        Id = colId,
                        Mandatory = mandatory,
                        Name = colName
                    };
                    pdmTable.Columns.Add(pdmCol);
                }

                foreach (var key in table.Descendants(o + "Key"))
                {
                    foreach (var refCol in key.Descendants(o + "Column"))
                    {
                        var refId = refCol.Attribute("Ref").Value;
                        pdmTable.Columns.First(x => x.Id == refId).IsKey = true;
                    }
                }
            }

            return tableList;
        }
    }

    public sealed class PdmTable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Comment { get; set; }

        public List<PdmTableColumn> Columns { get; } = new List<PdmTableColumn>();

        public override string ToString() => Code;
    }

    public sealed class PdmTableColumn
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Comment { get; set; }
        public string DataType { get; set; }
        public bool Mandatory { get; set; }
        public bool IsKey { get; set; } = false;

        public override string ToString() => Code;
    }
}
