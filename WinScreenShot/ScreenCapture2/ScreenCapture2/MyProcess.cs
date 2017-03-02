using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ScreenCapture2
{
    public class MyProcess
    {
        public string Name { private set; get; }
        public Process Process { private set; get; }
        public MyProcess(string name, Process process)
        {
            Name = name;
            Process = process;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
