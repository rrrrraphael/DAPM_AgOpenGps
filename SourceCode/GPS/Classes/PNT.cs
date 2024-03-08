using System;
using System.Collections.Generic;
using System.Text;

namespace IsoXml
{
    public class PNT
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }

        public enum PntType { Boundary = 10, FirstTrack = 6, Track=9, LastTrack=7 }

        public PntType Type { get; set; }

        public int Depth { get; set; }
        public PNT(decimal x, decimal y, PntType type)
        {
            this.X = x;
            this.Y = y;
            this.Type = type;
        }

        public override string ToString()
        {
            
            return string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"), IsoXml.Spaces(Depth) + "<PNT A=\"{0}\" C=\"{2}\" D=\"{1}\"/>", (int)Type, X, Y);
        }
    }
}
