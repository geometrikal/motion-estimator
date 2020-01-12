using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionEstimator.Model.Calculations
{
    public class Velocity : ObservableObject
    {
        private double distanceToPlane;
        public double DistanceToPlane
        {
            get => distanceToPlane;
            set => Set(ref distanceToPlane, value);
        }

        private double horizontalFieldOfView;
        public double HorizontalFieldOfView
        {
            get => horizontalFieldOfView;
            set => Set(ref horizontalFieldOfView, value);
        }

        private double horizontalPixels;
        public double HorizontalPixels
        {
            get => horizontalPixels;
            set => Set(ref horizontalPixels, value);
        }

        public (double, double) CalculateVelocity(double px, double py, double milliseconds)
        {
            var pixelDistance = Math.Sqrt(Math.Pow(px, 2) + Math.Pow(py, 2));
            var horizontalDistance = (DistanceToPlane / 2) * Math.Cos(HorizontalFieldOfView / 180 * Math.PI / 2);

            var dist = pixelDistance / HorizontalPixels * horizontalDistance;
            var speed = dist / milliseconds;
            var angle = Math.Atan2(py, px) * 180 / Math.PI;

            return (speed, angle);
        }
    }
}
