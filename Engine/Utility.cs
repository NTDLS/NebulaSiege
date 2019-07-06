﻿using AI2D.GraphicObjects;
using AI2D.Types;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AI2D.Engine
{
    public class Utility
    {
        const double RADIAN_CONV = Math.PI / 180.0;
        const double OFFSET90DEGREES = 180.0 * RADIAN_CONV;
        const double FULLCIRCLE = 360.0 * RADIAN_CONV;

        #region Graphics.

        public static Bitmap RotateImageWithClipping(Bitmap bmp, double angle)
        {
            Bitmap rotatedImage = new Bitmap(bmp.Width, bmp.Height);
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                // Set the rotation point to the center in the matrix
                g.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
                // Rotate
                g.RotateTransform((float)angle);
                // Restore rotation point in the matrix
                g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2);
                // Draw the image on the bitmap
                g.DrawImage(bmp, new Point(0, 0));
            }

            return rotatedImage;
        }
        public static Bitmap RotateImage(Bitmap b, double angle)
        {
            if (angle > 0)
            {
                int l = b.Width;
                int h = b.Height;
                double an = angle * Math.PI / 180;
                double cos = Math.Abs(Math.Cos(an));
                double sin = Math.Abs(Math.Sin(an));
                int nl = (int)(l * cos + h * sin);
                int nh = (int)(l * sin + h * cos);
                Bitmap returnBitmap = new Bitmap(nl, nh);
                Graphics g = Graphics.FromImage(returnBitmap);
                g.TranslateTransform((float)(nl - l) / 2, (float)(nh - h) / 2);
                g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
                g.RotateTransform((float)angle);
                g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
                g.DrawImage(b, new Point(0, 0));
                return returnBitmap;
            }
            else return b;
        }


        public static Image ResizeImage(Image image, int new_height, int new_width)
        {
            Bitmap new_image = new Bitmap(new_width, new_height);
            Graphics g = Graphics.FromImage((Image)new_image);
            g.InterpolationMode = InterpolationMode.High;
            g.DrawImage(image, 0, 0, new_width, new_height);
            return new_image;
        }

        #endregion

        #region Math.

        public static double RequiredAngleTo(BaseGraphicObject from, BaseGraphicObject to)
        {
            return RequiredAngleTo(from.Location, to.Location);
        }

        public static double RequiredAngleTo(PointD from, PointD to)
        {
            var fRadians = Math.Atan2((to.Y - from.Y), (to.X - from.X));
            var fDegrees = ((fRadians * (180 / Math.PI) + 360) + 90) % 360;
            return fDegrees;
        }

        public static bool IsPointingAt(BaseGraphicObject fromObj, BaseGraphicObject atObj, double toleranceDegrees)
        {
            var deltaAngle = Math.Abs(GetDeltaAngle(fromObj, atObj));
            return deltaAngle < toleranceDegrees;
        }

        public static double GetDeltaAngle(BaseGraphicObject fromObj, BaseGraphicObject atObj)
        {
            double angleTo = RequiredAngleTo(fromObj, atObj);

            if (fromObj.Velocity.Angle.Degree < 0) fromObj.Velocity.Angle.Degree = (0 - fromObj.Velocity.Angle.Degree);
            if (angleTo < 0) angleTo = (0 - angleTo);

            return fromObj.Velocity.Angle.Degree - angleTo;
        }

        public static PointD AngleToXY(double angle)
        {
            double radians = (Math.PI / 180) * (angle - 90);

            PointD result = new PointD()
            {
                X = Math.Cos(radians),
                Y = Math.Sin(-radians)
            };

            return result;
        }

        public static double CalculeDistance(PointD from, PointD to)
        {
            var deltaX = Math.Pow((to.X - from.X), 2);
            var deltaY = Math.Pow((to.Y - from.Y), 2);

            var distance = Math.Sqrt(deltaY + deltaX);

            return distance;
        }

        public static double CalculeDistance(BaseGraphicObject from, BaseGraphicObject to)
        {
            return CalculeDistance(from.Location, to.Location);
        }

        #endregion

        #region Random.

        public static Random Random = new Random();
        public static bool FlipCoin()
        {
            return Random.Next(0, 1000) >= 500;
        }

        public static Double RandomNumber(double min, double max)
        {
            return Random.Next(0, 1000) % max;
        }

        public static int RandomNumber(int min, int max)
        {
            return Random.Next(0, 1000) % max;
        }

        #endregion

    }
}

