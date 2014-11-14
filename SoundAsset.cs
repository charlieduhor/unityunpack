
using System;

namespace UnityUnpack {
    public class SoundAsset : Asset {
        public UInt32 SoundUnknown0;
        public UInt32 SoundUnknown1;
        public UInt32 SoundUnknown2;
        public UInt32 SoundType;
        public UInt32 SoundLength;
        public UInt32 SoundOffset;

        public SoundAsset(Asset asset) : base(asset) {
            SoundOffset = UInt32.MaxValue;
        }

        public override string Extension {
            get {
                switch (SoundType) {
                case 1:
                    return ".wav";
                }

                return base.Extension;
            }
        }

        public bool StoredExternally {
            get {
                return SoundOffset != UInt32.MaxValue;
            }
        }

        public UInt32 SoundEnd {
            get {
                if (SoundOffset == UInt32.MaxValue) {
                    return UInt32.MaxValue;
                }

                return SoundOffset + SoundLength;
            }

            set {
                SoundLength = value - SoundOffset;
            }
        }
    };
}
