
using System;
using System.Diagnostics;
using System.IO;

namespace UnityUnpack {
    [DebuggerDisplay("{Name}, size = {Size}, type = {Type}")]
    public class Asset {
        public UInt32 Index;
        public UInt32 Offset;
        public UInt32 Size;
        public AssetType Type;
        public UInt32 Magic;
        public string Name;
        public UInt32 HeaderLength;

        public Asset(Asset asset) {
            this.Index        = asset.Index;
            this.Offset       = asset.Offset;
            this.Size         = asset.Size;
            this.Type         = asset.Type;
            this.Magic        = asset.Magic;
            this.Name         = asset.Name;
            this.HeaderLength = asset.HeaderLength;
        }

        public Asset(BinaryReader reader) {
            Index  = reader.ReadUInt32();
            Offset = reader.ReadUInt32();
            Size   = reader.ReadUInt32();
            Type   = (AssetType)reader.ReadUInt32();
            Magic  = reader.ReadUInt32();
        }

        public override string ToString() {
            return string.Format("{0} {size = 1} {type = 2}", Name, Size, Type);
        }

        public static string TypeToExtension(AssetType type) {
            switch (type) {
            case AssetType.TEXTURE:
                return ".png";
            case AssetType.TEXT:
                return ".xml";
            case AssetType.SOUND:
                return ".ogg";
            default:
                return string.Format(".{0}", type);
            }
        }

        public virtual string Extension {
            get {
                return TypeToExtension(Type);
            }
        }

        public UInt32 ActualSize {
            get {
                return Size - HeaderLength;
            }
        }

        public virtual void Extract(Stream inStream, Stream outStream, UInt32 length) {
            AssetFile.Extract(inStream, outStream, length);
        }
    };
}
