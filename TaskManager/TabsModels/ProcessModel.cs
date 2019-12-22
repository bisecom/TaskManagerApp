using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TaskManager.TabsModels
{
    public class ProcessModel
    {
        public Icon ProcessIcon_ { get; set; }
        public string ProcessName_ { get; set; }
        public string ProcessUsername_ { get; set; }
        public string ProcessLoad_ { get; set; }
        public string ProcessMemory_ { get; set; }
        public string ProcessDescription_ { get; set; }
        public string ProcessId { get; set; }

        public ProcessModel() { }
    }
}
