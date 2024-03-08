using System;
using System.Collections.Generic;
using System.Text;

namespace IsoXml
{
    public class GPN { 
    
        public LSG LSG { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public enum GPNType { AB=1, Kurve=3}

        public GPNType Type { get; set; }
        public override string ToString()
        {
            string ret = IsoXml.Spaces(3) + string.Format("<GPN A=\"GPN{0}\" B=\"{1}\" C=\"{2}\" E=\"1\" F=\"1\" G=\"0\" I=\"0\">" + Environment.NewLine, Id, Name, (int)Type);
            ret += LSG.ToString();
            ret += IsoXml.Spaces(3) + "</GPN>" + Environment.NewLine;
            return ret;
        }
    }
}