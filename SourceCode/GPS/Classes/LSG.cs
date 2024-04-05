using System;
using System.Collections.Generic;
using System.Text;

namespace IsoXml
{
    public class LSG
    {
        // Problem: we have 2 different LSGs???
        public enum Type { Boundary=1, Track=5};
        public Type LSGType { get; set; }

        public string Name { get; set; }

        public List<PNT> Points { get; set; }

        public int Depth { get; set; }


        public LSG()
        {
            Points = new List<PNT>();
        }
        public override string ToString()
        {
            string ret = string.Empty;
            ret = IsoXml.Spaces(Depth) + string.Format("<LSG A=\"{0}\">" + Environment.NewLine, (int)LSGType);
            foreach(var e in Points)
            {
                ret +=e.ToString() + Environment.NewLine;
            }            
            ret += IsoXml.Spaces(Depth) + "</LSG>" + Environment.NewLine;
            return ret;
        }
    }
}
