using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameLibrary;
using GameLibrary.IO;
using System.IO;

namespace RisenEditor.Code.Loader
{
    public enum RisenPakAttributes : uint
    {
        RisenPakAttribute_ReadOnly = 0x00000001,  // FILE_ATTRIBUTE_READONLY
        RisenPakAttribute_Hidden = 0x00000002,  // FILE_ATTRIBUTE_HIDDEN
        RisenPakAttribute_System = 0x00000004,  // FILE_ATTRIBUTE_SYSTEM
        RisenPakAttribute_Directory = 0x00000010,  // FILE_ATTRIBUTE_DIRECTORY
        RisenPakAttribute_Archive = 0x00000020,  // FILE_ATTRIBUTE_ARCHIVE
        RisenPakAttribute_Normal = 0x00000080,  // FILE_ATTRIBUTE_NORMAL
        RisenPakAttribute_Temporary = 0x00000100,  // FILE_ATTRIBUTE_TEMPORARY
        RisenPakAttribute_Compressed = 0x00000800,  // FILE_ATTRIBUTE_COMPRESSED
        RisenPakAttribute_Encrypted = 0x00004000,  // FILE_ATTRIBUTE_ENCRYPTED
        RisenPakAttribute_Deleted = 0x00008000,  // Not used in Risen PAKs
        RisenPakAttribute_Virtual = 0x00010000,  // FILE_ATTRIBUTE_VIRTUAL
        RisenPakAttribute_Packed = 0x00020000,  // Stored in a PAK volume
        RisenPakAttribute_Cached = 0x00040000   // Not used in Risen PAKs
    };

    public enum RisenPakCompression : uint
    {
        RisenPakCompression_None = 0x00000000,
        RisenPakCompression_Auto = 0x00000001,
        RisenPakCompression_ZLib = 0x00000002
    };

    public enum RisenPakEncryption : uint
    {
        RisenPakEncryption_None = 0x00000000
    };

    public abstract class RisenPakFilePart
    {
        protected RisenPakFilePart parent;
        protected List<RisenPakFilePart> children;
        protected string name;
        public string nameCAP;
        public RisenPak _Container;

        protected void _init(RisenPak _Cont)
        {
            _Container = _Cont;
            if (name.Length != 0)
                name = name.Remove(name.Length - 1, 1);
            nameCAP = name.ToUpper();
            if (this is RisenPakDirectory && children != null)
            {
                children.Reverse();
            }
        }

        public abstract int CompressedSize
        {
            get;
        }

        public abstract int UnCompressedSize
        {
            get;
        }

        public RisenPakAttributes Attributes { get; protected set; }

        public long TimeCreated { get; protected set; }

        public long TimeLastAccessed { get; protected set; }

        public long TimeLastModified { get; protected set; }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public string FullName
        {
            get
            {
                if (parent != null)
                {
                    string a = parent.FullName.Length > 0 ? parent.FullName + "\\" : "";
                    return (a + Name).Replace("\\\\", "\\");
                }
                else return Container.rootFolderLoc + Name;
            }
        }

        public List<RisenPakFilePart> Children
        {
            get
            {
                return children;
            }
        }

