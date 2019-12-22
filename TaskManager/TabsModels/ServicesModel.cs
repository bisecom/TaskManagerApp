using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.TabsModels
{
    public class ServicesModel
    {
        public string ServicesName { get; set; }
        public string ProcessId { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceState { get; set; }
        public string ServiceGroup { get; set; }

        public ServicesModel() {}
    }
}
