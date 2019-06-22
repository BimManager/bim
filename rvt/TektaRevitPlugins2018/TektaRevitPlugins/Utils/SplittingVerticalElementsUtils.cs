using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitAppServices = Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using RevitExceptions = Autodesk.Revit.Exceptions;
using RevitCreation = Autodesk.Revit.Creation;

using System.Diagnostics;

namespace TektaRevitPlugins
{
    enum SelectionMethod { Manual, Automatic }

    static class SplittingVerticalElementsUtils
    {
        #region Static Data Fields
        static Element m_selectedElem;
        static Solid m_selectedElemSolid;
        static TraceSwitch generalSwitch =
            new TraceSwitch("General", "Entire Application");

        static IList<Element> m_interferingElems;
        #endregion

        public static void SplitElements(ExternalCommandData commanData, IList<Element> pickedElems)
        {
            UIDocument uidoc = commanData.Application.ActiveUIDocument;
            Document doc = commanData.Application.ActiveUIDocument.Document;

            TaskDialog taskDialog = CreateCustomTaskDialog();
            TaskDialogResult tResult = taskDialog.Show();

            if (tResult == TaskDialogResult.CommandLink1)
            {
                // Select interfering elements
                m_interferingElems = SelectInterferingElems(uidoc,doc);
                Trace.WriteIf(generalSwitch.TraceVerbose,
                    string.Format("The number of interfering elements selected is {0}",
                    m_interferingElems.Count));

                foreach (Element elem in pickedElems)
                    Split(doc, elem, SelectionMethod.Manual);

            }
            else if (tResult == TaskDialogResult.CommandLink2)
            {
                foreach (Element elem in pickedElems)
                    Split(doc, elem, SelectionMethod.Automatic);

                Trace.WriteIf(generalSwitch.TraceVerbose,
                    string.Format("The number of interfering elements discovered is {0}",
                    m_interferingElems.Count));
            }
            else
            {
                return;
            }
        }

        private static void Split(Document doc, Element selectedElem, SelectionMethod selectionMethod)
        {
            try
            {
                UnjoinElements(doc, selectedElem);

                if(selectionMethod==SelectionMethod.Automatic)
                    m_interferingElems
                        = GetInterferingFloors(doc, m_selectedElem);


                if (m_interferingElems.Count == 0)
                    return;

                if (m_selectedElem is Wall)
                {
                    WallSplitter(doc, m_selectedElem);
                }
                else if (m_selectedElem.Category.Id.IntegerValue ==
                    (int)BuiltInCategory.OST_StructuralColumns)
                {
                    ColumnSplitter(doc,m_selectedElem);
                }
                else
                {
                    // UNDER DEVELOPMENT 
                    return;
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Trace.WriteIf(generalSwitch.TraceError, string.Format("StackTrace:\n{0}\nMessage:\n{1}",
                                                          ex.StackTrace, ex.Message));
                TaskDialog.Show("Exception", string.Format("StackTrace:\n{0}\nMessage:\n{1}",
                                                          ex.StackTrace, ex.Message));
            }
            finally
            {
                m_interferingElems.Clear();
            }
        }

        static IList<Element> GetInterferingFloors(Document doc, Element elem)
        {
            Solid solid = RvtGeometryUtils.GetSolid(elem);
            if (solid == null)
                throw new ArgumentNullException("GetUppermostSolid(elem)==null");
            FilteredElementCollector collector =
                new FilteredElementCollector(doc);
            ElementIntersectsSolidFilter intrudingElemFilter =
                new ElementIntersectsSolidFilter(solid, false);
            ExclusionFilter exclusionFilter =
                new ExclusionFilter(new List<ElementId> { elem.Id });
            ICollection<Element> envadingFloors = collector
                .OfClass(typeof(Floor))
                .WherePasses(exclusionFilter)
                .WherePasses(intrudingElemFilter)
                .WhereElementIsNotElementType()
                .ToElements();

            IList<Element> envadingFloorsOrderedByLevel =
                envadingFloors.OrderBy((Element e) =>
                {
                    return ((Level)doc
                            .GetElement(e.LevelId)).Elevation;
                }).ToList();

            return envadingFloorsOrderedByLevel;
        }

        static IList<Element> SelectInterferingElems(UIDocument uidoc, Document doc)
        {
            Autodesk.Revit.UI.Selection.Selection selection = uidoc.Selection;
            IList<Reference> references =
                selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element,
                new HorizontalElementsSelectionFilter(), "Select splitting elements");

            // Detect whether the selected elements intersect the main element

            IList<Element> selectedElems = references.Select<Reference, Element>
                (r => (doc.GetElement(r))).ToList<Element>();

            return selectedElems.OrderBy(e =>
            {
                return ((Level)doc
                            .GetElement(e.LevelId)).Elevation;
            }).ToList();
        }

