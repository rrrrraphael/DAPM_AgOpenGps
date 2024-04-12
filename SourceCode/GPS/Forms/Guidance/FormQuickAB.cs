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
    public partial class FormQuickAB : Form
    {
        //access to the main GPS form and all its variables
        private readonly FormGPS mf;

        private double aveLineHeading;
        public List<CTrk> gTemp = new List<CTrk>();

        private bool isRefRightSide = true; //left side 0 middle 1 right 2

        //used throughout to acces the master Track list
        private int idx;

        public FormQuickAB(Form _mf)
        {
            mf = _mf as FormGPS;
            InitializeComponent();

            //btnPausePlay.Text = gStr.gsPause;
            this.Text = "Tracks";
        }

        private void FormQuickAB_Load(object sender, EventArgs e)
        {
            panelCurve.Top = 3; panelCurve.Left = 3;
            panelName.Top = 3; panelName.Left = 3;
            panelChoose.Top = 3; panelChoose.Left = 3;
            panelABLine.Top = 3; panelABLine.Left = 3;
            panelAPlus.Top = 3; panelAPlus.Left = 3;

            panelChoose.Visible = true;
            panelCurve.Visible = false;
            panelName.Visible = false;
            panelABLine.Visible = false;
            panelAPlus.Visible = false;

            this.Size = new System.Drawing.Size(270, 360);

            mf.curve.isMakingCurve = false;

            Location = Properties.Settings.Default.setWindow_QuickABLocation;

            nudHeading.Controls[0].Enabled = false;
            nudHeading.Value = 0;
        }

        private void FormQuickAB_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.setWindow_QuickABLocation = Location;
            Properties.Settings.Default.Save();

            mf.twoSecondCounter = 100;

            mf.PanelUpdateRightAndBottom();
        }
        
        #region Pick
        private void btnzABCurve_Click(object sender, EventArgs e)
        {
            panelChoose.Visible = false;
            panelCurve.Visible = true;

            btnACurve.Enabled = true;
            btnBCurve.Enabled = false;
            btnPausePlay.Enabled = false;
            mf.curve.desList?.Clear();
        }

        private void btnzAPlus_Click(object sender, EventArgs e)
        {
            panelChoose.Visible = false;
            panelAPlus.Visible = true;

            btnAPlus.Enabled = true;
            mf.curve.desList?.Clear();
            nudHeading.Enabled = false;
        }

        private void btnzABLine_Click(object sender, EventArgs e)
        {
            panelChoose.Visible = false;
            panelABLine.Visible = true;

            btnALine.Enabled = true;
            btnBLine.Enabled = false;
            btnPausePlay.Enabled = false;
            mf.curve.desList?.Clear();
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
                panelCurve.Visible = false;
                panelName.Visible = false;
                panelChoose.Visible = false;
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
            mf.trk.gArr[idx].name = mf.ABLine.desName;

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


        private void btnAddTime_Click(object sender, EventArgs e)
        {
            textBox1.Text += DateTime.Now.ToString(" hh:mm:ss", CultureInfo.InvariantCulture);
            mf.curve.desName = textBox1.Text;
        }

        private void btnCancelCurve_Click(object sender, EventArgs e)
        {
            mf.curve.desList?.Clear();
            mf.ABLine.isMakingABLine = false;

            Close();
        }

        private void textBox_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
                mf.KeyboardToText((TextBox)sender, this);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0) textBox1.Text = "No Name " + DateTime.Now.ToString("hh:mm:ss", CultureInfo.InvariantCulture);

            int idx = mf.trk.gArr.Count - 1;

            mf.trk.gArr[idx].name = textBox1.Text.Trim();

            panelName.Visible = false;

            mf.curve.desList?.Clear();

            mf.FileSaveTracks();

            if (mf.isBtnAutoSteerOn) mf.btnAutoSteer.PerformClick();
            if (mf.yt.isYouTurnBtnOn) mf.btnAutoYouTurn.PerformClick();

            mf.ABLine.isMakingABLine = false;
            mf.curve.desList?.Clear();
            mf.trk.idx = idx;

            Close();
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