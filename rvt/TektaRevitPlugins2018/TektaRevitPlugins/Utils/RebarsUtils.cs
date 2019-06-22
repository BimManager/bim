using System.Collections.Generic;
using System.Linq;

using Trace = System.Diagnostics.Trace;

using Autodesk.Revit.DB;

namespace TektaRevitPlugins
{
    internal static class RebarsUtils
    {
        internal const string PARTITION = "REI.TXT.Раздел";
        internal const string HOST_MARK = "REI.TXT.МаркаКонструкции";
        internal const string IS_SPECIFIABLE = "REI.YSN.Спецификация";
        internal const string LENGTH = "REI.LNG.Длина";
        internal const string DIAMETER = "REI.LNG.Диаметр";
        internal const string MARK = "REI.TXT.МаркаЭлемента";
        internal const string WEIGHT_PER_METER = "REI.YSN.ПогонныеМетры";
        internal const string ASSEMBLY_MARK = "REI.TXT.МаркаСборки";
        internal const string IS_IN_ASSEMBLY = "REI.YSN.Сборка";
        internal const string IS_CALCULABLE = "REI.YSN.РасчетКоличества";
        internal const string BLOCK_NUMBER = "#Блок";

        internal const string STRAIGHT = "Прямая";
        internal const string EXTRA = "Дополнительная";
        internal const string BACKGROUND = "Фоновая";
        internal const string POINT = "Точка";

        internal const string PARTITION_GUID = "81ef322e-63ee-49b0-a91a-47f10f6426e5";
        internal const string HOST_MARK_GUID = "b9ebcc55b-f539-4a9a-b869-d7892fee38e1";
        internal const string ASSEBMLY_GUID = "e947c65c-a08f-485f-8d2e-b903093808e1";

        internal const int PARAM_ID_PARTITION = 2698521;
        internal const int PARAM_ID_HOST_MARK = 2964603;
        internal const int PARAM_ID_ASSEMBLY = 2964679;

        internal static IList<FamilyInstance> GetAllRebarsInDoc(Document doc)
        {
            System.Func<Document, IList<FamilyInstance>> GetAllRebars = d =>
            {
                ElementParameterFilter shpFamNameFilter =
                new ElementParameterFilter(ParameterFilterRuleFactory.CreateBeginsWithRule(
                new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM), "R-SHP", false));

                ElementParameterFilter empFamNameFilter =
                new ElementParameterFilter(
                    ParameterFilterRuleFactory.CreateBeginsWithRule(
                    new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM),
                    "R-EMP", false));

                List<ElementFilter> filters =
                new List<ElementFilter>()
                {
                    shpFamNameFilter,
                    empFamNameFilter
                };

                LogicalOrFilter shpEmpFilter = new LogicalOrFilter(filters);

                return new FilteredElementCollector(d)
                        .OfCategory(BuiltInCategory.OST_DetailComponents)
                        .WhereElementIsNotElementType()
                        .WherePasses(shpEmpFilter)
                        .Cast<FamilyInstance>()
                        .Where(fi => fi.LookupParameter(IS_SPECIFIABLE).AsInteger() == 1)
                        .ToList();
            };

