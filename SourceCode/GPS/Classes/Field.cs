using System;
using System.Collections.Generic;
using System.Text;

namespace ShpToIsoXml
{
    public class Field
    {
        public string Name { get; set; }
        public FieldBoundary Boundary { get; set; }
        public List<Track> Tracks { get; set; }
        
    }
}
