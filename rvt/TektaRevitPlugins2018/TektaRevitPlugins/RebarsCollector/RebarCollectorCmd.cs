using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Tracer = System.Diagnostics.Trace;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class RebarCollectorCmd : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Tracer.Listeners.Add(new System.Diagnostics.EventLogTraceListener("Application"));

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            RebarsCollectorWnd wnd = null;
            System.Text.StringBuilder strBld = 
                new System.Text.StringBuilder();

            try
            {
                List<ViewType> allowedViews =
                    new List<ViewType>
                    {
                        ViewType.EngineeringPlan,
                        ViewType.FloorPlan,
                        ViewType.Section,
                        ViewType.Elevation,
                        ViewType.Detail,
                        ViewType.DraftingView
                    };

                List<ElementId> rebarIds = new List<ElementId>();

                // if the active view is a sheet, bag all the view belonging to it
                if (doc.ActiveView.ViewType == ViewType.DrawingSheet)
                {
                    ViewSheet vs = doc.ActiveView as ViewSheet;

                    foreach (ElementId viewId in vs.GetAllPlacedViews())
                    {
                        View view = doc.GetElement(viewId) as View;

                        if (allowedViews.Contains(view.ViewType))
                        {
                            rebarIds.AddRange(
                                RebarsUtils
                                .GetAllRebarIdsInView(view));
                        }
                    }
                }
                else if (allowedViews.Contains(doc.ActiveView.ViewType))
                {
                    rebarIds.AddRange(
                        RebarsUtils
                        .GetAllRebarIdsInView(doc.ActiveView));
                }

                if (rebarIds.Count == 0)
                {
                    TaskDialog.Show("Warning", "No rebars have been collected.");
                    return Result.Cancelled;
                }

                // Get partitions and host marks from the data storage
                IDictionary<string, ISet<string>> partsHostMarks =
                    ExtensibleStorageUtils.GetValues(
                        ExtensibleStorageUtils.GetSchema(UpdateRepository.SCHEMA_GUID),
                        ExtensibleStorageUtils.GetDataStorage(doc, UpdateRepository.DATA_STORAGE_NAME),
                        UpdateRepository.FN_PARTS_HOST_MARKS);

                // Dig out assemblies grouped by partition + host mark names
                IDictionary<string, ISet<string>> partsMarksAssemblies =
                    ExtensibleStorageUtils.GetValues(
                    ExtensibleStorageUtils.GetSchema(UpdateRepository.SCHEMA_GUID),
                    ExtensibleStorageUtils.GetDataStorage(doc, UpdateRepository.DATA_STORAGE_NAME),
                    UpdateRepository.FN_HOST_MARKS_ASSEMBLIES);

                wnd = new RebarsCollectorWnd(partsHostMarks, partsMarksAssemblies);

                wnd.ButtonClicked += (sender, e) =>
                {
                    using (Transaction t = new Transaction(doc, "Set rebars' properties"))
                    {
                        t.Start();
                        foreach (ElementId rebarId in rebarIds)
                        {
                            strBld.Clear();
                            strBld.AppendFormat("Id: {0}", 
                                rebarId.ToString());

                            // Get hold of the element represented by its id
                            Element rebar = doc.GetElement(rebarId);

                            // See about the partitions
                            Parameter partition = 
                            rebar.LookupParameter(RebarsUtils.PARTITION);
                            if (partition != null && 
                                !partition.IsReadOnly && 
                                e.Partition != null)
                            {
                                partition.Set(e.Partition);
                            }

                            // Set the value in the host mark parameter
                            Parameter hostMark = 
                            rebar.LookupParameter(RebarsUtils.HOST_MARK);
                            if (hostMark != null && 
                                !hostMark.IsReadOnly &&
                                e.HostMark != null)
                            {
                                hostMark.Set(e.HostMark);
                            }

                            // if checked, set the value in the assembly mark parameter
                            Parameter assemblyMark = 
                            rebar.LookupParameter(RebarsUtils.ASSEMBLY_MARK);
                            if (assemblyMark != null && 
                                !assemblyMark.IsReadOnly &&
                                e.AssemblyMark != null)
                            {
                                assemblyMark.Set(e.AssemblyMark);
                            }

                            // if checked and calculable, then set the value
                            Parameter isCalculable = 
                            rebar.LookupParameter(RebarsUtils.IS_CALCULABLE);
                            if (e.IsCalculable != null &&
                                isCalculable != null && 
                                !isCalculable.IsReadOnly)
                            {
                                isCalculable
                                .Set((e.IsCalculable == true ? 1 : 0));
                            }

                            // if checked and specifiable, then set the value
                            Parameter isSpecifiable = 
                            rebar.LookupParameter(RebarsUtils.IS_SPECIFIABLE);
                            if (e.IsSpecifiable != null &&
                                isSpecifiable != null &&
                                !isSpecifiable.IsReadOnly)
                            {
                                isSpecifiable
                                .Set((e.IsSpecifiable == true ? 1 : 0));
                            }
                            // if checked and in an assembly, then set the value
                            Parameter isAssembly = 
                            rebar.LookupParameter(RebarsUtils.IS_IN_ASSEMBLY);
                            if (e.IsAssembly != null && 
                                isAssembly != null &&
                                !isAssembly.IsReadOnly)
                            {
                                isAssembly
                                .Set((bool)e.IsAssembly ? 1 : 0);
                            }
                        }
                        t.Commit();
                    }
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
                if (wnd != null)
                    wnd.Close();

                TaskDialog.Show("Exception",
                    string.Format("{0}\n{1}\n{2}",
                    strBld.ToString(), ex.Message, ex.StackTrace));

                Tracer.Write(string.Format("{0}\n{1}",
                    ex.Message, ex.StackTrace));

                return Result.Failed;
            }
        }
    }

}