        static IList<Solid> GetInterferingElemsAsSolids(IList<Element> interferingElems)
        {

            return interferingElems.Select(e =>
            {
                return RvtGeometryUtils.GetSolid(e);
            }).ToList();
        }

        static Solid GetResultingSolid(Solid solid, IList<Solid> solids)
        {
            Solid resultingSolid = solid;

            foreach (Solid currSolid in solids)
            {
                resultingSolid = BooleanOperationsUtils
                    .ExecuteBooleanOperation(resultingSolid, currSolid,
                                             BooleanOperationsType.Difference);
            }

            return resultingSolid;
        }

        static void GetDirectShape(Document doc, Solid solid)
        {
            using (Transaction t = new Transaction(doc, "Create DirectShape object"))
            {
                t.Start();
                DirectShape ds = DirectShape
                    .CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetShape(new GeometryObject[] { solid });
                t.Commit();
            }
        }

        

        static bool ModifiedProfile(Wall wall)
        {
            Solid solid = RvtGeometryUtils.GetSolid(wall);
            XYZ orientation = wall.Orientation;
            Face face = null;
            foreach (Face f in solid.Faces)
            {
                if (orientation
                   .DotProduct(f.ComputeNormal(new UV(0, 0))) > 0)
                {
                    face = f;
                    break;
                }
            }
            if (face == null)
            {
                Trace.WriteIf(generalSwitch.TraceError, "Unable to extract a face from the wall.");
                throw new ArgumentNullException("Something has gone wrong!");
            }


            if (face.GetEdgesAsCurveLoops().Count > 1)
                return true;
            else
                return false;
        }

        static TaskDialog CreateCustomTaskDialog()
        {
            TaskDialog taskDialog = new TaskDialog("Choice");
            taskDialog.MainInstruction = "Please select how to split the element.";
            taskDialog.CommonButtons = TaskDialogCommonButtons.Cancel;
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Select elements manually");
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Automatically detect all intruding elements");

            return taskDialog;
        }

        static void UnjoinElements(Document doc, Element selectedElem)
        {
            ICollection<ElementId> joinedElements =
                    JoinGeometryUtils.GetJoinedElements(doc, selectedElem);

            if (joinedElements != null &&
               joinedElements.Count > 0)
            {
                using (Transaction t = new Transaction(doc, "Unjoin Elements"))
                {
                    t.Start();
                    foreach (ElementId id in joinedElements)
                    {
                        JoinGeometryUtils.UnjoinGeometry
                            (doc, selectedElem, doc.GetElement(id));
                    }
                    t.Commit();
                }
            }

            m_selectedElem = selectedElem;
            m_selectedElemSolid = RvtGeometryUtils.GetSolid(m_selectedElem);
        }

        static void RejoinElements()
        {
            // UNDER DEVELOPMENT
        }

