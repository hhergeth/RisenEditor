using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameLibrary;
using GameLibrary.Objekte;
using GameLibrary.IO;
using RisenEditor.Code;
using RisenEditor.Code.RisenTypes;
using System.IO;
using SlimDX;

namespace RisenEditor.Code.Loader
{
    public class xshmat : BinaryFileBlock
    {
        public bCAccessorPropertyObject propObj;
        public bCAccessorPropertyObject dataObj;
        public DateTime Time;

        public xshmat(IFile a_File)
        {
            deSerialize(a_File);
        }

        public xshmat(DateTime T)
        {
            Time = T;
        }

        public override void deSerialize(IFile a_File)
        {
            a_File.Position += 8;
            int poff = a_File.Read<int>();
            int plen = a_File.Read<int>();
            int doff = a_File.Read<int>();
            int dlen = a_File.Read<int>();
            Time = DateTime.FromFileTime(a_File.Read<long>());
            char[] A = a_File.Read<char>(8);
            propObj = new bCAccessorPropertyObject(a_File);
            dataObj = new bCAccessorPropertyObject(a_File);
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<char>("GR01SM01".ToCharArray());
            a_File.Write<int>(40);
            a_File.Write<int>(propObj.Size);
            a_File.Write<int>(40 + propObj.Size);
            a_File.Write<int>(dataObj.Size);
            a_File.Write<long>(Time.ToFileTime());
            a_File.Write<char>(".xshmat".ToCharArray());
            a_File.Write<byte>(1);
            propObj.Serialize(a_File);
            dataObj.Serialize(a_File);
        }

        public override int Size
        {
            get { return 40 + propObj.Size + dataObj.Size; }
        }
    }

    public class XmatWrapper
    {
        public xshmat Mat;
        EFile file;

        private void add0()
        {
            bCAccessorPropertyObject propObj = Mat.propObj;
            propObj.Properties.addProperty("PhysicMaterial", "bTPropertyContainer<enum eEShapeMaterial>", eEShapeMaterial.eEShapeMaterial_None);
            propObj.Properties.addProperty("IgnoredByTraceRay", "bool", false);
            propObj.Properties.addProperty("DisableCollision", "bool", false);
            propObj.Properties.addProperty("DisableResponse", "bool", false);
        }

        private void add1()
        {
            bCAccessorPropertyObject dataObj = Mat.dataObj;
            dataObj.Properties.addProperty("TransformationType", "bTPropertyContainer<enum eEShaderMaterialTransformation>", eEShaderMaterialTransformation.eEShaderMaterialTransformation_Default);
            dataObj.Properties.addProperty("EnableSpecular", "bool", true);
            dataObj.Properties.addProperty("DisableLighting", "bool", false);
            dataObj.Properties.addProperty("DisableFog", "bool", false);
            dataObj.Properties.addProperty("DisableDirLightShadows", "bool", false);
            dataObj.Properties.addProperty("DisablePntLightShadows", "bool", false);
            dataObj.Properties.addProperty("DisableAmbientOcclusion", "bool", false);
            dataObj.Properties.addProperty("CompressedNormalMap", "bool", true);
            dataObj.Properties.addProperty("BRDFLightingType", "bTPropertyContainer<enum eEShaderMaterialBRDFType>", eEShaderMaterialBRDFType.eEShaderMaterialBRDFType_Complex);
            dataObj.Properties.addProperty("EnableAlphaZWrite", "bool", false);
            dataObj.Properties.addProperty("BlendMode", "bTPropertyContainer<enum eEShaderMaterialBlendMode>", eEShaderMaterialBlendMode.eEShaderMaterialBlendMode_AlphaBlend);//AAA
            dataObj.Properties.addProperty("MaskReference", "char", (char)0);
            dataObj.Properties.addProperty("MaxShaderVersion", "bTPropertyContainer<enum eEShaderMaterialVersion>", eEShaderMaterialVersion.eEShaderMaterialVersion_3_0);
            dataObj.Properties.addProperty("FallbackMaterial", "bCImageOrMaterialResourceString", new bCImageOrMaterialResourceString(""));
            dataObj.Properties.addProperty("UseDepthBias", "bool", false);
            dataObj.Properties.addProperty("TradeAlphaAsSolid", "bool", true);
            dataObj.Properties.addProperty("RenderPriority", "int", 100);
            dataObj.Properties.addProperty("FrozenOverlayMaterial", "bCString", new bCString("Special_Overlay_Frozen_Default._xmat"));
            dataObj.Properties.addProperty("BurnedOverlayMaterial", "bCString", new bCString("Special_Overlay_Burned_Default._xmat"));
            dataObj.Properties.addProperty("HighlightedOverlayMaterial", "bCString", new bCString("Special_Overlay_Highlighted_Default._xmat"));
        }

        public XmatWrapper(EFile E)
        {
            file = E;
            eCFile stream = new eCFile(E);
            Mat = new xshmat(stream);
            stream.Close();
        }

        public XmatWrapper(EFile a_Handle, eCShaderDefault.GenericSampler a_Diffuse, eCShaderDefault.GenericSampler a_Opacity, eCShaderDefault.GenericSampler a_Specular, eCShaderDefault.GenericSampler a_SpecularPower)
        {
            file = a_Handle;
            Mat = new xshmat(DateTime.Now);
            Mat.propObj = new bCAccessorPropertyObject(new eCMaterialResource2());
            Mat.dataObj = new bCAccessorPropertyObject(new eCShaderDefault(a_Diffuse, a_Opacity, a_Specular, a_SpecularPower));
            add0();
            add1();
        }

        public void ToFile()
        {
            eCFile E = eCFile.CreateNew(file);
            E.Save(Mat);
            E.Close();
        }
    }

    class XmatLoader : IMaterialLoader
    {
        public Material LoadMaterial(EFile _File, API_Device D)
        {
            if (_File.IsOpenable)
            {
                ShaderResourceTexture T = null;
                if (_File.Name == "Nat_Stone_Rock_01_Diffuse_01_Specular._xmat")
                    T = new ShaderResourceTexture("Nat_Stone_Rock_01_Diffuse_01._ximg", D);
                else if (_File.Name == "Special_Water_Swamp_01_Diffuse_01._xmat")
                    T = new ShaderResourceTexture("Special_Water_Caustics_01_Diffuse_01._ximg", D);
                else
                {
                    XmatWrapper W = new XmatWrapper(_File);

                    bCGuid src_Diffuse = (W.Mat.dataObj.Class as eCShaderDefault).ColorSrcDiffuse.Token;
                    bCGuid src_Normal = (W.Mat.dataObj.Class as eCShaderDefault).ColorSrcNormal.Token;
                    bCGuid src_Specular = (W.Mat.dataObj.Class as eCShaderDefault).ColorSrcSpecular.Token;
                    if (src_Diffuse.IsValid)
                    {
                        eCShaderDefault shader = (W.Mat.dataObj.Class as eCShaderDefault);
                        eCShaderEllementBase ellement = shader.GetEllementByGuid(shader.ColorSrcDiffuse.Token);
                        if (ellement is subClassBase)
                            T = (ellement as subClassBase).CreateTexture(D);
                    }
                    if (T == null)
                        T = ShaderResourceTexture.WhiteTexture;
                }
                return new Material(T);
            }
            else
            {
                return new Material("42", true, D);
            }
        }

        public bool RightLoader(string ext)
        {
            if(ext == "._xmat")
                return true;
            return false;
        }

        public IMaterialLoader CreateInstance()
        {
            return new XmatLoader();
        }
    }
}
