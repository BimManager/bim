using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;

namespace TektaRevitPlugins
{
    class RoomUpdater : IUpdater
    {
        static AddInId m_addInId;
        static UpdaterId m_updaterId;

        public RoomUpdater(AddInId addInId)
        {
            m_addInId = addInId;
            m_updaterId = 
                new UpdaterId(m_addInId, new Guid("6945200D-DB7F-489D-8578-6C8E9A31D81A"));
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            foreach(ElementId id in data.GetAddedElementIds()) {
                string comment = string.Empty;
                Room room = doc.GetElement(id) as Room;
                RoomProperties window = new RoomProperties();
                window.Show();
                //if((bool)window.ShowDialog()) {
                //  comment = window.Test;
                //}
                room.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).Set(comment);
            }
        }

        public string GetAdditionalInformation()
        {
            return "N/A";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.RoomsSpacesZones;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "N/A";
        }
    }
}
