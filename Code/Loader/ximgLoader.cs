using GameLibrary.Objekte;
using System;
using SlimDX.Direct3D11;
using SlimDX;
using DX9 = SlimDX.Direct3D9;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using GameLibrary.IO;
using GameLibrary;
using Microsoft.Win32;
using System.Windows.Forms;

namespace RisenEditor.Code.Loader
{
    public class XIMGLoader : ITextureLoader
    {
        static string[] exts = new string[]
        {
            "._ximg",
        };

        public string[] FileExtensions
        {
            get
            {
                return exts;
            }
        }

        public bool RightLoader(string e)
        {
            foreach (string s in exts)
                if (s.Contains(e, System.StringComparison.CurrentCultureIgnoreCase))
                    return true;
            return false;
        }

        public Texture2D LoadTextureFromFile(EFile s, API_Device D)
        {
            Stream fStream = s.Open(FileAccess.Read);
            fStream.Position = 16L;
            int i0 = fStream.ReadInt();
            int i1 = fStream.ReadInt();
            fStream.Position = i0;
            Texture2D T = Texture2D.FromStream(D.HadrwareDevice(), fStream, i1);
            s.Close();
            return T;
            /*
            BinaryReader bReader = new BinaryReader(fStream);

            char[] ResMagic = bReader.ReadChars(4);
            char[] ResClass = bReader.ReadChars(4);
            int PropOffset = bReader.ReadInt32();
            int PropLength = bReader.ReadInt32();
            int DataOffset = bReader.ReadInt32();
            int DataLength = bReader.ReadInt32();
            long RawDateTime = bReader.ReadInt64();
            char[] RawFileExt = bReader.ReadChars(8);

            bReader.BaseStream.Position = PropOffset;
            byte[] Buffer = new byte[PropLength];
            for (int i = 0; i < PropLength; i++)
                Buffer[i] = bReader.ReadByte();

            bReader.BaseStream.Position = DataOffset;
            byte[] Surface = new byte[DataLength];
            for (int i = 0; i < DataLength; i++)
                Surface[i] = bReader.ReadByte();
            fStream.Close();
            bReader.Close();

            DataStream ds = new DataStream(Surface, true, true);
            Texture2D t11 = Texture2D.FromStream(D.HadrwareDevice(), ds, (int)ds.Length);
            ds.Dispose();
            Surface = null;
            ds = null;
            return t11;*/
        }

        public void Dispose()
        {

        }

        public ITextureLoader CreateInstance()
        {
            return new XIMGLoader();
        }
    }
}
