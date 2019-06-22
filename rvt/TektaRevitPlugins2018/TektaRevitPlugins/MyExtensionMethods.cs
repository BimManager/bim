using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DefGroup = Autodesk.Revit.DB.DefinitionGroup;
using DefGroups = Autodesk.Revit.DB.DefinitionGroups;

namespace TektaRevitPlugins
{
    static class MyExtensionMethods
    {
        internal static DefGroup FindDefinitionGroup(this DefGroups defGroups, string name)
        {
            return defGroups.Where(dg => dg.Name == name).FirstOrDefault();
        }
    }
}
