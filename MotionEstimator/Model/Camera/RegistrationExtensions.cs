using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MotionEstimator.Model.Camera
{
    public static class RegistrationExtensions
    {
        public static PointF CalculatePhaseCorrectionOld(this Mat mat, Mat reference)
        {
            //Argument check
            if (mat.NumberOfChannels != 1) throw new ArgumentException("This mat must be single channel");
            if (reference.NumberOfChannels != 1) throw new ArgumentException("Reference Mat must be single channel");

            //Phase calculation
            Mat window = Mat.Ones(mat.Rows, mat.Cols, DepthType.Cv32F, 1);
            MCvPoint2D64f offset = CvInvoke.PhaseCorrelate(mat, reference, window, out double response);
            PointF offsetPoint = new PointF((float)-offset.X, (float)-offset.Y);
            return offsetPoint;
        }

        public static PointF CalculatePhaseCorrection(this Mat image1, Mat image2)
        {
            int cols = image1.Cols; // CvInvoke.GetOptimalDFTSize(image1.Cols);
            int rows = image1.Rows; // CvInvoke.GetOptimalDFTSize(image1.Rows);

            //Debug.WriteLine($"{cols} {rows}");
            Mat fft1 = new Mat();// rows, cols, DepthType.Cv32F, 1);
            Mat fft2 = new Mat();// rows, cols, DepthType.Cv32F, 1);

            image1.ConvertTo(fft1, DepthType.Cv32F);
            image2.ConvertTo(fft2, DepthType.Cv32F);
            //image1.CopyTo(fft1);
            //image2.CopyTo(fft2);

            CvInvoke.Dft(fft1, fft1, DxtType.Forward, image1.Rows);
            CvInvoke.Dft(fft2, fft2, DxtType.Forward, image2.Rows);
            CvInvoke.MulSpectrums(fft1, fft2, fft1, MulSpectrumsType.Default, true);
            CvInvoke.Dft(fft1, fft1, DxtType.Inverse, image1.Rows);
            double minVal = 0;
            double maxVal = 0;
            Point minLoc = new Point();
            Point maxLoc = new Point();
            CvInvoke.MinMaxLoc(fft1, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            //idft(fft1, fft1);
            //double maxVal;
            //Point maxLoc;
            var resX = (maxLoc.X < cols / 2) ? (maxLoc.X) : (maxLoc.X - cols);
            var resY = (maxLoc.Y < rows / 2) ? (maxLoc.Y) : (maxLoc.Y - rows);

            fft1.Dispose();
            fft2.Dispose();

            return new PointF(resX, resY);
        }

        public static void PhaseCorrect(this Mat mat, PointF offset)
        {
            Mat transformation = TranslationMatrix(offset);
            CvInvoke.WarpAffine(mat, mat, transformation, mat.Size, borderMode: BorderType.Replicate);
            transformation.Dispose();
        }

        public static Mat TranslationMatrix(PointF offset)
        {
            float[] T = new float[6] { 1, 0, -offset.X, 0, 1, -offset.Y };

            Mat Tmat = Mat.Zeros(2, 3, DepthType.Cv32F, 1);
            Marshal.Copy(T, 0, Tmat.DataPointer, 6);

            return Tmat;
        }

        public static Mat SortAlong3rdDimension(this Mat mat)
        {
            //Dimensions
            var width = mat.Width;
            var height = mat.Height;
            var channels = mat.NumberOfChannels;

            //Sort
            var stack = mat.Clone();
            stack = stack.Reshape(1, width * height);
            CvInvoke.Sort(stack, stack, SortFlags.SortEveryRow & SortFlags.SortDescending);
            stack = stack.Reshape(channels, height);

            return stack;
        }

        public static Mat ArgsortAlong3rdDimension(this Mat mat)
        {
            //Dimensions
            var width = mat.Width;
            var height = mat.Height;
            var channels = mat.NumberOfChannels;

            //Sort
            var stack = mat.Clone();
            var idx = new Mat(); //height * width, channels, DepthType.Cv16U, 1);
            stack = stack.Reshape(1, width * height);
            CvInvoke.SortIdx(stack, idx, SortFlags.SortEveryRow & SortFlags.SortDescending);
            //int[] values = new int[idx.Width * idx.Height];
            //idx.CopyTo(values);
            idx = idx.Reshape(channels, height);
            stack.Dispose();

            return idx;
        }
    }
}
