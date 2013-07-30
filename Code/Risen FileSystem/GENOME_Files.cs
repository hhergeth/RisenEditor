using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using GameLibrary;
using GameLibrary.IO;
using System.IO;

namespace RisenEditor.Code.RisenTypes
{
    public class gCWorld : classData
    {
        short Version;
        public bCString WorldDataFile;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
            if (Version >= 36)
                WorldDataFile = new bCString(a_File);
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
            if (Version >= 36)
                WorldDataFile.Serialize(a_File);
        }

        public override int Size
        {
            get { return 2 + (Version >= 36 ? WorldDataFile.Size : 0); }
        }
    }

    public class gCSector : classData
    {
        short Version;
        byte Enabled;
        int Reserved;
        public List<bCString> DynamicLayers;

        public override void deSerialize(IFile a_File)
        {
            Enabled = 0;
            Version = a_File.Read<short>();
            if (Version >= 27)
            {
                Enabled = a_File.Read<byte>();
                if (Enabled != 0)
                {
                    DynamicLayers = new List<bCString>(a_File.Read<int>());
                    Reserved = a_File.Read<int>();
                    for (int i = 0; i < DynamicLayers.Capacity; i++)
                        DynamicLayers.Add(new bCString(a_File));
                }
            }
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
            if (Version >= 27)
            {
                a_File.Write<byte>(Enabled);
                if (Enabled != 0)
                {
                    a_File.Write<int>(DynamicLayers.Count);
                    a_File.Write<int>(Reserved);
                    foreach (bCString s in DynamicLayers)
                        s.Serialize(a_File);
                }
            }
        }

        public override int Size
        {
            get 
            {
                int q = 2;
                if (Version >= 27)
                    q += 1;
                if (Enabled != 0)
                    q += 4 + DynamicLayers.SizeOf();
                return q;
            }
        }
    }

    public class eCMeshResource22 : classData
    {
        byte[] b0;
        byte[] b1;

        public override void deSerialize(IFile a_File)
        {
            b0 = a_File.Read<byte>(204);
            b1 = a_File.Read<byte>(15);
        }

        public override BinaryFileBlock Clone()
        {
            throw new NotImplementedException();
        }

        public override void Serialize(IFile a_File)
        {
            throw new NotImplementedException();
        }

        public override int Size
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class eCSubMesh : BinaryFileBlock
    {
        class c0
        {
            public bCBox m0;
            public int m1;
            public int m2;
        }
        short Version;
        List<c0> data;

        public eCSubMesh(IFile a_File)
        {
            int q = (int)a_File.Position / 413;
            Version = a_File.Read<short>();
            int v10 = a_File.Read<int>() / 32;
            data = new List<c0>();
            int i = 0;
            while (true)
            {
                c0 c = new c0();
                c.m0 = new bCBox(a_File);
                c.m1 = a_File.Read<int>();
                c.m2 = a_File.Read<int>();
                data.Add(c);
                ++i;
                if (i >= v10)
                    break;
            }
            if(a_File.Length > (q + 1) * 408 + 5)
                a_File.Position = (q + 1) * 408 + 5;
        }

        public override BinaryFileBlock Clone()
        {
            throw new NotImplementedException();
        }

        public override void Serialize(IFile a_File)
        {
            throw new NotImplementedException();
        }

        public override void deSerialize(IFile a_File)
        {
            throw new NotImplementedException();
        }

        public override int Size
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class eCResourceCache2 : classData
    {
        class c0
        {
            public int i2;
            public bCAccessorPropertyObject obj;
            public bCString s0;
            public int i3;
        }

        int i0, i1;
        List<c0> C = new List<c0>();


        public eCResourceCache2(IFile a_File)
        {
            deSerialize(a_File);
        }

        public override void deSerialize(IFile a_File)
        {
            a_File.Position = 14L;
            i0 = a_File.Read<int>();
            if (i0 != 875579463)
                return;
            i1 = a_File.Read<int>();
            C = new List<c0>();
            for (int i = 0; i < 10; i++)
            {
                c0 c = new c0();
                c.i2 = a_File.Read<int>();
                c.obj = new bCAccessorPropertyObject(a_File);
                c.s0 = new bCString(a_File);
                c.i3 = a_File.Read<int>();
                a_File.Position += 16;
                C.Add(c);
            }
        }

        public override void Serialize(IFile a_File)
        {
            throw new NotImplementedException();
        }

        public override int Size
        {
            get { throw new NotImplementedException(); }
        }
    }
}
