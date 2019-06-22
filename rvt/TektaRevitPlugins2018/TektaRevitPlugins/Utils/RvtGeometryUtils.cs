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
using System.Reflection;

namespace TektaRevitPlugins
{
    public static class RvtGeometryUtils
    {
        public static Solid GetSolid(Element element)
        {
            Options options = new Options { IncludeNonVisibleObjects = true };

            foreach (GeometryObject gObj_1 in element.get_Geometry(new Options())) {
                if (gObj_1 is GeometryInstance) {
                    GeometryInstance instance =
                        gObj_1 as GeometryInstance;

                    foreach (GeometryObject gObj_2 in
                            instance.GetInstanceGeometry(instance.Transform)) {
                        if (gObj_2 is Solid) {
                            return (Solid)gObj_2;
                        }
                    }
                }
                else if (gObj_1 is Solid) {
                    return (Solid)gObj_1;
                }
            }
            return null;
        }

        public static IList<Element> GetInterferingElements(Document doc, Element element)
        {
            Solid solid = GetSolid(element);
            if (solid == null)
                throw new ArgumentNullException("GetSolid()==null");
            FilteredElementCollector collector =
                new FilteredElementCollector(doc);
            ElementIntersectsSolidFilter intrudingElemsFilter =
                new ElementIntersectsSolidFilter(solid, false);
            ExclusionFilter exclusionFilter =
                new ExclusionFilter(new List<ElementId> { element.Id });
            ICollection<Element> envadingElements = collector
                .OfClass(typeof(Floor))
                .WherePasses(exclusionFilter)
                .WherePasses(intrudingElemsFilter)
                .WhereElementIsNotElementType()
                .ToElements();

            IList<Element> orderedEnvadingElems =
                envadingElements.OrderBy((Element e) =>
                {
                    return ((Level)doc
                            .GetElement(e.LevelId)).Elevation;
                }).ToList();

            return orderedEnvadingElems;
        }

        public static Curve GetColumnAxis(Element element)
        {
            if (element.Category.Id.IntegerValue != (int)BuiltInCategory.OST_StructuralColumns)
                throw new System.ArgumentException(string.Format("The element is not a structural column."));
            Options options =
                new Options { IncludeNonVisibleObjects = true };

            foreach (GeometryObject gObj in
                    element.get_Geometry(options)) {
                if (gObj is Curve)
                    return (Curve)gObj;
            }

            return null;
        }

        public static Face ObtainWallFace(Wall wall)
        {
            Solid wallSolid = GetSolid(wall);
            XYZ wallOrientation = wall.Orientation;
            Face normalFace = null;

            foreach (Face currFace in wallSolid.Faces) {
                XYZ faceNormal = currFace
                    .ComputeNormal(new UV(0, 0)).Normalize();

                if (Math.Round(
                    wallOrientation.DotProduct(faceNormal), 2) > 0.1) {
                    normalFace = currFace;
                    break;
                }
            }
            return normalFace;
        }

        public static XYZ GetDirectionVector(Curve curve)
        {
            XYZ direction = null;

            if (curve is Line) {
                // Pick up the tangent vector of the wall
                direction = curve.ComputeDerivatives(0, true).BasisX.Normalize();
            }
            else {
                direction = (curve.GetEndPoint(1)
                           - curve.GetEndPoint(0)).Normalize();
            }

            return direction;
        }
    }
}
