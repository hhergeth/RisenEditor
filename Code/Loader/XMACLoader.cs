using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameLibrary.Objekte;
using SlimDX;
using System.Runtime.InteropServices;
using GameLibrary.IO;
using GameLibrary;
using SlimDX.Direct3D11;

namespace RisenEditor.Code.Loader
{
    public class XMACLoader : IModelLoader
    {
        class nNode
        {
            internal string s;
            internal Quaternion rot;
            internal Vector3 pos;
            internal uint parent;
            internal List<uint> children;

            internal nNode(char[] cs, Quaternion q, Vector3 v, uint p)
            {
                s = "";
                foreach (char c in cs)
                    s += c;
                pos = v;
                rot = q;
                parent = p;
                children = new List<uint>();
            }
        }

        class nMaterial
        {
            internal string n;
            internal List<nMap> Maps;

            internal nMaterial(char[] cs)
            {
                Maps = new List<nMap>();
                n = "";
                foreach (char c in cs)
                    n += c;
            }
        }

        class nMap
        {
            internal enum nMapType
            {
                Diffuse,
                Specular,
                Normal
            }
            internal nMapType nType;
            internal string sFile;

            public nMap(string s, nMapType M)
            {
                nType = M;
                sFile = s;
            }
        }

        Stream fStream;
        BinaryReader bReader;
        UInt32 nFaces = 0;
        UInt32[] baseVertIndices;
        UInt32 nVertices;
        List<nNode> Nodes = new List<nNode>();
        List<nMaterial> Materials = new List<nMaterial>();
        List<Vector3> Vertices = new List< Vector3>();
        List<Vector3> Normals = new List<Vector3>();
        List<Vector3> Tangents = new List<Vector3>();
        List<Vector3> TVertices = new List<Vector3>();
        List<UInt32> VerticesChecker = new List<UInt32>();
        List<UInt32> NormalsChecker = new List<UInt32>();
        List<UInt32> TangentsChecker = new List<UInt32>();
        List<UInt32> TVerticesChecker = new List<UInt32>();

        List<Tri> Tris = new List<Tri>();
        List<SubObj> SubObjekte = new List<SubObj>();

        public static Matrix mRot = Matrix.RotationX(-MathHelper.PiOver2);

        public void LoadModel(EFile file, API_Device D)
        {
            //fStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            fStream = file.Open(FileAccess.Read);
            bReader = new BinaryReader(fStream);

            Seek(136);
            UInt32 xacFileSize = bReader.ReadUInt32();
            char[] cs = bReader.ReadChars(3);
            string s = cs[0].ToString() + cs[1].ToString() + cs[2].ToString();
            if (s != "XAC")
                throw new Exception("This is not a valid XMAC file.");
            Seek(146);
            if (bReader.ReadByte() != 0)
            {
                long p = bReader.BaseStream.Position;
                bReader = new bReader2(fStream);
                bReader.BaseStream.Position = p;
            }
            Seek(148);
            while ((xacFileSize + 140) != Tell())
            {
                UInt32 secID = bReader.ReadUInt32();
                if (secID == 1)
                { // Mesh.
                    readMeshSection();
                }
                else if (secID == 2)
                { // Weighting.
                    readWeightingSection();
                }
                else if (secID == 7)
                { // General scene information.
                    readInfoSection();
                }
                else if (secID == 11)
                { // Nodes.
                    readNodesSection();
                }
                else if (secID == 13)
                { // Mats.
                    readMatsSection();
                }
                // (secID == 3) would belong to "MatSection" but all of them are read by readMatsSection().
                //else throw new EngineError("Unknown Error", EngineErrorType.NoInformation);
                else break;//this prevents it from crashing while loading faces, some other models can't be seen when this is activated due to some problems
            }

            var tmp = SubObj.CreateData(Vertices, TVertices, Normals, Tris);
            for (int i = 0; i < Materials.Count; i++)
            {
                if ((Materials[i].n.Contains("EMFX")) || (Materials[i].n.Contains("collision", StringComparison.CurrentCultureIgnoreCase)))
                    continue;
                string qs = Materials[i].n;
                SubObj so = new SubObj(qs, i);
                so.CreateVertexBuffer(tmp, true);
                so.LoadMaterial(D);
                SubObjekte.Add(so);
                //Matrix rm = Matrix.RotationAxis(Vector3.UnitY, MathHelper.PiOver2);
                //for (int i2 = 0; i2 < so.Vertices.Length; i2++)
                //    so.Vertices[i2].Position = Core.TransformVec3(so.Vertices[i2].Position.ToVec3(), rm).ToVec4(1.0f);
            }
            SubObjekte.Remove(null);
        }

