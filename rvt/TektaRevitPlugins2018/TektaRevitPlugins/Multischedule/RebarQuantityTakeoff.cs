using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Document = Autodesk.Revit.DB.Document;
using ElementId = Autodesk.Revit.DB.ElementId;
using ViewSchedule = Autodesk.Revit.DB.ViewSchedule;
using TableData = Autodesk.Revit.DB.TableData;
using ScheduleDefinition = Autodesk.Revit.DB.ScheduleDefinition;
using TableSectionData = Autodesk.Revit.DB.TableSectionData;
using ScheduleField = Autodesk.Revit.DB.ScheduleField;

namespace TektaRevitPlugins.Multischedule
{
    class RebarQuantityTakeoff : IMultitableSchedule
    {
        #region Properties
        public IDictionary<string, string> FiltersValues { get; private set; }
        public IList<Subschedule> Subschedules { get; private set; }
        public IList<string> ViewNameEndings { get; private set; }
        public IList<Subschedule> Templates { get; private set; }
        #endregion

        #region Constructors
        public RebarQuantityTakeoff(IDictionary<string, string> fltrsVals)
        {
            FiltersValues = fltrsVals;
            ViewNameEndings = new List<string>()
            {
                "Общая ведомость расхода стали, кг"
            };
            Subschedules = new List<Subschedule>();
            Templates = new List<Subschedule>();
        }
        #endregion

        #region Methods
        internal void HideZeroFieldsMaterialTakeOff(Document doc)
        {
            if (Subschedules.Count == 0 ||
                Subschedules[0].Id == ElementId.InvalidElementId)
            {
                return;
            }

            ViewSchedule viewSchedule = doc.GetElement(Subschedules[0].Id) as ViewSchedule;
            ScheduleDefinition scheduleDef = viewSchedule.Definition;
            TableData tableData = viewSchedule.GetTableData();
            TableSectionData tableSecData = tableData
                .GetSectionData(Autodesk.Revit.DB.SectionType.Body);
            List<ScheduleField> visibleFields = scheduleDef
                .GetFieldOrder()
                .Select(id => scheduleDef.GetField(id))
                .Where(f => !f.IsHidden)
                .ToList();

            for (int i = tableSecData.FirstColumnNumber; i <= tableSecData.LastColumnNumber; ++i)
            {
                if (tableSecData.GetCellText(tableSecData.FirstRowNumber + 5, i) == "0")
                {
                    double total = 0;

                    for (int j = tableSecData.FirstRowNumber + 5; j <= tableSecData.LastRowNumber; ++j)
                    {
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

        }
        #endregion
    }
}
