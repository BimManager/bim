using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;

namespace TektaRevitPlugins
{
    [Transaction(TransactionMode.Manual)]
    public class SpHandlerCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the hold of the active document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            try {
                SharedParameterSaveDialog window =
                    new SharedParameterSaveDialog(doc, GetSharedParameters(doc));

                window.ShowDialog();                

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Result.Cancelled;
            }
            catch (Exception ex) {
                TaskDialog.Show("Exception", string.Format("StackTrace:\n{0}\nMessage:\n{1}",
                                                          ex.StackTrace, ex.Message));
                return Result.Failed;
            }
        }

        #region Helper Methods
        static string GetPathToFile()
        {
            string path = null;

            // Create a Revit task dialog to communicate information to the user.
            TaskDialog customTaskDialog = new TaskDialog("Shared Parameters");
            customTaskDialog.MainInstruction = "Please browse for a file to which  to export the shared parameters.";
            customTaskDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;

            // Add commandLink options
            customTaskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Browser");

            // Set common buttons
            customTaskDialog.CommonButtons = TaskDialogCommonButtons.Cancel;

            // Show the task dialog
            TaskDialogResult tResult = customTaskDialog.Show();

            if (TaskDialogResult.CommandLink1 == tResult) {
                FileSaveDialog fileSaveDialog = new FileSaveDialog("Text files|*.txt");

                //FileOpenDialog openFileDialog = new FileOpenDialog("Text files|*.txt");
                ItemSelectionDialogResult sbResult = fileSaveDialog.Show();
                if (sbResult == ItemSelectionDialogResult.Canceled) {
                    return path;
                }
                else if (sbResult == ItemSelectionDialogResult.Confirmed) {
                    ModelPath modelPath = fileSaveDialog.GetSelectedModelPath();
                    path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                }

            }
            else if (TaskDialogResult.Cancel == tResult) {
                return path;
            }
            return path;
        }

        public static void ExportParameters(Document doc, string path)
        {

            DefinitionFile existingDefFile = doc.Application.OpenSharedParameterFile();

            // 
            doc.Application.SharedParametersFilename = path;
            DefinitionFile tempDefFile = doc.Application.OpenSharedParameterFile();

            // Manager the Exported group
            DefinitionGroup exported = null;

            var query = from dg in tempDefFile.Groups
                        where dg.Name.Equals(doc.PathName + "-Exported")
                        select dg;

            if (query.Count() == 0)
                exported = tempDefFile.Groups.Create(doc.PathName + "-Exported");
            else
                exported = query.First();

            // Iterate over the shared parameters in the document
            BindingMap bindingMap = doc.ParameterBindings;
            DefinitionBindingMapIterator it =
                bindingMap.ForwardIterator();
            it.Reset();

            while (it.MoveNext()) {
                InternalDefinition definition = it.Key as InternalDefinition;
                Autodesk.Revit.DB.Binding currBinding = bindingMap.get_Item(definition);

                // Corroborate that the current parameter has not been exported previously
                Definition existingDef = exported.Definitions
                    .Where(d => (d.Name.Equals(definition.Name))).FirstOrDefault();
                if (existingDef != null)
                    continue;

                // unearth and assign the parameter's GUID
                SharedParameterElement sharedParamElem =
                    doc.GetElement(definition.Id) as SharedParameterElement;

                if (sharedParamElem == null)
                    continue;

                ExternalDefinitionCreationOptions options =
                    new ExternalDefinitionCreationOptions(definition.Name, definition.ParameterType);
                options.GUID = sharedParamElem.GuidValue;

                Definition createdDefinition = exported.Definitions.Create(options);
            }
        }

        public static IList<SharedParameter> GetSharedParameters(Document doc)
        {
            IList<SharedParameter> extSharedParams = new List<SharedParameter>();
            BindingMap bindingMap = doc.ParameterBindings;
            DefinitionBindingMapIterator it =
                bindingMap.ForwardIterator();
            it.Reset();

            while (it.MoveNext()) {
                InternalDefinition definition = it.Key as InternalDefinition;
                Autodesk.Revit.DB.Binding currBinding = bindingMap.get_Item(definition);

                // unearth the parameter's GUID
                SharedParameterElement sharedParamElem =
                    doc.GetElement(definition.Id) as SharedParameterElement;

                if (sharedParamElem == null)
                    continue;

                SharedParameter sharedParam =
                    new SharedParameter(definition.Name, definition.ParameterType,
                    sharedParamElem.GuidValue, currBinding, definition.ParameterGroup);

                extSharedParams.Add(sharedParam);
            }

            return extSharedParams;
        }
        #endregion
    }

    public class SharedParameter
    {
        #region Data Fields
        private ParameterType parameterType;
        private BuiltInParameterGroup parameterGroup;
        private Binding binding;
        private CategorySet categorySet;
        #endregion

        #region Properties
        public string Name { get; private set; }
        public string ParameterType
        {
            get {
                return Enum.GetName(typeof(ParameterType), parameterType);
            }

        }
        public Guid Guid { get; private set; }
        public string Binding
        {
            get {
                if (binding is TypeBinding) {
                    TypeBinding typeBinding = binding as TypeBinding;
                    return "Type Binding";
                }
                else
                    return "Instance Binding";
            }
        }
        public string CategorySet
        {
            get {
                StringBuilder strBuilder = new StringBuilder();
                if(binding is TypeBinding) {
                    TypeBinding typeBinding = binding as TypeBinding;
                    this.categorySet = typeBinding.Categories;
                    foreach(Category category in this.categorySet) {
                        strBuilder.AppendLine(category.Name + "; ");
                    }
                    return strBuilder.ToString();
                }
                else {
                    InstanceBinding instanceBinding= binding as InstanceBinding;
                    this.categorySet = instanceBinding.Categories;
                    foreach(Category category in this.categorySet) {
                        strBuilder.AppendLine(category.Name + "; ");
                    }
                    return strBuilder.ToString();
                }
            }
        }
        public string ParameterGroup
        {
            get {
                return Enum.GetName(typeof(BuiltInParameterGroup), parameterGroup); 
            }
        }

        public IList<Category> GetCategories
        {
            get {
                IList<Category> categories = new List<Category>();
                if (binding is TypeBinding) {
                    TypeBinding typeBinding = binding as TypeBinding;
                    this.categorySet = typeBinding.Categories;
                    foreach (Category category in this.categorySet) {
                        categories.Add(category);
                    }
                    return categories;
                }
                else {
                    InstanceBinding instanceBinding = binding as InstanceBinding;
                    this.categorySet = instanceBinding.Categories;
                    foreach (Category category in this.categorySet) {
                        categories.Add(category);
                    }
                    return categories;
                }
            }
        }
        #endregion

        #region Constructors
        public SharedParameter(string name, ParameterType parameterType, Guid guid,
            Binding binding, BuiltInParameterGroup parameterGroup)
        {
            this.Name = name;
            this.parameterType = parameterType;
            this.Guid = guid;
            this.binding = binding;
            this.parameterGroup = parameterGroup;
        }

        #endregion

        #region Methods

        #endregion

        #region Helper Methods

        #endregion
    }
}
