using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameLibrary;
using GameLibrary.IO;
using GameLibrary.Objekte;
using System.IO;
using System.ComponentModel;
using SlimDX;

namespace RisenEditor.Code.RisenTypes
{
    public enum eEShaderColorModifier
    {
        eEShaderColorModifier_Default = 0x00000000,  // ""
        eEShaderColorModifier_Negate = 0x00000001,  // "-"
        eEShaderColorModifier_Invert = 0x00000002,  // "1-"
        eEShaderColorModifier_Saturate = 0x00000003,  // "saturate"
        eEShaderColorModifier_Ceil = 0x00000004,  // "ceil"
        eEShaderColorModifier_Floor = 0x00000005,  // "floor"
        eEShaderColorModifier_Abs = 0x00000006,  // "abs"
        eEShaderColorModifier_Frac = 0x00000007,  // "frac"
        eEShaderColorModifier_Count,
        eEShaderColorModifier_ForceDWORD = 0x7FFFFFFF
    };

    public enum eEShaderColorSrcComponent
    {
        eEShaderColorSrcComponent_Default = 0x00000000,  // ""
        eEShaderColorSrcComponent_RGB = 0x00000001,  // ".rgb"
        eEShaderColorSrcComponent_R = 0x00000002,  // ".r"
        eEShaderColorSrcComponent_G = 0x00000003,  // ".g"
        eEShaderColorSrcComponent_B = 0x00000004,  // ".b"
        eEShaderColorSrcComponent_A = 0x00000005,  // ".a"
        eEShaderColorSrcComponent_Count,
        eEShaderColorSrcComponent_ForceDWORD = 0x7FFFFFFF
    };

    public class eCColorSrcProxy : BinaryFileBlock
    {
        public int Version;
        public eEShaderColorSrcComponent ColorComponent;
        public eEShaderColorModifier ColorModifier;
        public bCGuid Token;
        public eCShaderBase Parent;

        public eCColorSrcProxy() { }

        public eCColorSrcProxy(eCShaderBase P)
        {
            Version = 666;
            ColorComponent = eEShaderColorSrcComponent.eEShaderColorSrcComponent_Default;
            ColorModifier = eEShaderColorModifier.eEShaderColorModifier_Default;
            Token = new bCGuid(Guid.Empty, false);
            Parent = P;
        }

        public eCColorSrcProxy(bCGuid G, eCShaderBase B,eEShaderColorSrcComponent comp = eEShaderColorSrcComponent.eEShaderColorSrcComponent_Default, eEShaderColorModifier mod = eEShaderColorModifier.eEShaderColorModifier_Default)
        {
            Version = 666;
            ColorComponent = comp;
            ColorModifier = mod;
            Token = G;
            Parent = B;
        }

        internal eCColorSrcProxy(IFile s, eCShaderBase B)
        {
            Parent = B;
            deSerialize(s);
        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<int>();
            if (Version >= 666)
            {
                ColorComponent = (eEShaderColorSrcComponent)a_File.Read<int>();// eEShaderColorSrcComponent (0 = "", 1 = ".rgb", 2 = ".r", 3 = ".g", 4 = ".b", 5 = ".a")
                ColorModifier = (eEShaderColorModifier)a_File.Read<int>();// eEShaderColorModifier (0 = "", 1 = "-", 2 = "1-", 3 = "saturate", 4 = "ceil", 5 = "floor", 6 = "abs", 7 = "frac")
            }
            Token = new bCGuid(a_File);
        }

        public override void Serialize(IFile s)
        {
            s.Write<int>(666);
            s.Write<int>((int)ColorComponent);
            s.Write<int>((int)ColorModifier);
            Token.Serialize(s);
        }

        public override int Size
        {
            get { return 12 + Token.Size; }
        }

        public eCShaderEllementBase Operand
        {
            get
            {
                return Parent.GetEllementByGuid(Token);
            }
        }

