using NetTopologySuite.Features;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Esri;
using System.Linq;
using NetTopologySuite.Geometries;
using System.Data.SQLite;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShpToIsoXml;
using IsoXml;
using NetTopologySuite.Operation;
using System.IO.Compression;

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
                    // GeoPackage
                    if (File.Exists(pathToField))
                    {
                        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                        {
                            ExportGeoPackage(pathToField, folderBrowserDialog1.SelectedPath + "\\" + lvLines.SelectedItems[0].Text + ".gpkg");
                        }
                    }
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
                else if(cbChooseFiletype.SelectedItem.ToString() == "ISOXML") {
                    // IsoXML

                    string farmer = this.txtFarmer.Text;
                    string farm = this.txtFarm.Text;
                    if (File.Exists(pathToField))
                    {
                        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                        {
                            Export_IsoXml(pathToField, folderBrowserDialog1.SelectedPath + "\\TASKDATA", farm, farmer);
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

        private void ExportGeoPackage(string kmlPath, string gpkgPath)
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

            coordinates = coordinates.Take(coordinates.Count() - 1).ToArray();

            List<double> xValuesList = new List<double>();
            List<double> yValuesList = new List<double>();
            string[] pointArr;

            

            foreach (string point in coordinates)
            {
                pointArr = point.Replace(',', ' ').Split(' ');
                xValuesList.Add(double.Parse(pointArr[0]));
                yValuesList.Add(double.Parse(pointArr[1]));
            }

            double[] xValues = xValuesList.ToArray();
            double[] yValues = yValuesList.ToArray();

            double xMax = xValues.Max();
            double xMin = xValues.Min();

            double yMax = yValues.Max();
            double yMin = yValues.Min();

            string tablename = "fields";

            List<Coordinate> cordList = new List<Coordinate>();
            for (int i = 0; i < xValues.Length; i++)
            {
                Coordinate cord = new Coordinate(xValues[i], yValues[i]);
                cordList.Add(cord);
            }

            Coordinate[] cordArr = cordList.ToArray();
            GeometryFactory geomFac = new GeometryFactory();
            Geometry geo = geomFac.CreatePolygon(cordArr);

            WKBWriter writer = new WKBWriter();

            byte[] header = { 71, 80, 0, 3, 230, 16, 0, 0, 136, 85, 15, 221, 159, 62, 46, 64, 191, 70, 63, 104, 1, 64, 46, 64, 11, 136, 47, 123, 112, 16, 72, 64, 62, 68, 255, 202, 185, 16, 72, 64 };

            byte[] rawData = writer.Write(geo);
            rawData = header.Concat(rawData).ToArray();

            if (File.Exists(gpkgPath))
            {
                File.Delete(gpkgPath);
            }

            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={gpkgPath};Version=3;"))
            {
                connection.Open();
                connection.Close();
            }


            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={gpkgPath};Version=3;"))
            {
                connection.Open();

                //connection.LoadExtension("mod_spatialite");

                List<string> queries = new List<string>();
                // SQL-Befehl zum Erstellen von Tabelle
                queries.Add(@"CREATE TABLE " + tablename + @" ( ""fid"" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, ""geometry"" POLYGON)");
                queries.Add(@"CREATE TABLE gpkg_contents (table_name TEXT NOT NULL PRIMARY KEY,data_type TEXT NOT NULL,identifier TEXT UNIQUE,description TEXT DEFAULT '',last_change DATETIME NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),min_x DOUBLE, min_y DOUBLE,max_x DOUBLE, max_y DOUBLE,srs_id INTEGER,CONSTRAINT fk_gc_r_srs_id FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys(srs_id))");
                queries.Add(@"CREATE TABLE gpkg_extensions (table_name TEXT,column_name TEXT,extension_name TEXT NOT NULL,definition TEXT NOT NULL,scope TEXT NOT NULL,CONSTRAINT ge_tce UNIQUE (table_name, column_name, extension_name))");
                queries.Add(@"CREATE TABLE gpkg_geometry_columns (table_name TEXT NOT NULL,column_name TEXT NOT NULL,geometry_type_name TEXT NOT NULL,srs_id INTEGER NOT NULL,z TINYINT NOT NULL,m TINYINT NOT NULL,CONSTRAINT pk_geom_cols PRIMARY KEY (table_name, column_name),CONSTRAINT uk_gc_table_name UNIQUE (table_name),CONSTRAINT fk_gc_tn FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name),CONSTRAINT fk_gc_srs FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys (srs_id))");
                queries.Add(@"CREATE TABLE gpkg_ogr_contents(table_name TEXT NOT NULL PRIMARY KEY,feature_count INTEGER DEFAULT NULL)");
                queries.Add(@"CREATE TABLE gpkg_spatial_ref_sys (srs_name TEXT NOT NULL,srs_id INTEGER NOT NULL PRIMARY KEY,organization TEXT NOT NULL,organization_coordsys_id INTEGER NOT NULL,definition  TEXT NOT NULL,description TEXT)");
                queries.Add(@"CREATE TABLE gpkg_tile_matrix (table_name TEXT NOT NULL,zoom_level INTEGER NOT NULL,matrix_width INTEGER NOT NULL,matrix_height INTEGER NOT NULL,tile_width INTEGER NOT NULL,tile_height INTEGER NOT NULL,pixel_x_size DOUBLE NOT NULL,pixel_y_size DOUBLE NOT NULL,CONSTRAINT pk_ttm PRIMARY KEY (table_name, zoom_level),CONSTRAINT fk_tmm_table_name FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name))");
                queries.Add(@"CREATE TABLE gpkg_tile_matrix_set (table_name TEXT NOT NULL PRIMARY KEY,srs_id INTEGER NOT NULL,min_x DOUBLE NOT NULL,min_y DOUBLE NOT NULL,max_x DOUBLE NOT NULL,max_y DOUBLE NOT NULL,CONSTRAINT fk_gtms_table_name FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name),CONSTRAINT fk_gtms_srs FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys (srs_id))");
                queries.Add(@"CREATE VIRTUAL TABLE ""rtree_" + tablename + @"_geometry"" USING rtree(id, minx, maxx, miny, maxy)");
                

                // SQL-Befehle zum einfuegen von Daten
                queries.Add(@"INSERT INTO gpkg_spatial_ref_sys (srs_name, srs_id, organization, organization_coordsys_id, definition, description) VALUES (""Undefined Cartesian SRS"", -1, ""NONE"", -1, ""undefinded"", ""undefined Cartesian coordinate reference system"");");
                queries.Add(@"INSERT INTO gpkg_spatial_ref_sys (srs_name, srs_id, organization, organization_coordsys_id, definition, description) VALUES (""Undefined geographic SRS"", 0, ""NONE"", 0, ""undefinded"", ""undefined geographic coordinate reference system"");");
                queries.Add(@"INSERT INTO gpkg_spatial_ref_sys (srs_name, srs_id, organization, organization_coordsys_id, definition, description) VALUES (""WGS 84 geodetic"", 4326, ""EPSG"", 4326, 'GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AXIS[""Latitude"",NORTH],AXIS[""Longitude"",EAST],AUTHORITY[""EPSG"",""4326""]]', ""longitude/latitude coordinates in decimal degrees on the WGS 84 spheroid"");");
                queries.Add(String.Format(@"INSERT INTO gpkg_contents (table_name, data_type, identifier, min_x, min_y, max_x, max_y, srs_id) VALUES (""{0}"", ""features"", ""{0}"", {1}, {2}, {3}, {4},  4326);", tablename, xMin, yMin, xMax, yMax));
                queries.Add(String.Format(@"INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES (""{0}"", ""geometry"", ""POLYGON"", 4326, 0, 0);", tablename));
                queries.Add(String.Format(@"INSERT INTO gpkg_ogr_contents (table_name, feature_count) VALUES (""{0}"", 1);", tablename));
                queries.Add(String.Format(@"INSERT INTO rtree_{0}_geometry_rowid (rowid, nodeno) VALUES (1,1);", tablename));
                queries.Add(String.Format(@"INSERT INTO rtree_{0}_geometry (minx, maxx, miny, maxy) VALUES ({1}, {2}, {3}, {4});", tablename, xMin, xMax, yMin, yMax));

                foreach (string q in queries)
                {
                    SQLiteCommand comm = new SQLiteCommand(q, connection);
                    Console.WriteLine(q);
                    Console.WriteLine(comm.ExecuteScalar());
                }

                connection.Close();
            }

            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={gpkgPath};Version=3;"))
            {

                connection.Open();
                connection.EnableExtensions(true);

                string insertQuery = @"
                INSERT INTO " + tablename + " (fid, Geometry) VALUES (1, @PolygonData)";

                string sqlCommand = "INSERT INTO fields (fid, geometry) VALUES (1, @BlobData)";
                using (SQLiteCommand command = new SQLiteCommand(sqlCommand, connection))
                {
                    // SQLite-Parameter hinzufügen
                    command.Parameters.Add("@BlobData", DbType.Binary).Value = rawData;

                    // Befehl ausführen
                    command.ExecuteNonQuery();
                }

                List<string> queries2 = new List<string>();

                queries2.Add(@"CREATE TRIGGER 'gpkg_tile_matrix_matrix_height_insert' BEFORE INSERT ON 'gpkg_tile_matrix' FOR EACH ROW BEGIN SELECT RAISE(ABORT, 'insert on table ''gpkg_tile_matrix'' violates constraint: matrix_height cannot be less than 1') WHERE (NEW.matrix_height < 1); END");
                queries2.Add(@"CREATE TRIGGER 'gpkg_tile_matrix_matrix_height_update' BEFORE UPDATE OF matrix_height ON 'gpkg_tile_matrix' FOR EACH ROW BEGIN SELECT RAISE(ABORT, 'update on table ''gpkg_tile_matrix'' violates constraint: matrix_height cannot be less than 1') WHERE (NEW.matrix_height < 1); END");
                queries2.Add(@"CREATE TRIGGER 'gpkg_tile_matrix_matrix_width_insert' BEFORE INSERT ON 'gpkg_tile_matrix' FOR EACH ROW BEGIN SELECT RAISE(ABORT, 'insert on table ''gpkg_tile_matrix'' violates constraint: matrix_width cannot be less than 1') WHERE (NEW.matrix_width < 1); END");
                queries2.Add(@"CREATE TRIGGER 'gpkg_tile_matrix_matrix_width_update' BEFORE UPDATE OF matrix_width ON 'gpkg_tile_matrix' FOR EACH ROW BEGIN SELECT RAISE(ABORT, 'update on table ''gpkg_tile_matrix'' violates constraint: matrix_width cannot be less than 1') WHERE (NEW.matrix_width < 1); END");
                queries2.Add(@"CREATE TRIGGER 'gpkg_tile_matrix_pixel_x_size_insert' BEFORE INSERT ON 'gpkg_tile_matrix' FOR EACH ROW BEGIN SELECT RAISE(ABORT, 'insert on table ''gpkg_tile_matrix'' violates constraint: pixel_x_size must be greater than 0') WHERE NOT (NEW.pixel_x_size > 0); END");
                queries2.Add(@"CREATE TRIGGER 'gpkg_tile_matrix_pixel_x_size_update' BEFORE UPDATE OF pixel_x_size ON 'gpkg_tile_matrix' FOR EACH ROW BEGIN SELECT RAISE(ABORT, 'update on table ''gpkg_tile_matrix'' violates constraint: pixel_x_size must be greater than 0') WHERE NOT (NEW.pixel_x_size > 0); END");
                queries2.Add(@"CREATE TRIGGER 'gpkg_tile_matrix_pixel_y_size_insert' BEFORE INSERT ON 'gpkg_tile_matrix' FOR EACH ROW BEGIN SELECT RAISE(ABORT, 'insert on table ''gpkg_tile_matrix'' violates constraint: pixel_y_size must be greater than 0') WHERE NOT (NEW.pixel_y_size > 0); END");
                queries2.Add(@"CREATE TRIGGER 'gpkg_tile_matrix_pixel_y_size_update' BEFORE UPDATE OF pixel_y_size ON 'gpkg_tile_matrix' FOR EACH ROW BEGIN SELECT RAISE(ABORT, 'update on table ''gpkg_tile_matrix'' violates constraint: pixel_y_size must be greater than 0') WHERE NOT (NEW.pixel_y_size > 0); END");
                queries2.Add(@"CREATE TRIGGER 'gpkg_tile_matrix_zoom_level_insert' BEFORE INSERT ON 'gpkg_tile_matrix' FOR EACH ROW BEGIN SELECT RAISE(ABORT, 'insert on table ''gpkg_tile_matrix'' violates constraint: zoom_level cannot be less than 0') WHERE (NEW.zoom_level < 0); END");
                queries2.Add(@"CREATE TRIGGER 'gpkg_tile_matrix_zoom_level_update' BEFORE UPDATE of zoom_level ON 'gpkg_tile_matrix' FOR EACH ROW BEGIN SELECT RAISE(ABORT, 'update on table ''gpkg_tile_matrix'' violates constraint: zoom_level cannot be less than 0') WHERE (NEW.zoom_level < 0); END");
                queries2.Add(@"CREATE TRIGGER ""rtree_" + tablename + @"_geometry_delete"" AFTER DELETE ON """ + tablename + @""" WHEN old.""geometry"" NOT NULL BEGIN DELETE FROM ""rtree_" + tablename + @"_geometry"" WHERE id = OLD.""fid""; END");
                queries2.Add(@"CREATE TRIGGER ""rtree_" + tablename + @"_geometry_insert"" AFTER INSERT ON """ + tablename + @""" WHEN (new.""geometry"" NOT NULL AND NOT ST_IsEmpty(NEW.""geometry"")) BEGIN INSERT OR REPLACE INTO ""rtree_" + tablename + @"_geometry"" VALUES (NEW.""fid"",ST_MinX(NEW.""geometry""), ST_MaxX(NEW.""geometry""),ST_MinY(NEW.""geometry""), ST_MaxY(NEW.""geometry"")); END");
                queries2.Add(@"CREATE TRIGGER ""rtree_" + tablename + @"_geometry_update1"" AFTER UPDATE OF ""geometry"" ON """ + tablename + @""" WHEN OLD.""fid"" = NEW.""fid"" AND (NEW.""geometry"" NOTNULL AND NOT ST_IsEmpty(NEW.""geometry"")) BEGIN INSERT OR REPLACE INTO ""rtree_" + tablename + @"_geometry"" VALUES (NEW.""fid"",ST_MinX(NEW.""geometry""), ST_MaxX(NEW.""geometry""),ST_MinY(NEW.""geometry""), ST_MaxY(NEW.""geometry"")); END");
                queries2.Add(@"CREATE TRIGGER ""rtree_" + tablename + @"_geometry_update2"" AFTER UPDATE OF ""geometry"" ON """ + tablename + @""" WHEN OLD.""fid"" = NEW.""fid"" AND (NEW.""geometry"" ISNULL OR ST_IsEmpty(NEW.""geometry"")) BEGIN DELETE FROM ""rtree_" + tablename + @"_geometry"" WHERE id = OLD.""fid""; END");
                queries2.Add(@"CREATE TRIGGER ""rtree_" + tablename + @"_geometry_update3"" AFTER UPDATE ON """ + tablename + @""" WHEN OLD.""fid"" != NEW.""fid"" AND (NEW.""geometry"" NOTNULL AND NOT ST_IsEmpty(NEW.""geometry"")) BEGIN DELETE FROM ""rtree_" + tablename + @"_geometry"" WHERE id = OLD.""fid""; INSERT OR REPLACE INTO ""rtree_" + tablename + @"_geometry"" VALUES (NEW.""fid"",ST_MinX(NEW.""geometry""), ST_MaxX(NEW.""geometry""),ST_MinY(NEW.""geometry""), ST_MaxY(NEW.""geometry"")); END");
                queries2.Add(@"CREATE TRIGGER ""rtree_" + tablename + @"_geometry_update4"" AFTER UPDATE ON """ + tablename + @""" WHEN OLD.""fid"" != NEW.""fid"" AND (NEW.""geometry"" ISNULL OR ST_IsEmpty(NEW.""geometry"")) BEGIN DELETE FROM ""rtree_" + tablename + @"_geometry"" WHERE id IN (OLD.""fid"", NEW.""fid""); END");
                queries2.Add(@"CREATE TRIGGER ""trigger_delete_feature_count_" + tablename + @""" AFTER DELETE ON """ + tablename + @""" BEGIN UPDATE gpkg_ogr_contents SET feature_count = feature_count - 1 WHERE lower(table_name) = lower('" + tablename + @"'); END");
                queries2.Add(@"CREATE TRIGGER ""trigger_insert_feature_count_" + tablename + @""" AFTER INSERT ON """ + tablename + @""" BEGIN UPDATE gpkg_ogr_contents SET feature_count = feature_count + 1 WHERE lower(table_name) = lower('" + tablename + @"'); END");

                foreach (string q in queries2)
                {
                    SQLiteCommand comm = new SQLiteCommand(q, connection);
                    Console.WriteLine(q);
                    Console.WriteLine(comm.ExecuteScalar());
                }

                connection.Close();
            }

        }

        private void Export_IsoXml(string kmlPath, string isoXmlDirPath, string farm, string farmer)
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

            coordinates = coordinates.Take(coordinates.Count() - 1).ToArray();

            IsoXml.IsoXml isoxml = new IsoXml.IsoXml(farmer, farm);
            FieldBoundary boundary = new FieldBoundary();

            PFD pfd = new PFD();
            int pfdCnt = 1;
            pfd.Name = Path.GetFileName(Path.GetDirectoryName(kmlPath)); 

            isoxml.PFDList.Add(pfd);

            pfd.Depth = 1;
            pfd.Id = pfdCnt;
            pfdCnt++;
            pfd.PLN = new PLN();
            pfd.PLN.Depth = 1;
            LSG lsg = new LSG();
            lsg.Depth = 1;
            lsg.LSGType = LSG.Type.Boundary;
            pfd.PLN.LSG = lsg;
            foreach(string coordinate in coordinates)
            {
                string[] coordAr = coordinate.Replace(",", "").Split(' ');
                lsg.Points.Add(new PNT(decimal.Parse(coordAr[0]), decimal.Parse(coordAr[1]), PNT.PntType.Boundary) { Depth = 4 });
            }

            Directory.CreateDirectory(isoXmlDirPath);
            string isoXmlDirPath2 = isoXmlDirPath + "\\TASKDATA";
            Directory.CreateDirectory(isoXmlDirPath2);
            string isoXmlPath = isoXmlDirPath2 + "\\TASKDATA.xml";

            File.WriteAllText(isoXmlPath, isoxml.ToString());
            string zipPath = isoXmlDirPath + ".zip";

            if (!File.Exists(zipPath))
            {
                ZipFile.CreateFromDirectory(isoXmlDirPath, zipPath);
                Directory.Delete(isoXmlDirPath, true);
            }
            else
            {
                Directory.Delete(isoXmlDirPath, true);
                
            }

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

        private void cbChooseFiletype_TextChanged(object sender, EventArgs e)
        {
            if(cbChooseFiletype.Text == "ISOXML")
            {
                this.lbFarm.Visible = true;
                this.lbFarmer.Visible = true;
                this.txtFarm.Visible = true;
                this.txtFarmer.Visible = true;
            }
            else
            {
                this.lbFarm.Visible = false;
                this.lbFarmer.Visible = false;
                this.txtFarm.Visible = false;
                this.txtFarmer.Visible = false;
            }
        }


        private void txtFarmer_Enter(object sender, EventArgs e)
        {
            this.txtFarmer.Text = "";
        }

        private void txtFarmer_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.txtFarmer.Text))
            {
                this.txtFarmer.Text = "AgOpenFarmer";
            }
        }

        private void txtFarm_Enter(object sender, EventArgs e)
        {
            this.txtFarm.Text = "";

        }

        private void txtFarm_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.txtFarm.Text))
            {
                this.txtFarm.Text = "AgOpenFarm";
            }
        }
    }
}