        public GraphicNode FinishObjekt(API_Device D)
        {
            List<MeshPart> GSOs = new List<MeshPart>();
            for (int i = 0; i < SubObjekte.Count; i++)
            {
                SubObj s = SubObjekte[i];
                if (s.Vertices.Length == 0)
                    continue;
                MeshPart sub = s.CreateMeshPart(D);
                if(sub != null && sub.Buffer.VertexCount > 0)
                    GSOs.Add(sub);
            }
            if (GSOs.Count != 0)
            {
                Mesh m = new Mesh(SubObj.CreateFrustum(SubObjekte), CullMode.Back, GSOs.ToArray());
                return new GraphicNode("", m, D);
            }
            return null;
        }

        public string ModelFileExtension
        {
            get
            {
                return "._xmac";
            }
        }

        public IModelLoader CreateInstance()
        {
            return new XMACLoader();
        }

        private void readMeshSection()
        {
            Int32 size = bReader.ReadInt32();
            Int32 sectionEnd = Tell() + size + 4;
            Skip(4);
            Int32 nodeNum = bReader.ReadInt32();
            Int32 nBaseVerts = bReader.ReadInt32();
            nVertices = bReader.ReadUInt32();
            Int32 nIndices = bReader.ReadInt32();
            nFaces = (UInt32)(nIndices / 3);
            Skip(22);
            Int16 magicValue = bReader.ReadInt16();
            Skip(-12);
            while (sectionEnd != Tell())
            {
                Skip(10);
                Int16 mvCheck = bReader.ReadInt16();
                Skip(-12);
                if (mvCheck == magicValue)
                {
                    Int32 subSecID = bReader.ReadInt32();
                    Int32 subSecBlockSize = bReader.ReadInt32();
                    Skip(4);
                    if ((subSecID == 0) && (subSecBlockSize == 12))
                    { // Vertices.
                        readVertsSubSection();
                    }
                    else if ((subSecID == 1) && (subSecBlockSize == 12))
                    { // (V)Normals.
                        readNormalsSubSection();
                    }
                    else if ((subSecID == 2) && (subSecBlockSize == 16))
                    { // (V)Tangents.
                        readTangentsSubSection();
                    }
                    else if ((subSecID == 3) && (subSecBlockSize == 8))
                    { // Texture vertices.
                        readTVertsSubSection();
                    }
                    else if ((subSecID == 5) && (subSecBlockSize == 4))
                    { // Indicates which vertices are printed more than one time.
                        baseVertIndices = new UInt32[nVertices];
                        readBaseVertsSubSection();
                    }
                    else
                    {
                        Skip(-12);
                        readFacesSubSection(); // mv is sometimes 0
                    }
                }
                else if (Tris.Count != nFaces)
                    readFacesSubSection();
                else break;//thats crappy and dangerous
            }
            //baseVertIndices = null;
        }

