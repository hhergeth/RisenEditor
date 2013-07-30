using System;
using System.Collections.Generic;
using System.IO;
using GameLibrary.Objekte;
using SlimDX;
using GameLibrary.IO;
using GameLibrary;
using SlimDX.Direct3D11;

namespace RisenEditor.Code.Loader
{
    public class XMSHLoader : IModelLoader
    {
        static Matrix mRot = Matrix.RotationX(-MathHelper.PiOver2);

        List<SubObj> SubObjekte;
        int numSubMehshes;
        BinaryReader bReader;

        public void LoadModel(EFile file, API_Device D)
        {
            List<List<Vector3>> verts = new List<List<Vector3>>();
            List<List<Vector3>> tverts = new List<List<Vector3>>();
            List<List<int>> faces = new List<List<int>>();
            List<int> vcounts = new List<int>();
            List<int> icounts = new List<int>();
            List<string> matnames = new List<string>();
            int allinds = 0;
            int allverts = 0;
            int numoldverts = 0;
            float x = 0, y = 0, z = 0, u = 0, v = 0, w = 0;

            this.SubObjekte = new List<SubObj>();
            //FileStream fStream = new FileStream(file., FileMode.Open, FileAccess.Read);
            Stream fStream = file.Open(FileAccess.Read);
            bReader = new BinaryReader(fStream);
            Skip(70);
            int vd = bReader.ReadInt32();
            Skip(53);
            numSubMehshes = bReader.ReadInt32();
            for (int h = 0; h < numSubMehshes; h++)
            {
                verts.Add(new List<Vector3>());
                tverts.Add(new List<Vector3>());
                faces.Add(new List<int>());
                Suche((int)vd, 77, 97, 116, 101, 114, 105, 97, 108);
                Skip(20);
                short len = Math.Abs(bReader.ReadInt16());
                //string name = bReader.ReadString();                                 
                //matnames.Add(name.Replace("._xmat", ".dds"));       
                byte[] namebs = bReader.ReadBytes(len);
                string name = new System.Text.ASCIIEncoding().GetString(namebs);
                matnames.Add(name);
                Suche((int)vd, 73, 110, 100, 101, 120, 67, 111, 117, 110, 116);
                Skip(12);
                int indcount = Math.Abs(bReader.ReadInt32());
                icounts.Add(indcount);
                allinds += indcount;
                Skip(54);
                int vertcount = Math.Abs(bReader.ReadInt32());
                vcounts.Add(vertcount);
                allverts += vertcount;
            }
            bReader.BaseStream.Position = (vd + 202);                               
            int vdsize = Math.Abs(bReader.ReadInt32());
            Skip(12);
            int o = Math.Abs(bReader.ReadInt32());
            o /= 4;
            Skip(56);
            int svd = (int)bReader.BaseStream.Position;
            for (int ii = 0; ii < numSubMehshes; ii++)
            {
                for (int i = 0; i < (o * vcounts[ii]) ; i++)
                {
                    float f = bReader.ReadSingle();
                    int id = (i + 1) % o;                                                
                    if (id == 1)
                    {
                        y = -f;
                    }
                    if (id == 2)
                    {
                        z = f;
                    }
                    if (id == 3)
                    {
                        x = f;
                        verts[ii].Add(new Vector3(x,y,z));
                    }
                    if (o == 13)
                    {
	                    if (id == 11)
                        {
                            w = f;
                        }
                        if (id == 12)
                        {
                            u = f;
                        }
                        if (id == 0)
                        {
                            v = f;//v = 1 - f;
                            tverts[ii].Add(new Vector3(u,v,w));
                        }
                    }
                   if (o == 15)
                    {
	                    if (id == 11)
                        {
                             w = f;
                        }
                        if (id == 12)
                        {
                            u = f;
                        }
                        if (id == 13)
                        {
                            v = f;
                            tverts[ii].Add(new Vector3(u,v,w));
                        }
                    }
                }
            }
            for (int jj = 0; jj < numSubMehshes; jj++)
            {
                for (int n = 0; n < icounts[jj]; n++)
                {
                    int f = (int)bReader.ReadUInt32();
                    f -= numoldverts;
                    float id = (n + 1) % 3;                                                   
                    if (id == 1)
                    {
                        x = f;
                    }
                    if (id == 2)
                    {
                        y = f;
                    }
                    if (id == 0)
                    {
                        z = f;
                        faces[jj].Add((int)x);
                        faces[jj].Add((int)y);
                        faces[jj].Add((int)z);
                    }
                }
                numoldverts += vcounts[jj];
            }
            for (int s = 0; s < numSubMehshes; s++)
            {
                SubObj so = new SubObj(matnames[s], s);
                List<Tri> ts = new List<Tri>();
                for (int fl = 0; fl < faces[s].Count; fl += 3)
                    ts.Add(new Tri(faces[s][fl], faces[s][fl + 1], faces[s][fl + 2], faces[s][fl], faces[s][fl + 1], faces[s][fl + 2], faces[s][fl], faces[s][fl + 1], faces[s][fl + 2], s));
                int c = verts[s].Count + 1;
                Vector3[] vtmp = new Vector3[c];
                List<Vector3> ns = new List<Vector3>(vtmp);
                for (int h = 0; h < verts[s].Count; h++)
                    verts[s][h] = Vector3.Transform(verts[s][h], mRot).ToVec3();
                var tmp = SubObj.CreateData(verts[s], tverts[s], ns, ts);
                so.CreateVertexBuffer(tmp, true);
                so.LoadMaterial(D);
                SubObjekte.Add(so);
            }
            file.Close();
        }

        public GraphicNode FinishObjekt(API_Device D)
        {
            List<MeshPart> GSOs = new List<MeshPart>();
            for (int i = 0; i < SubObjekte.Count; i++)
            {
                SubObj s = SubObjekte[i];
                MeshPart sub = s.CreateMeshPart(D);
                GSOs.Add(sub);
            }
            if (GSOs.Count != 0)
            {
                Mesh m = new Mesh(SubObj.CreateFrustum(SubObjekte), CullMode.Back, GSOs.ToArray());
                return new GraphicNode("", m, D);
            }
            return null;
        }

        private void Skip(int c)
        {
            bReader.ReadBytes(c);    
        }

        private void Suche(int range, params byte[] bytes)
        {
            byte b;
            int matches = 0;
            while (range > 0)
            {
                b = (byte)bReader.ReadSByte();
                if (bytes[matches] == b)
                {
                    matches += 1;
                    if (matches == bytes.Length)
                        range = 0;
                }
                else
                {
                    matches = 0;
                }
                range -= 1;
            }
        }

        public string ModelFileExtension
        {
            get
            {
                return "._xmsh";
            }
        }

        public IModelLoader CreateInstance()
        {
            return new XMSHLoader();
        }
    }
}
