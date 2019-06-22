using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace TektaRevitPlugins
{
    static class RoomUtils
    {
        public static ICollection<Apartment> GetApartments(Document doc) {
            try {
                FilteredElementCollector collector =
                    new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .OfClass(typeof(SpatialElement));

                IDictionary<string, Apartment> apartments =
                    new Dictionary<string, Apartment>();

                foreach (SpatialElement room in collector.ToElements()) {
                    if (room.LookupParameter("APARTMENT NUMBER") != null &&
                       room.LookupParameter("APARTMENT NUMBER").AsString() != null &&
                       room.LookupParameter("APARTMENT NUMBER").AsString().Length != 0) {
                        string aptNum = room.LookupParameter("APARTMENT NUMBER").AsString();

                        if (apartments.ContainsKey(aptNum)) {
                            Apartment apt = apartments[aptNum];
                            apt.AddRoom(room);
                        }
                        else {
                            Apartment newApartment = new Apartment(aptNum);
                            newApartment.AddRoom(room);
                            apartments.Add(aptNum, newApartment);
                        }
                    }
                }

                return apartments.Values;

            }
            catch (Exception ex) {
                throw ex;
            }
        }
    }
}
