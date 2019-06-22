using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class CalculateFlatAreaCmd : Autodesk.Revit.UI.IExternalCommand
    {
        const string BLOCK_PARAMETER = "RM_BLOCK";
        const string FLAT_NO_PARAMETER = "RM_FLAT_NO";
        const string FLAT_AREA_PARAMETER = "RM_FLAT_AREA";
        const string FLAT_AREA_RDC_PARAMETER = "RM_FLAT_AREA_RDC";
        const string ROOM_AREA_RDC_PARAMETER = "RM_ROOM_AREA_RDC";
        const string REDN_COEFF_PARAMETER = "RM_REDN_FACTOR";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            // Add an EventLogTraceListener object
            System.Diagnostics.Trace.Listeners.Add(
               new System.Diagnostics.EventLogTraceListener("Application"));

            // Lay the hands on the active document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // ensure that the shared parameters are defined in the current doc
            int foundParams = 0;
            List<ElementId> paramIds = new List<ElementId>();
            DefinitionBindingMapIterator itr = doc.ParameterBindings.ForwardIterator();
            while (itr.MoveNext()) {
                InternalDefinition def = itr.Key as InternalDefinition;
                if (def.Name == BLOCK_PARAMETER ||
                    def.Name == FLAT_NO_PARAMETER ||
                    def.Name == FLAT_AREA_PARAMETER ||
                    def.Name == FLAT_AREA_RDC_PARAMETER ||
                    def.Name == ROOM_AREA_RDC_PARAMETER ||
                    def.Name == REDN_COEFF_PARAMETER) {
                    ++foundParams;

                    if (def.Name != FLAT_AREA_PARAMETER &&
                        def.Name != FLAT_AREA_RDC_PARAMETER &&
                        def.Name != ROOM_AREA_RDC_PARAMETER &&
                        def.Name != REDN_COEFF_PARAMETER)
                        paramIds.Add(def.Id);
                }
            }
            if (foundParams < 6) {
                TaskDialog.Show("Error",
                    "Either not all of the required parameters are defined in the document or not all of the necessary parameters are assigned values." +
                    ".\n\nPlease ensure that the following parameters exit:" +
                    $"\n\n{BLOCK_PARAMETER} - shall not be empty.\n{FLAT_NO_PARAMETER} - shall not be empty.\n{FLAT_AREA_PARAMETER}" +
                    $"\n{FLAT_AREA_RDC_PARAMETER}\n{ROOM_AREA_RDC_PARAMETER}\n{REDN_COEFF_PARAMETER}");
                return Result.Failed;
            }

            // Set up a timer
            Timing myTimer = new Timing();
            myTimer.StartTime();
            List<ElementFilter> parameters =
                new List<ElementFilter>();
            try
            {
                foreach (ElementId paramId in paramIds)
                {
                    parameters.Add(new ElementParameterFilter
                        (ParameterFilterRuleFactory
                            .CreateNotEqualsRule(paramId, "", false)));
                }

                LogicalAndFilter andFilter = new LogicalAndFilter(parameters);

                var elementsCollected = new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .WherePasses(new Autodesk.Revit.DB.Architecture.RoomFilter())
                    .WherePasses(andFilter)
                    .ToElements()
                    .Cast<Autodesk.Revit.DB.Architecture.Room>()
                    .ToList();

                if (elementsCollected.Count == 0) {
                    TaskDialog.Show("Error", "No room has been detected.");
                    return Result.Failed;
                }

                var groupedByBlockNo = from elem in elementsCollected
                                       group new {
                                           elem.Id,
                                           elem.Area,
                                           FlatNo = elem.LookupParameter(FLAT_NO_PARAMETER).AsString(),
                                           Factor =
                                           elem.LookupParameter(REDN_COEFF_PARAMETER).AsDouble() != 0 ?
                                           elem.LookupParameter(REDN_COEFF_PARAMETER).AsDouble() : 1
                                       }
                                       by elem.LookupParameter(BLOCK_PARAMETER).AsString()
                                       into grp
                                       select grp;

                foreach (var grpByBlock in groupedByBlockNo) {
                    var groupedByFlatNo = from elem in grpByBlock
                                          group elem by elem.FlatNo
                                          into grpByFlat
                                          select grpByFlat;

                    using (Transaction t = new Transaction(doc, "Set Room & Flat Areas")) {
                        t.Start();
                        foreach (var grp in groupedByFlatNo) {
                            double area = 0;
                            double reducedArea = 0;

                            foreach (var rm in grp) {
                                area += rm.Area;
                                reducedArea += rm.Area * rm.Factor;
                                doc.GetElement(rm.Id)
                                    .LookupParameter(ROOM_AREA_RDC_PARAMETER)
                                    .Set(rm.Area * rm.Factor);
                            }
                            foreach (var rm in grp) {
                                Element rmAsElem = doc.GetElement(rm.Id);
                                rmAsElem.LookupParameter(FLAT_AREA_PARAMETER).Set(area);
                                rmAsElem.LookupParameter(FLAT_AREA_RDC_PARAMETER).Set(reducedArea);
                            }
                        }
                        t.Commit();
                    }
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

        bool DoesExist(Document doc, string paramName)
        {
            DefinitionBindingMapIterator itr =
                doc.ParameterBindings.ForwardIterator();

            while (itr.MoveNext())
            {
                if(itr.Key.Name == paramName)
                    return true;
            }
            return false;
        }

    }
}
