using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace TektaRevitPlugins.DataStorageMgr
{
    class DataStorageMgr
    {
        #region Properties
        internal Document Document { get; private set; }
        #endregion

        #region Constructors
        internal DataStorageMgr(Document doc)
        {
            Document = doc;
        }
        #endregion

        #region Methods
        IList<Schema> EnumerateAllSchemas()
        {
            return Schema.ListSchemas();
        }

        #endregion

        #region Events

        #endregion

        #region Helper Methods


        #endregion
    }
}
