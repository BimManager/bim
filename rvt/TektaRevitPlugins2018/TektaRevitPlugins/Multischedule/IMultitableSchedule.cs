using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TektaRevitPlugins.Multischedule
{
    internal interface IMultitableSchedule
    {
        #region Properties
        IDictionary<string, string> FiltersValues { get; }
        IList<Subschedule> Subschedules { get; }
        IList<string> ViewNameEndings { get; }
        IList<Subschedule> Templates { get; }
        #endregion

        #region Methods
        #endregion
    }
}
