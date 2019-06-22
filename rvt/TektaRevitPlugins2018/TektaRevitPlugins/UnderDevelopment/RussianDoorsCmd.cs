using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using System.Windows.Controls;
using RvtCnvt = Autodesk.Revit.DB.UnitUtils;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class RussianDoorsCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            // Door variables.
            string doorName = string.Empty;
            string itemType = string.Empty;
            string doorType = string.Empty;
            int height = 0;
            int width = 0;
            string props = string.Empty;

            // Find the right door family.
            FilteredElementCollector collector =
                new FilteredElementCollector(doc);

            FilterRule famNameRule = ParameterFilterRuleFactory
                .CreateBeginsWithRule(
                    new ElementId((int)BuiltInParameter.ALL_MODEL_FAMILY_NAME),
                    "A-M3-ConceptualDoor", true);

            FilterRule typeNameRule = ParameterFilterRuleFactory
                .CreateEqualsRule(
                    new ElementId((int)BuiltInParameter.ALL_MODEL_TYPE_NAME),
                    "Default", true);

            ElementParameterFilter famNameFlt =
                new ElementParameterFilter(famNameRule);

            ElementParameterFilter typeNameFlt =
                new ElementParameterFilter(typeNameRule);

            LogicalAndFilter andFilter =
                new LogicalAndFilter(famNameFlt, typeNameFlt);

            Func<FamilySymbol, bool> singleLeaf =
                (fs) => fs.FamilyName == "A-M3-ConceptualDoorSingleLeaf-LOD2";

            Func<FamilySymbol, bool> doubleLeaf =
                (fs) => fs.FamilyName == "A-M3-ConceptualDoorDoubleLeaf-LOD2";

            IEnumerable<FamilySymbol> doors = collector
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsElementType()
                .WherePasses(andFilter)
                .Cast<FamilySymbol>();

            FamilySymbol singleLeafDoor = doors
                .Where(singleLeaf)
                .FirstOrDefault();

            FamilySymbol doubleLeafDoor = doors
                .Where(doubleLeaf)
                .FirstOrDefault();

            if (singleLeafDoor == null) {
                TaskDialog.Show("No door family", 
                    "Please load the family: A-M3-ConceptualDoorSingleLeaf-LOD2");
                return Result.Failed;
            }

            // Show the custom window
            RussianDoorWnd win = new RussianDoorWnd();

            win.btnCreate.Click += delegate (object sender, RoutedEventArgs e) {
                doorName = win.lbg6629.Content.ToString();

                itemType = ((ComboBoxItem)win.itemType.SelectedItem).Content.ToString();
                doorType = ((ComboBoxItem)win.doorType.SelectedItem).Content.ToString();
                height = Int16.Parse((string)((ComboBoxItem)win.height.SelectedItem).Content) * 100;
                width = Int16.Parse((string)((ComboBoxItem)win.width.SelectedItem).Content) * 100;
                props = ((ComboBoxItem)win.props.SelectedItem).Content.ToString();

                win.Close();

                try {
                    using (TransactionGroup tg = new TransactionGroup(doc, "Create new type")) {
                        ElementType newType;

                        tg.Start();

                        using (Transaction t1 = new Transaction(doc, "Create new type")) {
                            t1.Start();
                            if (width <= 1200) {
                                newType = singleLeafDoor.Duplicate(doorName);
                            }
                            else {
                                newType = doubleLeafDoor.Duplicate(doorName);
                            }
                            t1.Commit();
                        }

                        using (Transaction t2 = new Transaction(doc, "Set params")) {
                            t2.Start();

                            switch (doorType) {
                                case "Г":
                                    newType.LookupParameter("Solid Leaf").Set(1);
                                    newType.LookupParameter("Glazed Leaf").Set(0);
                                    newType.LookupParameter("Leaf with Vent Grille").Set(0);
                                    break;
                                case "О":
                                    newType.LookupParameter("Solid Leaf").Set(0);
                                    newType.LookupParameter("Glazed Leaf").Set(1);
                                    newType.LookupParameter("Leaf with Vent Grille").Set(0);
                                    break;
                            }

                            newType.LookupParameter("Door Leaf Height")
                                .Set(RvtCnvt.ConvertToInternalUnits((height - 100), DisplayUnitType.DUT_MILLIMETERS));

                            newType.LookupParameter("Door Leaf Width")
                                .Set(RvtCnvt.ConvertToInternalUnits((width - 100), DisplayUnitType.DUT_MILLIMETERS));

                            if (props.Contains('Л'))
                                newType.LookupParameter("RH").Set(0);
                            else
                                newType.LookupParameter("RH").Set(1);

                            if (props.Contains('П'))
                                newType.LookupParameter("Threshold").Set(1);
                            else
                                newType.LookupParameter("Threshold").Set(0);

                            t2.Commit();
                        }

                        tg.Assimilate();
                    }
                }
                catch (Autodesk.Revit.Exceptions.ArgumentException ex) {
                    if (ex.ParamName == "name")
                        TaskDialog.Show("ArgumentError",
                                        string.Format("This type already exists."));
                }
                catch (Exception ex) {
                    TaskDialog.Show("Exception", ex.StackTrace);
                }
            };

            win.ShowDialog();

            return Result.Succeeded;
        }
    }
}
