using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    class SplitterCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            try
            {
                // Propmt the user to select a wall
                Reference r = sel.PickObject(ObjectType.Element, new VerticalElementsSelectionFilter(),
                                           "Please select a column or wall");
                Element elem = doc.GetElement(r);

                SplittingVerticalElementsUtils.Split(doc, elem.Id);

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