        static void WallSplitter(Document doc, Element selectedElem)
        {
            bool hasInserts = false;
            IList<Insert> insertedElements = new List<Insert>();
            IList<WallOpening> wallOpenings = new List<WallOpening>(); 

            IList<ElementId> inserts =
                        (selectedElem as Wall).FindInserts(true, true, true, true);

            Trace.Write(string.Format("Inserts.Count={0}", inserts.Count));

            if (inserts.Count != 0)
            {
                // IN THE WORKS
                //return;
                hasInserts = true;

                foreach(ElementId id in inserts)
                {
                    Element element = doc.GetElement(id);
                    if (element is FamilyInstance)
                    {
                        FamilyInstance famInst = doc.GetElement(id) as FamilyInstance;
                        LocationPoint lp = famInst.Location as LocationPoint;
                        if (famInst == null || lp == null)
                            continue;
                        Insert insert = new Insert(lp.Point, famInst.Symbol);
                        insertedElements.Add(insert);
                    }
                    else if(element is Opening)
                    {
                        Opening opening = element as Opening;
                        if(opening.IsRectBoundary)
                        {
                            WallOpening wallOpening =
                                new WallOpening(opening.BoundaryRect[0], opening.BoundaryRect[1]);
                            wallOpenings.Add(wallOpening);
                        }
                            
                    }
                }

                using (Transaction t = new Transaction(doc, "Scrap inserts"))
                {
                    t.Start();
                    doc.Delete(inserts);
                    t.Commit();
                }

                m_selectedElemSolid = RvtGeometryUtils.GetSolid(doc.GetElement(selectedElem.Id));
            }

            if (ModifiedProfile((Wall)(selectedElem)))
                return;

            // Get hold of all solids involed in the clash
            IList<Solid> interferingSolids =
                GetInterferingElemsAsSolids(m_interferingElems);

            // Perform the boolean opeartions to get the resulting solid
            Solid resultingSolid =
                GetResultingSolid(m_selectedElemSolid, interferingSolids);

            // Find the face whose normal matches the wall's normal
            Face face = null;

            XYZ wallOrientation = ((Wall)selectedElem).Orientation;
            foreach (Face currFace in resultingSolid.Faces)
            {
                XYZ faceNormal = currFace
                    .ComputeNormal(new UV(0, 0)).Normalize();

                if (Math.Round(
                    wallOrientation.DotProduct(faceNormal), 2) > 0.1)
                {
                    face = currFace;
                    break;
                }
            }

            if (face == null)
                throw new ArgumentNullException("Face is null");

            // Get a set of curveloops from the face
            IList<CurveLoop> crvLoops = face.GetEdgesAsCurveLoops();

            IList<CurveLoop> orderedCrvloops =
                (crvLoops.OrderBy(crvloop =>
                {
                    Curve crv = crvloop
                        .Where(c => RvtGeometryUtils.GetDirectionVector(c).Z == 1)
                        .First();

                    return crv.GetEndPoint(0).Z;
                })).ToList();

            // Get Wall's properties
            Wall wall = (Wall)selectedElem;
            WallType wallType = wall.WallType;
            IList<Wall> newWalls = new List<Wall>();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create new walls");
                for (int i = 0; i < orderedCrvloops.Count; i++)
                {
                    Curve selectedCrv =
                        orderedCrvloops[i].Where(crv =>
                                                 RvtGeometryUtils.
                                                 GetDirectionVector(crv).Z == 1).First();

                    double currWallHeight = selectedCrv.ApproximateLength;

                    if (i == 0)
                    {
                        double offset = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();

                        Wall newWall=Wall.Create(doc, ((LocationCurve)wall.Location).Curve, wallType.Id, wall.LevelId,
                                    currWallHeight, offset, false, true);

                        newWalls.Add(newWall);

                    }
                    else
                    {
                        Element intruder = m_interferingElems
                            .ElementAt(i - 1);

                        ElementId currLevelId = intruder.LevelId;

                        double offset = intruder
                            .get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM)
                            .AsDouble();

                        Wall newWall=Wall.Create(doc, ((LocationCurve)wall.Location).Curve, wallType.Id, currLevelId,
                                    currWallHeight, offset, false, true);

                        newWalls.Add(newWall);
                    }
                    
                }
                doc.Delete(wall.Id);
                t.Commit();

