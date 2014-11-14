using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace UnityUnpack {
    [DebuggerDisplay("{p} {i}")]
    public class Ptr {
        public byte[] p;
        public int i;

        public Ptr(Ptr p) {
            this.p = p.p;
            this.i = p.i;
        }

        public Ptr(byte[] p) {
            this.p = p;
        }

        public Ptr(byte[] p, int i) {
            this.p = p;
            this.i = i;
        }

        public UInt16 asUInt16() {
            return (UInt16)(p[i] | (p[i + 1] << 8));
        }

        public UInt32 asUInt32() {
            return (UInt32)(p[i] | (p[i + 1] << 8) | (p[i + 2] << 16) | (p[i + 3] << 24));
        }

        public void setUInt32(UInt32 value) {
            p[i + 0] = (byte)(value & 0xff);
            p[i + 1] = (byte)((value >> 8) & 0xff);
            p[i + 2] = (byte)((value >> 16) & 0xff);
            p[i + 3] = (byte)((value >> 24) & 0xff);
        }
    }

    [DebuggerDisplay("{p} {i}")]
    public sealed class UBytePtr : Ptr {
        public UBytePtr(Ptr p)
            : base(p) {
        }

        public UBytePtr(byte[] p)
            : base(p) {
        }

        public UBytePtr(byte[] p, int i)
            : base(p, i) {
        }

        public static UBytePtr operator +(UBytePtr p, int v) {
            if (v == 0) {
                return p;
            }

            return new UBytePtr(p.p, p.i + v);
        }

        public static UBytePtr operator +(UBytePtr p, UInt32 v) {
            if (v == 0) {
                return p;
            }

            return new UBytePtr(p.p, p.i + (int)v);
        }

        public byte this[int key] {
            get {
                return p[i+key];
            }

            set {
                p[i+key] = value;
            }
        }
    }

    [DebuggerDisplay("{p} {i}")]
    public sealed class UInt32Ptr : Ptr {
        public UInt32Ptr(Ptr p)
            : base(p) {
        }

        public UInt32Ptr(byte[] p)
            : base(p) {
        }

        public UInt32Ptr(byte[] p, int i)
            : base(p, i) {
        }

        public static UInt32Ptr operator +(UInt32Ptr p, int v) {
            if (v == 0) {
                return p;
            }

            return new UInt32Ptr(p.p, p.i + (v * 4));
        }

        public static UInt32Ptr operator +(UInt32Ptr p, UInt32 v) {
            if (v == 0) {
                return p;
            }

            return new UInt32Ptr(p.p, p.i + (int)(v * 4));
        }

        public UInt32 this[int key] {
            get {
                return (this + key).asUInt32();
            }

            set {
                (this + key).setUInt32(value);
            }
        }

        public UInt32 this[UInt32 key] {
            get {
                return (this + key).asUInt32();
            }

            set {
                (this + key).setUInt32(value);
            }
        }
    }
}
