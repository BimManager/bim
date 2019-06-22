using System;
using System.Collections.Generic;
using ArrayList = System.Collections.ArrayList;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Document = Autodesk.Revit.DB.Document;
using Transaction = Autodesk.Revit.DB.Transaction;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace TektaRevitPlugins
{
    internal static class ExtensibleStorageUtils
    {
        #region Static Fields
        const string VENDOR_ID = "ADSK";
        #endregion

        internal static Schema GetSchema(Guid guid, string schemaName, IDictionary<string, ISet<string>> values)
        {
            // Ensure that the schema being generated is unique -
            // - otherwise, return the existing one.
            Schema schema = Schema.Lookup(guid);

            if (schema == null) {
                // 1. Create and name a new schema
                SchemaBuilder schemaBuilder =
                    new SchemaBuilder(guid);
                schemaBuilder.SetSchemaName(schemaName);

                // 2. Set the read/write access
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Vendor);
                schemaBuilder.SetVendorId(VENDOR_ID);

                // 3. Define one or more fields of data
                foreach (string key in values.Keys) {
                    if (schemaBuilder.AcceptableName(key))
                        schemaBuilder.AddArrayField(key, typeof(string));
                    else
                        System.Diagnostics.Trace.Write($"{key}");
                }
                schema = schemaBuilder.Finish();
            }
            return schema;
        }
        internal static DataStorage GetDataStorage(Document doc, string name)
        {
            DataStorage dataStorage =
                new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(DataStorage))
                .Where(ds => ds.Name == name)
                .FirstOrDefault() as DataStorage;

            if (dataStorage == null) {
                using (Transaction t = new Transaction(doc, "Create data storag")) {
                    t.Start();
                    dataStorage = DataStorage.Create(doc);
                    dataStorage.Name = name;
                    t.Commit();
                }
            }
            return dataStorage;
        }

        internal static bool AssignValues(Schema schema, DataStorage dataStorage,
            IDictionary<string,ISet<string>> values)
        {
            try {
                // 4. Create an entity based on the schema
                Entity entity = new Entity(schema.GUID);

                // 5. Assign values to the fields
                foreach(string key in values.Keys) {
                    Field field = schema.GetField(key);
                    if (field != null) {
                        // only IList is supported
                        IList<string> components = values[key].ToList();
                        entity.Set<IList<string>>(key,components);
                    }
                }

                // 6. Associate the entity with a Revit element
                using (Transaction t = 
                    new Transaction(dataStorage.Document, "Set Entity")) {
                    t.Start();
                    dataStorage.SetEntity(entity);
                    t.Commit();
                }
                return true;
            }
            catch (Exception ex) {
                Autodesk.Revit.UI.TaskDialog.Show("Exception", $"{ex.Message}");
                return false;
            }
        }
    }
}
