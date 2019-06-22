using System;
using System.Collections.Generic;
using System.Linq;

using Excel = Microsoft.Office.Interop.Excel;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using System.IO;

using RvtSchedule = Autodesk.Revit.DB.ViewSchedule;
using RvtRoom = Autodesk.Revit.DB.Architecture.Room;

namespace TektaRevitPlugins
{
    static class ExcelUtils
    {
        /// <summary>
        /// Default schedule data file field delimiter.
        /// </summary>
        static char[] m_tabs = new char[] { '\t' };
        /// <summary>
        /// Strip the quotes around text strings
        /// in the schedule data files.
        /// </summary>
        static char[] m_quotes = new char[] { '"' };

        static Excel.Application m_excelApp;
        static Excel.Workbook m_workbook;
        static Excel.Worksheet m_worksheet;
        static object misValue = System.Reflection.Missing.Value;

        static int counter = 1;

        public static void ExportToExcel(IEnumerable<dynamic> entities)
        {
            m_excelApp = new Excel.Application();
            m_excelApp.Visible = true;

            // Create a new, empty workbook and add it to the collection returned
            // by property Workbooks. The new workbook becomes the active workbook.
            // Add has an optional parameter for specifying a particular template.
            m_excelApp.Workbooks.Add();

            // This examples uses a single workSheet.
            m_worksheet = m_excelApp.ActiveSheet;

            // Establish column headings in cells A1 and A2
            m_worksheet.Cells[1, "A"] = "ID Number";
            m_worksheet.Cells[1, "B"] = "Current Balance";

            var row = 1;
            foreach (var entity in entities) {
                row++;
                m_worksheet.Cells[row, "A"] = entity.ID;
                m_worksheet.Cells[row, "B"] = entity.Balance;
            }

            m_worksheet.Columns[1].AutoFit();
            m_worksheet.Columns[2].AutoFit();

            // Put the spreadsheet content on the clipboard.
            m_worksheet.Range["A1:B3"].Copy();
        }

        public static void ImportFromExcel(string path, int sheet)
        {
            m_excelApp = new Excel.Application();
            m_workbook = m_excelApp.Workbooks.Open(path);
            m_worksheet = m_workbook.Worksheets[sheet];

            Console.WriteLine(ReadCell(0, 0));
        }

        static string ReadCell(int i, int j)
        {
            i++;
            j++;
            if (m_worksheet.Cells[i, j].Value2 != null)
                return m_worksheet.Cells[i, j].Value2;
            else
                return string.Empty;
        }

