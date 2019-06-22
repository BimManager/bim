using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TektaRevitPlugins.Commands
{
    using Trace = System.Diagnostics.Trace;

    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class DoorSwingDetectorCmd : Autodesk.Revit.UI.IExternalCommand
    {
        const string SWING_PARAMETER = "SWING_DIRECTION";

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
                // temporary line of code
                Trace.Write(System.Reflection.Assembly.GetCallingAssembly().GetName().Name);
                Trace.Write(System.Reflection.Assembly.GetCallingAssembly().GetName().FullName);

                // take care of shared parameters
                CheckNecessarySharedParam(doc, SWING_PARAMETER);

                // find all necessary elements
                IList<FamilyInstance> doors;
                GetDoors(doc, out doors);

                // Iterate over the collection, 
                // detecting the swing direction and
                // setting this value to every element
                for(int i = 0; i < doors.Count; ++i)
                {
                    string swingDirection;

                    DetectSwing(doors[i], out swingDirection);

                    SetSwingParameter(SWING_PARAMETER, swingDirection, doors[i]);
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

        #region Helper Methods
        void CheckNecessarySharedParam(Document doc, string paramName)
        {
            SharedParametersManager spMgr = new SharedParametersManager(doc);
            if (!spMgr.DoesParameterExist(paramName))
            {
                spMgr.CreateSharedParameter(
                    paramName,
                    ParameterType.Text,
                    new List<BuiltInCategory> { BuiltInCategory.OST_Doors },
                    BuiltInParameterGroup.PG_CONSTRAINTS,
                    true,
                    false,
                    true,
                    "Custom",
                    true,
                    new Guid("7BCDBE01-EFED-4210-AE27-3FED48DCDF08")
                    );
            }
        }

        void GetDoors(Document doc, out IList<FamilyInstance> doors)
        {
            doors = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<FamilyInstance>()
                .ToList();
        }

        void DetectSwing(FamilyInstance door, out string swing)
        {
            // 0 - handFlipped
            // 1 - facingFlipped
            // 2 - mirrored
            //int mirrored = door.Mirrored ? 1 : 0;
            int handFlipped = door.HandFlipped ? 1 : 0;
            int facingFlipped = door.FacingFlipped ? 1 : 0;

            int comb =
                handFlipped | (facingFlipped << 1);

            switch (comb)
            {
                case 0:
                    swing = "Левая";
                    break;
                case 1:
                    swing = "Правая";
                    break;
                case 2:
                    swing = "Правая";
                    break;
                case 3:
                    swing = "Левая";
                    break;
                default:
                    swing = "N/A";
                    break;
            }
        }

        void SetSwingParameter(
            string paramName,string value, FamilyInstance door)
        {
            using(Transaction t = 
                new Transaction(door.Document, "Set swing value"))
            {
                t.Start();
                door.LookupParameter(paramName).Set(value);
                t.Commit();
            }
        }
        #endregion
    }
}