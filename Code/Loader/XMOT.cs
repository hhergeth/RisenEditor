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
    class XMOTLoader
    {
        public class Vertex4
        {
	        public  Vector3	Pos;
	        public  float	Quaternion;

            public Vertex4(BinaryStream s)
            {
                Pos = s.Read<Vector3>();
                Quaternion = s.Read<float>();
            }
        }

        public class Vertex6
        {
            public Vector3 Pos;
	        public Quaternion	Orient;

            public Vertex6(BinaryStream s)
            {
                Pos = s.Read<Vector3>();
                Orient = new Quaternion(s.Read<float>(), s.Read<float>(), s.Read<float>(), 0);
                float t = 1.0f - (Orient.X * Orient.X) - (Orient.Y * Orient.Y) - (Orient.Z * Orient.Z);
                if (t <= 0.0f)
                {
                    Orient.W = 0.0f;
                }
                else
                {
                    Orient.W = -(float)Math.Sqrt(t);
                }
                Orient.Normalize();
            }
        }

        public class VertexAni4
        {
            public Vector3 Pos;
            public float TimeKadrAlfa;

            public VertexAni4(BinaryStream s)
            {
                Pos = s.Read<Vector3>();
                TimeKadrAlfa = s.Read<float>();
            }
        }

        public class VertexAni5
        {
            public Vector3 Pos;
            public float Quaternion;
            public float TimeKadrAlfa;

            public VertexAni5(BinaryStream s)
            {
                Pos = s.Read<Vector3>();
                Quaternion = s.Read<float>();
                TimeKadrAlfa = s.Read<float>();
            }
        }

        public class Vertex4_6
        {
	        Vertex4[]		Vertex_4;
	        Vertex6[]		Vertex_6;

            public Vertex4_6(BinaryStream s)
            {
                Vertex_4 = new Vertex4[4];
                Vertex_6 = new Vertex6[2];
                for (int i = 0; i < 4; i++)
                    Vertex_4[i] = new Vertex4(s);
                for (int i = 0; i < 2; i++)
                    Vertex_6[i] = new Vertex6(s);
            }
        }

        public class string16
        {
            string s;
            public string16(BinaryStream s)
            {
                short l = s.Read<short>();
                this.s = string.Empty;
                for (int i = 0; i < l; i++)
                    this.s = this.s + (char)s.Read<byte>();
            }
        }

        public class string32
        {
            string s;
            public string32(BinaryStream s)
            {
                uint l = s.Read<uint>();
                this.s = string.Empty;
                for (int i = 0; i < l; i++)
                    this.s = this.s + (char)s.Read<byte>();
            }
        }

        public class RedactorMAX_MAYA
        {
	        byte		TypeOf3D;	//
            string Type;

            public RedactorMAX_MAYA(BinaryStream s)
            {
                TypeOf3D = s.Read<byte>();
                switch (TypeOf3D)
                {
                    case 0:
                        Type = "Type3D_Redactor" + "Alias Maya 2008";
                        break;
                    case 1:
                        Type = "Type3D_Redactor" + "3D Studio Max 7";
                        break;
                }
                if (Type != string.Empty)
                    1.ToString();
            }
        }

        public class IMAGE_EFFECTS
        {
	        uint NumberOfEffects;
            ushort EndOfBlock;
            Dictionary<ushort, string16> Effects = new Dictionary<ushort, string16>();

            public IMAGE_EFFECTS(BinaryStream s)
            {
                NumberOfEffects = s.Read<uint>();
                if (NumberOfEffects > 0)
                {
                    for (int i = 0; i < NumberOfEffects; i++)
                    {	
                        ushort SectionEffects = s.Read<ushort>();
                        string16 NameEffects = new string16(s);
                        Effects.Add(SectionEffects, NameEffects);
                    }
                    EndOfBlock = s.Read<ushort>();
                }
                else
                {
                    EndOfBlock = s.Read<ushort>();
                }
            }
        };

        public class Class_eCMotionResource2
        {
            string16 NameClass;
            byte NumberClass;
            ushort NumberOfBlock;
            ushort EndOfBlock;
            //ushort EndOfBlock;
            uint SizeOfBlock;
            //ushort EndOfBlock;
            uint NumberEffects;
            string16 NameEffects;
            string16 NameClassObj;
            ushort CommandsOfBlockEffects;
            uint SizeOfBlockEffects;
            byte NumberBlockOfEffects;
            IMAGE_EFFECTS Effects;

            public Class_eCMotionResource2(BinaryStream s)
            {
                NameClass = new string16(s);
                NumberClass = s.Read<byte>();
                NumberOfBlock = s.Read<ushort>();
                EndOfBlock = s.Read<ushort>();
                EndOfBlock = s.Read<ushort>();
                SizeOfBlock = s.Read<uint>();
                EndOfBlock = s.Read<ushort>();
                NumberEffects = s.Read<uint>();
                NameEffects = new string16(s);
                NameClassObj = new string16(s);
                CommandsOfBlockEffects = s.Read<ushort>();
                SizeOfBlockEffects = s.Read<uint>();
                NumberBlockOfEffects = s.Read<byte>();
                Effects = new IMAGE_EFFECTS(s);
            }
        }

        public class IMAGE_XMOT_HEADERS
        {
            ushort Number01;
            byte Number02;
            ushort Number03;
            byte Number04;
            Class_eCMotionResource2 eCMotionResource2;

            public IMAGE_XMOT_HEADERS(BinaryStream s)
            {
                Number01 = s.Read<ushort>();
                Number02 = s.Read<byte>();
                Number03 = s.Read<ushort>();
                Number04 = s.Read<byte>();
                eCMotionResource2 = new Class_eCMotionResource2(s);
            }
        }

        public class Joints_3D
        {
	        uint			    Keys_Parent;
	        uint			    Keys_Child;
	        uint			    Keys_03;
	        uint			    Keys_04;

            public uint TypeCommands;
            public string32 NameJoints;			//

            public VertexAni4[] Joints_Parent_ROOT;
            public VertexAni5[] Joints_Child;
            public VertexAni5[] Joints_Kadrs_03;
            public VertexAni5[] Joints_Kadrs_04;

            public Vertex4_6 Vertex;

            public Joints_3D(BinaryStream s)
            {
                Keys_Parent = s.Read<uint>();
                Keys_Child = s.Read<uint>();
                Keys_03 = s.Read<uint>();
                Keys_04 = s.Read<uint>();
                TypeCommands = s.Read<uint>();
                NameJoints = new string32(s);
                Joints_Parent_ROOT = new VertexAni4[Keys_Parent];
                Joints_Child = new VertexAni5[Keys_Child];
                Joints_Kadrs_03 = new VertexAni5[Keys_03];
                Joints_Kadrs_04 = new VertexAni5[Keys_04];

                for (int i = 0; i < Keys_Parent; i++)
                    Joints_Parent_ROOT[i] = new VertexAni4(s);

                for (int i = 0; i < Keys_Child; i++)
                    Joints_Child[i] = new VertexAni5(s);

                for (int i = 0; i < Keys_03; i++)
                    Joints_Kadrs_03[i] = new VertexAni5(s);

                for (int i = 0; i < Keys_04; i++)
                    Joints_Kadrs_04[i] = new VertexAni5(s);

                if (s.Position != s.Length)
                    Vertex = new Vertex4_6(s);
            }
        }

        public class IMAGE_BONE_3D
        {
	        uint				NumberOfBlock;		//
	        uint				SizeOfBlock;		//

	        byte[]				NameXSM;		//

	        uint				NumOfBlocks;			//

	        RedactorMAX_MAYA	TypeOf3DRedactor;			//

	        uint				DataUnknow0;			//
	        uint				DataUnknow1;			//
	        uint				DataUnknow2;			//
	        uint				DataUnknow3;			//
	        uint				DataUnknow4;			//
	        uint				DataUnknow5;			//
	        short			    DataUnknow6_1;
	        byte				DataUnknow6_2;
	        byte				DataUnknow6_3;
            public string32 Name3D_Redactor;		//
            public string32 PathFileOfWork;			//
            public string32 DateFileOfWork;			//
	        uint			    Zero;				//
	        uint				Command202;				//
	        uint				DataUnKnown;				//
	        uint				Num_01;				//
	        uint				numJoints;				//	joints
            public Vertex4_6 Primitive3D_Setup;
            public Joints_3D[] SkeletalAnimation;

            public IMAGE_BONE_3D(BinaryStream s)
            {
                NumberOfBlock = s.Read<uint>();
                SizeOfBlock = s.Read<uint>();
                NameXSM = s.ReadRange<byte>(3);
                NumOfBlocks = s.Read<uint>();
                TypeOf3DRedactor = new RedactorMAX_MAYA(s);
                DataUnknow0 = s.Read<uint>();
                DataUnknow1 = s.Read<uint>();
                DataUnknow2 = s.Read<uint>();
                DataUnknow3 = s.Read<uint>();
                DataUnknow4 = s.Read<uint>();
                DataUnknow5 = s.Read<uint>();
                DataUnknow6_1 = s.Read<short>();
                DataUnknow6_2 = s.Read<byte>();
                DataUnknow6_3 = s.Read<byte>();
                Name3D_Redactor = new string32(s);
                PathFileOfWork = new string32(s);
                DateFileOfWork = new string32(s);
                Zero = s.Read<uint>();
                Command202 = s.Read<uint>();
                DataUnKnown = s.Read<uint>();
                Num_01 = s.Read<uint>();
                numJoints = s.Read<uint>();
                Primitive3D_Setup = new Vertex4_6(s);
                SkeletalAnimation = new Joints_3D[numJoints];
                for (int i = 0; i < numJoints; i++)
                    SkeletalAnimation[i] = new Joints_3D(s);
            }
        }

        public class HeaderXmot
        {
	        uint		    ResMagic;
	        uint		    ResClass;
	        uint		    PropOffset;//IMAGE_XMOT_HEADERS
	        uint		    PropLength;
	        uint		    DataOffset;//IMAGE_BONE_3D
	        uint		    DataLength;
	        uint		    RawDateTime;
	        char[]		    RawFileExt;
            public IMAGE_XMOT_HEADERS header;
            public IMAGE_BONE_3D data;

            public HeaderXmot(BinaryStream s)
            {
                ResMagic = s.Read<uint>();
                ResClass = s.Read<uint>();
                PropOffset = s.Read<uint>();
                PropLength = s.Read<uint>();
                DataOffset = s.Read<uint>();
                DataLength = s.Read<uint>();
                RawDateTime = s.Read<uint>();
                RawFileExt = new char[8];
                for (int i = 0; i < 8; i++)
                    RawFileExt[i] = (char)s.Read<byte>();
                s.Position = PropOffset;
                header = new IMAGE_XMOT_HEADERS(s);
                s.Position = DataOffset;
                data = new IMAGE_BONE_3D(s);
            }
        }

        public void Load(EFile F, API_Device D)
        {
            BinaryStream s = F.Open(FileAccess.Read).CopyToBin();
            HeaderXmot a = new HeaderXmot(s);
            //a.data.
        }
    }
}
