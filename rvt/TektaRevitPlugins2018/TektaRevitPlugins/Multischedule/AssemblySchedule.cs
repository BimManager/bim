using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TektaRevitPlugins.Multischedule
{
    class AssemblySchedule : IMultitableSchedule
    {
        #region Data Fields

        #endregion

        #region Properties
        public IList<string> ViewNameEndings { get; private set; }
        public IDictionary<string, string> FiltersValues { get; private set; }
        public IList<Subschedule> Subschedules { get; private set; }
        public IList<Subschedule> Templates { get; private set; }
        #endregion

        #region Constructors
        public AssemblySchedule(IDictionary<string,string> fltrsVals)
        {
            ViewNameEndings = new List<string>
            {
                "Заголовок изделия",
                "Спецификация изделия, фоновая",
                "Спецификация изделия"
            };
            
            if (fltrsVals["ConcreteQuantity"].Equals("1"))
            {
                ViewNameEndings.Add("Ведомость расхода бетона");
            }
            
            FiltersValues = fltrsVals;
            Subschedules = new List<Subschedule>();
            Templates = new List<Subschedule>();
        }
        #endregion

    }
}