        //TODO
        private void readWeightingSection()
        {
            List<uint> nWeights = new List<uint>();
            List<bool> used = new List<bool>();
            
            uint size = bReader.ReadUInt32();
            Skip((int)size + 4);
            return;
            Skip(4);
            uint count = bReader.ReadUInt32();
            Skip(4);
            uint nTotalWeights = bReader.ReadUInt32();
            Skip(4);
            uint weightingsPos = (uint)Tell();
            uint weightingCountsPos = weightingsPos + (nTotalWeights * 8);
            uint nVerts = ((size - 16 - (nTotalWeights * 8)) / 8);
            for (int i = 0; i < nVerts; i++)
            {
                used.Add(false);
            }
            Seek((int)weightingCountsPos);
            for (int i = 0; i < nVerts; i++)
		    {
			    Skip(4);
                nWeights.Add(bReader.ReadUInt32());
		    }
            Seek((int)weightingsPos);
            for (int i = 0; i < nVerts; i++)
            {
                float w = 0.0f;
                ushort boneID = 0;
                uint nVertexWeights = nWeights[i];
                List<float> weights = new List<float>();
                List<ushort> boneIDs = new List<ushort>();
                for (int j = 0; j < nVertexWeights; j++)
                {
                    w = bReader.ReadSingle();
                    boneID = bReader.ReadUInt16();
                    Skip(2);
                    weights.Add(w);
                    boneIDs.Add(boneID);
                    used[boneID] = true;
                }
            }
            Seek((int)(weightingCountsPos + (nVerts * 8)));
        }

        //TODO
        private void readInfoSection()
        {
            UInt32 size = bReader.ReadUInt32();
            Skip((int)size + 4);
        }

        private void readNodesSection()
        {
            UInt32 size = bReader.ReadUInt32();
            //Skip((int)size + 4);
            Skip(4);
            UInt32 nNodes = bReader.ReadUInt32();
            Skip(4);
            for (UInt32 i = 0; i < nNodes; ++i)
            {
                Quaternion q = new Quaternion(bReader.ReadSingle(), bReader.ReadSingle(), bReader.ReadSingle(), bReader.ReadSingle());
                Skip(16);
                Vector3 v = new Vector3(bReader.ReadSingle(), bReader.ReadSingle(), bReader.ReadSingle());
                Skip(32);
                int p = bReader.ReadInt32();
                Skip(76);
                int l = bReader.ReadInt32();
                char[] cs = bReader.ReadChars(l);
                nNode n = new nNode(cs, q, v, (uint)p);
                Nodes.Add(n);
            }
        }

        private void readMatsSection()
        {
            UInt32 size = bReader.ReadUInt32();
            Skip(4);
            UInt32 nMaterials = bReader.ReadUInt32();
            UInt32 control = bReader.ReadUInt32();
            if (nMaterials != control)
                throw new Exception("Strange problem occured");
            Skip(4);
            for (UInt32 i = 0; i < nMaterials; i++)
            {
                Skip(95);
                byte nMaps = bReader.ReadByte();
                nMaterial mat = new nMaterial(bReader.ReadChars(bReader.ReadInt32()));
                for (UInt32 j = 0; j < nMaps; j++)
                {
                    Skip(26);
                    byte xmacMapType = bReader.ReadByte();
                    Skip(1);
                    char[] buf = new char[4];
                    //_itoa_s((j + 1), buf, 4, 10); no idea what this is
                    //rmMap map((string("Map Nr. ").append(string(buf))), fs.readString(fs.readLong()).append(string(".jpg")));
                    char[] bf1 = bReader.ReadChars(bReader.ReadInt32());
                    nMap.nMapType t = nMap.nMapType.Diffuse;
                    if (xmacMapType == 2)
                    { // Diffuse.
                        t = nMap.nMapType.Diffuse;
                    }
                    else if (xmacMapType == 3)
                    { // Specular.
                        t = nMap.nMapType.Specular;
                    }
                    else if (xmacMapType == 5)
                    { // Normal.
                        t = nMap.nMapType.Normal;
                    }
                    mat.Maps.Add(new nMap(chrTostr(bf1), t));
                }
                Materials.Add(mat);
            }
        }

