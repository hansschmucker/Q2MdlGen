using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2MdlGen
{
    /**
     * Simple class to load images and save them as RGBA TGA
     **/
    class TGA
    {
        public byte[] RGBData;
        int Width;
        int Height;
        public byte[] AData;
        string Path;
        public unsafe void LoadImage(string path)
        {
            var rgb= new Bitmap(path, false);
            var data=rgb.LockBits(new Rectangle(0, 0, rgb.Width, rgb.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            RGBData = new byte[data.Width * data.Height * 3];
            var scan0 = (byte*)data.Scan0.ToPointer();
            for (var y = 0; y < data.Height; y++)
            {
                for (var x = 0; x < data.Width; x++)
                {
                    byte r = scan0[y * data.Stride + x * 3 + 0], g = scan0[y * data.Stride + x * 3 + 1], b=scan0[y * data.Stride + x * 3 + 2];
                    var d = ((data.Height-y-1) * data.Width + x) * 3;
                    RGBData[d + 0] = r;
                    RGBData[d + 1] = g;
                    RGBData[d + 2] = b;
                }
            }
            rgb.UnlockBits(data);

            Width = rgb.Width;
            Height = rgb.Height;
            Path = path;

            rgb.Dispose();
        }

        //Merges the RED channel of one TGA as alpha into the current one
        public void MergeAlpha(TGA Alpha)
        {
            AData = new byte[Width*Height];

            for (var i = 0; i < Width * Height; i++)
            {
                var rgbI = i * 3;
                AData[i] = Alpha.RGBData[rgbI];
            }
        }

        //Writes the TGA as RGB or RGBA to disk
        public void WriteTGA(string path)
        {
            var header = 19;
            var image = new byte[header + RGBData.Length + (AData != null ? AData.Length : 0)];
            image[0] = 1;
            image[2] = 2;

            System.BitConverter.GetBytes((UInt16)Width).CopyTo(image, 12);
            System.BitConverter.GetBytes((UInt16)Height).CopyTo(image, 14);
            image[16] = AData != null ? (byte)32 : (byte)24;
            image[17] = AData != null ? (byte)8 : (byte)0;

            if (AData == null)
                RGBData.CopyTo(image, header);
            else
            {
                for (var i = 0; i < AData.Length; i++)
                {
                    image[header + i * 4 + 0] = RGBData[i * 3 + 0];
                    image[header + i * 4 + 1] = RGBData[i * 3 + 1];
                    image[header + i * 4 + 2] = RGBData[i * 3 + 2];
                    image[header + i * 4 + 3] = AData[i];
                }
            }
            File.WriteAllBytes(path, image);


        }
    }
}
