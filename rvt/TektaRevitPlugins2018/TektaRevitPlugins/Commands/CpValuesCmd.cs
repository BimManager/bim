using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Trace = System.Diagnostics.Trace;

using TektaRevitPlugins.Windows;

namespace TektaRevitPlugins
{
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class CpValuesCmd : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            // Add an EventLogTraceListener object
            System.Diagnostics.Trace.Listeners.Add(
                new System.Diagnostics.EventLogTraceListener("Application"));

            // Lay the hands on the active document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Set up a timer
            Timing myTimer = new Timing();
            myTimer.StartTime();

            CpValuesWnd wnd = null;

            try {
                List<Element> elemsCpFrom = new List<Element>();
                List<Element> elemsCpTo = new List<Element>();

                if (uidoc.Selection.GetElementIds().Count == 0)
                {
                    // prompt the user to get hold of an element
                    // from which parameter values are to be copied
                    Reference refCpFrom = uidoc.Selection
                        .PickObject(
                        Autodesk.Revit.UI.Selection.ObjectType.Element,
                        "Please pick out an element to copy values from.");

                    elemsCpFrom.Add(doc.GetElement(refCpFrom));

                    // get the user to select elements 
                    // to which the values will be copied if doable
                    IList<Reference> refsCpTo = uidoc.Selection
                        .PickObjects(
                        Autodesk.Revit.UI.Selection.ObjectType.Element,
                        "Please choose element(s) to copy values to.");

                    elemsCpTo.AddRange(refsCpTo.Select(r => doc.GetElement(r)));
                }
                else
                {
                    ICollection<ElementId> selectedElemIds = 
                        uidoc.Selection.GetElementIds();

                    elemsCpFrom.AddRange(selectedElemIds.Select(id => doc.GetElement(id)));
                    elemsCpTo.AddRange(selectedElemIds.Select(id => doc.GetElement(id)));
                }
                
                // Generate a dictionary of parameters to copy from
                SortedList<string, string> paramsValues;
                GetSortedParamsValues(elemsCpFrom[0], out paramsValues);

                // Produce a list of names of parameters to copy to
                if (elemsCpTo.Count != 0)
                {
                    List<string> paramNamesCopyTo;
                    GetParametersCopyTo(
                        elemsCpTo,
                        doc,
                        out paramNamesCopyTo);

                    wnd = new CpValuesWnd(paramsValues, paramNamesCopyTo);
                }
                else
                {
                    wnd = new CpValuesWnd(paramsValues, paramsValues.Keys.ToList());
                }

                wnd.OkClicked += (sender, args) => 
                {
                    using (Transaction t = new Transaction(doc, "Copy Parameters"))
                    {
                        Parameter pmCopyFrom = 
                        elemsCpFrom[0].LookupParameter(args.CopyFromParam);

                        Parameter pmCopyTo =
                        GetParameterFromParameterMap(elemsCpTo[0], args.CopyToParam);

                        if (pmCopyFrom.StorageType != pmCopyTo.StorageType)
                        {
                            TaskDialog.Show("Error", "The parameter types do not match.");
                            return;
                        }
                        else if (pmCopyTo.IsReadOnly)
                        {
                            TaskDialog.Show("Error", "The parameter is read-only.");
                            return;
                        }
                        else if(elemsCpFrom.Count == 1)
                        {
                            t.Start();
                            foreach(Element e in elemsCpTo)
                            {
                                Parameter pmCpTo = GetParameterFromParameterMap(
                                    e, args.CopyToParam);
                                CopyParameter(pmCopyFrom, pmCpTo);
                            }
                            t.Commit();
                        }
                        else
                        {
                            t.Start();

                            for(int i = 0; i < elemsCpFrom.Count; ++i)
                            {
                                CopyParameter(
                                    GetParameterFromParameterMap(elemsCpFrom[i], args.CopyFromParam),
                                    GetParameterFromParameterMap(elemsCpTo[i], args.CopyToParam));
                            }

                            t.Commit();
                        }

                    }
                };

                wnd.ShowDialog();

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
                return Result.Cancelled;
            }
            catch (Exception ex) {
                wnd.Close();    
                TaskDialog.Show("Exception",
                  string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                System.Diagnostics.Trace.Write(string.Format("{0}\n{1}",
                  ex.Message, ex.StackTrace));
                return Result.Failed;
            }
            finally {
                myTimer.StopTime();
                System.Diagnostics.Trace
                    .Write(string.Format("Time elapsed: {0}s",
                  myTimer.Duration.TotalSeconds));
            }
        }

