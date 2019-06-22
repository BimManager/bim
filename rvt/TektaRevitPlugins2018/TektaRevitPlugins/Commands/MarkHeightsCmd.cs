using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class MarkHeightsCmd : Autodesk.Revit.UI.IExternalCommand
    {
        const string SILL_HEIGHT = "SILL_HEIGHT";
        const string HEAD_HEIGHT = "HEAD_HEIGHT";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Add an EventLogTraceListener object
            System.Diagnostics.Trace.Listeners.Add(
               new System.Diagnostics.EventLogTraceListener("Application"));

            // Lay the hands on the active document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // set up a timer
            Timing myTimer = new Timing();
            myTimer.StartTime();
            try {
                CheckNecessarySharedParameters(doc);

                // Set up a filter
                ElementCategoryFilter doorsFilter =
                    new ElementCategoryFilter(BuiltInCategory.OST_Doors);
                ElementCategoryFilter windowsFilter =
                    new ElementCategoryFilter(BuiltInCategory.OST_Windows);
                LogicalOrFilter orFilter =
                    new LogicalOrFilter(doorsFilter, windowsFilter);

                IList<FamilyInstance> doorsAndWindows =
                    new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .OfClass(typeof(FamilyInstance))
                    .WherePasses(orFilter)
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .ToList();

                if (doorsAndWindows.Count != 0) {
                    foreach (FamilyInstance fi in doorsAndWindows) {
                        try {
                            ElementId hostId = fi.Host.Id;
                            Wall hostWall = doc.GetElement(hostId) as Wall;
                            if (hostWall == null ||
                                hostWall.WallType.Kind == WallKind.Curtain)
                                continue;
                            SetHeights(fi);
                        }
                        catch (Exception) {
                            System.Diagnostics.Trace.Write(
                                string.Format("The object which caused failure is : {0}",
                                fi.Name));
                            continue;
                        }

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

        #region Helper Methods
        static void SetHeights(FamilyInstance fi)
        {
            Document doc = fi.Document;
            using (Transaction t = new Transaction(doc)) {
                t.Start("Set heights");

                fi.LookupParameter(SILL_HEIGHT)
                    .Set(FormatDoubleIntoString(fi, 
                    BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM));

                fi.LookupParameter(HEAD_HEIGHT)
                    .Set(FormatDoubleIntoString(fi, 
                    BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM));

                t.Commit();
            }
        }

        static string FormatDoubleIntoString(FamilyInstance fi, BuiltInParameter parameter)
        {
            // get the value as a double
            double elev = ((Level)fi.Document
                    .GetElement(fi.LevelId)).Elevation + fi.get_Parameter
                    (parameter).AsDouble();
            elev = Math.Round(UnitUtils
                .ConvertFromInternalUnits(elev, DisplayUnitType.DUT_METERS), 3);

            // Convert to string
            System.Globalization.CultureInfo cultureInfo =
                new System.Globalization.CultureInfo("en-US");
            string elevAsStr = elev.ToString("F3", cultureInfo);
            if (elev > 0)
                elevAsStr = "+" + elevAsStr;
            return elevAsStr;
        }

        static void CheckNecessarySharedParameters(Document doc) {
            // iterate the procedure over all the shared parameters in the document
            SharedParametersManager sharedParamMng = new SharedParametersManager(doc);
            if (!sharedParamMng.DoesParameterExist(SILL_HEIGHT)) {
                List<BuiltInCategory> categories =
                    new List<BuiltInCategory> { BuiltInCategory.OST_Doors,
                            BuiltInCategory.OST_Windows };

                sharedParamMng.CreateSharedParameter(
                    SILL_HEIGHT,
                    ParameterType.Text,
                    categories,
                    BuiltInParameterGroup.PG_CONSTRAINTS,
                    true, false, true, "Custom", true, 
                    new Guid("CB9AF2A5-BA75-404E-A462-0AAB7C3297A1"));

                sharedParamMng.CanVaryBtwGroups(SILL_HEIGHT, true);
            }
            if (!sharedParamMng.DoesParameterExist(HEAD_HEIGHT)) {
                List<BuiltInCategory> categories =
                    new List<BuiltInCategory> { BuiltInCategory.OST_Doors,
                            BuiltInCategory.OST_Windows };

                sharedParamMng.CreateSharedParameter(
                    HEAD_HEIGHT,
                    ParameterType.Text,
                    categories,
                    BuiltInParameterGroup.PG_CONSTRAINTS,
                    true, false, true, "Custom", true, 
                    new Guid("492127E8-04F4-47A2-B0AB-8ADDD61ABDCF"));

                sharedParamMng.CanVaryBtwGroups(HEAD_HEIGHT, true);
            }
        }
        #endregion
    }
}
