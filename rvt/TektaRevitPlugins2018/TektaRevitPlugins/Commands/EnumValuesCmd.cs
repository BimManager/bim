using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using TektaRevitPlugins.Windows;

using Tree;

namespace TektaRevitPlugins.Commands
{
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class EnumValuesCmd : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Add an EventLogTraceListener object
            System.Diagnostics.Trace.Listeners.Add(
                new System.Diagnostics.EventLogTraceListener("Application"));

            // Lay the hands on the active document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Set up a timer
            Timing myTimer = new Timing();
            myTimer.StartTime();
            StringBuilder strBld = new StringBuilder();

            // Make some preparations
            List<ElementId> selectedElemIds = new List<ElementId>();
            EnumValuesWnd wnd = null;

            try
            {
                if (doc.ActiveView.ViewType != ViewType.Schedule)
                {
                    TaskDialog.Show("Exception", "Open a schedule view before running this command.");
                    return Result.Failed;
                }
                else if (uidoc.Selection.GetElementIds().Count == 0)
                {
                    TaskDialog.Show("Exception", "No element has been picked out.");
                    return Result.Failed;
                }

                selectedElemIds = uidoc.Selection.GetElementIds().ToList();
                ViewSchedule scheduleView = doc.ActiveView as ViewSchedule;
                ScheduleDefinition scheduleDefinition = scheduleView.Definition;
                bool sorted = scheduleDefinition.GetSortGroupFieldCount() > 0 ? true : false;
                List<TmpObject> tmpObjs = new List<TmpObject>();

                for (int i = 0; i < selectedElemIds.Count; ++i)
                {
                    Element elem = doc.GetElement(selectedElemIds[i]);
                    TmpObject tmp = new TmpObject(elem.Id);

                    for (int j = 0; j < scheduleDefinition.GetSortGroupFieldCount(); ++j)
                    {
                        ScheduleSortGroupField srtGrpField = scheduleDefinition.GetSortGroupField(j);
                        ScheduleField schField = scheduleDefinition.GetField(srtGrpField.FieldId);

                        Parameter p = GetScheduleFieldParameter(schField, elem);
                        if (p != null)
                        {
                            tmp.AddParameter(p);
                        }
                    }
                    tmpObjs.Add(tmp);
                }
                
                // Sort the elements by a series of parameters
                tmpObjs.Sort();

                // Create the window and subscribe to its event
                IList<string> parameterNames = doc.GetElement(selectedElemIds[0])
                                                  .GetOrderedParameters()
                                                  .Where(p => p.StorageType == StorageType.String && !p.IsReadOnly)
                                                  .Select(p => p.Definition.Name)
                                                  .ToList();

                wnd = new EnumValuesWnd(parameterNames);
                wnd.OkClicked += (sender, args) =>
                {
                    using (Transaction t = new Transaction(doc))
                    {
                        t.Start("Enumerate parameters");

                        // Deal with the first element
                        TmpObject prevObj = tmpObjs[0];
                        doc.GetElement(prevObj.Id).LookupParameter(args.ParameterName).Set(
                            string.Format("{0}{1}{2}", args.Prefix, args.Position, args.Suffix));
                        int position = args.Position;

                        // See about the other elements in the list
                        for (int i = 1; i < tmpObjs.Count; ++i)
                        {
                            TmpObject curObj = tmpObjs[i];
                            Element e = doc.GetElement(curObj.Id);

                            if (curObj.CompareTo(prevObj) == 0 && sorted)
                            {
                                e.LookupParameter(args.ParameterName).Set(
                                    string.Format("{0}{1}{2}", args.Prefix, position, args.Suffix));
                            }
                            else
                            {
                                ++position;
                                e.LookupParameter(args.ParameterName).Set(
                                    string.Format("{0}{1}{2}", args.Prefix, position, args.Suffix));
                            }
                            prevObj = curObj;
                        }
                        t.Commit();
                    }
                };

                // Show the window
                wnd.ShowDialog();

                return Result.Succeeded;

            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception",
                  string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                System.Diagnostics.Trace.Write(string.Format("{0}\n{1}",
                  ex.Message, ex.StackTrace));
                return Result.Failed;
            }
            finally
            {
                wnd.Close();
                myTimer.StopTime();
                System.Diagnostics.Trace
                    .Write(string.Format("Time elapsed: {0}s",
                  myTimer.Duration.TotalSeconds));
            }
        }

        dynamic GetValue(Parameter p)
        {
            switch (p.StorageType)
            {
                case StorageType.Double:
                    return p.AsDouble();

                case StorageType.ElementId:
                    return p.AsElementId();

                case StorageType.Integer:
                    return p.AsInteger();

                case StorageType.String:
                    return p.AsString();

                default:
                    return null;
            }
        }

        Parameter GetScheduleFieldParameter(ScheduleField scheduleField, Element elem)
        {
            Parameter parameter = null;

            switch (scheduleField.FieldType)
            {
                case ScheduleFieldType.Instance:
                    // If the parameter is not a shared parameter,
                    // then it is an instance parameter
                    if (scheduleField.ParameterId == null)
                    {
                        parameter = 
                            elem.LookupParameter(scheduleField.GetName());
                    }
                    // Conversely, it is a shared parameter
                    else
                    {
                        Document doc = elem.Document;
                        SharedParameterElement sharedParam =
                            doc.GetElement(scheduleField.ParameterId) 
                            as SharedParameterElement;

                        // All shared parameteres use this type, 
                        // be they instance or type parameters

                        // Try retrieving the parameter from an instance
                        parameter = elem.LookupParameter(sharedParam.Name);

                        // If the parameter is not present, 
                        // then resort to a family type
                        if (parameter == null)
                        {
                            parameter = ((FamilyInstance)elem)
                                .Symbol.LookupParameter(sharedParam.Name);
                        }
                    }
                    return parameter;

                case ScheduleFieldType.ElementType:
                    // A type parameter of the schedules elements
                    parameter = ((FamilyInstance)elem)
                        .Symbol.LookupParameter(scheduleField.GetName());
                    return parameter;

                default:
                    throw new NotImplementedException(
                        "Other filed types have not been implemented yet.");
            }
        }

        bool IsInstance(SharedParameterElement sharedParameter)
        {
            Document doc = sharedParameter.Document;
            BindingMap bindingMap = doc.ParameterBindings;
            Binding binding = bindingMap.get_Item(sharedParameter.GetDefinition());

            if (binding is InstanceBinding)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    class TmpObject : IComparable<TmpObject>
    {
        IList<Parameter> m_parameters;

        public ElementId Id { get; set; }
        public int ParameterCount
        {
            get { return m_parameters.Count; }
        }

        public TmpObject(ElementId id)
        {
            Id = id;
            m_parameters = new List<Parameter>();
        }

        public void AddParameter(Parameter p)
        {
            m_parameters.Add(p);
        }

        public Parameter GetParameter(int i)
        {
            if (i < m_parameters.Count)
            {
                return m_parameters[i];

            }
            else
            {
                return null;
            }
        }

        public int CompareTo(TmpObject other)
        {
            for (int i = 0; i < m_parameters.Count; ++i)
            {
                Parameter p1 = m_parameters[i];
                Parameter p2 = other.GetParameter(i);
                object val1 = null;
                object val2 = null;

                switch (p1.StorageType)
                {
                    case StorageType.Double:
                        val1 = p1.AsDouble();
                        val2 = p2.AsDouble();
                        if ((double)val1 > (double)val2)
                        {
                            return 1;
                        }
                        else if ((double)val1 < (double)val2)
                        {
                            return -1;
                        }
                        else
                        {
                            continue;
                        }

                    case StorageType.Integer:
                        val1 = p1.AsInteger();
                        val2 = p2.AsInteger();
                        if ((int)val1 > (int)val2)
                        {
                            return 1;
                        }
                        else if ((int)val1 < (int)val2)
                        {
                            return -1;
                        }
                        else
                        {
                            continue;
                        }

                    case StorageType.ElementId:
                        val1 = p1.AsElementId();
                        val2 = p2.AsElementId();
                        Document doc = p1.Element.Document;
                        Element e1 = doc.GetElement((ElementId)val1);
                        Element e2 = doc.GetElement((ElementId)val2);
                        if (e1 is Level)
                        {
                            Level lvl1 = e1 as Level;
                            Level lvl2 = e2 as Level;
                            if (lvl1.Elevation > lvl2.Elevation)
                            {
                                return 1;
                            }
                            else if (lvl1.Elevation < lvl2.Elevation)
                            {
                                return -1;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            throw new Exception("Cannot compare ElementIds.");
                        }

                    case StorageType.String:
                        val1 = p1.AsString();
                        val2 = p2.AsString();
                        int result = strcmp((string)val1, (string)val2);
                        if(result == 0)
                        {
                            continue;
                        }
                        else
                        {
                            return result;
                        }

                    default:
                        throw new Exception(string.Format("Unknown storage type. Parameter: {0}.", p1.Definition.Name));
                }
            }
            return 0;
        }

        int strcmp(string s1, string s2)
        {
            for (int i = 0, j = 0; i < s1.Length && j < s2.Length; ++i, ++j)
            {
                if (s1[i] > s2[i])
                {
                    return 1;
                }
                else if (s1[i] < s2[i])
                {
                    return -1;
                }
                else
                {
                    continue;
                }
            }
            return 0;
        }
    }
}

namespace Tree
{
    class Node<K, V>
        where V : ElementId
        where K : IComparable
    {
        List<Node<K, V>> m_children;
        List<V> m_ids;
        K m_value;

        public K Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public List<Node<K, V>> Children
        {
            get { return m_children; }
        }

        public Node()
        {
            m_children = new List<Node<K, V>>();
        }

        public Node(K value)
        {
            Value = value;
            m_children = new List<Node<K, V>>();
        }

        public void Add(Node<K, V> node)
        {
            m_children.Add(node);
        }

        public void Remove(Node<K, V> node)
        {
            if (m_children.Contains(node))
            {
                m_children.Remove(node);
            }
        }
    }

}


/*
 * switch (schField.FieldType)
                        {
                            case ScheduleFieldType.Instance:
                                if (schField.ParameterId == null)
                                {
                                    strBld.AppendFormat("Not Shared Parameter: {0}; ", 
                                        e.LookupParameter(schField.GetName()).Definition.Name);
                                    tmp.AddParameter(e.LookupParameter(schField.GetName()));
                                }
                                else
                                { 
                                    SharedParameterElement sharedParam = 
                                        doc.GetElement(schField.ParameterId) as SharedParameterElement;

                                    Parameter p = e.LookupParameter(sharedParam.Name);

                                    if (p == null)
                                    {
                                        p = ((FamilyInstance)e).Symbol.LookupParameter(sharedParam.Name);

                                        if (p == null)
                                        {
                                            continue;
                                        }
                                        
                                    }
                                    tmp.AddParameter(p);
                                }
                                break;
                                
                            case ScheduleFieldType.ElementType:
                                tmp.AddParameter(((FamilyInstance)e).Symbol.LookupParameter(schField.GetName()));
                                break;

                            default:
                                throw new NotImplementedException("Other filed types have not been implemented yet.");
                        }*/
