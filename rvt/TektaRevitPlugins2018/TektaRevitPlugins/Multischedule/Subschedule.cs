using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ElementId = Autodesk.Revit.DB.ElementId;

namespace TektaRevitPlugins.Multischedule
{
    class Subschedule
    {
        public ElementId Id { get; set; }
        public string ViewName { get; set; }
        public bool IsTemplate { get; set; }
        public bool ShowTitle { get; set; }
    }
}
