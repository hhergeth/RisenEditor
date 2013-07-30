using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RisenEditor.Code;
using RisenEditor.Code.RisenTypes;
using GameLibrary.IO;
using System.Runtime.InteropServices;
using GameLibrary;

namespace RisenEditor.Code
{
    public class ximgHeader : BinaryFileBlock
    {
        public char[] Ident;
        public uint PropOffset;
        public uint PropLength;
        public uint DataOffset;
        public uint DataLength;
        public ulong RawDateTime;    // (FILETIME)
        public char[] RawFileExt;  // ".dds", ".png", ".tga"

        public ximgHeader() { }

        public ximgHeader(IFile a_File)
        {
            deSerialize(a_File);
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<char>(Ident);
            a_File.Write<uint>(PropOffset);
            a_File.Write<uint>(PropLength);
            a_File.Write<uint>(DataOffset);
            a_File.Write<uint>(DataLength);
            a_File.Write<ulong>(RawDateTime);
            a_File.Write<char>(RawFileExt);
        }

        public override int Size
        {
            get { return 40; }
        }

        public override void deSerialize(IFile a_File)
        {
            Ident = a_File.Read<char>(8);
            PropOffset = a_File.Read<uint>();
            PropLength = a_File.Read<uint>();
            DataOffset = a_File.Read<uint>();
            DataLength = a_File.Read<uint>();
            RawDateTime = a_File.Read<ulong>();
            RawFileExt = a_File.Read<char>(8);
        }
    }

    public class eCImageResource2 : classData
    {
        short version;

        public eCImageResource2() { version = 201; }

        public eCImageResource2(IFile a_File)
        {
            deSerialize(a_File);
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(version);
            a_File.Write<byte>(new byte[32]);
        }

        public override int Size
        {
            get { return 34; }
        }

        public override void deSerialize(IFile a_File)
        {
            version = a_File.Read<short>();
            a_File.Position += 32;
        }
    }

    public class ximg : BinaryFileBlock
    {
        ximgHeader header;
        bCAccessorPropertyObject Prop;
        byte[] tgaData;

        public ximg(ShaderResourceTexture T)
        {
            Prop = new bCAccessorPropertyObject(new eCImageResource2());
            header = new ximgHeader();
            Prop.Properties.addProperty("Width", "int", T.Width);
            Prop.Properties.addProperty("Height", "int", T.Height);
            Prop.Properties.addProperty("SkipMips", "long", 0);
            Prop.Properties.addProperty("PixelFormat", "bTPropertyContainer<enum eCGfxShared::eEColorFormat>", eCImageResource2_eCGfxShared_eEColorFormat.eEColorFormat_DXT1);
            System.IO.MemoryStream M = new System.IO.MemoryStream(T.Width * T.Height);
            M.Position = 0;
            SlimDX.Result r = SlimDX.Direct3D11.Texture2D.ToStream(T.Device.Context, T.Texture, SlimDX.Direct3D11.ImageFileFormat.Dds, M);
            long l = M.Position;
            M.Position = 0;
            tgaData = M.ReadBytes((int)l);
            M.Dispose();
        }

        public ximg(ShaderResourceTexture T, EFile rawFile)
        {
            Prop = new bCAccessorPropertyObject(new eCImageResource2());
            header = new ximgHeader();
            Prop.Properties.addProperty("Width", "int", T.Width);
            Prop.Properties.addProperty("Height", "int", T.Height);
            Prop.Properties.addProperty("SkipMips", "long", 0);
            Prop.Properties.addProperty("PixelFormat", "bTPropertyContainer<enum eCGfxShared::eEColorFormat>", eCImageResource2_eCGfxShared_eEColorFormat.eEColorFormat_DXT1);
            var s = rawFile.Open(System.IO.FileAccess.Read);
            tgaData = s.GetAllBytes();
            rawFile.Close();
        }

        public ximg(EFile e)
        {
            eCFile s = new eCFile(e);
            deSerialize(s);
            s.Close();
        }

        public override void Serialize(IFile a_File)
        {
            header.DataLength = (uint)tgaData.Length;
            header.PropLength = (uint)Prop.Size;
            header.PropOffset = 40;
            header.RawDateTime = (ulong)DateTime.Now.ToFileTime();
            header.RawFileExt = ".dds0000".ToCharArray();
            header.Ident = "GR01IM04".ToCharArray();
            header.DataOffset = header.PropOffset + header.PropLength;
            header.Serialize(a_File);
            Prop.Serialize(a_File);
            a_File.Write<byte>(tgaData);
        }

        public override int Size
        {
            get 
            {
                int a = header.Size + Prop.Size + tgaData.Length;
                return a;
            }
        }

        public override void deSerialize(IFile a_File)
        {
            header = new ximgHeader(a_File);
            a_File.Position = header.PropOffset;
            Prop = new bCAccessorPropertyObject(a_File);
            a_File.Position = header.DataOffset;
            tgaData = a_File.Read<byte>((int)header.DataLength);
        }
    }
}
