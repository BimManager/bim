using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Tracer = System.Diagnostics.Trace;

using Autodesk.Revit.DB;

namespace TektaRevitPlugins
{
    class CustomSchedule
    {
        #region Data Fields
        ElementId m_createdScheduleId;
        Document m_doc;
        IDictionary<string, string> m_filtersValues;
        ScheduleType m_scheduleType;
        int m_counter;
        #endregion

        #region Properties
        internal ElementId ScheduleId
        {
            get { return m_createdScheduleId; }
            private set { m_createdScheduleId = value; }
        }
        #endregion

        #region Constructors
        internal CustomSchedule(Document doc,
            IDictionary<string, string> paramsValues, ScheduleType type)
        {
            m_doc = doc;
            m_filtersValues = paramsValues;
            m_scheduleType = type;
        }
        #endregion

        #region Methods
        internal void DuplicateSchedule(ViewSchedule protoSchedule)
        {
            try
            {   
                using (Transaction t =
                    new Transaction(m_doc, "Duplicate Schedule"))
                {
                    t.Start();
                    // Save the copied schedule's id
                    m_createdScheduleId = protoSchedule
                        .Duplicate(ViewDuplicateOption.Duplicate);
                    t.Commit();
                }
            }
            catch (Exception)
            {
                throw new Exception("Failed to copy a schedule.");
            }
        }

