using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Schema = Autodesk.Revit.DB.ExtensibleStorage.Schema;

using System.Linq;
using System.Collections.Generic;
using Tracer = System.Diagnostics.Trace;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class RebarMarkerCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            Tracer.Listeners.Add(new
                System.Diagnostics.EventLogTraceListener("Application"));

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            RebarsMarkerWnd wnd = null;
            try
            {
                // Get all the necessary particulars about partitions and
                // host marks from the repository
                Schema schema = ExtensibleStorageUtils
                    .GetSchema(UpdateRepository.SCHEMA_GUID);
                if (schema == null)
                {
                    TaskDialog.Show("Warning",
                        "No repository exists. " +
                        "Please generate one before proceeding with this command.");
                    return Result.Cancelled;
                }

                // Dig up the data storage element
                Autodesk.Revit.DB.ExtensibleStorage.DataStorage ds =
                    ExtensibleStorageUtils.GetDataStorage(doc, UpdateRepository.DATA_STORAGE_NAME);
                if (ds == null)
                {
                    TaskDialog.Show("Warning", "No data storage element. " +
                        "Please update the repository.");
                    return Result.Cancelled;
                }

                // Retrieve host marks grouped by partition
                IDictionary<string, ISet<string>> partsHostMarks =
                    ExtensibleStorageUtils.GetValues(
                        schema,
                        ds,
                        UpdateRepository.FN_PARTS_HOST_MARKS);

                if (partsHostMarks.Keys.Count == 0)
                {
                    TaskDialog.Show("Warning",
                        "No rebars to mark have been bagged.");
                    return Result.Cancelled;
                }

                // Get hold of all assemblies grouped by host marks
                IDictionary<string, ISet<string>> hostMarksAssemblies =
                    ExtensibleStorageUtils.GetValues(
                        schema,
                        ds,
                        UpdateRepository.FN_HOST_MARKS_ASSEMBLIES);

                wnd = new RebarsMarkerWnd(partsHostMarks, hostMarksAssemblies);


                wnd.OkClicked += (sender, args) =>
                {
                    IList<FamilyInstance> filteredRebars = null;

                    if (args.Assemblies.Count == 0)
                    {
                        filteredRebars =
                        //GetRebarsNotInAssemblyByPartitionHostMark(
                        //    doc,
                        //    args.Partition,
                        //    args.HostMark);
                            RebarsUtils.GetAllRebars(
                                doc,
                                args.Partition,
                                args.HostMark)
                                .Cast<FamilyInstance>()
                                .ToList();
                    }
                    else
                    {
                        filteredRebars = 
                            //GetRebarsByPartitionHostMarkAssembly(
                            //    doc,
                            //    args.Partition,
                            //    args.HostMark,
                            //    args.Assemblies.ElementAt(i));
                            RebarsUtils.GetAllRebars(
                                doc,
                                args.Partition,
                                args.HostMark,
                                args.Assemblies.ToList())
                                .Cast<FamilyInstance>()
                                .ToList();
                    }

                    Tracer.Write("The number of rebars looked up amounts to " + filteredRebars.Count);

                    // Group the rebars into three categories and mark them
                    GroupRebars(filteredRebars);
                };
                wnd.ShowDialog();

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions
            .OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (System.Exception ex)
            {
                string errorMsg = string.Format("{0}\n{1}", ex.Message, ex.StackTrace);
                Tracer.Write(errorMsg);
                TaskDialog.Show("Exception", errorMsg);
                return Result.Failed;
            }
            finally
            {
                if (wnd != null)
                {
                    wnd.Close();
                }
            }
        }

        #region Helper Methods
        void GroupRebars(IList<FamilyInstance> filteredRebars)
        {
            // Group the rebars into three categories and mark them
            var rebarType1 = from fi in filteredRebars
                             where fi.Symbol.FamilyName.Contains(RebarsUtils.STRAIGHT) ||
                                   fi.Symbol.FamilyName.Contains(RebarsUtils.EXTRA) ||
                                   fi.Symbol.FamilyName.Contains(RebarsUtils.POINT)
                             select fi;

            Tracer.Write("RT1 = " + rebarType1.Count());

            // Mark the first category
            if (rebarType1.Count() != 0)
            {
                MarkRebarType1(rebarType1.ToList());
            }

            var rebarType2 = from fi in filteredRebars
                             where fi.Symbol.FamilyName.Contains(RebarsUtils.BACKGROUND)
                             select fi;

            Tracer.Write("RT2 = " + rebarType2.Count());

            // Mark the second category
            if (rebarType2.Count() != 0)
            {
                MarkRebarType2(rebarType2.ToList());
            }

            var rebarType3 = from fi in filteredRebars
                             where
                             !fi.Symbol.FamilyName.StartsWith("R-EMP") &&
                             !fi.Symbol.FamilyName.Contains(RebarsUtils.STRAIGHT) &&
                             !fi.Symbol.FamilyName.Contains(RebarsUtils.POINT) &&
                             !fi.Symbol.FamilyName.Contains(RebarsUtils.EXTRA) &&
                             !fi.Symbol.FamilyName.Contains(RebarsUtils.BACKGROUND)
                             select fi;

            Tracer.Write("RT3 = " + rebarType3.Count());

            // Mark the third category
            if (rebarType3.Count() != 0)
            {
                GroupAndMarkRebarT3(rebarType3.ToList());
            }

            var rebarType4 = from fi in filteredRebars
                             where
                             fi.Symbol.FamilyName.StartsWith("R-EMP")
                             select fi;

            Tracer.Write("RT4 = " + rebarType4.Count());

            // Mark the forth type
            if (rebarType4.Count() != 0)
            {
                GroupMarkRebarType4(rebarType4.ToList());
            }
        }
        #endregion

        IList<FamilyInstance>
            GetRebarsNotInAssemblyByPartitionHostMark(
            Document doc, string partition, string hostMark)
        {
            return RebarsUtils.GetAllRebarsInDoc(doc)
                .Where(fi => (
                (fi.LookupParameter(RebarsUtils.PARTITION).AsString() == partition) &&
                (fi.LookupParameter(RebarsUtils.HOST_MARK).AsString() == hostMark) &&
                (fi.LookupParameter(RebarsUtils.IS_IN_ASSEMBLY).AsInteger() == 0)))
                .ToList();
        }

        IList<FamilyInstance>
            GetRebarsByPartitionHostMarkAssembly(
            Document doc, string partition, string hostMark, string assembly)
        {
            return RebarsUtils.GetAllRebarsInDoc(doc)
                .Where(fi => (
                (fi.LookupParameter(RebarsUtils.PARTITION).AsString() == partition) &&
                (fi.LookupParameter(RebarsUtils.HOST_MARK).AsString() == hostMark) &&
                (fi.LookupParameter(RebarsUtils.ASSEMBLY_MARK).AsString() == assembly)))
                .ToList();
        }

        void GroupAndMarkRebarT3(IList<FamilyInstance> hostMarkGroup)
        {
            IDictionary<string, IList<FamilyInstance>> collectedRebars =
                new Dictionary<string, IList<FamilyInstance>>();

            // Filter all the rebars collected into four categories
            // according to their type
            foreach (FamilyInstance fi in hostMarkGroup)
            {
                string rebarType = GetRebarType(fi);
                if (!collectedRebars.ContainsKey(rebarType))
                {
                    collectedRebars
                        .Add(rebarType, new List<FamilyInstance> { fi });
                }
                else
                {
                    collectedRebars[rebarType].Add(fi);
                }
            }

            // Begin grouping the rebars by diameter, then by family type,
            // and by length
            foreach (string key in collectedRebars.Keys)
            {
                var groupedByDiameter = from fi in collectedRebars[key]
                                        group fi by fi.Symbol
                                        .LookupParameter(RebarsUtils.DIAMETER).AsDouble()
                                        into temp
                                        orderby temp.Key
                                        select temp;

                foreach (var diaGrp in groupedByDiameter)
                {
                    var groupedByFamilies = from fi in diaGrp
                                            group fi by fi.Symbol.FamilyName
                                            into temp
                                            orderby temp.Key
                                            select temp;
                    int i = 1;
                    foreach (var famGrp in groupedByFamilies)
                    {
                        var groupedByLength = from fi in famGrp
                                              group fi by System.Math.Round
                                              (fi.LookupParameter(RebarsUtils.LENGTH).AsDouble(), 2)
                                              into temp
                                              orderby temp.Key
                                              select temp;

                        foreach (var lenGrp in groupedByLength)
                        {
                            MarkRebarType3(lenGrp.ToList(), i);
                            ++i;
                        }

                    }
                }

            }
        }

        string GetRebarType(FamilyInstance rebar)
        {
            string fName = rebar.Symbol.FamilyName;
            if (fName.Contains("Хомут"))
                return "Х";
            else if (fName.Contains("П"))
                return "П";
            else if (fName.Contains("Шпилька"))
                return "Ш";
            else
                return "Г";
        }

        void MarkRebarType1(IList<FamilyInstance> rebars)
        {
            Document doc = rebars[0].Document;
            using (Transaction t = new Transaction(doc, "Mark rebars T1"))
            {
                t.Start();
                foreach (FamilyInstance fi in rebars)
                {
                    string mark = string.Empty;
                    if (fi.LookupParameter(RebarsUtils.WEIGHT_PER_METER) == null ||
                        fi.LookupParameter(RebarsUtils.WEIGHT_PER_METER).AsInteger() == 0)
                    {
                        mark = string.Format("{0}-{1}", UnitUtils.ConvertFromInternalUnits
                           (fi.Symbol.LookupParameter(RebarsUtils.DIAMETER).AsDouble(),
                           DisplayUnitType.DUT_MILLIMETERS).ToString(),
                           System.Math.Round(UnitUtils.ConvertFromInternalUnits
                           (fi.LookupParameter(RebarsUtils.LENGTH).AsDouble(),
                           DisplayUnitType.DUT_CENTIMETERS)).ToString());

                    }
                    else
                    {
                        mark = string.Format("{0}*",
                        System.Math.Round(UnitUtils.ConvertFromInternalUnits
                        (fi.Symbol.LookupParameter(RebarsUtils.DIAMETER).AsDouble(),
                        DisplayUnitType.DUT_MILLIMETERS), 0).ToString());
                    }
                    fi.LookupParameter(RebarsUtils.MARK).Set(mark);
                }
                t.Commit();
            }
        }

        void MarkRebarType2(IList<FamilyInstance> rebars)
        {
            Document doc = rebars[0].Document;
            using (Transaction t = new Transaction(doc, "Mark rebars T2"))
            {
                t.Start();
                foreach (FamilyInstance fi in rebars)
                {
                    string mark = string.Format("{0}*",
                        System.Math.Round(UnitUtils.ConvertFromInternalUnits
                        (fi.Symbol.LookupParameter(RebarsUtils.DIAMETER).AsDouble(),
                        DisplayUnitType.DUT_MILLIMETERS), 0).ToString());
                    fi.LookupParameter(RebarsUtils.MARK).Set(mark);
                }
                t.Commit();
            }
        }

        void MarkRebarType3(IList<FamilyInstance> rebars, int i)
        {
            Document doc = rebars[0].Document;
            using (Transaction t = new Transaction(doc, "Mark rebars T3"))
            {
                t.Start();
                foreach (FamilyInstance fi in rebars)
                {
                    string mark = string.Format("{0}{1}-{2}", GetRebarType(fi),
                        UnitUtils.ConvertFromInternalUnits
                        (fi.Symbol.LookupParameter(RebarsUtils.DIAMETER).AsDouble(),
                        DisplayUnitType.DUT_MILLIMETERS).ToString(), i);
                    fi.LookupParameter(RebarsUtils.MARK).Set(mark);
                }
                t.Commit();
            }
        }

        void GroupMarkRebarType4(IList<FamilyInstance> rebars)
        {
            Document doc = rebars[0].Document;

            var groupedByType =
                rebars.GroupBy(fi => fi.get_Parameter(
                    BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString());

            using (Transaction t = new Transaction(doc, "Mark rebars T4"))
            {
                t.Start();
                foreach (IGrouping<string, FamilyInstance> grp in groupedByType)
                {
                    int i = 1;
                    foreach (FamilyInstance fi in grp)
                    {
                        string mark = string.Format("{0}", i);
                        fi.LookupParameter(RebarsUtils.MARK).Set(mark);
                        ++i;
                    }
                }
                t.Commit();
            }
        }
    }
}