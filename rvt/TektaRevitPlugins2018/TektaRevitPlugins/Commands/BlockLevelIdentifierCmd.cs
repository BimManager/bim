using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Architecture;

namespace TektaRevitPlugins.Commands
{
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class BlockLevelIdentifierCmd : Autodesk.Revit.UI.IExternalCommand
    {
        // Constants PRM -> parameter
        const string PRM_BLOCK_NO = "RM_BLOCK_NO";
        const string PRM_FUNCTION_NO = "RM_FUNCTION_NO";
        const string PRM_LEVEL = "RM_LEVEL";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Add an EventLogTraceListener object
            System.Diagnostics.Trace.Listeners.Add(
                new System.Diagnostics.EventLogTraceListener("Application"));

            // Lay the hands on the active document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Set up a timer
            Timing myTimer = new Timing();
            myTimer.StartTime();

            StringBuilder strBld = new StringBuilder();
            try
            {
                // Check the active view
                Level curLvl = doc.ActiveView.GenLevel;
                if(curLvl == null)
                {
                    TaskDialog.Show("Execption", "Add-in only operates in Floor Plans!");
                    return Result.Failed;
                }

                // Take care of the shared parameters
                if(!CheckNessarySharedParameters(doc))
                {
                    TaskDialog.Show("Exception", "Necessary parameters do not exit!");
                    return Result.Failed;
                }

                // Get scope boxes
                IList<Element> boxes = GetScopeBoxes(doc);

                // Ensure that both, rooms and boxes, are present
                if (boxes.Count == 0)
                {
                    TaskDialog.Show("Execption", "No scope box!");
                    return Result.Failed;
                }

                // Generate level name
                string lvlName = GenLvlName(doc);

                // Pull in all rooms intersected by each scope box 
                // and get their parameters set
                for (int i  = 0; i < boxes.Count; ++i)
                {
                    BoundingBoxXYZ boundingBox =
                        boxes[i].get_BoundingBox(doc.ActiveView);
                    Outline outline = 
                        new Outline(boundingBox.Min, boundingBox.Max);
                    BoundingBoxIsInsideFilter insideBndBoxFltr =
                        new BoundingBoxIsInsideFilter(outline);

                    RoomFilter roomFltr = new RoomFilter();
                    LogicalAndFilter roomAndIntersectBndBox = 
                        new LogicalAndFilter(insideBndBoxFltr, roomFltr);

                    IList<Element> rooms = new FilteredElementCollector(doc, doc.ActiveView.Id)
                        .WherePasses(roomAndIntersectBndBox)
                        .ToElements();

                    string blockNo = GenBlockName(boxes[i]);

                    using (Transaction t = new Transaction(doc, "Set rooms' parameters"))
                    {
                        t.Start();
                        for (int j = 0; j < rooms.Count; ++j)
                        {
                            Element rm = rooms[j];
                            rm.LookupParameter(PRM_BLOCK_NO).Set(blockNo);
                            rm.LookupParameter(PRM_LEVEL).Set(lvlName);
                            string commentsContent = 
                                rm.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
                            if(commentsContent != null && commentsContent.Length != 0)
                            {
                                rm.LookupParameter(PRM_FUNCTION_NO).Set(commentsContent.Substring(0, 1));
                            }
                            
                        }
                        t.Commit();
                    }
                }
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception",
                  string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                System.Diagnostics.Trace.Write(string.Format("{0}\n{1}",
                  ex.Message, ex.StackTrace));
                return Result.Failed;
            }
            finally
            {
                myTimer.StopTime();
                System.Diagnostics.Trace
                    .Write(string.Format("Time elapsed: {0}s",
                  myTimer.Duration.TotalSeconds));
            }
        }

        bool CheckNessarySharedParameters(Document doc)
        {
            SharedParametersManager spMgr = new SharedParametersManager(doc);
            if(!spMgr.DoesParameterExist(PRM_BLOCK_NO) ||
                !spMgr.DoesParameterExist(PRM_FUNCTION_NO) ||
                !spMgr.DoesParameterExist(PRM_LEVEL))
            {
                return false;
            }
            return true;
        }

        IList<Element> GetScopeBoxes(Document doc)
        {
            return new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_VolumeOfInterest)
                .ToElements()
                .ToList();
        }

        bool ContainsRoom(Element scopeBox, SpatialElement room)
        {
            BoundingBoxXYZ boundingBox = scopeBox.get_BoundingBox(room.Document.ActiveView);
            XYZ min = boundingBox.Min;
            XYZ max = boundingBox.Max;
            XYZ rmPnt = ((LocationPoint)room.Location).Point;
            if(rmPnt.X >= min.X && rmPnt.X <= max.X &&
                rmPnt.Y >= min.Y && rmPnt.Y <= max.Y &&
                rmPnt.Z >= min.Z && rmPnt.Z <= max.Z)
            {
                return true;
            }
            return false;
        }

        IList<Room> GetRooms(Document doc)
        {
            return new FilteredElementCollector(doc, doc.ActiveView.Id)
                .WherePasses(new RoomFilter())
                .Cast<Room>()
                .ToList();
        }
        IList<ElementId> GetLevelIds(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .ToElementIds()
                .OrderBy(id => ((Level)doc.GetElement(id)).Elevation)
                .ToList();
        }

        string GenLvlName(Document doc)
        {
            int curLvlIndex;
            string lvlName = null;

            // Get all the levels in the doc
            IList<ElementId> levelIds = GetLevelIds(doc);

            // Get the active view level
            Level curLvl = doc.ActiveView.GenLevel;

            // Divide up the levels into two groups
            if (curLvl.Elevation < 0)
            {
                // Get only underground levels whose elevation is less than 0
                IList<ElementId> undLevelIds = 
                    levelIds.Where(id => ((Level)doc.GetElement(id)).Elevation < 0).ToList();

                // Get the active view index
                curLvlIndex = undLevelIds.IndexOf(curLvl.Id);

                switch (curLvlIndex)
                {
                    case 0:
                        lvlName = "П" + 3;
                        break;

                    case 1:
                        lvlName = "П" + 2;
                        break;

                    default:
                        lvlName = "П" + 1;
                        break;
                }
            }
            else
            {
                // Get levels above ground
                IList<ElementId> aboveLevelIds =
                    levelIds.Where(id => ((Level)doc.GetElement(id)).Elevation >= 0).ToList();

                // Get the active view index
                curLvlIndex = aboveLevelIds.IndexOf(curLvl.Id);

                lvlName = (curLvlIndex + 1).ToString();
            }
            return lvlName;
        }
        string GenBlockName(Element scopeBox)
        {
            if (scopeBox.Name.Contains("1"))
            {
                return "Б1";
            }
            else if (scopeBox.Name.Contains("2"))
            {
                return "Б2";
            }
            else if (scopeBox.Name.Contains("3"))
            {
                return "Б3";
            }
            else
            {
                return "Б4";
            }
        }

    }
}
