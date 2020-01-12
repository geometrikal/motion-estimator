using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight;
using MotionEstimator.Model.Calculations;
using MotionEstimator.Model.Camera;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MotionEstimator.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private WriteableBitmap image;
        public WriteableBitmap Image
        {
            get => image;
            set => Set(ref image, value);
        }

        private double speed;
        public double Speed
        {
            get => speed;
            set => Set(ref speed, value);
        }

        private double direction;
        public double Direction
        {
            get => direction;
            set => Set(ref direction, value);
        }

        private CameraFrame previousFrame;

        public BaslerCamera Camera { get; set; } = new BaslerCamera();
        private SemaphoreSlim cameraImageGuiUpdateSemaphoreSlim = new SemaphoreSlim(1);
        public Velocity Velocity { get; set; } = new Velocity();

        public MainViewModel()
        {
            Image = new WriteableBitmap(BaslerCamera.Width, BaslerCamera.Height, 0, 0, PixelFormats.Bgr24, null);

            Camera.Gain = 0;
            Camera.Exposure = 3000;
            Camera.RedBalance = 1.8;
            Camera.GreenBalance = 1;
            Camera.BlueBalance = 1.4;
            Camera.CameraImageEvent += Camera_CameraImageEvent;

            Velocity.HorizontalPixels = BaslerCamera.Height;
            Velocity.HorizontalFieldOfView = 19.967;
            Velocity.DistanceToPlane = 1000;

            if (Camera.IsConnected) Camera.Stop();
            Camera.UpdateCameraList();
            Camera.Start();
            if (!Camera.IsConnected)
            {
                Debug.WriteLine("No camera to connect to...");
            }
        }

        public void OnClosing()
        {
            Debug.WriteLine("Closing...");
            Camera.Stop();
        }

        private async void Camera_CameraImageEvent(object sender, EventArgs e)
        {
            if (!BaslerCamera.Pause)
            {
                bool free;
                if (free = cameraImageGuiUpdateSemaphoreSlim.Wait(0))
                {
                    var frame = Camera.GetFrame();

                    if (previousFrame != null)
                    {
                        var timestep = frame.Timestamp - previousFrame.Timestamp;
                        Debug.WriteLine(timestep.TotalMilliseconds);

                        var g1 = new Mat();
                        var g2 = new Mat();
                        CvInvoke.Resize(frame.Image, g1, new Size(0, 0), 0.125, 0.125, Inter.Nearest);
                        CvInvoke.CvtColor(g1, g1, ColorConversion.Bgr2Gray);
                        CvInvoke.Resize(previousFrame.Image, g2, new Size(0, 0), 0.125, 0.125, Inter.Nearest);
                        CvInvoke.CvtColor(g2, g2, ColorConversion.Bgr2Gray);
                        var shift = await Task.Run(() =>
                        {
                            return g2.CalculatePhaseCorrection(g1);
                        });
                        g1.Dispose();
                        g2.Dispose();
                        (Speed, Direction) = Velocity.CalculateVelocity(shift.X, shift.Y, timestep.TotalMilliseconds);
                        Debug.WriteLine($"{shift.X}, {shift.Y}");
                        CvInvoke.Line(frame.Image, new Point(frame.Image.Cols / 2, frame.Image.Rows / 2), new Point(frame.Image.Cols / 2 + (int)shift.X*4, frame.Image.Rows / 2 + (int)shift.Y*4), new MCvScalar(0, 0, 255), 30);
                    }
                    previousFrame?.Dispose();
                    previousFrame = frame;
                    frame.Image.CopyToWriteableBitmap(image);
                    cameraImageGuiUpdateSemaphoreSlim.Release();
                }
            }
        }
    }
}