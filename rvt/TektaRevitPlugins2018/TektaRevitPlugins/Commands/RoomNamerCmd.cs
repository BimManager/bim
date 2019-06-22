using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using RvtRoom = Autodesk.Revit.DB.Architecture.Room;


namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class RoomNamerCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Add an EventLogTraceListener
            Trace.Listeners.Add(new EventLogTraceListener("Application"));

            // Gain access to the document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try {
                Reference r = uidoc.Selection.PickObject(ObjectType.Element, 
                    new RoomSelectionFilter());

                RvtRoom pickedRoom = doc.GetElement(r) as RvtRoom;

                RoomProperties window = new RoomProperties();
                window.ShowDialog();
                IDictionary<string,string> values= window.Values;
                if (values == null)
                    return Result.Cancelled;

                using(Transaction t=new Transaction(doc,"Name room")) {

                    if(t.Start()==TransactionStatus.Started) {

                        foreach(string key in values.Keys) {
                            if(pickedRoom.LookupParameter(key)!=null) {
                                pickedRoom.LookupParameter(key).Set(values[key]);
                            }
                        }

                        if(t.Commit()==TransactionStatus.Committed) {

                        }
                        else {
                            t.RollBack();
                            TaskDialog.Show("Transaction Failed",
                            "Name room transaction fell through.");
                        }

                    }
                    else {
                        TaskDialog.Show("Transaction Failed", 
                            "Name room transaction failed to start.");
                    }
                }
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Result.Cancelled;
            }
            catch(Exception ex) {
                TaskDialog.Show("Command Exception",
                    string.Format("{0}\n{1]", ex.Message, ex.StackTrace));
                return Result.Failed;
            }
            finally {

            }
        }
    }

    
}
