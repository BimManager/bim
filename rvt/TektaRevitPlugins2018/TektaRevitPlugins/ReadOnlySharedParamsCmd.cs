using System;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ReadOnlySharedParamsCmd : Autodesk.Revit.UI.IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData, 
            ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            System.Diagnostics.Trace.Listeners.
                Add(new System.Diagnostics.EventLogTraceListener("Application"));

            Autodesk.Revit.UI.UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            
            try {
                // set the location of a new .txt file
                string filePath = System.Environment
                    .GetFolderPath(Environment.SpecialFolder.Personal) +
                    "\\" + doc.Title + ".txt";

                // Create the .txt file containing shared parameter definitions
                using (System.IO.Stream s = System.IO.File.Create(filePath)) {
                    s.Close();
                }

                // set the file as a shared parameter source file to the application
                doc.Application.SharedParametersFilename = filePath;

                // Get access to the file
                Autodesk.Revit.DB.DefinitionFile defFile = 
                    doc.Application.OpenSharedParameterFile();

                // Create a new group called 'ReadOnly'
                Autodesk.Revit.DB.DefinitionGroup defGroup = 
                    defFile.Groups.Create("ReadOnly");

                // Create a new defintion
                Autodesk.Revit.DB.ExternalDefinitionCreationOptions defCrtOptns =
                    new Autodesk.Revit.DB.ExternalDefinitionCreationOptions
                    ("RM_BLOCK", Autodesk.Revit.DB.ParameterType.Text);
                defCrtOptns.UserModifiable = false;
                defCrtOptns.Visible = true;

                // Insert the definition into the group
                Autodesk.Revit.DB.Definition def = 
                    defGroup.Definitions.Create(defCrtOptns);

                // Lay out the categories to which the param
                // will be bound
                Autodesk.Revit.DB.CategorySet catSet =
                    new Autodesk.Revit.DB.CategorySet();
                catSet.Insert(doc.Settings.Categories
                    .get_Item(Autodesk.Revit.DB.BuiltInCategory.OST_Rooms));

                // Define a binding type
                Autodesk.Revit.DB.InstanceBinding instBnd = 
                    new Autodesk.Revit.DB.InstanceBinding(catSet);

                // Bind the parameter to the active document
                using (Autodesk.Revit.DB.Transaction t =
                    new Autodesk.Revit.DB.Transaction(doc)) {
                    t.Start("Add Param");
                    Autodesk.Revit.DB.SubTransaction st = 
                        new Autodesk.Revit.DB.SubTransaction(doc);
                    doc.ParameterBindings.Insert
                        (def, instBnd, Autodesk.Revit.DB.BuiltInParameterGroup.PG_DATA);
                    t.Commit();
                }

                Autodesk.Revit.DB.DefinitionBindingMapIterator itr =
                    doc.ParameterBindings.ForwardIterator();
                
                //
                while(itr.MoveNext()) {
                    Autodesk.Revit.DB.InternalDefinition intDef = itr.Current as
                    Autodesk.Revit.DB.InternalDefinition;
                }

                Autodesk.Revit.UI.TaskDialog.Show("Success", 
                    string.Format("The parameter called {0} has been successfully added to the document", 
                    def.Name));

                return Autodesk.Revit.UI.Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Autodesk.Revit.UI.Result.Cancelled;
            }
            catch(System.Exception ex) {
                Autodesk.Revit.UI.TaskDialog.Show("Exception",
                    string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                return Autodesk.Revit.UI.Result.Failed;
            }
        }
    }
}
