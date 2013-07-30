using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using GameLibrary.Objekte;
using SlimDX;
using GameLibrary.IO;
using GameLibrary;
using System.Threading;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.Code
{
    public class LrentFile : IEnumerable<ILrentObject>
    {
        List<ILrentObject> rObjects;
        API_Device D;
        eCArchiveFile _Handle;
        eCProcessibleElement _Element;

        public LrentFile(EFile a_File, API_Device D)
        {
            _Handle = new eCArchiveFile(a_File);
            _Handle.Position = 14L;
            _Element = new eCProcessibleElement(_Handle);
            _Handle.Close();

            rObjects = new List<ILrentObject>();
            this.D = D;
            IsLevelLrent = _Handle.Handle.Name.Contains("Level");
            eCEntityDynamicContext c = _context();
            foreach (eCDynamicEntity e in c.entitys)
                rObjects.Add(new ILrentObject(this, e));
        }

        private LrentFile()
        {

        }

        public static LrentFile CreateNew(string a_Name, API_Device D)
        {
            LrentFile LF = new LrentFile();
            eCFile _F2 = new eCFile(FileManager.GetFile("Resources/emptyLrent.bin"));
            LF._Element = new eCProcessibleElement(-790693154, _F2);
            LF._Handle = eCArchiveFile.CreateNew(FileManager.CreateNewPhysicalFile(FileManager.NewFileType.Lrent, a_Name));
            LF._Handle.Close();
            LF._Handle.Save(LF._Element);

            LF._context().entitys[0].GUID = bCPropertyID.NewRandomID();

            LF.rObjects = new List<ILrentObject>();
            LF.D = D;
            LF.IsLevelLrent = LF._Handle.Handle.Name.Contains("Level");
            eCEntityDynamicContext c = LF._context();
            foreach (eCDynamicEntity e in c.entitys)
                LF.rObjects.Add(new ILrentObject(LF, e));
            return LF;
        }

        public eCArchiveFile File
        {
            get
            {
                return _Handle;
            }
        }

        public eCEntityDynamicContext Context
        {
            get
            {
                return _context();
            }
        }

        public List<ILrentObject> Objects
        {
            get
            {
                return rObjects;
            }
            set
            {
                rObjects = value;
            }
        }

        public ILrentObject this[string name]
        {
            get
            {
                foreach (ILrentObject o in rObjects)
                    if (o.Name == name)
                        return o;
                return null;
            }
        }

        public ILrentObject this[int i]
        {
            get
            {
                return rObjects[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<ILrentObject> GetEnumerator()
        {
            return (IEnumerator<ILrentObject>)this.rObjects.GetEnumerator();
        }

        public string Path
        {
            get
            {
                return _Handle.Handle.Path;
            }
        }

        public string Name
        {
            get
            {
                return _Handle.Handle.Name;
            }
        }

        public void SaveFile()
        {
            //MemoryStream S0 = new MemoryStream(PhysicalFile.Open(FileAccess.Read).GetAllBytes());
            //PhysicalFile.Close();
            _Handle.Save(_Element);
            /*
            if (!eFile.IsVirtual && !eFile.Path.Contains(FileManager.APP_PATH))
            {
                Stream S1 = PhysicalFile.Open(FileAccess.Read);
                bool same = MyExtensions.StreamsContentsAreEqual(S0, S1);
                if (!same)//create copy
                {
                    string path = Environment.GetEnvironmentVariable("appdata").Replace("Roaming", "") + @"\Local\Risen";
                    if (eFile.Path.Contains(path, StringComparison.CurrentCultureIgnoreCase)) return;
                    string s2 = path + @"\" + eFile.Name;
                    if (System.IO.File.Exists(s2))
                        System.IO.File.Delete(s2);
                    System.IO.File.WriteAllBytes(s2, S1.GetAllBytes());
                }
                PhysicalFile.Close();
            }
            S0.Dispose();*/
        }

        public void addObject(ILrentObject RO)
        {
            _context().addEntitys(new List<eCDynamicEntity>() { RO.Entity });
            rObjects.Add(RO);
            UpdateContextBox();
        }

        public void UpdateContextBox()
        {
            BoundingBox bb = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
            foreach (ILrentObject o in Objects)
                bb = bb.Extend(o.BoundingBox);
            bb = GENOMEMath.toGENOME(bb);
            (_Element.acc.Class as gCDynamicLayer).acc.Properties[1].Object = new bCBox(bb);
            _context().ContextBox = bb;
        }

        public void removeObject(ILrentObject RO)
        {
            _context().removeEntitys(new List<eCDynamicEntity>() { RO.Entity });
            rObjects.Remove(RO);
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsLevelLrent { get; private set; }

        eCEntityDynamicContext _context()
        {
            return (_Element.acc.Class as gCDynamicLayer).acc.Class as eCEntityDynamicContext;
        }
    }

    public class SecFile
    {
        eCArchiveFile _Handle;
        eCProcessibleElement _Element;

        public SecFile(EFile a_File)
        {
            _Handle = new eCArchiveFile(a_File);
            _Handle.Position = 14L;
            _Element = new eCProcessibleElement(_Handle);
            _Handle.Close();
        }

        private SecFile() { }

        public static SecFile CreateNew(string a_Name)
        {
            SecFile S = new SecFile();
            eCFile F = new eCFile(FileManager.GetFile("Resources/emptySec.bin"));
            S._Element = new eCProcessibleElement(F);
            S._Handle = eCArchiveFile.CreateNew(FileManager.CreateNewPhysicalFile(FileManager.NewFileType.Sec, a_Name));
            S._Handle.Close();
            S._Handle.Save(S._Element);
            return S;
        }

        public string Name
        {
            get
            {
                return _Handle.Handle.Name;
            }
        }

        public List<string> LrentFiles
        {
            get
            {
                gCSector sec = _Element.acc.Class as gCSector;
                List<string> files = new List<string>();
                if(sec.DynamicLayers != null)
                    foreach (bCString s in sec.DynamicLayers)
                        files.Add(s.pString + ".lrent");
                return files;
            }
        }

        public void AddLrent(string a_FileName)
        {
            string s = a_FileName.Replace(".lrent", "");
            bCString s2 = new bCString(s);
            if ((_Element.acc.Class as gCSector).DynamicLayers.Contains(s2))
                return;
            (_Element.acc.Class as gCSector).DynamicLayers.Add(s2);
        }

        public void AddLrent(LrentFile a_File)
        {
            AddLrent(a_File.Name);
        }

        public void RemoveLrent(string a_FileName)
        {
            string s2 = a_FileName.Replace(".lrent", "");
            for (int i = 0; i < (_Element.acc.Class as gCSector).DynamicLayers.Count; i++)
            {
                bCString s = (_Element.acc.Class as gCSector).DynamicLayers[i];
                if (s.pString.Contains(s2))
                {
                    (_Element.acc.Class as gCSector).DynamicLayers.RemoveAt(i);
                    return;
                }
            }
        }

        public void RemoveLrent(LrentFile a_File)
        {
            RemoveLrent(a_File.Name);
        }

        public void SaveFile()
        {
            _Handle.Save(_Element);
        }
    }

    public class WrlFile
    {
        eCArchiveFile _Handle;
        eCProcessibleElement _Element;

        public WrlFile(EFile a_File)
        {
            _Handle = new eCArchiveFile(a_File);
            _Handle.Position = 14L;
            _Element = new eCProcessibleElement(_Handle);
            _Handle.Close();
        }

        private WrlFile() { }

        public static WrlFile CreateNew(string a_Name)
        {
            WrlFile W = new WrlFile();
            eCFile F = new eCFile(FileManager.GetFile("Resources/emptyWrl.bin"));
            W._Element = new eCProcessibleElement(F);
            W._Handle = eCArchiveFile.CreateNew(FileManager.CreateNewPhysicalFile(FileManager.NewFileType.Wrl, a_Name));
            W._Handle.Close();

            string wrldatasc = a_Name.Replace(".wrl", "") + "_data.wrldatasc";
            (W._Element.acc.Class as gCWorld).WorldDataFile.pString = wrldatasc;
            string p = FileManager.CreateNewPhysicalFile(FileManager.NewFileType.Wrl, wrldatasc).Path;
            if (File.Exists(p))
                File.Delete(p);
            StreamWriter SW = File.CreateText(p);
            SW.Write(@"[Common]
;Insert here all available sectors
[Sector.List]");

            SW.Flush();
            SW.Close();

            W._Handle.Save(W._Element);

            return W;
        }

        public string WorldFile
        {
            get
            {
                return (_Element.acc.Class as gCWorld).WorldDataFile.pString;
            }
        }

        public List<string> SecFiles
        {
            get
            {
                EFile _H = FileManager.GetFile(WorldFile);
                StreamReader sr = new StreamReader(_H.Open(FileAccess.Read));
                while (sr.ReadLine() != "[Sector.List]") ;
                List<string> _F = new List<string>();
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    string[] s2 = s.Split('=');
                    if (bool.Parse(s2[1]))
                        _F.Add(s2[0] + ".sec");
                }
                _H.Close();
                return _F;
            }
        }

        public void AddSec(string a_FileName)
        {
            List<string> s_F = SecFiles;
            if (s_F.Contains(a_FileName))
                return;

            string s = a_FileName.Replace(".sec", "") + "=true";
            byte[] b = new byte[s.Length + 2];
            b[0] = (byte)Environment.NewLine[0];
            b[1] = (byte)Environment.NewLine[1];//13 10
            for (int i = 0; i < s.Length; i++)
                b[i + 2] = (byte)s[i];
            EFile _H = FileManager.GetFile(WorldFile);
            Stream _s = _H.Open(FileAccess.ReadWrite);
            byte[] bc = _s.ReadBytes((int)_s.Length);
            _s.SetLength(_s.Length + b.Length);
            _s.Position = 0L;
            _s.Write(bc, 0, bc.Length);
            _s.Write(b, 0, b.Length);
            _H.Close();
        }

        public void AddSec(SecFile a_File)
        {
            AddSec(a_File.Name);
        }

        public void RemoveSec(string a_FileName)
        {
            throw new NotImplementedException("Sorry no time yet.");
        }

        public void RemoveSec(LrentFile a_File)
        {
            RemoveSec(a_File.Name);
        }

        public void SaveFile()
        {

        }
    }
}
