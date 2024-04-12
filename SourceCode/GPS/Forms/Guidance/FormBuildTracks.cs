﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.LinkLabel;

namespace AgOpenGPS
{
    public partial class FormBuildTracks : Form
    {
        //access to the main GPS form and all its variables
        private readonly FormGPS mf;

        private double aveLineHeading;
        private int originalLine = 0;
        private bool isClosing;
        private int selectedItem = -1;
        public List<CTrk> gTemp = new List<CTrk>();

        private bool isRefRightSide = true; //left side 0 middle 1 right 2

        private bool isOn = true;

        //used throughout to acces the master Track list
        private int idx;

        public FormBuildTracks(Form _mf)
        {
            mf = _mf as FormGPS;
            InitializeComponent();

            //btnPausePlay.Text = gStr.gsPause;
            this.Text = "Tracks";
        }

        private void FormBuildTracks_Load(object sender, EventArgs e)
        {
            idx = mf.trk.gArr.Count - 1;

            gTemp.Clear();

            foreach (var item in mf.trk.gArr)
            {
                gTemp.Add(new CTrk(item));
            }

            panelMain.Top = 3; panelMain.Left = 3;
            panelCurve.Top = 3; panelCurve.Left = 3;
            panelName.Top = 3; panelName.Left = 3;
            panelKML.Top = 3; panelKML.Left = 3;
            panelEditName.Top = 3; panelEditName.Left = 3;
            panelChoose.Top = 3; panelChoose.Left = 3;
            panelABLine.Top = 3; panelABLine.Left = 3;
            panelAPlus.Top = 3; panelAPlus.Left = 3;
            panelLatLonPlus.Top = 3; panelLatLonPlus.Left = 3;
            panelLatLonLatLon.Top = 3; panelLatLonLatLon.Left = 3;
            panelPivot.Top = 3; panelPivot.Left = 3;

            panelEditName.Visible = false;
            panelMain.Visible = true;
            panelCurve.Visible = false;
            panelName.Visible = false;
            panelKML.Visible = false;
            panelChoose.Visible = false;
            panelABLine.Visible = false;
            panelAPlus.Visible = false;
            panelLatLonPlus.Visible = false;
            panelLatLonLatLon.Visible = false;
            panelPivot.Visible = false;

            this.Size = new System.Drawing.Size(650, 480);

            originalLine = mf.trk.idx;

            mf.curve.isMakingCurve = false;
            selectedItem = -1;
            Location = Properties.Settings.Default.setWindow_buildTracksLocation;

            nudLatitudeA.Controls[0].Enabled = false;
            nudLongitudeA.Controls[0].Enabled = false;
            nudLatitudeB.Controls[0].Enabled = false;
            nudLatitudeB.Controls[0].Enabled = false;
            nudHeading.Controls[0].Enabled = false;
            nudLatitudePlus.Controls[0].Enabled = false;
            nudLongitudePlus.Controls[0].Enabled = false;
            nudHeadingLatLonPlus.Controls[0].Enabled = false;   

            nudLatitudeA.Value = (decimal)mf.pn.latitude;
            nudLatitudeB.Value = (decimal)mf.pn.latitude + 0.000005m;
            nudLongitudeA.Value = (decimal)mf.pn.longitude;
            nudLongitudeB.Value = (decimal)mf.pn.longitude + 0.000005m;
            nudLatitudePlus.Value = (decimal)mf.pn.latitude;
            nudLongitudePlus.Value = (decimal)mf.pn.longitude;
            nudHeading.Value = 0;
            nudHeadingLatLonPlus.Value = 0;

            UpdateTable();
        }

        private void FormBuildTracks_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isClosing)
            {
                e.Cancel = true;
                return;
            }

            Properties.Settings.Default.setWindow_buildTracksLocation = Location;
            Properties.Settings.Default.Save();

            mf.twoSecondCounter = 100;

