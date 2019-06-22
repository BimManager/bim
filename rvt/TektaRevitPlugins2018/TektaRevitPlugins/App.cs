using System;
using System.Text.RegularExpressions;

using System.Windows.Media.Imaging;
using System.Reflection;
using System.IO;

using Autodesk.Revit.UI;

namespace TektaRevitPlugins
{
    class App : IExternalApplication
    {
        #region Data Fields
        static string ASSEMBLY_PATH = Assembly.GetExecutingAssembly().Location;
        static string NAMESPACE = Assembly.GetExecutingAssembly().GetTypes()[0].Namespace;
        const string PANEL_NAME = "Tekta";
        #endregion

        public Result OnShutdown(UIControlledApplication application)
        {
            //application.ControlledApplication.DocumentOpened -= docOpenedHandler;
            //application.ControlledApplication.DocumentClosing -= docClosingHandler;
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            #region Events
            //application.ControlledApplication.DocumentOpened += docOpenedHandler;
            //application.ControlledApplication.DocumentClosing += docClosingHandler;
            #endregion

            #region Create Ribbon Panel
            RibbonPanel ribbonPanel =
                application.CreateRibbonPanel(Tab.AddIns, PANEL_NAME);
            #endregion

            #region Push Buttons
            PushButton pbSplitter = ribbonPanel
                .AddItem(new PushButtonData
                ("Splitter", "Splitter", ASSEMBLY_PATH, 
                "TektaRevitPlugins.SplitterCmd")) as PushButton;
            pbSplitter.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.axe_32x32.png");
            pbSplitter.Image =
                BmpImageSource("TektaRevitPlugins.Resources.axe_16x16.png");

            PushButton pbFontReplacer = ribbonPanel.AddItem(
                new PushButtonData("Font Replacer", "Font Replacer", ASSEMBLY_PATH,
                "TektaRevitPlugins.FontReplacerCmd")) as PushButton;
            pbFontReplacer.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.replacer_32x32.png");
            pbFontReplacer.Image =
                BmpImageSource("TektaRevitPlugins.Resources.replacer_16x16.png");

            PushButton pbParametersHandler=ribbonPanel.AddItem(
                new PushButtonData("Parameter Handler", "Parameter Handler", ASSEMBLY_PATH,
                "TektaRevitPlugins.SpHandlerCmd")) as PushButton;
            pbParametersHandler.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.export_32x32.png");
            pbParametersHandler.Image =
                BmpImageSource("TektaRevitPlugins.Resources.export_16x16.png");

            #endregion

            #region Split Buttons
            PushButtonData pbdRebarCollector =
                new PushButtonData("Rebar Collector", "Rebar Collector", ASSEMBLY_PATH,
                "TektaRevitPlugins.RebarCollectorCmd");
            pbdRebarCollector.LargeImage = 
                BmpImageSource("TektaRevitPlugins.Resources.rebar_32x32.png");
            pbdRebarCollector.Image = 
                BmpImageSource("TektaRevitPlugins.Resources.rebar_16x16.png");

            PushButtonData pbdRebarMarker =
                new PushButtonData("Rebar Marker", "Rebar Marker", ASSEMBLY_PATH,
                "TektaRevitPlugins.RebarMarkerCmd");
            pbdRebarMarker.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.tag_32x32.png");
            pbdRebarMarker.Image =
                BmpImageSource("TektaRevitPlugins.Resources.tag_16x16.png");

            PushButtonData pbdScheduleGenerator =
                new PushButtonData("Schedule Generator", "Schedule Generator", ASSEMBLY_PATH,
                "TektaRevitPlugins.Multischedule.CmdConstructMultischedule");
            pbdScheduleGenerator.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.schedule_32x32.png");
            pbdScheduleGenerator.Image =
                BmpImageSource("TektaRevitPlugins.Resources.schedule_16x16.png");

            PushButtonData pbdUpdateRepository =
                new PushButtonData("Generate/Update Repository", "Generate/Update Repository", ASSEMBLY_PATH,
                "TektaRevitPlugins.UpdateRepository");
            pbdUpdateRepository.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.sync_32x32.png");
            pbdUpdateRepository.Image =
                BmpImageSource("TektaRevitPlugins.Resources.sync_16x16.png");

            SplitButton sbRebarCmds = 
                ribbonPanel.AddItem(new SplitButtonData(
                    "Rebar Commands", "Reinfrocement")) as SplitButton;
            sbRebarCmds.AddPushButton(pbdRebarCollector);
            sbRebarCmds.AddPushButton(pbdRebarMarker);
            sbRebarCmds.AddPushButton(pbdScheduleGenerator);
            sbRebarCmds.AddPushButton(pbdUpdateRepository);

            PushButtonData pbdExcelExporter = new PushButtonData
                ("Excel Exporter", "Excel Exporter", ASSEMBLY_PATH,
                "TektaRevitPlugins.ExcelExporterCmd");
            pbdExcelExporter.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.excel_32x32.png");
            pbdExcelExporter.Image =
                BmpImageSource("TektaRevitPlugins.Resources.excel_16x16.png");

            PushButtonData pbdSheetSelector = new PushButtonData
                ("PDF Printer", "PDF Printer", ASSEMBLY_PATH,
                "TektaRevitPlugins.SheetSelectorCmd");
            pbdSheetSelector.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.pdf_32x32.png");
            pbdSheetSelector.Image =
                BmpImageSource("TektaRevitPlugins.Resources.pdf_16x16.png");

            SplitButton sbExportCmds =
                ribbonPanel.AddItem(new SplitButtonData(
                    "Export Commands", "Export Commands")) as SplitButton;
            sbExportCmds.AddPushButton(pbdExcelExporter);
            sbExportCmds.AddPushButton(pbdSheetSelector);

            PushButtonData pbdRoomNamer =
                new PushButtonData("Room Namer", "Room Namer", ASSEMBLY_PATH,
                "TektaRevitPlugins.RoomNamerCmd");
            pbdRoomNamer.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.reroom_32x32.png");
            pbdRoomNamer.Image =
                BmpImageSource("TektaRevitPlugins.Resources.reroom_16x16.png");

            PushButtonData pbdHeightsMarker =
                new PushButtonData("Heights Marker", "Heights Marker", ASSEMBLY_PATH,
                "TektaRevitPlugins.MarkHeightsCmd");
            pbdHeightsMarker.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.height_32x32.png");
            pbdHeightsMarker.Image =
                BmpImageSource("TektaRevitPlugins.Resources.height_16x16.png");

            PushButtonData pdCalculateFlatArea =
                new PushButtonData("Flat Area Calculator", "Flat Area Calculator", ASSEMBLY_PATH,
                "TektaRevitPlugins.CalculateFlatAreaCmd");
            pdCalculateFlatArea.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.calculator_32x32.png");
            pdCalculateFlatArea.Image =
                BmpImageSource("TektaRevitPlugins.Resources.calculator_16x16.png");

            PushButtonData pdMhbkFlatHandler =
                new PushButtonData("MHBK Flat Handler", "MHBK Flat Handler", ASSEMBLY_PATH,
                "TektaRevitPlugins.RoomAreasHandlerCmd");
            pdMhbkFlatHandler.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.calculator_32x32.png");
            pdMhbkFlatHandler.Image =
                BmpImageSource("TektaRevitPlugins.Resources.calculator_16x16.png");
            pdMhbkFlatHandler.AvailabilityClassName = "TektaRevitPlugins.FlatHandlerAvailability";

            PushButtonData pbdTagElements=
                new PushButtonData("Tag Elements", "Tag Elements", ASSEMBLY_PATH,
                "TektaRevitPlugins.TagElemsCmd");
            pbdTagElements.LargeImage =
                BmpImageSource("TektaRevitPlugins.Resources.tag_32x32.png");
            pdCalculateFlatArea.Image =
                BmpImageSource("TektaRevitPlugins.Resources.tag_16x16.png");

            PushButtonData pbdDoorSwingDirection =
                SpecifyPushButtonData("Swing Direction", "Swing Direction",
                ASSEMBLY_PATH, "TektaRevitPlugins.Commands.DoorSwingDetectorCmd",
                "TektaRevitPlugins.Resources.door_swing_32x32.png",
                "TektaRevitPlugins.Resources.door_swing_16x16.png");

            PushButtonData pbdCopyValues =
                SpecifyPushButtonData("Copy Values", "Copy Values",
                ASSEMBLY_PATH, "TektaRevitPlugins.CpValuesCmd",
                "TektaRevitPlugins.Resources.copy_32x32.png",
                "TektaRevitPlugins.Resources.copy_16x16.png");

            PushButtonData pbdBlkLvlId =
                SpecifyPushButtonData("Room Data", "Room Data",
                ASSEMBLY_PATH, "TektaRevitPlugins.Commands.BlockLevelIdentifierCmd",
                "TektaRevitPlugins.Resources.calculator_32x32.png",
                "TektaRevitPlugins.Resources.calculator_16x16.png");

            PushButtonData pbdBEnumValues =
                SpecifyPushButtonData("Enumerate Values", "Enumerate Values",
                ASSEMBLY_PATH, "TektaRevitPlugins.Commands.EnumValuesCmd",
                "TektaRevitPlugins.Resources.numbers_32x32.png",
                "TektaRevitPlugins.Resources.numbers_16x16.png");


            // Split Buttons
            SplitButton sbArchCmds =
                ribbonPanel.AddItem(new SplitButtonData(
                    "Arch Commands", "Arch Commands")) as SplitButton;
            sbArchCmds.AddPushButton(pbdRoomNamer);
            sbArchCmds.AddPushButton(pbdHeightsMarker);
            sbArchCmds.AddPushButton(pdCalculateFlatArea);
            sbArchCmds.AddPushButton(pdMhbkFlatHandler);
            sbArchCmds.AddPushButton(pbdTagElements);
            sbArchCmds.AddPushButton(pbdDoorSwingDirection);
            sbArchCmds.AddPushButton(pbdCopyValues);
            sbArchCmds.AddPushButton(pbdBlkLvlId);
            sbArchCmds.AddPushButton(pbdBEnumValues);
            #endregion

            #region Register Updaters

            #endregion

            return Result.Succeeded;
        }

