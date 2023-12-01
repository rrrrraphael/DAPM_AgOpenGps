using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using Newtonsoft.Json;
using GeoJSON.Net.Feature;
using System.Collections.ObjectModel;
using GeoJSON.Net.Geometry;
using GeoJSON.Net;
using Point = GeoJSON.Net.Geometry.Point;
using Polygon = GeoJSON.Net.Geometry.Polygon;
using LineString = GeoJSON.Net.Geometry.LineString;
using MultiPolygon = GeoJSON.Net.Geometry.MultiPolygon;
using System.Data.SQLite;
using System.Drawing;
using System.Data;

namespace AgOpenGPS
{
    public partial class FormImportField : Form
    {
        //class variables
        private readonly FormGPS mf = null;
        private double easting, northing, latK, lonK;

        public FormImportField(Form _callingForm)
        {
            //get copy of the calling main form
            mf = _callingForm as FormGPS;

            InitializeComponent();

            label1.Text = gStr.gsEnterFieldName;
            this.Text = gStr.gsCreateNewField;
            cbChooseFiletype.SelectedIndex = 0;
        }

        private void FormFieldDir_Load(object sender, EventArgs e)
        {
            btnSave.Enabled = false;
        }

        private void tboxFieldName_TextChanged(object sender, EventArgs e)
        {
            TextBox textboxSender = (TextBox)sender;
            int cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, glm.fileRegex, "");
            textboxSender.SelectionStart = cursorPosition;

            if (String.IsNullOrEmpty(tboxFieldName.Text.Trim()) && cbChooseFiletype.SelectedItem == null)
            {
                btnLoadField.Enabled = false;
            }
            else
            {
                btnLoadField.Enabled = true;
            }
        }

