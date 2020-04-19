using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using RDRNetwork.Gui.DirectXHook;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace RDRNetwork.Gui
{
    public class ImageElement
    {
        internal SharpDX.Direct2D1.Bitmap D2D1Bitmap;

        public ImageElement()
        {
            DxHook.ImageElements.Add(this);
        }

        public ImageElement(string file, PointF position, float? width = null, float? height = null)
        {
            FromFile(file, position, width, height);
            DxHook.ImageElements.Add(this);
        }

        public static ImageElement FromFile(string file, PointF position, float? width = null, float? height = null)
        {
            var imageElement = new ImageElement();

            // Loads from file using System.Drawing.Image
            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(file))
            {
                imageElement.SetBitmap(bitmap, position, width, height);
            }
            return imageElement;
        }

        internal void SetBitmap(System.Drawing.Bitmap bitmap, PointF position, float? width = null, float? height = null)
        {
            System.Drawing.Rectangle sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapProperties bitmapProperties = new BitmapProperties(new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
            Size2 size = new Size2(bitmap.Width, bitmap.Height);

            // Transform pixels from BGRA to RGBA
            int stride = bitmap.Width * sizeof(int);
            using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
            {
                // Lock System.Drawing.Bitmap
                var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                // Convert all pixels 
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int offset = bitmapData.Stride * y;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        // Not optimized 
                        byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        int rgba = R | (G << 8) | (B << 16) | (A << 24);
                        tempStream.Write(rgba);
                    }

                }
                bitmap.UnlockBits(bitmapData);
                tempStream.Position = 0;

                Position = position;
                Height = height == null ? bitmap.Height : height.Value;
                Width = width == null ? bitmap.Width : width.Value;

                if (D2D1Bitmap == null)
                    D2D1Bitmap = new SharpDX.Direct2D1.Bitmap(DirectXHook.DxHook.CurrentRenderTarget, size, tempStream, stride, bitmapProperties);
                else
                {
                    D2D1Bitmap.CopyFromStream(tempStream, stride, 0);
                }
            }
        }

        internal void Dispose()
        {
            D2D1Bitmap = null;
            DxHook.ImageElements.Remove(this);
        }

        public PointF Position;
        public float Height;
        public float Width;
        public bool Hidden;

        public void Draw()
        {
            if (D2D1Bitmap == null || Hidden)
                return;

            // Draw the TextLayout
            //renderTarget.DrawBitmap(_bitmap, 1.0f, BitmapInterpolationMode.Linear);
            DirectXHook.DxHook.CurrentRenderTarget.DrawBitmap(D2D1Bitmap, new SharpDX.Mathematics.Interop.RawRectangleF(Position.X, Position.Y, Position.X + Width, Position.Y + Height), 1.0f, BitmapInterpolationMode.NearestNeighbor);
            //new SharpDX.Mathematics.Interop.RawRectangleF(10, 10, 2000, 500)
        }
    }
}