        #region Helper Methods
        System.Windows.Media.ImageSource BmpImageSource(string embeddedPath)
        {
            Stream stream = this.GetType().Assembly.GetManifestResourceStream(embeddedPath);
            var decoder = new System.Windows.Media.Imaging.PngBitmapDecoder
                (stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            return decoder.Frames[0];
        }

        PushButtonData SpecifyPushButtonData(
            string name, string text, string assemblyName,
            string className, string largeImage, string image)
        {
            PushButtonData pushButtonData =
                new PushButtonData(name, text, ASSEMBLY_PATH, className);

            pushButtonData.LargeImage =
                BmpImageSource(largeImage);
            pushButtonData.Image =
                BmpImageSource(image);

            return pushButtonData;
        }
        #endregion

        #region Event Handlers
        EventHandler<Autodesk.Revit.DB.Events.DocumentOpenedEventArgs> docOpenedHandler =
            (object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs args) => {
                string pattern = @"NEZH-RELIDE-[UND]*[TH]*(B0[12])*-ZZ-M3-A-0001[_a-z]*.rvt";
                Regex regex = new Regex(pattern);
                if (regex.IsMatch(args.Document.Title)) {
                    OpeningUpdater openingUpdater = new OpeningUpdater(args.Document.Application.ActiveAddInId);
                    openingUpdater.Register(args.Document);
                    //Autodesk.Revit.UI.TaskDialog.Show("Info", "Registered!");
                    openingUpdater.AddTriggerForUpdater(args.Document);
                    //Autodesk.Revit.UI.TaskDialog.Show("Info", "Triggers set!");

                    if (!Autodesk.Revit.DB.UpdaterRegistry.IsUpdaterEnabled(openingUpdater.GetUpdaterId()))
                        Autodesk.Revit.DB.UpdaterRegistry.EnableUpdater(openingUpdater.GetUpdaterId());
                }
            };

        EventHandler <Autodesk.Revit.DB.Events.DocumentClosingEventArgs> docClosingHandler =
            (object sender, Autodesk.Revit.DB.Events.DocumentClosingEventArgs args) => {
                string pattern = @"NEZH-RELIDE-[UND]*[TH]*(B0[12])*-ZZ-M3-A-0001[_a-z]*.rvt";
                Regex regex = new Regex(pattern);
                if (regex.IsMatch(args.Document.Title)) {
                    Autodesk.Revit.DB.Document doc = args.Document;
                    OpeningUpdater openingUpdater = new OpeningUpdater(args.Document.Application.ActiveAddInId);

                    Autodesk.Revit.DB.UpdaterRegistry.RemoveAllTriggers(openingUpdater.GetUpdaterId());
                    Autodesk.Revit.DB.UpdaterRegistry.IsUpdaterRegistered(openingUpdater.GetUpdaterId(), doc);
                    }
            };
        #endregion
    }
}
