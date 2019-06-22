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
using Autodesk.Revit.Exceptions;
using RevitCreation = Autodesk.Revit.Creation;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(TransactionMode.Manual)]
    class RoomAreaCalCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = 
                commandData.Application.ActiveUIDocument.Document;
            Selection selection =
                commandData.Application.ActiveUIDocument.Selection;

            Reference r = selection.PickObject(ObjectType.Element);
            SpatialElement spElem = doc.GetElement(r) as SpatialElement;

            SpatialElementBoundaryOptions boundaryOptions = 
                new SpatialElementBoundaryOptions
                { SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center };

            double centreGrossArea = UnitUtils.ConvertFromInternalUnits(
                GetRoomArea(spElem, boundaryOptions), DisplayUnitType.DUT_SQUARE_METERS);

            boundaryOptions.SpatialElementBoundaryLocation = 
                SpatialElementBoundaryLocation.CoreBoundary;

            double coreGrossArea = UnitUtils.ConvertFromInternalUnits(
                GetRoomArea(spElem, boundaryOptions), DisplayUnitType.DUT_SQUARE_METERS);

            double strWallArea;
            GetstructuralWallArea(doc, spElem, out strWallArea);

            double wantedArea = centreGrossArea - UnitUtils.ConvertFromInternalUnits(strWallArea,
                DisplayUnitType.DUT_SQUARE_METERS);

            TaskDialog.Show("Result:", string.Format("Area along the centre line={0}\n" +
                 "Area along the core boundary={1}\n" +
                 "strWallArea={2}\n" +
                 "WantedArea={3}", centreGrossArea, coreGrossArea, UnitUtils.ConvertFromInternalUnits(
                strWallArea, DisplayUnitType.DUT_SQUARE_METERS), wantedArea));

            return Result.Succeeded;
        }

        
        private static double GetRoomArea(SpatialElement spElem, 
            SpatialElementBoundaryOptions boundaryOptions)
        {
            IList<IList<BoundarySegment>> bndSegmentsList =
                        spElem.GetBoundarySegments(boundaryOptions);

            double grossArea = 0;

            for (int i = 0; i < bndSegmentsList.Count; ++i)
            {
                if (i == 0)
                    grossArea += PolygonArea(bndSegmentsList[i]);
                else
                    grossArea -= PolygonArea(bndSegmentsList[i]);
            }

            return grossArea;
        }

        private static void GetstructuralWallArea(Document doc, SpatialElement spElem, out double wallArea)
        {
            SpatialElementBoundaryOptions boundaryOptions =
                new SpatialElementBoundaryOptions
                { SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center };

            IList<IList<BoundarySegment>> bndSegmentsList =
                        spElem.GetBoundarySegments(boundaryOptions);

            wallArea = 0;
            for (int i = 0; i < bndSegmentsList.Count; ++i)
            {
                for (int j = 0; j < bndSegmentsList[i].Count; ++j)
                {
                    Element boundaryElem = doc.GetElement(bndSegmentsList[i][j].ElementId);
                    if (boundaryElem is Wall &&
                        ((Wall)boundaryElem).get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT).AsInteger() == 1)
                    {
                        Wall strWall = (Wall)boundaryElem;
                        wallArea += (strWall.WallType.Width/2) *
                            (bndSegmentsList[i][j].GetCurve().Length);
                    }
                }
            }
        }

        private static double PolygonArea(IList<BoundarySegment> segments)
        {
            IList<XYZ> vertices = segments
                .Select<BoundarySegment, XYZ>(s => s.GetCurve().GetEndPoint(0))
                .ToList();

            double area = 0;
            int i, j = vertices.Count - 1;
            for (i = 0; i < vertices.Count; ++i)
            {
                area += (vertices[j].X + vertices[i].X) *
                    (vertices[j].Y - vertices[i].Y);

                j = i;
            }
            return Math.Abs(area/2);
        }


    }
}
