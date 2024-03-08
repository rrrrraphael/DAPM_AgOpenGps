using System;
using System.Collections.Generic;
using System.Text;

namespace IsoXml
{
    public class PFD
    {
        //ein Feld
        public PLN PLN { get; set; }
        public List<GGP> GGPList { get; set; }

        public string Name { get; set; }

        public int Depth { get; set; }
        public int Id { get; set; }

        public PFD()
        {
            GGPList = new List<GGP>();
        }

        public override string ToString()
        {
            string ret = IsoXml.Spaces(Depth) + string.Format("<PFD A=\"PFD{0}\" C=\"{1}\"  E=\"CTR1\" F=\"FRM1\">" + Environment.NewLine, Id, Name);
            ret += PLN.ToString();
            foreach (var ggp in GGPList)
            {
                ret += ggp.ToString();
            }           
            
            ret += IsoXml.Spaces(Depth) + "</PFD>" + Environment.NewLine;
            return ret;
        }
    }
}
