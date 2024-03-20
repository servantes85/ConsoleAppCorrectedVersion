using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace ConsoleApp
{
    public class DataReader
    {
        List<ImportedObject> ImportedObjects;

        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            ImportedObjects = new List<ImportedObject>();

            using (var streamReader = new StreamReader(fileToImport))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    var values = line.Split(';');
                    if (values.Length >= 7)
                    {
                        var importedObject = new ImportedObject
                        {
                            Type = values[0].Trim(),
                            Name = values[1].Trim(),
                            Schema = values[2].Trim(),
                            ParentName = values[3].Trim(),
                            ParentType = values[4].Trim(),
                            DataType = values[5].Trim(),
                            IsNullable = values[6].Trim()
                        };
                        ImportedObjects.Add(importedObject);
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            // Clean and validate imported data
            foreach (var importedObject in ImportedObjects)
            {
                importedObject.Type = CleanAndNormalize(importedObject.Type);
                importedObject.Name = CleanAndNormalize(importedObject.Name);
                importedObject.Schema = CleanAndNormalize(importedObject.Schema);
                importedObject.ParentName = CleanAndNormalize(importedObject.ParentName);
                importedObject.ParentType = CleanAndNormalize(importedObject.ParentType);
            }

            // Assign number of children
            foreach (var importedObject in ImportedObjects)
            {
                importedObject.NumberOfChildren = ImportedObjects
                    .Count(obj => obj.ParentType == importedObject.Type && obj.ParentName == importedObject.Name);
            }

            // Print data
            if (printData)
            {
                foreach (var database in ImportedObjects.Where(obj => obj.Type.Equals("DATABASE", StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");
                    foreach (var table in ImportedObjects.Where(obj => obj.ParentType.Equals(database.Type, StringComparison.OrdinalIgnoreCase) && obj.ParentName == database.Name))
                    {
                        Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");
                        foreach (var column in ImportedObjects.Where(obj => obj.ParentType.Equals(table.Type, StringComparison.OrdinalIgnoreCase) && obj.ParentName == table.Name))
                        {
                            Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                        }
                    }
                }
            }
        }

        private string CleanAndNormalize(string value)
        {
            return value?.Trim().Replace(" ", "") ?? string.Empty;
        }

    }


    class ImportedObject : ImportedObjectBaseClass
    {
        public new string Name
        {
            get;
            set;
        }
        public string Schema;

        public string ParentName;
        public string ParentType
        {
            get; set;
        }

        public string DataType { get; set; }
        public string IsNullable { get; set; }

        public double NumberOfChildren;
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
