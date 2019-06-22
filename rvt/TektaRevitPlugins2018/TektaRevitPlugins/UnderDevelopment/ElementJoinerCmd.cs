using System;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ElementJoinerCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Tracing settings
            Trace.Listeners.Add(new EventLogTraceListener("Application"));
            TraceSwitch traceSwitch =
                new TraceSwitch("JoinerListener", "Join structural elements command");
            traceSwitch.Level = TraceLevel.Verbose;

            // Get access to the active uidoc and doc
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = commandData.Application.ActiveUIDocument.Document;

            // Time the execution
            Timing timer = new Timing();
            timer.StartTime();

            try {
                IList<Element> elemsToJoin =
                    GetJoinableElems(doc);

                foreach (Element elem in elemsToJoin) {
                    Trace.Write(string.Format("Element: {0}; elemsToJoin = {1}",
                        elem.Name, elemsToJoin.Count));
                    JoinAllIntrudingElems(doc, elem);
                }
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Result.Cancelled;
            }
            catch (Exception ex) {
                TaskDialog.Show("Exception", string.Format("Message: {0}\nStackTrace: {1}",
                    ex.Message, ex.StackTrace));
                return Result.Failed;
            }
            finally {
                timer.StopTime();
                Trace.Write("Time elapsed: " + timer.Duration.Seconds + "s");
            }
        }

        static IList<Element> GetIntersectSolidElems(Document doc, Element elem)
        {
            Solid solid = RvtGeometryUtils.GetSolid(elem);
            if (solid == null)
                throw new System.ArgumentNullException("GetSolid(elem)==null");
            FilteredElementCollector collector =
                new FilteredElementCollector(doc);
            ElementIntersectsSolidFilter intrudingElemFilter =
                new ElementIntersectsSolidFilter(solid, false);
            ExclusionFilter exclusionFilter =
                new ExclusionFilter(new List<ElementId> { elem.Id });

            IList<ElementFilter> strElemFilters =
                new List<ElementFilter> { new ElementCategoryFilter(BuiltInCategory.OST_Columns),
                                           new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming),
                                           new ElementCategoryFilter(BuiltInCategory.OST_Floors),
                                           new ElementCategoryFilter(BuiltInCategory.OST_Walls)
                                        };

            LogicalOrFilter anyStrElemFlt =
                new LogicalOrFilter(strElemFilters);

            ICollection<Element> envadingElems = collector
                .WherePasses(exclusionFilter)
                .WherePasses(intrudingElemFilter)
                .WherePasses(anyStrElemFlt)
                .WhereElementIsNotElementType()
                .ToElements();

            return envadingElems.ToList();
        }

        public unsafe static IList<Element> GetIntersectBoundingBoxElems(Document doc, Element elem)
        {
            IntPtr ptr = new IntPtr();
            void* pv = ptr.ToPointer();

            BoundingBoxXYZ bndBx = elem.get_Geometry
                (new Options { IncludeNonVisibleObjects = true }).GetBoundingBox();

            BoundingBoxIntersectsFilter bndBxFlt =
                new BoundingBoxIntersectsFilter(new Outline(bndBx.Min, bndBx.Max), 1);

            IList<ElementFilter> strElemFilters =
                new List<ElementFilter> { new ElementCategoryFilter(BuiltInCategory.OST_Columns),
                                           new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming),
                                           new ElementCategoryFilter(BuiltInCategory.OST_Floors),
                                           new ElementCategoryFilter(BuiltInCategory.OST_Walls)
                                        };

            LogicalOrFilter anyStrElemFlt =
                new LogicalOrFilter(strElemFilters);

            IList<Element> elements = new FilteredElementCollector(doc)
                    .WherePasses(bndBxFlt)
                    .WherePasses(anyStrElemFlt)
                    .ToElements()
                    .ToList();

            return elements;
        }

        public static IList<Element> GetWallsIntersectBoundingBox(Document doc, Element elem)
        {
            BoundingBoxXYZ bndBx = elem.get_Geometry
                (new Options { IncludeNonVisibleObjects = true }).GetBoundingBox();

            BoundingBoxIntersectsFilter bndBxFlt =
                new BoundingBoxIntersectsFilter(new Outline(bndBx.Min, bndBx.Max), 1);

            IList<Element> elements = new FilteredElementCollector(doc)
                    .OfClass(typeof(Wall))
                    .WherePasses(bndBxFlt)
                    .ToElements()
                    .ToList();

            return elements;
        }

        private static IList<Element> GetJoinableElems(Document doc)
        {
            LogicalOrFilter categories =
                new LogicalOrFilter(new List<ElementFilter> {
                    new ElementCategoryFilter(BuiltInCategory.OST_Walls),
                    new ElementCategoryFilter(BuiltInCategory.OST_Floors),
                });
            return new FilteredElementCollector(doc)
                .WherePasses(categories)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();
        }

        //new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns),
        //new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming)

        static void JoinAllIntrudingElems(Document doc, Element elem)
        {
            IList<Element> intruders = GetWallsIntersectBoundingBox(doc, elem);
            Trace.Write("GetIntersetBoundingBoxElems.Count = "
                + intruders.Count);
            int loops = 0;

            Transaction:
            if (intruders.Count > 0) {
                using (Transaction t = new Transaction(doc, "Join all elements")) {
                    FailureHandlingOptions failureHandling =
                        t.GetFailureHandlingOptions();
                    failureHandling.SetFailuresPreprocessor(new AllWarningSwallower());
                    t.SetFailureHandlingOptions(failureHandling);
                    t.Start();
                    foreach (Element intruder in intruders) {
                        try {
                            JoinGeometryUtils.JoinGeometry
                                (doc, elem, intruder);
                        }
                        catch (Exception) {
                            continue;
                        }
                    }
                    t.Commit();
                }
            }

            intruders = GetIntersectSolidElems(doc, elem);
            Trace.Write("GetIntersectSolidElems.Count = "
                + intruders.Count);

            if (intruders.Count!=0 && loops < 1) {
                ++loops;
                goto Transaction;
            }
        }
    }

    /*[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class BoundingBoxIntesector : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get access to the active uidoc and doc
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = uidoc.Selection;

            try {

                Reference r = sel.PickObject(ObjectType.Element);
                Element elem = doc.GetElement(r);

                IList<ElementId> elemsWithinBBox =
                    ElementJoinerCmd.GetWallsIntersectBoundingBox(doc, elem)
                    .Select(e => e.Id)
                    .ToList();

                sel.SetElementIds(elemsWithinBBox);
                
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Result.Cancelled;
            }
            catch (Exception ex) {
                TaskDialog.Show("Exception", string.Format("Message: {0}\nStackTrace: {1}",
                    ex.Message, ex.StackTrace));
                return Result.Failed;
            }
        }
    }*/
}
