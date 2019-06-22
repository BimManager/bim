using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace TektaRevitPlugins.Multischedule
{
    class StructureSchedule : IMultitableSchedule
    {
        #region Data Fields
        #endregion

        #region Properties
        public IList<string> ViewNameEndings { get; private set; }
        public IDictionary<string, string> FiltersValues { get; private set; }
        public IList<Subschedule> Subschedules { get; private set; }
        public IList<Subschedule> Templates { get; set; }
        #endregion

        #region Constructors
        internal StructureSchedule(IDictionary<string, string> parametersValues)
        {
            ViewNameEndings = new List<string>
            {
                "Заголовок конструкций",
                "Спецификация элементов армирования, фоновая",
                "Спецификация элементов армирования",
                "Сборочные единицы",
                "Ведомость расхода бетона"
            };

            FiltersValues = parametersValues;
            Subschedules = new List<Subschedule>();
            Templates = new List<Subschedule>();
        }
        #endregion

        #region Methods
        #endregion

        #region Helper Methods
        
        #endregion
    }


}
