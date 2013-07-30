using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RisenEditorWin32Interop;
using System.IO;
using SlimDX;
using RisenEditor.Code.Loader;
using GameLibrary.IO;
using System.Text.RegularExpressions;
using GameLibrary;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.Code
{
    public class WavefrontMaterial
    {
        public string Name { get; set; }
        public Vector3 Ambient { get; set; }
        public Vector3 Diffuse { get; set; }
        public Vector3 Specular { get; set; }
        public string DiffuseTexture { get; set; }
        public string SpecularTexture { get; set; }
        public float SpecularPower { get; set; }

        public static string[] FilteredSplit(string strIn)
        {
            string[] valuesUnfiltered = strIn.Replace(".", ",").Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return valuesUnfiltered;
        }

        public static WavefrontMaterial[] LoadFromFile(string a_File)
        {
            List<WavefrontMaterial> materials = new List<WavefrontMaterial>();
            WavefrontMaterial currentMaterial = new WavefrontMaterial();
            bool first = true;
            StreamReader sr = new StreamReader(a_File);

            //Read the first line of text
            string line = sr.ReadLine();

            //Continue to read until you reach end of file
            while (line != null)
            {
                if (line.StartsWith("\t"))
                    line = line.Remove(0, 1);
                if (line.StartsWith("#"))
                {
                    // Nothing to read, these are comments.
                }
                else if (line.StartsWith("newmtl"))
                {
                    if (!first)
                    {
                        materials.Add(currentMaterial);
                        currentMaterial = new WavefrontMaterial();
                    }
                    first = false;
                    currentMaterial.Name = line.Substring(7);
                    if (currentMaterial.Name.Last() == ' ')
                        currentMaterial.Name = currentMaterial.Name.Remove(currentMaterial.Name.Length - 1, 1);
                }
                else if (line.StartsWith("Ka"))
                {
                    string tmp = line.Substring(3);

                    string[] values = FilteredSplit(tmp);


                    currentMaterial.Ambient = new Vector3(float.Parse(values[0], System.Globalization.CultureInfo.InstalledUICulture),
                                                float.Parse(values[1], System.Globalization.CultureInfo.InstalledUICulture),
                                                float.Parse(values[2], System.Globalization.CultureInfo.InstalledUICulture));
                }
                else if (line.StartsWith("Kd"))
                {
                    string tmp = line.Substring(3);

                    string[] values = FilteredSplit(tmp);

                    currentMaterial.Diffuse = new Vector3(float.Parse(values[0], System.Globalization.CultureInfo.InstalledUICulture),
                            float.Parse(values[1], System.Globalization.CultureInfo.InstalledUICulture),
                            float.Parse(values[2], System.Globalization.CultureInfo.InstalledUICulture));
                }

                else if (line.StartsWith("Ks"))
                {
                    string tmp = line.Substring(3);

                    string[] values = FilteredSplit(tmp);

                    currentMaterial.Specular = new Vector3(float.Parse(values[0], System.Globalization.CultureInfo.InstalledUICulture),
                            float.Parse(values[1], System.Globalization.CultureInfo.InstalledUICulture),
                            float.Parse(values[2], System.Globalization.CultureInfo.InstalledUICulture));
                }
                else if (line.StartsWith("map_Kd"))
                {
                    string tmp = line.Substring(7);
                    if (tmp.Last() == ' ')
                        tmp = tmp.Remove(tmp.Length - 1, 1);
                    currentMaterial.DiffuseTexture = tmp;
                }
                else if (line.StartsWith("map_Ks"))
                {
                    string tmp = line.Substring(7);
                    if (tmp.Last() == ' ')
                        tmp = tmp.Remove(tmp.Length - 1, 1);
                    currentMaterial.SpecularTexture = tmp;

                }
                else if (line.StartsWith("Ns"))
                {
                    string tmp = line.Substring(2).Replace(".", ",");
                    currentMaterial.SpecularPower = float.Parse(tmp, System.Globalization.CultureInfo.InstalledUICulture);
                }

                //Read the next line
                line = sr.ReadLine();
            }
            materials.Add(currentMaterial);

            //close the file
            sr.Close();

            //string ABC = File.ReadAllText(a_File);
            //foreach(string s in E)
            //    ABC = ABC.Replace(s, RisenEditorWin32Interop.MeshConverter.MapFilename(s));
            //File.WriteAllText(a_File, ABC);

            return materials.ToArray();
        }
    }

    public static class LevelMeshConverter
    {
        static API_Device D;
        static string MainFolder;

        static string ConvertImage(string a_OriginalImage, DirectoryInfo a_Dir)
        {
            if (!a_OriginalImage.EndsWith("._ximg"))
            {
                FileInfo FI = null;
                if (File.Exists(a_Dir.FullName + "/" + a_OriginalImage))
                    FI = new FileInfo(a_Dir.FullName + "/" + a_OriginalImage);
                else if (File.Exists(MainFolder + "/" + a_OriginalImage))
                    FI = new FileInfo(MainFolder + "/" + a_OriginalImage);

                if (FI != null && FI.Exists)
                {
                    string s2 = FI.Name.Replace(FI.Extension, "");
                    string r = s2 + "._ximg";
                    EFile e = FileManager.CreateNewPhysicalFile(FileManager.NewFileType.Texture, r, false);
                    if (!e.IsOpenable)
                    {
                        ShaderResourceTexture T = new ShaderResourceTexture(FI.FullName, D);
                        ximg X = new ximg(T, new EFile(FI.FullName, new StdFileReader()));
                        eCFile Fi = eCFile.CreateNew(e);
                        Fi.Save(X);
                        Fi.Close();
                    }
                    return r;
                }
            }
            return a_OriginalImage;
        }

        public static void Convert(API_Device D, string a_ObjFile, Vector3 a_Size, string a_ObjColFile = null, string a_Root = null)
        {
            LevelMeshConverter.D = D;
            LevelMeshConverter.MainFolder = a_Root;

            FileInfo F = new FileInfo(a_ObjFile);
            string objn = F.Name.Replace(F.Extension, "");
            FileInfo M = new FileInfo(F.FullName.Replace(F.Extension, "") + ".mtl");
            if (M.Exists)
            {
                WavefrontMaterial[] Mats = WavefrontMaterial.LoadFromFile(M.FullName);
                foreach (WavefrontMaterial m in Mats)
                {
                    if (m.Name == null)
                        continue;
                    if (m.Name.EndsWith("._xmat"))
                        break;
                    EFile e = FileManager.CreateNewPhysicalFile(FileManager.NewFileType.Material, RisenEditorWin32Interop.MeshConverter.MapFilename(m.Name, objn) + "._xmat", false);
                    if (e.IsOpenable)
                        continue;
                    XmatWrapper m2 = null;

                    Func<string, Vector3, eCShaderDefault.GenericSampler> G = (x, y) =>
                    {
                        if (!string.IsNullOrEmpty(x))
                        {
                            x = ConvertImage(x, F.Directory);
                            return new eCShaderDefault.GenericSampler(x);
                        }
                        else return new eCShaderDefault.GenericSampler(new Color4(y));
                    };
                    eCShaderDefault.GenericSampler d = G(m.DiffuseTexture, m.Diffuse), s = G(m.SpecularTexture, m.Specular), sp = new eCShaderDefault.GenericSampler(new Color4(1, m.SpecularPower, m.SpecularPower, m.SpecularPower)), op = new eCShaderDefault.GenericSampler(new Color4(1));
                    m2 = new XmatWrapper(e, d, d, s, sp);
                    m2.ToFile();
                }
            }

            if (a_ObjColFile == null)
                a_ObjColFile = a_ObjFile;
            EFile t0 = FileManager.GetFile(objn + "._xmsh"), t1 = FileManager.GetFile(objn + "_COL._xcom");
            if(t0.IsOpenable && t1.IsOpenable)
                return;
            EFile e0 = FileManager.CreateNewPhysicalFile(FileManager.NewFileType.StaticModel, objn + "._xmsh"), e1 = FileManager.CreateNewPhysicalFile(FileManager.NewFileType.StaticModel, objn + "_COL._xcom");
            MeshConverter.Convert(a_ObjFile, e0.Path, e1.Path, a_Size.X, a_Size.Y, a_Size.Z);
        }
    }
}