            mf.PanelUpdateRightAndBottom();
        }
        
        private void btnCancelMain_Click(object sender, EventArgs e)
        {
            //reload what was
            isClosing = true;
            mf.curve.desList?.Clear();

            if (mf.isBtnAutoSteerOn) mf.btnAutoSteer.PerformClick();
            if (mf.yt.isYouTurnBtnOn) mf.btnAutoYouTurn.PerformClick();

            mf.trk.gArr.Clear();

            foreach (var item in gTemp)
            {
                mf.trk.gArr.Add(new CTrk(item));
            }

            mf.trk.idx = originalLine;

            mf.curve.isCurveValid = false;
            mf.ABLine.isABValid = false;

            mf.twoSecondCounter = 100;

            Close();
        }

        private void btnListUse_Click(object sender, EventArgs e)
        {
            isClosing = true;
            //reset to generate new reference
            mf.curve.isCurveValid = false;
            mf.ABLine.isABValid = false;
            mf.curve.desList?.Clear();

            if (mf.yt.isYouTurnBtnOn) mf.btnAutoYouTurn.PerformClick();
            
            mf.FileSaveTracks();

            if (selectedItem > -1 && mf.trk.gArr.Count > 0 && mf.trk.gArr[selectedItem].isVisible)
            {
                mf.trk.idx = selectedItem;
                mf.yt.ResetYouTurn();

                Close();
            }

            else if (mf.trk.gArr.Count > 0)
            {               
                bool isOneVis = false;
                int trac = -1;

                foreach (var item in mf.trk.gArr)
                {
                    trac++;
                    if (item.isVisible)
                    {
                        isOneVis = true;
                        break;                        
                    }
                }

                //just choose a visible something
                if (isOneVis)
                {
                    mf.trk.idx = trac;
                    mf.yt.ResetYouTurn();
                    Close();
                }
                else //nothing visible
                {
                    idx = -1;
                    mf.DisableYouTurnButtons();
                    if (mf.isBtnAutoSteerOn) mf.btnAutoSteer.PerformClick();
                    Close();
                }
            }
            else
            {
                idx = -1;
                mf.DisableYouTurnButtons();
                if (mf.yt.isYouTurnBtnOn) mf.btnAutoYouTurn.PerformClick();

                //mf.curve.numCurveLineSelected = 0;
                Close();
            }
        }

        #region Main Controls
        private void UpdateTable()
        {
            int scrollPixels = flp.VerticalScroll.Value;

            Font backupfont = new Font(Font.FontFamily, 18F, FontStyle.Regular);
            flp.Controls.Clear();


            for (int i = 0; i < mf.trk.gArr.Count; i++)
            {
                //outer inner
                Button a = new Button
                {
                    Margin = new Padding(20, 10, 2, 10),
                    Size = new Size(40, 25),
                    Name = i.ToString(),
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                a.Click += A_Click;

                if (mf.trk.gArr[i].isVisible)
                    a.BackColor = System.Drawing.Color.Green;
                else
                    a.BackColor = System.Drawing.Color.Red;

                Button b = new Button
                {
                    Margin = new Padding(1, 10, 3, 10),
                    Size = new Size(35, 25),
                    Name = i.ToString(),
                    TextAlign = ContentAlignment.MiddleCenter,
                    FlatStyle = FlatStyle.Flat,
            };

                if (mf.trk.gArr[i].mode == (int)TrackMode.AB)
                    b.Image = Properties.Resources.TrackLine;
                else if (mf.trk.gArr[i].mode == (int)TrackMode.waterPivot)
                    b.Image = Properties.Resources.TrackPivot;
                else
                    b.Image = Properties.Resources.TrackCurve;

                b.FlatAppearance.BorderSize = 0;

                TextBox t = new TextBox
                {
                    Margin = new Padding(3),
                    Size = new Size(330, 35),
                    Text = mf.trk.gArr[i].name,
                    Name = i.ToString(),
                    Font = backupfont,
                    ReadOnly = true                    
                };
                t.Click += LineSelected_Click;
                t.Cursor = System.Windows.Forms.Cursors.Default;

                if (mf.trk.gArr[i].isVisible)
                    t.ForeColor = System.Drawing.Color.Black;
                else
                    t.ForeColor = System.Drawing.Color.Gray;

                if (i == selectedItem)
                {
                    t.BackColor = Color.LightBlue;
                }
                else
                {
                    t.BackColor = Color.AliceBlue;
                }

                flp.Controls.Add(b);
                flp.Controls.Add(t);
                flp.Controls.Add(a);
            }

            flp.VerticalScroll.Value = 1;
            flp.VerticalScroll.Value = scrollPixels;
            flp.PerformLayout();
        }

        private void A_Click(object sender, EventArgs e)
        {
            if (sender is Button b)
            {
                int line = Convert.ToInt32(b.Name);
                mf.trk.gArr[line].isVisible = !mf.trk.gArr[line].isVisible;

                for (int i = 0; i < mf.trk.gArr.Count; i++)
                {
                    flp.Controls[(i) * 3 + 1].BackColor = Color.AliceBlue;
                }
                selectedItem = -1;

                b.BackColor = mf.trk.gArr[line].isVisible ? System.Drawing.Color.Green : System.Drawing.Color.Red;

                flp.Controls[(line) * 3 + 1].ForeColor = mf.trk.gArr[line].isVisible ? System.Drawing.Color.Black : System.Drawing.Color.Gray;
            }
        }

        private void LineSelected_Click(object sender, EventArgs e)
        {
            if (sender is TextBox t)
            {
                int line = Convert.ToInt32(t.Name);
                int numLines = mf.trk.gArr.Count;

                //un highlight selected item
                for (int i = 0; i < numLines; i++)
                {
                    flp.Controls[(i) * 3 + 1].BackColor = Color.AliceBlue;
                }

                if (mf.trk.gArr[line].isVisible)
                {
                    //just highlight it
                    if (selectedItem == -1)
                    {
                        selectedItem = line;
                        selectedItem = Convert.ToInt32(t.Name);
                        flp.Controls[(line) * 3 + 1].BackColor = Color.LightBlue;
                    }

                    //a different line was selcted and one already was
                    else if (selectedItem != line)
                    {
                        selectedItem = line;
                        flp.Controls[(line) * 3 + 1].BackColor = Color.LightBlue;
                    }
                }
                else
                {
                    selectedItem = -1;
                }

                //UpdateTable();
            }
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            if (selectedItem == -1 || selectedItem == 0)
                return;

            mf.trk.gArr.Reverse(selectedItem - 1, 2);
            selectedItem--;
            idx = selectedItem;

            int scrollPixels = flp.VerticalScroll.Value;

            scrollPixels -= 45;
            if (scrollPixels < 0) scrollPixels = 0;

            flp.VerticalScroll.Value = 1;
            flp.VerticalScroll.Value = scrollPixels;
            flp.PerformLayout();

            UpdateTable();
        }

        private void btnMoveDn_Click(object sender, EventArgs e)
        {
            if (selectedItem == -1 || selectedItem == (mf.trk.gArr.Count - 1))
                return;

            mf.trk.gArr.Reverse(selectedItem, 2);
            selectedItem++;

            idx = selectedItem;

            int scrollPixels = flp.VerticalScroll.Value;

            scrollPixels += 45;
            if (scrollPixels > flp.VerticalScroll.Maximum) scrollPixels = flp.VerticalScroll.Maximum;

            flp.VerticalScroll.Value = 1;
            flp.VerticalScroll.Value = scrollPixels;
            flp.PerformLayout();

            UpdateTable();
        }

        private void btnSwapAB_Click(object sender, EventArgs e)
        {
            if (selectedItem > -1)
            {
                idx = selectedItem;

                if (mf.trk.gArr[idx].mode == (int)TrackMode.AB)
                {
                    vec2 bob = mf.trk.gArr[idx].ptA;
                    mf.trk.gArr[idx].ptA = mf.trk.gArr[idx].ptB;
                    mf.trk.gArr[idx].ptB = new vec2(bob);

                    mf.trk.gArr[idx].heading += Math.PI;
                    if (mf.trk.gArr[idx].heading < 0) mf.trk.gArr[idx].heading += glm.twoPI;
                    if (mf.trk.gArr[idx].heading > glm.twoPI) mf.trk.gArr[idx].heading -= glm.twoPI;
                }
                else
                {
                    int cnt = mf.trk.gArr[idx].curvePts.Count;
                    if (cnt > 0)
                    {
                        mf.trk.gArr[idx].curvePts.Reverse();

                        vec3[] arr = new vec3[cnt];
                        cnt--;
                        mf.trk.gArr[idx].curvePts.CopyTo(arr);
                        mf.trk.gArr[idx].curvePts.Clear();

                        mf.trk.gArr[idx].heading += Math.PI;
                        if (mf.trk.gArr[idx].heading < 0) mf.trk.gArr[idx].heading += glm.twoPI;
                        if (mf.trk.gArr[idx].heading > glm.twoPI) mf.trk.gArr[idx].heading -= glm.twoPI;

                        for (int i = 1; i < cnt; i++)
                        {
                            vec3 pt3 = arr[i];
                            pt3.heading += Math.PI;
                            if (pt3.heading > glm.twoPI) pt3.heading -= glm.twoPI;
                            if (pt3.heading < 0) pt3.heading += glm.twoPI;
                            mf.trk.gArr[idx].curvePts.Add(new vec3(pt3));
                        }

                        vec2 temp = new vec2(mf.trk.gArr[idx].ptA);

                        (mf.trk.gArr[idx].ptA) = new vec2(mf.trk.gArr[idx].ptB);
                        (mf.trk.gArr[idx].ptB) = new vec2(temp);
                    }
                }

                UpdateTable();
                flp.Focus();

                mf.TimedMessageBox(1500, "A B Swapped", "Curve is Reversed");
            }
        }

        private void btnHideShow_Click(object sender, EventArgs e)
        {
                for (int i = 0; i < mf.trk.gArr.Count; i++)
                {
                    mf.trk.gArr[i].isVisible = isOn;
                }

                isOn = !isOn;

                UpdateTable();
        }

        private void btnNewTrack_Click(object sender, EventArgs e)
        {
            panelChoose.Visible = false;
            panelMain.Visible = false;
            panelCurve.Visible = false;
            panelName.Visible = false;
            panelABLine.Visible = false;
            panelAPlus.Visible = false;
            panelKML.Visible = false;

            mf.curve.desList?.Clear();
            panelChoose.Visible = true;
        }

        private void btnListDelete_Click(object sender, EventArgs e)
        {
            if (selectedItem > -1)
            {
                mf.trk.gArr.RemoveAt(selectedItem);
                selectedItem = -1;

                mf.trk.idx = mf.trk.gArr.Count - 1;

                UpdateTable();
                flp.Focus();
            }
        }

        private void btnDuplicate_Click(object sender, EventArgs e)
        {
            if (selectedItem > -1)
            {
                int idx = selectedItem;

                panelMain.Visible = false;
                panelName.Visible = true;
                this.Size = new System.Drawing.Size(270, 360);

                mf.trk.gArr.Add(new CTrk (mf.trk.gArr[idx]));

                idx = mf.trk.gArr.Count - 1;

                selectedItem = -1;

                textBox1.Text = mf.trk.gArr[idx].name + " Copy";
            }
        }

        private void btnEditName_Click(object sender, EventArgs e)
        {
            if (selectedItem > -1)
            {
                idx = selectedItem;

                textBox2.Text = mf.trk.gArr[idx].name;

                panelMain.Visible = false;
                panelEditName.Visible = true;

                this.Size = new System.Drawing.Size(270, 360);
            }
        }

        #endregion

        #region Pick
        private void btnzABCurve_Click(object sender, EventArgs e)
        {
            panelChoose.Visible = false;
            panelCurve.Visible = true;

            btnACurve.Enabled = true;
            btnBCurve.Enabled = false;
            btnPausePlay.Enabled = false;
            mf.curve.desList?.Clear();

            this.Size = new System.Drawing.Size(270, 360);
        }

        private void btnzAPlus_Click(object sender, EventArgs e)
        {
            panelChoose.Visible = false;
            panelAPlus.Visible = true;

            btnAPlus.Enabled = true;
            mf.curve.desList?.Clear();
            nudHeading.Enabled = false;

            this.Size = new System.Drawing.Size(270, 360);
        }

        private void btnzABLine_Click(object sender, EventArgs e)
        {
            panelChoose.Visible = false;
            panelABLine.Visible = true;

            btnALine.Enabled = true;
            btnBLine.Enabled = false;
            btnPausePlay.Enabled = false;
            mf.curve.desList?.Clear();

            this.Size = new System.Drawing.Size(270, 360);
        }

        private void btnzLatLonPlusHeading_Click(object sender, EventArgs e)
        {
            panelChoose.Visible = false;
            panelLatLonPlus.Visible = true;
            this.Size = new System.Drawing.Size(370, 460);

            nudLatitudePlus.Value = (decimal)mf.pn.latitude;
            nudLongitudePlus.Value = (decimal)mf.pn.longitude;
        }

        private void btnzLatLon_Click(object sender, EventArgs e)
        {
            panelChoose.Visible = false;
            panelLatLonLatLon.Visible = true;
            this.Size = new System.Drawing.Size(370, 460);
        }
        private void btnLatLonPivot_Click(object sender, EventArgs e)
        {

            panelChoose.Visible = false;
            panelPivot.Visible = true;
            this.Size = new System.Drawing.Size(370,360);

            nudLatitudePivot.Value = (decimal)mf.pn.latitude;
            nudLongitudePivot.Value = (decimal)mf.pn.longitude;
        }

        #endregion

        #region Curve
        private void btnRefSideCurve_Click(object sender, EventArgs e)
        {
            isRefRightSide = !isRefRightSide;
            btnRefSideCurve.Image = isRefRightSide ?
            Properties.Resources.BoundaryRight : Properties.Resources.BoundaryLeft;
        }

        private void btnACurve_Click(object sender, System.EventArgs e)
        {
            lblCurveExists.Text = gStr.gsDriving;

            btnBCurve.Enabled = true;
            btnACurve.Enabled = false;

            btnPausePlay.Enabled = true;
            btnPausePlay.Visible = true;

            mf.curve.isMakingCurve = true;
        }

        private void btnBCurve_Click(object sender, System.EventArgs e)
        {
            aveLineHeading = 0;
            mf.curve.isMakingCurve = false;
            panelCurve.Visible = false;
            panelName.Visible = true;
            
            int cnt = mf.curve.desList.Count;
            if (cnt > 3)
            {
                //make sure point distance isn't too big 
                mf.curve.MakePointMinimumSpacing(ref mf.curve.desList, 1.6);
                mf.curve.CalculateHeadings(ref mf.curve.desList);

                mf.trk.gArr.Add(new CTrk());
                //array number is 1 less since it starts at zero
                idx = mf.trk.gArr.Count - 1;

                mf.trk.gArr[idx].ptA =
                    new vec2(mf.curve.desList[0].easting, mf.curve.desList[0].northing);
                mf.trk.gArr[idx].ptB =
                    new vec2(mf.curve.desList[mf.curve.desList.Count - 1].easting,
                    mf.curve.desList[mf.curve.desList.Count - 1].northing);

                mf.trk.gArr[idx].mode = (int)TrackMode.Curve;

                //calculate average heading of line
                double x = 0, y = 0;
                foreach (vec3 pt in mf.curve.desList)
                {
                    x += Math.Cos(pt.heading);
                    y += Math.Sin(pt.heading);
                }
                x /= mf.curve.desList.Count;
                y /= mf.curve.desList.Count;
                aveLineHeading = Math.Atan2(y, x);
                if (aveLineHeading < 0) aveLineHeading += glm.twoPI;

                mf.trk.gArr[idx].heading = aveLineHeading;

                //build the tail extensions
                mf.curve.AddFirstLastPoints(ref mf.curve.desList);
                SmoothAB(4);
                mf.curve.CalculateHeadings(ref mf.curve.desList);

                //write out the Curve Points
                foreach (vec3 item in mf.curve.desList)
                {
                    mf.trk.gArr[idx].curvePts.Add(item);
                }

                mf.curve.desName = "Cu " +
                    (Math.Round(glm.toDegrees(aveLineHeading), 1)).ToString(CultureInfo.InvariantCulture) + "\u00B0 ";

                textBox1.Text = mf.curve.desName;

                panelCurve.Visible = false;
                panelName.Visible = true;

                double dist;

                if (isRefRightSide)
                {
                    dist = (mf.tool.width - mf.tool.overlap) * 0.5 + mf.tool.offset;
                    mf.trk.idx = idx;
                    mf.trk.NudgeRefCurve(dist);
                }
                else
                {
                    dist = (mf.tool.width - mf.tool.overlap) * -0.5 + mf.tool.offset;
                    mf.trk.idx = idx;
                    mf.trk.NudgeRefCurve(dist);
                }
            }
            else
            {
                mf.curve.isMakingCurve = false;
                mf.curve.desList?.Clear();

                panelMain.Visible = true;
                panelCurve.Visible = false;
                panelName.Visible = false;
                panelChoose.Visible = false;

                this.Size = new System.Drawing.Size(650, 480);
            }
        }

        private void btnPausePlayCurve_Click(object sender, EventArgs e)
        {
            if (mf.curve.isMakingCurve)
            {
                mf.curve.isMakingCurve = false;
                btnPausePlay.Image = Properties.Resources.BoundaryRecord;
                //btnPausePlay.Text = gStr.gsRecord;
                btnBCurve.Enabled = false;
            }
            else
            {
                mf.curve.isMakingCurve = true;
                btnPausePlay.Image = Properties.Resources.boundaryPause;
                //btnPausePlay.Text = gStr.gsPause;
                btnBCurve.Enabled = true;
            }
        }

        #endregion

        #region AB Line
        private void btnRefSideAB_Click(object sender, EventArgs e)
        {
            isRefRightSide = !isRefRightSide;
            btnRefSideAB.Image = isRefRightSide ?
            Properties.Resources.BoundaryRight : Properties.Resources.BoundaryLeft;
        }

        private void btnALine_Click(object sender, EventArgs e)
        {
            mf.ABLine.isMakingABLine = true;
            btnALine.Enabled = false;

            mf.ABLine.desPtA = new vec2(mf.pivotAxlePos.easting, mf.pivotAxlePos.northing);

            mf.ABLine.desLineEndA.easting = mf.ABLine.desPtA.easting - (Math.Sin(mf.pivotAxlePos.heading) * 1000);
            mf.ABLine.desLineEndA.northing = mf.ABLine.desPtA.northing - (Math.Cos(mf.pivotAxlePos.heading) * 1000);


            mf.ABLine.desLineEndB.easting = mf.ABLine.desPtA.easting + (Math.Sin(mf.pivotAxlePos.heading) * 1000);
            mf.ABLine.desLineEndB.northing = mf.ABLine.desPtA.northing + (Math.Cos(mf.pivotAxlePos.heading) * 1000);

            btnBLine.Enabled = true;
            btnALine.Enabled = false;

            btnEnter_AB.Enabled = true;
        }

        private void btnBLine_Click(object sender, EventArgs e)
        {
            mf.ABLine.desPtB = new vec2(mf.pivotAxlePos.easting, mf.pivotAxlePos.northing);

            btnBLine.BackColor = System.Drawing.Color.Teal;

            mf.ABLine.desHeading = Math.Atan2(mf.ABLine.desPtB.easting - mf.ABLine.desPtA.easting,
               mf.ABLine.desPtB.northing - mf.ABLine.desPtA.northing);
            if (mf.ABLine.desHeading < 0) mf.ABLine.desHeading += glm.twoPI;

            mf.ABLine.desLineEndA.easting = mf.ABLine.desPtA.easting - (Math.Sin(mf.ABLine.desHeading) * 1000);
            mf.ABLine.desLineEndA.northing = mf.ABLine.desPtA.northing - (Math.Cos(mf.ABLine.desHeading) * 1000);

            mf.ABLine.desLineEndB.easting = mf.ABLine.desPtA.easting + (Math.Sin(mf.ABLine.desHeading) * 1000);
            mf.ABLine.desLineEndB.northing = mf.ABLine.desPtA.northing + (Math.Cos(mf.ABLine.desHeading) * 1000);
        }

        private void btnEnter_AB_Click(object sender, EventArgs e)
        {
            mf.ABLine.isMakingABLine = false;
            mf.trk.gArr.Add(new CTrk());

            idx = mf.trk.gArr.Count - 1;

            mf.trk.gArr[idx].ptA = new vec2(mf.ABLine.desPtA);
            mf.trk.gArr[idx].ptB = new vec2(mf.ABLine.desPtB);

            mf.trk.gArr[idx].mode = (int)TrackMode.AB;

            mf.trk.gArr[idx].heading = mf.ABLine.desHeading;

            mf.ABLine.desName = "AB " +
                (Math.Round(glm.toDegrees(mf.ABLine.desHeading), 1)).ToString(CultureInfo.InvariantCulture) + "\u00B0 " ;
            textBox1.Text = mf.ABLine.desName;

            double dist;
            if (isRefRightSide)
            {
                dist = (mf.tool.width - mf.tool.overlap) * 0.5 + mf.tool.offset;
                mf.trk.idx = idx;
                mf.trk.NudgeRefABLine(dist);

            }
            else
            {
                dist = (mf.tool.width - mf.tool.overlap) * -0.5 + mf.tool.offset;
                mf.trk.idx = idx;
                mf.trk.NudgeRefABLine(dist);
            }


            panelABLine.Visible = false;
            panelName.Visible = true;

        }

        #endregion

        #region A Plus

        private void btnRefSideAPlus_Click(object sender, EventArgs e)
        {
            isRefRightSide = !isRefRightSide;
            btnRefSideAPlus.Image = isRefRightSide ?
            Properties.Resources.BoundaryRight : Properties.Resources.BoundaryLeft;
        }

        private void btnAPlus_Click(object sender, EventArgs e)
        {
            mf.ABLine.isMakingABLine = true;

            mf.ABLine.desPtA = new vec2(mf.pivotAxlePos.easting, mf.pivotAxlePos.northing);

            mf.ABLine.desPtB.easting = mf.ABLine.desPtA.easting + (Math.Sin(mf.pivotAxlePos.heading) * 200);
            mf.ABLine.desPtB.northing = mf.ABLine.desPtA.northing + (Math.Cos(mf.pivotAxlePos.heading) * 200);

            mf.ABLine.desLineEndA.easting = mf.ABLine.desPtA.easting - (Math.Sin(mf.pivotAxlePos.heading) * 1000);
            mf.ABLine.desLineEndA.northing = mf.ABLine.desPtA.northing - (Math.Cos(mf.pivotAxlePos.heading) * 1000);

            mf.ABLine.desLineEndB.easting = mf.ABLine.desPtA.easting + (Math.Sin(mf.pivotAxlePos.heading) * 1000);
            mf.ABLine.desLineEndB.northing = mf.ABLine.desPtA.northing + (Math.Cos(mf.pivotAxlePos.heading) * 1000);

            mf.ABLine.desHeading = mf.pivotAxlePos.heading;

            btnEnter_AB.Enabled = true;
            nudHeading.Enabled = true;
           
            nudHeading.Value = (decimal)(glm.toDegrees(mf.ABLine.desHeading));
        }

        private void nudHeading_Click(object sender, EventArgs e)
        {
            if (mf.KeypadToNUD((NudlessNumericUpDown)sender, this))
            {
                //original A pt. 
                mf.ABLine.desHeading = glm.toRadians((double)nudHeading.Value);

                //start end of line
                mf.ABLine.desPtB.easting = mf.ABLine.desPtA.easting + (Math.Sin(mf.ABLine.desHeading) * 200);
                mf.ABLine.desPtB.northing = mf.ABLine.desPtA.northing + (Math.Cos(mf.ABLine.desHeading) * 200);

                mf.ABLine.desLineEndA.easting = mf.ABLine.desPtA.easting - (Math.Sin(mf.ABLine.desHeading) * 1000);
                mf.ABLine.desLineEndA.northing = mf.ABLine.desPtA.northing - (Math.Cos(mf.ABLine.desHeading) * 1000);

                mf.ABLine.desLineEndB.easting = mf.ABLine.desPtA.easting + (Math.Sin(mf.ABLine.desHeading) * 1000);
                mf.ABLine.desLineEndB.northing = mf.ABLine.desPtA.northing + (Math.Cos(mf.ABLine.desHeading) * 1000);
            }
        }

        private void btnEnter_APlus_Click(object sender, EventArgs e)
        {
            mf.ABLine.isMakingABLine = false;
            mf.trk.gArr.Add(new CTrk());

            idx = mf.trk.gArr.Count - 1;

            mf.trk.gArr[idx].ptA = new vec2(mf.ABLine.desPtA);
            mf.trk.gArr[idx].ptB = new vec2(mf.ABLine.desPtB);

            mf.trk.gArr[idx].mode = (int)TrackMode.AB;

            mf.trk.gArr[idx].heading = mf.ABLine.desHeading;

            mf.ABLine.desName = "A+" +
                (Math.Round(glm.toDegrees(mf.ABLine.desHeading), 1)).ToString(CultureInfo.InvariantCulture) + "\u00B0 " ;
            textBox1.Text = mf.ABLine.desName;

            double dist;
            if (isRefRightSide)
            {
                dist = (mf.tool.width - mf.tool.overlap) * 0.5 + mf.tool.offset;
                mf.trk.idx = idx;
                mf.trk.NudgeRefABLine(dist);

            }
            else
            {
                dist = (mf.tool.width - mf.tool.overlap) * -0.5 + mf.tool.offset;
                mf.trk.idx = idx;
                mf.trk.NudgeRefABLine(dist);
            }

            panelAPlus.Visible = false;
            panelName.Visible = true;
        }

        #endregion

        #region KML Curve and line

        private void btnLoadABFromKML_Click(object sender, EventArgs e)
        {
            panelChoose.Visible = false;
            panelKML.Visible = true;

            mf.curve.desList?.Clear();

            this.Size = new System.Drawing.Size(270, 360);

            string fileAndDirectory;
            {
                //create the dialog instance
                OpenFileDialog ofd = new OpenFileDialog
                {
                    //set the filter to text KML only
                    Filter = "KML files (*.KML)|*.KML",

                    //the initial directory, fields, for the open dialog
                    InitialDirectory = mf.fieldsDirectory + mf.currentFieldDirectory
                };

                //was a file selected
                if (ofd.ShowDialog(this) == DialogResult.Cancel) return;
                else fileAndDirectory = ofd.FileName;
            }

            XmlDocument doc = new XmlDocument
            {
                PreserveWhitespace = false
            };

            try
            {
                doc.Load(fileAndDirectory);
                string trackName = Path.GetFileName(fileAndDirectory);
                trackName = trackName.Substring(0, trackName.Length - 4);

                XmlElement root = doc.DocumentElement;
                XmlNodeList trackList = root.GetElementsByTagName("coordinates");
                XmlNodeList namelist = root.GetElementsByTagName("name");

                if (namelist.Count > 1)
                {
                    trackName = namelist[1].InnerText;
                }

                //each element in the list is a track
                for (int i = 0; i < trackList.Count; i++)
                {
                    string line = trackList[i].InnerText;
                    line.Trim();
                    //line = coordinates;
                    char[] delimiterChars = { ' ', '\t', '\r', '\n' };
                    string[] numberSets = line.Split(delimiterChars);

                    //at least 3 points
                    if (numberSets.Length > 1)
                    {
                        foreach (string item in numberSets)
                        {
                            string[] fix = item.Split(',');
                            if (fix.Length != 3) continue;
                            double.TryParse(fix[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double lonK);
                            double.TryParse(fix[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double latK);

                            mf.pn.ConvertWGS84ToLocal(latK, lonK, out double norting, out double easting);

                            vec3 bndPt = new vec3(easting, norting, 0);
                            mf.curve.desList.Add(new vec3(bndPt));
                        }
                    }

                    //2 points
                    if (mf.curve.desList.Count == 2)
                    {
                        mf.ABLine.desPtA.easting = mf.curve.desList[0].easting;
                        mf.ABLine.desPtA.northing = mf.curve.desList[0].northing;

                        mf.ABLine.desPtB.easting = mf.curve.desList[1].easting;
                        mf.ABLine.desPtB.northing = mf.curve.desList[1].northing;

                        // heading based on AB points
                        mf.ABLine.desHeading = Math.Atan2(mf.ABLine.desPtB.easting - mf.ABLine.desPtA.easting,
                            mf.ABLine.desPtB.northing - mf.ABLine.desPtA.northing);
                        if (mf.ABLine.desHeading < 0) mf.ABLine.desHeading += glm.twoPI;

                        if (namelist.Count > i)
                        {
                            trackName = namelist[i + 1].InnerText;
                            mf.ABLine.desName = trackName;
                        }
                        else mf.ABLine.desName = "AB " +
                            (Math.Round(glm.toDegrees(mf.ABLine.desHeading), 1)).ToString(CultureInfo.InvariantCulture) + "\u00B0 ";

                        mf.trk.gArr.Add(new CTrk());

                        idx = mf.trk.gArr.Count - 1;

                        mf.trk.gArr[idx].heading = mf.ABLine.desHeading;
                        mf.trk.gArr[idx].mode = (int)TrackMode.AB;

                        mf.trk.gArr[idx].ptA = new vec2(mf.ABLine.desPtA);
                        mf.trk.gArr[idx].ptB = new vec2(mf.ABLine.desPtB);

                        //create a name
                        mf.trk.gArr[idx].name = mf.ABLine.desName;

                        mf.curve.desList?.Clear();
                    }
                    else if (mf.curve.desList.Count > 2)
                    {
                        //make sure point distance isn't too big 
                        mf.curve.MakePointMinimumSpacing(ref mf.curve.desList, 1.6);
                        mf.curve.CalculateHeadings(ref mf.curve.desList);

                        mf.trk.gArr.Add(new CTrk());

                        //array number is 1 less since it starts at zero
                        idx = mf.trk.gArr.Count - 1;

                        mf.trk.gArr[idx].ptA =
                            new vec2(mf.curve.desList[0].easting, mf.curve.desList[0].northing);
                        mf.trk.gArr[idx].ptB =
                            new vec2(mf.curve.desList[mf.curve.desList.Count - 1].easting,
                            mf.curve.desList[mf.curve.desList.Count - 1].northing);

                        mf.trk.gArr[idx].mode = (int)TrackMode.Curve;

                        //calculate average heading of line
                        double x = 0, y = 0;
                        foreach (vec3 pt in mf.curve.desList)
                        {
                            x += Math.Cos(pt.heading);
                            y += Math.Sin(pt.heading);
                        }
                        x /= mf.curve.desList.Count;
                        y /= mf.curve.desList.Count;
                        aveLineHeading = Math.Atan2(y, x);
                        if (aveLineHeading < 0) aveLineHeading += glm.twoPI;

                        mf.trk.gArr[idx].heading = aveLineHeading;

                        //build the tail extensions
                        mf.curve.AddFirstLastPoints(ref mf.curve.desList);
                        //SmoothAB(4);
                        mf.curve.CalculateHeadings(ref mf.curve.desList);

                        //write out the Curve Points
                        foreach (vec3 item in mf.curve.desList)
                        {
                            mf.trk.gArr[idx].curvePts.Add(new vec3(item));
                        }
                        if (namelist.Count > i)
                        {
                            trackName = namelist[i + 1].InnerText;
                            mf.curve.desName = trackName;
                        }
                        else mf.curve.desName = "Cu " +
                                 (Math.Round(glm.toDegrees(aveLineHeading), 1)).ToString(CultureInfo.InvariantCulture) + "\u00B0 ";

                        mf.trk.gArr[idx].name = mf.curve.desName;

                        mf.curve.desList?.Clear();
                    }
                    else
                    {
                        mf.TimedMessageBox(2000, gStr.gsErrorreadingKML, gStr.gsMissingABLinesFile);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }

            panelKML.Visible = false;
            panelName.Visible = false;
            panelMain.Visible = true;

            this.Size = new System.Drawing.Size(650, 480);

            mf.curve.desList?.Clear();

            UpdateTable();
            flp.Focus();
        }

        #endregion

        #region LatLon LatLon

        private void nudLatitudeA_Click(object sender, EventArgs e)
        {
            mf.KeypadToNUD((NudlessNumericUpDown)sender, this);
        }

        private void nudLongitudeA_Click(object sender, EventArgs e)
        {
            mf.KeypadToNUD((NudlessNumericUpDown)sender, this);
        }

        private void nudLatitudeB_Click(object sender, EventArgs e)
        {
            mf.KeypadToNUD((NudlessNumericUpDown)sender, this);
        }

        private void nudLongitudeB_Click(object sender, EventArgs e)
        {
            mf.KeypadToNUD((NudlessNumericUpDown)sender, this);
        }

        private void btnEnter_LatLonLatLon_Click(object sender, EventArgs e)
        {
            CalcHeadingAB();

            mf.ABLine.isMakingABLine = false;
            mf.trk.gArr.Add(new CTrk());

            idx = mf.trk.gArr.Count - 1;

            mf.trk.gArr[idx].ptA = new vec2(mf.ABLine.desPtA);
            mf.trk.gArr[idx].ptB = new vec2(mf.ABLine.desPtB);

            mf.trk.gArr[idx].mode = (int)TrackMode.AB;

            mf.trk.gArr[idx].heading = mf.ABLine.desHeading;

            mf.ABLine.desName = "AB " +
                (Math.Round(glm.toDegrees(mf.ABLine.desHeading), 1)).ToString(CultureInfo.InvariantCulture) + "\u00B0 ";
            textBox1.Text = mf.ABLine.desName;

            panelLatLonLatLon.Visible = false;
            panelName.Visible = true;

            this.Size = new System.Drawing.Size(270, 360);
        }

        public void CalcHeadingAB()
        {
            mf.pn.ConvertWGS84ToLocal((double)nudLatitudeA.Value, (double)nudLongitudeA.Value, out double nort, out double east);

            mf.ABLine.desPtA.easting = east;
            mf.ABLine.desPtA.northing = nort;

            mf.pn.ConvertWGS84ToLocal((double)nudLatitudeB.Value, (double)nudLongitudeB.Value, out nort, out east);
            mf.ABLine.desPtB.easting = east;
            mf.ABLine.desPtB.northing = nort;

            // heading based on AB points
            mf.ABLine.desHeading = Math.Atan2(mf.ABLine.desPtB.easting - mf.ABLine.desPtA.easting,
                mf.ABLine.desPtB.northing - mf.ABLine.desPtA.northing);
            if (mf.ABLine.desHeading < 0) mf.ABLine.desHeading += glm.twoPI;
        }

        private void btnFillLatLonLatLonA_Click(object sender, EventArgs e)
        {
            nudLatitudeA.Value = (decimal)mf.pn.latitude;
            nudLongitudeA.Value = (decimal)mf.pn.longitude;
        }

        private void btnFillLatLonLatLonB_Click(object sender, EventArgs e)
        {
            nudLatitudeB.Value = (decimal)mf.pn.latitude;
            nudLongitudeB.Value = (decimal)mf.pn.longitude;
        }

        #endregion

        #region LatLon +

        private void nudLatitudePlus_Click(object sender, EventArgs e)
        {
            mf.KeypadToNUD((NudlessNumericUpDown)sender, this);
        }

        private void nudLongitudePlus_Click(object sender, EventArgs e)
        {
            mf.KeypadToNUD((NudlessNumericUpDown)sender, this);
        }

        private void nudHeadingLatLonPlus_Click(object sender, EventArgs e)
        {
            mf.KeypadToNUD((NudlessNumericUpDown)sender, this);
        }

        private void btnEnter_LatLonPlus_Click(object sender, EventArgs e)
        {
            CalcHeadingAPlus();

            mf.ABLine.isMakingABLine = false;
            mf.trk.gArr.Add(new CTrk());

            idx = mf.trk.gArr.Count - 1;

            //start end of line
            mf.ABLine.desPtB.easting = mf.ABLine.desPtA.easting + (Math.Sin(mf.ABLine.desHeading) * 200);
            mf.ABLine.desPtB.northing = mf.ABLine.desPtA.northing + (Math.Cos(mf.ABLine.desHeading) * 200);

            mf.trk.gArr[idx].ptA = new vec2(mf.ABLine.desPtA);
            mf.trk.gArr[idx].ptB = new vec2(mf.ABLine.desPtB);

            mf.trk.gArr[idx].mode = (int)TrackMode.AB;

            mf.trk.gArr[idx].heading = mf.ABLine.desHeading;

            mf.ABLine.desName = "A+ " +
                (Math.Round(glm.toDegrees(mf.ABLine.desHeading), 1)).ToString(CultureInfo.InvariantCulture) + "\u00B0 ";
            textBox1.Text = mf.ABLine.desName;

            panelLatLonPlus.Visible = false;
            panelName.Visible = true;

            this.Size = new System.Drawing.Size(270, 360);
        }

        private void btnFillLatLonPlus_Click(object sender, EventArgs e)
        {
            nudLatitudePlus.Value = (decimal)mf.pn.latitude;
            nudLongitudePlus.Value = (decimal)mf.pn.longitude;
        }

        public void CalcHeadingAPlus()
        {
            mf.pn.ConvertWGS84ToLocal((double)nudLatitudePlus.Value, (double)nudLongitudePlus.Value, out double nort, out double east);

            mf.ABLine.desHeading = glm.toRadians((double)nudHeadingLatLonPlus.Value);
                mf.ABLine.desPtA.easting = east;
                mf.ABLine.desPtA.northing = nort;            
        }

        #endregion

        #region Lat Lon Pivot

        private void nudLatitudePivot_Click(object sender, EventArgs e)
        {
            mf.KeypadToNUD((NudlessNumericUpDown)sender, this);
        }

        private void nudLongitudePivot_Click(object sender, EventArgs e)
        {
            mf.KeypadToNUD((NudlessNumericUpDown)sender, this);
        }

        private void btnEnter_Pivot_Click(object sender, EventArgs e)
        {
            mf.pn.ConvertWGS84ToLocal((double)nudLatitudePivot.Value, (double)nudLongitudePivot.Value, out double nort, out double east);

            mf.trk.gArr.Add(new CTrk());

            idx = mf.trk.gArr.Count - 1;

            mf.trk.gArr[idx].ptA.easting = east;
            mf.trk.gArr[idx].ptA.northing = nort;
            mf.trk.gArr[idx].mode = (int)TrackMode.waterPivot;

            mf.ABLine.desName = "Piv";
            textBox1.Text = mf.ABLine.desName;

            panelPivot.Visible = false;
            panelName.Visible = true;

            this.Size = new System.Drawing.Size(270, 360);
        }

        private void btnFillLAtLonPivot_Click(object sender, EventArgs e)
        {
            nudLatitudePivot.Value = (decimal)mf.pn.latitude;
            nudLongitudePivot.Value = (decimal)mf.pn.longitude;
        }

        #endregion

        private void btnCancelCurve_Click(object sender, EventArgs e)
        {
            mf.curve.isMakingCurve = false;
            mf.curve.desList?.Clear();

            panelMain.Visible = true;
            panelEditName.Visible = false;
            panelName.Visible = false;
            panelChoose.Visible = false;
            panelCurve.Visible = false;
            panelABLine.Visible = false;
            panelAPlus.Visible = false;
            panelLatLonLatLon.Visible = false;
            panelLatLonPlus.Visible = false;
            panelKML.Visible = false;
            panelPivot.Visible = false;

            this.Size = new System.Drawing.Size(650, 480);
        }

        private void textBox_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
                mf.KeyboardToText((TextBox)sender, this);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0) textBox2.Text = "No Name " + DateTime.Now.ToString("hh:mm:ss", CultureInfo.InvariantCulture);

            int idx = mf.trk.gArr.Count - 1;

            mf.trk.gArr[idx].name = textBox1.Text.Trim();

            panelMain.Visible = true;
            panelName.Visible = false;

            this.Size = new System.Drawing.Size(650, 480);

            mf.curve.desList?.Clear();
            UpdateTable();
        }

        private void btnAddTime_Click(object sender, EventArgs e)
        {
            textBox1.Text += DateTime.Now.ToString(" hh:mm:ss", CultureInfo.InvariantCulture);
            mf.curve.desName = textBox1.Text;
        }

        private void btnAddTimeEdit_Click(object sender, EventArgs e)
        {
            textBox2.Text += DateTime.Now.ToString(" hh:mm:ss", CultureInfo.InvariantCulture);
        }

        private void btnSaveEditName_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Trim() == "") textBox2.Text = "No Name " + DateTime.Now.ToString("hh:mm:ss", CultureInfo.InvariantCulture);

            panelEditName.Visible = false;
            panelMain.Visible = true;

            mf.curve.desList?.Clear();

            mf.trk.gArr[idx].name = textBox2.Text.Trim();

            this.Size = new System.Drawing.Size(700, 450);

            UpdateTable();
            flp.Focus();
        }

        public void SmoothAB(int smPts)
        {
            //count the reference list of original curve
            int cnt = mf.curve.desList.Count;

            //the temp array
            vec3[] arr = new vec3[cnt];

            //read the points before and after the setpoint
            for (int s = 0; s < smPts / 2; s++)
            {
                arr[s].easting = mf.curve.desList[s].easting;
                arr[s].northing = mf.curve.desList[s].northing;
                arr[s].heading = mf.curve.desList[s].heading;
            }

            for (int s = cnt - (smPts / 2); s < cnt; s++)
            {
                arr[s].easting = mf.curve.desList[s].easting;
                arr[s].northing = mf.curve.desList[s].northing;
                arr[s].heading = mf.curve.desList[s].heading;
            }

            //average them - center weighted average
            for (int i = smPts / 2; i < cnt - (smPts / 2); i++)
            {
                for (int j = -smPts / 2; j < smPts / 2; j++)
                {
                    arr[i].easting += mf.curve.desList[j + i].easting;
                    arr[i].northing += mf.curve.desList[j + i].northing;
                }
                arr[i].easting /= smPts;
                arr[i].northing /= smPts;
                arr[i].heading = mf.curve.desList[i].heading;
            }

            //make a list to draw
            mf.curve.desList?.Clear();
            for (int i = 0; i < cnt; i++)
            {
                mf.curve.desList.Add(arr[i]);
            }
        }
    }
}