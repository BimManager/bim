using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ExcelExporterCmd : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Add an EventLogTraceListener object
            Trace.Listeners.Add(new EventLogTraceListener("Application"));

            // Lay hands on the active document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Timing myTimer = new Timing();
            myTimer.StartTime();
            try {
                // Show the export window
                ExportImportExcel exportWindow =
                    new ExportImportExcel(GetAllSchedules(doc));
                exportWindow.ShowDialog();
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Result.Cancelled;
            }
            catch (Exception ex) {
                TaskDialog.Show("Exception",
                    string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                return Result.Failed;
            }
            finally {
                myTimer.StopTime();
                Trace.Write(string.Format("Time elapsed: {0}s", 
                    myTimer.Duration.TotalSeconds));
            }
        }

        #region Helper Methods
        private static IList<ViewSchedule> GetAllSchedules(Document doc)
        {
            ElementParameterFilter elemNameFilter =
                new ElementParameterFilter(ParameterFilterRuleFactory
                .CreateNotContainsRule(new ElementId(BuiltInParameter.VIEW_NAME),
                "<Revision Schedule>", false));

            return new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSchedule))
                    .WhereElementIsNotElementType()
                    .WherePasses(elemNameFilter)
                    .Cast<ViewSchedule>()
                    .ToList()
                    .OrderBy(s => s.Name)
                    .ToList();
        }
        #endregion
    }
}
