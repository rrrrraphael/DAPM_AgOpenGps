using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri.Shapefiles.Writers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.IO;
using NetTopologySuite.IO.Esri;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace AgOpenGPS
{
    public partial class FormFilePickerForExport : Form
    {
        private readonly FormGPS mf = null;

        private int order;

        private readonly List<string> fileList = new List<string>();

        public FormFilePickerForExport(Form callingForm)
        {
            //get copy of the calling main form
            mf = callingForm as FormGPS;

            InitializeComponent();
            btnByDistance.Text = gStr.gsSort;
            btnExportLv.Text = gStr.gsExport;
        }
        private void FormFilePicker_Load(object sender, EventArgs e)
        {
            order = 0;
            timer1.Enabled = true;
            ListViewItem itm;

            string[] dirs = Directory.GetDirectories(mf.fieldsDirectory);

            cbChooseFiletype.SelectedIndex = 0;

            //fileList?.Clear();

            if (dirs == null || dirs.Length < 1)
            {
                mf.TimedMessageBox(2000, gStr.gsCreateNewField, gStr.gsFileError);
                Close();
                return;
            }

            foreach (string dir in dirs)
            {
                double latStart = 0;
                double lonStart = 0;
                double distance = 0;
                string fieldDirectory = Path.GetFileName(dir);
                string filename = dir + "\\Field.txt";
                string line;

                //make sure directory has a field.txt in it
                if (File.Exists(filename))
                {
                    using (StreamReader reader = new StreamReader(filename))
                    {
                        try
                        {
                            //Date time line
                            for (int i = 0; i < 8; i++)
                            {
                                line = reader.ReadLine();
                            }

                            //start positions
                            if (!reader.EndOfStream)
                            {
                                line = reader.ReadLine();
                                string[] offs = line.Split(',');

                                latStart = (double.Parse(offs[0], CultureInfo.InvariantCulture));
                                lonStart = (double.Parse(offs[1], CultureInfo.InvariantCulture));


                                distance = Math.Pow((latStart - mf.pn.latitude), 2) + Math.Pow((lonStart - mf.pn.longitude), 2);
                                distance = Math.Sqrt(distance);
                                distance *= 100;

                                fileList.Add(fieldDirectory);
                                fileList.Add(Math.Round(distance, 3).ToString().PadLeft(10));
                            }
                            else
                            {
                                MessageBox.Show(fieldDirectory + " is Damaged, Please Delete This Field", gStr.gsFileError,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                                fileList.Add(fieldDirectory);
                                fileList.Add("Error");
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(fieldDirectory + " is Damaged, Please Delete, Field.txt is Broken", gStr.gsFileError,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                            fileList.Add(fieldDirectory);
                            fileList.Add("Error");

                        }
                    }
                }
                else continue;

                //grab the boundary area
                filename = dir + "\\Boundary.txt";
                if (File.Exists(filename))
                {
                    List<vec3> pointList = new List<vec3>();
                    double area = 0;

                    using (StreamReader reader = new StreamReader(filename))
                    {
                        try
                        {
                            //read header
                            line = reader.ReadLine();//Boundary

                            if (!reader.EndOfStream)
                            {
                                //True or False OR points from older boundary files
                                line = reader.ReadLine();

                                //Check for older boundary files, then above line string is num of points
                                if (line == "True" || line == "False")
                                {
                                    line = reader.ReadLine(); //number of points
                                }

                                //Check for latest boundary files, then above line string is num of points
                                if (line == "True" || line == "False")
                                {
                                    line = reader.ReadLine(); //number of points
                                }

                                int numPoints = int.Parse(line);

                                if (numPoints > 0)
                                {
                                    //load the line
                                    for (int i = 0; i < numPoints; i++)
                                    {
                                        line = reader.ReadLine();
                                        string[] words = line.Split(',');
                                        vec3 vecPt = new vec3(
                                        double.Parse(words[0], CultureInfo.InvariantCulture),
                                        double.Parse(words[1], CultureInfo.InvariantCulture),
                                        double.Parse(words[2], CultureInfo.InvariantCulture));

                                        pointList.Add(vecPt);
                                    }

                                    int ptCount = pointList.Count;
                                    if (ptCount > 5)
                                    {
                                        area = 0;         // Accumulates area in the loop
                                        int j = ptCount - 1;  // The last vertex is the 'previous' one to the first

                                        for (int i = 0; i < ptCount; j = i++)
                                        {
                                            area += (pointList[j].easting + pointList[i].easting) * (pointList[j].northing - pointList[i].northing);
                                        }
                                        if (mf.isMetric)
                                        {
                                            area = (Math.Abs(area / 2)) * 0.0001;
                                        }
                                        else
                                        {
                                            area = (Math.Abs(area / 2)) * 0.00024711;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            area = 0;
                        }
                    }
                    if (area == 0) fileList.Add("No Bndry");
                    else fileList.Add(Math.Round(area, 1).ToString().PadLeft(10));
                }

                else
                {
                    fileList.Add("Error");
                    MessageBox.Show(fieldDirectory + " is Damaged, Missing Boundary.Txt " +
                        "               \r\n Delete Field or Fix ", gStr.gsFileError,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                filename = dir + "\\Field.txt";
            }

            if (fileList == null || fileList.Count < 1)
            {
                mf.TimedMessageBox(2000, gStr.gsNoFieldsFound, gStr.gsCreateNewField);
                Close();
                return;
            }
            for (int i = 0; i < fileList.Count; i += 3)
            {
                string[] fieldNames = { fileList[i], fileList[i + 1], fileList[i + 2] };
                itm = new ListViewItem(fieldNames);
                lvLines.Items.Add(itm);
            }

            //string fieldName = Path.GetDirectoryName(dir).ToString(CultureInfo.InvariantCulture);

            if (lvLines.Items.Count > 0)
            {
                this.chName.Text = "Field Name";
                this.chName.Width = 680;

                this.chDistance.Text = "Distance";
                this.chDistance.Width = 140;

                this.chArea.Text = "Area";
                this.chArea.Width = 140;
            }
            else
            {
                mf.TimedMessageBox(2000, gStr.gsNoFieldsFound, gStr.gsCreateNewField);
                Close();
                return;
            }
        }

        private void btnByDistance_Click(object sender, EventArgs e)
        {
            ListViewItem itm;

            lvLines.Items.Clear();
            order += 1;
            if (order == 3) order = 0;


            for (int i = 0; i < fileList.Count; i += 3)
            {
                if (order == 0)
                {
                    string[] fieldNames = { fileList[i], fileList[i + 1], fileList[i + 2] };
                    itm = new ListViewItem(fieldNames);
                }
                else if (order == 1)
                {
                    string[] fieldNames = { fileList[i + 1], fileList[i], fileList[i + 2] };
                    itm = new ListViewItem(fieldNames);
                }
                else
                {
                    string[] fieldNames = { fileList[i + 2], fileList[i], fileList[i + 1] };
                    itm = new ListViewItem(fieldNames);
                }

                lvLines.Items.Add(itm);
            }

            if (lvLines.Items.Count > 0)
            {
                if (order == 0)
                {
                    this.chName.Text = "Field Name";
                    this.chName.Width = 680;

                    this.chDistance.Text = "Distance";
                    this.chDistance.Width = 140;

                    this.chArea.Text = "Area";
                    this.chArea.Width = 140;
                }
                else if (order == 1)
                {
                    this.chName.Text = "Distance";
                    this.chName.Width = 140;

                    this.chDistance.Text = "Field Name";
                    this.chDistance.Width = 680;

                    this.chArea.Text = "Area";
                    this.chArea.Width = 140;
                }

                else
                {
                    this.chName.Text = "Area";
                    this.chName.Width = 140;

                    this.chDistance.Text = "Field Name";
                    this.chDistance.Width = 680;

                    this.chArea.Text = "Distance";
                    this.chArea.Width = 140;
                }
            }
        }

        private void btnOpenExistingLv_Click(object sender, EventArgs e)
        {

        }

        private void btnDeleteAB_Click(object sender, EventArgs e)
        {
            mf.filePickerFileAndDirectory = "";
        }

        private void btnDeleteField_Click(object sender, EventArgs e)
        {
            int count = lvLines.SelectedItems.Count;
            string dir2Delete;
            if (count > 0)
            {
                if (order == 0) dir2Delete = (mf.fieldsDirectory + lvLines.SelectedItems[0].SubItems[0].Text);
                else dir2Delete = (mf.fieldsDirectory + lvLines.SelectedItems[0].SubItems[1].Text);

                DialogResult result3 = MessageBox.Show(
                    dir2Delete,
                    gStr.gsDeleteForSure,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result3 == DialogResult.Yes)
                {
                    System.IO.Directory.Delete(dir2Delete, true);
                }
                else return;
            }
            else return;

            ListViewItem itm;

            string[] dirs = Directory.GetDirectories(mf.fieldsDirectory);

            fileList?.Clear();

            foreach (string dir in dirs)
            {
                double latStart = 0;
                double lonStart = 0;
                double distance = 0;
                string fieldDirectory = Path.GetFileName(dir);
                string filename = dir + "\\Field.txt";
                string line;

                //make sure directory has a field.txt in it
                if (File.Exists(filename))
                {
                    using (StreamReader reader = new StreamReader(filename))
                    {
                        try
                        {
                            //Date time line
                            for (int i = 0; i < 8; i++)
                            {
                                line = reader.ReadLine();
                            }

                            //start positions
                            if (!reader.EndOfStream)
                            {
                                line = reader.ReadLine();
                                string[] offs = line.Split(',');

                                latStart = (double.Parse(offs[0], CultureInfo.InvariantCulture));
                                lonStart = (double.Parse(offs[1], CultureInfo.InvariantCulture));


                                distance = Math.Pow((latStart - mf.pn.latitude), 2) + Math.Pow((lonStart - mf.pn.longitude), 2);
                                distance = Math.Sqrt(distance);
                                distance *= 100;

                                fileList.Add(fieldDirectory);
                                fileList.Add(Math.Round(distance, 3).ToString().PadLeft(10));
                            }
                            else
                            {
                                MessageBox.Show(fieldDirectory + " is Damaged, Please Delete This Field", gStr.gsFileError,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                                fileList.Add(fieldDirectory);
                                fileList.Add("Error");
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(fieldDirectory + " is Damaged, Please Delete, Field.txt is Broken", gStr.gsFileError,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);

                            fileList.Add(fieldDirectory);
                            fileList.Add("Error");

                        }
                    }

                    //grab the boundary area
                    filename = dir + "\\Boundary.txt";
                    if (File.Exists(filename))
                    {
                        List<vec3> pointList = new List<vec3>();
                        double area = 0;

                        using (StreamReader reader = new StreamReader(filename))
                        {
                            try
                            {
                                //read header
                                line = reader.ReadLine();//Boundary

                                if (!reader.EndOfStream)
                                {
                                    //True or False OR points from older boundary files
                                    line = reader.ReadLine();

                                    //Check for older boundary files, then above line string is num of points
                                    if (line == "True" || line == "False")
                                    {
                                        line = reader.ReadLine(); //number of points
                                    }

                                    //Check for latest boundary files, then above line string is num of points
                                    if (line == "True" || line == "False")
                                    {
                                        line = reader.ReadLine(); //number of points
                                    }

                                    int numPoints = int.Parse(line);

                                    if (numPoints > 0)
                                    {
                                        //load the line
                                        for (int i = 0; i < numPoints; i++)
                                        {
                                            line = reader.ReadLine();
                                            string[] words = line.Split(',');
                                            vec3 vecPt = new vec3(
                                            double.Parse(words[0], CultureInfo.InvariantCulture),
                                            double.Parse(words[1], CultureInfo.InvariantCulture),
                                            double.Parse(words[2], CultureInfo.InvariantCulture));

                                            pointList.Add(vecPt);
                                        }

                                        int ptCount = pointList.Count;
                                        if (ptCount > 5)
                                        {
                                            area = 0;         // Accumulates area in the loop
                                            int j = ptCount - 1;  // The last vertex is the 'previous' one to the first

                                            for (int i = 0; i < ptCount; j = i++)
                                            {
                                                area += (pointList[j].easting + pointList[i].easting) * (pointList[j].northing - pointList[i].northing);
                                            }
                                            if (mf.isMetric)
                                            {
                                                area = (Math.Abs(area / 2)) * 0.0001;
                                            }
                                            else
                                            {
                                                area = (Math.Abs(area / 2)) * 0.00024711;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                area = 0;
                            }
                        }
                        if (area == 0) fileList.Add("No Bndry");
                        else fileList.Add(Math.Round(area, 1).ToString().PadLeft(10));
                    }

                    else
                    {
                        fileList.Add("Error");
                        MessageBox.Show(fieldDirectory + " is Damaged, Missing Boundary.Txt " +
                            "               \r\n Delete Field or Fix ", gStr.gsFileError,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            lvLines.Items.Clear();

            for (int i = 0; i < fileList.Count; i += 3)
            {
                string[] fieldNames = { fileList[i], fileList[i + 1], fileList[i + 2] };
                itm = new ListViewItem(fieldNames);
                lvLines.Items.Add(itm);
            }

            //string fieldName = Path.GetDirectoryName(dir).ToString(CultureInfo.InvariantCulture);

            if (lvLines.Items.Count > 0)
            {
                this.chName.Text = "Field Name";
                this.chName.Width = 680;

                this.chDistance.Text = "Distance";
                this.chDistance.Width = 140;

                this.chArea.Text = "Area";
                this.chArea.Width = 140;
            }
            else
            {
                //var form2 = new FormTimedMessage(2000, gStr.gsNoFieldsCreated, gStr.gsCreateNewFieldFirst);
                //form2.Show(this);
            }
        }

        private void btnExportLv_Click(object sender, EventArgs e)
        {
            if (lvLines.SelectedIndices.Count != 0)
            {
                string pathToField = Path.Combine(Environment.GetFolderPath(@Environment.SpecialFolder.MyDocuments), "AgOpenGPS", "Fields", lvLines.SelectedItems[0].Text, "Field.kml");
                if (cbChooseFiletype.SelectedItem.ToString() == "KML")
                {
                    
                    if (File.Exists(pathToField))
                    {
                        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                        {
                            File.Copy(pathToField, folderBrowserDialog1.SelectedPath + "\\" + lvLines.SelectedItems[0].Text + ".kml");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Feld existiert nicht");
                    }
                }
                else if (cbChooseFiletype.SelectedItem.ToString() == "Shapefile")
                {
                    // Shapefile
                    if (File.Exists(pathToField))
                    {
                        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                        {
                            ExportShapefile(pathToField, folderBrowserDialog1.SelectedPath + "\\" + lvLines.SelectedItems[0].Text + ".shp");
                        }
                    }
                }
                else if (cbChooseFiletype.SelectedItem.ToString() == "Geopackage")
                {
                    // Geopackage
                }
                else if (cbChooseFiletype.SelectedItem.ToString() == "GeoJSON")
                {
                    // GeoJSON
                    if (File.Exists(pathToField))
                    {
                        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                        {
                            Export_GeoJson(pathToField, folderBrowserDialog1.SelectedPath + "\\" + lvLines.SelectedItems[0].Text + ".geojson");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Kein Feld ausgewählt");
            }
        }
            


        private string[] ReadExistingKML(string kmlPath)
        {
            string[] coordinates;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(kmlPath))
            {
                bool alreadyOneField = false;

                string lineOfCoordinates = null;
                int startIndex;
                while (!reader.EndOfStream)
                {
                    //start to read the file
                    string line = reader.ReadLine();

                    startIndex = line.IndexOf("<coordinates>");

                    if (startIndex != -1)
                    {
                        if (alreadyOneField)
                        {
                            mf.TimedMessageBox(4000, gStr.gsTooManyFields, gStr.gsFirstOneIsUsed);
                            break;
                        }
                        alreadyOneField = true;
                        while (true)
                        {
                            int endIndex = line.IndexOf("</coordinates>");

                            if (endIndex == -1)
                            {
                                //just add the line
                                if (startIndex == -1) lineOfCoordinates += line.Substring(0);
                                else lineOfCoordinates += line.Substring(startIndex + 13);
                            }
                            else
                            {
                                if (startIndex == -1) lineOfCoordinates += line.Substring(0, endIndex);
                                else lineOfCoordinates += line.Substring(startIndex + 13, endIndex - (startIndex + 13));
                                break;
                            }
                            line = reader.ReadLine();
                            line = line.Trim();
                            startIndex = -1;
                        }

                        char[] delimiterChars = { ' ', '\t', '\r', '\n' };
                        coordinates = lineOfCoordinates.Split(delimiterChars);
                        return coordinates;

                    }

                }
                return null;
            }
        }

        private void ExportShapefile(string kmlPath, string shpPath)
        {

            string[] coordinates = this.ReadExistingKML(kmlPath);

            
            for (int i = 0; i < coordinates.Length; i++)
            {
                coordinates[i] = coordinates[i].Replace(",0", "");
                coordinates[i] = coordinates[i].Replace(',', ' ');
                coordinates[i] = coordinates[i] + ",";
            }
            coordinates[coordinates.Length - 1] = coordinates[coordinates.Length - 1].Replace(",", "");
            coordinates[coordinates.Length - 2] = coordinates[coordinates.Length - 2].Replace(",", "");

            string coordinateString = String.Join(" ", coordinates);



            string wkt = $"POLYGON((" + coordinateString + "))";

            var features = new List<Feature>();
            var wktReader = new WKTReader();

            var geometry2 = wktReader.Read(wkt);

            var attributes = new AttributesTable
            {
                { "Date", new DateTime(2022, 1, 1) },
                { "Content", $"I am No. 1" }
            };

            var feature = new Feature(geometry2, attributes);
            features.Add(feature);

            Shapefile.WriteAllFeatures(features, shpPath);



        }

        private void Export_GeoJson(string kmlPath, string geojsonPath) 
        {
            var feature = new JObject();
            feature["type"] = "Feature";
            feature["geometry"] = new JObject();
            feature["geometry"]["type"] = "Polygon";
            feature["geometry"]["coordinates"] = new JArray();

            var crs = new JObject();
            var featureCollection = new JObject();
            crs["type"] = "name";
            var properties = new JObject();
            properties["name"] = "urn:ogc:def:crs:OGC:1.3:CRS84";
            crs["properties"] = properties;
            featureCollection["crs"] = crs;

            var coordinatesArray = new JArray();
            string[] coordinates = this.ReadExistingKML(kmlPath);

            foreach (var coordinate in coordinates)
            {
                if (coordinate != string.Empty)
                {
                    string temp = coordinate.Substring(0, coordinate.Length - 2); ;
                    var coords = temp.Split(',');
                    var longitude = double.Parse(coords[0], CultureInfo.InvariantCulture);
                    var latitude = double.Parse(coords[1], CultureInfo.InvariantCulture);
                    coordinatesArray.Add(new JArray(longitude, latitude));
                }
            }
            // Add the array of coordinates to the Polygon feature
            ((JArray)feature["geometry"]["coordinates"]).Add(coordinatesArray);

            // Add properties if needed
            feature["properties"] = new JObject();

            // Create a FeatureCollection object to hold the Polygon feature
            featureCollection["type"] = "FeatureCollection";
            featureCollection["features"] = new JArray(feature);

            // Save GeoJSON to a file
            File.WriteAllText(geojsonPath, JsonConvert.SerializeObject(featureCollection, Formatting.Indented));
        }

    }
}
