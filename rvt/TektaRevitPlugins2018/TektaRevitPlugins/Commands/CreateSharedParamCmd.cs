using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class CreateSharedParamCmd : Autodesk.Revit.UI.IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData, 
            ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            System.Diagnostics.Trace.Listeners.
                Add(new System.Diagnostics.EventLogTraceListener("Application"));

            Autodesk.Revit.UI.UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
      
            try {
                CreateSharedParamWin hwnd =
                    new CreateSharedParamWin(doc);
                hwnd.ShowDialog();

                if (hwnd.ParameterName == null || 
                    hwnd.ParameterName.Length == 0)
                    return Autodesk.Revit.UI.Result.Cancelled;

                SharedParametersManager spManager = 
                    new SharedParametersManager(doc);
                spManager.CreateSharedParameter(
                    hwnd.ParameterName,
                    hwnd.ParameterType,
                    new List<BuiltInCategory>
                    { hwnd.Category },
                    hwnd.ParameterGroup,
                    hwnd.IsInstance,
                    hwnd.IsModifiable,
                    hwnd.IsVisible,
                    hwnd.GroupName
                    );

                if (hwnd.CanVaryBtwGroups)
                    spManager.CanVaryBtwGroups(hwnd.ParameterName, true);

                return Autodesk.Revit.UI.Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Autodesk.Revit.UI.Result.Cancelled;
            }
            catch(System.Exception ex) {
                Autodesk.Revit.UI.TaskDialog.Show("Exception",
                    string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                return Autodesk.Revit.UI.Result.Failed;
            }
        }
    }
}
