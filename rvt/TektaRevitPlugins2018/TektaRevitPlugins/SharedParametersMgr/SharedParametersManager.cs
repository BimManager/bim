using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace TektaRevitPlugins
{
    class SharedParametersManager
    {
        #region Data Fields
        Document m_doc;
        #endregion

        #region Constructors
        public SharedParametersManager(Document doc) {
            m_doc = doc;
        }
        #endregion

        #region Methods
        /// <summary>
        /// The method generates a new shared parameter, 
        /// then binding it to a particular category in a document
        /// </summary>
        /// <param name="parameterName">Shared Parameter Name</param>
        /// <param name="parameterType">Shared Parameter Type</param>
        /// <param name="categories">Categories to bind the parameter to</param>
        /// <param name="parameterGroup"></param>
        /// <param name="isInstance">Whether the parameter is an instance or type one</param>
        /// <param name="isModifiable">Whether the user is allowed to modify the parameter</param>
        /// <param name="isVisible">Is it visible to the user</param>
        /// <param name="defGroupName">The name of a group in a shared parameter file</param>
        /// <param name="createTmpFile">Is a temp shared parameter file to be generated</param>
        /// <param name="guid">The guid of the shared parameter</param>
        internal void CreateSharedParameter(
            string parameterName,
            ParameterType parameterType,
            IList<BuiltInCategory> categories,
            BuiltInParameterGroup parameterGroup,
            bool isInstance, bool isModifiable, bool isVisible,
            string defGroupName = "Custom", bool createTmpFile = false, 
            Guid guid = default(Guid)) { 
            string tempFilePath = null;
            try {
                // Get access to the file
                DefinitionFile defFile =
                    m_doc.Application.OpenSharedParameterFile();

                if (defFile == null || createTmpFile) {
                    // set the location of a new .txt file
                    tempFilePath =
                            System.IO.Path.GetTempFileName() + ".txt";

                    // Create the .txt file containing shared parameter definitions
                    using (System.IO.Stream s = System.IO.File.Create(tempFilePath)) {
                        s.Close();
                    }

                    // set the file as a shared parameter source file to the application
                    m_doc.Application.SharedParametersFilename = tempFilePath;
                    defFile =
                        m_doc.Application.OpenSharedParameterFile();
                }

                // Create a new group called
                DefinitionGroup defGroup =
                    defFile.Groups.FindDefinitionGroup(defGroupName);
                if (defGroup == null)
                    defGroup = defFile.Groups.Create(defGroupName);

                // Create a new defintion
                ExternalDefinitionCreationOptions defCrtOptns =
                    new ExternalDefinitionCreationOptions
                    (parameterName, parameterType);
                defCrtOptns.UserModifiable = isModifiable;
                defCrtOptns.Visible = isVisible;
                if (guid != default(Guid))
                    defCrtOptns.GUID = guid;

                // Insert the definition into the group
                Definition def =
                    defGroup.Definitions.Create(defCrtOptns);

                // Lay out the categories to which the params
                // will be bound
                CategorySet categorySet = new CategorySet();

                foreach (BuiltInCategory category in categories)
                    categorySet.Insert(m_doc.Settings.Categories.get_Item(category));

                // Define a binding type
                Binding binding = null;
                if (isInstance)
                    binding = new InstanceBinding(categorySet);
                else
                    binding = new TypeBinding(categorySet);

                // Bind the parameter to the active document
                using (Transaction t = new Transaction(m_doc)) {
                    t.Start("Add Read Only Parameter");
                    m_doc.ParameterBindings.Insert
                        (def, binding, parameterGroup);
                    t.Commit();
                }

                Autodesk.Revit.UI.TaskDialog.Show("Success",
                    string.Format("The paramter {0} has been successfully added to the document",
                    def.Name));
            }
            finally {
                if (tempFilePath != null) {
                    System.IO.File.Delete(tempFilePath);
                    m_doc.Application.SharedParametersFilename = null;
                }
            }
        }

        internal void CanVaryBtwGroups(
            string parameterName,
            bool allowVaryBtwGrps) {
            BindingMap bindingMap = m_doc.ParameterBindings;
            DefinitionBindingMapIterator itr = bindingMap.ForwardIterator();

            while (itr.MoveNext()) {
                InternalDefinition internalDef = itr.Key as InternalDefinition;

                if (internalDef.Name == parameterName) {
                    using (Transaction t = new Transaction(m_doc)) {
                        t.Start("Allow varying b/w groups");
                        internalDef.SetAllowVaryBetweenGroups(m_doc, allowVaryBtwGrps);
                        t.Commit();
                    }
                }
            }
        }

        internal bool DoesParameterExist(string parameterName) {
            // iterate the procedure over 
            // all the shared parameters in the document
            BindingMap bindingMap = m_doc.ParameterBindings;
            DefinitionBindingMapIterator itr = bindingMap.ForwardIterator();
            while (itr.MoveNext()) {
                InternalDefinition internalDefinition
                    = itr.Key as InternalDefinition;
                if (internalDefinition.Name == parameterName)
                    return true;
            }
            return false;
        }

        #endregion
    }
}