        public ShaderResourceTexture CreateTexture(API_Device D)
        {
            if(ColorModifier == eEShaderColorModifier.eEShaderColorModifier_Default)
                return (Operand as subClassBase).CreateTexture(D);
            ShaderResourceTexture src = (Operand as subClassBase).CreateTexture(D);
            ShaderResourceTexture R = new ShaderResourceTexture(src.Width, src.Height, 1, SlimDX.Direct3D11.ResourceUsage.Default, SlimDX.Direct3D11.BindFlags.ShaderResource, SlimDX.Direct3D11.CpuAccessFlags.None, SlimDX.DXGI.Format.B8G8R8A8_UNorm, 1, SlimDX.Direct3D11.ResourceOptionFlags.None, D);
            FileManager.e_MatEffect.Variables["sourceTex0"].SetVariable(src.ShaderResourceView);
            FileManager.e_MatEffect.Variables["dim"].SetVariable(new Vector2(R.Width, R.Height));
            bool all = ColorComponent == eEShaderColorSrcComponent.eEShaderColorSrcComponent_Default;
            bool rgb = ColorComponent == eEShaderColorSrcComponent.eEShaderColorSrcComponent_RGB;
            FileManager.e_MatEffect.Variables["r"].SetVariable(ColorComponent == eEShaderColorSrcComponent.eEShaderColorSrcComponent_R || all || rgb);
            FileManager.e_MatEffect.Variables["g"].SetVariable(ColorComponent == eEShaderColorSrcComponent.eEShaderColorSrcComponent_G || all || rgb);
            FileManager.e_MatEffect.Variables["b"].SetVariable(ColorComponent == eEShaderColorSrcComponent.eEShaderColorSrcComponent_B || all || rgb);
            FileManager.e_MatEffect.Variables["a"].SetVariable(ColorComponent == eEShaderColorSrcComponent.eEShaderColorSrcComponent_A || all);
            FileManager.e_MatEffect.Variables["op"].SetVariable((int)ColorModifier - 1);
            FileManager.g_pApp.PauseRendering();
            FileManager.r_MatTarget.SetTarget(ClearType.All);
            FullScreenQuad.RenderQuad(FileManager.e_MatEffect.Techniques["blend"].Passes[0]);
            R.CopyFrom(FileManager.r_MatTarget, 0, 0, R.Width, 0, R.Height, 0, 0);
            FileManager.g_pApp.ResumeRendering();
            return R;
        }

        public override BinaryFileBlock Clone()
        {
            throw new NotImplementedException();
        }
    }

    public class eCColorScale : classData
    {
        int Version;  // 0x00000001
        List<float[]> data;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<int>();
            int ArraySize = bs.Read<int>();
            data = new List<float[]>();
            if (ArraySize != 0)
            {
                int ItemCount = bs.Read<int>();
                for (int j = 0; j < ItemCount; j++)
                {
                    float[] q = new float[5];
                    for (int i = 0; i < 5; i++)
                        q[i] = bs.Read<float>();
                    data.Add(q);
                }
            }
        }

        public override int Size
        {
            get
            {
                int a = 8;
                if (data.Count != 0)
                    a += 4 + 20 * data.Count;
                return a;
            }
        }

