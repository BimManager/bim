using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace TektaRevitPlugins
{
    internal enum ScheduleType: byte
    {
        ElementSchedule,
        AssemblySchedule,
        ElementBarBending,
        AssemblyBarBending,
        MaterialTakeOff,
        BillOfQuantities,
        PartitionSheetList
    }
    internal class ComboScheduleGenerator
    {
        // hardcoded border width value
        const double BORDER_WIDTH = 0.00694444444444624; 
        #region Data Fields
        IList<ElementId> m_scheduleIds =
            new List<ElementId>();
        string m_scheduleCode;
        bool m_placeOnSheet = true;
        bool m_title;
        CustomSchedule m_customSchedule;
        ScheduleType m_scheduleType;
        #endregion

        #region Constructors
        public ComboScheduleGenerator(Document doc, ScheduleType type, 
            IDictionary<string, string> paramsValuesDic, bool showTitle)
        {
            m_customSchedule = new CustomSchedule(doc, paramsValuesDic, type);
            m_title = showTitle;
            m_scheduleType = type;

            switch (type)
            {
                case ScheduleType.ElementSchedule:
                    m_scheduleCode = "*30 - A";
                    break;
                case ScheduleType.AssemblySchedule:
                    m_scheduleCode = "*30 - B";
                    break;
                case ScheduleType.ElementBarBending:
                    m_scheduleCode = "*30 - D1";
                    break;
                case ScheduleType.AssemblyBarBending:
                    m_scheduleCode = "*30 - D2";
                    break;
                case ScheduleType.MaterialTakeOff:
                    m_scheduleCode = "*30 - E1";
                    break;
                case ScheduleType.PartitionSheetList:
                    m_scheduleCode = "*30 - L1";
                    break;
                case ScheduleType.BillOfQuantities:
                    m_scheduleCode = "#30 - E2";
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Methods
        IList<ViewSchedule> GetNecessarySchedules(Document doc, string scheduleCode)
        {
            ElementParameterFilter viewNameFilter =
                new ElementParameterFilter(
                    ParameterFilterRuleFactory
                    .CreateBeginsWithRule(
                        new ElementId(BuiltInParameter.VIEW_NAME),
                        scheduleCode, false));
            return
                new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSchedule))
                    .WherePasses(viewNameFilter)
                    .Cast<ViewSchedule>()
                    .OrderBy(vs => vs.ViewName)
                    .ToList();
        }

        ViewSchedule FindScheduleByName(Document doc, string scheduleName)
        {
            ElementParameterFilter viewNameFilter =
                new ElementParameterFilter(
                    ParameterFilterRuleFactory
                    .CreateBeginsWithRule(
                        new ElementId(BuiltInParameter.VIEW_NAME),
                        scheduleName, false));
            return
                new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSchedule))
                    .WherePasses(viewNameFilter)
                    .Cast<ViewSchedule>()
                    .FirstOrDefault();
        }

        internal void CreateSubSchedules(Document doc)
        {
            try
            {
                foreach (ViewSchedule vs in GetNecessarySchedules(doc, m_scheduleCode))
                {
                    // Check whether a schedule with the same name already exists
                    ViewSchedule foundSchedule = 
                        FindScheduleByName(doc, m_customSchedule.GenerateName(vs.ViewName));
                    if (foundSchedule != null)
                    {
                        m_scheduleIds.Add(foundSchedule.Id);
                        continue;
                    }

                    // Duplicate the current schedule
                    m_customSchedule.DuplicateSchedule(vs);

                    try
                    {
                        // Rename the schedule
                        m_customSchedule.RenameSchedule(m_title);
                    }
                    catch (RenameException)
                    {
                        m_placeOnSheet = false;
                    }

                    // Reset the schedule filters
                    m_customSchedule.ResetFieldFilters();

                    if (m_customSchedule.ScheduleId != null)
                    {
                        m_scheduleIds.Add(m_customSchedule.ScheduleId);
                    }

                    // Hide all columns with values equal zero
                    if(m_scheduleType == ScheduleType.MaterialTakeOff)
                    {
                        m_customSchedule.HideZeroFieldsMaterialTakeOff();
                    }

                }
            }
            catch(Exception ex)
            {
                Autodesk.Revit.UI.TaskDialog.Show("Exception", ex.Message);
                throw ex;
            }
        }

        internal void PlaceOnSheet(ViewSheet sheet)
        {
            if (!m_placeOnSheet)
            {
                return;
            }
            try
            {
                Document doc = sheet.Document;
                using (Transaction t = new Transaction(doc, "Place on sheet"))
                {
                    t.Start();
                    XYZ placementPoint = new XYZ(0, 0, 0);
                    foreach (ElementId scheduleId in m_scheduleIds)
                    {
                        ScheduleSheetInstance scheduleInstance =
                            ScheduleSheetInstance.Create(doc, sheet.Id, scheduleId, placementPoint);

                        placementPoint = new XYZ(0, scheduleInstance.get_BoundingBox(sheet).Min.Y + BORDER_WIDTH, 0);
                    }
                    t.Commit();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
