using System;
using System.Collections.Generic;
using System.Text;

namespace ShpToIsoXml
{
    public class FieldBoundary
    {
        public string Name { get; set; }
        public List<Coord> Points = new List<Coord>();
    }
}
