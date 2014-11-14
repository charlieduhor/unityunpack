
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
/*
using System.Windows.Media;
using System.Windows.Media.Imaging;
*/
namespace UnityUnpack {
    public enum TextureFormat {
        Alpha8          = 1,
        ARGB4444        = 2,
        RGB24           = 3,
        RGBA32          = 4,
        ARGB32          = 5,
        RGB565          = 7,
        DXT1            = 10,
        DXT5            = 12,
        RGBA4444        = 13,
        PVRTC_2BPP_RGB  = 30,
        PVRTC_2BPP_RGBA = 31,
        PVRTC_4BPP_RGB  = 32,
        PVRTC_4BPP_RGBA = 33,
        ETC_RGB4        = 34,
        ATC_RGB4        = 35,
        ATC_RGBA8       = 36,
        BGRA32          = 37,
        ATF_RGB_DXT1    = 38,
        ATF_RGBA_JPG    = 39,
        ATF_RGB_JPG     = 40
    }

    public class TextureAsset : Asset {
        public UInt32        Width;
        public UInt32        Height;
        public UInt32        TextureSize;
        public TextureFormat TextureType;
        public UInt32        Unknown1;
        public UInt32        Unknown2;
        public UInt32        Unknown3;
        public UInt32        Unknown4;
        public UInt32        Unknown5;
        public UInt32        Unknown6;
        public UInt32        Unknown7;
        public UInt32        Unknown8;
        public UInt32        Unknown9;
        public UInt32        Size2;

        public TextureAsset(Asset asset) : base(asset) {
        }

        public void Parse(Stream stream) {
            BinaryReader reader = new BinaryReader(stream);

            Width       = reader.ReadUInt32();
            Height      = reader.ReadUInt32();
            TextureSize = reader.ReadUInt32();
            TextureType = (TextureFormat)reader.ReadUInt32();
            Unknown1    = reader.ReadUInt32();
            Unknown2    = reader.ReadUInt32();
            Unknown3    = reader.ReadUInt32();
            Unknown4    = reader.ReadUInt32();
            Unknown5    = reader.ReadUInt32();
            Unknown6    = reader.ReadUInt32();
            Unknown7    = reader.ReadUInt32();
            Unknown8    = reader.ReadUInt32();
            Unknown9    = reader.ReadUInt32();
            Size2       = reader.ReadUInt32();
        }

        public static void ARGB_To_BGRA32(byte[] pixels) {
            if (pixels.Length % 4 != 0) {
                throw new Exception("Invalid data");
            }

            for (int i = 0; i < pixels.Length; i += 4) {
                byte r, g, b, a;

                a = pixels[i + 0];
                r = pixels[i + 1];
                g = pixels[i + 2];
                b = pixels[i + 3];

                pixels[i + 0] = b;
                pixels[i + 1] = g;
                pixels[i + 2] = r;
                pixels[i + 3] = a;
            }
        }

        public static void RGBA_To_BGRA32(byte[] pixels) {
            if (pixels.Length % 4 != 0) {
                throw new Exception("Invalid data");
            }

            for (int i = 0; i < pixels.Length; i += 4) {
                byte r, g, b, a;

                r = pixels[i + 0];
                g = pixels[i + 1];
                b = pixels[i + 2];
                a = pixels[i + 3];

                pixels[i + 0] = b;
                pixels[i + 1] = g;
                pixels[i + 2] = r;
                pixels[i + 3] = a;
            }
        }

        public static void RGB24_To_BGR24(byte[] pixels) {
            if (pixels.Length % 3 != 0) {
                throw new Exception("Invalid data");
            }

            for (int i = 0; i < pixels.Length; i += 3) {
                byte r, g, b;

                r = pixels[i + 0];
                g = pixels[i + 1];
                b = pixels[i + 2];

                pixels[i + 0] = b;
                pixels[i + 1] = g;
                pixels[i + 2] = r;
            }
        }

        public static byte four_to_eight(byte b) {
            float f = (b & 0xf);

            f /= 15.0f;
            f *= 255.0f;

            return (byte)(int)f;
        }