        void GetSortedParamsValues(
            Element elem, out SortedList<string, string> paramsValues) {
            ParameterMap parameterMap = elem.ParametersMap;
            ParameterMapIterator itr = parameterMap.ForwardIterator();

            paramsValues = new SortedList<string, string>();

            while (itr.MoveNext()) {
                string paramName = itr.Key;
                Parameter parameter
                    = parameterMap.get_Item(paramName);

                paramsValues.Add(paramName,
                    GetParamValueAsStr(parameter));
            }
        }

        string GetParamValueAsStr(Parameter parameter) {
            switch (parameter.StorageType) {
                case StorageType.String:
                    return parameter.AsString();

                case StorageType.Integer:
                    return parameter.AsInteger().ToString();

                case StorageType.Double:
                    return parameter.AsValueString();

                case StorageType.ElementId:
                    ElementId eid = parameter.AsElementId();
                    if (eid.IntegerValue < 0)
                        return "(none)";
                    else
                        return parameter
                            .Element
                            .Document
                            .GetElement(parameter.AsElementId()).Name;

                case StorageType.None:
                    return "N/A";

                default:
                    return "N/A";
            }
        }

        void SetParameter(Parameter parameter, string value) {
            int tmpInt;
            double tmpDbl;
            switch (parameter.StorageType) {
                case StorageType.String:
                    parameter.Set(value);
                    break;

                case StorageType.Integer:
                    if (Int32.TryParse(value, out tmpInt)) {
                        parameter.Set(tmpInt);
                        break;
                    }
                    throw new Exception();

                case StorageType.Double:
                    if (Double.TryParse(value, out tmpDbl)) {
                        parameter.Set(tmpDbl);
                        break;
                    }
                    throw new Exception();

                case StorageType.ElementId:
                    throw new Exception();

                case StorageType.None:
                    throw new Exception();

                default:
                    throw new Exception();
            }
        }

        void CopyParameter(Parameter cpFrom, Parameter cpTo) {
            switch (cpFrom.StorageType) {
                case StorageType.String:
                    cpTo.Set(cpFrom.AsString());
                    break;

                case StorageType.Integer:
                    cpTo.Set(cpFrom.AsInteger());
                    break;

                case StorageType.Double:
                    cpTo.Set(cpFrom.AsDouble());
                    break;

                case StorageType.ElementId:
                    cpTo.Set(cpFrom.AsInteger());
                    break;

                case StorageType.None:
                    throw new Exception();

                default:
                    throw new Exception();
            }
        }

        void GetParamNames(Element elem, out List<string> parameterNames) {
            ParameterMap parameterMap = elem.ParametersMap;
            ParameterMapIterator itr = parameterMap.ForwardIterator();
            parameterNames = new List<string>();
            while (itr.MoveNext()) {
                parameterNames.Add(itr.Key);
            }
            parameterNames.Sort();
        }

        void GetParametersCopyTo(
            IList<Element> elems,
            Document doc,
            out List<string> outParams) {
            
            GetParamNames(elems[0], out outParams);

            for (int i = 1; i < elems.Count; ++i) {
                List<string> elemParams;
                GetParamNames(elems[i], out elemParams);

                List<string> tmp = new List<string>();

                foreach (string param in elemParams)
                    if (outParams.Contains(param) &&
                        !elems[i].LookupParameter(param).IsReadOnly)
                        tmp.Add(param);

                outParams.Clear();
                outParams.InsertRange(0, tmp);
            }

            outParams.Sort();
        }

        void ConvertParameterMapToDictionary(
            Element elem, out SortedDictionary<string, Parameter> outDictionary) {
            ParameterMap parameterMap = elem.ParametersMap;
            ParameterMapIterator itr = parameterMap.ForwardIterator();

            outDictionary = new SortedDictionary<string, Parameter>();

            while (itr.MoveNext()) {
                string paramName = itr.Key;
                Parameter parameter
                    = parameterMap.get_Item(paramName);
                outDictionary.Add(paramName, parameter);
            }
        }

        Parameter GetParameterFromParameterMap(
            Element elem, string paramName) {
            ParameterMap parameterMap = elem.ParametersMap;
            ParameterMapIterator itr = parameterMap.ForwardIterator();

            while (itr.MoveNext())
                if (paramName == itr.Key)
                    return parameterMap.get_Item(paramName);

            return null;
        }
    }
}
