using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Document = Autodesk.Revit.DB.Document;

namespace TektaRevitPlugins.Multischedule
{
    internal enum MultischeduleType : byte
    {
        StructureSchedule,
        AssemblySchedule,
        BarBendingByStructure,
        BarBendingByAssembly,
        RebarQuantityTakeoff,
        ScheduleOfWork,
        PartitionDrawingSets
    }

    static class MultitableScheduleFactory
    {
        #region Methods
        internal static IMultitableSchedule 
            CreateMultiTableSchedule(MultischeduleType type, 
            IDictionary<string, string> parametersValues)
        {
            switch(type)
            {
                case MultischeduleType.StructureSchedule:
                    return new StructureSchedule(parametersValues);

                case MultischeduleType.AssemblySchedule:
                    return new AssemblySchedule(parametersValues);

                case MultischeduleType.BarBendingByStructure:
                    return new BarBendingByStructure(parametersValues);

                case MultischeduleType.BarBendingByAssembly:
                    return new BarBendingByAssembly(parametersValues);

                case MultischeduleType.RebarQuantityTakeoff:
                    return new RebarQuantityTakeoff(parametersValues);

                case MultischeduleType.ScheduleOfWork:
                    return new ScheduleOfWork(parametersValues);

                case MultischeduleType.PartitionDrawingSets:
                    return new PartitionDrawingSets(parametersValues);

                default:
                    throw new NotImplementedException(
                        "This type of schedule has not been implemented yet. My humble apologies.");
            }
        }
        #endregion
    }
}