        private void readVertsSubSection()
        {
            Vector3 v = new Vector3(0);
            for (UInt32 i = 0; i < nVertices; ++i)
            {
                v.Y = bReader.ReadSingle();
                v.Z = bReader.ReadSingle();
                v.X = bReader.ReadSingle();
                v.Y *= -1.0f;
                v = Vector3.Transform(v, mRot).ToVec3();
                if (baseVertIndices != null)
                {
                    Vertices.Add(v);
                    VerticesChecker.Add(baseVertIndices[i]);
                }
                else
                {
                    Vertices.Add(v);
                    VerticesChecker.Add(i);
                }
            }
        }

        private void readNormalsSubSection()
        {
            Vector3 v = new Vector3(0);
            //m.setNumVNormals(nVertices);
            for (UInt32 i = 0; i < nVertices; ++i)
            {
                v.X = bReader.ReadSingle();
                v.Y = bReader.ReadSingle();
                v.Z = bReader.ReadSingle();
                v.Y *= -1.0f;
                v = Vector3.Transform(v, mRot).ToVec3();
                Normals.Add(v);
                NormalsChecker.Add(i);
            }
        }

        private void readTangentsSubSection()
        {
            Vector3 v = new Vector3(0);
            //m.setNumVTangents(nVertices);
            for (UInt32 i = 0; i < nVertices; ++i)
            {
                v.X = bReader.ReadSingle();
                v.Y = bReader.ReadSingle();
                v.Z = bReader.ReadSingle();
                v.Y *= -1.0f;
                v.Z *= -1.0f;
                v = Vector3.Transform(v, mRot).ToVec3();
                Single w = bReader.ReadSingle();
                Tangents.Add(v);
                TangentsChecker.Add(i);
            }
        }

        private void readTVertsSubSection()
        {
            Vector3 v = new Vector3(0);
            for (UInt32 i = 0; i < nVertices; ++i)
            {
                v.X = bReader.ReadSingle();
                v.Y = bReader.ReadSingle();
                TVertices.Add(v);
                TVerticesChecker.Add(i);
            }
        }

        private void readBaseVertsSubSection()
        {
            for (UInt32 i = 0; i < nVertices; ++i)
                baseVertIndices[i] = bReader.ReadUInt32();
        }

        private void readFacesSubSection()
        {
	        UInt32 nStrangeDWords = 0;
	        UInt32 nFacesToRead = nFaces;
	        UInt32 nFacesRead = 0;
	        UInt32 nFacesReadAfter = 0;
	        UInt32 nVertsPassed = 0;
	        UInt32 nVertsPassedAfter = 0;

            while (nFacesRead != nFacesToRead)
            {
                nFacesReadAfter += (bReader.ReadUInt32() / 3);
                nVertsPassedAfter += bReader.ReadUInt32();
                UInt32 fID = bReader.ReadUInt32();
                nStrangeDWords = bReader.ReadUInt32();

                for (; nFacesRead != nFacesReadAfter; ++nFacesRead)
                {
                    Int32 s3 = (bReader.ReadInt32() + (int)nVertsPassed);
                    Int32 s2 = (bReader.ReadInt32() + (int)nVertsPassed);
                    Int32 s1 = (bReader.ReadInt32() + (int)nVertsPassed);
                    Tris.Add(new Tri(s1, s2, s3, s1, s2, s3, s1, s2, s3, (int)fID));
                }

                nVertsPassed = nVertsPassedAfter;
                Skip((int)nStrangeDWords * sizeof(UInt32));
            }/*
            for (int i = 0; i < nFacesToRead; i++)
            {
                Tri t = Tris[i];
                t.N1 = t.T1 = t.V1 = (int)baseVertIndices[t.V1];
                t.N2 = t.T2 = t.V2 = (int)baseVertIndices[t.V2];
                t.N3 = t.T3 = t.V3 = (int)baseVertIndices[t.V3];
            }*/
        }

