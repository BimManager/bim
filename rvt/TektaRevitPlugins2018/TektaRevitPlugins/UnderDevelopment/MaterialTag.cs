using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;

using Autodesk.Revit.DB.ExtensibleStorage;


namespace TektaRevitPlugins
{
    enum MultilayerElements { Wall, Floor, Roof, Ceiling };
    public class MultilayerStrFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Name == "Walls")
                if (((Wall)elem).WallType.Kind == WallKind.Basic)
                    return true;
            if ((elem.Category.Name == "Floors") ||
                (elem.Category.Name == "Ceilings") ||
                (elem.Category.Name == "Roofs"))
                return true;
            else
                return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    [Autodesk.Revit.Attributes.Transaction(TransactionMode.Manual)]
    class MaterialTag : IExternalCommand, IExternalCommandAvailability
    {
        bool IExternalCommandAvailability.IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            return true;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            const string TAG_FAMILY_NAME = "Z-M2-MultilayerMaterialTag-LOD1";
            const string TAG_TYPE_NAME = "2.5mm";

            #region Get access to the current document and aplication.
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = commandData.Application.Application;
            Autodesk.Revit.DB.Document doc = commandData.Application.ActiveUIDocument.Document;
            #endregion

            try
            {
                Category walls = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);
                Category floors = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Floors);
                Category ceilings = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Ceilings);
                Category roofs = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Roofs);
                CategorySet catSet = new CategorySet();
                catSet.Insert(walls); catSet.Insert(floors); catSet.Insert(ceilings); catSet.Insert(roofs);

                using (Transaction t = new Transaction(doc, "Add project parameter"))
                {
                    t.Start();
                    CreateProjectParam(app, "LinkedTag", ParameterType.Integer, false, catSet, BuiltInParameterGroup.INVALID, true);
                    if (t.Commit() == TransactionStatus.Committed) { }
                    else { t.RollBack(); }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }

            if (Schema.Lookup(new Guid("2B195204-1C04-4538-8881-AD22FA697B41")) == null)
                BuildNewSchema(doc);

            // Retrieving a specific family from the database.
            try
            {
                ElementParameterFilter famNameFlt =
                    new ElementParameterFilter(ParameterFilterRuleFactory
                    .CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME), TAG_FAMILY_NAME, false));
                ElementParameterFilter typeNameFlt =
                    new ElementParameterFilter(ParameterFilterRuleFactory
                    .CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME), TAG_TYPE_NAME, false));


                // Collect the familySymbol's id
                AnnotationSymbolType materialTag = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_GenericAnnotation)
                    .WherePasses(famNameFlt)
                    .WherePasses(typeNameFlt)
                    .Cast<AnnotationSymbolType>()
                    .FirstOrDefault();

                if (materialTag == null)
                {
                    message = "No FamilyType has been detected.";
                    return Result.Failed;
                }

                if (CreateWorkPlane(doc) != true)
                    return Result.Failed;

                // Prompt the user to select select the element to be tagged. 
                Reference pickedRef = null;
                pickedRef = uidoc.Selection.PickObject
                    (ObjectType.Element, new MultilayerStrFilter(), "Please select a multilayer element.");
                Element selectedElm = doc.GetElement(pickedRef.ElementId);

                XYZ point = uidoc.Selection.PickPoint("Please pick a point to place the family");

                StringBuilder strBld = new StringBuilder();
                int rows;
                int shelfLength;

                switch (selectedElm.Category.Name)
                {
                    case "Walls":
                        Wall wl = selectedElm as Wall;
                        WallType wlType = wl.WallType;
                        CompoundStructure cmpStr_wl = wlType.GetCompoundStructure();
                        strBld = LayersAsString(cmpStr_wl, doc, out shelfLength);
                        rows = wlType.GetCompoundStructure().LayerCount;
                        break;

                    case "Floors":
                        Floor fl = selectedElm as Floor;
                        FloorType flType = fl.FloorType;
                        CompoundStructure cmpStr_fl = flType.GetCompoundStructure();
                        strBld = LayersAsString(cmpStr_fl, doc, out shelfLength);
                        rows = flType.GetCompoundStructure().LayerCount;
                        break;

                    case "Ceilings":
                        Ceiling cl = selectedElm as Ceiling;
                        CeilingType clType = doc.GetElement(cl.GetTypeId()) as CeilingType;
                        CompoundStructure cmpStr_cl = clType.GetCompoundStructure();
                        strBld = LayersAsString(cmpStr_cl, doc, out shelfLength);
                        rows = clType.GetCompoundStructure().LayerCount;
                        break;

                    case "Roofs":
                        RoofBase rf = selectedElm as RoofBase;
                        RoofType rfType = rf.RoofType;
                        CompoundStructure cmpStr_rf = rfType.GetCompoundStructure();
                        strBld = LayersAsString(cmpStr_rf, doc, out shelfLength);
                        rows = rfType.GetCompoundStructure().LayerCount;
                        break;

                    default:
                        TaskDialog.Show("Warning!", "This category is not supported.");
                        return Result.Failed;
                }

                using (Transaction trn_1 = new Transaction(doc, "Materials Mark"))
                {
                    FamilyInstance createdElm = null;

                    if (trn_1.Start() == TransactionStatus.Started)
                    {
                        createdElm = doc.Create.NewFamilyInstance(point, materialTag, doc.ActiveView);
                        selectedElm.LookupParameter("LinkedTag").Set(createdElm.Id.IntegerValue);

                        if (trn_1.Commit() == TransactionStatus.Committed)
                        {
                            Transaction trn_2 = new Transaction(doc, "Set parameters");
                            try
                            {
                                trn_2.Start();
                                createdElm.LookupParameter("multilineText").Set(strBld.ToString());
                                createdElm.LookupParameter("# of Rows").Set(rows);
                                createdElm.LookupParameter("Shelf Length").Set(ToFt(shelfLength));
                                XYZ vec;
                                if (selectedElm.Category.Name != "Walls")
                                {
                                    vec = new XYZ(0, 0, createdElm.LookupParameter("Arrow Length").AsDouble() * doc.ActiveView.Scale);
                                    createdElm.LookupParameter("Down Arrow Direction").Set(1);
                                }
                                else
                                {
                                    vec = new XYZ(-createdElm.LookupParameter("Arrow Length").AsDouble() * doc.ActiveView.Scale, 0, 0);
                                    createdElm.LookupParameter("Down Arrow Direction").Set(0);
                                }

                                ElementTransformUtils.MoveElement(doc, createdElm.Id, vec);
                                trn_2.Commit();

                                AddElementToSchema(Schema.Lookup(new Guid("2B195204-1C04-4538-8881-AD22FA697B41")), createdElm, selectedElm.Id);
                            }
                            finally
                            {
                                if (trn_2 != null)
                                    trn_2.Dispose();
                            }
                        }
                        else
                        {
                            trn_1.RollBack();
                            return Result.Failed;
                        }
                    }
                    else
                    {
                        trn_1.RollBack();
                        return Result.Failed;
                    }

                    return Result.Succeeded;
                }

            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        #region Helper Methods
        private static double ToMM(double i)
        {
            return i * 304.8;
        }
        private static double ToFt(double i)
        {
            return i / 304.8;
        }

        private static bool CreateWorkPlane(Autodesk.Revit.DB.Document doc)
        {
            if ((doc.ActiveView.ViewType != ViewType.Detail) &&
                    (doc.ActiveView.ViewType != ViewType.Elevation) &&
                    (doc.ActiveView.ViewType != ViewType.Section))
            {
                TaskDialog.Show("Warning!", "Family instance cannot be created in the current view.");
                return false;
            }

            Transaction trn = new Transaction(doc, "WorkPlane");
            trn.Start();
            Plane plane = Plane.CreateByNormalAndOrigin(doc.ActiveView.ViewDirection, doc.ActiveView.Origin);
            SketchPlane sp = SketchPlane.Create(doc, plane);
            doc.ActiveView.SketchPlane = sp;
            trn.Commit();
            return true;
        }

        private static StringBuilder LayersAsString(CompoundStructure cmpStr, Autodesk.Revit.DB.Document doc, out int max)
        {
            StringBuilder strBld = new StringBuilder();
            int[] intList = new int[] { 0 };

            foreach (CompoundStructureLayer layer in cmpStr.GetLayers())
            {
                string material = null;
                double layerWidth;
                string materialAndWidth;

                if (layer.MaterialId != ElementId.InvalidElementId)
                {
                    material = doc.GetElement(layer.MaterialId).get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION).AsString();
                    layerWidth = UnitUtils.ConvertFromInternalUnits(layer.Width, DisplayUnitType.DUT_MILLIMETERS);
                }
                else
                {
                    material = "No Material";
                    layerWidth = UnitUtils.ConvertFromInternalUnits(layer.Width, DisplayUnitType.DUT_MILLIMETERS);
                }

                materialAndWidth = material + " - " + layerWidth + "mm";

                if (materialAndWidth.Length > intList[0])
                    intList[0] = materialAndWidth.Length;

                if (layer.Width != 0)
                    strBld.AppendLine(materialAndWidth);
                else
                    strBld.AppendLine(material);
            }

            max = intList[0] + 10;

            return strBld;
        }


        internal static void UpdateTag(ElementId modifiedElmId, Autodesk.Revit.DB.Document doc)
        {
            StringBuilder strBld = new StringBuilder();
            int rows = 0;
            int shelfLength = 0;
            Element modifiedElm = doc.GetElement(modifiedElmId);

            switch (modifiedElm.Category.Name)
            {
                case "Walls":
                    Wall wl = modifiedElm as Wall;
                    WallType wlType = wl.WallType;
                    CompoundStructure cmpStr_wl = wlType.GetCompoundStructure();
                    strBld = LayersAsString(cmpStr_wl, doc, out shelfLength);
                    rows = wlType.GetCompoundStructure().LayerCount;
                    break;

                case "Floors":
                    Floor fl = modifiedElm as Floor;
                    FloorType flType = fl.FloorType;
                    CompoundStructure cmpStr_fl = flType.GetCompoundStructure();
                    strBld = LayersAsString(cmpStr_fl, doc, out shelfLength);
                    rows = flType.GetCompoundStructure().LayerCount;
                    break;

                case "Ceilings":
                    Ceiling cl = modifiedElm as Ceiling;
                    CeilingType clType = doc.GetElement(cl.GetTypeId()) as CeilingType;
                    CompoundStructure cmpStr_cl = clType.GetCompoundStructure();
                    strBld = LayersAsString(cmpStr_cl, doc, out shelfLength);
                    rows = clType.GetCompoundStructure().LayerCount;
                    break;

                case "Roofs":
                    RoofBase rf = modifiedElm as RoofBase;
                    RoofType rfType = rf.RoofType;
                    CompoundStructure cmpStr_rf = rfType.GetCompoundStructure();
                    strBld = LayersAsString(cmpStr_rf, doc, out shelfLength);
                    rows = rfType.GetCompoundStructure().LayerCount;
                    break;
            }

            FamilyInstance tag = doc.GetElement(new ElementId(modifiedElm.LookupParameter("LinkedTag").AsInteger())) as FamilyInstance;
            try
            {
                tag.LookupParameter("multilineText").Set(strBld.ToString());
                tag.LookupParameter("# of Rows").Set(rows);
                tag.LookupParameter("Shelf Length").Set(ToFt(shelfLength));
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        private static void CreateProjectParam(Autodesk.Revit.ApplicationServices.Application app,
            string name, ParameterType type, bool visible, CategorySet catSet, BuiltInParameterGroup group, bool inst)
        {
            // Check whether the parameter has already been added. 
            BindingMap map = (new UIApplication(app)).ActiveUIDocument.Document.ParameterBindings;
            DefinitionBindingMapIterator itr = map.ForwardIterator();
            while (itr.MoveNext())
            {
                Definition exstDef = itr.Key;
                if (exstDef.Name == "LinkedTag")
                    return;
            }

            string orgFile = app.SharedParametersFilename;
            string tempFile = Path.GetTempFileName() + ".txt";
            using (File.Create(tempFile)) { }
            app.SharedParametersFilename = tempFile;

            ExternalDefinition def = app.OpenSharedParameterFile().Groups.Create("TemporaryDefinitionGroup").Definitions.Create
                (new ExternalDefinitionCreationOptions(name, type) { Visible = visible }) as ExternalDefinition;

            app.SharedParametersFilename = orgFile;
            File.Delete(tempFile);

            Binding binding = app.Create.NewTypeBinding(catSet);
            if (inst) binding = app.Create.NewInstanceBinding(catSet);

            map.Insert(def, binding);
        }

        private static void BuildNewSchema(Autodesk.Revit.DB.Document doc)
        {
            using (Transaction trn_1 = new Transaction(doc, "Build a new schema"))
            {
                if(trn_1.Start()==TransactionStatus.Started)
                {
                    SchemaBuilder builder = new SchemaBuilder(new Guid("2B195204-1C04-4538-8881-AD22FA697B41"));
                    // Set up accessibility levels.
                    builder.SetReadAccessLevel(AccessLevel.Public);
                    builder.SetWriteAccessLevel(AccessLevel.Vendor);
                    builder.SetVendorId("ADSK");
                    builder.SetSchemaName("Test");

                    // Create a field to store an ElementId
                    FieldBuilder elmIdField=builder.AddSimpleField("SelElmIds", typeof(ElementId));
                    elmIdField.SetDocumentation("Add the selected element's id to the created element's id");

                    Schema schema = builder.Finish();

                    if (trn_1.Commit() == TransactionStatus.Committed) { }
                    else trn_1.RollBack();
                }
            }
        }

        private static void AddElementToSchema(Schema schema, FamilyInstance elmToStoreIn, ElementId dataToStore)
        {
            using (Transaction trn_1 = new Transaction(elmToStoreIn.Document, "Add element Id to the schema"))
            {
                if (trn_1.Start() == TransactionStatus.Started)
                {
                    Entity entity = new Entity(schema);
                    entity.Set<ElementId>(schema.GetField("SelElmIds"), dataToStore);
                    elmToStoreIn.SetEntity(entity);
                    if (trn_1.Commit() == TransactionStatus.Committed) { }
                    else trn_1.RollBack();
                }
            }
        }
        #endregion
    }

    class ElementModified : IUpdater
    {
        static AddInId appId;
        static UpdaterId updaterId;

        public ElementModified(AddInId id)
        {
            appId = id;
            updaterId = new UpdaterId(appId, new Guid("519C2C24-A8CD-4F34-8E9C-BC95593A20DF"));
        }
        public void Execute(UpdaterData data)
        {
            Autodesk.Revit.DB.Document doc = data.GetDocument();

            foreach (ElementId modElmId in data.GetModifiedElementIds())
            {
                if(doc.GetElement(modElmId).LookupParameter("LinkedTag")!=null &&
                    doc.GetElement(modElmId).LookupParameter("LinkedTag").AsInteger()!= 0)
                {
                    MaterialTag.UpdateTag(modElmId, doc);
                }
            }
        }

        public string GetAdditionalInformation()
        {
            return "Test1";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Annotations;
        }

        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }

        public string GetUpdaterName()
        {
            return "Material Tag Updater";
        }
    }

    class ElementDeleted : IUpdater
    {
        private static AddInId _appId;
        private static UpdaterId _updaterId;

        public ElementDeleted(AddInId id)
        {
            _appId = id;
            _updaterId = new UpdaterId(_appId, new Guid("36227B9E-2879-4E2F-987C-80006356F4FB"));
        }

        public void Execute(UpdaterData data)
        {
            Autodesk.Revit.DB.Document doc = data.GetDocument();
            Schema sch = Schema.Lookup(new Guid("2B195204-1C04-4538-8881-AD22FA697B41"));
            ExtensibleStorageFilter filter = new ExtensibleStorageFilter(new Guid("2B195204-1C04-4538-8881-AD22FA697B41"));
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> tags=collector.WherePasses(filter).ToElements();
            foreach(Element tag in tags)
            {
                Entity retrievedEntity = tag.GetEntity(sch);
                if (retrievedEntity.Get<ElementId>("SelElmIds") == ElementId.InvalidElementId)
                    doc.Delete(tag.Id);
            }
        }

        public string GetAdditionalInformation()
        {
            return "N/A";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.DetailComponents;
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public string GetUpdaterName()
        {
            return "Test 2";
        }
    }

}
