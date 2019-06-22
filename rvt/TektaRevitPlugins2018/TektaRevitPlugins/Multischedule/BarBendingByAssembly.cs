using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace TektaRevitPlugins.Multischedule
{
    class BarBendingByAssembly : IMultitableSchedule
    {
        #region Properties
        public IDictionary<string, string> FiltersValues { get; private set; }
        public IList<Subschedule> Subschedules { get; private set; }
        public IList<string> ViewNameEndings { get; private set; }
        public IList<Subschedule> Templates { get; private set; }
        #endregion

        #region Constructors
        public BarBendingByAssembly(IDictionary<string,string> fltrsVals)
        {
            FiltersValues = fltrsVals;
            ViewNameEndings = new List<string>()
            {
                "Ведомость деталей изделия"
            };
            Subschedules = new List<Subschedule>();
            Templates = new List<Subschedule>();
        }
        #endregion

        
    }
}
