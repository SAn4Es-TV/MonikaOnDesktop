using System;
using System.Collections.Generic;
using System.Text;

namespace MonikaOnDesktop
{
    class ProcInfo
    {
        public string proc_name { get; set; }
        public List<string> phrases { get; set; }
        public ProcInfo(string procName, List<string> Phrases)
        {
            proc_name = procName;
            phrases = Phrases;
        }
    }
}
