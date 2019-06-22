using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TektaRevitPlugins.Multischedule
{
    class ScheduleOfWork : IMultitableSchedule
    {
        #region Properties
        public IDictionary<string, string> FiltersValues { get; private set; }
        public IList<Subschedule> Subschedules { get; private set; }
        public IList<string> ViewNameEndings { get; private set; }
        public IList<Subschedule> Templates { get; private set; }
        #endregion

        #region Constructors
        public ScheduleOfWork(IDictionary<string,string> fltrsVals)
        {
            FiltersValues = fltrsVals;
            ViewNameEndings = new List<string>()
            {
                "Сводная ведомость объемов работ"
            };
            Subschedules = new List<Subschedule>();
            Templates = new List<Subschedule>();
        }
        #endregion
    }
}
