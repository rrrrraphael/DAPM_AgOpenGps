using System;
using System.Collections.Generic;
using System.Text;

namespace ShpToIsoXml
{
    public enum TrackType { AB, Curve };
    public class Track
    {
        public TrackType Type { get; set; }
        public List<Coord> Points { get; set; }
        public string Name { get; set; }

        public Track() {
            this.Points = new List<Coord>();
        }
    }
}
