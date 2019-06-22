using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB.ExtensibleStorage;
using Document = Autodesk.Revit.DB.Document;
using Transaction = Autodesk.Revit.DB.Transaction;

namespace TektaRevitPlugins
{
    internal static class ExtensibleStorageUtils
    {
        const string VENDOR_ID = "ADSK";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="extraDetails">extraDetails[0] holds the schema's name,
        /// with other array elements naming the fields</param>
        /// <returns></returns>
        internal static Schema GetSchema(Guid guid, params string[] extraDetails)
        {
            // 1. Try to look up the schema
            Schema schema = Schema.Lookup(guid);

            if (schema == null &&
                extraDetails.Length > 2) {
                // 2. Create and name a new schema
                SchemaBuilder schemaBuilder = new SchemaBuilder(guid);
                schemaBuilder.SetSchemaName(extraDetails[0]);

                // 3. Set read/write access for the schema 
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Vendor);
                schemaBuilder.SetVendorId(VENDOR_ID);

                // 4. Define one or more fields of data
                for (int i = 1; i < extraDetails.Length; ++i)
                {
                    schemaBuilder.AddMapField(
                        extraDetails[i], typeof(string), typeof(string));
                }
                schema = schemaBuilder.Finish();
            }
            return schema;
        }

        internal static DataStorage GetDataStorage(Document doc, string dsName)
        {
            DataStorage dataStorage =
                new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(DataStorage))
                .Where(e => e.Name == dsName)
                .Cast<DataStorage>()
                .FirstOrDefault();

            if (dataStorage == null) {
                using (Transaction t = new Transaction(doc)) {
                    t.Start("Create Data Storage");
                    dataStorage = DataStorage.Create(doc);
                    dataStorage.Name = dsName;
                    t.Commit();
                }
            }
            return dataStorage;
        }

        internal static void AssignValues(
                Schema schema,
                DataStorage dataStorage,
                string fieldPartsHosts,
                string fieldHostsAssemblies,
                string fieldPartsStrTypes,
                IDictionary<string, ISet<string>> partHostValues,
                IDictionary<string, ISet<string>> hostAssemblyValues,
                SortedList<string, SortedSet<string>> partsStrTypes)
        {
            // 5. Create an entity based on the schema
            Entity entity = new Entity(schema);

            // 6. Assign values to the fields for the entity
            entity.Set<IDictionary<string, string>>(
                fieldPartsHosts, ConvertToSimpleDic(partHostValues));
            entity.Set<IDictionary<string, string>>(
                fieldHostsAssemblies, ConvertToSimpleDic(hostAssemblyValues));
            entity.Set<IDictionary<string, string>>(
                fieldPartsStrTypes, ConvertToSimpleDic(partsStrTypes));

            // 7. Associate the entity with a revit element
            using (Transaction t = new Transaction(dataStorage.Document))
            {
                t.Start("Save Data");
                dataStorage.SetEntity(entity);
                t.Commit();
            }
        }

        internal static IDictionary<string, ISet<string>> GetValues(
                        Schema schema,
                        DataStorage dataStorage,
                        string fieldName)
        {
            return ConvertFromSimpleDic(
                dataStorage.GetEntity(schema)
                .Get<IDictionary<string, string>>(fieldName));
        }

        static IDictionary<string, string> ConvertToSimpleDic(
            IDictionary<string, ISet<string>> inDic)
        {
            IDictionary<string, string> outDic =
                new Dictionary<string, string>();
            foreach(string key in inDic.Keys) {
                string values = string.Empty;
                foreach (string val in inDic[key])
                    values += val + ";";
                outDic.Add(key, values);
            }
            return outDic;
        }
        static IDictionary<string, string> ConvertToSimpleDic(
            SortedList<string, SortedSet<string>> inDic)
        {
            IDictionary<string, string> outDic =
                new Dictionary<string, string>();
            foreach (string key in inDic.Keys)
            {
                string values = string.Empty;
                foreach (string val in inDic[key])
                    values += val + ";";
                outDic.Add(key, values);
            }
            return outDic;
        }

        static IDictionary<string, ISet<string>> ConvertFromSimpleDic(
            IDictionary<string, string> inDic)
        {
            IDictionary<string, ISet<string>> outDic =
                new SortedDictionary<string, ISet<string>>();
            foreach(string key in inDic.Keys) {
                ISet<string> values = new SortedSet<string>();
                foreach(string val in inDic[key].Split(';')) {
                    if (val.Length != 0)
                        values.Add(val);
                }
                outDic.Add(key, values);
            }
            return outDic;
        }
    }
}
