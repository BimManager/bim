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
    class SaveRebarsCmd : Autodesk.Revit.UI.IExternalCommand
    {
        readonly Guid SCHEMA_GUID = new Guid("6F354CFE-CA0D-4383-835B-8CF855F7518A");
        const string SCHEMA_NAME = "RebarHandler";
        const string DATA_STORAGE_NAME = "6F354CFE";
        const string FIELD_NAME = "Rebars";

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
                IDictionary<string, ISet<string>> values =
                    RebarsUtils.GetPartitionHostMarkDic(doc);
                Schema schema =
                    ExtensibleStorageUtils.GetSchema(SCHEMA_GUID, SCHEMA_NAME, values);
                DataStorage dataStorage =
                    ExtensibleStorageUtils.GetDataStorage(doc, DATA_STORAGE_NAME);

                if (ExtensibleStorageUtils
                    .AssignValues(schema, 
                    dataStorage,
                    values)) {
                    TaskDialog.Show("Success!", "Everything has gone smoothly.");
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
