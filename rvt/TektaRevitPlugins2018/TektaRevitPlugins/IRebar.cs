using Autodesk.Revit.DB;
using IComparer = System.Collections.IComparer;
using ElementId = Autodesk.Revit.DB.ElementId;
using FamilyInstance = Autodesk.Revit.DB.FamilyInstance;
using UnitUtils = Autodesk.Revit.DB.UnitUtils;
using System.Collections.Generic;
using System;

namespace TektaRevitPlugins
{
    internal interface IRebar: IComparer
    {
        #region Properties
        ElementId Id { get; set; }
        string Mark { get; set; }
        byte Diameter { get; set; }
        float Length { get; set; }
        string Partition { get; set; }
        string HostMark { get; set; }
        string AssemblyMark { get; set; }
        bool IsScheduled { get; set; }
        bool BelongsToRebarCage { get; set; }
        bool IsWeighedPerMetre { get; set; }
        #endregion

        #region Methods

        #endregion

        #region Events

        #endregion

        #region Indexers

        #endregion
    }

    class GeneralRebar : IRebar
    {
        #region Data Fields
        ElementId m_id;
        string m_mark;
        byte m_diameter;
        float m_length;
        string m_partition;
        string m_hostMark;
        string m_assemblyMark;
        bool m_isScheduled;
        bool m_belongsToRebarCage;
        bool m_isWeighedPerMetre;     
        #endregion

        #region Properties
        public ElementId Id {
            get { return m_id; }
            set { m_id = value; }
        }
        public string Mark {
            get { return m_mark; }
             set { m_mark = value; }
        }
        public byte Diameter {
            get { return m_diameter; }
             set { m_diameter = value; }
        }
        public float Length {
            get { return m_length; }
             set { m_length = value; }
        }
        public string Partition {
            get { return m_partition; }
             set { m_partition = value; }
        }
        public string HostMark {
            get { return m_hostMark; }
             set { m_hostMark = value; }
        }
        public string AssemblyMark {
             set { m_assemblyMark = value; }
            get { return m_assemblyMark; }
        }
        public bool IsScheduled {
            get { return m_isScheduled; }
             set { m_isScheduled = value; }
        }
        public bool BelongsToRebarCage {
            get { return m_belongsToRebarCage; }
             set { m_belongsToRebarCage = value; }
        }
        public bool IsWeighedPerMetre {
            get { return m_isWeighedPerMetre; }
            set { m_isWeighedPerMetre = value; }
        }
        #endregion

        #region Constructors
        internal GeneralRebar() { }
        internal GeneralRebar(FamilyInstance rebar)
        {
            Id = rebar.Id;
            Partition = rebar.LookupParameter(RebarsUtils.PARTITION) != null ?
                rebar.LookupParameter(RebarsUtils.PARTITION).AsString() : string.Empty;
            HostMark = rebar.LookupParameter(RebarsUtils.HOST_MARK) != null ?
                rebar.LookupParameter(RebarsUtils.HOST_MARK).AsString() : string.Empty;
            Diameter = (byte)UnitUtils
                .ConvertFromInternalUnits(rebar.Symbol
                .LookupParameter(RebarsUtils.DIAMETER) != null ?
                rebar.Symbol.LookupParameter(RebarsUtils.DIAMETER).AsDouble() : 0,
                DisplayUnitType.DUT_MILLIMETERS);
            Length = (short)UnitUtils
                .ConvertFromInternalUnits(rebar.LookupParameter(RebarsUtils.LENGTH) != null ?
                rebar.LookupParameter(RebarsUtils.LENGTH).AsDouble() : 0,
                DisplayUnitType.DUT_MILLIMETERS);
            IsScheduled = rebar.LookupParameter(RebarsUtils.IS_SPECIFIABLE) != null ?
                (rebar.LookupParameter(RebarsUtils.IS_SPECIFIABLE).AsInteger() == 1 ? true : false) : false;
            IsWeighedPerMetre = rebar.LookupParameter(RebarsUtils.WEIGHT_PER_METER) != null ?
                (rebar.LookupParameter(RebarsUtils.WEIGHT_PER_METER).AsInteger() == 1 ? true : false) : false;
            BelongsToRebarCage = rebar.LookupParameter(RebarsUtils.IS_IN_ASSEMBLY) != null ?
                (rebar.LookupParameter(RebarsUtils.IS_IN_ASSEMBLY).AsInteger() == 1 ? true : false) : false;
            AssemblyMark = rebar.LookupParameter(RebarsUtils.ASSEMBLY_MARK) != null ?
                rebar.LookupParameter(RebarsUtils.ASSEMBLY_MARK).AsString() : string.Empty;
        }
        #endregion

        #region Methods
        public  int Compare(object x, object y)
        {
            GeneralRebar rebar1 = (GeneralRebar)x;
            GeneralRebar rebar2 = (GeneralRebar)y;
            if (rebar1.Diameter == rebar2.Diameter &&
                rebar1.Length > rebar2.Length)
                return 1;
            else if (rebar1.Diameter == rebar2.Diameter &&
                rebar1.Length < rebar2.Length)
                return -1;
            else if (rebar1.Diameter == rebar2.Diameter &&
                rebar1.Length == rebar2.Length)
                return 0;
            else if (rebar1.Diameter > rebar2.Diameter)
                return 1;
            else
                return -1;
        }
        #endregion
    }

    internal class RebarContainer
    {
        #region Data Fields
        IList<GeneralRebar> m_rebars;
        #endregion

        #region Constructors
        public RebarContainer() {
            m_rebars = new List<GeneralRebar>();
        }
        public RebarContainer(IList<GeneralRebar> rebars)
        {
            m_rebars = rebars;
        }
        #endregion

        #region Methods
        public void Add(GeneralRebar rebar)
        {
            m_rebars.Add(rebar);
        }

        public ISet<string> GetPartitions()
        {
            ISet<string> partitions = new SortedSet<string>();
            foreach(GeneralRebar rebar in m_rebars) {
                partitions.Add(rebar.Partition);
            }
            return partitions;
        }
        public ISet<string> GetHostMarks(string partition)
        {
            ISet<string> hostMarks = new SortedSet<string>();
            foreach(GeneralRebar rebar in m_rebars) {
                if (rebar.Partition == partition)
                    hostMarks.Add(rebar.HostMark);
            }
            return hostMarks;
        }
        public ISet<string> GetAssemblies(string partition, string hostMark)
        {
            ISet<string> assemblies = new SortedSet<string>();
            foreach (GeneralRebar rebar in m_rebars) {
                if (rebar.Partition == partition && rebar.HostMark == hostMark)
                    assemblies.Add(rebar.AssemblyMark);
            }
            return assemblies;
        }
        public IDictionary<string, ISet<string>> GetHostAssemblies()
        {
            IDictionary<string, ISet<string>> dictionary =
                new Dictionary<string, ISet<string>>();
            foreach(GeneralRebar rebar in m_rebars) {
                string key = rebar.Partition + rebar.HostMark;
                if(!dictionary.ContainsKey(key) && 
                    rebar.AssemblyMark.Length != 0) {
                    ISet<string> assebmlies =
                        new SortedSet<string> { rebar.AssemblyMark };
                    dictionary.Add(key, assebmlies);
                }
                else if (rebar.AssemblyMark.Length != 0) {
                    ISet<string> assebmlies = dictionary[key];
                    assebmlies.Add(rebar.AssemblyMark);
                    dictionary[key] = assebmlies;
                }
            }
            return dictionary;
        }
        #endregion


    }
}
