using System.Collections.Generic;

using System.Linq;
using Trace = System.Diagnostics.Trace;
using Converter = Autodesk.Revit.DB.UnitUtils;
using Collector = Autodesk.Revit.DB.FilteredElementCollector;
using FamilyInstance = Autodesk.Revit.DB.FamilyInstance;
using ViewSheet = Autodesk.Revit.DB.ViewSheet;
using Document = Autodesk.Revit.DB.Document;




namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class AutoSizeSelectorCmd : Autodesk.Revit.UI.IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData,
            ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            Trace.Listeners
                .Add(new System.Diagnostics.EventLogTraceListener("Application"));

            Autodesk.Revit.UI.UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            try {
                // get hold of the print manager
                Autodesk.Revit.DB.PrintManager printManager =
                    doc.PrintManager;

                // select the printer
                printManager.SelectNewPrintDriver("Adobe PDF");
                printManager.PrintRange = Autodesk.Revit.DB.PrintRange.Select;
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

                // 
                IList<Autodesk.Revit.DB.ViewSheetSet> viewSheetSets = GetViewSheetSet(doc);
                System.Text.StringBuilder strBld = new System.Text.StringBuilder();
                foreach(Autodesk.Revit.DB.ViewSheetSet vss in viewSheetSets) {
                    strBld.AppendLine(string.Format("Name:{0}; Views.Count={1}", vss.Name, vss.Views.Size));
                }
                Trace.Write(strBld.ToString());
                return Autodesk.Revit.UI.Result.Succeeded;
                //

                // apply the updated settings and print 
                foreach (Autodesk.Revit.DB.View view in printManager.ViewSheetSetting.AvailableViews) {
                    if (view.ViewType == Autodesk.Revit.DB.ViewType.DrawingSheet) {
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
                size = "A4";

            return size;
        }

        void SetUpSizeAndPrint(Autodesk.Revit.DB.ViewSheet vs,
            Autodesk.Revit.DB.PrintManager printManager,
            Autodesk.Revit.DB.IPrintSetting printSetting)
        {
            Autodesk.Revit.DB.BoundingBoxUV bbUV = vs.Outline;
            double x = bbUV.Max.U - bbUV.Min.U;
            double y = bbUV.Max.V - bbUV.Min.V;
            if (x == 0 || y == 0)
                return;

            x = Converter.ConvertFromInternalUnits(x,
                Autodesk.Revit.DB.DisplayUnitType.DUT_MILLIMETERS);
            y = Converter.ConvertFromInternalUnits(y,
                Autodesk.Revit.DB.DisplayUnitType.DUT_MILLIMETERS);

            Trace.Write("x = " + x + "; y = " + y);

            string sheetSize = GetSheetSize(x, y, 100);

            if (x > y) {
                printSetting.PrintParameters.PageOrientation =
                    Autodesk.Revit.DB.PageOrientationType.Landscape;
            }
            else
                printSetting.PrintParameters.PageOrientation
                    = Autodesk.Revit.DB.PageOrientationType.Portrait;

            Trace.Write("sheetSize = " + sheetSize +
                "; PageOrientation: " + printSetting.PrintParameters.PageOrientation.ToString());

            foreach (Autodesk.Revit.DB.PaperSize ps in printManager.PaperSizes) {
                if (ps.Name == sheetSize) {
                    Trace.Write("ps.Name = " + ps.Name);
                    printSetting.PrintParameters.PaperSize = ps;
                    break;
                }
            }
            using (Autodesk.Revit.DB.Transaction t = new Autodesk.Revit.DB.Transaction(vs.Document)) {

                t.Start("temp");
                printManager.PrintToFileName =
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + vs.Name + ".pdf";
                printManager.PrintSetup.CurrentPrintSetting = printSetting;
                printManager.PrintSetup.SaveAs("temp");
                printManager.Apply();
                vs.Print(true);
                t.RollBack();
                /*unsafe {
                    System.IntPtr hwnd = NativeDlls.FindWindow(null, "Сохранить PDF-файл как");
                    Trace.Write("hwnd = " + hwnd.ToInt64());
                    if (hwnd.ToInt64() > 0) {
                        long lResult = NativeDlls.SendMessage(hwnd, 16, 0, 0);
                    }
                }*/
            }
        }

        private static IList<Autodesk.Revit.DB.ViewSheetSet> GetViewSheetSet(Document doc)
        {
            return new Collector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.ViewSheetSet))
                .Cast< Autodesk.Revit.DB.ViewSheetSet>()
                .ToList();
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
