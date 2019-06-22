using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace TektaRevitPlugins.Multischedule
{
    static class MultischeduleUtils
    {
        internal static SortedList<string, SortedSet<string>> 
            DigUpPartsStrTypes(Document doc)
        {
            IList<ElementFilter> categoriesFilters =
                new List<ElementFilter>()
                {
                    new ElementCategoryFilter(BuiltInCategory.OST_Floors),
                    new ElementCategoryFilter(BuiltInCategory.OST_Stairs),
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming),
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns),
                    new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation),
                    new ElementCategoryFilter(BuiltInCategory.OST_Walls)
                };
            LogicalOrFilter orFilter =
                new LogicalOrFilter(categoriesFilters);

            FilteredElementCollector collector =
                new FilteredElementCollector(doc);

            IList<Element> dugUpElems = collector
                .WherePasses(orFilter)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            SortedList<string, SortedSet<string>> partsStrTypes =
                new SortedList<string, SortedSet<string>>();

            IEnumerator<Element> it = dugUpElems.GetEnumerator();
            while (it.MoveNext())
            {
                Element e = it.Current;
                string part = e.LookupParameter(MultischeduleParameters.PARTITION).AsString();
                string strType = e.LookupParameter(MultischeduleParameters.STRUCTURE_TYPE).AsString();

                if (part == null || strType == null ||
                    part.Length == 0 || strType.Length == 0)
                {
                    continue;
                }

                SortedSet<string> strTypes;
                if (partsStrTypes.TryGetValue(part, out strTypes))
                {
                    strTypes.Add(strType);
                    partsStrTypes[part] = strTypes;
                }
                else
                {
                    strTypes = new SortedSet<string>();
                    strTypes.Add(strType);
                    partsStrTypes.Add(part, strTypes);
                }
            }
            return partsStrTypes;
        }
    }
}
