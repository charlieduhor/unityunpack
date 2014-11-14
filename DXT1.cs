using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityUnpack {
    public class DXT1 {
        protected static UInt32 PackRGBA(byte r, byte g, byte b, byte a) {
            return (UInt32)((a << 24) | (r << 16) | (g << 8) | b);
        }

        public static void DecompressBlockDXT1(UInt32 x, UInt32 y, UInt32 width, UBytePtr blockStorage, UInt32Ptr image) {
            UInt16 color0 = blockStorage.asUInt16();
            UInt16 color1 = (blockStorage + 2).asUInt16();
 
            UInt32 temp;
 
            temp = (UInt32)((color0 >> 11) * 255 + 16);
            byte r0 = (byte)((temp/32 + temp)/32);
            temp = (UInt32)(((color0 & 0x07E0) >> 5) * 255 + 32);
            byte g0 = (byte)((temp/64 + temp)/64);
            temp = (UInt32)((color0 & 0x001F) * 255 + 16);
            byte b0 = (byte)((temp/32 + temp)/32);
 
            temp = (UInt32)((color1 >> 11) * 255 + 16);
            byte r1 = (byte)((temp/32 + temp)/32);
            temp = (UInt32)(((color1 & 0x07E0) >> 5) * 255 + 32);
            byte g1 = (byte)((temp/64 + temp)/64);
            temp = (UInt32)((color1 & 0x001F) * 255 + 16);
            byte b1 = (byte)((temp/32 + temp)/32);
 
            UInt32 code = (blockStorage + 4).asUInt32();
 
            for (int j=0; j < 4; j++)
            {
                for (int i=0; i < 4; i++)
                {
                    UInt32 finalColor = 0;
                    byte   positionCode = (byte)((code >>  2*(4*j+i)) & 0x03);
 
                    if (color0 > color1)
                    {
                        switch (positionCode)
                        {
                            case 0:
                                finalColor = PackRGBA(r0, g0, b0, 255);
                                break;
                            case 1:
                                finalColor = PackRGBA(r1, g1, b1, 255);
                                break;
                            case 2:
                                finalColor = PackRGBA((byte)((2*r0+r1)/3), (byte)((2*g0+g1)/3), (byte)((2*b0+b1)/3), 255);
                                break;
                            case 3:
                                finalColor = PackRGBA((byte)((r0+2*r1)/3), (byte)((g0+2*g1)/3), (byte)((b0+2*b1)/3), 255);
                                break;
                        }
                    }
                    else
                    {
                        switch (positionCode)
                        {
                            case 0:
                                finalColor = PackRGBA(r0, g0, b0, 255);
                                break;
                            case 1:
                                finalColor = PackRGBA(r1, g1, b1, 255);
                                break;
                            case 2:
                                finalColor = PackRGBA((byte)((r0+r1)/2), (byte)((g0+g1)/2), (byte)((b0+b1)/2), 255);
                                break;
                            case 3:
                                finalColor = PackRGBA(0, 0, 0, 255);
                                break;
                        }
                    }
 
                    if (x + i < width)
                        image[(UInt32)((y + j)*width + (x + i))] = finalColor;
                }
            }
        }

        public static void Decompress(UInt32 width, UInt32 height, byte[] blockStorage, byte[] image) {
            UInt32 blockCountX = (width + 3) / 4;
            UInt32 blockCountY = (height + 3) / 4;
            UInt32 blockWidth  = (width < 4) ? width : 4;
            UInt32 blockHeight = (height < 4) ? height : 4;

            UBytePtr blockStoragePtr = new UBytePtr(blockStorage);

            for (UInt32 j = 0; j < blockCountY; j++) {
                for (UInt32 i = 0; i < blockCountX; i++) {
                    DecompressBlockDXT1(i * 4, j * 4, width, blockStoragePtr + (i * 8), new UInt32Ptr(image));
                }

                blockStoragePtr += (blockCountX * 8);
            }
        }
    }
}