        public static void RGBA4444_To_BGRA32(byte[] pixels) {
            if (pixels.Length % 4 != 0) {
                throw new Exception("Invalid data");
            }

            int i = pixels.Length - 4;
            int j = (pixels.Length / 2) - 2;

            for (; i >= 0 && j >= 2; i -= 4, j -= 2) {
                byte r, g, b, a;

                b   = pixels[j + 0];
                a   = (byte)(b & 0xf);
                b >>= 4;

                r   = pixels[j + 1];
                g   = (byte)(r & 0xf);
                r >>= 4;

                pixels[i + 0] = four_to_eight(b);
                pixels[i + 1] = four_to_eight(g);
                pixels[i + 2] = four_to_eight(r);
                pixels[i + 3] = four_to_eight(a);
            }
        }

        public static void ARGB4444_To_BGRA32(byte[] pixels) {
            if (pixels.Length % 4 != 0) {
                throw new Exception("Invalid data");
            }

            int i = pixels.Length - 4;
            int j = (pixels.Length / 2) - 2;

            for (; i >= 0 && j >= 2; i -= 4, j -= 2) {
                byte r, g, b, a;

                g = pixels[j + 0];
                b = (byte)(g & 0xf);
                g >>= 4;

                a = pixels[j + 1];
                r = (byte)(a & 0xf);
                a >>= 4;

                pixels[i + 0] = four_to_eight(b);
                pixels[i + 1] = four_to_eight(g);
                pixels[i + 2] = four_to_eight(r);
                pixels[i + 3] = four_to_eight(a);
            }
        }

        public const int TEXTURE_HEADER_SIZE = 14 * 4;

        public override void Extract(Stream inStream, Stream outStream, UInt32 length) {
            PixelFormat format;
            UInt32      inputStride;
            UInt32      outputStride;
            byte[]      array;
            byte[]      arrayInput = null;
            int         arrayInputSize = 0;

            switch (TextureType) {
            case TextureFormat.RGB24:
                format      = PixelFormat.Format24bppRgb;
                inputStride = outputStride = Width * 3;
                break;

            case TextureFormat.RGBA4444:
            case TextureFormat.ARGB4444:
                format = PixelFormat.Format32bppArgb;
                inputStride  = Width * 2;
                outputStride = Width * 4;
                break;

            case TextureFormat.RGB565:
                format      = PixelFormat.Format16bppRgb565;
                inputStride = outputStride = Width * 2;
                break;

            default:
                throw new Exception("Unsupported pixel format");

            case TextureFormat.RGBA32:
            case TextureFormat.ARGB32:
                format      = PixelFormat.Format32bppArgb;
                inputStride = outputStride = Width * 4;
                break;

            case TextureFormat.DXT1:
            case TextureFormat.DXT5:
                format         = PixelFormat.Format32bppArgb;
                inputStride    = outputStride = Width * 4;
                arrayInput     = new byte[this.ActualSize - TEXTURE_HEADER_SIZE];
                arrayInputSize = arrayInput.Length;
                break;
            }

            // Skip header. We already readed it.
            inStream.Position += TEXTURE_HEADER_SIZE;
            array = new byte[outputStride * Height];

            if (arrayInput == null) {
                arrayInput     = array;
                arrayInputSize = (int)(inputStride * Height);
            }

            int readed = inStream.Read(arrayInput, 0, arrayInputSize);
            
            switch (TextureType) {
            case TextureFormat.ARGB32:
                ARGB_To_BGRA32(array);
                break;
            case TextureFormat.RGBA32:
                RGBA_To_BGRA32(array);
                break;
            case TextureFormat.RGBA4444:
                RGBA4444_To_BGRA32(array);
                break;
            case TextureFormat.ARGB4444:
                ARGB4444_To_BGRA32(array);
                break;
            case TextureFormat.RGB24:
                RGB24_To_BGR24(array);
                break;
            case TextureFormat.DXT1:
                DXT1.Decompress(Width, Height, arrayInput, array);
                break;
            case TextureFormat.DXT5:
                DXT5.Decompress(Width, Height, arrayInput, array);
                break;
            default:
                break;
            }


            Bitmap     bitmap = new Bitmap((int)Width, (int)Height, format);
            BitmapData data   = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, format);

            IntPtr scan = data.Scan0;
            UInt32 end = ((UInt32)array.Length) - outputStride;

            for (int y = 0; y < Height; y++) {
                Marshal.Copy(array, (int)end, scan, (int)outputStride);
                scan = (IntPtr)((UInt64)scan + outputStride);
                end -= outputStride;
            }

            bitmap.UnlockBits(data);
            bitmap.Save(outStream, ImageFormat.Png);
        }
    }
}
