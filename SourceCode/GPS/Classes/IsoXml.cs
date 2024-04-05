using OpenTK.Graphics.ES10;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsoXml
{
    public class IsoXml
    {        
        public List<PFD> PFDList {get;set;}
        private string farmerName;
        private string farmName;


        public IsoXml(string farmerName, string farmName)
        {
            this.farmName = farmName;
            this.farmerName = farmerName;
            PFDList = new List<PFD>();
        }

        public override string ToString()
        {
            string ret = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + Environment.NewLine;
            ret += IsoXml.Spaces(1) + "<ISO11783_TaskData ManagementSoftwareManufacturer=\"AgOpenGPS\" VersionMajor=\"3\" VersionMinor=\"0\" ManagementSoftwareVersion=\"5.7.2\" DataTransferOrigin=\"1\">" + Environment.NewLine;
            ret += IsoXml.Spaces(1) + $"<CTR A=\"CTR1\" B=\"{farmerName}\" />" + Environment.NewLine;
            ret += IsoXml.Spaces(1) + $"<FRM A=\"FRM1\" B=\"{farmName}\" I=\"CTR1\" />" + Environment.NewLine;
            foreach (var pfd in PFDList)
            {
                ret += IsoXml.Spaces(1) + pfd.ToString();
            }
            ret += "<TSK A=\"TSK1\" B=\"AgOpenDummyFieldTask\" C=\"CTR1\" D=\"FRM1\" E=\"PFD1\" G=\"1\" H=\"1\" I=\"1\" J=\"0\"> </TSK>" +Environment.NewLine;
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
