using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.ExtensibleStorage;

using Tracer = System.Diagnostics.Trace;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class UpdateRepository : Autodesk.Revit.UI.IExternalCommand
    {
        internal static readonly Guid SCHEMA_GUID = new Guid("E5D35FEE-2F4D-4A4A-B38B-4944E7BD0EAF");
        internal static readonly Guid PRECURSORY_GUID = new Guid("6F354CFE-CA0D-4383-835B-8CF855F7518A");
        const string SCHEMA_NAME = "Repository";
        internal const string DATA_STORAGE_NAME = "7F354CFE";
        internal const string FOREGOING_DATA_STORAGE_NAME = "6F354CFE";
        internal const string FN_PARTS_HOST_MARKS = "PHM";
        internal const string FN_HOST_MARKS_ASSEMBLIES = "HMA";
        internal const string FN_PARTS_STR_TYPES = "PST";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Add an EventLogTraceListener object
            System.Diagnostics.Trace.Listeners.Add(
               new System.Diagnostics.EventLogTraceListener("Application"));

            // Lay the hands on the active document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Set up a timer
            Timing myTimer = new Timing();
            myTimer.StartTime();
            try {
                // Track down all partitions as well as map them
                // to host marks
                IDictionary<string, ISet<string>> partHostValues =
                    RebarsUtils.GetPartitionHostMarkDic(doc);

                RebarContainer rebarContainer = new RebarContainer();
                foreach(FamilyInstance fi in RebarsUtils.GetAllRebarsInDoc(doc))
                {
                    GeneralRebar rebar = new GeneralRebar(fi);
                    rebarContainer.Add(rebar);
                }

                // Dig up all host marks and map them
                // to assemblies
                IDictionary<string, ISet<string>> hostAssemblyValues =
                    rebarContainer.GetHostAssemblies();

                // Get hold of all partitions and
                // map them to structural types
                SortedList<string, SortedSet<string>> partsStrTypes = 
                    TektaRevitPlugins.Multischedule.MultischeduleUtils.DigUpPartsStrTypes(doc);
                
                Schema schema =
                    ExtensibleStorageUtils.GetSchema(
                        SCHEMA_GUID,SCHEMA_NAME, 
                        FN_PARTS_HOST_MARKS, 
                        FN_HOST_MARKS_ASSEMBLIES,
                        FN_PARTS_STR_TYPES);

                DataStorage dataStorage =
                    ExtensibleStorageUtils.GetDataStorage(
                        doc, DATA_STORAGE_NAME);

                ExtensibleStorageUtils
                    .AssignValues(schema,
                    dataStorage,
                    FN_PARTS_HOST_MARKS,
                    FN_HOST_MARKS_ASSEMBLIES,
                    FN_PARTS_STR_TYPES,
                    partHostValues,
                    hostAssemblyValues,
                    partsStrTypes);

                TaskDialog.Show("Success!", "Repository has been updated.");
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Result.Cancelled;
            }
            catch (Exception ex) {
                TaskDialog.Show("Exception",
                  string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                System.Diagnostics.Trace.Write(string.Format("{0}\n{1}",
                  ex.Message, ex.StackTrace));
                return Result.Failed;
            }
            finally {
                myTimer.StopTime();
                System.Diagnostics.Trace
                    .Write(string.Format("Time elapsed: {0}s",
                  myTimer.Duration.TotalSeconds));
            }
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    class DiscardRepository : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            try
            {
                Schema schema = Schema.Lookup(UpdateRepository.PRECURSORY_GUID);

                using (Transaction t = new Transaction(doc, "Do away with the schema"))
                {
                    if (t.Start() == TransactionStatus.Started)
                    {

                        Element ds = ExtensibleStorageUtils
                            .GetDataStorage(doc, UpdateRepository.FOREGOING_DATA_STORAGE_NAME);

                        if (ds != null)
                        {
                            doc.Delete(ds.Id);
                            if (t.Commit() == TransactionStatus.Committed)
                            {
                                TaskDialog.Show("Info", "The data storage element has been discarded");
                            }
                            else
                            {
                                t.RollBack();
                                return Result.Failed;
                            }
                        }
                        else
                        {
                            TaskDialog.Show("Info", "No such data storage element in the document.");
                            t.RollBack();
                        }
                        
                        if (t.Start() == TransactionStatus.Started)
                        {

                            Schema.EraseSchemaAndAllEntities(schema, true);

                            if (t.Commit() == TransactionStatus.Committed)
                            {
                                TaskDialog.Show("Schema Remove", "The schema has been disposed of.");
                            }
                        }
                        else
                        {
                            t.RollBack();
                            return Result.Failed;
                        }
                    }
                    else
                    {
                        return Result.Failed;
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception",
                  string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                System.Diagnostics.Trace.Write(string.Format("{0}\n{1}",
                  ex.Message, ex.StackTrace));
                return Result.Failed;
            }
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ReadRepository : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Add an EventLogTraceListener object
            System.Diagnostics.Trace.Listeners.Add(
               new System.Diagnostics.EventLogTraceListener("Application"));

            // Lay the hands on the active document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Set up a timer
            Timing myTimer = new Timing();
            myTimer.StartTime();
            try
            {
                int i = 0;
                string partsHostMarks = UpdateRepository.FN_PARTS_HOST_MARKS;
                while (i < 2)
                {
                    IDictionary<string, ISet<string>> values =
                    ExtensibleStorageUtils.GetValues(
                        ExtensibleStorageUtils.GetSchema(UpdateRepository.SCHEMA_GUID),
                        ExtensibleStorageUtils.GetDataStorage(doc, UpdateRepository.DATA_STORAGE_NAME),
                        partsHostMarks);

                    StringBuilder strBld = new StringBuilder();

                    foreach (string key in values.Keys) {
                        strBld.AppendLine(key);
                        foreach (string value in values[key])
                            strBld.Append(string.Format("{0} ", value));
                        strBld.AppendLine();
                    }
                    Tracer.Write(strBld.ToString());
                    strBld.Clear();
                    ++i;
                    partsHostMarks = UpdateRepository.FN_HOST_MARKS_ASSEMBLIES;
                }

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Result.Cancelled;
            }
            catch (Exception ex) {
                TaskDialog.Show("Exception",
                  string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                System.Diagnostics.Trace.Write(string.Format("{0}\n{1}",
                  ex.Message, ex.StackTrace));
                return Result.Failed;
            }
            finally {
                myTimer.StopTime();
                System.Diagnostics.Trace
                    .Write(string.Format("Time elapsed: {0}s",
                  myTimer.Duration.TotalSeconds));
            }
        }
    }
}