        internal void RenameSchedule(bool showTitle)
        {
            ++m_counter;
            using (Transaction t = new Transaction(m_doc, "Rename Schedule"))
            {
                ViewSchedule tmpSchedule =
                    m_doc.GetElement(m_createdScheduleId) as ViewSchedule;
                string newName = GenerateName(tmpSchedule.ViewName);
                try
                {
                    t.Start();
                    tmpSchedule.ViewName = newName;
                    // Find the shared parameter utilized for grouping 
                    // schedules in the schedule browser and set it 
                    // to a particualr value
                    tmpSchedule.LookupParameter(RebarsUtils.PARTITION)
                        .Set(m_filtersValues[RebarsUtils.PARTITION]);
                    // If this is the first schedule and
                    // Show Title is ticked, then 
                    // make the title visible
                    if (m_counter == 1 && showTitle)
                    {
                        tmpSchedule.Definition.ShowTitle = true;
                        try
                        {
                            if (m_scheduleType == ScheduleType.AssemblySchedule)
                                ChangeTitle(tmpSchedule, m_filtersValues[RebarsUtils.ASSEMBLY_MARK]);
                        }
                        catch (Exception ex)
                        {
                            Autodesk.Revit.UI.TaskDialog.Show("Exception", $"{ex.Message}\n{ex.StackTrace}.");
                        }
                    }
                    else
                        tmpSchedule.Definition.ShowTitle = false;
                }
                catch (Exception)
                {
                    if (t.GetStatus() == TransactionStatus.Started)
                    {
                        t.RollBack();
                    }
                    Autodesk.Revit.UI.TaskDialog.Show("Exception", $"{newName} already exists.");
                    using (Transaction tt = new Transaction(m_doc, "Clean up"))
                    {
                        tt.Start();
                        m_doc.Delete(ScheduleId);
                        tt.Commit();
                    }
                    m_createdScheduleId = null;
                    throw new RenameException();
                }
                t.Commit();
            }
        }
        internal void ResetFieldFilters()
        {
            if (m_createdScheduleId == null)
                return;
            try
            {
                using (Transaction t = new Transaction(m_doc, "Reset Filters"))
                {
                    ViewSchedule tmpSchedule =
                        m_doc.GetElement(m_createdScheduleId) as ViewSchedule;
                    t.Start();
                    foreach (string paramName in m_filtersValues.Keys)
                    {
                        FieldFilter fieldFilter =
                            new FieldFilter(tmpSchedule, paramName);
                        fieldFilter.UpdateFilter(m_filtersValues[paramName]);
                    }
                    t.Commit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region Helper Methods
        internal string GenerateName(string protoTitle)
        {
            string pattern = "[А-Яа-я]+";
            Regex regex = new Regex(pattern);
            string name = string.Empty;
            foreach (Match match in regex.Matches(protoTitle))
            {
                if (match.Value == "копия")
                    continue;
                name += match.Value + ' ';
            }
            name = name.Remove(name.Length - 1);

            string title = string.Empty;
            switch (m_scheduleType)
            {
                case ScheduleType.ElementSchedule:
                    title = string.Format("{0} - {1} - {2}",
                        m_filtersValues[RebarsUtils.PARTITION],
                        m_filtersValues[RebarsUtils.HOST_MARK], name);
                    break;
                case ScheduleType.AssemblySchedule:
                    title = string.Format("{0} - {1} - {2} - {3}",
                        m_filtersValues[RebarsUtils.PARTITION],
                        m_filtersValues[RebarsUtils.HOST_MARK],
                        m_filtersValues[RebarsUtils.ASSEMBLY_MARK], name);
                    break;
                case ScheduleType.ElementBarBending:
                    title = string.Format("{0} - {1} - {2}",
                        m_filtersValues[RebarsUtils.PARTITION],
                        m_filtersValues[RebarsUtils.HOST_MARK], name);
                    break;
                case ScheduleType.AssemblyBarBending:
                    title = string.Format("{0} - {1} - {2} - {3}",
                        m_filtersValues[RebarsUtils.PARTITION],
                        m_filtersValues[RebarsUtils.HOST_MARK],
                        m_filtersValues[RebarsUtils.ASSEMBLY_MARK], name);
                    break;
                case ScheduleType.MaterialTakeOff:
                    title = string.Format("{0} - {1}",
                        m_filtersValues[RebarsUtils.PARTITION], name);
                    break;
                case ScheduleType.PartitionSheetList:
                    title = string.Format("{0} - {1}",
                        m_filtersValues[RebarsUtils.PARTITION], name);
                    break;
                case ScheduleType.BillOfQuantities:
                    title = string.Format("{0} - {1}",
                        m_filtersValues[RebarsUtils.PARTITION], name);
                    break;
            }
            return title;
        }

        internal bool IsEmpty()
        {
            if (m_createdScheduleId != null)
            {
                ViewSchedule vs = m_doc.GetElement(m_createdScheduleId) as ViewSchedule;
                TableSectionData tableData = vs.GetTableData().GetSectionData(SectionType.Body);
                if (tableData.LastRowNumber == -1)
                {
                    return true;
                }
                return false;
            }
            throw new Exception("Schedule id is null.");
        }

        internal void HideZeroFieldsMaterialTakeOff()
        {
            ViewSchedule viewSchedule = m_doc.GetElement(m_createdScheduleId) as ViewSchedule;
            ScheduleDefinition scheduleDef = viewSchedule.Definition;
            TableData tableData = viewSchedule.GetTableData();
            TableSectionData tableSecData = tableData.GetSectionData(SectionType.Body);
            List<ScheduleField> visibleFields = scheduleDef.GetFieldOrder().Select(id => scheduleDef.GetField(id)).Where(f => !f.IsHidden).ToList();

            System.Text.StringBuilder strBld = new System.Text.StringBuilder();

            using (Transaction t = new Transaction(viewSchedule.Document))
            {
                t.Start("Hide Zeroed Fields");
                for (int i = tableSecData.FirstColumnNumber; i <= tableSecData.LastColumnNumber; ++i)
                {
                    if (tableSecData.GetCellText(tableSecData.FirstRowNumber + 5, i) == "0")
                    {
                        double total = 0;

                        for (int j = tableSecData.FirstRowNumber + 5; j <= tableSecData.LastRowNumber; ++j)
                        {
                            strBld.AppendFormat("{0} = {1}; column = {2}\n", 
                                tableSecData.GetCellText(tableSecData.FirstRowNumber + 4, i), tableSecData.GetCellText(j, i), i);
                            double tmp;
                            if (Double.TryParse(tableSecData.GetCellText(j, i), out tmp))
                            {
                                total += tmp;
                            }
                        }

                        if (total == 0)
                        {
                            visibleFields[i].IsHidden = true;
                        }

                    }
                }
                t.Commit();
            }
            Tracer.Write(strBld.ToString());
        }

        void ChangeTitle(ViewSchedule vs, string assemblyName)
        {
            ScheduleDefinition sd = vs.Definition;
            TableData td = vs.GetTableData();
            TableSectionData tsd = td.GetSectionData(SectionType.Header);
            string oldValue = tsd.GetCellText(tsd.FirstRowNumber, tsd.FirstColumnNumber);
            Tracer.Write($"oldValue={oldValue}");
            string newValue = string.Empty;
            for (int i = 0; i < oldValue.Split(' ').Length - 1; i++)
                newValue += oldValue.Split(' ')[i] + " ";
            newValue += assemblyName;
            Tracer.Write($"newValue={newValue}");
            tsd.SetCellText(tsd.FirstRowNumber, tsd.FirstColumnNumber, newValue);
        }
        #endregion

        #region Nested Classes
        private class FieldFilter
        {
            ScheduleDefinition m_scheduleDef;
            internal int Index { get; private set; }
            internal ScheduleFilter Filter { get; private set; }
            internal string ParameterName { get; private set; }

            internal FieldFilter(ViewSchedule vs, string paramName)
            {
                ParameterName = paramName;
                m_scheduleDef = vs.Definition;

                for (int i = 0; i < m_scheduleDef.GetFilterCount(); ++i)
                {
                    ScheduleFilter sFilter =
                        m_scheduleDef.GetFilter(i);

                    if (m_scheduleDef.GetField(sFilter.FieldId).GetName() == ParameterName)
                    {
                        Index = i;
                        Filter = sFilter;
                    }
                }
            }

            // bypasses static type checking 
            internal void UpdateFilter(dynamic value)
            {
                if (Filter != null)
                {
                    Filter.SetValue(value);
                    m_scheduleDef.SetFilter(Index, Filter);
                }
            }
        }
        #endregion
    }

}
