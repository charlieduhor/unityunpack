
using System;
using System.IO;
using System.Text;

namespace UnityUnpack {
    public class AssetFile {
        public string file;
        public string fileSound;

        public UInt32 HeaderLength;
        public UInt32 FileSize;
        public UInt32 Unknown2;
        public UInt32 BeginningOfData;
        public UInt32 Unknown4;
        public string Version;
        public UInt32 Unknown5;
        public UInt32 Unknown6;
        public UInt32 Unknown7;
        public Asset[] Assets;

        public static UInt32 ReverseBytes(UInt32 value) {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public AssetFile(string file) {
            this.file = file;
            this.fileSound = file + ".resS";

            if (!File.Exists(fileSound)) {
                fileSound = null;
            }

            FileStream stream = File.OpenRead(file);

            try {
                BinaryReader reader = new BinaryReader(stream);
                UInt32 assetCount;

                HeaderLength    = ReverseBytes(reader.ReadUInt32());
                FileSize        = ReverseBytes(reader.ReadUInt32());
                Unknown2        = ReverseBytes(reader.ReadUInt32());
                BeginningOfData = ReverseBytes(reader.ReadUInt32());
                Unknown4        = reader.ReadUInt32();
                Version         = Encoding.UTF8.GetString(reader.ReadBytes(8)).Trim('\0');
                Unknown5        = reader.ReadUInt32();
                Unknown6        = reader.ReadUInt32();
                Unknown7        = reader.ReadUInt32();
                assetCount      = reader.ReadUInt32();
                Assets          = new Asset[assetCount];

                for (int index = 0; index < assetCount; index++) {
                    Assets[index] = new Asset(reader);

                    switch (Assets[index].Type) {
                    case AssetType.SOUND:
                        Assets[index] = new SoundAsset(Assets[index]);
                        break;
                    case AssetType.TEXTURE:
                        Assets[index] = new TextureAsset(Assets[index]);
                        break;
                    default:
                        break;
                    }
                }

                foreach (Asset asset in Assets) {
                    if (asset.Type == (AssetType)1) {
                        asset.Name = "";
                        continue;
                    }

                    stream.Position = BeginningOfData + asset.Offset;

                    //*******************
                    //* Read the name
                    //*******************
                    reader = new BinaryReader(stream);

                    UInt32 nameLength = reader.ReadUInt32();

                    asset.Name = Encoding.UTF8.GetString(reader.ReadBytes((int)nameLength));

                    if (nameLength % 4 == 0) {
                        asset.HeaderLength = 4 + nameLength;
                    }
                    else {
                        asset.HeaderLength = 4 + nameLength + (4 - (nameLength % 4));
                    }

                    //**************************
                    //* Parse the sound header
                    //**************************
                    if (asset.Type == AssetType.SOUND) {
                        stream.Position = BeginningOfData + asset.Offset + asset.HeaderLength;

                        SoundAsset soundAsset = (SoundAsset)asset;

                        reader = new BinaryReader(stream);
                        soundAsset.SoundUnknown0 = reader.ReadUInt32();
                        soundAsset.SoundUnknown1 = reader.ReadUInt32();
                        soundAsset.SoundUnknown2 = reader.ReadUInt32();
                        soundAsset.SoundType     = reader.ReadUInt32();
                        soundAsset.SoundLength   = reader.ReadUInt32();

                        if (soundAsset.ActualSize == 24) {
                            soundAsset.SoundOffset = reader.ReadUInt32();
                        }
                    }

                    //**************************
                    //* Parse the texture header
                    //**************************
                    if (asset.Type == AssetType.TEXTURE) {
                        stream.Position = BeginningOfData + asset.Offset + asset.HeaderLength;

                        ((TextureAsset)asset).Parse(stream);
                    }
                }
            }
            finally {
                stream.Dispose();
            }

            RecalculateSoundSizes();
        }

        protected void RecalculateSoundAssetSize(int i, SoundAsset asi) {
            SoundAsset nearest = null;

            for (int j = 0; j < Assets.Length; j++) {
                if (j == i) {
                    continue;
                }

                Asset aj = Assets[j];

                if (aj.Type != AssetType.SOUND) {
                    continue;
                }

                SoundAsset asj = (SoundAsset)aj;

                if (!asj.StoredExternally) {
                    continue;
                }

                if (asj.SoundOffset >= asi.SoundEnd) {
                    if (asj.SoundOffset == asi.SoundEnd) {
                        nearest = asj;
                        break;
                    }

                    if (nearest == null) {
                        nearest = asj;
                    }
                    else if (nearest.SoundOffset > asj.SoundOffset) {
                        nearest = asj;
                    }
                }
            }

            if ((nearest != null) &&
                (nearest.SoundOffset != UInt32.MaxValue)) {

                if (asi.SoundEnd != nearest.SoundOffset) {
                    asi.SoundEnd = nearest.SoundOffset;
                }
            }
        }

        protected void RecalculateSoundSizes() {
            for (int i = 0; i < Assets.Length; i++) {
                Asset ai = Assets[i];

                if (ai.Type != AssetType.SOUND) {
                    continue;
                }

                SoundAsset asi = (SoundAsset)ai;

                if (!asi.StoredExternally) {
                    continue;
                }

                RecalculateSoundAssetSize(i, asi);
            }
        }

        public void Extract(UInt32 index, string outFile) {
            Stream outStream = File.OpenWrite(outFile);

            try {
                Extract(index, outStream);
            }
            finally {
                outStream.Dispose();
            }
        }

        public static void Extract(Stream inStream, Stream outStream, UInt32 length) {
            if (length == 0) {
                throw new Exception("Invalid Length");
            }

            byte[] buffer = new byte[4096];

            while (length > 0) {
                int readed = inStream.Read(buffer, 0, (int)(length > 4096 ? 4096 : length));

                if (readed == 0) {
                    break;
                }

                outStream.Write(buffer, 0, readed);
                length -= (UInt32)readed;
            }
        }

        protected static bool CheckAudioHeader(Stream inStream) {
            byte[] b = new byte[4];

            inStream.Read(b, 0, 4);
            inStream.Position -= 4;

            if (b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F') {
                return true;
            }

            if (b[0] == 'O' && b[1] == 'g' && b[2] == 'g' && b[3] == 'S') {
                return true;
            }

            return false;
        }

        protected void Extract(Asset asset, Stream outStream) {
            FileStream stream;

            if (asset.Type == AssetType.SOUND) {
                SoundAsset soundAsset = (SoundAsset)asset;

                if (soundAsset.StoredExternally) {
                    stream = File.OpenRead(fileSound);

                    try {
                        stream.Position = soundAsset.SoundOffset;
                        Extract(stream, outStream, soundAsset.SoundLength);
                        return;
                    }
                    finally {
                        stream.Close();
                    }
                }
            }

            stream = File.OpenRead(file);

            try {
                stream.Position = BeginningOfData + asset.Offset + asset.HeaderLength;

                UInt32 length = asset.ActualSize;

                switch (asset.Type) {
                case AssetType.TEXT:
                    // Text files are length prefixed.
                    stream.Position += 4;
                    length          -= 4;
                    break;
                case AssetType.SOUND:
                    if (asset.ActualSize > 24) {
                        stream.Position += 20;
                        length -= 20;

                        if (!CheckAudioHeader(stream)) {
                            throw new Exception("Invalid sound format");
                        }
                    }
                    break;
                default:
                    break;
                }

                asset.Extract(stream, outStream, length);
            }
            finally {
                stream.Dispose();
            }
        }

        public void Extract(UInt32 index, Stream outStream) {
            Extract(Assets[index], outStream);
        }

        public void ExtractToFolder(string folder) {
            foreach (Asset asset in Assets) {
                if (asset.ActualSize == 0) {
                    continue;
                }

                Stream stream = File.OpenWrite(Path.Combine(folder, string.Format("{0}.{1}{2}", asset.Index, asset.Name, asset.Extension)));

                try {
                    Extract(asset, stream);
                }
                finally {
                    stream.Dispose();
                }
            }
        }
    };
}
