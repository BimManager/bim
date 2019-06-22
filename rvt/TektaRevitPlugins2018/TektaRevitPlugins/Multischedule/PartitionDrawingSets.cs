using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TektaRevitPlugins.Multischedule
{
    class PartitionDrawingSets : IMultitableSchedule
    {
        #region Properties
        public IDictionary<string, string> FiltersValues { get; private set; }
        public IList<Subschedule> Subschedules { get; private set; }
        public IList<string> ViewNameEndings { get; private set; }
        public IList<Subschedule> Templates { get; private set; }
        #endregion

        #region Constructors
        public PartitionDrawingSets(IDictionary<string, string> fltrsVals)
        {
            FiltersValues = fltrsVals;
            ViewNameEndings = new List<string>()
            { 
                "Ведомость рабочих чертежей"
            };
            Subschedules = new List<Subschedule>();
            Templates = new List<Subschedule>();
        }

        #endregion
    }
}