            return GetAllRebars(doc);
        }

        internal static IList<ElementId> GetAllRebarIdsInView(View view)
        {
            ElementParameterFilter shpFamNameFilter =
                new ElementParameterFilter(
                    ParameterFilterRuleFactory.CreateBeginsWithRule(
                    new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM),
                    "R-SHP", false));

            ElementParameterFilter sumFamNameFilter =
                new ElementParameterFilter(
                    ParameterFilterRuleFactory.CreateBeginsWithRule(
                    new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM),
                    "R-SUM", false));

            ElementParameterFilter empFamNameFilter =
                new ElementParameterFilter(
                    ParameterFilterRuleFactory.CreateBeginsWithRule(
                    new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM),
                    "R-EMP", false));

            List<ElementFilter> filters =
                new List<ElementFilter>()
                {
                    shpFamNameFilter,
                    sumFamNameFilter,
                    empFamNameFilter
                };

            LogicalOrFilter shpSumEmpFilter =
                new LogicalOrFilter(filters);

            return new FilteredElementCollector(view.Document, view.Id)
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .WhereElementIsNotElementType()
                .WherePasses(shpSumEmpFilter)
                .ToElementIds()
                .ToList();
        }


        internal static IList<Element> GetAllRebars
            (Document doc, string partition = null,
            string hostMark = null, IList<string> assemblies = null)
        {
            // Dig out all families beginning with R-SHP and R-EMP
            ElementParameterFilter shpFamNameFltr =
                new ElementParameterFilter(
                    ParameterFilterRuleFactory
                    .CreateBeginsWithRule(
                new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM),
                "R-SHP", false));

            ElementParameterFilter empFamNameFltr =
            new ElementParameterFilter(
                ParameterFilterRuleFactory
                .CreateBeginsWithRule(
                new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM),
                "R-EMP", false));

            List<ElementFilter> nameFilters =
                new List<ElementFilter>()
                {
                    shpFamNameFltr,
                    empFamNameFltr
                };

            LogicalOrFilter orNames =
                new LogicalOrFilter(nameFilters);

            // Select all rebars before sorting through them
            // provided that ancillary parameters are supplied
            FilteredElementCollector collector =
                new FilteredElementCollector(doc);

            IList<Element> allRebars = collector
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .WhereElementIsNotElementType()
                .WherePasses(orNames)
                .Where(e => e.LookupParameter(IS_SPECIFIABLE).AsInteger() == 1)
                .ToList();

            Trace.Write("All rebars dug up = " + allRebars.Count);

            if (partition == null &&
                hostMark == null &&
                assemblies == null)
            {
                return allRebars;
            }
            else
            {
                if (allRebars.Count == 0)
                {
                    return allRebars;
                }
                else
                {
                    IList<Element> partFilteredRebars = new List<Element>();
                    // Filter out the rebars by partition
                    if (partition != null)
                    {
                        IEnumerator<Element> itr = allRebars.GetEnumerator();
                        while (itr.MoveNext())
                        {
                            if (itr.Current.LookupParameter(PARTITION).AsString().Equals(partition))
                            {
                                partFilteredRebars.Add(itr.Current);
                            }
                        }

                        Trace.Write("By partition: " + partFilteredRebars.Count);

                        if (hostMark == null && assemblies == null)
                        {
                            return partFilteredRebars;
                        }
                        else if (hostMark == null && assemblies != null)
                        {
                            SelectByAssemblyMarks(partFilteredRebars, assemblies);
                            return partFilteredRebars;
                        }
                        else
                        {
                            // Filter out the rebars by host mark
                            IList<Element> partHostFilteredRebars = new List<Element>();

                            itr = partFilteredRebars.GetEnumerator();

                            while (itr.MoveNext())
                            {
                                if (itr.Current.LookupParameter(HOST_MARK).AsString().Equals(hostMark))
                                {
                                    partHostFilteredRebars.Add(itr.Current);
                                }
                            }

                            Trace.Write("By partition and host mark: " +
                                partHostFilteredRebars.Count);

                            if (assemblies == null)
                            {
                                return partHostFilteredRebars;
                            }
                            else
                            {
                                // Filter out the rebars by assembly
                                SelectByAssemblyMarks(partHostFilteredRebars, assemblies);

                                return partHostFilteredRebars;
                            }
                        }
                    }
                    return partFilteredRebars.ToList();
                }
            }
        }

        internal static IDictionary<string, ISet<string>> GetPartitionHostMarkDic(Document doc)
        {
            IDictionary<string, ISet<string>> grpByPartition =
                    new SortedDictionary<string, ISet<string>>();

            foreach (FamilyInstance fi in GetAllRebarsInDoc(doc))
            {
                string key = fi.LookupParameter(PARTITION).AsString();
                if (key.Length == 0)
                    continue;
                string val = fi.LookupParameter(HOST_MARK).AsString();
                if (val.Length == 0)
                    continue;

                if (grpByPartition.Keys.Contains(key))
                {
                    grpByPartition[key].Add(val);
                }
                else
                {
                    grpByPartition.Add(key, new SortedSet<string> { val });
                }
            }
            return grpByPartition;
        }

        #region Helper Methods
        static void SelectByAssemblyMarks(IList<Element> rebars, IList<string> asmMarks)
        {
            // Filter out the rebars by assembly
            IList<Element> selectedAssemblies = new List<Element>();

            IEnumerator<Element> itr = rebars.GetEnumerator();

            while (itr.MoveNext())
            {
                Element curElem = itr.Current;
                string curAsmMark = curElem
                    .LookupParameter(ASSEMBLY_MARK).AsString();

                if (curAsmMark != null)
                {
                    if (asmMarks.Contains(curAsmMark))
                    {
                        selectedAssemblies.Add(curElem);
                    }
                }
            }
            Trace.Write("By partition and host mark and assemblies: " +
                selectedAssemblies.Count);

            rebars = selectedAssemblies;
        }
        #endregion
    }
}
