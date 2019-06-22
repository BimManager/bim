using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using RevitAppServices = Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevExceptions=Autodesk.Revit.Exceptions;
using RevitCreation = Autodesk.Revit.Creation;

namespace TektaRevitPlugins
{
    [Transaction(TransactionMode.Manual)]
    public class SplitterCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Trace.Listeners.Add(new EventLogTraceListener("Application"));
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            try
            {
                // Propmt the user to select a vertical element
                IList<Element> selectedElems = sel.PickObjects(ObjectType.Element, new VerticalElementsSelectionFilter(),
                                           "Please select a column or wall")
                                           .Select<Reference, Element>(r => doc.GetElement(r)).ToList();

                SplittingVerticalElementsUtils.SplitElements(commandData, selectedElems);

                return Result.Succeeded;
            }
            catch(RevExceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Exception", string.Format("StackTrace:\n{0}\nMessage:\n{1}",
                                                          ex.StackTrace, ex.Message));
                return Result.Failed;
            }
        }
    }
}