                if (hasInserts)
                {
                    foreach (Wall newWall in newWalls)
                    {
                        ReinsertHostedObject(doc, insertedElements, newWall);
                        if (wallOpenings.Count != 0)
                            ReinsertHostedObject(doc, wallOpenings, newWall);
                    }
                }
                
            }
            
        }

        static void ColumnSplitter(Document doc, Element selectedElem)
        {
            List<Curve> crvs = new List<Curve>();
            IDictionary<Curve, Level> crvsLvls = new Dictionary<Curve, Level>();

            Curve currCrv =RvtGeometryUtils.GetColumnAxis(selectedElem);

            ElementId prevElemId =
                m_interferingElems.ElementAt(0).LevelId;

            for (int i = 0; i < m_interferingElems.Count; i++)
            {
                Element currElem = m_interferingElems.ElementAt(i);

                Solid elemSolid = RvtGeometryUtils.GetSolid(currElem);

                SolidCurveIntersection results =
                    elemSolid.IntersectWithCurve
                    (currCrv,
                     new SolidCurveIntersectionOptions
                     { ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside });

                if (results.SegmentCount == 2)
                {
                    // if it is not the last segment
                    if (i != m_interferingElems.Count - 1)
                    {
                        crvs.Add(results.GetCurveSegment(0));
                        currCrv = results.GetCurveSegment(1);
                        crvsLvls.Add(results.GetCurveSegment(0), (Level)doc.GetElement(prevElemId));
                    }
                    else
                    {
                        crvs.Add(results.GetCurveSegment(0));
                        crvs.Add(results.GetCurveSegment(1));
                        crvsLvls.Add(results.GetCurveSegment(0), (Level)doc.GetElement(prevElemId));
                        crvsLvls.Add(results.GetCurveSegment(1), (Level)doc.GetElement(currElem.LevelId));
                    }
                }
                else
                {
                    currCrv = results.GetCurveSegment(0);
                }
                prevElemId = currElem.LevelId;
            }

            FamilySymbol columnType = ((FamilyInstance)selectedElem).Symbol;

            using (Transaction t = new Transaction(doc, "Split Column"))
            {
                t.Start();
                foreach (Curve crv in crvsLvls.Keys)
                {
                    doc.Create.NewFamilyInstance
                        (crv, columnType, crvsLvls[crv], StructuralType.Column);
                }

                doc.Delete(selectedElem.Id);
                t.Commit();

            }
        }

        

        private static Curve ObtainVerticalCurve(Face face)
        {
            CurveLoop crvLoop= face.GetEdgesAsCurveLoops().First<CurveLoop>();
            foreach(Curve crv in crvLoop)
            {
                if (RvtGeometryUtils.GetDirectionVector(crv).Z == 1)
                    return crv;
            }
            return null;
        }

        private static void ReinsertHostedObject(Document doc, IList<Insert> inserts, Wall newWall)
        {
            Face face = RvtGeometryUtils.ObtainWallFace(newWall);
            XYZ lowerPoint = ObtainVerticalCurve(face).GetEndPoint(0);
            XYZ upperPoint = ObtainVerticalCurve(face).GetEndPoint(1);
            Level wallLevel = doc.GetElement(newWall.LevelId) as Level;

            Trace.Write(string.Format("lowerPoint.Z={0}; upperPoint.Z={1}; Level={2}", 
                lowerPoint.Z, upperPoint.Z, wallLevel.Name));

            foreach (Insert insert in inserts)
            {
                Trace.Write(string.Format("locationPoint.Point={0}", 
                    insert.LocationPoint.ToString()));

                if (insert.LocationPoint.Z>=lowerPoint.Z && 
                    insert.LocationPoint.Z<=upperPoint.Z)
                {
                    Trace.Write(string.Format("famSbl.Name={0}", insert.FamilySymbol.Name));

                    using (Transaction t = new Transaction(doc))
                    {
                        t.Start("reinsert hosted element");
                        doc.Create.NewFamilyInstance(insert.LocationPoint, 
                            insert.FamilySymbol, newWall, wallLevel, StructuralType.NonStructural);
                        t.Commit();
                    }
                }
            }
        }

        private static void ReinsertHostedObject(Document doc, IList<WallOpening> openings, Wall newWall)
        {
            Face face = RvtGeometryUtils.ObtainWallFace(newWall);
            XYZ lowerPoint = ObtainVerticalCurve(face).GetEndPoint(0);
            XYZ upperPoint = ObtainVerticalCurve(face).GetEndPoint(1);
            Level wallLevel = doc.GetElement(newWall.LevelId) as Level;

            Trace.Write(string.Format("lowerPoint.Z={0}; upperPoint.Z={1}; Level={2}",
                lowerPoint.Z, upperPoint.Z, wallLevel.Name));

            foreach (WallOpening opening in openings)
            {
                Trace.Write(string.Format("locationPoint.Point={0}",
                    opening.LocationPoint.ToString()));

                if (opening.LocationPoint.Z >= lowerPoint.Z &&
                    opening.LocationPoint.Z <= upperPoint.Z)
                {
                    
                    using (Transaction t = new Transaction(doc))
                    {
                        t.Start("reinsert opening");
                        doc.Create.NewOpening(newWall, opening.LowerEndPoint, opening.UpperEndPoint);
                        t.Commit();
                    }
                }
            }
        }
    }

    class Insert
    {
        // Properties
        public XYZ LocationPoint { get; private set; }
        public FamilySymbol FamilySymbol { get; private set; }

        // Constructors
        /// <summary>
        /// Create an instance of the Insert class
        /// </summary>
        /// <param name="locPnt">Location Point</param>
        /// <param name="famSbl">Family Instance</param>
        public Insert(XYZ locPnt, FamilySymbol famSbl)
        {
            this.LocationPoint = locPnt;
            this.FamilySymbol = famSbl;
        }
    }

    class WallOpening
    {
        #region Properties
        public XYZ LowerEndPoint { get; private set; }
        public XYZ UpperEndPoint { get; private set; }
        public XYZ LocationPoint
        {
            get
            {
                return LowerEndPoint.Add(UpperEndPoint).Divide(2);
            }
        }
        #endregion
        #region Constructors
        public WallOpening(XYZ lowEndPtr, XYZ uppEndPnt)
        {
            this.LowerEndPoint = lowEndPtr;
            this.UpperEndPoint = uppEndPnt;
        }
        #endregion

    }
}
