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

    If your current directory has only one pdm file, you can only type:
    
    ```
    PdmCodeGen
    ```
    Don't have to specify a file name.

## Tips

You can create template files and type mapping file in current directory with prefix "_", if that, PdmCodeGen use these content to generate code files.

```
_ClassTemplate.cs
_PropertyTemplate.cs
_TypeMapping.txt
```

### Dependencies

* .NET Framework 4.0

### Binary
  
[Current Release](https://github.com/Ruikuan/PdmCodeGen/raw/master/Download/PdmCodeGen_Release.zip)