        private void btnSerialCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void tboxFieldName_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
            {
                mf.KeyboardToText((TextBox)sender, this);
                btnSerialCancel.Focus();
            }
        }

        private void tboxTask_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
            {
                mf.KeyboardToText((TextBox)sender, this);
                btnSerialCancel.Focus();
            }
        }

        private void tboxVehicle_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
            {
                mf.KeyboardToText((TextBox)sender, this);
                btnSerialCancel.Focus();
            }
        }

        private void btnLoadField_Click(object sender, EventArgs e)
        {
            // default readCoordinates is set for Shapefile
            ReadCoordinates ReadCoordinates = ReadCoordinatesFromKML;

            OpenFileDialog ofd = new OpenFileDialog
            {
                // default the filter is set for KML
                Filter = "KML files (*.KML)|*.KML",
                //the initial directory, fields, for the open dialog
                InitialDirectory = mf.fieldsDirectory
            };
            if (cbChooseFiletype.SelectedItem == "Geopackage")
            {
                //set the filter to Geopackage only
                ofd.Filter = "Geopackage files (*.GPKG)|*.GPKG";

                ReadCoordinates = ReadCoordinatesFromGeoPackage;
            }
            else if (cbChooseFiletype.SelectedItem == "Shapefile")
            {
                //set the filter to KML only
                ofd.Filter = "Shapefiles (*.SHP)|*.SHP";

                ReadCoordinates = ReadCoordinatesFromShapefile;
            }
            else if (cbChooseFiletype.SelectedItem == "GeoJSON")
            {
                //set the filter to GeoJSON only
                ofd.Filter = "GeoJSON files (*.GEOJSON)|*.GEOJSON";

                ReadCoordinates = ReadCoordinatesFromGeoJSON;
            }
            tboxFieldName.Enabled = false;
            btnAddDate.Enabled = false;
            btnAddTime.Enabled = false;

            //was a file selected
            if (ofd.ShowDialog() == DialogResult.Cancel) return;

            // read coordinates
            string[] coordinates = { };
            ReadCoordinates(ofd.FileName, ref coordinates);

  


            //at least 3 points
            if (coordinates.Length < 3)
            {
                mf.TimedMessageBox(2000, gStr.gsErrorReadingFile, gStr.gsChooseBuildDifferentone);
                return;
            }

            //get lat and lon from boundary
            FindLatLon(coordinates);

            //reset sim and world to field position
            CreateNewField();

            //Load the outer boundary
            LoadKMLBoundary(coordinates);
        }

        private delegate void ReadCoordinates(string filename, ref string[] coordinates);

        private void ReadCoordinatesFromShapefile(string filePath, ref string[] coordinates)
        {

            string[] numbersets = { };

            List<string> numberslist = new List<string>();
            try
            {
                NetTopologySuite.Features.Feature[] feature = Shapefile.ReadAllFeatures(filePath);
                if (feature.Length > 1)
                {
                    mf.TimedMessageBox(4000, gStr.gsError, gStr.gsError);
                }
                byte[] rawData = feature[0].Geometry.ToBinary();

                WKBReader reader = new WKBReader();

                Geometry geo = reader.Read(rawData);

                geo.Coordinates.ToList().ForEach(c => numberslist.Add(c.ToString().Replace("(", " ").Replace(")", " ").Replace(" ", "").Trim()));

                coordinates = numberslist.ToArray();
            }
            catch (Exception)
            {
                mf.TimedMessageBox(4000, gStr.gsError, gStr.gsError);
            }
            coordinates = numberslist.ToArray();

        }


        private void ReadCoordinatesFromGeoPackage(string filepath, ref string[] coordinates)
        {
           
            byte[] rawData = null;

            byte[] gpkgData = null;

            List<string> numberslist = new List<string>();

            string[] tableNames = { "gpkg_spatial_ref_sys", "gpkg_contents", "gpkg_ogr_contents", "gpkg_geometry_columns", "gpkg_tile_matrix_set", "gpkg_tile_matrix", "sqlite_sequence", "gpkg_extensions", "rtree_felder_geometry", "rtree_felder_geometry_rowid", "rtree_felder_geometry_node", "rtree_felder_geometry_parent" };

            string table = "";
            //SQlite connection
            try
            {
                using (var conn = new SQLiteConnection("Data Source = " + filepath + "; Version = 3;"))
                {
                    conn.Open();
                    var command = conn.CreateCommand();

                    //get geometry
                    // get table name
                    DataTable dt = conn.GetSchema("Tables");
                    foreach (DataRow row in dt.Rows)
                    {
                        string tablename = (string)row[2];
                        if (!tableNames.Contains(tablename))
                        {
                            table = tablename;
                            break;
                        }
                    }

                    // check field number

                    int fieldNumber = 0;
                    command.CommandText = @"SELECT count(*) FROM " + table + ";";
                    using (var SQLreader = command.ExecuteReader())
                    {
                        while (SQLreader.Read())
                        {
                            fieldNumber = Convert.ToInt32(SQLreader[0]);

                        }
                        if (fieldNumber > 1)
                        {
                            mf.TimedMessageBox(4000, gStr.gsTooManyFields, gStr.gsFirstOneIsUsed);
                        }
                    }



                    //get data

                    command.CommandText = @"SELECT geometry from " + table + ";";

                    using (var SQLreader = command.ExecuteReader())
                    {
                        while (SQLreader.Read())
                        {
                            rawData = (byte[])SQLreader["geometry"];
                            gpkgData = rawData.Skip(40).ToArray();      //SKIP gpkg Header
                        }

                    }

                    conn.Close();
                }

                WKBReader reader = new WKBReader();


                Geometry geo = reader.Read(gpkgData);

                geo.Coordinates.ToList().ForEach(c => numberslist.Add(c.ToString().Replace("(", " ").Replace(")", " ").Replace(" ", "").Trim()));

                coordinates = numberslist.ToArray();
            }
            catch (Exception ex)
            {
                mf.TimedMessageBox(2000, gStr.gsError, gStr.gsError);
            }
        }

        private void ReadCoordinatesFromGeoJSON(string filePath, ref string[] coordinates)
        {
            
            List<string> numberslist = new List<string>();

            string text = File.ReadAllText(filePath);
            try
            {
                FeatureCollection collection = JsonConvert.DeserializeObject<FeatureCollection>(text);
                // Ignore every GeoJSONObjectType but Polygon
                if (collection.Features[0].Geometry.Type.Equals(GeoJSONObjectType.Polygon))
                {
                    Polygon field = collection.Features[0].Geometry as Polygon;
                    if (field.Coordinates.Count > 0)
                    {
                        LineString border = field.Coordinates[0];
                        foreach (var borderCoordinates in border.Coordinates)
                        {
                            numberslist.Add(String.Format("{0},{1}", borderCoordinates.Longitude, borderCoordinates.Latitude));
                        }
                    }
                }

                coordinates = numberslist.ToArray();
            }
            catch (Exception) { }
        }

        private void ReadCoordinatesFromKML(string filePath, ref string[] coordinates)
        {

            using (System.IO.StreamReader reader = new System.IO.StreamReader(filePath))
            {
                bool alreadyOneField = false;
                try
                {
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
                            
                        }
                    }

                }
                catch (Exception)
                {
                    mf.TimedMessageBox(2000, "Exception", "Catch Exception");
                    return;
                }
            }

            mf.bnd.isOkToAddPoints = false;
        }

        private void btnAddDate_Click(object sender, EventArgs e)
        {
            tboxFieldName.Text += " " + DateTime.Now.ToString("MMM.dd", CultureInfo.InvariantCulture);

        }

        private void btnAddTime_Click(object sender, EventArgs e)
        {
            tboxFieldName.Text += " " + DateTime.Now.ToString("HH_mm", CultureInfo.InvariantCulture);

        }

        private void cbChooseFiletype_SelectedValueChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tboxFieldName.Text.Trim()))
            {
                btnLoadField.Enabled = false;
            }
            else
            {
                btnLoadField.Enabled = true;
            }
        }

        private void LoadKMLBoundary(string[] coordinates)
        {
            CBoundaryList New = new CBoundaryList();

            foreach (string item in coordinates)
            {
                if (item.Length < 3)
                    continue;
                string[] fix = item.Split(',');
                double.TryParse(fix[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lonK);
                double.TryParse(fix[1], NumberStyles.Float, CultureInfo.InvariantCulture, out latK);

                mf.pn.ConvertWGS84ToLocal(latK, lonK, out northing, out easting);

                //add the point to boundary
                New.fenceLine.Add(new vec3(easting, northing, 0));
            }

            //build the boundary, make sure is clockwise for outer counter clockwise for inner
            New.CalculateFenceArea(mf.bnd.bndList.Count);
            New.FixFenceLine(mf.bnd.bndList.Count);

            mf.bnd.bndList.Add(New);

            mf.btnABDraw.Visible = true;

            mf.FileSaveBoundary();
            mf.bnd.BuildTurnLines();
            mf.fd.UpdateFieldBoundaryGUIAreas();
            mf.CalculateMinMax();

            btnSave.Enabled = true;
            btnLoadField.Enabled = false;
        }

        private void FindLatLon(string[] coordinates)
        {
            double counter = 0, lat = 0, lon = 0;
            latK = lonK = 0;
            foreach (string item in coordinates)
            {
                if (item.Length < 3)
                    continue;
                string[] fix = item.Split(',');
                double.TryParse(fix[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lonK);
                double.TryParse(fix[1], NumberStyles.Float, CultureInfo.InvariantCulture, out latK);
                lat += latK;
                lon += lonK;
                counter += 1;
            }
            lonK = lon / counter;
            latK = lat / counter;
        }

        private void CreateNewField()
        {
            //fill something in
            if (String.IsNullOrEmpty(tboxFieldName.Text.Trim()))
            {
                Close();
                return;
            }

            //append date time to name

            mf.currentFieldDirectory = tboxFieldName.Text.Trim();

            //get the directory and make sure it exists, create if not
            string dirNewField = mf.fieldsDirectory + mf.currentFieldDirectory + "\\";

            mf.menustripLanguage.Enabled = false;
            //if no template set just make a new file.
            try
            {
                //start a new job
                mf.JobNew();

                //create it for first save
                string directoryName = Path.GetDirectoryName(dirNewField);

                if ((!string.IsNullOrEmpty(directoryName)) && (Directory.Exists(directoryName)))
                {
                    MessageBox.Show(gStr.gsChooseADifferentName, gStr.gsDirectoryExists, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                else
                {
                    mf.pn.latStart = latK;
                    mf.pn.lonStart = lonK;

                    if (mf.timerSim.Enabled)
                    {
                        mf.sim.latitude = Properties.Settings.Default.setGPS_SimLatitude = latK;
                        mf.sim.longitude = Properties.Settings.Default.setGPS_SimLongitude = lonK;

                        mf.pn.latitude = latK;
                        mf.pn.longitude = lonK;

                        Properties.Settings.Default.Save();
                    }

                    mf.pn.SetLocalMetersPerDegree();

                    //make sure directory exists, or create it
                    if ((!string.IsNullOrEmpty(directoryName)) && (!Directory.Exists(directoryName)))
                    { Directory.CreateDirectory(directoryName); }

                    mf.displayFieldName = mf.currentFieldDirectory;

                    //create the field file header info

                    if (!mf.isJobStarted)
                    {
                        using (FormTimedMessage form = new FormTimedMessage(3000, gStr.gsFieldNotOpen, gStr.gsCreateNewField))
                        { form.Show(this); }
                        return;
                    }
                    string myFileName, dirField;

                    //get the directory and make sure it exists, create if not
                    dirField = mf.fieldsDirectory + mf.currentFieldDirectory + "\\";
                    directoryName = Path.GetDirectoryName(dirField);

                    if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
                    { Directory.CreateDirectory(directoryName); }

                    myFileName = "Field.txt";

                    using (StreamWriter writer = new StreamWriter(dirField + myFileName))
                    {
                        //Write out the date
                        writer.WriteLine(DateTime.Now.ToString("yyyy-MMMM-dd hh:mm:ss tt", CultureInfo.InvariantCulture));

                        writer.WriteLine("$FieldDir");
                        writer.WriteLine("KML Derived");

                        //write out the easting and northing Offsets
                        writer.WriteLine("$Offsets");
                        writer.WriteLine("0,0");

                        writer.WriteLine("Convergence");
                        writer.WriteLine("0");

                        writer.WriteLine("StartFix");
                        writer.WriteLine(mf.pn.latStart.ToString(CultureInfo.InvariantCulture) + "," + mf.pn.lonStart.ToString(CultureInfo.InvariantCulture));
                    }

                    mf.FileCreateSections();
                    mf.FileCreateRecPath();
                    mf.FileCreateContour();
                    mf.FileCreateElevation();
                    mf.FileSaveFlags();
                    //mf.FileSaveABLine();
                    //mf.FileSaveCurveLine();
                    //mf.FileSaveHeadland();
                }
            }
            catch (Exception ex)
            {
                mf.WriteErrorLog("Creating new field " + ex);

                MessageBox.Show(gStr.gsError, ex.ToString());
                mf.currentFieldDirectory = "";
            }
        }
    }
}
