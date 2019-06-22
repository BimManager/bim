using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Trace = System.Diagnostics.Trace;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    class TagElemsCmd : IExternalCommand
    {
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
                if(doc.ActiveView.ViewType == ViewType.ThreeD && 
                    !((View3D)doc.ActiveView).IsLocked) {
                    TaskDialog.Show("Warning", "No tag shall be placed in an unlocked 3D view.");
                    return Result.Failed;
                }

                Reference refTag = uidoc.Selection.PickObject(
                    Autodesk.Revit.UI.Selection.ObjectType.Element,
                    new TagSelectionFilter(), "Please pick out a tag.");

                IndependentTag extTag = doc.GetElement(refTag) as IndependentTag;

                IList<Reference> elemsToTag = 
                    uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, "Please choose elements to tag.");

                using (Transaction t = new Transaction(doc, "Spawn new tags")) {
                    t.Start();
                    foreach (Reference r in elemsToTag) {
                        IndependentTag newTag = IndependentTag.Create(
                            doc, doc.ActiveView.Id, r, true,
                            TagMode.TM_ADDBY_CATEGORY, extTag.TagOrientation, extTag.TagHeadPosition);
                        newTag.TagHeadPosition = extTag.TagHeadPosition;
                        if (extTag.HasElbow) newTag.LeaderElbow = extTag.LeaderElbow;
                        if (extTag.LeaderEndCondition == LeaderEndCondition.Free) {
                            newTag.LeaderEndCondition = LeaderEndCondition.Free;
                            Element elemToTag = doc.GetElement(r);
                            XYZ locPnt = ((LocationPoint)elemToTag.Location).Point;
                            newTag.LeaderEnd = new XYZ(locPnt.X, locPnt.Y, extTag.LeaderEnd.Z);
                        }
                    }
                    t.Commit();
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