        public RisenPakFilePart Parent
        {
            get
            {
                return parent;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public RisenPakFilePart getChild(string name)
        {
            if(children == null)return null;
            foreach (RisenPakFilePart f in children)
                if (f.nameCAP == name)
                    return f;
            return null;
        }

        public RisenPak Container
        {
            get
            {
                return _Container;
            }
        }
    }

    public class RisenPakDirectory : RisenPakFilePart
    {
        internal RisenPakDirectory(System.IO.BinaryReader bR, RisenPakFilePart P2, RisenPak Container)
        {
            base.name = string.Empty;
            base.parent = P2;
            int l = bR.ReadInt32();
            if (l != 0)
                l++;
            base.name = new string(bR.ReadChars(l));
            base.TimeCreated = bR.ReadInt64();
            base.TimeLastAccessed = bR.ReadInt64();
            base.TimeLastModified = bR.ReadInt64();
            base.Attributes = (RisenPakAttributes)bR.ReadUInt32() | RisenPakAttributes.RisenPakAttribute_Packed;
            int count = bR.ReadInt32();
            children = new List<RisenPakFilePart>();
            for (int i = 0; i < count; i++)
            {
                RisenPakAttributes attrib = (RisenPakAttributes)bR.ReadUInt32();
                if ((attrib & RisenPakAttributes.RisenPakAttribute_Directory) == RisenPakAttributes.RisenPakAttribute_Directory)
                    children.Add(new RisenPakDirectory(bR, this, Container));
                else children.Add(new RisenPakFile(bR, this, Container));
            }
            _init(Container);
        }

        public override int CompressedSize
        {
            get { int r = 0; foreach (RisenPakFilePart q in children) r += q.CompressedSize; return r; }
        }

        public override int UnCompressedSize
        {
            get { int r = 0; foreach (RisenPakFilePart q in children) r += q.UnCompressedSize; return r; }
        }
    }

    public class RisenPakFile : RisenPakFilePart
    {
        protected int compSize, uncompSize;
        protected long offset;
        RisenPakEncryption encrypt;
        Stream m_Stream;

        internal RisenPakFile(System.IO.BinaryReader bR, RisenPakFilePart P, RisenPak Container)
        {
            base.name = string.Empty;
            base.parent = P;
            int l = bR.ReadInt32();
            if (l != 0)
                l++;
            base.name = new string(bR.ReadChars(l));
            offset = bR.ReadInt64();
            base.TimeCreated = bR.ReadInt64();
            base.TimeLastAccessed = bR.ReadInt64();
            base.TimeLastModified = bR.ReadInt64();
            base.Attributes = (RisenPakAttributes)bR.ReadUInt32() | RisenPakAttributes.RisenPakAttribute_Packed;
            encrypt = (RisenPakEncryption)bR.ReadUInt32();
            Compression = (RisenPakCompression)bR.ReadUInt32();
            compSize = bR.ReadInt32();
            uncompSize = bR.ReadInt32();
            m_Stream = null;
            _init(Container);
        }

        protected RisenPakFile() { }

        public RisenPakCompression Compression{ get; private set; }

        public override int CompressedSize
        {
            get { return compSize; }
        }

        public override int UnCompressedSize
        {
            get { return uncompSize; }
        }

        public long Offset
        {
            get
            {
                return offset;
            }
        }

        virtual public Stream OpenInNewStream(Stream a_Stream, FileAccess fA)
        {
            bool b0 = true;
            if (m_Stream != null && (fA & FileAccess.Write) == FileAccess.Write)
                if (!m_Stream.CanWrite)
                    b0 = false;
            if (m_Stream != null && b0)
                return m_Stream;
            else
            {
                a_Stream.Position = Offset;
                byte[] data = a_Stream.ReadBytes(CompressedSize);
                if ((this.Compression & RisenPakCompression.RisenPakCompression_ZLib) == RisenPakCompression.RisenPakCompression_ZLib)
                {
                    b0 = (fA & FileAccess.Write) == FileAccess.Write;
                    MemoryStream mStream = new MemoryStream(UnCompressedSize);
                    zlib.ZOutputStream outp = new zlib.ZOutputStream(mStream);
                    outp.Write(data, 0, data.Length);
                    outp.Position = 0;
                    outp.Flush();
                    outp.finish();
                    mStream.Position = 0;
                    return mStream;
                }
                else if (((this.Compression & RisenPakCompression.RisenPakCompression_None) == RisenPakCompression.RisenPakCompression_None) ||
                        ((this.Compression & RisenPakCompression.RisenPakCompression_Auto) == RisenPakCompression.RisenPakCompression_Auto)    )
                {
                    MemoryStream mS = new MemoryStream(data.Length);
                    mS.Write(data, 0, data.Length);
                    mS.Position = 0;
                    return mS;
                }
                return null;
            }
        }

        virtual public void Invalidate()
        {
            m_Stream = null;
        }
    }

    public class RPakFile : EFile
    {
        public RisenPakFilePart _Native;
        public RisenPak _Container;
        private Stream lastStream;
        private FileAccess lastAccess;

        public RPakFile(RisenPakFilePart p)
        {
            _Native = p;
            _Container = p._Container;
        }

        public override EFile[] Children
        {
            get
            {
                if (_Native is RisenPakDirectory)
                {
                    EFile[] f = new EFile[_Native.Children.Count];
                    for (int i = 0; i < f.Length; i++)
                        f[i] = new RPakFile(_Native.Children[i]);
                    return f;
                }
                else return null;
            }
        }

        public override string Extension
        {
            get
            {
                if (IsDirectory) return string.Empty;
                int i = _Native.Name.Length - 1;
                while(_Native.Name[i] != '.') i--;
                return _Native.Name.Substring(i, _Native.Name.Length - i);
            }
        }

        public override bool IsDirectory
        {
            get
            {
                return _Native is RisenPakDirectory;
            }
        }

        public override bool IsOpenable
        {
            get
            {
                return _Native is RisenPakFile;
            }
        }

        public override string Name
        {
            get
            {
                return _Native.Name;
            }
        }

        public override EFile Parent
        {
            get
            {
                return new RPakFile(_Native.Parent);
            }
        }

        public override string Path
        {
            get
            {
                return _Native.FullName;
            }
        }

        public override EFileReader Reader
        {
            get
            {
                throw new Exception("");
            }
        }

        public override Stream Open(FileAccess fA)
        {
            if (lastStream != null)
                throw new Exception("File was not closed!");
            lastStream =  _Container.OpenFile(this, fA);
            lastAccess = fA;
            return lastStream;
        }

        public override void Close()
        {
            if((lastAccess & FileAccess.Write) == FileAccess.Write)
                _Container.CloseFile(this, lastStream);
            lastStream = null;
        }

        public override EFile GetChild(string name)
        {
            foreach (RisenPakFilePart f in _Native.Children)
                if (f.Name == name)
                    return new RPakFile(f);
            return null;
        }

        public override void Create(FileMode fM)
        {
            throw new Exception();
        }
    }

    public sealed class RisenPak
    {
        Stream m_Stream;
        public RisenPakDirectory root;
        public string rootFolderLoc;

        public RisenPak(EFile F, Dictionary<string, RisenPakFile> files, bool DoOverwrite = false)
        {
            rootFolderLoc = F.Path.Replace(FileManager.g_pGamepath, "").ToLower().Replace(".pak", "") + "\\";
            m_Stream = F.Open(FileAccess.Read);
            BinaryReader bReader = new BinaryReader(m_Stream);
            m_Stream.Position = 24L;
            long DataOffset = m_Stream.ReadLong();
            long RootOffset = m_Stream.ReadLong();
            long VolumeSize = m_Stream.ReadLong();
            bReader.BaseStream.Position = (long)RootOffset;
            root = new RisenPakDirectory(bReader, null, this);
            if (files == null)
                return;

            Stack<RisenPakDirectory> dirs = new Stack<RisenPakDirectory>();
            dirs.Push(root);
            while (dirs.Count != 0)
            {
                RisenPakDirectory cd = dirs.Pop();
                foreach (RisenPakFilePart q in cd.Children)
                {
                    if (q is RisenPakDirectory)
                    {
                        dirs.Push(q as RisenPakDirectory);
                    }
                    else if(q is RisenPakFile)
                    {
                        if (!files.ContainsKey(q.nameCAP))
                            files.Add(q.nameCAP, (RisenPakFile)q);
                        else if (DoOverwrite)
                            files[q.nameCAP] = (RisenPakFile)q;
                        else throw new Exception();
                    }
                }
            }
        }

        public Stream OpenFile(RPakFile f, FileAccess fA)
        {
            if (f._Native is RisenPakDirectory) return null;
            return (f._Native as RisenPakFile).OpenInNewStream(m_Stream, fA);
        }

        public void CloseFile(RPakFile f, Stream newStream)
        {
            newStream.Position = 0L;
            (f._Native as RisenPakFile).Invalidate();
            Stream oldStream = (f._Native as RisenPakFile).OpenInNewStream(m_Stream, FileAccess.Read);
            bool same = oldStream.StreamsContentsAreEqual(newStream);
            oldStream.Close();
            if (!same)
            {
                System.IO.Stream FS = FileManager.GetPhysicalFileLocation(f.Path).Open(FileAccess.Write);
                FS.Write(newStream.GetAllBytes(), 0, (int)newStream.Length);
                FS.Flush();
                FS.Close();
            }
        }
    }
}
