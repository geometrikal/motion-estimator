using Emgu.CV;
using MotionEstimator.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionEstimator.Model.Camera
{
    public class CameraFrame : DisposableObject
    {
        public DateTime Timestamp { get; private set; }
        public Mat Image { get; private set; }

        public CameraFrame(Mat mat, DateTime time)
        {
            Image = mat;
            Timestamp = time;
        }

        public override void OnDispose()
        {
            Debug.WriteLine("Image disposed");
            Image?.Dispose();
        }

        public CameraFrame Clone()
        {
            return new CameraFrame(Image.Clone(), Timestamp);
        }
    }
}
