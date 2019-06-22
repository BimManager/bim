using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Trace = System.Diagnostics.Trace;

using Room = Autodesk.Revit.DB.Architecture.Room;
using ElementId = Autodesk.Revit.DB.ElementId;
using XYZ = Autodesk.Revit.DB.XYZ;
using Document = Autodesk.Revit.DB.Document;
using UnitUtils = Autodesk.Revit.DB.UnitUtils;

namespace TektaRevitPlugins
{
    [Serializable]
    class Apartment
    {
        #region Data
        string m_number;
        double m_totalArea;
        double m_totalAreaSansPartitions;
        double m_totalAreaSansWetAreas;
        IList<double> m_roomAreas;
        IList<int> m_iRoomId;
        Document m_doc;
        #endregion

        #region Constructors
        public Apartment(Document doc, string number) {
            m_doc = doc;
            m_number = number;
            m_iRoomId = new List<int>();
            m_totalArea = 0;
            m_totalAreaSansPartitions = 0;
            m_totalAreaSansWetAreas = 0;
            m_roomAreas = new List<double>();
        }

        #endregion

        #region Properties
        internal IList<int> IDs {
            get { return m_iRoomId; }
        }
        internal double AreaNoWetAreas {
            get { return m_totalAreaSansWetAreas;
            }
        }
        internal double TotalArea {
            get { return KahanSum(m_roomAreas); }
        }
        #endregion

        #region Methods
        internal void AddRoom(Room room) {
            if (!m_iRoomId.Contains(room.Id.IntegerValue))
                m_iRoomId.Add(room.Id.IntegerValue);
            m_totalArea += room.Area;
            m_roomAreas.Add(room.Area);
            if (room.Number != "3")
                m_totalAreaSansWetAreas += room.Area;
        }
        internal IList<int> GetIds() {
            return m_iRoomId;
        }

        internal int GetRoomIdContainingCentroid() {
            XYZ centroid = GetAptCentroid();
            double distToClosestPnt = 100;
            Room outRoom = null;

            foreach (int id in m_iRoomId) {
                Room rm = m_doc.GetElement(new ElementId(id)) as Room;
                Autodesk.Revit.DB.LocationPoint currRoomCnt =
                    rm.Location as Autodesk.Revit.DB.LocationPoint;

                if (centroid.DistanceTo(currRoomCnt.Point) < distToClosestPnt) {
                    distToClosestPnt = centroid.DistanceTo(currRoomCnt.Point);
                    outRoom = rm;
                }
            }

            return outRoom.Id.IntegerValue;
        }
        public override string ToString() {
            return string.Format("Number: {0}; Total Area: {1};\nTotal Area sans wet areas: {2};\nTotal Area sans partitions: {3}",
                m_number, UnitUtils.ConvertFromInternalUnits(m_totalArea, Autodesk.Revit.DB.DisplayUnitType.DUT_SQUARE_METERS),
                UnitUtils.ConvertFromInternalUnits(m_totalAreaSansWetAreas, Autodesk.Revit.DB.DisplayUnitType.DUT_SQUARE_METERS),
                UnitUtils.ConvertFromInternalUnits(m_totalAreaSansPartitions, Autodesk.Revit.DB.DisplayUnitType.DUT_SQUARE_METERS));
        }

        #endregion

        #region Helper Methods
        private XYZ GetAptCentroid() {
            double x = 0, y = 0, z = 0;

            foreach (int id in m_iRoomId) {
                Room rm = m_doc.GetElement(new ElementId(id)) as Room;
                XYZ roomCntPnt = ((Autodesk.Revit.DB.LocationPoint)rm.Location).Point;
                x += roomCntPnt.X; y += roomCntPnt.Y;
                z = roomCntPnt.Z;
            }
            return new XYZ(x / m_iRoomId.Count, y / m_iRoomId.Count, z);
        }
        static double KahanSum(IList<double> dArr) {
            double dSum = 0;
            double dCom = 0;
            for (int i = 0; i < dArr.Count; ++i) {
                double dY = dArr[i] - dCom;
                double dT = Math.Round(dSum + dY, 2);
                dCom = (dT - dSum) - dY;
                Trace.Write(string.Format("dArr[{0}] = {1}; dSum={2}; dCom = {3}, dY = {4}; dT = {5}",
                    i, dArr[i], dSum, dCom, dY, dT));
                dSum = dT;
            }
            return dSum;
        }
        #endregion
    }
}
