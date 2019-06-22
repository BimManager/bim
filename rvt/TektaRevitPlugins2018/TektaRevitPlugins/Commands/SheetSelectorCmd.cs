using System.Collections.Generic;

using System.Linq;
using Trace = System.Diagnostics.Trace;
using Converter = Autodesk.Revit.DB.UnitUtils;
using Collector = Autodesk.Revit.DB.FilteredElementCollector;
using FamilyInstance = Autodesk.Revit.DB.FamilyInstance;
using ViewSheet = Autodesk.Revit.DB.ViewSheet;
using Document = Autodesk.Revit.DB.Document;
using ViewSheetSet = Autodesk.Revit.DB.ViewSheetSet;
using RvtElement = Autodesk.Revit.DB.Element;

using UnitUtils = Autodesk.Revit.DB.UnitUtils;



namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class SheetSelectorCmd : Autodesk.Revit.UI.IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData,
            ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            Trace.Listeners
                .Add(new System.Diagnostics.EventLogTraceListener("Application"));

            Autodesk.Revit.UI.UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try {
                // get hold of the print manager
                Autodesk.Revit.DB.PrintManager printManager =
                    doc.PrintManager;

                // select the printer
                printManager.SelectNewPrintDriver("Adobe PDF");
                printManager.PrintRange = Autodesk.Revit.DB.PrintRange.Select;
                printManager.CombinedFile = false;
                if (printManager.IsVirtual != Autodesk.Revit.DB.VirtualPrinterType.AdobePDF)
                    return Autodesk.Revit.UI.Result.Failed;

                // access the in-session print settings
                Autodesk.Revit.DB.IPrintSetting printSetting =
                    printManager.PrintSetup.InSession;

                printSetting.PrintParameters.PaperPlacement =
                    Autodesk.Revit.DB.PaperPlacementType.Margins;
                printSetting.PrintParameters.MarginType =
                    Autodesk.Revit.DB.MarginType.NoMargin;
                printSetting.PrintParameters.ZoomType = Autodesk.Revit.DB.ZoomType.Zoom;
                printSetting.PrintParameters.Zoom = 100;
                printSetting.PrintParameters.ColorDepth = Autodesk.Revit.DB.ColorDepthType.BlackLine;
                printSetting.PrintParameters.RasterQuality = Autodesk.Revit.DB.RasterQualityType.High;
                //printManager.Apply();

                // Promt the user to select the sets to print out
                PrintWnd hwnd = new PrintWnd(doc);
                hwnd.ShowDialog();

                IList<RvtElement> sets = hwnd.SelectedSets;

                for(int i=0; i < sets.Count; ++i) {
                    ViewSheetSet set = sets[i] as ViewSheetSet;

                    foreach (Autodesk.Revit.DB.View view in set.Views) {
                        if (view is ViewSheet)
                            SetUpSizeAndPrint((ViewSheet)view, printManager, printSetting);
                    }
                }
                return Autodesk.Revit.UI.Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Autodesk.Revit.UI.Result.Cancelled;
            }
            catch (System.Exception ex) {
                Trace.Write(string.Format("Command Exception:\n{0}\n{1}",
                    ex.Message, ex.StackTrace));
                return Autodesk.Revit.UI.Result.Failed;
            }
        }

        private static string GetSheetSize(double x, double y, double d)
        {
            System.Func<double, double, double> getMax =
                (a, b) => { if (a > b) return a; else return b; };

            double max = getMax(x, y);
            string size;

            if (max <= (297 + d) &&
                max >= (297 - d))
                size = "A4";
            else if (max <= (420 + d) &&
                max >= (420 - d))
                size = "A3";
            else if (max <= (594 + d) &&
                max >= (594 - d))
                size = "A2";
            else if (max <= (841 + d) &&
                max >= (841 - d))
                size = "A1";
            else if (max <= (1189 + d) &&
                max >= (1189 - d))
                size = "A0";
            else
                size = "A0";

            return size;
        }

        void SetUpSizeAndPrint(
            Autodesk.Revit.DB.ViewSheet vs,
            Autodesk.Revit.DB.PrintManager printManager,
            Autodesk.Revit.DB.IPrintSetting printSetting
            )
        {
            Autodesk.Revit.DB.BoundingBoxUV bbUV = vs.Outline;
            double x = UnitUtils.ConvertFromInternalUnits(
                bbUV.Max.U - bbUV.Min.U, Autodesk.Revit.DB.DisplayUnitType.DUT_MILLIMETERS);
            double y = UnitUtils.ConvertFromInternalUnits(
                bbUV.Max.V - bbUV.Min.V, Autodesk.Revit.DB.DisplayUnitType.DUT_MILLIMETERS);
            if (x == 0 || y == 0)
                return;

            string sheetSize = GetSheetSize(x, y, 100);

            if (x > y) {
                printSetting.PrintParameters.PageOrientation =
                    Autodesk.Revit.DB.PageOrientationType.Landscape;
            }
            else
                printSetting.PrintParameters.PageOrientation
                    = Autodesk.Revit.DB.PageOrientationType.Portrait;

            foreach (Autodesk.Revit.DB.PaperSize ps in printManager.PaperSizes) {
                if (ps.Name == sheetSize) {
                    printSetting.PrintParameters.PaperSize = ps;
                    break;
                }
            }
            Trace.Write("Selected paper sized: " + printSetting.PrintParameters.PaperSize.Name);

            using (Autodesk.Revit.DB.Transaction t = new Autodesk.Revit.DB.Transaction(vs.Document)) {
                t.Start("temp");
                printManager.PrintSetup.CurrentPrintSetting = printSetting;
                //printManager.Apply();
                printManager.PrintSetup.SaveAs("temp");
                printManager.SubmitPrint(vs);
                //vs.Print(true);
                t.RollBack();
            }
        }

        public static IList<Autodesk.Revit.DB.ViewSheetSet> GetViewSheetSets(Document doc)
        {
            return new Collector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.ViewSheetSet))
                .Cast< Autodesk.Revit.DB.ViewSheetSet>()
                .ToList();
        }

        void SetPDFSettings(string destFileName, string dirName) {
            Microsoft.Win32.RegistryKey  pjcKey =
                Microsoft.Win32.Registry.CurrentUser
                .OpenSubKey(@"Software\Adobe\Acrobate Distill\PrinterJobControl", true);

            string appPath = @"C:\Program Files\Autodesk\Revit 2018\Revit.exe";
            pjcKey?.SetValue(appPath, destFileName);
            pjcKey?.SetValue("LastPfdPortFolder - Revit.exe", dirName);
        }
    }

    unsafe class NativeDlls
    {
        [System.Runtime.InteropServices.DllImport("User32.dll",
            CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        public static extern System.IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("User32.dll",
            CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        public static extern long SendMessage(System.IntPtr hWnd, uint msg, ulong wParam, long lParam);
    }

}
