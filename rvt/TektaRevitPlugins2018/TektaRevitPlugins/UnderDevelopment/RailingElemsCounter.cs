using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Selection = Autodesk.Revit.UI.Selection.Selection;
using Tracer = System.Diagnostics.Trace;
using Autodesk.Revit.DB.Architecture;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class RailingElemsCounter : Autodesk.Revit.UI.IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            // Add an EventLogTraceListener object
            System.Diagnostics.Trace.Listeners.Add(
               new System.Diagnostics.EventLogTraceListener("Application"));

            // Lay the hands on the active document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Set up a timer
            Timing myTimer = new Timing();
            myTimer.StartTime();
            try {
                Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                Railing railing = doc.GetElement(r) as Railing;
                GeometryElement geomElem = railing.get_Geometry(new Options { IncludeNonVisibleObjects = true });

                RvtRailing rvtRailing = new RvtRailing();
                RailingType railingType =
                    doc.GetElement(railing.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsElementId())
                    as RailingType;

                // Extract and sort out the railign's parts
                foreach (GeometryObject geomObj in geomElem) {
                    if (geomObj is GeometryInstance)
                        GetRailingParts((GeometryInstance)geomObj, rvtRailing);
                }

                #region Tracing
                // Just a check on the number of total solids 
                // obtained from the railing
                IList<Solid> allSolids = new List<Solid>();

                // Get all solids from the revit railing element
                GetSolid(railing.get_Geometry(new Options { IncludeNonVisibleObjects = true }), allSolids);

                StringBuilder strBld = new StringBuilder();
                for (int i = 0; i < allSolids.Count; ++i)
                    strBld.AppendFormat("s{0}.Volume = {1}\n", i, allSolids[i].Volume);

                System.Diagnostics.Trace.Write(strBld.ToString());
                #endregion

                TaskDialog.Show("Parts", string.Format("Top Rails: {0}; Length = {1:F2}\nRails: {2}; Length = {1:F2}; Area = {6:F2}sq ft\nBalusters: {3}; Length = {4:F2}\nAllSolids: {5}",
                    rvtRailing.TopRails.Count, 
                    UnitUtils.ConvertFromInternalUnits(rvtRailing.TopRailLength, DisplayUnitType.DUT_MILLIMETERS), 
                    rvtRailing.Rails.Count, 
                    rvtRailing.Balusters.Count,
                    UnitUtils.ConvertFromInternalUnits(rvtRailing.BalusterLength, DisplayUnitType.DUT_MILLIMETERS),
                    allSolids.Count,
                    rvtRailing.Rails[0].Volume / rvtRailing.TopRailLength));

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

        static void GetSolid(GeometryElement geomElem, IList<Solid> solids) {
            foreach (GeometryObject geomObj in geomElem) {
                if (geomObj is Solid && ((Solid)geomObj).Volume > 0)
                    solids.Add((Solid)geomObj);
                else if (geomObj is GeometryInstance)
                    GetSolid((GeometryInstance)geomObj, solids);
            }
        }
        static void GetSolid(GeometryInstance geomInst, IList<Solid> solids) {
            foreach (GeometryObject geomObj in geomInst.SymbolGeometry) {
                if (geomObj is Solid && ((Solid)geomObj).Volume > 0)
                    solids.Add((Solid)geomObj);
                else if (geomObj is GeometryInstance)
                    GetSolid((GeometryInstance)geomObj, solids);
            }
        }

        static void GetRailingParts(GeometryInstance geomInst, RvtRailing rvtRailing) {
            if (geomInst.Symbol is TopRail) {
                GetSolid(geomInst.SymbolGeometry, rvtRailing.TopRails);
                rvtRailing.TopRailLength = ((TopRail)geomInst.Symbol).Length;
            }
            else {
                foreach (GeometryObject gObj in geomInst.SymbolGeometry) {
                    if (gObj is Solid) {
                        rvtRailing.AddRail((Solid)gObj);
                    }
                    else if (gObj is GeometryInstance)
                        GetSolid((GeometryInstance)gObj, rvtRailing.Balusters);
                    //GetSolid((GeometryInstance)gObj, rvtRailing);
                }
            }
        }
    }



    class RvtRailing
    {
        #region Data Fields
        IList<Solid> m_topRail;
        IList<Solid> m_balusters;
        IList<Solid> m_rails;
        double m_topRailRailLength;
        double m_sumRailAreas;
        #endregion

        #region Constructors
        public RvtRailing() {
            m_topRail = new List<Solid>();
            m_balusters = new List<Solid>();
            m_rails = new List<Solid>();
        }
        #endregion

        #region Properties
        public IList<Solid> TopRails {
            get { return m_topRail; }
        }
        public IList<Solid> Balusters {
            get { return m_balusters; }
        }
        public IList<Solid> Rails {
            get { return m_rails; }
        }
        internal double TopRailLength {
            get { return m_topRailRailLength; }
            set { m_topRailRailLength = value; }
        }
        internal double RailLength {
            get { return m_topRailRailLength; }
            set { m_topRailRailLength = value; }
        }
        internal double BalusterLength {
            get { return GetLength(m_balusters); }
        }
        #endregion

        #region Methods
        public void AddBaluster(Solid s) {
            AddElem(s, m_balusters);
        }
        public void AddTopRail(Solid s) {
            AddElem(s, m_topRail);
        }
        public void AddRail(Solid s) {
            AddElem(s, m_rails);
        }
        #endregion

        #region Helper Methods
        void AddElem<T>(T elem, IList<T> collection) {
            if (collection == null)
                collection = new List<T>();
            collection.Add(elem);
        }
        double GetLength(IList<Solid> solids) {
            Solid maxVolumeSolid =
                solids.OrderByDescending(s => s.Volume).ElementAt(0);

            double maxLength = 0;
            FaceArray faceArray = maxVolumeSolid.Faces;
            FaceArrayIterator itr = faceArray.ForwardIterator();
            while (itr.MoveNext()) {
                Face curFace = itr.Current as Face;
                if(curFace is CylindricalFace) {
                    EdgeArrayArray edgeArrArr = ((CylindricalFace)curFace).EdgeLoops;
                    foreach(EdgeArray edgeArr in edgeArrArr) {
                        foreach(Edge edge in edgeArr) {
                            if (edge.ApproximateLength > maxLength)
                                maxLength = edge.ApproximateLength;
                        }
                    }
                    return maxLength;
                }
                else {
                    EdgeArrayArray edgeArrArr = curFace.EdgeLoops;
                    foreach (EdgeArray edgeArr in edgeArrArr) {
                        foreach (Edge edge in edgeArr) {
                            if (edge.ApproximateLength > maxLength)
                                maxLength = edge.ApproximateLength;
                        }
                    }
                }
            }
            return maxLength;
        }
        #endregion
    }

}