        public override void Serialize(IFile s)
        {
            s.Write<int>(Version);
            s.Write<int>(data.Count != 0 ? 1 : 0);
            if (data.Count > 0)
            {
                s.Write<int>(data.Count);
                foreach (float[] f in data)
                    s.Write<float>(f);
            }
        }
    }

    public class eCTexCoordSrcProxy : classData
    {
        int VertexTexCoord;
        bCGuid Token;

        public eCTexCoordSrcProxy()
        {
            VertexTexCoord = 0;
            Token = new bCGuid(Guid.Empty, false);
        }

        public override void deSerialize(IFile bs)
        {
            VertexTexCoord = bs.Read<int>();
            Token = new bCGuid(bs);
        }

        public override int Size
        {
            get { return 4 + Token.Size; }
        }

        public override void Serialize(IFile s)
        {
            s.Write<int>(VertexTexCoord);
            Token.Serialize(s);
        }
    }

    public class eCShaderEllementBase : classData
    {
        public short Version;
        public bCGuid Token;
        public bCRect EditorLayout;
        public byte Unknown;
        private eCShaderBase p;

        public eCShaderEllementBase()
        {
            Version = 2;
            Unknown = 0;
            Token = new bCGuid(Guid.NewGuid());
            EditorLayout = new bCRect(496, 264, 240, 168);
        }

        public eCShaderEllementBase(bCGuid G, bCRect r, eCShaderBase P)
        {
            Version = 2;
            Unknown = 0;
            Token = G;
            EditorLayout = r;
            p = P;
        }

        public override void deSerialize(IFile s)
        {
            Version = s.Read<short>();
            Token = new bCGuid(s);
            EditorLayout = new bCRect(s);
            if (Version >= 2)
                Unknown = s.Read<byte>();
        }

        public override int Size
        {
            get { return 2 + Token.Size + EditorLayout.Size + (Version >= 2 ? 1 : 0); }
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            Token.Serialize(s);
            EditorLayout.Serialize(s);
            if (Version >= 2)
                s.Write<byte>(Unknown);
        }

        public eCShaderBase Parent
        {
            get
            {
                return p;
            }
            set
            {
                p = value;
                foreach (System.Reflection.FieldInfo m in this.GetType().GetFields())
                {
                    if (m.FieldType == typeof(eCColorSrcProxy))
                    {
                        object v = m.GetValue(this);
                        if( v != null)
                            (v as eCColorSrcProxy).Parent = value;
                    }
                }
            }
        }
    }

    public class eCMaterialResource2 : classData
    {
        short Version;

        public eCMaterialResource2()
        {
            Version = 201;
        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
        }

        public override int Size
        {
            get { return 2; }
        }
    }

    #region MainClasses
    public class eCShaderBase : eCShaderEllementBase
    {
        new public short Version;
        public bCAccessorPropertyObject[] ShaderEllement;

        public eCShaderBase()
        {
            
        }

        public eCShaderBase(int N)
        {
            Version = 1;
            ShaderEllement = new bCAccessorPropertyObject[N];
        }

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            base.deSerialize(bs);
            int ShaderEllementCount = bs.Read<int>();
            ShaderEllement = new bCAccessorPropertyObject[ShaderEllementCount];
            for (int i = 0; i < ShaderEllementCount; i++)
            {
                ShaderEllement[i] = new bCAccessorPropertyObject(bs);
                (ShaderEllement[i].Class as eCShaderEllementBase).Parent = this;
            }

        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            base.Serialize(s);
            s.Write<int>(ShaderEllement.Length);
            foreach (bCAccessorPropertyObject a in ShaderEllement)
                a.Serialize(s);
        }

        public override int Size
        {
            get
            {
                int a = 0;
                foreach (bCAccessorPropertyObject b in ShaderEllement)
                    a += b.Size;
                return 2 + base.Size + 4 + a;
            }
        }

        public eCShaderEllementBase GetEllementByGuid(bCGuid G)
        {
            foreach (bCAccessorPropertyObject a in ShaderEllement)
                if ((a.Class as eCShaderEllementBase).Token == G)
                    return a.Class as eCShaderEllementBase;
            return null;
        }
    }

    public class eCShaderDefault : eCShaderBase
    {
        public struct GenericSampler
        {
            public Color4? Color;
            public string Texture;

            public GenericSampler(string Tex)
            {
                Color = null;
                Texture = Tex;
            }

            public GenericSampler(Color4 c)
            {
                Color = c;
                Texture = null;
            }

            public bCAccessorPropertyObject CreateSampler()
            {
                if (Color.HasValue)
                    return eCColorSrcConstant.CreateNew(Color.Value);
                else if (Texture != null)
                    return eCColorSrcSampler.CreateNew(Texture);
                else return null;
            }

            public bool IsValid
            {
                get
                {
                    return Color.HasValue || Texture != null;
                }
            }
        }

        new public short Version;
        public eCColorSrcProxy ColorSrcDiffuse;
        public eCColorSrcProxy ColorSrcOpacity;
        public eCColorSrcProxy ColorSrcSelfIllumination;
        public eCColorSrcProxy ColorSrcSpecular;
        public eCColorSrcProxy ColorSrcSpecularPower;
        public eCColorSrcProxy ColorSrcNormal;
        public eCColorSrcProxy ColorSrcDistortion;
        public eCColorSrcProxy ColorSrcLightEmission;

        public eCShaderDefault() { }

        public eCShaderDefault(GenericSampler a_Diffuse, GenericSampler a_Opacity, GenericSampler a_Specular, GenericSampler a_SpecularPower)
            : base(1)
        {
            Version = 5;
            ColorSrcDiffuse = ColorSrcOpacity = ColorSrcSelfIllumination = ColorSrcSpecular = ColorSrcSpecularPower = ColorSrcNormal = ColorSrcDistortion = ColorSrcLightEmission = new eCColorSrcProxy(this);
            List<bCAccessorPropertyObject> B = new List<bCAccessorPropertyObject>();
            Func<GenericSampler, eCColorSrcProxy> A = (x) =>
            {
                if (x.IsValid)
                {
                    bCAccessorPropertyObject b = x.CreateSampler();
                    B.Add(b);
                    return new eCColorSrcProxy((b.Class as eCShaderEllementBase).Token, this);
                }
                else return new eCColorSrcProxy(this);
            };
            ColorSrcDiffuse = A(a_Diffuse);
            ColorSrcOpacity = A(a_Opacity);
            ColorSrcSpecular = A(a_Specular);
            ColorSrcSpecularPower = A(a_SpecularPower);
            ShaderEllement = B.ToArray();
        }

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            ColorSrcDiffuse = new eCColorSrcProxy(bs, this);
            ColorSrcOpacity = new eCColorSrcProxy(bs, this);
            ColorSrcSelfIllumination = new eCColorSrcProxy(bs, this);
            ColorSrcSpecular = new eCColorSrcProxy(bs, this);
            ColorSrcSpecularPower = new eCColorSrcProxy(bs, this);
            ColorSrcNormal = new eCColorSrcProxy(bs, this);
            if (Version >= 2)
                ColorSrcDistortion = new eCColorSrcProxy(bs, this);
            else ColorSrcDistortion = new eCColorSrcProxy(new bCGuid(), this);
            if (Version >= 3)
                ColorSrcLightEmission = new eCColorSrcProxy(bs, this);
            else ColorSrcLightEmission = new eCColorSrcProxy(new bCGuid(), this);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(5);
            ColorSrcDiffuse.Serialize(s);
            ColorSrcOpacity.Serialize(s);
            ColorSrcSelfIllumination.Serialize(s);
            ColorSrcSpecular.Serialize(s);
            ColorSrcSpecularPower.Serialize(s);
            ColorSrcNormal.Serialize(s);
            ColorSrcDistortion.Serialize(s);
            ColorSrcLightEmission.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + ColorSrcDiffuse.Size + ColorSrcOpacity.Size + ColorSrcSelfIllumination.Size + ColorSrcSpecular.Size 
                    + ColorSrcSpecularPower.Size + ColorSrcNormal.Size + ColorSrcDistortion.Size + ColorSrcLightEmission.Size + base.Size;
            }
        }
    }

    public class eCShaderLeafs : eCShaderDefault
    {

    }

    public class eCShaderLightStreaks : eCShaderBase
    {
        new public short Version;
        public eCColorSrcProxy ColorSrcDiffuse;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            ColorSrcDiffuse = new eCColorSrcProxy(bs, this);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            ColorSrcDiffuse.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + ColorSrcDiffuse.Size + base.Size;
            }
        }
    }

    public class eCShaderParticle : eCShaderBase
    {
        new public short Version;
        public eCColorSrcProxy ColorSrcDiffuse;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            ColorSrcDiffuse = new eCColorSrcProxy(bs, this);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            ColorSrcDiffuse.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + ColorSrcDiffuse.Size + base.Size;
            }
        }
    }

    public class eCShaderSkyDome : eCShaderBase
    {
        new public short Version;
        public eCColorSrcProxy ColorSrcClouds;
        public eCColorSrcProxy ColorSrcAbsorbation;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            ColorSrcClouds = new eCColorSrcProxy(bs, this);
            ColorSrcAbsorbation = new eCColorSrcProxy(bs, this);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            ColorSrcClouds.Serialize(s);
            ColorSrcAbsorbation.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + ColorSrcClouds.Size + ColorSrcAbsorbation.Size + base.Size;
            }
        }
    }
    #endregion

    #region SubClasses
    public abstract class subClassBase : eCShaderEllementBase
    {
        public abstract ShaderResourceTexture CreateTexture(API_Device D);
    }

    public abstract class eCColorSrcBase : subClassBase
    {
        new public short Version;

        public eCColorSrcBase() { Version = 1; }

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + base.Size;
            }
        }

        public override ShaderResourceTexture CreateTexture(API_Device D)
        {
            this.GetType();
            return null;
        }
    }

    public class eCColorSrcBlend : eCColorSrcBase
    {
        new public short Version;  // 0x0001
        public eCColorSrcProxy ColorSrc1;
        public eCColorSrcProxy ColorSrc2;
        public eCColorSrcProxy BlendSrc;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            ColorSrc1 = new eCColorSrcProxy(bs, Parent);
            ColorSrc2 = new eCColorSrcProxy(bs, Parent);
            BlendSrc = new eCColorSrcProxy(bs, Parent);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            ColorSrc1.Serialize(s);
            ColorSrc2.Serialize(s);
            BlendSrc.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + ColorSrc1.Size + ColorSrc2.Size + BlendSrc.Size + base.Size;
            }
        }

        public eCShaderEllementBase ColorSource0
        {
            get
            {
                return Parent.GetEllementByGuid(ColorSrc1.Token);
            }
        }

        public eCShaderEllementBase ColorSource1
        {
            get
            {
                return Parent.GetEllementByGuid(ColorSrc2.Token);
            }
        }

        public eCShaderEllementBase BlendSource
        {
            get
            {
                return Parent.GetEllementByGuid(BlendSrc.Token);
            }
        }

        public override ShaderResourceTexture CreateTexture(API_Device D)
        {
            return (ColorSrc1.Operand as subClassBase).CreateTexture(D);
            ShaderResourceTexture src0 = ColorSrc1.CreateTexture(D);
            ShaderResourceTexture src1 = ColorSrc1.CreateTexture(D);
            ShaderResourceTexture src2 = BlendSrc.CreateTexture(D);
            int w = Math.Max(Math.Max(src0.Width, src1.Width), src2.Width);
            int h = Math.Max(Math.Max(src0.Height, src1.Height), src2.Height);
            ShaderResourceTexture R = new ShaderResourceTexture(w, h, 1, SlimDX.Direct3D11.ResourceUsage.Default, SlimDX.Direct3D11.BindFlags.ShaderResource, SlimDX.Direct3D11.CpuAccessFlags.None, SlimDX.DXGI.Format.B8G8R8A8_UNorm, 1, SlimDX.Direct3D11.ResourceOptionFlags.None, D);
            FileManager.e_MatEffect.Variables["sourceTex0"].SetVariable(src0.ShaderResourceView);
            FileManager.e_MatEffect.Variables["sourceTex1"].SetVariable(src1.ShaderResourceView);
            FileManager.e_MatEffect.Variables["sourceTex2"].SetVariable(src2.ShaderResourceView);
            FileManager.e_MatEffect.Variables["dim"].SetVariable(new Vector2(w, h));
            FileManager.g_pApp.PauseRendering();
            FileManager.r_MatTarget.SetTarget(ClearType.All);
            FullScreenQuad.RenderQuad(FileManager.e_MatEffect.Techniques["Lerp"].Passes[0]);
            R.CopyFrom(FileManager.r_MatTarget, 0, 0, R.Width, 0, R.Height, 0, 0);
            FileManager.g_pApp.ResumeRendering();
            return R;
        }
    }

    public class eCColorSrcCamDistance : eCColorSrcBase
    {
        new public short Version;  // 0x0001

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + base.Size;
            }
        }
    }

    public class eCColorSrcCamObstacle : eCColorSrcBase
    {
        new public short Version;  // 0x0001

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + base.Size;
            }
        }
    }

    public class eCColorSrcCombiner : eCColorSrcBase
    {
        new public short Version;  // 0x0001
        public eCColorSrcProxy ColorSrc1;
        public eCColorSrcProxy ColorSrc2;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            ColorSrc1 = new eCColorSrcProxy(bs, Parent);
            ColorSrc2 = new eCColorSrcProxy(bs, Parent);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            ColorSrc1.Serialize(s);
            ColorSrc2.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + ColorSrc1.Size + ColorSrc2.Size + base.Size;
            }
        }

        public eCShaderEllementBase Source0
        {
            get
            {
                return Parent.GetEllementByGuid(ColorSrc1.Token);
            }
        }

        public eCShaderEllementBase Source1
        {
            get
            {
                return Parent.GetEllementByGuid(ColorSrc2.Token);
            }
        }

        public override ShaderResourceTexture CreateTexture(API_Device D)
        {
            return (Source0 as subClassBase).CreateTexture(D);
            eEColorSrcCombinerType q = (eEColorSrcCombinerType)base.Container.Properties["CombinerType"].Object;
            ShaderResourceTexture src0 = ColorSrc1.CreateTexture(D);
            ShaderResourceTexture src1 = ColorSrc2.CreateTexture(D);
            if (src0 == null || src1 == null)
                return src0 != null ? src0 : src1;
            ShaderResourceTexture R = new ShaderResourceTexture(Math.Max(src0.Width, src1.Width), Math.Max(src0.Height, src1.Height), 1, SlimDX.Direct3D11.ResourceUsage.Default, SlimDX.Direct3D11.BindFlags.ShaderResource, SlimDX.Direct3D11.CpuAccessFlags.None, SlimDX.DXGI.Format.B8G8R8A8_UNorm, 1, SlimDX.Direct3D11.ResourceOptionFlags.None, D);
            FileManager.e_MatEffect.Variables["sourceTex0"].SetVariable(src0.ShaderResourceView);
            FileManager.e_MatEffect.Variables["sourceTex1"].SetVariable(src1.ShaderResourceView);
            FileManager.e_MatEffect.Variables["dim"].SetVariable(new Vector2(R.Width, R.Height));
            FileManager.g_pApp.PauseRendering();
            FileManager.r_MatTarget.SetTarget(ClearType.All);
            switch (q)
            {
                case eEColorSrcCombinerType.eEColorSrcCombinerType_Add :
                    FullScreenQuad.RenderQuad(FileManager.e_MatEffect.Techniques["Combiner_Add"].Passes[0]);
                    break;
                case eEColorSrcCombinerType.eEColorSrcCombinerType_Subtract:
                    FullScreenQuad.RenderQuad(FileManager.e_MatEffect.Techniques["Combiner_Sub"].Passes[0]);
                    break;
                case eEColorSrcCombinerType.eEColorSrcCombinerType_Multiply:
                    FullScreenQuad.RenderQuad(FileManager.e_MatEffect.Techniques["Combiner_Mul"].Passes[0]);
                    break;
                case eEColorSrcCombinerType.eEColorSrcCombinerType_Max:
                    FullScreenQuad.RenderQuad(FileManager.e_MatEffect.Techniques["Combiner_Max"].Passes[0]);
                    break;
                case eEColorSrcCombinerType.eEColorSrcCombinerType_Min:
                    FullScreenQuad.RenderQuad(FileManager.e_MatEffect.Techniques["Combiner_Min"].Passes[0]);
                    break;
            }
            R.CopyFrom(FileManager.r_MatTarget, 0, 0, R.Width, 0, R.Height, 0, 0);
            FileManager.g_pApp.ResumeRendering();
            //src0.ToFile("src0.dds", SlimDX.Direct3D11.ImageFileFormat.Dds);
            //src1.ToFile("src1.dds", SlimDX.Direct3D11.ImageFileFormat.Dds);
            //R.ToFile("res.dds", SlimDX.Direct3D11.ImageFileFormat.Dds);
            return R;
        }
    }

    public class eCColorSrcConstant : eCColorSrcBase
    {
        new public short Version;  // 0x0001

        public eCColorSrcConstant() { Version = 1; }

        public static bCAccessorPropertyObject CreateNew(Color4 c)
        {
            bCAccessorPropertyObject a = new bCAccessorPropertyObject(new eCColorSrcConstant());
            a.Properties.addProperty("Color", "bCFloatColor", new bCFloatColor(c.ToColor3()));
            a.Properties.addProperty("Alpha", "float", c.Alpha);
            return a;
        }

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + base.Size;
            }
        }

        public override ShaderResourceTexture CreateTexture(API_Device D)
        {
            bCFloatColor q0 = (bCFloatColor)base.Container.Properties["Color"].Object;
            float al = (float)base.Container.Properties["Alpha"].Object;
            Color4 q = new Color4((Color3)q0);
            q.Alpha = al;
            ShaderResourceTexture s = new ShaderResourceTexture(q, D);
            return s;
        }
    }

    public class eCColorSrcConstantAnimation : eCColorSrcBase
    {
        new public short Version;  // 0x0001
        public eCColorScale ColorScale;
        public eCColorSrcBase Inherited;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            ColorScale = new eCColorScale();
            ColorScale.deSerialize(bs);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            ColorScale.Serialize(s);
            Inherited.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + ColorScale.Size + Inherited.Size + base.Size;
            }
        }
    }

    public class eCColorSrcConstantSwitch : eCColorSrcBase
    {
        new public short Version;  // 0x0001

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + base.Size;
            }
        }

        public override ShaderResourceTexture CreateTexture(API_Device D)
        {
            bTObjArray<bCFloatAlphaColor> Colors = base.Container.Properties["Colors"].Object as bTObjArray<bCFloatAlphaColor>;
            if (Colors == null || Colors.Length == 0)
                return ShaderResourceTexture.WhiteTexture;
            return new ShaderResourceTexture(Colors[0], D);
        }
    }

    public class eCColorSrcCubeSampler : eCColorSrcBase
    {
        new short Version;  // 0x0001

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + base.Size;
            }
        }

        public override ShaderResourceTexture CreateTexture(API_Device D)
        {
            bCImageResourceString s = base.Container.Properties["ImageFilePath"].Object as bCImageResourceString;
            return new ShaderResourceTexture(s.pString, D);
        }
    }

    public class eCColorSrcFresnel : eCColorSrcBase
    {
        new short Version;  // 0x0001

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + base.Size;
            }
        }
    }

    public class eCColorSrcMask : eCColorSrcBase
    {
        new short Version;  // 0x0001
        eCColorSrcProxy Color;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            Color = new eCColorSrcProxy(bs, Parent);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            Color.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + Color.Size + base.Size;
            }
        }
    }

    public class eCColorSrcSampler : eCColorSrcBase
    {
        new public short Version;  // 0x0001
        public eCTexCoordSrcProxy TexCoord;
        new public int Unknown;  // 0xFFFFFFFF

        public eCColorSrcSampler()
        {
            Version = 1;
            TexCoord = new eCTexCoordSrcProxy();
            Unknown = -1;
        }

        public static bCAccessorPropertyObject CreateNew(string a_File)
        {
            bCAccessorPropertyObject a = new bCAccessorPropertyObject(new eCColorSrcSampler());
            a.Properties.addProperty("ImageFilePath", "bCImageResourceString", new bCImageResourceString(a_File));
            a.Properties.addProperty("TexRepeatU", "bTPropertyContainer<enum eEColorSrcSampleTexRepeat>", eEColorSrcSampleTexRepeat.eEColorSrcSampleTexRepeat_Wrap);
            a.Properties.addProperty("TexRepeatV", "bTPropertyContainer<enum eEColorSrcSampleTexRepeat>", eEColorSrcSampleTexRepeat.eEColorSrcSampleTexRepeat_Wrap);
            a.Properties.addProperty("AnimationSpeed", "float", 0.0f);
            a.Properties.addProperty("SwitchRepeat", "bTPropertyContainer<enum eEColorSrcSwitchRepeat>", eEColorSrcSwitchRepeat.eEColorSrcSwitchRepeat_Repeat);
            a.Properties.addProperty("SwitchBegin", "int", 0);
            //a.Properties.addProperty("SRGBRead", "bool", false);
            return a;
        }

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            TexCoord = new eCTexCoordSrcProxy();
            TexCoord.deSerialize(bs);
            Unknown = bs.Read<int>();
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            TexCoord.Serialize(s);
            s.Write<int>(Unknown);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 6 + TexCoord.Size + base.Size;
            }
        }

        public string TexturePath
        {
            get
            {
                string s = (base.Container.Properties["ImageFilePath"].Object as bCString).pString;
                return s;
            }
        }

        public override ShaderResourceTexture CreateTexture(API_Device D)
        {
            if (TexturePath == "._ximg")
                return null;
            return new ShaderResourceTexture(TexturePath, D);
        }
    }

    public class eCColorSrcSkydomeSampler : eCColorSrcBase
    {
        new short Version;  // 0x0001

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + base.Size;
            }
        }
    }

    public class eCColorSrcVertexColor : eCColorSrcBase
    {
        new short Version;  // 0x0001

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + base.Size;
            }
        }
    }

    public abstract class eCTexCoordSrcBase : subClassBase
    {
        new short Version;  // 0x0001

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            base.deSerialize(bs);
        }

        public override ShaderResourceTexture CreateTexture(API_Device D)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + base.Size;
            }
        }
    }

    public class eCTexCoordSrcBumpOffset : eCTexCoordSrcBase
    {
        new short Version;  // 0x0001
        eCColorSrcProxy Height;
        eCTexCoordSrcProxy TexCoord;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            Height = new eCColorSrcProxy(bs, Parent);
            TexCoord = new eCTexCoordSrcProxy();
            TexCoord.deSerialize(bs);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            Height.Serialize(s);
            TexCoord.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + Height.Size + TexCoord.Size + base.Size;
            }
        }
    }

    public class eCTexCoordSrcColor : eCTexCoordSrcBase
    {
        new short Version;  // 0x0001
        eCColorSrcProxy ColorU;
        eCColorSrcProxy ColorV;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            ColorU = new eCColorSrcProxy(bs, Parent);
            ColorV = new eCColorSrcProxy(bs, Parent);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            ColorU.Serialize(s);
            ColorV.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + ColorU.Size + ColorV.Size + base.Size;
            }
        }
    }

    public class eCTexCoordSrcOscillator : eCTexCoordSrcBase
    {
        new short Version;  // 0x0001
        eCTexCoordSrcProxy TexCoord;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            TexCoord = new eCTexCoordSrcProxy();
            TexCoord.deSerialize(bs);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            TexCoord.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + TexCoord.Size + base.Size;
            }
        }
    }

    public class eCTexCoordSrcReflect : eCTexCoordSrcBase
    {
    }

    public class eCTexCoordSrcRotator : eCTexCoordSrcBase
    {
        new short Version;  // 0x0001
        eCTexCoordSrcProxy TexCoord;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            TexCoord = new eCTexCoordSrcProxy();
            TexCoord.deSerialize(bs);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            TexCoord.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + TexCoord.Size + base.Size;
            }
        }
    }

    public class eCTexCoordSrcScale : eCTexCoordSrcBase
    {
        new short Version;  // 0x0001
        eCTexCoordSrcProxy TexCoord;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            TexCoord = new eCTexCoordSrcProxy();
            TexCoord.deSerialize(bs);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            TexCoord.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + TexCoord.Size + base.Size;
            }
        }
    }

    public class eCTexCoordSrcScroller : eCTexCoordSrcBase
    {
        new short Version;  // 0x0001
        eCTexCoordSrcProxy TexCoord;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            TexCoord = new eCTexCoordSrcProxy();
            TexCoord.deSerialize(bs);
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            TexCoord.Serialize(s);
            base.Serialize(s);
        }

        public override int Size
        {
            get
            {
                return 2 + TexCoord.Size + base.Size;
            }
        }
    }
    #endregion
}
