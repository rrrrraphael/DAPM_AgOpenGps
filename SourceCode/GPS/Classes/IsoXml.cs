using System;
using System.Collections.Generic;
using System.Text;

namespace IsoXml
{
    public class IsoXml
    {        
        public List<PFD> PFDList {get;set;}

        public IsoXml()
        {
            PFDList = new List<PFD>();
        }

        public override string ToString()
        {
            string ret = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine;
            ret += IsoXml.Spaces(1) + "<ISO11783_TaskData ManagementSoftwareManufacturer=\"RainsOfCastamere\" VersionMajor=\"3\" VersionMinor=\"0\" ManagementSoftwareVersion=\"99.9\" DataTransferOrigin=\"1\">" + Environment.NewLine;
            ret += IsoXml.Spaces(1) + "<CTR A=\"CTR1\" B=\"Riegler THOMAS\" />" + Environment.NewLine;
            ret += IsoXml.Spaces(1) + "<FRM A=\"FRM1\" B=\"Riegler THOMAS\" I=\"CTR1\" />" + Environment.NewLine;
            //ret += IsoXml.Spaces(1) + "<FRM A=\"FRM2\" B=\"Praxis FJ\" I=\"CTR1\" />" + Environment.NewLine;
            foreach (var pfd in PFDList)
            {
                ret += IsoXml.Spaces(1) + pfd.ToString();
            }
            ret += "</ISO11783_TaskData>" + Environment.NewLine;
            return ret;
        }

        public static string Spaces(int depth)
        {
            string ret = string.Empty;
            for(int i = 0; i < depth; i++)
            {
                ret += " ";
            }
            return ret;
        }
    }
}
