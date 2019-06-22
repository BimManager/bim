using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace TektaRevitPlugins.Multischedule
{
    class MultischeduleManipulator
    {
        #region Data Fields
        // hardcoded border width value
        const double BORDER_WIDTH = 0.00694444444444624;
        Document m_doc;
        #endregion

        #region Constructors
        public MultischeduleManipulator(Document doc)
        {
            m_doc = doc;
        }
        #endregion

        #region Methods
        internal void SetSubschedules(IMultitableSchedule multiSchd)
        {
            for (int i = 0; i < multiSchd.ViewNameEndings.Count; ++i)
            {
                ElementId tmplId =
                    FindTemplateByViewNameEnding(multiSchd.ViewNameEndings[i]);

                if (tmplId != null)
                {
                    Subschedule subSchedule =
                        new Subschedule
                        {
                            Id = tmplId,
                            IsTemplate = true,
                            ViewName = GenerateViewName(multiSchd, i),
                            ShowTitle = false
                        };

                    if (i == 0 && multiSchd.FiltersValues[
                        MultischeduleParameters.SHOW_TITLE].Equals("1"))
                    {
                        subSchedule.ShowTitle = true;
                    }
                    multiSchd.Subschedules.Add(subSchedule);
                }
            }
        }

        internal void FindExistingSchedules(IMultitableSchedule multiSchedule)
        {
            for (int i = 0; i < multiSchedule.Subschedules.Count; ++i)
            {
                ElementId extSchdId = FindScheduleByName(multiSchedule.Subschedules[i].ViewName);
                if (extSchdId != null)
                {
                    multiSchedule.Subschedules[i].Id = extSchdId;
                    multiSchedule.Subschedules[i].IsTemplate = false;
                }
                else
                {
                    multiSchedule.Templates.Add(multiSchedule.Subschedules[i]);
                }

            }
        }

        internal void DuplicateSchedules(Subschedule template)
        {
            ViewSchedule vs = m_doc.GetElement(template.Id)
                as ViewSchedule;

            template.Id =
                vs.Duplicate(ViewDuplicateOption.Duplicate);
        }

        internal void RenameSchedules(Subschedule template)
        {
            ViewSchedule vs = m_doc.GetElement(template.Id)
                as ViewSchedule;
            vs.ViewName = template.ViewName;
        }

        internal void ShowTitle(Subschedule template)
        {
            ViewSchedule vs = m_doc.GetElement(template.Id)
                    as ViewSchedule;

            if (template.ShowTitle)
            {
                vs.Definition.ShowTitle = true;
            }
        }

        internal void SetNewTitle(IMultitableSchedule multiSchd)
        {
            if (multiSchd is AssemblySchedule && multiSchd.Subschedules[0].ShowTitle)
            {
                ViewSchedule vs = m_doc.GetElement(multiSchd.Subschedules[0].Id)
                    as ViewSchedule;

                string newTitle = "Спецификация элементов " +
                    multiSchd.FiltersValues[MultischeduleParameters.ASSEMBLY_MARK];

                vs.GetTableData()
                    .GetSectionData(SectionType.Header)
                    .SetCellText(vs.GetTableData().GetSectionData(SectionType.Header).FirstRowNumber,
                    vs.GetTableData().GetSectionData(SectionType.Header).FirstColumnNumber,
                    newTitle);
            }
        }

        internal void UpdateFilters(Subschedule template, IMultitableSchedule multiSchedule)
        {
            ViewSchedule vs = m_doc.GetElement(template.Id)
                as ViewSchedule;

            for (int j = 0; j < multiSchedule.FiltersValues.Count; ++j)
            {
                string fltrName = multiSchedule.FiltersValues.Keys.ElementAt(j);
                dynamic value = multiSchedule.FiltersValues[fltrName];

                UpdateFilter(fltrName, value, vs.Definition);
            }
        }

        internal bool DiscardEmptySchedule(Subschedule template)
        {
            if (IsEmpty(template.Id))
            {
                m_doc.Delete(template.Id);
                template.Id = ElementId.InvalidElementId;
                return true;
            }
            return false;
        }

        internal void SetBlockAndPartition(Subschedule template, IMultitableSchedule multiSchedule)
        {
            if (template.Id == ElementId.InvalidElementId)
            {
                return;
            }

            Element subSchd = m_doc.GetElement(template.Id);

            //Parameter blockNo = subSchd.LookupParameter(RebarsUtils.BLOCK_NUMBER);
            Parameter blockNo = subSchd.LookupParameter(
                MultischeduleParameters.BLOCK_NUMBER);
            //Parameter partition = subSchd.LookupParameter(RebarsUtils.PARTITION);
            Parameter partition = subSchd.LookupParameter(
                MultischeduleParameters.PARTITION);

            if (blockNo != null)
            {
                blockNo.Set(multiSchedule
                    .FiltersValues[MultischeduleParameters.BLOCK_NUMBER]);
            }

            if (partition != null)
            {
                partition.Set(multiSchedule
                    .FiltersValues[MultischeduleParameters.PARTITION]);
            }
        }

        internal void CarryOutSpecificOperations(IMultitableSchedule multiSchd)
        {
            if (multiSchd is RebarQuantityTakeoff)
            {
                ((RebarQuantityTakeoff)multiSchd)
                    .HideZeroFieldsMaterialTakeOff(m_doc);
            }
            else if (multiSchd is BarBendingByStructure ||
                multiSchd is BarBendingByAssembly)
            {
                System.Diagnostics.Trace.Write(
                    string.Format("Id: {0}", multiSchd.Subschedules[0].Id));
                HideEmptyColumns(multiSchd.Subschedules[0].Id);
            }
        }

        internal void PlaceOnSheet(ViewSheet sheet, IMultitableSchedule multiSchedule)
        {
            XYZ placementPoint = new XYZ(0, 0, 0);

            for (int i = 0; i < multiSchedule.Subschedules.Count; ++i)
            {
                if (multiSchedule.Subschedules[i].Id == ElementId.InvalidElementId)
                {
                    continue;
                }

                ScheduleSheetInstance scheduleInstance =
                    ScheduleSheetInstance
                    .Create(m_doc, sheet.Id, multiSchedule.Subschedules[i].Id, placementPoint);

                placementPoint =
                    new XYZ(0, scheduleInstance.get_BoundingBox(sheet).Min.Y + BORDER_WIDTH, 0);
            }
        }
        #endregion

        #region Helper Methods
        private string GenerateViewName(IMultitableSchedule multiSchd, int index)
        {
            string partition;
            string hostMark;
            string assembly;

            if (multiSchd.FiltersValues.TryGetValue(
                MultischeduleParameters.PARTITION, out partition) &&
                multiSchd.FiltersValues.TryGetValue(
                    MultischeduleParameters.HOST_MARK, out hostMark) &&
                    multiSchd.FiltersValues.TryGetValue(
                        MultischeduleParameters.ASSEMBLY_MARK, out assembly))
            {
                return string.Format("{0} - {1} - {2} - {3}",
                            partition,
                            hostMark,
                            assembly,
                            multiSchd.ViewNameEndings[index]
                            );
            }
            else if (multiSchd.FiltersValues.TryGetValue(
                MultischeduleParameters.PARTITION, out partition) &&
                multiSchd.FiltersValues.TryGetValue(
                    MultischeduleParameters.HOST_MARK, out hostMark))
            {
                return string.Format("{0} - {1} - {2}",
                            partition,
                            hostMark,
                            multiSchd.ViewNameEndings[index]
                            );
            }
            else if (multiSchd.FiltersValues
                .TryGetValue(MultischeduleParameters.PARTITION, out partition))
            {
                return string.Format("{0} - {1}",
                            partition,
                            multiSchd.ViewNameEndings[index]
                            );
            }
            else
            {
                throw new NotImplementedException("Partion and hostMark are both null.");
            }
        }

        private ElementId FindTemplateByViewNameEnding(string viewNameEnding)
        {
            const int PARTITION_PARAMETER_ID = 2698521;
            ElementId partitionParamId = new ElementId(PARTITION_PARAMETER_ID);

            ElementParameterFilter viewNameFilter =
                new ElementParameterFilter(
                    ParameterFilterRuleFactory
                    .CreateEndsWithRule(
                        new ElementId(BuiltInParameter.VIEW_NAME),
                        viewNameEnding, false));

            ElementParameterFilter partitionFilter =
                new ElementParameterFilter(
                    ParameterFilterRuleFactory
                    .CreateBeginsWithRule(partitionParamId, "#Стандарт", false));

            LogicalAndFilter andFilter = new LogicalAndFilter(viewNameFilter, partitionFilter);

            return
                new FilteredElementCollector(m_doc)
                    .OfCategory(BuiltInCategory.OST_Schedules)
                    .WherePasses(andFilter)
                    .ToElementIds()
                    .FirstOrDefault();
        }

        private ElementId FindScheduleByName(string scheduleName)
        {
            ElementParameterFilter viewNameFilter =
                new ElementParameterFilter(
                    ParameterFilterRuleFactory
                    .CreateBeginsWithRule(
                        new ElementId(BuiltInParameter.VIEW_NAME),
                        scheduleName, false));

            return
                new FilteredElementCollector(m_doc)
                    .OfCategory(BuiltInCategory.OST_Schedules)
                    .WherePasses(viewNameFilter)
                    .ToElementIds()
                    .FirstOrDefault();
        }

        private void UpdateFilter(string filterName,
           dynamic newValue, ScheduleDefinition scheduleDefinition)
        {
            ScheduleFilter filter = null;
            int index = 0;

            for (int i = 0; i < scheduleDefinition.GetFilterCount(); ++i)
            {
                ScheduleFilter curFilter = scheduleDefinition.GetFilter(i);
                ScheduleField field = scheduleDefinition.GetField(curFilter.FieldId);

                if (field.GetName().Equals(filterName))
                {
                    index = i;
                    filter = curFilter;
                    break;
                }
            }

            if (newValue != null && filter != null && scheduleDefinition != null)
            {
                filter.SetValue(newValue);
                scheduleDefinition.SetFilter(index, filter);
            }
        }
        private bool IsEmpty(ElementId schdId)
        {
            if (schdId != null)
            {
                ViewSchedule vs = m_doc
                    .GetElement(schdId) as ViewSchedule;
                TableSectionData tableData = vs.GetTableData()
                    .GetSectionData(SectionType.Body);

                bool showHeaders = vs.Definition.ShowHeaders;

                System.Diagnostics.Trace.Write(
                    string.Format("LastRowNumber = {0}", tableData.LastRowNumber));

                if (tableData.LastRowNumber == -1)
                {
                    return true;
                }
                else if (showHeaders && 
                    tableData.LastRowNumber == 0)
                {
                    return true;
                }
                return false;
            }
            throw new Exception("Schedule id is null.");
        }

        private void HideEmptyColumns(ElementId id)
        {
            if (id == ElementId.InvalidElementId)
            {
                return;
            }

            ViewSchedule viewSchd = m_doc.GetElement(id)
                as ViewSchedule;
            ScheduleDefinition schdDef = viewSchd.Definition;
            TableSectionData tableData =
                viewSchd.GetTableData().GetSectionData(SectionType.Body);

            Func<ScheduleFieldId, bool> filterHiddenFields = delegate (ScheduleFieldId fId)
            {
                if (schdDef.GetField(fId).IsHidden)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            };

            IList<ScheduleFieldId> visibleFields = schdDef
                .GetFieldOrder()
                .Where(filterHiddenFields)
                .ToList();

            StringBuilder strBld = new StringBuilder();

            for (int i = tableData.FirstColumnNumber + 2; i <= tableData.LastColumnNumber; ++i)
            {
                double sum = 0;

                for (int j = tableData.FirstRowNumber + 1; j <= tableData.LastRowNumber; ++j)
                {
                    string cellContent = tableData.GetCellText(j, i);
                    strBld.AppendFormat("({0}, {1}) = {2}\n", i, j, cellContent);
                    double cellValue;
                    if (Double.TryParse(cellContent, out cellValue))
                    {
                        sum += cellValue;
                    }
                }

                // if the current column holds no value, then have it hidden
                if (sum == 0)
                {
                    ScheduleField field = schdDef.GetField(visibleFields[i]);
                    field.IsHidden = true;
                }
            }
            System.Diagnostics.Trace.Write(strBld.ToString());
        }
        #endregion
    }
}
