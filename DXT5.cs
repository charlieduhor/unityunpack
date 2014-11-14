using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityUnpack {
    public class DXT5 {
        protected static UInt32 PackRGBA(byte r, byte g, byte b, byte a) {
            return (UInt32)((a << 24) | (r << 16) | (g << 8) | b);
        }

        protected static void DecompressBlockDXT5(UInt32 x, UInt32 y, UInt32 width, UBytePtr blockStorage, UInt32Ptr image) {
            byte     alpha0 = blockStorage[0];
            byte     alpha1 = blockStorage[1];
            UBytePtr bits   = blockStorage + 2;

            UInt32 alphaCode1 = (UInt32)(bits[2] | (bits[3] << 8) | (bits[4] << 16) | (bits[5] << 24));
            UInt16 alphaCode2 = (UInt16)(bits[0] | (bits[1] << 8));
 
            UInt16 color0 = (blockStorage +  8).asUInt16();
            UInt16 color1 = (blockStorage + 10).asUInt16();

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
 
            UInt32 code = (blockStorage + 12).asUInt32();

            string s = "";

            for (int j=0; j < 4; j++) {
                for (int i=0; i < 4; i++) {
                    int alphaCodeIndex = 3*(4*j+i);
                    int alphaCode;
 
                    if (alphaCodeIndex <= 12)
                    {
                        alphaCode = (alphaCode2 >> alphaCodeIndex) & 0x07;
                    }
                    else if (alphaCodeIndex == 15)
                    {
                        alphaCode = (int)(((UInt32)alphaCode2 >> 15) | (((UInt64)alphaCode1 << 1) & 0x06));
                    }
                    else // alphaCodeIndex >= 18 && alphaCodeIndex <= 45
                    {
                        alphaCode = (int)((alphaCode1 >> (alphaCodeIndex - 16)) & 0x07);
                    }
 
                    byte finalAlpha;

                    if (alphaCode == 0)
                    {
                        finalAlpha = alpha0;
                    }
                    else if (alphaCode == 1)
                    {
                        finalAlpha = alpha1;
                    }
                    else
                    {
                        if (alpha0 > alpha1)
                        {
                            finalAlpha = (byte)(((8-alphaCode)*alpha0 + (alphaCode-1)*alpha1)/7);
                        }
                        else
                        {
                            if (alphaCode == 6)
                                finalAlpha = 0;
                            else if (alphaCode == 7)
                                finalAlpha = 255;
                            else
                                finalAlpha = (byte)(((6-alphaCode)*alpha0 + (alphaCode-1)*alpha1)/5);
                        }
                    }

                    s += string.Format("{0:X2}", finalAlpha);

                    byte colorCode = (byte)((code >> 2 * (4 * j + i)) & 0x03);
                    UInt32 finalColor = 0;

                    switch (colorCode)
                    {
                        case 0:
                            finalColor = PackRGBA(r0, g0, b0, finalAlpha);
                            break;
                        case 1:
                            finalColor = PackRGBA(r1, g1, b1, finalAlpha);
                            break;
                        case 2:
                            finalColor = PackRGBA((byte)((2 * r0 + r1) / 3), (byte)((2 * g0 + g1) / 3), (byte)((2 * b0 + b1) / 3), finalAlpha);
                            break;
                        case 3:
                            finalColor = PackRGBA((byte)((r0 + 2 * r1) / 3), (byte)((g0 + 2 * g1) / 3), (byte)((b0 + 2 * b1) / 3), finalAlpha);
                            break;
                    }
 
                    if (x + i < width)
                        image[(UInt32)((y + j)*width + (x + i))] = finalColor;
                }
            }
        }

        public static void Decompress(UInt32 width, UInt32 height, byte[] blockStorage, byte[] image) {
            UInt32 blockCountX = (width  + 3) / 4;
            UInt32 blockCountY = (height + 3) / 4;
            UInt32 blockWidth  = (width  < 4)? width : 4;
            UInt32 blockHeight = (height < 4)? height : 4;

            UBytePtr blockStoragePtr = new UBytePtr(blockStorage);

            for (UInt32 j = 0; j < blockCountY; j++) {
                for (UInt32 i = 0; i < blockCountX; i++) {
                    DecompressBlockDXT5(i * 4, j * 4, width, blockStoragePtr + (i * 16), new UInt32Ptr(image));
                }

                blockStoragePtr += (blockCountX * 16);
            }
        }
    }
}
