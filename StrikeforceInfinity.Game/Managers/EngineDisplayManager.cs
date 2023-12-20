﻿using StrikeforceInfinity.Game.Engine;
using StrikeforceInfinity.Game.Engine.Types;
using StrikeforceInfinity.Game.Engine.Types.Geometry;
using StrikeforceInfinity.Game.Utility;
using StrikeforceInfinity.Game.Utility.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace StrikeforceInfinity.Game.Managers
{
    /// <summary>
    /// Various metrics related to display.
    /// </summary>
    internal class EngineDisplayManager
    {
        private readonly EngineCore _gameCore;

        public Dictionary<Point, HgQuadrant> Quadrants { get; private set; } = new();
        public SiPoint BackgroundOffset { get; private set; } = new();
        public SiFrameCounter GameLoopCounter { get; private set; } = new();
        public Control DrawingSurface { get; private set; }

        public bool IsDrawingSurfaceFocused()
        {
            if (DrawingSurface.InvokeRequired)
            {
                bool result = false;
                DrawingSurface.Invoke(new Action(() =>
                {
                    result = DrawingSurface.Focused;
                }));
                return result;
            }
            return DrawingSurface.Focused;
        }

        public double OverrideSpeedOrientedFrameScalingFactor { get; set; } = double.NaN;

        public double SpeedOrientedFrameScalingFactor()
        {
            if (OverrideSpeedOrientedFrameScalingFactor is not double.NaN)
            {
                return OverrideSpeedOrientedFrameScalingFactor;
            }

            double weightedThrottlePercent = (
                    _gameCore.Player.Sprite.Velocity.ThrottlePercentage * 0.60 //n-percent of the zoom is throttle.
                    + _gameCore.Player.Sprite.Velocity.BoostPercentage * 0.40  //n-percent of the zoom is boost.
                ).Box(0, 1);

            double remainingRatioZoom = 1 - BaseDrawScale;
            double debugFactor = remainingRatioZoom * weightedThrottlePercent;
            return BaseDrawScale + debugFactor;
        }

        public double BaseDrawScale => 100.0 / _gameCore.Settings.OverdrawScale / 100.0;

        /// <summary>
        /// The number of extra pixles to draw beyond the NatrualScreenSize.
        /// </summary>
        public Size OverdrawSize { get; private set; }

        /// <summary>
        /// The total size of the rendering surface (no scaling).
        /// </summary>
        public Size TotalCanvasSize { get; private set; }

        /// <summary>
        /// The size of the screen with no scaling.
        /// </summary>
        public Size NatrualScreenSize { get; private set; }

        /// <summary>
        /// The bounds of the screen with no scaling.
        /// </summary>
        public RectangleF NatrualScreenBounds =>
            new RectangleF(OverdrawSize.Width / 2.0f, OverdrawSize.Height / 2.0f, NatrualScreenSize.Width, NatrualScreenSize.Height);

        /// <summary>
        /// The total bounds of the drawing surface (canvas) natrual + overdraw (with no scaling).
        /// </summary>
        public RectangleF TotalCanvasBounds => new RectangleF(0, 0, TotalCanvasSize.Width, TotalCanvasSize.Height);

        public RectangleF GetCurrentScaledScreenBounds()
        {
            var scale = SpeedOrientedFrameScalingFactor();

            if (scale < -1 || scale > 1)
            {
                throw new ArgumentException("Scale must be in the range [-1, 1].");
            }

            float centerX = TotalCanvasSize.Width * 0.5f;
            float centerY = TotalCanvasSize.Height * 0.5f;

            float smallerWidth = (float)(TotalCanvasSize.Width * scale);
            float smallerHeight = (float)(TotalCanvasSize.Height * scale);

            float left = centerX - smallerWidth * 0.5f;
            float top = centerY - smallerHeight * 0.5f;
            float right = smallerWidth;
            float bottom = smallerHeight;

            if (scale >= 0)
            {
                return new RectangleF(left, top, right, bottom);
            }
            else
            {
                //TODO: Zoom-in is untested.
                return new RectangleF(right, bottom, left, top);
            }
        }

        public SiPoint RandomOnScreenLocation()
        {
            var currentScaledScreenBounds = GetCurrentScaledScreenBounds();

            return new SiPoint(
                    HgRandom.Generator.Next((int)currentScaledScreenBounds.Left, (int)(currentScaledScreenBounds.Left + currentScaledScreenBounds.Width)),
                    HgRandom.Generator.Next((int)currentScaledScreenBounds.Top, (int)(currentScaledScreenBounds.Top + currentScaledScreenBounds.Height))
                );
        }

        public SiPoint RandomOffScreenLocation(int minOffscreenDistance = 100, int maxOffscreenDistance = 500)
        {
            if (HgRandom.FlipCoin())
            {
                if (HgRandom.FlipCoin())
                {
                    return new SiPoint(
                        -HgRandom.Between(minOffscreenDistance, maxOffscreenDistance),
                        HgRandom.Between(0, TotalCanvasSize.Height));
                }
                else
                {
                    return new SiPoint(
                        -HgRandom.Between(minOffscreenDistance, maxOffscreenDistance),
                        HgRandom.Between(0, TotalCanvasSize.Width));
                }
            }
            else
            {
                if (HgRandom.FlipCoin())
                {
                    return new SiPoint(
                        TotalCanvasSize.Width + HgRandom.Between(minOffscreenDistance, maxOffscreenDistance),
                        HgRandom.Between(0, TotalCanvasSize.Height));
                }
                else
                {
                    return new SiPoint(
                        TotalCanvasSize.Height + HgRandom.Between(minOffscreenDistance, maxOffscreenDistance),
                    HgRandom.Between(0, TotalCanvasSize.Width));
                }
            }
        }

        public EngineDisplayManager(EngineCore gameCore, Control drawingSurface, Size visibleSize)
        {
            _gameCore = gameCore;
            DrawingSurface = drawingSurface;
            NatrualScreenSize = visibleSize;

            int totalSizeX = (int)(visibleSize.Width * _gameCore.Settings.OverdrawScale);
            int totalSizeY = (int)(visibleSize.Height * _gameCore.Settings.OverdrawScale);

            if (totalSizeX % 2 != 0) totalSizeX++;
            if (totalSizeY % 2 != 0) totalSizeY++;

            TotalCanvasSize = new Size(totalSizeX, totalSizeY);

            OverdrawSize = new Size(totalSizeX - NatrualScreenSize.Width, totalSizeY - NatrualScreenSize.Height);
        }

        public HgQuadrant GetQuadrant(double x, double y)
        {
            var coord = new Point(
                    (int)(x / NatrualScreenSize.Width),
                    (int)(y / NatrualScreenSize.Height)
                );

            if (Quadrants.ContainsKey(coord) == false)
            {
                var absoluteBounds = new Rectangle(
                    NatrualScreenSize.Width * coord.X,
                    NatrualScreenSize.Height * coord.Y,
                    NatrualScreenSize.Width,
                    NatrualScreenSize.Height);

                var quad = new HgQuadrant(coord, absoluteBounds);

                Quadrants.Add(coord, quad);
            }

            return Quadrants[coord];
        }
    }
}
