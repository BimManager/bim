using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TektaRevitPlugins
{
    class VerticalElementsSelectionFilter: ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is Wall)
                return true;
            else if (elem.Category.Id.IntegerValue ==
                    (int)BuiltInCategory.OST_StructuralColumns)
                return true;
            else
                return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}
