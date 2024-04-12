﻿//Please, if you use this, share the improvements

using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace AgOpenGPS
{
    public class CWorldGrid
    {
        private readonly FormGPS mf;

        //Y
        public double northingMax;

        public double northingMin;

        //X
        public double eastingMax;

        public double eastingMin;

        //Y
        public double northingMaxGeo;

        public double northingMinGeo;

        //X
        public double eastingMaxGeo;

        public double eastingMinGeo;

        //Y
        public double northingMaxRate;

        public double northingMinRate;

        //X
        public double eastingMaxRate;

        public double eastingMinRate;

        public double GridSize = 6000;
        public double Count = 40;
        public bool isGeoMap = false;
        public bool isRateMap = false, isRateTrigger = false;
        public int numRateChannels = 1;

        public CWorldGrid(FormGPS _f)
        {
            mf = _f;

            northingMaxGeo = 300;
            northingMinGeo = -300;
            eastingMaxGeo = 300;
            eastingMinGeo = -300;
            northingMaxRate = 300;
            northingMinRate = -300;
            eastingMaxRate = 300;
            eastingMinRate = -300;
        }

        public void DrawFieldSurface()
        {
            Color field = mf.fieldColorDay;
            if (!mf.isDay) field = mf.fieldColorNight;

            if (isRateMap)
            {
                GL.Enable(EnableCap.Texture2D);

                if (mf.camera.zoomValue > 100) Count = 4;
                else if (mf.camera.zoomValue > 80) Count = 8;
                else if (mf.camera.zoomValue > 50) Count = 16;
                else if (mf.camera.zoomValue > 20) Count = 32;
                else if (mf.camera.zoomValue > 10) Count = 64;
                else Count = 80;

                GL.Enable(EnableCap.Texture2D);
                GL.Color3(field.R, field.G, field.B);
                GL.BindTexture(TextureTarget.Texture2D, mf.texture[(int)FormGPS.textures.Floor]);
                GL.Begin(PrimitiveType.TriangleStrip);

                GL.TexCoord2(0, 0);
                GL.Vertex3(eastingMin, northingMax, -2.10);
                GL.TexCoord2(Count, 0.0);
                GL.Vertex3(eastingMax, northingMax, -2.10);
                GL.TexCoord2(0.0, Count);
                GL.Vertex3(eastingMin, northingMin, -2.10);
                GL.TexCoord2(Count, Count);
                GL.Vertex3(eastingMax, northingMin, -2.10);

                GL.End();

                GL.Enable(EnableCap.Blend);

                GL.Color4(1.0f, 1.0f, 1.0f, 0.5f);
                GL.BindTexture(TextureTarget.Texture2D, mf.texture[(int)FormGPS.textures.RateMap1]);
                GL.Begin(PrimitiveType.TriangleStrip);

                GL.TexCoord2(0, 0);
                GL.Vertex3(eastingMinRate, northingMaxRate, -1.05);
                GL.TexCoord2(1, 0);
                GL.Vertex3(eastingMaxRate, northingMaxRate, -1.05);
                GL.TexCoord2(0, 1);
                GL.Vertex3(eastingMinRate, northingMinRate, -1.05);
                GL.TexCoord2(1, 1);
                GL.Vertex3(eastingMaxRate, northingMinRate, -1.05);

                GL.End();

                GL.Disable(EnableCap.Blend);

                //GL.BindTexture(TextureTarget.Texture2D, mf.texture[(int)FormGPS.textures.RateMap2]);
                //GL.Begin(PrimitiveType.TriangleStrip);

                //GL.TexCoord2(0, 0);
                //GL.Vertex3(eastingMinGeo, northingMaxGeo, -0.045);
                //GL.TexCoord2(1, 0);
                //GL.Vertex3(eastingMaxGeo, northingMaxGeo, -0.045);
                //GL.TexCoord2(0, 1);
                //GL.Vertex3(eastingMinGeo, northingMinGeo, -0.045);
                //GL.TexCoord2(1, 1);
                //GL.Vertex3(eastingMaxGeo, northingMinGeo, -0.045);

                //GL.End();
                //GL.BindTexture(TextureTarget.Texture2D, mf.texture[(int)FormGPS.textures.RateMap1]);
                //GL.Begin(PrimitiveType.TriangleStrip);

                //GL.TexCoord2(0, 0);
                //GL.Vertex3(eastingMinGeo, northingMaxGeo, -0.035);
                //GL.TexCoord2(1, 0);
                //GL.Vertex3(eastingMaxGeo, northingMaxGeo, -0.035);
                //GL.TexCoord2(0, 1);
                //GL.Vertex3(eastingMinGeo, northingMinGeo, -0.035);
                //GL.TexCoord2(1, 1);
                //GL.Vertex3(eastingMaxGeo, northingMinGeo, -0.035);

                //GL.End();
                GL.Disable(EnableCap.Texture2D);
                //GL.Disable(EnableCap.Blend);

            }
            else
            {
                //adjust bitmap zoom based on cam zoom
                if (mf.camera.zoomValue > 100) Count = 4;
                else if (mf.camera.zoomValue > 80) Count = 8;
                else if (mf.camera.zoomValue > 50) Count = 16;
                else if (mf.camera.zoomValue > 20) Count = 32;
                else if (mf.camera.zoomValue > 10) Count = 64;
                else Count = 80;

                GL.Color3(field.R, field.G, field.B);
                if (mf.isTextureOn)
                {
                    GL.Enable(EnableCap.Texture2D);
                    GL.BindTexture(TextureTarget.Texture2D, mf.texture[(int)FormGPS.textures.Floor]);
                }

                GL.Begin(PrimitiveType.TriangleStrip);
                    GL.TexCoord2(0, 0);
                    GL.Vertex3(eastingMin, northingMax, -0.10);
                    GL.TexCoord2(Count, 0.0);
                    GL.Vertex3(eastingMax, northingMax, -0.10);
                    GL.TexCoord2(0.0, Count);
                    GL.Vertex3(eastingMin, northingMin, -0.10);
                    GL.TexCoord2(Count, Count);
                    GL.Vertex3(eastingMax, northingMin, -0.10);
                GL.End();

                if (isGeoMap)
                {
                    GL.BindTexture(TextureTarget.Texture2D, mf.texture[(int)FormGPS.textures.bingGrid]);
                    GL.Begin(PrimitiveType.TriangleStrip);
                        GL.Color4(0.6f, 0.6f, 0.6f, 0.5f);
                        GL.TexCoord2(0, 0);
                        GL.Vertex3(eastingMinGeo, northingMaxGeo, -0.05);
                        GL.TexCoord2(1, 0.0);
                        GL.Vertex3(eastingMaxGeo, northingMaxGeo, -0.05);
                        GL.TexCoord2(0.0, 1);
                        GL.Vertex3(eastingMinGeo, northingMinGeo, -0.05);
                        GL.TexCoord2(1, 1);
                        GL.Vertex3(eastingMaxGeo, northingMinGeo, -0.05);
                    GL.End();
                }
                GL.Disable(EnableCap.Texture2D);
            }
        }

        public void DrawWorldGrid(double _gridZoom)
        {
            _gridZoom *= 0.5;

            if (mf.isDay)
            {
                GL.Color3(0.35, 0.35, 0.35);
            }
            else
            {
                GL.Color3(0.17, 0.17, 0.17);
            }
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.Lines);
            for (double num = Math.Round(eastingMin / _gridZoom, MidpointRounding.AwayFromZero) * _gridZoom; num < eastingMax; num += _gridZoom)
            {
                if (num < eastingMin) continue;

                GL.Vertex3(num, northingMax, 0.1);
                GL.Vertex3(num, northingMin, 0.1);
            }
            for (double num2 = Math.Round(northingMin / _gridZoom, MidpointRounding.AwayFromZero) * _gridZoom; num2 < northingMax; num2 += _gridZoom)
            {
                if (num2 < northingMin) continue;

                GL.Vertex3(eastingMax, num2, 0.1);
                GL.Vertex3(eastingMin, num2, 0.1);
            }
            GL.End();
        }

        public void checkZoomWorldGrid(double northing, double easting)
        {
            double n = Math.Round(northing / (GridSize / Count * 2), MidpointRounding.AwayFromZero) * (GridSize / Count * 2);
            double e = Math.Round(easting / (GridSize / Count * 2), MidpointRounding.AwayFromZero) * (GridSize / Count * 2);

            northingMax = n + GridSize;
            northingMin = n - GridSize;
            eastingMax = e + GridSize;
            eastingMin = e - GridSize;
        }
    }
}