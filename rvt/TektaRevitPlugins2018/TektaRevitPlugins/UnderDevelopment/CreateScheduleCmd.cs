using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Tracer = System.Diagnostics.Trace;
using StringBuilder = System.Text.StringBuilder;
using Schema = Autodesk.Revit.DB.ExtensibleStorage.Schema;
using DataStorage = Autodesk.Revit.DB.ExtensibleStorage.DataStorage;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    class CreateScheduleCmd : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Add an EventLogTraceListener object
            Tracer.Listeners.Add(
                new System.Diagnostics.EventLogTraceListener("Application"));

            // Lay the hands on the active document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // set up a timer
            Timing myTimer = new Timing();
            myTimer.StartTime();
            try
            {
                // Try retrieving partitions, host marks and assemblies from the repository (schema)
                Schema schema = ExtensibleStorageUtils.GetSchema(UpdateRepository.SCHEMA_GUID);
                if(schema == null)
                {
                    TaskDialog.Show("Message", "Please update the repository.");
                    return Result.Failed;
                }
                DataStorage dataStorage =
                    ExtensibleStorageUtils.GetDataStorage(doc, UpdateRepository.DATA_STORAGE_NAME);

                IDictionary<string, ISet<string>> partitionHostMarks = null;
                IDictionary<string, ISet<string>> hostMarkAssemblies = null;
                try {
                    partitionHostMarks =
                    ExtensibleStorageUtils.GetValues(
                        schema,
                        dataStorage,
                        UpdateRepository.FN_PARTS_HOST_MARKS);

                    hostMarkAssemblies =
                    ExtensibleStorageUtils.GetValues(
                        schema,
                        dataStorage,
                        UpdateRepository.FN_HOST_MARKS_ASSEMBLIES);

                    if (partitionHostMarks.Count == 0 ||
                        hostMarkAssemblies.Count == 0)
                        throw new Exception("No rebars have been detected.");
                }
                catch(UpdateRepositoryException ex)
                {
                    TaskDialog.Show("Warning", ex.Message);
                    return Result.Failed;
                }
                catch(Exception ex)
                {
                    TaskDialog.Show("Warning", ex.Message);
                    return Result.Failed;
                }
                return Result.Failed;
                //WndMultitableSchedule wnd =
                        //new WndMultitableSchedule(partitionHostMarks, hostMarkAssemblies);

                /*wnd.OkClicked += (sender, args) => 
                {
                    ComboScheduleGenerator scheduleGenerator =
                        new ComboScheduleGenerator(
                        doc,
                        args.MultiTableScheduleType,
                        args.ParametersValues,
                        args.ShowTitle);

                    scheduleGenerator.CreateSubSchedules(doc);

                    if (doc.ActiveView.ViewType == ViewType.DrawingSheet)
                    {
                        scheduleGenerator.PlaceOnSheet((ViewSheet)doc.ActiveView);
                    }
                };
                try
                {
                    wnd.ShowDialog();
                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    wnd.Close();
                    throw ex;
                }*/
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (RenameException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception",
                  string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                Tracer.Write(string.Format("{0}\n{1}",
                  ex.Message, ex.StackTrace));
                return Result.Failed;
            }
            finally
            {
                myTimer.StopTime();
                Tracer.Write(string.Format("Time elapsed: {0}s",
                  myTimer.Duration.TotalSeconds));
            }
        }

        /*string StringifyDic<K, V, T>(IDictionary<K, V> dic) where V : ISet<T>
        {
            StringBuilder strBuilder =
                new StringBuilder();
            foreach(K k in dic.Keys) {
                strBuilder.AppendLine(k.ToString());
                foreach(T v in dic[k]) {    
                    strBuilder.AppendFormat($"{v}; ");
                }
            }
            return strBuilder.ToString();
        }*/
    }
}
