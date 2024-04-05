using System;
using System.Collections.Generic;
using System.Text;

namespace IsoXml
{
    public class GGP
    {
        public GPN GPN { get; set; }
        public string Name { get; set; }

        public int Id { get; set; }

        public override string ToString()
        {
            string ret = IsoXml.Spaces(2) + string.Format("<GGP A=\"GGP{0}\" B=\"{0}\">" + Environment.NewLine, Id, Name);
            ret += GPN.ToString();
            ret += IsoXml.Spaces(2) + "</GGP>" + Environment.NewLine;
            return ret;
        }
    }
}