        public static IList<string> ReadExcel()
        {
            IList<RvtRoom> rooms = new List<RvtRoom>();
            IList<string> types = new List<string>();
            // Connection string
            string connectionString =
                ConfigurationManager.ConnectionStrings["excel"].ConnectionString;

            // Select using a Named range
            //string selectString = "SELECT * FROM Cutomers";

            // Select using a worksheet name
            //string selectStringSheet = "SELECT * FROM [Sheet1$]";

            using (System.Data.OleDb.OleDbConnection connection 
                = new System.Data.OleDb.OleDbConnection(connectionString)) {
                connection.Open();

                string sheetName =
                    connection.GetOleDbSchemaTable
                    (System.Data.OleDb.OleDbSchemaGuid.Tables, null).Rows[0]["TABLE_NAME"] as string;

                string selectString = "SELECT mcRm_SubZone FROM [ZONE_1$]";

                System.Data.OleDb.OleDbCommand myCommand = 
                    new System.Data.OleDb.OleDbCommand(selectString, connection);

                using (System.Data.OleDb.OleDbDataReader reader = 
                    myCommand.ExecuteReader()) {
                    try {
                        while (reader.Read()) {
                            //Room room = new Room(reader.GetString(0), reader.GetString(4),
                            //reader.GetString(6), reader.GetString(8), reader.GetString(14), reader.GetString(21));
                            //rooms.Add(room);
                            types.Add(reader.GetString(0));

                        }
                    }
                    catch (Exception ex) {
                        Trace.Write(string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                    }
                }
            }

            //return rooms;
            return types;

        }

        public static DataSet GetExcelDataAsDataSet(string commandString)
        {
            DataSet ds = new DataSet();

            try {
                // 1. Get the connection string from the .config file.
                string connectionString =
                    ConfigurationManager.ConnectionStrings["excel"].ConnectionString;

                // 2. Create and open a connection.
                using (System.Data.OleDb.OleDbConnection connection =
                    new System.Data.OleDb.OleDbConnection(connectionString)) {
                    connection.Open();

                    // 3. Create an OleDb command.
                    //string commandString = "SELECT * FROM [ZONE_1$]";
                    System.Data.OleDb.OleDbCommand command = 
                        new System.Data.OleDb.OleDbCommand(commandString, connection);

                    // 4. Obtain a data reader a la ExecuteReader().
                    //using (OleDbDataReader dataReader = command.ExecuteReader()) {
                    // 5. Loop over the results.
                    //}

                    System.Data.OleDb.OleDbDataAdapter dataAdapter = 
                        new System.Data.OleDb.OleDbDataAdapter(command);
                    dataAdapter.Fill(ds);
                    connection.Close();
                }
            }
            catch(Exception ex) {
                Trace.WriteLine(string.Format("Exception while connection to database: {0}\n{1}", 
                    ex.Message, ex.StackTrace));
            }

            return ds;
        }

        public static DataSet LoadAsBinary(string path)
        {
            DataSet ds = null;
            using(System.IO.FileStream fs=new FileStream(path, FileMode.Open)) {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bFormater =
                    new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                ds = (DataSet)bFormater.Deserialize(fs);
                fs.Close();
            }
            return ds;
        }
        [System.Obsolete]
        public static void ExportToExcel(DataTable dt)
        {
            try {
                m_excelApp = new Excel.Application();
                m_excelApp.EnableEvents = false;
                m_excelApp.Visible = false;
                m_excelApp.Interactive = false;
                m_excelApp.ScreenUpdating = false;

                // Create a new, empty workbook and add it to the collection returned
                // by property Workbooks. The new workbook becomes the active workbook.
                // Add has an optional parameter for specifying a particular template.
                m_workbook = m_excelApp.Workbooks.Add();
                m_worksheet = m_excelApp.ActiveSheet;
                var row = 1;

                // This examples uses a single workSheet.
                if (dt.TableName.Count() > 0) {
                    m_worksheet.Cells[row, 1] = dt.TableName;
                    ++row;
                }

                // Establish column headings in cells A1 and A2
                for (int i = 0; i < dt.Columns.Count; i++) {
                    m_worksheet.Cells[row, i + 1] = dt.Columns[i].ColumnName;
                }

                foreach (DataRow dataRow in dt.Rows) {
                    ++row;
                    for (int j = 0; j < dataRow.ItemArray.Length; j++) {
                        m_worksheet.Cells[row, j + 1] = dataRow.ItemArray[j];
                    }
                }

                // Format a worksheet
                m_worksheet.Columns.AutoFit();

                // Restore interactivity, GUI updates and calculation
                m_excelApp.Interactive = true;
                m_excelApp.ScreenUpdating = true;

                // save the workbook and exit
                string path =
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Test" +
                                                    new Random().Next(Int32.MaxValue);
                m_workbook.SaveAs(path, Excel.XlFileFormat.xlOpenXMLWorkbook,
                    misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlNoChange,
                    Excel.XlSaveConflictResolution.xlUserResolution, misValue, misValue, misValue, misValue);
                m_workbook.Close(false, misValue, misValue);
                m_excelApp.Quit();
            }
            catch (Exception ex) {
                Trace.Write("Exception while dealing with excel: " 
                    + ex.Message);
            }
            finally {
                ReleaseObject(m_excelApp);
                ReleaseObject(m_workbook);
                ReleaseObject(m_worksheet);
            }
        }

        public static void ExportToExcel(RvtSchedule schedule, string outputDirectory)
        {
            // Obtain and prepare data for export to excel
            object[,] data;
            int row;
            int column;
            string csvpath = GetCSVFilePath(schedule);

            FormatDataAsArray(csvpath, schedule, out data, out row, out column);

            try {
                // Open Excel
                m_excelApp = new Excel.Application();
                m_excelApp.EnableEvents = false;
                m_excelApp.Visible = false;
                m_excelApp.Interactive = false;
                m_excelApp.ScreenUpdating = false;

                m_workbook = m_excelApp.Workbooks.Add();
                m_worksheet = m_excelApp.ActiveSheet;

                // Write the data to the file
                var startCell = (Excel.Range)m_worksheet.Cells[1, 1];
                var endCell = (Excel.Range)m_worksheet.Cells[row, column];
                var writeRange = m_worksheet.Range[startCell, endCell];
                writeRange.Value = data;

                // Format a worksheet
                m_worksheet.Columns.AutoFit();

                // Restore interactivity, GUI updates and calculation
                m_excelApp.Interactive = true;
                m_excelApp.ScreenUpdating = true;

                // save the workbook and exit
                m_workbook.SaveAs(outputDirectory + "\\" + RemoveIllegalCharacters(schedule.Name),
                    Excel.XlFileFormat.xlOpenXMLWorkbook, misValue, misValue, misValue, misValue,
                    Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlUserResolution,
                    misValue, misValue, misValue, misValue);
            }
            catch (Exception ex) {
                Trace.Write("Exception while working with excel" + 
                    ex.Message);
            }
            finally {
                // Close the workbook and app
                m_workbook.Close(false, misValue, misValue);
                m_excelApp.Quit();

                // Release COM objects
                ReleaseObject(m_excelApp);
                ReleaseObject(m_workbook);
                ReleaseObject(m_worksheet);
            }
        }

        public static void ExportToExcel(IList<RvtSchedule> schedules, string outputDirectory)
        {
            try {
                m_excelApp = new Excel.Application();
                m_excelApp.EnableEvents = false;
                m_excelApp.Visible = false;
                m_excelApp.Interactive = false;
                m_excelApp.ScreenUpdating = false;

                m_workbook = m_excelApp.Workbooks.Add();

                foreach (RvtSchedule vs in schedules) {
                    string filepath = GetCSVFilePath(vs);
                    Trace.Write(filepath);
                    ExportToNewSheet(m_workbook, filepath, vs);
                    File.Delete(filepath);
                }

                // Restore interactivity, GUI updates and calculation
                m_excelApp.Interactive = true;
                m_excelApp.ScreenUpdating = true;

                // Save the file as an .xlsx
                m_workbook.SaveAs(outputDirectory + "\\" + schedules[0].Document.Title.Replace(".rvt", string.Empty),
                    Excel.XlFileFormat.xlOpenXMLWorkbook, misValue, misValue, misValue, misValue,
                    Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlUserResolution,
                    misValue, misValue, misValue, misValue);

               
            }
            catch (Exception ex) {
                Trace.Write(string.Format("{0},{1}", ex.Message, ex.StackTrace));
            }
            finally {
                // Close the workbook and app
                m_workbook.Close(false, misValue, misValue);
                m_excelApp.Quit();

                // release com objects
                ReleaseObject(m_excelApp);
                ReleaseObject(m_workbook);
            }
        }

        public static void ExportToNewSheet(Excel.Workbook workbook, string filename, RvtSchedule schedule)
        {
            Excel.Worksheet worksheet = null;
            int row;
            int column;
            object[,] data;
            FormatDataAsArray(filename, schedule, 
                out data, out row, out column);

            try {
                
                // add a new worksheet and begin to perform operations
                worksheet = workbook.Sheets.Add();

                /*string scheduleName = schedule.Name;
                string currWorksheetName = workSheet.Name;
                try {
                    workSheet.Name = RemoveIllegalCharacters(scheduleName);
                }
                catch(Exception ex) {
                    workSheet.Name = currWorksheetName;
                }*/

                worksheet.Activate();

                // select a range and assign the data to it
                var startCell = (Excel.Range)worksheet.Cells[1, 1];
                var endCell = (Excel.Range)worksheet.Cells[row, column];
                var writeRange = worksheet.Range[startCell, endCell];
                writeRange.Value = data;

                // Format a worksheet
                worksheet.Columns.AutoFit();
            }
            catch (Exception ex) {
                Trace.Write(string.Format
                    ("Exception while working with a worksheet!\n{0},{1}", 
                    ex.Message, ex.StackTrace));
            }
            finally {
                // Release a COM object
                ReleaseObject(worksheet);
            }
        }


        #region Helper Methods
        private static void ReleaseObject(object obj)
        {
            try {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex) {
                obj = null;
                Trace.Write(string.Format("Unable to release the object.\n{0}\n{1}",
                    ex.Message, ex.StackTrace));
            }
            finally {
                GC.Collect();
            }
        }

        private static string RemoveIllegalCharacters(string scheduleName) {
            // get all illicit characters
            string reservedCharacters = new string(Path.GetInvalidFileNameChars()) +
                new string(Path.GetInvalidPathChars()) + "#$?•";

            // replace all illegitimate chars in the schedule name
            foreach (Char c in reservedCharacters) {
                scheduleName = scheduleName.Replace(c.ToString(), string.Empty);
            }

            scheduleName = scheduleName.Replace('.', '_');

            return scheduleName;
        }

        private static string GetCSVFilePath(RvtSchedule vs) {
            // Create a temp file for exporting a .csv file
            string tempPath = System.IO.Path.GetTempPath();
            string scheduleName = RemoveIllegalCharacters(vs.Name);

            Trace.Write("After: " + scheduleName);

            vs.Export(tempPath, counter + ".txt",
                new Autodesk.Revit.DB.ViewScheduleExportOptions());

            string filepath = tempPath + counter + ".txt";
            ++counter;

            return filepath;
        }

        private static void FormatDataAsArray(string csvfilename, RvtSchedule schedule,
            out object[,] data, out int row, out int column) {
            row = 0;
            column = 0;
            data = new object[1000, 500];

            using (StreamReader stream = File.OpenText(csvfilename)) {
                string line;
                string[] a;

                while ((line = stream.ReadLine()) != null) {
                    a = line
                        .Split(m_tabs)
                        .Select<string, string>(s => s.Trim(m_quotes))
                        .ToArray();

                    // First line of text file contains
                    // schedule name
                    if (schedule.Definition.ShowTitle && row == 0) {
                        data[row, column] = a[0];
                        ++row;
                        continue;
                    }

                    // Second line of text file contains
                    // schedule column names
                    if (schedule.Definition.ShowHeaders && (row == 0 || row == 1)) {
                        for (int i = 0; i < a.Count(); i++) {
                            data[row, column] = a[i];
                            ++column;
                        }
                        ++row;
                        continue;
                    }

                    // Remaining lines define schedule data
                    for (int i = 0; i < a.Count(); i++) {
                        data[row, i] = a[i];
                        if (column == 0)
                            column = a.Count();
                    }
                    ++row;
                }
                stream.Close();
            }
        }
        #endregion


    }
}
