using System;
using System.Collections.Generic;
using System.Text;

namespace IsoXml
{
    public class PLN
    {
        public LSG LSG { get;set; }
        public int Depth { get; set; }

        public override string ToString()
        {
            string ret = IsoXml.Spaces(Depth) + "<PLN A=\"1\">" + Environment.NewLine;
            ret += LSG.ToString();
            ret += IsoXml.Spaces(Depth) + "</PLN>" + Environment.NewLine;
            return ret;
        }
    }
}
