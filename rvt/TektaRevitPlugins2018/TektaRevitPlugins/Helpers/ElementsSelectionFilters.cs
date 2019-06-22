
using Autodesk.Revit.DB;

namespace TektaRevitPlugins
{
    class VerticalElementsSelectionFilter: Autodesk.Revit.UI.Selection.ISelectionFilter
    {
        public bool AllowElement(Autodesk.Revit.DB.Element elem)
        {
            if (elem is Autodesk.Revit.DB.Wall)
                return true;
            else if (elem.Category.Id.IntegerValue ==
                    (int)Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns)
                return true;
            else
                return false;
        }

        public bool AllowReference(Autodesk.Revit.DB.Reference reference,
            Autodesk.Revit.DB.XYZ position)
        {
            throw new System.NotImplementedException();
        }
    }

    class HorizontalElementsSelectionFilter : Autodesk.Revit.UI.Selection.ISelectionFilter
    {
        public bool AllowElement(Autodesk.Revit.DB.Element elem)
        {
            if (elem is Autodesk.Revit.DB.Floor)
                return true;
            else
                return false;
        }

        public bool AllowReference(Autodesk.Revit.DB.Reference reference, 
            Autodesk.Revit.DB.XYZ position)
        {
            throw new System.NotImplementedException();
        }
    }

    class RoomSelectionFilter : Autodesk.Revit.UI.Selection.ISelectionFilter
    {
        public bool AllowElement(Autodesk.Revit.DB.Element elem)
        {
            if (elem.Category.Id.IntegerValue ==
                (int)Autodesk.Revit.DB.BuiltInCategory.OST_Rooms)
                return true;
            else
                return false;
        }

        public bool AllowReference(Autodesk.Revit.DB.Reference reference, 
            Autodesk.Revit.DB.XYZ position)
        {
            return false;
        }
    }

    class TagSelectionFilter : Autodesk.Revit.UI.Selection.ISelectionFilter
    {
        public bool AllowElement(Element elem) {
            if (elem is IndependentTag)
                return true;
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }
}
