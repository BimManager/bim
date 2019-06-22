using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tracer = System.Diagnostics.Trace;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class SetOpeningPropsCmd : Autodesk.Revit.UI.IExternalCommand
    {
        const string SO_BTM_ELEV = "S/O BTM ELEV";
        const string SO_FAMILY_NAME = "Z-M3-WAllBasedApertureMaker-LOD2";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Tracer.Listeners.Add(new System.Diagnostics.EventLogTraceListener("Application"));

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try {

                IList<FamilyInstance> openings =
                    new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .OfCategory(BuiltInCategory.OST_Windows)
                    .WherePasses(new ElementParameterFilter
                    (ParameterFilterRuleFactory.CreateEqualsRule
                    (new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME), 
                    SO_FAMILY_NAME, false)))
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .ToList();

                if (openings.Count == 0)
                    throw new Exception("No opening has been unearthed. Toodle-pip.");

                using(Transaction t = new Transaction(doc,"Mark Openings")) {
                    t.Start();
                    foreach(FamilyInstance fi in openings) {
                        if (fi.LookupParameter(SO_BTM_ELEV) == null)
                            throw new Exception(string.Format("{0} is null", nameof(SO_BTM_ELEV)));
                        double elev = ((Level)doc.GetElement(fi.LevelId)).Elevation + fi.get_Parameter
                            (BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsDouble();
                        fi.LookupParameter(SO_BTM_ELEV)
                            .Set(elev);
                    }
                    t.Commit();
                }
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Result.Cancelled;
            }
            catch(Exception ex) {
                Tracer.Write(string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                return Result.Failed;
            }
        }
    }
}