        private void Skip(int c)
        {
            if (c > 0)
                bReader.ReadBytes(c);
            else bReader.BaseStream.Position -= (long)Math.Abs(c);
        }

        private void Seek(int p)
        {
            this.bReader.BaseStream.Seek((int)p, SeekOrigin.Begin);
        }

        private int Tell()
        {
            return (int)bReader.BaseStream.Position;
        }

        private string chrTostr(char[] cs)
        {
            string r = "";
            foreach (char c in cs)
                r += c;
            return r;
        }
    }

    internal class bReader2 : BinaryReader
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        struct UValue
        {
            internal void Write(byte[] bs2)
            {
                byte[] bs = new byte[8];
                if (bs2.Length == 2)
                {
                    bs[0] = bs2[1];
                    bs[1] = bs2[0];
                }
                if (bs2.Length == 4)
                {
                    bs[0] = bs2[3];
                    bs[1] = bs2[2];
                    bs[2] = bs2[1];
                    bs[3] = bs2[0];
                }
                if (bs2.Length == 8)
                {
                    bs[0] = bs2[7];
                    bs[1] = bs2[6];
                    bs[2] = bs2[5];
                    bs[3] = bs2[4];
                    bs[4] = bs2[3];
                    bs[5] = bs2[2];
                    bs[6] = bs2[1];
                    bs[7] = bs2[0];
                }
                b0 = bs[0];
                b1 = bs[1];
                b2 = bs[2];
                b3 = bs[3];

                b4 = bs[4];
                b5 = bs[5];
                b6 = bs[6];
                b7 = bs[7];
            }

            [FieldOffset(0)]
            public byte b0;

            [FieldOffset(1)]
            public byte b1;

            [FieldOffset(2)]
            public byte b2;

            [FieldOffset(3)]
            public byte b3;

            [FieldOffset(4)]
            public byte b4;

            [FieldOffset(5)]
            public byte b5;

            [FieldOffset(6)]
            public byte b6;

            [FieldOffset(7)]
            public byte b7;

            [FieldOffset(0)]
            public short s;

            [FieldOffset(0)]
            public int i;

            [FieldOffset(0)]
            public long l;

            [FieldOffset(0)]
            public float f;

            [FieldOffset(0)]
            public double d;
        }

        public bReader2(Stream fs)
            : base(fs)
        {

        }

        public override short ReadInt16()
        {
            byte[] bs = base.ReadBytes(2);
            UValue u = new UValue();
            u.Write(bs);
            return u.s;
        }

        public override int ReadInt32()
        {
            byte[] bs = base.ReadBytes(4);
            UValue u = new UValue();
            u.Write(bs);
            return u.i;
        }

        public override long ReadInt64()
        {
            byte[] bs = base.ReadBytes(8);
            UValue u = new UValue();
            u.Write(bs);
            return u.l;
        }

        public override UInt16 ReadUInt16()
        {
            byte[] bs = base.ReadBytes(2);
            UValue u = new UValue();
            u.Write(bs);
            return (UInt16)u.s;
        }

        public override UInt32 ReadUInt32()
        {
            byte[] bs = base.ReadBytes(4);
            UValue u = new UValue();
            u.Write(bs);
            return (UInt16)u.i;
        }

        public override UInt64 ReadUInt64()
        {
            byte[] bs = base.ReadBytes(8);
            UValue u = new UValue();
            u.Write(bs);
            return (UInt64)u.l;
        }

        public override double ReadDouble()
        {
            byte[] bs = base.ReadBytes(8);
            UValue u = new UValue();
            u.Write(bs);
            return u.d;
        }

        public override float ReadSingle()
        {
            byte[] bs = base.ReadBytes(4);
            UValue u = new UValue();
            u.Write(bs);
            return u.f;
        }
    }
}
