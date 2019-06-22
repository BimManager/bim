    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TektaRevitPlugins
{
    class FlatHandlerAvailability : Autodesk.Revit.UI.IExternalCommandAvailability
    {
        const string DOC_TITLE = "MHBK-RELIDE";

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories) {
            Document doc = applicationData.ActiveUIDocument.Document;
            if (doc.Title.StartsWith(DOC_TITLE))
                return true;
            else
                return false;
        }
    }
}
