using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace TektaRevitPlugins.Multischedule
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    class CmdConstructMultischedule : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Add an EventLogTraceListener object
            System.Diagnostics.Trace.Listeners.Add(
                new System.Diagnostics.EventLogTraceListener("Application"));

            // Lay the hands on the active document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // For reporting exceptions
            StringBuilder reporter = new StringBuilder();

            // Set up a timer
            Timing myTimer = new Timing();
            myTimer.StartTime();
            try
            {
                // Try retrieving partitions, host marks and assemblies from the repository (schema)
                Schema schema = ExtensibleStorageUtils.GetSchema(UpdateRepository.SCHEMA_GUID);
                if (schema == null)
                {
                    TaskDialog.Show("Message", "Please update the repository.");
                    return Result.Failed;
                }
                DataStorage dataStorage =
                    ExtensibleStorageUtils.GetDataStorage(doc, UpdateRepository.DATA_STORAGE_NAME);

                IDictionary<string, ISet<string>> partitionHostMarks = null;
                IDictionary<string, ISet<string>> hostMarkAssemblies = null;
                IDictionary<string, ISet<string>> partsStrTypes = null;
                try
                {
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

                    partsStrTypes =
                        ExtensibleStorageUtils.GetValues(
                        schema,
                        dataStorage,
                        UpdateRepository.FN_PARTS_STR_TYPES);

                    if (partitionHostMarks.Count == 0 ||
                        hostMarkAssemblies.Count == 0)
                        throw new Exception("No rebars have been detected.");
                }
                catch (UpdateRepositoryException ex)
                {
                    TaskDialog.Show("Warning", ex.Message);
                    return Result.Failed;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Warning", ex.Message);
                    return Result.Failed;
                }

                // Create a window and subscribe to its event
                WndMultitableSchedule wnd =
                    new WndMultitableSchedule(
                        partitionHostMarks, 
                        hostMarkAssemblies,
                        partsStrTypes);

                wnd.OkClicked += (sender, args) =>
                {
                    // Turn out a multi-table schedule
                    IMultitableSchedule multiSchd =
                    MultitableScheduleFactory.CreateMultiTableSchedule(
                        args.MultischeduleType,
                        args.ParametersValues
                        );

                    // Produce a manip   
                    MultischeduleManipulator manip = new MultischeduleManipulator(doc);

                    // 1. Populate the subschedule
                    manip.SetSubschedules(multiSchd);

                    // 2. Find Existing Schedules
                    manip.FindExistingSchedules(multiSchd);

                    using (Transaction t = new Transaction(doc, "Produce a Multischedule"))
                    {
                        t.Start();
                        for (int i = 0; i < multiSchd.Templates.Count; ++i)
                        {
                            System.Diagnostics.Trace.Write(string.Format("ID:{0}; VN:{1}; ",
                                multiSchd.Templates[i].Id, multiSchd.Templates[i].ViewName));

                            // Duplicate template
                            manip.DuplicateSchedules(multiSchd.Templates[i]);

                            // Rename template
                            manip.RenameSchedules(multiSchd.Templates[i]);

                            // Update the filters of the template
                            manip.UpdateFilters(multiSchd.Templates[i], multiSchd);

                            // Discard an empty template
                            if (manip.DiscardEmptySchedule(multiSchd.Templates[i]))
                            {
                                continue;
                            }

                            // Show the title
                            manip.ShowTitle(multiSchd.Templates[i]);

                            // Set Block Number and Partition to certain values
                            manip.SetBlockAndPartition(multiSchd.Templates[i], multiSchd);
                        }

                        // Schedule-specific settings
                        manip.CarryOutSpecificOperations(multiSchd);

                        // Change the title
                        manip.SetNewTitle(multiSchd);

                        t.Commit();

                        // Place on a sheet if the active view is of DrawingSheet type
                        if (doc.ActiveView.ViewType == ViewType.DrawingSheet)
                        {
                            t.Start();
                            manip.PlaceOnSheet((ViewSheet)doc.ActiveView, multiSchd);
                            t.Commit();
                        }
                    }
                };

                // Show the window
                try
                {
                    wnd.ShowDialog();
                }
                finally
                {
                    wnd.Close();
                }

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception",
                  string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                System.Diagnostics.Trace.Write(string.Format("{0}\n{1}",
                  ex.Message, ex.StackTrace));
                return Result.Failed;
            }
            finally
            {
                myTimer.StopTime();
                System.Diagnostics.Trace
                    .Write(string.Format("Time elapsed: {0}s",
                  myTimer.Duration.TotalSeconds));
            }
        }
    }
}
