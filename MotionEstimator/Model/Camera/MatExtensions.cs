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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MotionEstimator.Model.Camera
{
    public static class MatExtensions
    {
        public static WriteableBitmap CreateWriteableBitmap(this Mat mat, double scale)
        {
            if (mat.Depth != DepthType.Cv8U)
            {
                Mat temp = mat.Clone();
                if (scale != 1.0) temp.Multiply(scale);
                temp.ConvertTo(temp, DepthType.Cv8U);
                var wb = temp.CreateWriteableBitmap();
                temp.Dispose();
                return wb;
            }
            else
            {
                return mat.CreateWriteableBitmap();
            }
        }

        public static WriteableBitmap CreateWriteableBitmap(this Mat mat)
        {
            WriteableBitmap seg;
            if (mat.NumberOfChannels > 1)
            {
                seg = new WriteableBitmap(mat.Width, mat.Height, 0, 0, PixelFormats.Bgr24, null);
                seg.Lock();
                seg.WritePixels(new System.Windows.Int32Rect(0, 0, mat.Width, mat.Height), mat.DataPointer, mat.Width * mat.Height * mat.NumberOfChannels, mat.Width * mat.NumberOfChannels);
                seg.AddDirtyRect(new System.Windows.Int32Rect(0, 0, seg.PixelWidth, seg.PixelHeight));
                seg.Unlock();
            }
            else
            {
                Mat temp = new Mat();
                CvInvoke.CvtColor(mat, temp, ColorConversion.Gray2Bgr);
                seg = new WriteableBitmap(temp.Width, temp.Height, 0, 0, PixelFormats.Bgr24, null);
                seg.Lock();
                seg.WritePixels(new System.Windows.Int32Rect(0, 0, temp.Width, temp.Height), temp.DataPointer, temp.Width * temp.Height * temp.NumberOfChannels, temp.Width * temp.NumberOfChannels);
                seg.AddDirtyRect(new System.Windows.Int32Rect(0, 0, seg.PixelWidth, seg.PixelHeight));
                seg.Unlock();
                temp.Dispose();
            }
            return seg;
        }

        public static void CopyToWriteableBitmap(this Mat mat, WriteableBitmap seg)
        {
            seg.Lock();
            seg.WritePixels(new System.Windows.Int32Rect(0, 0, mat.Width, mat.Height), mat.DataPointer, mat.Width * mat.Height * mat.NumberOfChannels, mat.Width * mat.NumberOfChannels);
            seg.AddDirtyRect(new System.Windows.Int32Rect(0, 0, seg.PixelWidth, seg.PixelHeight));
            seg.Unlock();
        }

        public static Mat ToMat(this BitmapSource source)
        {
            if (source.Format == PixelFormats.Bgra32)
            {
                Mat result = new Mat();
                result.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 4);
                source.CopyPixels(System.Windows.Int32Rect.Empty, result.DataPointer, result.Step * result.Rows, result.Step);
                return result;
            }
            else if (source.Format == PixelFormats.Bgr24)
            {
                Mat result = new Mat();
                result.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 3);
                source.CopyPixels(System.Windows.Int32Rect.Empty, result.DataPointer, result.Step * result.Rows, result.Step);
                return result;
            }
            else if (source.Format == PixelFormats.Pbgra32)
            {
                Mat result = new Mat();
                result.Create(source.PixelHeight, source.PixelWidth, DepthType.Cv8U, 4);
                source.CopyPixels(System.Windows.Int32Rect.Empty, result.DataPointer, result.Step * result.Rows, result.Step);
                return result;
            }
            else
            {
                throw new Exception(String.Format("Conversion from BitmapSource of format {0} is not supported.", source.Format));
            }
        }

        public static void Multiply(this Mat mat, double value)
        {
            mat.ConvertTo(mat, mat.Depth, value, 0);
            //mat.ConvertTo(mat, mat.Depth, value, 0);
            //Mat op = new Mat(mat.Size, mat.Depth, mat.NumberOfChannels);
            //op.SetTo(new MCvScalar(value));
            //CvInvoke.Multiply(mat, op, mat);
            //op.Dispose();
        }

        public static void Divide(this Mat mat, double value)
        {
            mat.ConvertTo(mat, mat.Depth, 1.0 / value, 0);
            //mat.ConvertTo(mat, mat.Depth, value, 0);
            //Mat op = new Mat(mat.Size, mat.Depth, mat.NumberOfChannels);
            //op.SetTo(new MCvScalar(value));
            //CvInvoke.Multiply(mat, op, mat);
            //op.Dispose();
        }

        public static void Add(this Mat mat, double value)
        {
            mat.ConvertTo(mat, mat.Depth, 1, value);
            //mat.ConvertTo(mat, mat.Depth, 0, value);
            //Mat op = new Mat(mat.Size, mat.Depth, mat.NumberOfChannels);
            //op.SetTo(new MCvScalar(value));
            //CvInvoke.Add(mat, op, mat);
            //op.Dispose();
        }

        public static void Add(this Mat mat, double value0, double value1, double value2)
        {
            //mat.ConvertTo(mat, mat.Depth, 0, value);
            Mat op = new Mat(mat.Size, mat.Depth, mat.NumberOfChannels);
            op.SetTo(new MCvScalar(value0, value1, value2));
            CvInvoke.Add(mat, op, mat);
            op.Dispose();
        }

        public static void Subtract(this Mat mat, double value)
        {
            mat.Add(-value);
            //Mat op = new Mat(mat.Size, mat.Depth, mat.NumberOfChannels);
            //op.SetTo(new MCvScalar(value));
            //CvInvoke.Subtract(mat, op, mat);
            //op.Dispose();
        }

        public static void Subtract(this Mat mat, double value0, double value1, double value2)
        {
            Mat op = new Mat(mat.Size, mat.Depth, mat.NumberOfChannels);
            op.SetTo(new MCvScalar(value0, value1, value2));
            CvInvoke.Subtract(mat, op, mat);
            op.Dispose();
        }

        public static void MultiplyAdd(this Mat mat, double mult, double add)
        {
            mat.ConvertTo(mat, mat.Depth, mult, add);
        }

        public static void AbsDiff(this Mat mat, double value)
        {
            Mat op = new Mat(mat.Size, mat.Depth, mat.NumberOfChannels);
            op.SetTo(new MCvScalar(value));
            CvInvoke.AbsDiff(mat, op, mat);
            op.Dispose();
        }

        public static void Translate(this Mat mat, PointF offset, BorderType borderMode, MCvScalar borderValue = default(MCvScalar))
        {
            //Translation matrix
            float[] T = new float[6] { 1, 0, -offset.X, 0, 1, -offset.Y };
            Mat Tmat = Mat.Zeros(2, 3, DepthType.Cv32F, 1);
            Marshal.Copy(T, 0, Tmat.DataPointer, 6);
            //Apply
            CvInvoke.WarpAffine(mat, mat, Tmat, mat.Size, borderMode: borderMode, borderValue: borderValue);
            Tmat.Dispose();
        }

        public static Mat Crop(this Mat mat, Rectangle cropRect)
        {
            Mat cropTemp = new Mat(mat, cropRect);
            Mat crop = new Mat();
            cropTemp.CopyTo(crop);
            cropTemp.Dispose();
            return crop;
        }

        public static float[] CopyAsFloatArray(this Mat mat)
        {
            if (mat.Depth != DepthType.Cv32F) throw new ArgumentException(nameof(mat), "Mat must be float (DepthType.Cv32F)");
            float[] array = new float[mat.Height * mat.Width * mat.NumberOfChannels];
            mat.CopyTo(array);

            return array;
        }

        public static byte[] CopyAsByteArray(this Mat mat)
        {
            if (mat.Depth != DepthType.Cv8U) throw new ArgumentException(nameof(mat), "Mat must be byte (DepthType.Cv8U)");
            byte[] array = new byte[mat.Height * mat.Width * mat.NumberOfChannels];
            mat.CopyTo(array);

            return array;
        }

        public static Mat SliceFromChannelIndex(this Mat mat, Mat index)
        {
            if (mat.Depth == DepthType.Cv8U) return SliceFromChannelIndex<byte>(mat, index);
            if (mat.Depth == DepthType.Cv32F) return SliceFromChannelIndex<float>(mat, index);
            return null;
        }

        private static Mat SliceFromChannelIndex<T>(Mat mat, Mat index)
        {
            var height = mat.Height;
            var width = mat.Width;
            var channels = mat.NumberOfChannels;

            //Index
            byte[] idxData = index.CopyAsByteArray();

            //Image
            T[] matData = new T[height * width * channels];
            mat.CopyTo(matData);

            //Output
            T[] outputData = new T[height * width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //Channel index value
                    var idx = idxData[i * width + j];

                    //Offset into input image
                    int offset = i * width * channels + j * channels;

                    //Assign values
                    outputData[i * width + j] = matData[offset + idx];
                }
            }

            Mat output = new Mat(height, width, mat.Depth, 1);
            output.SetTo(outputData);
            return output;
        }

        public static Mat InterpolateFromChannelIndex8U(this Mat mat, Mat index)
        {
            var height = mat.Height;
            var width = mat.Width;
            var channels = mat.NumberOfChannels;

            //Index
            float[] idxData = index.CopyAsFloatArray();

            //Image
            byte[] matData = new byte[height * width * channels];
            mat.CopyTo(matData);

            //Output
            float[] outputData = new float[height * width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //Channel index value
                    var idx = idxData[i * width + j];

                    //Floor and ceiling of index
                    int floor = (int)Math.Floor(idx);
                    int ceil = (int)Math.Ceiling(idx);

                    //Sanity check
                    if (ceil >= channels) ceil = channels - 1;
                    if (floor < 0) floor = 0;

                    //Fraction of the ceiling
                    var ceilFrac = (float)(idx - Math.Truncate(idx));

                    //Offset into input image
                    int offset = i * width * channels + j * channels;

                    //Assign values
                    outputData[i * width + j] = matData[offset + floor] * (1 - ceilFrac)
                                                + matData[offset + ceil] * ceilFrac;
                }
            }

            Mat output = new Mat(height, width, DepthType.Cv32F, 1);
            output.SetTo(outputData);
            output.ConvertTo(output, DepthType.Cv8U);
            return output;
        }

        public static Mat SliceFromListIndex(this List<Mat> mats, Mat index)
        {
            if (mats[0].Depth == DepthType.Cv8U) return SliceFromListIndex<byte>(mats, index);
            if (mats[0].Depth == DepthType.Cv32F) return SliceFromListIndex<float>(mats, index);
            return null;
        }

        private static Mat SliceFromListIndex<T>(List<Mat> mats, Mat index)
        {
            var height = index.Height;
            var width = index.Width;
            var channels = mats[0].NumberOfChannels;
            var count = mats.Count;

            //Index
            byte[] idxData = index.CopyAsByteArray();

            //Image
            T[][] matData = new T[count][];
            for (int i = 0; i < count; i++)
            {
                matData[i] = new T[height * width * channels];
                mats[i].CopyTo(matData[i]);
            }

            //Output
            T[] outputData = new T[height * width * channels];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //Channel index value
                    var idx = idxData[i * width + j];

                    //Offset into input image
                    int offset = i * width * channels + j * channels;

                    //Assign values
                    for (int k = 0; k < channels; k++)
                    {
                        outputData[offset + k] = matData[idx][offset + k];
                    }
                }
            }

            Mat output = new Mat(height, width, mats[0].Depth, channels);
            output.SetTo(outputData);

            return output;
        }
    }
}
