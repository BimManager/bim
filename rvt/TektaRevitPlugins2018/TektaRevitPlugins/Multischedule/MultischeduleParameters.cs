using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TektaRevitPlugins.Multischedule
{
    static class MultischeduleParameters
    {
        internal const string PARTITION = "REI.TXT.Раздел"; 
        internal const string HOST_MARK = "REI.TXT.МаркаКонструкции";
        internal const string MARK = "REI.TXT.МаркаЭлемента";
        internal const string ASSEMBLY_MARK = "REI.TXT.МаркаСборки";
        internal const string BLOCK_NUMBER = "#Блок";

        internal const string PROJECT_PARTITION = "INF.L.РазделПроекта"; // for drawing sets
        internal const string DRAWING_SET_NAME = "INF.L.НаименованиеТома";
        internal const string STRUCTURE_TYPE = "#Тип конструкции"; // for 

        internal const string SHOW_TITLE = "ShowTitle";
        internal const string SHOW_CONCRETE_QUANTITY = "ConcreteQuantity";
    }
}
