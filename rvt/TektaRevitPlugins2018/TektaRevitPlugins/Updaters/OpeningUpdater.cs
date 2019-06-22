using System;

using UpdaterRegistry = Autodesk.Revit.DB.UpdaterRegistry;
using UpdaterId = Autodesk.Revit.DB.UpdaterId;
using Document = Autodesk.Revit.DB.Document;
using ElementParameterFilter = Autodesk.Revit.DB.ElementParameterFilter;
using ParameterFilterRuleFactory = Autodesk.Revit.DB.ParameterFilterRuleFactory;
using Element = Autodesk.Revit.DB.Element;
using ElementId = Autodesk.Revit.DB.ElementId;
using FamilyInstance = Autodesk.Revit.DB.FamilyInstance;
using BuiltInParameter = Autodesk.Revit.DB.BuiltInParameter;

namespace TektaRevitPlugins
{
    class OpeningUpdater : Autodesk.Revit.DB.IUpdater
    {
        #region Data Fields
        static UpdaterId m_updaterId;
        const string SO_BTM_ELEV 
            = "S/O BTM ELEV";
        const string SO_FAMILY_NAME 
            = "Z-M3-WAllBasedApertureMaker-LOD2";
        #endregion

        #region Properties
        internal string GetFamilyName {
            get { return SO_FAMILY_NAME; }
        }
        internal string GetParameterName {
            get { return SO_BTM_ELEV; }
        }
        #endregion

        #region Constructors
        internal OpeningUpdater(Autodesk.Revit.DB.AddInId addInId)
        {
            m_updaterId = new UpdaterId(addInId,
                new Guid("98B33153-D81D-45BF-8AEC-FAF080BF27E9"));
        }
        #endregion

        #region Methods
        // Register itself with Revit
        internal void Register(Document doc)
        {
            UpdaterRegistry.RegisterUpdater(this, doc, true);
        }

        internal void AddTriggerForUpdater(Document doc)
        {
            ElementParameterFilter familyNameFilter =
              new ElementParameterFilter(ParameterFilterRuleFactory
            .CreateEqualsRule(new ElementId
            (BuiltInParameter.ALL_MODEL_FAMILY_NAME), GetFamilyName, false));

            UpdaterRegistry.AddTrigger(m_updaterId, doc, familyNameFilter,
                Element.GetChangeTypeElementAddition());

            UpdaterRegistry.AddTrigger(m_updaterId, doc, familyNameFilter,
                Element.GetChangeTypeAny());
        }

        public void Execute(Autodesk.Revit.DB.UpdaterData data)
        {
            Document doc = data.GetDocument();
            try {
                Action<Document, ElementId> setElev = (d, id) => {
                    FamilyInstance fi = d.GetElement(id) as FamilyInstance;
                    SetBottomElev(fi);
                };

                foreach(ElementId addedElemId in data.GetAddedElementIds())
                    setElev(doc, addedElemId);

                foreach (ElementId changedElemId in data.GetModifiedElementIds()) 
                  setElev(doc, changedElemId);
            }
            catch(Exception ex) {
                Autodesk.Revit.UI.TaskDialog.Show("Exception",(string.Format
                    ("{0}\n{1}", ex.Message, ex.StackTrace)));
            }
        }

        public string GetAdditionalInformation()
        {
            return "Updates the S/O BTM ELEV parameter";
        }

        public Autodesk.Revit.DB.ChangePriority GetChangePriority()
        {
            return Autodesk.Revit.DB.ChangePriority.DoorsOpeningsWindows;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "OpeningUpdater";
        }
        #endregion

        #region Helper Methods
        static void SetBottomElev(FamilyInstance fi)
        {
            Document doc = fi.Document;
            if (fi.LookupParameter(SO_BTM_ELEV) == null)
                throw new Exception(string.Format("{0} is null", nameof(SO_BTM_ELEV)));
            double elev = ((Autodesk.Revit.DB.Level)doc
                .GetElement(fi.LevelId)).Elevation + fi.get_Parameter
                (BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsDouble();
            fi.LookupParameter(SO_BTM_ELEV)
                .Set(elev);
        }
        #endregion
    }


}
