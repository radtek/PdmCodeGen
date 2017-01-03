# Gen Domain Classes From PDM

Pdm files are physical data model files created by Sybase PowerDesigner. We use pdm generate database, and we create domain classes to map with database. Coding domain classes is so boring, so we can use this tool to generate domain classes for us.

## How to use

1. Modify "ClassTemplate.cs" and "PropertyTemplate.cs" to fit your need.

ClassTemplate
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNameSpace
{
    /// <summary>
    /// {TableName}
    /// </summary>
    public class {TableCode}
    {
{Cols}
    }
}
```
PropertyTemplate
```csharp
        /// <summary>
        /// {ColName}
        /// </summary>
        public {ColDataType} {ColCode} { get; set; }
```

2. Modify "TypeMapping.txt" to add more types.

```
bit bool?
uniqueidentifier Guid?
nvarchar string
varchar string
text string
ntext string
date DateTime?
int int?
bigint long?
datetime DateTime?
image byte[]
tinyint byte?
```
One map per line, and split with space. If target type is valuetype, you should add "?" after it.

3. Execute command line.

```
PdmCodeGen "{file.pdm}"
```

It will generate files in "Code" sub directory of your current directory.