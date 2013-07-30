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
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.Code
{
    public static class FileManager
    {
        static Dictionary<string, tabFile> tab_files = new Dictionary<string, tabFile>();
        static Dictionary<string, tabFile> tab_FilesPrefix = new Dictionary<string, tabFile>();
        static Dictionary<string, RisenPak> m_Paks;
        static Dictionary<string, RisenPakFile> _Files = new Dictionary<string, RisenPakFile>(53474);
        static Dictionary<string, EFile> _PhyFiles = new Dictionary<string, EFile>();
        static FileSystemWatcher m_Watcher = new FileSystemWatcher();

        public static bool NotifyOnPhysicalWatch { get; set; }
        public static string g_pGamepath;
        public static EffectWrapper e_MatEffect;
        public static RenderTarget r_MatTarget;
        public static GameLibrary.D3DApplication g_pApp;
        
        static bool initFinished = false;

        static bool isArchiveExtension(FileInfo F)
        {
            string ex = F.Extension, k = F.Name.ToUpper();
            if (ex.Length == 4)
                if (ex.Substring(2, 2).IsInteger() && ex.ToUpper()[1] == 'P')
                    return true;
            if (k.EndsWith(".PAK"))
                return true;
            return false;
        }

        public static void InitFileManager(D3DApplication a_App, string a_Basepath = null)
        {
            NotifyOnPhysicalWatch = true;
            g_pApp = a_App;
            API_Device D = a_App.Device;

            e_MatEffect = new EffectWrapper("matCombiner.fx", D);
            r_MatTarget = new RenderTarget(2048, 2048, 1, SlimDX.Direct3D11.BindFlags.RenderTarget, SlimDX.DXGI.Format.B8G8R8A8_UNorm, 1, SlimDX.Direct3D11.ResourceOptionFlags.None, D);

            D.Content.FileHandleCreator = getHandle;
            D.Content.RegisterMaterialLoader(new XmatLoader());
            D.Content.RegisterModelLoader(new XMACLoader());
            D.Content.RegisterModelLoader(new XMSHLoader());
            D.Content.RegisterTextureLoader(new XIMGLoader());

            string basePath2 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Deep Silver\Risen\", "INSTALL_DIR", string.Empty) as string;
            if (basePath2 != null && Directory.Exists(basePath2))
            {//registry based
                g_pGamepath = basePath2.Replace(@"\bin", @"\");
            }
            else if(!string.IsNullOrEmpty(a_Basepath))
            {//ini file based
                g_pGamepath = a_Basepath + @"\";
            }
            else
            {
                var msg = "Couldn't find risen directory. Terminating";
                SystemLog.Append(LogImportance.System, msg);
                SystemLog.Flush();
                MessageBox.Show(msg);
                throw new Exception();
            }

            m_Paks = new Dictionary<string, RisenPak>();
            DirectoryInfo[] Ds = new DirectoryInfo[] { new DirectoryInfo(g_pGamepath + "/data/common"), new DirectoryInfo(g_pGamepath + "/data/compiled") };
            for (int i = 0; i < Ds.Length; i++)
            {
                if (!Ds[i].Exists)
                    continue;

                int n = -1;
                while (true)
                {
                    string a = n.ToString().PadLeft(2, '0');
                    FileInfo[] Fs = Ds[i].GetFiles(n < 0 ? ("*.pak") : ("*.p" + a));
                    foreach (FileInfo f in Fs)
                    {
                        string key = Ds[i].Name + @"\" + f.Name.Replace(f.Extension, "") + (n < 0 ? "" : a);
                        RisenPak val = new RisenPak(FileManager.GetFile(f.FullName), _Files, n >= 0);
                        m_Paks.Add(key, val);
                    }
                    n++;
                    if (Fs.Length == 0)
                        break;
                }
            }

            m_Watcher.Path = g_pGamepath;
            m_Watcher.IncludeSubdirectories = true;
            m_Watcher.NotifyFilter = NotifyFilters.FileName;
            m_Watcher.Created += W_Created;
            m_Watcher.Deleted += W_Created;
            m_Watcher.Renamed += W_Created;
            m_Watcher.EnableRaisingEvents = true;

            initFinished = true;

            StdFileReader S = new StdFileReader();
            Dictionary<string, EFile> _PhyFiles = new Dictionary<string, EFile>();
            List<EFile> newFiles = new List<EFile>();
            Stack<DirectoryInfo> Dirs = new Stack<DirectoryInfo>();
            Dirs.Push(new DirectoryInfo(g_pGamepath + "/Data"));
            while (Dirs.Count != 0)
            {
                DirectoryInfo d = Dirs.Pop();
                DirectoryInfo[] _d = d.GetDirectories();
                FileInfo[] _f = d.GetFiles();
                foreach (DirectoryInfo q0 in _d)
                    Dirs.Push(q0);
                foreach (FileInfo q1 in _f)
                {
                    string k = q1.Name.ToUpper();
                    if (!isArchiveExtension(q1))
                    {
                        EFile e = new EFile(q1.FullName, S);
                        if (_PhyFiles.ContainsKey(k))
                        {
                            SystemLog.Append(LogImportance.System, "Two identical physical files found!");
                            throw new Exception("Two identical physical files found!");
                        }
                        _PhyFiles.Add(k, e);
                        if (!FileManager._PhyFiles.ContainsKey(k))
                            newFiles.Add(e);
                    }
                }
            }
            FileManager._PhyFiles = _PhyFiles;

            List<EFile> tabfiles = new List<EFile>();
            System.IO.StreamReader tr = new System.IO.StreamReader(g_pGamepath + "data\\ini\\loc.ini");
            while (!tr.EndOfStream)
            {
                string l = tr.ReadLine();
                while (!tr.EndOfStream && string.IsNullOrEmpty(l))
                    l = tr.ReadLine();
                if (tr.EndOfStream)
                    break;
                string prefix = tr.ReadLine().Replace("prefix=", ""), csv = tr.ReadLine().Replace("csv=", ""), bin = tr.ReadLine().Replace("bin=", "");
                EFile a = GetFile(bin.Replace(@"#G3:/", ""));
                if (!a.IsOpenable)
                    SystemLog.Append(LogImportance.Warning, "Tab file not found : " + a.Name);
                tabFile b = new tabFile(a);
                tab_files.Add(a.Name, b);
                tab_FilesPrefix.Add(prefix.ToLower(), b);
            }
            tr.Close();
        }

        static void W_Created(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath))
                return;
            EFile e2 = null;
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                e2 = new EFile(e.FullPath, new StdFileReader());
                if(!_PhyFiles.ContainsKey(e2.Name.ToUpper()))
                    //lock (m_PhyBlock)
                    {
                        _PhyFiles.Add(e.Name.ToUpper(), e2);
                    }
            }
            else if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                e2 = new EFile(e.FullPath, new StdFileReader());
                if (!_PhyFiles.ContainsKey(e2.Name.ToUpper()))
                    //lock (m_PhyBlock)
                    {
                        _PhyFiles.Add(e.Name.ToUpper(), e2);
                    }
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {

            }
            if (NotifyOnPhysicalWatch && NewFilesFound != null && e2 != null)
                NewFilesFound(new List<EFile>() { e2});
        }

        public static EFile getHandle(string s, FileType t)
        {
            if (t == FileType.Effect)
            {
                if(File.Exists("resources/shaders/" + s))
                    return GetFile("resources/shaders/" + s);
            }
            else if (t == FileType.Effect_Include)
            {
                if (File.Exists("resources/shaders/" + s))
                    return GetFile("resources/shaders/" + s);
            }
            else if (t == FileType.Material && !s.EndsWith("mat"))
            {
                return GetFile(s + "._xmat");
            }
            if (initFinished)
                return GetFile(s);
            else return new EFile("", new StdFileReader());
        }

        public static EFile GetRoot(string Name2)
        {
            string Name = Name2.Replace(".pak", "").Replace("raw", "common").Replace("/", @"\");
            if (m_Paks.ContainsKey(Name))
                return new RPakFile(m_Paks[Name].root);

            if (m_Paks.ContainsKey(@"common\" + Name))
                return new RPakFile(m_Paks[@"common\" + Name].root);
            else if (m_Paks.ContainsKey(@"compiled\" + Name))
                return new RPakFile(m_Paks[@"compiled\" + Name].root);
            else return null;
        }

        public static EFile GetFile(string Name)
        {
            Name = Name.Replace("/", @"\");
            string s2 = Name.Split(@"\"[0]).Last();
            if (File.Exists(Name))
                return new EFile(Name, new StdFileReader());
            else if (Directory.Exists(Name))
                return new EFile(Name, new StdFileReader());
            else if (File.Exists(g_pGamepath + Name))
                return new EFile(g_pGamepath + Name, new StdFileReader());
            else if (Directory.Exists(g_pGamepath + Name))
                return new EFile(g_pGamepath + Name, new StdFileReader());

            s2 = s2.ToUpper();
            if (_PhyFiles.ContainsKey(s2))
                return _PhyFiles[s2];

            if (s2.EndsWith("XMSH") && !s2.EndsWith("_XMSH"))
                s2 = s2.Replace("XMSH", "_XMSH");
            else if (s2.EndsWith("XAC"))
                s2 = s2.Replace("XAC", "_XMAC");

            if (!_Files.ContainsKey(s2))
                return new EFile(Name, new StdFileReader());
            else return new RPakFile(_Files[s2]);
        }

        public static EFile[] GetFiles(params string[] Names)
        {
            EFile[] F = new EFile[Names.Length];
            for (int i = 0; i < Names.Length; i++)
                F[i] = GetFile(Names[i]);
            return F;
        }

        public static EFile[] GetPhysicalFiles(string a_Filter, bool IgnoreCase = false)
        {
            a_Filter = "^" + a_Filter.Replace(@".", @"\.").Replace(@"*", @".*").Replace("?", "(.{1,1})");
            System.Text.RegularExpressions.Regex R = new System.Text.RegularExpressions.Regex(a_Filter, IgnoreCase ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : System.Text.RegularExpressions.RegexOptions.None);
            List<EFile> f = new List<EFile>();
            var c = _PhyFiles.Values.ToArray();
            foreach (EFile e in c)
                if (R.IsMatch(e.Name))
                    f.Add(e);
            return f.ToArray();
        }

        public static EFile GetPhysicalFileLocation(string a_PakFilePath, bool a_Create = true)
        {
            FileInfo F = new FileInfo(g_pGamepath + a_PakFilePath);
            if (!F.Directory.Exists)
                F.Directory.Create();
            string a = F.Directory.FullName + "\\" + F.Name;
            EFile b = new EFile(a, new StdFileReader());
            if (a_Create)
                b.Create(FileMode.Create);
            return b;
        }

        public enum NewFileType
        {
            StaticModel,
            AnimModel,
            Texture,
            Material,
            Lrent,
            Sec,
            Wrl,
            Tab,
            Info,
            Quest,
        }
        public static EFile CreateNewPhysicalFile(NewFileType a_Type, string a_Name, bool a_Create = true)
        {
            Func<string, EFile> A = (x) => new EFile(g_pGamepath + "data\\" + x, new StdFileReader());
            Func<EFile> B = () =>
            {
                switch (a_Type)
                {
                    case NewFileType.Lrent:
                    case NewFileType.Sec:
                    case NewFileType.Wrl:
                        return A("common\\projects\\" + a_Name);
                    case NewFileType.Texture:
                        return A("compiled\\images\\" + a_Name);
                    case NewFileType.StaticModel:
                        return A("common\\meshes\\" + a_Name);
                    case NewFileType.AnimModel:
                        return A("compiled\\animations\\" + a_Name);
                    case NewFileType.Material:
                        return A("common\\materials\\" + a_Name);
                    case NewFileType.Info:
                        return A("raw\\infos\\" + a_Name);
                    case NewFileType.Tab:
                        return A("compiled\\strings\\" + a_Name);
                    default:
                        throw new Exception();
                }
            };
            EFile a = B();
            if (a_Create)
                a.Create(FileMode.Create);
            if (!_PhyFiles.ContainsKey(a.Name.ToUpper()))
                //lock (m_PhyBlock)
                {
                    _PhyFiles.Add(a.Name.ToUpper(), a);
                }
            return a;
        }

        public static string mapFilename(string file)
        {
            string b = "#G3:/";
            EFile e = GetFile(file);
            if (!e.IsOpenable)
                return file;
            Uri uri1 = new Uri(e.Path);
            Uri uri2 = new Uri(g_pGamepath);
            string relativePath = uri2.MakeRelativeUri(uri1).ToString();
            return b + relativePath;
        }

        public static event Action<List<EFile>> NewFilesFound;

        #region TabFiles
        public static void SaveTabFiles()
        {
            foreach (KeyValuePair<string, tabFile> k in tab_files)
                k.Value.serialize();
        }

        public static tabFile GetTabFile(string a_Name)
        {
            return tab_files[a_Name];
        }

        public static List<tabFile> GetTabFiles()
        {
            return tab_files.Values.ToList();
        }

        public static tabFile mapTab(string prefix)
        {
            prefix = prefix.ToLower();
            if (tab_FilesPrefix.ContainsKey(prefix))
                return tab_FilesPrefix[prefix];
            return null;
        }

        public static string GetString(string prefix, string key, int index)
        {
            tabFile T = mapTab(prefix);
            if (T == null)
                return "NOT_FOUND";
            string[] S = T.getString(key);
            return S.Length > index ? S[index] : "";
        }

        public static void SetString(string prefix, string key, int index, string value)
        {
            tabFile T = mapTab(prefix);
            if (T != null)
                T.setString(key, value, index);
        }

        public static tabFile CreateNewTabFile(string name, string prefix, params string[] columns)
        {
            EFile a = CreateNewPhysicalFile(NewFileType.Tab, name + ".tab");
            tabFile t = new tabFile(a, columns);
            t.serialize();

            tab_files.Add(a.Name, t);
            tab_FilesPrefix.Add(prefix.ToLower(), t);

            string n = g_pGamepath + "data\\ini\\loc.ini";
            List<string> A = new List<string>(File.ReadAllLines(n));
            A.Add(Environment.NewLine + "[" + prefix + "]");
            A.Add("prefix=" + prefix);
            A.Add("csv=#G3:/Data/Raw/Strings/" + name + ".csv");
            A.Add("bin=#G3:/Data/Compiled/Strings/" + name + ".tab");
            File.WriteAllLines(n, A.ToArray());

            return t;
        }
        #endregion
    }

    static class ResourceManager
    {
        static Dictionary<string, string> Guids = new Dictionary<string, string>();
        static Dictionary<string, string> IGUids = new Dictionary<string, string>();
        public static string[] AIFunctions;
        public static string[] AIStates;
        public static string[] Scripts;
        public static string[] AICallbacks;
        public static Dictionary<string, string> HeadGuids = new Dictionary<string, string>();
        public static Dictionary<string, string> BodyGuids = new Dictionary<string, string>();
        public static Dictionary<Guid, Rectangle> Rects = new Dictionary<Guid, Rectangle>();

        public static string GetlName(Guid G)
        {
            int i = 0;
            string s2 = BitConverter.ToString(G.ToByteArray());
            s2 = s2.Replace("-", "").ToLower();
        LABEL001:
            foreach (KeyValuePair<string, string> kvp in ResourceManager.Guids)
            {
                if (kvp.Key.Contains(s2, StringComparison.CurrentCultureIgnoreCase) && !kvp.Value.Contains("no"))
                {
                    return kvp.Value;
                }
            }
            foreach (KeyValuePair<string, string> kvp in ResourceManager.IGUids)
            {
                if (kvp.Key.Contains(s2, StringComparison.CurrentCultureIgnoreCase) && !kvp.Value.Contains("no"))
                {
                    return kvp.Value;
                }
            }
            if (i == 0)
            {
                i++;
                s2 = G.ToString("N");
                goto LABEL001;
            }

            string s = G.ToString("N");
            if (Guids.ContainsKey(s2))
                return Guids[s2];
            if (IGUids.ContainsKey(s2))
                return IGUids[s2];
            return string.Empty;
        }

        public static Guid getGuidFromName(string Name)
        {
            foreach (KeyValuePair<string, string> kvp in Guids)
                if (kvp.Value == Name)
                    return new Guid(kvp.Key);
            foreach (KeyValuePair<string, string> kvp in IGUids)
                if (kvp.Value == Name)
                    return new Guid(kvp.Key);
            return Guid.Empty;
        }

        public static void InitResourceManager()
        {
            AIFunctions = File.ReadAllLines("Resources\\AIFunctions.txt");
            AIStates = File.ReadAllLines("Resources\\AIStates.txt");
            Scripts = File.ReadAllLines("Resources\\Scripts.txt");
            AICallbacks = File.ReadAllLines("Resources\\AICallbacks.txt");

            StreamReader stream = new StreamReader("resources/TemplateGuIDs.txt");
            while (!stream.EndOfStream)
            {
                string n = stream.ReadLine();
                string id = stream.ReadLine();
                string q = stream.ReadLine();
                Guids.Add(id, q);
                if (q.Contains("Ani_Hero_Head_"))
                    HeadGuids.Add(id, q);
                if (q.Contains("Ani_Hero_Armor"))
                    BodyGuids.Add(id, q);
            }
            stream.Close();

            stream = new StreamReader("resources/ItemGuids.txt");
            while (!stream.EndOfStream)
            {
                string q = stream.ReadLine();
                string id = stream.ReadLine();
                IGUids.Add(id, q);
            }
            stream.Close();

            stream = new StreamReader("resources/GuidToRect.txt");
            while (!stream.EndOfStream)
            {
                string l = stream.ReadLine();
                string[] p = l.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                Guid g = new Guid(p[0]);
                string[] p2 = p[1].Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                int x = int.Parse(p2[1].Replace(",Y", ""));
                int y = int.Parse(p2[2].Replace(",Width", ""));
                int w = int.Parse(p2[3].Replace(",Height", ""));
                int h = int.Parse(p2[4].Replace("}", ""));
                Rects.Add(g, new Rectangle(x, y, w, h));
            }
            stream.Close();
        }
    }

    static class RegistryManager
    {
        /// <summary>
        /// An useful class to read/write/delete/count registry keys
        /// </summary>
        public class ModifyRegistry
        {
            private bool showError = false;
            /// <summary>
            /// A property to show or hide error messages 
            /// (default = false)
            /// </summary>
            public bool ShowError
            {
                get { return showError; }
                set { showError = value; }
            }

            private string subKey = "SOFTWARE\\" + Application.ProductName.ToUpper();
            /// <summary>
            /// A property to set the SubKey value
            /// (default = "SOFTWARE\\" + Application.ProductName.ToUpper())
            /// </summary>
            public string SubKey
            {
                get { return subKey; }
                set { subKey = value; }
            }

            private RegistryKey baseRegistryKey = Registry.CurrentUser;
            /// <summary>
            /// A property to set the BaseRegistryKey value.
            /// (default = Registry.LocalMachine)
            /// </summary>
            public RegistryKey BaseRegistryKey
            {
                get { return baseRegistryKey; }
                set { baseRegistryKey = value; }
            }

            /* **************************************************************************
             * **************************************************************************/

            /// <summary>
            /// To read a registry key.
            /// input: KeyName (string)
            /// output: value (string) 
            /// </summary>
            public string Read(string KeyName)
            {
                // Opening the registry key
                RegistryKey rk = baseRegistryKey;
                // Open a subKey as read-only
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                // If the RegistrySubKey doesn't exist -> (null)
                if (sk1 == null)
                {
                    return null;
                }
                else
                {
                    try
                    {
                        // If the RegistryKey exists I get its value
                        // or null is returned.
                        return (string)sk1.GetValue(KeyName.ToUpper());
                    }
                    catch (Exception)
                    {
                        //throw new Exception("Reading registry " + KeyName.ToUpper());
                        return null;
                    }
                }
            }

            /* **************************************************************************
             * **************************************************************************/

            /// <summary>
            /// To write into a registry key.
            /// input: KeyName (string) , Value (object)
            /// output: true or false 
            /// </summary>
            public bool Write(string KeyName, object Value)
            {
                try
                {
                    // Setting
                    RegistryKey rk = baseRegistryKey;
                    // I have to use CreateSubKey 
                    // (create or open it if already exits), 
                    // 'cause OpenSubKey open a subKey as read-only
                    RegistryKey sk1 = rk.CreateSubKey(subKey);
                    // Save the value
                    sk1.SetValue(KeyName.ToUpper(), Value);

                    return true;
                }
                catch (Exception)
                {
                    //new EngineError("Writing registry " + KeyName.ToUpper(), EngineErrorType.InvalidOperation);
                    return false;
                }
            }

            /* **************************************************************************
             * **************************************************************************/

            /// <summary>
            /// To delete a registry key.
            /// input: KeyName (string)
            /// output: true or false 
            /// </summary>
            public bool DeleteKey(string KeyName)
            {
                try
                {
                    // Setting
                    RegistryKey rk = baseRegistryKey;
                    RegistryKey sk1 = rk.CreateSubKey(subKey);
                    // If the RegistrySubKey doesn't exists -> (true)
                    if (sk1 == null)
                        return true;
                    else
                        sk1.DeleteValue(KeyName);

                    return true;
                }
                catch (Exception)
                {
                    //new EngineError("Deleting SubKey " + subKey, EngineErrorType.InvalidOperation);
                    return false;
                }
            }

            /* **************************************************************************
             * **************************************************************************/

            /// <summary>
            /// To delete a sub key and any child.
            /// input: void
            /// output: true or false 
            /// </summary>
            public bool DeleteSubKeyTree()
            {
                try
                {
                    // Setting
                    RegistryKey rk = baseRegistryKey;
                    RegistryKey sk1 = rk.OpenSubKey(subKey);
                    // If the RegistryKey exists, I delete it
                    if (sk1 != null)
                        rk.DeleteSubKeyTree(subKey);

                    return true;
                }
                catch (Exception)
                {
                    //new EngineError("Deleting SubKey " + subKey, EngineErrorType.InvalidOperation);
                    return false;
                }
            }

            /* **************************************************************************
             * **************************************************************************/

            /// <summary>
            /// Retrive the count of subkeys at the current key.
            /// input: void
            /// output: number of subkeys
            /// </summary>
            public int SubKeyCount()
            {
                try
                {
                    // Setting
                    RegistryKey rk = baseRegistryKey;
                    RegistryKey sk1 = rk.OpenSubKey(subKey);
                    // If the RegistryKey exists...
                    if (sk1 != null)
                        return sk1.SubKeyCount;
                    else
                        return 0;
                }
                catch (Exception)
                {
                    //new EngineError("Retriving subkeys of " + subKey, EngineErrorType.InvalidOperation);
                    return 0;
                }
            }

            /* **************************************************************************
             * **************************************************************************/

            /// <summary>
            /// Retrive the count of values in the key.
            /// input: void
            /// output: number of keys
            /// </summary>
            public int ValueCount()
            {
                try
                {
                    // Setting
                    RegistryKey rk = baseRegistryKey;
                    RegistryKey sk1 = rk.OpenSubKey(subKey);
                    // If the RegistryKey exists...
                    if (sk1 != null)
                        return sk1.ValueCount;
                    else
                        return 0;
                }
                catch (Exception)
                {
                    //new EngineError("Retriving subkeys of " + subKey, EngineErrorType.InvalidOperation);
                    return 0;
                }
            }
        }

        static ModifyRegistry mReg = new ModifyRegistry();
        const string lFolder = "Lrent_Directory";
        const string pFolder = "Pak_Directory";
        const string xFolder = "Xinf_Directory";
        const string xBaseFolder = "Xinf_BaseDirectory";
        const string sFolder= "SelFolder_Directory";
        const string tFile = "Tab_File";

        public static string getLrentFolder()
        {
            string iP = mReg.Read(lFolder);
            if (iP != string.Empty && iP != null)
                return iP;
            return string.Empty;
        }

        public static void setLrentFolder(string f)
        {
            mReg.Write(lFolder, f);
        }

        public static void setPakFolder(string f)
        {
            mReg.Write(pFolder, f);
        }

        public static string getPakFolder()
        {
            string iP = mReg.Read(pFolder);
            if (iP != string.Empty && iP != null)
                return iP;
            return string.Empty;
        }

        public static string getXinfFolder()
        {
            string iP = mReg.Read(xFolder);
            if (iP != string.Empty && iP != null)
                return iP;
            return string.Empty;
        }

        public static void setXinfFolder(string f)
        {
            mReg.Write(xFolder, f);
        }

        public static string getXinfBaseFolder()
        {
            string iP = mReg.Read(xBaseFolder);
            if (iP != string.Empty && iP != null)
                return iP;
            return string.Empty;
        }

        public static void setXinfBaseFolder(string f)
        {
            mReg.Write(xBaseFolder, f);
        }

        public static string getFolderSelectorDirectory()
        {
            string iP = mReg.Read(sFolder);
            if (iP != string.Empty && iP != null)
                return iP;
            return string.Empty;
        }

        public static void setFolderSelectorDirectory(string f)
        {
            mReg.Write(sFolder, f);
        }

        public static string getTabFilePath()
        {
            string iP = mReg.Read(tFile);
            if (iP != string.Empty && iP != null)
                return iP;
            return string.Empty; 
        }

        public static void setTabFilePath(string f)
        {
            mReg.Write(tFile, f);
        }

        public class Indexer
        {
            public string this[string key]
            {
                get
                {
                    string iP = mReg.Read(key);
                    if (iP != string.Empty && iP != null)
                        return iP;
                    return string.Empty;
                }
                set
                {
                    mReg.Write(key, value);
                }
            }
        }

        public static Indexer Index
        {
            get
            {
                return new Indexer();
            }
        }
    }
}
