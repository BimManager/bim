using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Room = Autodesk.Revit.DB.Architecture.Room;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class RoomAreasHandlerCmd : Autodesk.Revit.UI.IExternalCommand
    {
        const string FLAT_NO_PARAM = "APARTMENT NUMBER";
        const string TOTAL_AREA_PARAM = "TOTAL AREA";
        const string TOTAL_AREA_SAW = "TOTAL AREA SANS WA";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
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
                var flatsGroupedByFlatNo = new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .WherePasses(new Autodesk.Revit.DB.Architecture.RoomFilter())
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Room>()
                    .GroupBy(rm => rm.LookupParameter(FLAT_NO_PARAM).AsString());

                IList<Apartment> aptsOnFloor = new List<Apartment>();

                StringBuilder strBld = new StringBuilder();

                foreach(var grp in flatsGroupedByFlatNo) {
                    string number = grp.Key;
                    Apartment apt = new Apartment(doc, number);
                    foreach(Room rm in grp) {
                        apt.AddRoom(rm);
                    }
                    aptsOnFloor.Add(apt);
                    strBld.AppendLine(apt.ToString());
                }
                System.Diagnostics.Trace.Write(strBld.ToString());

                using(Transaction t = new Transaction(doc, "Set Area Parameters")) {
                    t.Start();
                    foreach(Apartment apt in aptsOnFloor) {
                        foreach(int id in apt.GetIds()) {
                            Room rm = doc.GetElement(new ElementId(id)) as Room;
                            rm.LookupParameter(TOTAL_AREA_SAW)
                                .Set(apt.AreaNoWetAreas);
                            rm.LookupParameter(TOTAL_AREA_PARAM)
                                .Set(apt.TotalArea);
                        }
                    }
                    t.Commit();
                }

                TaskDialog.Show("Success", "The task has been completed successfully.");

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
