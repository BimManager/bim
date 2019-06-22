using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using RvtColor = Autodesk.Revit.DB.Color;

namespace TektaRevitPlugins
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class FontReplacerCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // add a listener 
            EventLogTraceListener logListener =
                new EventLogTraceListener("Application");
            Trace.Listeners.Add(logListener);
            TraceSwitch traceSwitch = new TraceSwitch("General", ("Font Replacer Supervisor"));
            traceSwitch.Level = TraceLevel.Verbose;

            // access the current document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                // generate and show a selection dialog box
                SelectFontWindow fontWindow = new SelectFontWindow();
                fontWindow.ShowDialog();

                // get the selected font
                Font newFont = fontWindow.PickedFont;
                if (newFont == null) // if no font has been picked, an empty string is returned.
                    return Result.Cancelled;

                if (fontWindow.TextNoteTypes)
                    FontReplacer.ReplaceFontInTextNotes(doc, newFont);
                if (fontWindow.DimensionTypes)
                    FontReplacer.ReplaceFontInDimTypes(doc, newFont);
                if (fontWindow.Families)
                    FontReplacer.ReplaceFontInFamilies(doc, newFont);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Trace.WriteIf(traceSwitch.TraceError,
                    string.Format("Message:{0}\nStackTrack:{1}",
                        ex.Message, ex.StackTrace));
                TaskDialog.Show("Exception", ex.Message);
                return Result.Failed;
            }
            finally
            {
                Trace.WriteIf(traceSwitch.TraceVerbose,
                    FontReplacer.ProcessedFamilies);
                Trace.Flush();
                Trace.Close();
            }
        }
    }

    internal class SimpleFamilyLoadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            if (!familyInUse)
            {
                overwriteParameterValues = true;
                return true;
            }
            else
            {
                overwriteParameterValues = true;
                return true;
            }
        }

        public bool OnSharedFamilyFound(Family sharedFamily,
            bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            if (!familyInUse)
            {
                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
            else
            {
                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
        }
    }

    internal class AllWarningSwallower : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            failuresAccessor.DeleteAllWarnings();

            return FailureProcessingResult.Continue;
        }
    }

    internal class FontReplacer
    {
        #region Data Fields
        static StringBuilder strBld = new StringBuilder();
        static IList<string> processedFamilies = new List<string>();
        public static string ProcessedFamilies
        {
            get
            {
                StringBuilder strBld = new StringBuilder();
                foreach (string family in processedFamilies)
                    strBld.AppendLine(family);

                return strBld.ToString();
            }
        }
                    
        #endregion

        #region Methods
        public static void ReplaceFontInFamilies(Document currDoc, Font pickedFont)
        {
            // get hold of all annotation families in the document
            IList<Family> familiesToProcess = GetAnnotationFamilies(currDoc);

            // iterate over the family collection
            foreach (Family family in familiesToProcess)
            {
                //if (processedFamilies.Contains(family.Name))
                //continue;

                // If the present family is related to an elevation mark body, then skip it.
                if (family.get_Parameter(BuiltInParameter.FAMILY_IS_ELEVATION_MARK_BODY) != null &&
                    family.get_Parameter(BuiltInParameter.FAMILY_IS_ELEVATION_MARK_BODY).AsInteger() == 0)
                    continue;

                Document nestedDoc = currDoc.EditFamily(family);
                try
                {
                    // unearth all TextElementTypes
                    IList<TextElementType> textElementTypes =
                           GetTextElementTypes(nestedDoc);

                    // modify Text Element Types
                    if (textElementTypes.Count != 0)
                        ModifyTextElemTypes(nestedDoc, textElementTypes, pickedFont);

                    // mark that the family has been dealt with
                    processedFamilies.Add(family.Name);

                    // if the document contains nested families, then deal with them
                    // through recursion
                    if (GetAnnotationFamilies(nestedDoc).Count != 0)
                        ReplaceFontInFamilies(nestedDoc, pickedFont);

                    // load the modified family back into the document/family
                    Family reloadedFamily =
                        nestedDoc.LoadFamily(currDoc, new SimpleFamilyLoadOptions());
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Reloading Exception",
                        string.Format("Message:{0}\nStackTrack:{1}.",
                        ex.Message, ex.StackTrace));
                    break;
                }
                finally
                {
                    nestedDoc.Close(false);
                    nestedDoc.Dispose();
                }
            }
        }
        public static void ReplaceFontInTextNotes(Document doc, Font pickedFont)
        {
            // get hold of all textote in the doc
            IList<TextElementType> textNoteTypes = new List<TextElementType>();

            // cast them into the TextNoteType
            foreach (TextNoteType txtNoteType in GetTextNoteTypes(doc))
            {
                textNoteTypes.Add((TextElementType)txtNoteType);
            }

            // Modify them
            ModifyTextElemTypes(doc, textNoteTypes, pickedFont);
        }
        public static void ReplaceFontInDimTypes(Document doc, Font pickedFont)
        {
            IList<DimensionType> dimTypes = GetDimensionTypes(doc);
            if (dimTypes.Count == 0)
                return;

            using (Transaction t = new Transaction(doc, "Rename Dim Types"))
            {
                t.Start();
                FailureHandlingOptions failureHandling = t.GetFailureHandlingOptions();
                failureHandling.SetFailuresPreprocessor(new AllWarningSwallower());
                t.SetFailureHandlingOptions(failureHandling);
                foreach (DimensionType dimType in dimTypes)
                {
                    if (!dimType.CanBeRenamed)
                        continue;

                    using (SubTransaction st = new SubTransaction(doc))
                    {
                        st.Start();
                        SetElemTypeAttributes(dimType, pickedFont);
                        st.Commit();

                        doc.Regenerate();

                        st.Start();
                        dimType.Name = GenerateElemTypeName(dimType);
                        st.Commit();
                    }
                }
                t.Commit();
            }
        }
        #endregion

        #region Helper Methods
        private static IList<Family> GetAnnotationFamilies(Document doc)
        {
            return
                (from family in new FilteredElementCollector(doc)
                 .OfClass(typeof(Family))
                 .Cast<Family>()
                 where family.FamilyCategory.CategoryType ==
                 CategoryType.Annotation && family.IsEditable
                 select family)
                 .OrderBy<Family, string>(f => f.Name)
                 .ToList();
        }
        private static IList<TextElementType> GetTextElementTypes(Document doc)
        {
            return
                new FilteredElementCollector(doc)
                .OfClass(typeof(TextElementType))
                .Cast<TextElementType>()
                .ToList();
        }
        private static IList<TextNoteType> GetTextNoteTypes(Document doc)
        {
            return
                new FilteredElementCollector(doc)
                .OfClass(typeof(TextNoteType))
                .Cast<TextNoteType>()
                .ToList();
        }
        private static IList<DimensionType> GetDimensionTypes(Document doc)
        {
            return
                new FilteredElementCollector(doc)
                .OfClass(typeof(DimensionType))
                .Cast<DimensionType>()
                .ToList();
        }

        private static Color ConvertIntToColor(int rgb)
        {
            byte[] rgbAsBytes = BitConverter.GetBytes(rgb);
            if (BitConverter.IsLittleEndian)
                return new Color(rgbAsBytes[0], rgbAsBytes[1], rgbAsBytes[2]);
            else
                return new Color(rgbAsBytes[1], rgbAsBytes[2], rgbAsBytes[3]);
        }
        private static string GenerateElemTypeName(ElementType elemType)
        {
            Color color = ConvertIntToColor(elemType.get_Parameter(BuiltInParameter.LINE_COLOR).AsInteger());
            string colorAsString = string.Format("({0},{1},{2})", color.Red, color.Green, color.Blue);

            string pattern = @"[\d]+.[\d]{1}";

            return string.Format("{0}({1})-{2}{3}{4}{5}-{6} {7}",
                                 Regex.Match(elemType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString(), pattern).Value,
                                 elemType.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).AsValueString(),
                                 elemType.get_Parameter(BuiltInParameter.TEXT_FONT).AsString(),
                                 elemType.get_Parameter(BuiltInParameter.TEXT_STYLE_UNDERLINE).AsInteger() == 1 ? "-U" : "",
                                 elemType.get_Parameter(BuiltInParameter.TEXT_STYLE_BOLD).AsInteger() == 1 ? "-B" : "",
                                 elemType.get_Parameter(BuiltInParameter.TEXT_STYLE_ITALIC).AsInteger() == 1 ? "-I" : "",
                                 colorAsString,
                                 (elemType is DimensionType) ? elemType.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME).AsString().Split(' ')[0] : ""
                                 );
        }

        private static void ModifyTextElemTypes(Document doc, IList<TextElementType> textElementTypes, Font pickedFont)
        {
            using (Transaction t = new Transaction(doc, "Replace fonts in family"))
            {
                t.Start();
                FailureHandlingOptions failureHandling = t.GetFailureHandlingOptions();
                failureHandling.SetFailuresPreprocessor(new AllWarningSwallower());
                t.SetFailureHandlingOptions(failureHandling);
                foreach (TextElementType textElementType in textElementTypes)
                {
                    try
                    {
                        using (SubTransaction st = new SubTransaction(doc))
                        {
                            // set the new font family to the existing type
                            st.Start();
                            SetElemTypeAttributes(textElementType, pickedFont);
                            st.Commit();

                            doc.Regenerate();

                            // rename the type
                            st.Start();
                            if (textElementType.CanBeRenamed)
                            {
                                textElementType.Name =
                                    GenerateElemTypeName(textElementType);
                            }
                            st.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Exception", String.Format("Message:{0}\nStackTrace:{1}\n",
                            ex.Message, ex.StackTrace));
                        return;
                    }
                }
                t.Commit();
            }
        }

        static void SetElemTypeAttributes(ElementType elementType, Font pickedFont)
        {
            using (SubTransaction st = new SubTransaction(elementType.Document)) {
                st.Start();
                elementType.get_Parameter(BuiltInParameter.TEXT_FONT).Set(pickedFont.FontName);
                elementType.get_Parameter(BuiltInParameter.TEXT_STYLE_BOLD).Set(pickedFont.IsBold ? 1 : 0);
                elementType.get_Parameter(BuiltInParameter.TEXT_STYLE_UNDERLINE).Set(pickedFont.IsUnderline ? 1 : 0);
                elementType.get_Parameter(BuiltInParameter.TEXT_STYLE_ITALIC).Set(pickedFont.IsItalic ? 1 : 0);
                elementType.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).Set(pickedFont.WidthFactor);
                st.Commit();
            }
        }
        #endregion

    }

    public class Font
    {
        #region Properties
        public string FontName { get; protected set; }
        public double FontSize { get; protected set; }
        public bool IsBold { get; protected set; }
        public bool IsItalic { get; protected set; }
        public bool IsUnderline { get; protected set; }
        public double WidthFactor { get; protected set; }
        public int Color { get; protected set; }
        public bool IsOpaque { get; protected set; }
        #endregion
        #region Constructors
        public Font(string font, double size, double widthFactor = 1,
            bool isBold = false, bool isItalic = false,
            bool isUnderline = false, bool isOpaque = true)
        {
            FontName = font;
            FontSize = size;
            Color = ColorToInt(new RvtColor(0, 0, 0));
            WidthFactor = widthFactor;
            IsBold = isBold;
            IsItalic = isItalic;
            IsUnderline = isUnderline;
            IsOpaque = isOpaque;
        }
        #endregion

        #region Helper Methods
        private static int ColorToInt(RvtColor color)
        {
            return (int)(Math.Pow(256, 0) * color.Red +
                Math.Pow(256, 1) * color.Green +
                Math.Pow(256, 2) * color.Blue);
        }
        private static RvtColor IntToColor(int value)
        {
            byte red = (byte)(value % 256);
            byte blue = (byte)Math.Floor((double)value / (255 * 256 + 255));
            byte green = (byte)((value - red - blue * Math.Pow(256, 2)) / 256);

            return new RvtColor(red, green, blue);
        }
        #endregion
    }

}
