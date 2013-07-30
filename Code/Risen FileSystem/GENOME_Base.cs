using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GameLibrary.IO;
using System.ComponentModel;
using SlimDX;
using GameLibrary;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Collections;

namespace RisenEditor.Code.RisenTypes
{
    #region MainTypes
    public abstract class BinaryFileBlock
    {
        public abstract void Serialize(IFile a_File);

        [Browsable(false)]
        public abstract int Size
        {
            get;
        }
        public abstract void deSerialize(IFile a_File);

        public virtual BinaryFileBlock Clone()
        {
            classData d = (classData)Activator.CreateInstance(this.GetType(), null);
            TempStream S = new TempStream(Size, eCArchiveFile.CreateBuffer());
            Serialize(S);
            S.Position = 0L;
            d.deSerialize(S);
            return d;
        }
    }

    public abstract class classData : BinaryFileBlock
    {
        private bCObjectBase container;
        static Dictionary<string, string> WrappedNames = new Dictionary<string, string>()
        {
            {"gCStateGraphActionCallScript", "gCStateGraphAction"},
            {"gCStateGraphActionEventDispatcher", "gCStateGraphAction"},
            {"gCStateGraphActionFollowState", "gCStateGraphAction"},
            {"gCStateGraphActionPauseAni", "gCStateGraphAction"},
            {"gCStateGraphActionPlayAni", "gCStateGraphAction"},
            {"gCStateGraphActionStartEffect", "gCStateGraphAction"},
            {"gCStateGraphActionStop", "gCStateGraphAction"},
            {"gCStateGraphActionTrigger", "gCStateGraphAction"},
            {"gCStateGraphActionTriggerState", "gCStateGraphAction"},
            {"gCStateGraphActionUnTrigger", "gCStateGraphAction"},
            {"gCStateGraphActionVisibility", "gCStateGraphAction"},
            {"gCStateGraphActionWait", "gCStateGraphAction"},
        };
        static List<string> g_NotFound = new List<string>();

        public string ClassName
        {
            get
            {
                return GetType().Name;
            }
        }

        public static classData CreateClassData(string a_Name, bCObjectBase a_Container, int si)
        {
            if (a_Name == "INVALID" || a_Name == string.Empty)
                return null;
            if (WrappedNames.ContainsKey(a_Name))
                a_Name = WrappedNames[a_Name];
            Type T2 = TypeCache.GetType(a_Name);
            if (T2 == null)
            {
                string n = a_Name;
                a_Name += "_" + si; 
                if (!g_NotFound.Contains(a_Name))
                {
                    string m = "PS " + n + "(Size : " + si.ToString() + ") not found.";
                    SystemLog.Append(LogImportance.Warning, m);
                    System.Diagnostics.Debug.WriteLine(m);
                    g_NotFound.Add(a_Name);
                }
                return null;
            }
            object o = (classData)Activator.CreateInstance(T2, true);
            (o as classData).container = a_Container;
            return o as classData;
        }

        public static classData CloneClassData(classData a_Object, bCObjectBase a_NewContainer)
        {
            classData v_N = a_Object.Clone() as classData;
            v_N.container = a_NewContainer;
            return v_N;
        }

        public bCObjectBase Container
        {
            get
            {
                return container;
            }
            internal set
            {
                container = value;
            }
        }
    }

    public class BUFFER : classData
    {
        byte[] B;

        public BUFFER(IFile S)
        {
            B = S.Read<byte>((int)S.Length);
        }

        public BUFFER(params byte[] b)
        {
            B = b;
        }

        public BUFFER(IFile S, int l)
        {
            B = S.Read<byte>(l);
        }

        public BUFFER() { }

        public override void Serialize(IFile s)
        {
            s.Write<byte>(B);
        }

        public override void deSerialize(IFile a_File)
        {
            throw new Exception();
        }

        public override int Size
        {
            get { return B.Length; }
        }
    }

    public class g_PropertyCollection : BinaryFileBlock, IEnumerable<bCProperty>
    {
        short Version;
        List<bCProperty> m_Properties;

        public g_PropertyCollection(IFile s)
        {
            deSerialize(s);
        }

        public g_PropertyCollection()
        {
            this.Version = 201;
            m_Properties = new List<bCProperty>();
        }

        public override void deSerialize(IFile s)
        {
            Version = s.Read<short>();
            m_Properties = new List<bCProperty>(s.Read<int>());
            for (int i = 0; i < m_Properties.Capacity; i++)
                m_Properties.Add(new bCProperty(s));
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            s.Write<int>(m_Properties.Count);
            for (int i = 0; i < m_Properties.Count; i++)
                m_Properties[i].Serialize(s);
        }

        public override BinaryFileBlock Clone()
        {
            g_PropertyCollection g = new g_PropertyCollection();
            g.Version = Version;
            g.m_Properties = new List<bCProperty>(m_Properties.Count);
            for (int i = 0; i < m_Properties.Count; i++)
                g.m_Properties.Add(m_Properties[i].Clone() as bCProperty);
            return g;
        }

        public bCProperty addProperty(string name, BinaryFileBlock o)
        {
            return addProperty(name, o.GetType().Name, o);
        }

        public bCProperty addProperty(string name, Enum e)
        {
            return addProperty(name, "bTPropertyContainer<enum " + e.GetType().Name + ">", e);
        }
        
        public bCProperty addProperty(string name, string type, object o)
        {
            if (TypeCache.GetType(type) != null && TypeCache.GetType(type).IsEnum)
                type = "bTPropertyContainer<enum " + type + ">";
            bCProperty q = new bCProperty( name, type, o);
            m_Properties.Add(q);
            return q;
        }
        
        public override int Size
        {
            get
            {
                return 2 + m_Properties.SizeOf();
            }
        }

        public bCProperty this[string Name]
        {
            get
            {
                foreach (bCProperty p in m_Properties)
                    if (p.PropertyName == Name)
                        return p;
                return null;
            }
        }

        public bCProperty this[int index]
        {
            get
            {
                return m_Properties[index];
            }
        }

        public IEnumerator<bCProperty> GetEnumerator()
        {
            return m_Properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Properties.GetEnumerator();
        }
    }

    public abstract class IFile
    {
        public abstract long Position
        {
            get;
            set;
        }

        public abstract long Length
        {
            get;
        }

        public abstract T Read<T>() where T : struct;

        public abstract T[] Read<T>(int l) where T : struct;

        public abstract void Write<T>(T t) where T : struct;

        public abstract void Write<T>(T[] t) where T : struct;

        public abstract string ReadString();

        public abstract void WriteString(string s);
    }

    public class eCFile : IFile
    {
        public EFile Handle { get; protected set; }
        protected Stream m_Native;

        protected eCFile() { }

        public eCFile(EFile f)
        {
            Handle = f;
            m_Native = new MemoryStream(f.Open(FileAccess.Read).GetAllBytes());
        }

        public static eCFile CreateNew(EFile a_File)
        {
            a_File.Create(FileMode.Create);
            eCFile a = new eCFile();
            a.Handle = a_File;
            return a;
        }

        public virtual void Close()
        {
            Handle.Close();
        }

        public virtual void Save(BinaryFileBlock a_DataBlock)
        {
            m_Native = Handle.Open(FileAccess.Write);
            bCString.IsArchiveFile = false;
            m_Native.SetLength(a_DataBlock.Size);
            a_DataBlock.Serialize(this);
            bCString.IsArchiveFile = true;
            Handle.Close();
        }

        public override string ReadString()
        {
            short l = Read<short>();
            return new String(Read<char>(l));
        }

        public override void WriteString(string s)
        {
            Write<short>((short)s.Length);
            Write<char>(s.ToCharArray());
        }

        public override long Length
        {
            get { return m_Native.Length; }
        }

        public override void Write<T>(T[] data)
        {
            long count = data.Length;
            if (!m_Native.CanWrite)
            {
                throw new NotSupportedException();
            }
            //Utilities.CheckArrayBounds(data, offset, ref count);
            long num2 = (long)((ulong)Marshal.SizeOf(new T()));
            long num = count * num2;
            if ((this.Position + num) > this.Length)
            {
                throw new EndOfStreamException();
            }
            for (int j = 0; j < count; j++)
            {
                byte[] bs = data[j].GetBytes();
                m_Native.Write(bs, 0, bs.Length);
            }
        }

        public override void Write<T>(T value)
        {
            if (!m_Native.CanWrite)
            {
                throw new NotSupportedException();
            }
            long num = (long)((ulong)Marshal.SizeOf(new T()));
            long position = this.Position;
            if ((position + num) > this.Length)
            {
                throw new EndOfStreamException();
            }
            byte[] bs = value.GetBytes();
            m_Native.Write(bs, 0, bs.Length);
        }

        public override long Position
        {
            get
            {
                return m_Native.Position;
            }
            set
            {
                m_Native.Position = value;
            }
        }

        public override T[] Read<T>(int count)
        {
            if (new T() is byte)
            {
                byte[] qt = new byte[count];
                m_Native.Read(qt, 0, count);
                return qt as T[];
            }
            uint num;
            if (!m_Native.CanRead)
            {
                throw new NotSupportedException();
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            long num2 = (long)((ulong)Marshal.SizeOf(new T()));
            if (((int)((this.Length - this.Position) / num2)) < count)
            {
                num = (uint)((int)((this.Length - this.Position) / num2));
            }
            else
            {
                num = (uint)count;
            }
            T[] localArray = new T[num];
            long num3 = num * num2;
            byte[] tBuf = new byte[num2];
            for (int i = 0; i < num; i++)
            {
                long q = Position + i * num2;
                m_Native.Read(tBuf, 0, (int)num2);
                localArray[i] = tBuf.GetStructure<T>();
            }
            return localArray;
        }

        public override T Read<T>()
        {
            if (!m_Native.CanRead)
            {
                throw new NotSupportedException();
            }
            long num2 = (long)((ulong)Marshal.SizeOf(new T()));
            long position = this.Position;
            if ((this.Length - position) < num2)
            {
                throw new EndOfStreamException();
            }
            byte[] tAr = new byte[num2];
            m_Native.Read(tAr, 0, (int)num2);
            T local = tAr.GetStructure<T>();
            return local;
        }
    }

    public class eCArchiveFile : eCFile
    {
        public List<string> m_Strings;
        char[] DeadBeef;
        short Version;
        int magic;
        byte Version2;

        protected eCArchiveFile(){}

        public eCArchiveFile(EFile a_File)
        {
            Handle = a_File;
            m_Native = a_File.Open(FileAccess.Read);
            DeadBeef = Read<char>(8);
            Version = Read<short>();
            int StringOffset = Read<int>();
            Position = StringOffset;
            magic = Read<int>();
            Version2 = Read<byte>();
            m_Strings = new List<string>(Read<int>());
            for (int i = 0; i < m_Strings.Capacity; i++)
                m_Strings.Add(new String(Read<char>(Read<short>())));
        }

        new public static eCArchiveFile CreateNew(EFile a_File)
        {
            a_File.Create(FileMode.Create);
            eCArchiveFile a = new eCArchiveFile();
            a.Handle = a_File;
            a.m_Native = a_File.Open(FileAccess.Read);
            a.m_Strings = new List<string>();
            a.DeadBeef = "GENOMFLE".ToCharArray();
            a.Version = 1;
            a.magic = -559038737;
            a.Version2 = 1;

            return a;
        }

        public static eCArchiveFile CreateBuffer()
        {
            eCArchiveFile e = new eCArchiveFile();
            e.m_Strings = new List<string>();
            return e;
        }

        public override void Save(BinaryFileBlock a_DataBlock)
        {
            TempStream t0 = new TempStream(a_DataBlock.Size, this);
            a_DataBlock.Serialize(t0);

            int size = 14 + 9;
            for (int i = 0; i < m_Strings.Count; i++)
                size += m_Strings[i].Length + 2;

            m_Native = Handle.Open(FileAccess.Write);
            m_Native.SetLength(size + (int)t0.Length);
            Write<char>(DeadBeef);
            Write<short>(Version);
            Write<int>(14 + (int)t0.Length);
            Write<byte>(t0.GetAllBytes());
            Write<int>(magic);
            Write<byte>(Version2);
            Write<int>(m_Strings.Count);
            for (int i = 0; i < m_Strings.Count; i++)
            {
                Write<short>((short)m_Strings[i].Length);
                Write<char>(m_Strings[i].ToCharArray());
            }
            Handle.Close();
        }

        public override void Close()
        {
            Handle.Close();
        }

        public override string ReadString()
        {
            short s = Read<short>();
            return m_Strings[s];
        }

        public override void WriteString(string s)
        {
            int i = getIndex(s);
            Write<short>((short)i);
        }

        public int getIndex(string s)
        {
            int i = m_Strings.IndexOf(s);
            if (i == -1)
            {
                i = m_Strings.Count;
                m_Strings.Add(s);
            }
            return i;
        }
    }

    public class eCDocArchive : BinaryFileBlock, IEnumerable<bCAccessorPropertyObject>
    {
        byte b0;
        List<c0> data;

        class c0
        {
            public int i0, i1;
            public bCString b0;
            public bCAccessorPropertyObject obj0;
            public override string ToString()
            {
                return b0.ToString();
            }
        }

        class c1 : IEnumerator<bCAccessorPropertyObject>
        {
            List<c0> L;
            int i = 0;

            internal c1(List<c0> A)
            {
                L = A;
                i = -1;
            }

            public bCAccessorPropertyObject Current
            {
                get { return i < L.Count ? L[i].obj0 : null; }
            }

            public void Dispose()
            {
                Reset();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                i++;
                return i < L.Count;
            }

            public void Reset()
            {
                i = -1;
            }
        }

        public eCDocArchive(IFile a_File)
        {
            deSerialize(a_File);
        }

        public override void deSerialize(IFile _File)
        {
            uint magic = _File.Read<uint>();
            if (magic == 825241686)
            {
                b0 = _File.Read<byte>();
                data = new List<c0>(_File.Read<int>());
                for (int i = 0; i < data.Capacity; i++)
                {
                    c0 c = new c0();
                    c.i0 = _File.Read<int>();
                    c.i1 = _File.Read<int>();
                    c.b0 = new bCString(_File);
                    data.Add(c);
                }
                for (int i = 0; i < data.Count; i++)
                    data[i].obj0 = new bCAccessorPropertyObject(_File);
            }
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<uint>(825241686);
            a_File.Write<byte>(b0);
            a_File.Write<int>(data.Count);
            foreach (c0 c in data)
            {
                a_File.Write<int>(c.i0);
                a_File.Write<int>(c.i1);
                c.b0.Serialize(a_File);
            }
            foreach (c0 c in data)
                c.obj0.Serialize(a_File);
        }

        public override int Size
        {
            get 
            {
                int q = 9;
                foreach (c0 c in data)
                {
                    q += c.b0.Size + c.obj0.Size + 8;
                }
                return q;
            }
        }

        public bCAccessorPropertyObject this[int index]
        {
            get
            {
                return data[index].obj0;
            }
        }

        public IEnumerator<bCAccessorPropertyObject> GetEnumerator()
        {
            return new c1(data);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new c1(data);
        }
    }

    public class TempStream : IFile
    {
        eCArchiveFile file;
        BinaryStream data;

        public TempStream(byte[] buffer, IFile file)
        {
            this.file = file as eCArchiveFile;
            if (file is TempStream)
                this.file = (file as TempStream).file;
            data = new BinaryStream(buffer);
        }

        public TempStream(long length, IFile file)
        {
            this.file = file as eCArchiveFile;
            if (file is TempStream)
                this.file = (file as TempStream).file;
            data = new BinaryStream(length);
        }

        public override long Length
        {
            get { return data.Length; }
        }

        public override long Position
        {
            get
            {
                return data.Position;
            }
            set
            {
                data.Position = value;
            }
        }

        public override T Read<T>()
        {
            return data.Read<T>();
        }

        public override T[] Read<T>(int l)
        {
            return data.ReadRange<T>(l);
        }

        public override string ReadString()
        {
            short l = data.Read<short>();
            if (file == null)
                return new string(data.ReadRange<char>(l));
            else return file.m_Strings[l];
        }

        public override void Write<T>(T t)
        {
            data.Write<T>(t);
        }

        public override void Write<T>(T[] t)
        {
            data.WriteRange<T>(t);
        }

        public override void WriteString(string s)
        {
            if (file == null)
            {
                data.Write<short>((short)s.Length);
                data.WriteRange<char>(s.ToCharArray());
            }
            else
            {
                data.Write<short>((short)file.getIndex(s));
            }
        }

        public byte[] GetAllBytes()
        {
            return data.GetAllBytes();
        }
    }

    public class eCProcessibleElement : BinaryFileBlock
    {
        int magic;
        public bCAccessorPropertyObject acc;

        private eCProcessibleElement() { }

        public eCProcessibleElement(int magic, IFile F)
        {
            this.magic = magic;
            acc = new bCAccessorPropertyObject(F);
        }

        public eCProcessibleElement(IFile a_File)
        {
            deSerialize(a_File);
        }

        public override void deSerialize(IFile a_File)
        {
            magic = a_File.Read<int>();
            acc = new bCAccessorPropertyObject(a_File); 
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<int>(magic);
            acc.Serialize(a_File);
        }

        public override BinaryFileBlock Clone()
        {
            eCProcessibleElement e = new eCProcessibleElement();
            e.magic = magic;
            e.acc = (bCAccessorPropertyObject)acc.Clone();
            return e;
        }

        public override int Size
        {
            get { return 4 + acc.Size; }
        }
    }

    public class bCObjectBase : classData
    {
        protected short Version;
        public g_PropertyCollection Properties { get; protected set; }
        public classData Class { get; protected set; }

        public bCObjectBase(IFile a_File)
        {
            deSerialize(a_File);
        }

        public bCObjectBase()
        {
            Version = 210;
            Properties = new g_PropertyCollection();
            Class = new BUFFER(1, 0);
        }

        public override void deSerialize(IFile a_File)
        {
            string ObjectClassName = "INVALID";
            Version = a_File.Read<short>();
            int Size = a_File.Read<int>();
            int p = (int)a_File.Position;
            Properties = new g_PropertyCollection(a_File);
            p = (int)a_File.Position - p;
            int cSize = Size - p;
            object o = classData.CreateClassData(ObjectClassName, this, cSize);
            if (o is classData)
            {
                (o as classData).deSerialize(a_File);
                Class = o as classData;
            }
            else Class = new BUFFER(a_File, cSize);
        }

        public override void Serialize(IFile s)
        {
            int cL = Class.Size;
            s.Write<short>(Version);
            s.Write<int>((int)(Properties.Size + cL));
            Properties.Serialize(s);
            Class.Serialize(s);
        }

        public override BinaryFileBlock Clone()
        {
            bCObjectBase r = (bCObjectBase)Activator.CreateInstance(this.GetType());
            r.Version = Version;
            r.Properties = Properties.Clone() as g_PropertyCollection;
            r.Class = classData.CloneClassData(Class, r);
            return r;
        }

        public override int Size
        {
            get
            {
                int b = 6 + Properties.Size;
                b += Class.Size;
                return b;
            }
        }

        public override string ToString()
        {
            return Class.ClassName;
        }        
    }

    public class bCObjectRefBase : bCObjectBase
    {
        short Version;

        public bCObjectRefBase(IFile S)
        {
            deSerialize(S);
        }

        public bCObjectRefBase()
        {
            Version = 210;
        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
        }

        public override int Size
        {
            get { return 2; }
        }

        public override BinaryFileBlock Clone()
        {
            bCObjectRefBase B = new bCObjectRefBase();
            B.Version = Version;
            return B;
        }
    }

    public class bCAccessorPropertyObject : bCObjectBase
    {
        new short Version;
        byte Valid;
        short Unknown1;
        byte Unknown2;
        bCString className;
        short Unknown3;
        byte Unknown4;
        short ObjectVersion;
        short Unknown5;

        public bCAccessorPropertyObject(IFile s)
        {
            deSerialize(s);
        }

        public bCAccessorPropertyObject(classData C, string N = null)
        {
            C.Container = this;
            if (N == null)
                N = C.ClassName;
            Version = 1;
            Valid = 1;
            Unknown1 = 1;
            Unknown2 = 1;
            className = new bCString(N);
            Unknown3 = 1;
            Unknown4 = 0;
            ObjectVersion = 1;
            Unknown5 = 201;
            base.Version = 201;
            base.Class = C;
            System.Reflection.FieldInfo FIs = C.GetType().GetField( "Version", BindingFlags.Instance | BindingFlags.NonPublic);
            if (FIs != null)
            {
                if(FIs.FieldType == typeof(short))
                    ObjectVersion = (short)FIs.GetValue(C);
                else if (FIs.FieldType == typeof(int))
                    ObjectVersion = (short)(int)FIs.GetValue(C);
                else if (FIs.FieldType == typeof(uint))
                    ObjectVersion = (short)(uint)FIs.GetValue(C);
                else if (FIs.FieldType == typeof(ushort))
                    ObjectVersion = (short)(ushort)FIs.GetValue(C);
            }
        }

        private bCAccessorPropertyObject() { }

        public override void deSerialize(IFile s)
        {
            Version = s.Read<short>();
            Valid = s.Read<byte>();
            if (Valid != 0)
            {
                Unknown1 = s.Read<short>();
                Unknown2 = s.Read<byte>();
                className = new bCString(s);
                Unknown3 = s.Read<short>();
                Unknown4 = s.Read<byte>();
                ObjectVersion = s.Read<short>();
                Unknown5 = s.Read<short>();
                int PropertyDataSize = s.Read<int>();
                int PropertyStart = (int)s.Position;
                Properties = new g_PropertyCollection(s);
                int ClassStart = (int)s.Position;
                int ClassEnd = PropertyDataSize - ((int)s.Position - PropertyStart) + ClassStart;
                object o = classData.CreateClassData(this.className.pString, this, ClassEnd - ClassStart);
                if (o is classData)
                {
                    (o as classData).deSerialize(s);
                    if (ClassEnd != s.Position)
                        SystemLog.Append(LogImportance.Information, "Property Set : " + className.pString + ", was not correctly read or the preceeding sizeof was incorrect.");
                    Class = o as classData;
                }
                else Class = new BUFFER(s, (int)(ClassEnd - (int)s.Position));
            }
        }

        public override void Serialize(IFile s)
        {
            int l = Class.Size;
            s.Write<short>(Version);
            s.Write<byte>(Valid);
            if (Valid != 0)
            {
                s.Write<short>(Unknown1);
                s.Write<byte>(Unknown2);
                className.Serialize(s);
                s.Write<short>(Unknown3);
                s.Write<byte>(Unknown4);
                s.Write<short>(ObjectVersion);
                s.Write<short>(Unknown5);
                s.Write<int>(Properties.Size + l);
                Properties.Serialize(s);
                Class.Serialize(s);
            };
        }

        public override BinaryFileBlock Clone()
        {
            bCAccessorPropertyObject g = new bCAccessorPropertyObject();
            g.Version = Version;
            g.Valid = Valid;
            g.Unknown1 = Unknown1;
            g.Unknown2 = Unknown2;
            g.className = (bCString)className.Clone();
            g.Unknown3 = Unknown3;
            g.Unknown4 = Unknown4;
            g.ObjectVersion = ObjectVersion;
            g.Unknown5 = Unknown5;
            g.Properties = Properties.Clone() as g_PropertyCollection;
            g.Class = classData.CloneClassData(Class, g);
            return g;
        }

        public override int Size
        {
            get
            {
                int l = Class.Size;
                int b = 3;
                if (Valid != 0)
                    b += 14 + Properties.Size + l + className.Size;
                return b;
            }
        }

        public string ClassName
        {
            get
            {
                return className.pString;
            }
        }

        public T Query<T>() where T : classData
        {
            return Class as T;
        }

        public override string ToString()
        {
            return ClassName;
        }
    }

    public class bCProperty : BinaryFileBlock
    {
        bCString gName { get; set; }
        bCString gType { get; set; }
        short Version;
        object savedOBJ;//native = int; BinaryFileBlock = bCString; Enums = ;

        public bCProperty(IFile s)
        {
            deSerialize(s);
        }

        public bCProperty(string N, string T1, object O)
        {
            gName = new bCString(N);
            gType = new bCString(T1);
            Version = 30;
            savedOBJ = O;
        }

        public override void deSerialize(IFile s)
        {
            gName = new bCString(s);
            gType = new bCString(s);
            Version = s.Read<short>();
            byte[] Data = s.Read<byte>(s.Read<int>());
            savedOBJ = getObject(gType.pString, new TempStream(Data, s), true);
        }

        public override void Serialize(IFile s)
        {
            gName.Serialize(s);
            gType.Serialize(s);
            s.Write<short>(Version);
            setObject(gType.pString, savedOBJ, s, true);
        }

        public override int Size
        {
            get
            {
                int q = 6 + gName.Size + gType.Size;
                q += getObjectSize(gType.pString, savedOBJ, true);
                return q;
            }
        }

        static string getTrueType(string subType)
        {
            string trueType = subType;
            trueType = trueType.Replace("long", "int").Replace("bCVector", "Vector3").Replace("bCVector2", "Vector2").Replace("bCVector4", "Vector4").Replace("bCQuaternion", "Quaternion").Replace("bCMatrix", "Matrix");
            if (subType.Contains(" *")) trueType = "int";
            return trueType;
        }
        internal static object getObject(string gType, IFile Data, bool readEnumHeader)
        {
            switch (gType)
            {
                case "int"://NATIVE
                    return Data.Read<int>();
                case "bool":
                    return Convert.ToBoolean(Data.Read<byte>());
                case "short":
                    return Data.Read<short>();
                case "float":
                    return Data.Read<float>();
                case "long":
                    return Data.Read<int>();
                case "char":
                    return (char)Data.Read<byte>();
                case "bCVector":
                    return Data.Read<Vector3>();
                case "bCVector2":
                    return Data.Read<Vector2>();
                case "bCVector4":
                    return Data.Read<Vector4>();
                case "bCMatrix":
                    return Data.Read<Matrix>();
                case "bCQuaternion":
                    return Data.Read<Quaternion>();
                case "bCGuid"://FILE BLOCKS
                    return new bCGuid(Data);
                case "bCString":
                    return new bCString(Data);
                case "bCImageOrMaterialResourceString":
                    return new bCImageOrMaterialResourceString(Data);
                case "eCLocString":
                    return new eCLocString(Data);
                case "bCImageResourceString":
                    return new bCImageResourceString(Data);
                case "gCLetterLocString":
                    return new gCLetterLocString(Data);
                case "bCSpeedTreeResourceString":
                    return new bCSpeedTreeResourceString(Data);
                case "gCBookLocString":
                    return new gCBookLocString(Data);
                case "bCMeshResourceString":
                    return new bCMeshResourceString(Data);
                case "gCInfoLocString":
                    return new gCInfoLocString(Data);
                case "gCQuestLocString":
                    return new gCQuestLocString(Data);
                case "eCTipLocString":
                    return new eCTipLocString(Data);
                case "gCNPCInfoLocString":
                    return new gCNPCInfoLocString(Data);
                case "bCFloatColor" :
                    return new bCFloatColor(Data);
                case "bCByteColor":
                    return new bCByteColor(Data);
                case "bCByteAlphaColor":
                    return new bCByteAlphaColor(Data);
                case "bCEulerAngles":
                    return new bCEulerAngles(Data);
                case "bCRange1":
                    return new bCRange1(Data);
                case "bCRange2":
                    return new bCRange1(Data);
                case "bCRange3":
                    return new bCRange1(Data);
                case "bCRange4":
                    return new bCRange1(Data);
                case "bCBox" :
                    return new bCBox(Data);
                case "bCFloatAlphaColor" :
                    return new bCFloatAlphaColor(Data);
                case "eCGuiBitmapProxy2":
                    return new eCGuiBitmapProxy2(Data);
                case "eCScriptProxyScript":
                    return new eCScriptProxyScript(Data);
                case "eCEntityProxy":
                    return new eCEntityProxy(Data);
                case "eCTemplateEntityProxy":
                    return new eCTemplateEntityProxy(Data);
                case "gCScriptProxyAIState":
                    return new gCScriptProxyAIState(Data);
                case "gCScriptProxyAIFunction":
                    return new gCScriptProxyAIFunction(Data);
                case "gCFlightPathSeeking":
                    return new gCFlightPathSeeking(Data);
                case "gCFlightPathBallistic":
                    return new gCFlightPathBallistic(Data);
                case "gCSkillValue":
                    return new gCSkillValue(Data);
                case "gCModifySkill":
                    return new gCModifySkill(Data);
                case "gCCraftIngredient":
                    return new gCCraftIngredient(Data);
                case"bCMotion":
                    return new bCMotion(Data);
                case "eCSubMesh":
                    return new eCSubMesh(Data);
                case "gCMapEntry" :
                    return new gCMapEntry(Data);
                case "eCEntityStringProxy" :
                    return new eCEntityStringProxy(Data);
                case "gSRoutine" :
                    return new gSRoutine(Data);
                case "gCWaterSubZone_PS" :
                    return new gCWaterSubZone_PS(Data);
            }
            if (gType.Contains("Array<"))
            {
                if (gType.Contains("*"))
                    return new BUFFER(Data);
                int cf = gType.IndexOf('<');
                int ce = gType.IndexOf('>');
                string name = gType.Substring(0, cf);
                string subType = gType.Substring(cf + 1, ce - cf - 1).Replace("struct ", "").Replace("class ", "").Replace("enum ", "");
                if (subType.Contains("::"))
                    subType = subType.Remove(0, subType.IndexOf("::") + 2);
                string trueType = getTrueType(subType);
                Type d1 = typeof(bTObjArray<int>).GetGenericTypeDefinition();
                Type[] typeArgs = { TypeCache.GetType(trueType) };
                if (typeArgs[0] == null)//TMP
                    throw new Exception();
                Type makeme = d1.MakeGenericType(typeArgs);
                if(typeArgs[0].IsEnum)
                    subType = "bTPropertyContainer " + subType;
                object o = Activator.CreateInstance(makeme, new object[] { Data, subType });
                return o;
            }
            else if (gType.Contains("bTPropertyContainer"))
            {
                string ClassName = gType.Replace("bTPropertyContainer<enum ", "").Replace("bTPropertyContainer ", "");
                ClassName = typeof(eECollisionShapeType).Namespace + "." + ClassName.Replace(">", "");
                Type t = Type.GetType(ClassName, false, true);
                if (t == null)
                    return null;
                if(readEnumHeader)
                    Data.Position += 2L;
                return Enum.ToObject(t, Data.Read<uint>());
            }
            else if (gType.Contains("bTInterpolator"))
            {
                string fName = gType.Substring(0, gType.IndexOf("<"));
                string tN = gType.Replace(fName + "<", "");
                tN = tN.Remove(tN.Length - 2, 2).Replace("struct ", "").Replace("class ", "").Replace("enum ", "");
                string[] pTs = tN.Split(',');
                string[] pFs = new string[] { getTrueType(pTs[0]), getTrueType(pTs[1]) };
                Type[] typeArgs = { TypeCache.GetType(pFs[0]), TypeCache.GetType(pFs[1]) };
                for(int i = 0; i < pTs.Length; i++)//this must operate on real names
                    if (pTs[i].Contains("<"))
                    {
                        string falseTopType = pTs[i].Substring(0, pTs[i].IndexOf('<'));
                        string falseSubType = pTs[i].Replace(falseTopType + "<", "").Replace(">", "");
                        string realSubType = getTrueType(falseSubType);
                        string realTopType = getTrueType(falseTopType);
                        Type topType = TypeCache.GetType(realTopType + "`1");
                        typeArgs[i] = topType.MakeGenericType(TypeCache.GetType(realSubType));
                    }
                Type d1 = TypeCache.GetType(fName + "`2");
                Type makeme = d1.MakeGenericType(typeArgs);
                object o = Activator.CreateInstance(makeme, new object[] { Data, pTs });
                return o;
            }
            else if (gType.Contains("bSKeyFrame"))
            {
                string fName = gType.Substring(0, gType.IndexOf("<"));
                string tN = gType.Replace(fName + "<", "");
                tN = tN.Remove(tN.Length - 1, 1).Replace("struct ", "").Replace("class ", "").Replace("enum ", "");
                string trueType = getTrueType(tN);
                Type d1 = (gType.Contains("Bezier") ? typeof(bSKeyFrameBezier<int>) : typeof(bSKeyFrameLinearEx<int>)).GetGenericTypeDefinition();
                Type[] typeArgs = { TypeCache.GetType(trueType) };
                if (typeArgs[0] == null)//TMP
                    throw new Exception();
                Type makeme = d1.MakeGenericType(typeArgs);
                object o = Activator.CreateInstance(makeme, new object[] { Data, tN });
                return o;
            }
            else if (TypeCache.GetType(gType) != null)
            {
                Type T = TypeCache.GetType(gType);
                if (T.IsEnum)
                    return Enum.ToObject(T, Data.Read<uint>());
                else if (T.IsSubclassOf(typeof(classData)))
                {
                    object o = Activator.CreateInstance(T); ;
                    (o as classData).deSerialize(Data);
                    return o;
                }
                else if (T.IsSubclassOf(typeof(BinaryFileBlock)))
                    return Activator.CreateInstance(T, new object[] { Data });
            }
            throw new Exception("Type not found : " + gType);
        }
        internal static void setObject(string gType, object savedOBJ, IFile s, bool writeEnumHeader)
        {
            if (gType.Contains("bTPropertyContainer"))
            {
                if (writeEnumHeader)
                {
                    s.Write<int>(6);
                    s.Write<short>(201);//something like that
                }
                Enum e = (Enum)savedOBJ;
                s.Write<uint>(Convert.ToUInt32(e));
            }
            else if (savedOBJ is BinaryFileBlock)
            {
                if (writeEnumHeader)
                    s.Write<int>((savedOBJ as BinaryFileBlock).Size);
                (savedOBJ as BinaryFileBlock).Serialize(s);
            }
            else
            {
                switch (gType)
                {
                    case "int"://NATIVE
                        if (writeEnumHeader)
                            s.Write<int>(4);
                        s.Write<int>((int)savedOBJ);
                        break;
                    case "bool":
                        if (writeEnumHeader)
                            s.Write<int>(1);
                        s.Write<byte>(Convert.ToByte(savedOBJ));
                        break;
                    case "short":
                        if (writeEnumHeader)
                            s.Write<int>(2);
                        s.Write<short>((short)savedOBJ);
                        break;
                    case "float":
                        if (writeEnumHeader)
                            s.Write<int>(4);
                        s.Write<float>((float)savedOBJ);
                        break;
                    case "long":
                        if (writeEnumHeader)
                            s.Write<int>(4);
                        s.Write<int>((int)savedOBJ);
                        break;
                    case "char":
                        if (writeEnumHeader)
                            s.Write<int>(1);
                        s.Write<char>((char)savedOBJ);
                        break;
                    case "bCVector":
                        if (writeEnumHeader)
                            s.Write<int>(12);
                        s.Write<Vector3>((Vector3)savedOBJ);
                        break;
                    case "bCVector2":
                        if (writeEnumHeader)
                            s.Write<int>(8);
                        s.Write<Vector2>((Vector2)savedOBJ);
                        break;
                    case "bCVector4":
                        if (writeEnumHeader)
                            s.Write<int>(16);
                        s.Write<Vector4>((Vector4)savedOBJ);
                        break;
                    case "bCMatrix":
                        if (writeEnumHeader)
                            s.Write<int>(16 * 4);
                        s.Write<Matrix>((Matrix)savedOBJ);
                        break;
                }
            }
        }
        internal static int getObjectSize(string gType, object savedOBJ, bool countEnumHeader)
        {
            int q = 0;
            if (gType.Contains("bTPropertyContainer"))
                q += countEnumHeader ? 6 : 4;
            else if (savedOBJ is BinaryFileBlock)
                q += (savedOBJ as BinaryFileBlock).Size;
            else
            {
                switch (gType)
                {
                    case "int":
                        q += 4;
                        break;
                    case "bool":
                        q += 1;
                        break;
                    case "short":
                        q += 2;
                        break;
                    case "float":
                        q += 4;
                        break;
                    case "long":
                        q += 4;
                        break;
                    case "char":
                        q += 1;
                        break;
                    case "bCVector":
                        q += 12;
                        break;
                    case "bCVector2":
                        q += 8;
                        break;
                    case "bCVector4":
                        q += 16;
                        break;
                    case "bCMatrix":
                        q += 16 * 4;
                        break;
                }
            }
            return q;
        }

        public override BinaryFileBlock Clone()
        {
            TempStream S = new TempStream(Size, eCArchiveFile.CreateBuffer());
            Serialize(S);
            S.Position = 0;
            return new bCProperty(S);
        }

        public object Object
        {
            get
            {
                return savedOBJ;
            }

            set
            {
                savedOBJ = value;
            }
        }

        public T Query<T>() where T : struct
        {
            if (savedOBJ is T)
                return (T)savedOBJ;
            else throw new Exception("");
        }

        public T Query_<T>() where T : BinaryFileBlock
        {
            if (savedOBJ is T)
                return (T)savedOBJ;
            else throw new Exception("");
        }

        public string PropertyName
        {
            get
            {
                return gName.pString;
            }
        }

        public string PropertyType
        {
            get
            {
                return gType.pString;
            }
        }

        public override string ToString()
        {
            string d = Object == null ? "NotDisplayable" : Object.ToString();
            return "Name : " + gName.pString + " | DataType : " + gType.pString + " | Data : " + d;
        }
    }

    public class eCEntityPropertySet : bCObjectRefBase
    {
        short Version;
        byte unknown;

        private eCEntityPropertySet() { }

        public eCEntityPropertySet(IFile s)
        {
            deSerialize(s);   
        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
            if (Version > 1)
                unknown = a_File.Read<byte>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            if (Version > 1)
                s.Write<byte>(unknown);
        }

        public override int Size
        {
            get 
            {
                return 2 + ((Version > 1) ? 1 : 0);
            }
        }

        public override BinaryFileBlock Clone()
        {
            eCEntityPropertySet e = new eCEntityPropertySet();
            e.Version = Version;
            e.unknown = unknown;
            return e;
        }
    }

    public class bTPropertyContainer : BinaryFileBlock
    {
        uint val;
        Type m_Type;

        public bTPropertyContainer(uint i, string a_EnumTypeName)
        {
            val = i;
            a_EnumTypeName = typeof(eECollisionShapeType).Namespace + "." + a_EnumTypeName.Replace("bTPropertyContainer", "").Replace("<", "").Replace(">", "").Replace("enum ", "");
            m_Type = Type.GetType(a_EnumTypeName, false, true);
        }

        public bTPropertyContainer(IFile a_File, string a_EnumTypeName)
        {
            a_EnumTypeName = typeof(eECollisionShapeType).Namespace + "." + a_EnumTypeName.Replace("bTPropertyContainer", "").Replace("<", "").Replace(">", "").Replace("enum ", "");
            m_Type = Type.GetType(a_EnumTypeName, false, true);
            deSerialize(a_File);
        }

        public override void deSerialize(IFile a_File)
        {
            a_File.Position += 2L;
            val = a_File.Read<uint>();
        }

        public override BinaryFileBlock Clone()
        {
            return new bTPropertyContainer(val, m_Type.Name);
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(201);
            a_File.Write(val);
        }

        public override int Size
        {
            get { return 6; }
        }

        public Enum Object
        {
            get
            {
                return (Enum)Enum.ToObject(m_Type, val);
            }
        }
    }
    #endregion

    #region SubTypes
    #region BASIC
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCGuid : BinaryFileBlock
    {
        Guid Guid;
        byte Valid;
        byte Alignement0;
        byte Alignement1;
        byte Alignement2;

        public bCGuid()
        {
            Guid = Guid.Empty;
            Valid = 0;
            Alignement0 = Alignement1 = Alignement2 = 0;
        }

        public bCGuid(Guid g)
        {
            Guid = g;
            Valid = 1;
        }

        public bCGuid(IFile s)
        {
            deSerialize(s);
        }

        public bCGuid(Guid g, bool valid)
        {
            Guid = g;
            Valid = (byte)(valid ? 1 : 0);
        }

        public override void deSerialize(IFile a_File)
        {
            Guid = new System.Guid(a_File.Read<byte>(16));
            Valid = a_File.Read<byte>();
            Alignement0 = a_File.Read<byte>();
            Alignement1 = a_File.Read<byte>();
            Alignement2 = a_File.Read<byte>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<byte>(Guid.GetBytes());
            s.Write<byte>(Valid);
            s.Write<byte>(Alignement0);
            s.Write<byte>(Alignement1);
            s.Write<byte>(Alignement2);
        }

        public override BinaryFileBlock Clone()
        {
            bCGuid g = new bCGuid(Guid);
            g.Alignement0 = Alignement0;
            g.Alignement1 = Alignement1;
            g.Alignement2 = Alignement2;
            g.Valid = Valid;
            return g;
        }

        public override int Size
        {
            get
            {
                return 20;
            }
        }

        public override string ToString()
        {
            return "Valid : " + Valid.ToString() + (Valid != 0 ? "Value : " + Guid : "");
        }

        public static implicit operator Guid(bCGuid d)
        {
            return d.Guid;
        }

        public bool IsValid
        {
            get
            {
                return Valid == 1;
            }
            set
            {
                Valid = (byte)(value ? 1 : 0);
            }
        }

        public static bool operator ==(bCGuid x, bCGuid y)
        {
            if ((object)x == null && (object)y != null)
                return ((object)y == null);
            if ((object)y == null)
                return ((object)x == null);
            return x.Guid == y.Guid;
        }

        public static bool operator !=(bCGuid x, bCGuid y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            if (obj is bCGuid)
                return this == (bCGuid)obj;
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Guid Value
        {
            get
            {
                return Guid;
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCPropertyID : BinaryFileBlock
    {
        Guid Guid;
        int Alignement;

        static Guid G = new Guid("{00000000-0000-0000-0000-000000000000}");

        public bCPropertyID()
        {
            Guid = G;
            Alignement = 0;
        }

        public bCPropertyID(IFile bs)
        {
            deSerialize(bs);
        }

        public bCPropertyID(Guid g)
        {
            Guid = g;
            Alignement = 0;
        }

        public override void deSerialize(IFile a_File)
        {
            Guid = new System.Guid(a_File.Read<byte>(16));
            Alignement = a_File.Read<int>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<byte>(Guid.ToByteArray());
            s.Write<int>(Alignement);
        }

        public override BinaryFileBlock Clone()
        {
            bCPropertyID p = new bCPropertyID(Guid);
            p.Alignement = Alignement;
            return p;
        }

        public static bCPropertyID NewRandomID()
        {
            bCPropertyID id = new bCPropertyID(Guid.NewGuid());
            return id;
        }

        public override int Size
        {
            get
            {
                return 20;
            }
        }

        public override string ToString()
        {
            return Guid.ToString();
        }

        public Guid Value
        {
            get
            {
                return Guid;
            }
            set
            {
                Guid = value;
            }
        }

        public static bool operator ==(bCPropertyID x, bCPropertyID y)
        {
            if ((object)x == null && (object)y != null)
                return ((object)y == null);
            if ((object)y == null)
                return ((object)x == null);
            return x == y;
        }

        public static bool operator !=(bCPropertyID x, bCPropertyID y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            if (obj is bCPropertyID)
                return this == (bCPropertyID)obj;
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCDateTime : BinaryFileBlock
    {
        long l;

        public bCDateTime()
            :this(DateTime.Now)
        {

        }

        public bCDateTime(DateTime T)
        {
            l = T.ToFileTime();
        }

        public bCDateTime(IFile S)
        {
            deSerialize(S);
        }

        public bCDateTime(long l)
        {
            this.l = l;
        }

        public override void deSerialize(IFile a_File)
        {
            l = a_File.Read<long>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<long>(l);
        }

        public override int Size
        {
            get { return 8; }
        }

        public override BinaryFileBlock Clone()
        {
            return new bCDateTime(l);
        }

        public static implicit operator DateTime(bCDateTime time)
        {
            return DateTime.FromFileTime(time.l);
        }

        public DateTime Time
        {
            get
            {
                return DateTime.FromFileTime(l);
            }
            set
            {
                l = value.ToFileTime();
            }
        }

        public override string ToString()
        {
            return Time.ToString();
        }
    }
    #endregion

    #region MATH
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCEulerAngles : BinaryFileBlock
    {
        public float m_fYaw { get; set; }
        public float m_fPitch { get; set; }
        public float m_fRoll { get; set; }

        public bCEulerAngles(float m1, float m2, float m3)
        {
            m_fYaw = m1;
            m_fPitch = m2;
            m_fRoll = m3;
        }

        public bCEulerAngles(IFile s)
        {
            deSerialize(s);
        }

        public override void deSerialize(IFile a_File)
        {
            m_fYaw = a_File.Read<float>();
            m_fPitch = a_File.Read<float>();
            m_fRoll = a_File.Read<float>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<float>(m_fYaw);
            s.Write<float>(m_fPitch);
            s.Write<float>(m_fRoll);
        }

        public override int Size
        {
            get { return 12; }
        }

        public override string ToString()
        {
            return "Yaw : " + m_fYaw.ToString() + " | Pitch : " + m_fPitch.ToString() + " | Roll : " + m_fRoll.ToString();
        }

        public override BinaryFileBlock Clone()
        {
            return new bCEulerAngles(m_fYaw, m_fPitch, m_fRoll);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCBox : BinaryFileBlock
    {
        Vector3 Min { get; set; }
        Vector3 Max { get; set; }

        public bCBox(BoundingBox bb)
        {
            bb = bb.nCalc();
            Min = bb.Minimum;
            Max = bb.Maximum;
        }

        public bCBox(Vector3 mi, Vector3 ma)
        {
            Min = mi;
            Max = ma;
        }

        public bCBox(IFile S)
        {
            deSerialize(S);
        }

        public override void deSerialize(IFile a_File)
        {
            Min = a_File.Read<Vector3>();
            Max = a_File.Read<Vector3>(); 
        }

        public override void Serialize(IFile s)
        {
            s.Write<Vector3>(Min);
            s.Write<Vector3>(Max);
        }

        public override int Size
        {
            get { return 24; }
        }

        public override string ToString()
        {
            return "Min : " + Min.ToString() + " | Max : " + Max.ToString();
        }

        public void Scale(float f)
        {
            Min *= f;
            Max *= f;
        }

        public override BinaryFileBlock Clone()
        {
            return new bCBox(Min, Max);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCOrientedBox : BinaryFileBlock
    {
        Vector3 m_vecCenter;
        Vector3 m_vecExtent;
        bCMatrix3 m_matOrientation;

        public bCOrientedBox(IFile S)
        {
            deSerialize(S);
        }

        public override void deSerialize(IFile a_File)
        {
            m_vecCenter = a_File.Read<Vector3>();
            m_vecExtent = a_File.Read<Vector3>();
            m_matOrientation = new bCMatrix3(a_File);
        }

        public override void Serialize(IFile s)
        {
            s.Write<Vector3>(m_vecCenter);
            s.Write<Vector3>(m_vecExtent);
            m_matOrientation.Serialize(s);
        }

        public override int Size
        {
            get { return 24 + m_matOrientation.Size; }
        }

        private bCOrientedBox() { }
        public override BinaryFileBlock Clone()
        {
            bCOrientedBox a = new bCOrientedBox();
            a.m_vecCenter = m_vecCenter;
            a.m_vecExtent = m_vecExtent;
            a.m_matOrientation = (bCMatrix3)m_matOrientation.Clone();
            return a;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCRange<T> : BinaryFileBlock where T : struct
    {
        public T Min
        {
            get;
            set;
        }
        public T Max
        {
            get;
            set;
        }

        public bCRange(T min, T max)
        {
            Min = min;
            Max = max;
        }

        public bCRange(IFile a_File)
        {
            deSerialize(a_File);
        }

        public override void deSerialize(IFile a_File)
        {
            Min = a_File.Read<T>();
            Max = a_File.Read<T>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<T>(Min);
            a_File.Write<T>(Max);
        }

        public override BinaryFileBlock Clone()
        {
            return new bCRange<T>(Min, Max);
        }

        public unsafe override int Size
        {
            get { return Marshal.SizeOf(Min) * 2; }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCRange1 : bCRange<float>
    {
        public bCRange1(float mi, float ma)
            : base(mi, ma)
        {
        }
        public bCRange1(IFile a_File)
            : base(a_File)
        {
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCRange2 : bCRange<Vector2>
    {
        public bCRange2(Vector2 mi, Vector2 ma)
            : base(mi, ma)
        {
        }
        public bCRange2(IFile a_File)
            : base(a_File)
        {
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCRange3 : bCRange<Vector3>
    {
        public bCRange3(Vector3 mi, Vector3 ma)
            : base(mi, ma)
        {
        }
        public bCRange3(IFile a_File)
            : base(a_File)
        {
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCRange4 : bCRange<Vector4>
    {
        public bCRange4(Vector4 mi, Vector4 ma)
            : base(mi, ma)
        {
        }
        public bCRange4(IFile a_File)
            : base(a_File)
        {
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCSphere : BinaryFileBlock
    {
        public float Radius { get; set; }//RELEVANT
        public Vector3 Position { get; set; }//RELEVANT

        public bCSphere(IFile bs)
        {
            deSerialize(bs);
        }

        public bCSphere(BoundingSphere B)
        {
            Radius = B.Radius;
            Position = B.Center;
        }

        private bCSphere()
        {

        }

        public override void deSerialize(IFile a_File)
        {
            Radius = a_File.Read<float>();
            Position = a_File.Read<Vector3>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<float>(Radius);
            s.Write<Vector3>(Position);
        }

        public override BinaryFileBlock Clone()
        {
            bCSphere s = new bCSphere();
            s.Radius = Radius;
            s.Position = Position;
            return s;
        }

        public override int Size
        {
            get
            {
                return 16;
            }
        }

        public override string ToString()
        {
            return new BoundingSphere(Position, Radius).ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCPlane : BinaryFileBlock
    {
        public Vector3 m_vecNormal { get; set; }
        public float m_fDistance { get; set; }

        public bCPlane(IFile bs)
        {
            deSerialize(bs);
        }

        public bCPlane(bCPlane B)
        {
            m_vecNormal = B.m_vecNormal;
            m_fDistance = B.m_fDistance;
        }

        private bCPlane()
        {

        }

        public override void deSerialize(IFile a_File)
        {
            m_vecNormal = a_File.Read<Vector3>();
            m_fDistance = a_File.Read<float>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<Vector3>(m_vecNormal);
            s.Write<float>(m_fDistance);
        }

        public override BinaryFileBlock Clone()
        {
            bCPlane s = new bCPlane();
            s.m_vecNormal = m_vecNormal;
            s.m_fDistance = m_fDistance;
            return s;
        }

        public override int Size
        {
            get
            {
                return 16;
            }
        }

        public override string ToString()
        {
            return new Plane(m_vecNormal, m_fDistance).ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCMatrix3 : BinaryFileBlock
    {
        public Vector3 row1, row2, row3;

        public bCMatrix3(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            row1 = v1;
            row2 = v2;
            row3 = v3;
        }

        public bCMatrix3(IFile s)
        {
            deSerialize(s);
        }

        public override void deSerialize(IFile a_File)
        {
            row1 = a_File.Read<Vector3>();
            row2 = a_File.Read<Vector3>();
            row3 = a_File.Read<Vector3>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<Vector3>(row1);
            s.Write<Vector3>(row2);
            s.Write<Vector3>(row3);
        }

        public override BinaryFileBlock Clone()
        {
            return new bCMatrix3(row1, row2, row3);
        }

        public override int Size
        {
            get { return 36; }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCCapsule : BinaryFileBlock
    {
        float m_fHeight;
        float m_fRadius;
        bCMatrix3 m_matOrientation;
        Vector3 m_vecCenter;

        public bCCapsule(float h, float r, bCMatrix3 M, Vector3 center)
        {
            m_fHeight = h;
            m_fRadius = r;
            m_matOrientation = M;
            m_vecCenter = center;
        }

        public bCCapsule(IFile S)
        {
            deSerialize(S);
        }

        public override void deSerialize(IFile a_File)
        {
            m_fHeight = a_File.Read<float>();
            m_fRadius = a_File.Read<float>();
            m_matOrientation = new bCMatrix3(a_File);
            m_vecCenter = a_File.Read<Vector3>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<float>(m_fHeight);
            s.Write<float>(m_fRadius);
            m_matOrientation.Serialize(s);
            s.Write<Vector3>(m_vecCenter);
        }

        public override BinaryFileBlock Clone()
        {
            return new bCCapsule(m_fHeight, m_fRadius, m_matOrientation, m_vecCenter);
        }

        public override int Size
        {
            get { return 56; }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCRect : BinaryFileBlock
    {
        public int Left, Top, Right, Bottom;

        public bCRect(IFile s)
        {
            deSerialize(s);
        }

        public bCRect(int x, int y, int w, int h)
        {
            Left = x;
            Top = y;
            Right = w + x;
            Bottom = h + y;
        }

        private bCRect()
        {

        }

        public System.Drawing.Rectangle pRectangle
        {
            get
            {
                return new System.Drawing.Rectangle(Left, Top, Right - Left, Bottom - Top);
            }
        }

        public override void deSerialize(IFile a_File)
        {
            Left = a_File.Read<int>();
            Top = a_File.Read<int>();
            Right = a_File.Read<int>();
            Bottom = a_File.Read<int>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<int>(Left);
            s.Write<int>(Top);
            s.Write<int>(Right);
            s.Write<int>(Bottom);
        }

        public override BinaryFileBlock Clone()
        {
            bCRect c = new bCRect();
            c.Left = Left;
            c.Top = Top;
            c.Right = Right;
            c.Bottom = Bottom;
            return c;
        }

        public override int Size
        {
            get
            {
                return 16;
            }
        }

        public override string ToString()
        {
            return pRectangle.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCPoint : BinaryFileBlock
    {
        public int X { get; set; }
        public int Y { get; set; }

        public bCPoint(int x, int y)
        {

        }

        public bCPoint(System.Drawing.Point p)
        {

        }

        public bCPoint(IFile a_File)
        {
            deSerialize(a_File);
        }

        public override BinaryFileBlock Clone()
        {
            return new bCPoint(X, Y);
        }

        public override void deSerialize(IFile a_File)
        {
            X = a_File.Read<int>();
            Y = a_File.Read<int>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<int>(X);
            a_File.Write<int>(Y);
        }

        public override int Size
        {
            get { return 8; }
        }

        public override string ToString()
        {
            return "X : " + X.ToString() + " | Y : " + Y.ToString();
        }
    }
    #endregion

    #region PROXIES
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class eCGuiBitmapProxy2 : BinaryFileBlock
    {
        public short Data { get; set; }

        public eCGuiBitmapProxy2(short D)
        {
            Data = D;
        }

        public override BinaryFileBlock Clone()
        {
            return new eCGuiBitmapProxy2(Data);
        }

        public eCGuiBitmapProxy2(IFile s)
        {
            deSerialize(s);
        }

        public override void deSerialize(IFile a_File)
        {
            Data = a_File.Read<short>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Data);
        }

        public unsafe override int Size
        {
            get { return 2; }
        }

        public override string ToString()
        {
            return Data.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Editor(typeof(eCScriptProxyScript_TypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class eCScriptProxyScript : BinaryFileBlock
    { 
        class eCScriptProxyScript_TypeEditor : System.Drawing.Design.UITypeEditor
        {
            static ComboBox dispBox;

            static eCScriptProxyScript_TypeEditor()
            {
                dispBox = new ComboBox();
                dispBox.DropDownStyle = ComboBoxStyle.DropDownList;
                string[] A = ResourceManager.Scripts;
                foreach (string s in A)
                    dispBox.Items.Add(s);
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                ILrentObject O = (context.Instance as PropertySetWrapper).Object;
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc != null)
                {
                    dispBox.SelectedIndex = dispBox.Items.IndexOf(value);
                    edSvc.DropDownControl(dispBox);
                    if(dispBox.SelectedItem == null)
                        return new eCScriptProxyScript("Empty");
                    return new eCScriptProxyScript(dispBox.SelectedItem.ToString());
                }
                return "Empty";
            }

            public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
            }

            public override bool GetPaintValueSupported(ITypeDescriptorContext context)
            {
                return false;
            }
        }

        bCString data;
        short version;

        public eCScriptProxyScript(IFile bs)
        {
            deSerialize(bs);
        }

        public eCScriptProxyScript(string s)
        {
            data = new bCString(s);
            version = 1;
        }

        public eCScriptProxyScript()
        {
            version = 1;
            data = new bCString();
        }

        public override BinaryFileBlock Clone()
        {
            eCScriptProxyScript q = new eCScriptProxyScript();
            q.data = (bCString)data.Clone();
            q.version = version;
            return q;
        }

        public override void deSerialize(IFile a_File)
        {
            version = a_File.Read<short>();
            byte b = a_File.Read<byte>();
            if (b != 0)
                data = new bCString(a_File);
            else data = null;
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(version);
            if (data != null && data.pString != "Emtpty")
            {
                s.Write<byte>(1);
                data.Serialize(s);
            }
            else s.Write<byte>(0);
        }

        public override int Size
        {
            get
            {
                if (this.data != null)
                    return 3 + data.Size;
                return 3;
            }
        }

        public string Object
        {
            get
            {
                return data == null ? "" : data.pString;
            }

            set
            {
                data = value == null ? null : new bCString(value);
            }
        }

        public override string ToString()
        {
            if (data == null) return "Emtpty";
            return data.pString;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class eCEntityProxy : BinaryFileBlock
    {
        short Version;
        public bCPropertyID ID { get; set; }

        public eCEntityProxy(IFile bs)
        {
            deSerialize(bs);
        }

        public eCEntityProxy(bCPropertyID ID)
        {
            Version = 1;
            this.ID = ID;
        }

        private eCEntityProxy()
        {

        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
            byte Valid = a_File.Read<byte>();
            if (Valid != 0)
                ID = new bCPropertyID(a_File);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            s.Write<byte>(Convert.ToByte(ID != null));
            if (ID != null)
                ID.Serialize(s);
        }

        public override BinaryFileBlock Clone()
        {
            eCEntityProxy e = new eCEntityProxy();
            e.Version = Version;
            if (ID != null)
                e.ID = ID.Clone() as bCPropertyID;
            return e;
        }

        public override int Size
        {
            get
            {
                int b = 3;
                if (ID != null)
                    b += ID.Size;
                return b;
            }
        }

        public override string ToString()
        {
            if (Entity != null)
                return Entity.Name;
            if(ID != null)
                return ID.ToString();
            return "Invalid";
        }

        public ILrentObject Entity
        {
            get
            {
                if (ID == null)
                    return null;
                foreach (LrentFile f in RisenWorld.Files)
                    foreach (ILrentObject f2 in f)
                        if (f2.GUID == ID.Value)
                            return f2;
                return null;
            }
            set
            {
                if (value == null)
                    ID = new bCPropertyID();
                else ID = new bCPropertyID(value.GUID);
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class eCTemplateEntityProxy : BinaryFileBlock
    {
        short Version;
        public bCGuid Guid { get; set; }

        public eCTemplateEntityProxy()
        {
            Version = 2;
            Guid = new bCGuid();
        }

        public eCTemplateEntityProxy(bCGuid g)
        {
            Version = 2;
            Guid = g;
        }

        public eCTemplateEntityProxy(IFile bs)
        {
            deSerialize(bs);
        }

        public eCTemplateEntityProxy(short Version, bCGuid g)
        {
            this.Version = Version;
            Guid = g;
        }

        public override BinaryFileBlock Clone()
        {
            eCTemplateEntityProxy q = new eCTemplateEntityProxy(Version, Guid);
            return q;
        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
            byte Valid = a_File.Read<byte>();
            if (Valid != 0)
                Guid = new bCGuid(a_File);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            s.Write<byte>(Convert.ToByte(Guid != null));
            if (Guid != null)
                Guid.Serialize(s);
        }

        public string ObjectName
        {
            get
            {
                if (Guid == null)
                    return string.Empty;
                return ResourceManager.GetlName(Guid);
            }
        }

        public override int Size
        {
            get
            {
                int b = 3;
                if (Guid != null)
                    b += Guid.Size;
                return b;
            }
        }

        public override string ToString()
        {
            return ObjectName;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Editor(typeof(gCScriptProxyAIState_TypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class gCScriptProxyAIState : BinaryFileBlock
    {
        class gCScriptProxyAIState_TypeEditor : System.Drawing.Design.UITypeEditor
        {
            static ComboBox dispBox;

            static gCScriptProxyAIState_TypeEditor()
            {
                dispBox = new ComboBox();
                dispBox.DropDownStyle = ComboBoxStyle.DropDownList;
                string[] A = ResourceManager.AIStates;
                foreach (string s in A)
                    dispBox.Items.Add(s);
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                ILrentObject O = (context.Instance as PropertySetWrapper).Object;
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc != null)
                {
                    dispBox.SelectedIndex = dispBox.Items.IndexOf(value);
                    edSvc.DropDownControl(dispBox);
                    if(dispBox.SelectedItem == null)
                        return new gCScriptProxyAIState("Empty");
                    return new gCScriptProxyAIState(dispBox.SelectedItem.ToString());
                }
                return "Empty";
            }

            public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
            }

            public override bool GetPaintValueSupported(ITypeDescriptorContext context)
            {
                return false;
            }
        }

        bCString data;
        short version;

        public gCScriptProxyAIState(IFile bs)
        {
            deSerialize(bs);
        }

        public gCScriptProxyAIState(string s)
        {
            data = new bCString(s);
            version = 1;
        }

        private gCScriptProxyAIState()
        {

        }

        public override BinaryFileBlock Clone()
        {
            gCScriptProxyAIState q = new gCScriptProxyAIState();
            q.data = (bCString)data.Clone();
            q.version = version;
            return q;
        }

        public override void deSerialize(IFile a_File)
        {
            version = a_File.Read<short>();
            byte b = a_File.Read<byte>();
            if (b != 0)
                data = new bCString(a_File);
            else data = null;
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(version);
            if (data != null && data.pString != "Emtpty")
            {
                s.Write<byte>(1);
                data.Serialize(s);
            }
            else s.Write<byte>(0);
        }

        public override int Size
        {
            get
            {
                if (data == null)
                    return 3;
                else return 3 + data.Size;
            }
        }

        public string Object
        {
            get
            {
                return data.pString;
            }

            set
            {
                data.pString = value;
            }
        }

        public override string ToString()
        {
            if (data == null) return "Emtpty";
            return data.pString;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Editor(typeof(gCScriptProxyAIFunction_TypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class gCScriptProxyAIFunction : BinaryFileBlock
    {
        class gCScriptProxyAIFunction_TypeEditor : System.Drawing.Design.UITypeEditor
        {
            static ComboBox dispBox;

            static gCScriptProxyAIFunction_TypeEditor()
            {
                dispBox = new ComboBox();
                dispBox.DropDownStyle = ComboBoxStyle.DropDownList;
                string[] A = ResourceManager.AIFunctions;
                foreach (string s in A)
                    dispBox.Items.Add(s);
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                ILrentObject O = (context.Instance as PropertySetWrapper).Object;
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc != null)
                {
                    dispBox.SelectedIndex = dispBox.Items.IndexOf(value);
                    edSvc.DropDownControl(dispBox);
                    if(dispBox.SelectedItem == null)
                        return new gCScriptProxyAIFunction("Empty");
                    return new gCScriptProxyAIFunction(dispBox.SelectedItem.ToString());
                }
                return "Empty";
            }

            public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
            }

            public override bool GetPaintValueSupported(ITypeDescriptorContext context)
            {
                return false;
            }
        }

        bCString data;
        short version;

        public gCScriptProxyAIFunction(IFile bs)
        {
            deSerialize(bs);
        }

        public gCScriptProxyAIFunction(string s)
        {
            data = new bCString(s);
            version = 1;
        }

        private gCScriptProxyAIFunction()
        {

        }

        public override BinaryFileBlock Clone()
        {
            gCScriptProxyAIFunction q = new gCScriptProxyAIFunction();
            q.data = (bCString)data.Clone();
            q.version = version;
            return q;
        }

        public override void deSerialize(IFile a_File)
        {
            version = a_File.Read<short>();
            byte b = a_File.Read<byte>();
            if (b != 0)
                data = new bCString(a_File);
            else data = null;
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(version);
            if (data != null && data.pString != "Emtpty")
            {
                s.Write<byte>(1);
                data.Serialize(s);
            }
            else s.Write<byte>(0);
        }

        public override int Size
        {
            get
            {
                if (data == null)
                    return 3;
                else return 3 + data.Size;
            }
        }

        public string Object
        {
            get
            {
                return data.pString;
            }

            set
            {
                data.pString = value;
            }
        }

        public override string ToString()
        {
            if (data == null) return "Emtpty";
            return data.pString;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class eCEntityStringProxy : classData
    {
        short version;
        bCString data;

        public eCEntityStringProxy(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCEntityStringProxy(string Data)
        {
            version = 1;
            if(Data != null)
                data = new bCString(Data);
        }

        public eCEntityStringProxy()
        {
            version = 1;
            data = null;
        }

        public override void deSerialize(IFile bs)
        {
            version = bs.Read<short>();
            byte b = bs.Read<byte>();
            if (b != 0)
                data = new bCString(bs);
            else data = null;
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(version);
            if (data != null && data.pString != "Empty")
            {
                s.Write<byte>(1);
                data.Serialize(s);
            }
            else s.Write<byte>(0);
        }

        public override int Size
        {
            get
            {
                if (data == null)
                    return 3;
                else return 3 + data.Size;
            }
        }

        public string Object
        {
            get
            {
                return data == null ? "" : data.pString;
            }

            set
            {
                data = value == null ? null : new bCString(value);
            }
        }
    }
    #endregion

    #region STRINGS
    [TypeConverter(typeof(bCString_TypeConverter))]
    public class bCString : BinaryFileBlock
    {
        public class bCString_TypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                {
                    return new bCString(value as string);
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return (value as bCString).pString;
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        public static bool IsArchiveFile = true;
        string String;

        public bCString(IFile s)
        {
            String = s.ReadString();
        }

        public bCString(string s)
        {
            String = s;
        }

        public bCString()
        {
            String = string.Empty;
        }

        public override BinaryFileBlock Clone()
        {
            return new bCString(String);
        }

        public virtual string pString
        {
            get
            {
                return String;
            }

            set
            {
                if (value == null)
                    throw new Exception();
                String = value;
            }
        }

        public override void deSerialize(IFile a_File)
        {
            String = a_File.ReadString();
        }

        public override void Serialize(IFile s)
        {
            s.WriteString(String);
        }

        public override int Size
        {
            get
            {
                if (IsArchiveFile) return 2;
                else return 2 + String.Length;
            }
        }

        public override string ToString()
        {
            return pString;
        }

        public static implicit operator string(bCString d)
        {
            return d.pString;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is bCString)
                return pString == (obj as bCString).pString;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(bCString a, bCString b)
        {
            bool a0 = (object)a == null, b0 = (object)b == null;
            if (a0 ^ b0)
                return false;
            else if (a0 && b0)
                return true;
            else return a.Equals(b);
        }

        public static bool operator !=(bCString a, bCString b)
        {
            return !(a == b);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCImageOrMaterialResourceString : bCString
    {
        public bCImageOrMaterialResourceString(IFile s)
            : base(s)
        {
        }

        public bCImageOrMaterialResourceString(string s)
            : base(s)
        {

        }

        public override string pString
        {
            get
            {
                string s = base.pString;
                s = s.Replace("xshmat", "._xmat");
                if (s.EndsWith("._xmat")) return s;
                while (s.Length > 0 && !char.IsLetter(s[0]))
                    s = s.Remove(0, 1);
                s = s.Replace(".tga", "._ximg").Replace(".dds", "._ximg");
                if (!s.Contains("._ximg"))
                    s = s + "._ximg";
                return s;
            }
            set
            {
                base.pString = value;
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class eCLocString : bCString
    {
        public eCLocString(IFile s)
            : base(s)
        {
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCImageResourceString : bCString
    {
        public bCImageResourceString(IFile s)
            : base(s)
        {

        }

        public bCImageResourceString(string s)
            : base(s)
        {
        }

        public override string pString
        {
            get
            {
                string s = base.pString;
                s = s.Replace("xshmat", "._xmat");
                if (s.EndsWith("._xmat")) return s;
                while (s.Length > 0 && !char.IsLetter(s[0]))
                    s = s.Remove(0, 1);
                s = s.Replace(".tga", "._ximg").Replace(".dds", "._ximg");
                if (!s.Contains("._ximg"))
                    s = s + "._ximg";
                return s;
            }
            set
            {
                base.pString = value;
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class gCLetterLocString : bCString
    {
        public gCLetterLocString(IFile s)
            : base(s)
        {
        }

        public override string ToString()
        {
            return base.ToString() + " | Ref : " + RefString;
        }

        public string RefString
        {
            get
            {
                return FileManager.GetString("", pString, 1);
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class gCBookLocString : bCString
    {
        public gCBookLocString(IFile s)
            : base(s)
        {
        }

        public override string ToString()
        {
            return base.ToString() + " | Ref : " + RefString;
        }

        public string RefString
        {
            get
            {
                return FileManager.GetString("", pString, 1);
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCSpeedTreeResourceString : bCString
    {
        public bCSpeedTreeResourceString(IFile s)
            : base(s)
        {
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCMeshResourceString : bCString
    {
        public bCMeshResourceString(IFile s)
            : base(s)
        {
        }
    }

    [TypeConverter(typeof(gCInfoLocString_TypeConverter))]
    public class gCInfoLocString : bCString
    {
        public class gCInfoLocString_TypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                {
                    string a = value as string;
                    int b = a.IndexOf("}");
                    if (b == -1)
                        return base.ConvertFrom(context, culture, value);
                    string k = a.Substring(1, b - 1), g = a.Substring(b + 1, a.Length - b - 1);
                    gCInfoLocString r = new gCInfoLocString();
                    r.pString = k;
                    r.German = g;
                    return r;
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    return "{" + (value as gCInfoLocString).pString + "}" + (value as gCInfoLocString).German;
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        public static string getprefix(string s)
        {
            if (s.Length == 0)
                return "";
            int a = s.IndexOf('_');
            return s.Substring(0, a);
        }

        public gCInfoLocString(IFile s)
            : base(s)
        {
        }

        public gCInfoLocString()
        {
        }

        public string German
        {
            get
            {
                return FileManager.GetString(getprefix(pString), pString, 1);
            }
            set
            {
                FileManager.SetString(getprefix(pString), this.pString, 1, value);
            }
        }

        public override string pString
        {
            get
            {
                return base.pString;
            }
            set
            {
                base.pString = value;
                string[] old = new string[0];
                tabFile T = FileManager.mapTab(getprefix(pString));
                if (T != null)
                    old = T.getString(pString);
                if (T != null && !T.ContainsKey(value))
                    T.addString(value, old);
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class gCQuestLocString : bCString
    {
        public gCQuestLocString(IFile s)
            : base(s)
        {
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class eCTipLocString : bCString
    {
        public eCTipLocString(IFile s)
            : base(s)
        {
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class gCNPCInfoLocString : bCString
    {
        public gCNPCInfoLocString(IFile s)
            : base(s)
        {
        }
        public gCNPCInfoLocString()
        {
        }
    }

    [TypeConverter(typeof(bCUnicodeString_TypeConverter))]
    public class bCUnicodeString : BinaryFileBlock
    {
        public class bCUnicodeString_TypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                {
                    return new bCUnicodeString(value as string);
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return (value as bCUnicodeString).data;
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        string data;

        public bCUnicodeString(IFile s)
        {
            deSerialize(s);
        }

        public bCUnicodeString(string s)
        {
            data = s;
        }

        public bCUnicodeString()
        {
            data = string.Empty;
        }

        public override BinaryFileBlock Clone()
        {
            return new bCUnicodeString(data);
        }

        public override void deSerialize(IFile a_File)
        {
            short l = a_File.Read<short>();
            data = new string(Encoding.Unicode.GetChars(a_File.Read<byte>(l)));
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>((short)(data.Length * 2));
            a_File.Write<byte>(Encoding.Unicode.GetBytes(data.ToCharArray()));
        }

        public override int Size
        {
            get { return 2 + data.Length * 2; }
        }
    }
    #endregion

    #region ARRAYS
    public abstract class bCArrayBase<T> : BinaryFileBlock, IEnumerable<T>
    {
        protected List<T> nativeList;

        public T this[int index]
        {
            get
            {
                return nativeList[index];
            }

            set
            {
                nativeList[index] = value;
            }
        }

        public void Add(T t)
        {
            nativeList.Add(t);
        }

        public void AddRange(System.Collections.Generic.IEnumerable<T> t)
        {
            nativeList.AddRange(t);
        }

        public void Insert(int index, T t)
        {
            nativeList.Insert(index, t);
        }

        public void Remove(T t)
        {
            nativeList.Remove(t);
        }

        public void RemoveAt(int index)
        {
            nativeList.RemoveAt(index);
        }

        public void RemoveRange(int index, int count)
        {
            nativeList.RemoveRange(index, count);
        }

        public int Length
        {
            get
            {
                return nativeList.Count;
            }
        }

        public void Clear()
        {
            nativeList.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return nativeList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return nativeList.GetEnumerator();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bTObjArray<T> : bCArrayBase<T>
    {
        string nT;

        public bTObjArray(IFile S, string nativeType)
        {
            nT = nativeType;
            deSerialize(S);
        }

        public override void deSerialize(IFile a_File)
        {
            if (a_File.Length == 0)
            {
                nativeList = new List<T>();
                return;
            }
            byte b = a_File.Read<byte>();
            if (b == 0)
                return;
            nativeList = new List<T>(a_File.Read<int>());
            for (int i = 0; i < nativeList.Capacity; i++)
            {
                T t = (T)bCProperty.getObject(nT, a_File, false);
                nativeList.Add(t);
            }
        }

        public override void Serialize(IFile s)
        {
            byte b = Convert.ToByte(nativeList != null);
            s.Write<byte>(b);
            if (b == 0)
                return;
            s.Write<int>(nativeList.Count);
            for (int i = 0; i < nativeList.Count; i++)
                bCProperty.setObject(nT, nativeList[i], s, false);
        }

        public override int Size
        {
            get 
            {
                int q = 5;
                for (int i = 0; i < nativeList.Count; i++)
                    q += bCProperty.getObjectSize(nT, nativeList[i], false);
                return q;
            }
        }

        public override BinaryFileBlock Clone()
        {
            IFile F = new TempStream(Size, eCArchiveFile.CreateBuffer());
            Serialize(F);
            return new bTObjArray<T>(F, nT);
        }
    }
                
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bTValArray<T> : bCArrayBase<T> where T : struct
    {
        public bTValArray(IFile S)
        {
            deSerialize(S);
        }

        private bTValArray() { }

        public override void deSerialize(IFile a_File)
        {
            if (a_File.Length == 0)
            {
                nativeList = new List<T>();
                return;
            }
            byte b = a_File.Read<byte>();
            if (b == 0)
                return;
            nativeList = new List<T>(a_File.Read<int>());
            for (int i = 0; i < nativeList.Capacity; i++)
            {
                T t = a_File.Read<T>();
                nativeList.Add(t);
            }
        }

        public override void Serialize(IFile s)
        {
            byte b = Convert.ToByte(nativeList != null);
            s.Write<byte>(b);
            if (b == 0)
                return;
            s.Write<int>(nativeList.Count);
            for (int i = 0; i < nativeList.Count; i++)
                s.Write<T>(nativeList[i]);         
        }

        public override int Size
        {
            get
            {
                int q = 5;
                for (int i = 0; i < nativeList.Count; i++)
                    q += Marshal.SizeOf(nativeList[i]);
                return q;
            }
        }

        public override BinaryFileBlock Clone()
        {
            bTValArray<T> v = new bTValArray<T>();
            v.nativeList = new List<T>();
            foreach (T t in nativeList)
                v.nativeList.Add(t);
            return v;
        }
    }
    /* Useless atm
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bTRefPropertyArray<T> : bTObjArray<T>
    {
        public bTRefPropertyArray(BinaryStream S, g_StringTable T, string nativeType)
            : base(S, T, nativeType)
        {

        }
    }
    */
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bTRefPtrArray<T> : bCArrayBase<T> where T : BinaryFileBlock
    {
        public bTRefPtrArray(IFile S)
        {
            deSerialize(S);
        }

        private bTRefPtrArray() { }

        public override void deSerialize(IFile a_File)
        {
            if (a_File.Length == 0)
            {
                nativeList = new List<T>();
                return;
            }
            byte b = a_File.Read<byte>();
            if (b == 0)
                return;
            nativeList = new List<T>(a_File.Read<int>());
            for (int i = 0; i < nativeList.Capacity; i++)
            {
                T t = (T)Activator.CreateInstance(typeof(T), new object[] { a_File });
                nativeList.Add(t);
            }
        }

        public override void Serialize(IFile s)
        {
            byte b = Convert.ToByte(nativeList != null);
            s.Write<byte>(b);
            if (b == 0)
                return;
            s.Write<int>(nativeList.Count);
            for (int i = 0; i < nativeList.Count; i++)
                nativeList[i].Serialize(s);
        }

        public override int Size
        {
            get 
            {
                int q = 5;
                for (int i = 0; i < nativeList.Count; i++)
                    q += nativeList[i].Size;
                return q;   
            }
        }

        public override BinaryFileBlock Clone()
        {
            bTRefPtrArray<T> e = new bTRefPtrArray<T>();
            e.nativeList = new List<T>();
            foreach (T t in nativeList)
                e.Add((T)t.Clone());
            return e;
        }
    }
    #endregion

    #region SPECIAL
    public class gCFlightPathBallistic : bCObjectBase
    {
        public gCFlightPathBallistic(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCFlightPathBallistic()
        {
            Properties.addProperty("VelocityLoss", "float", 0.0f);
            Properties.addProperty("DeactivationVelocity", "float", 0.01f);
            Properties.addProperty("Gravity", "float", 9.81f);
        }
    }

    public class gCFlightPathSeeking : bCObjectBase
    {
        public gCFlightPathSeeking(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCFlightPathSeeking()
        {
            Properties.addProperty("TargetEntity", new eCEntityProxy(null as bCPropertyID));
        }
    }

    public class gCSkillValue : bCObjectBase
    {
        public gCSkillValue()
        {
            Properties.addProperty("Amount", "long", 1);
            Properties.addProperty("Skill", "gESkill", gESkill.gESkill_Stat_XP);
        }

        public gCSkillValue(int val, gESkill s)
        {
            Amount = val;
            Skill = s;
        }

        public gCSkillValue(IFile S)
        {
            deSerialize(S);
        }

        public override string ToString()
        {
            return Skill.ToString() + " " + Amount; 
        }

        public int Amount
        {
            get
            {
                return (int)Properties["Amount"].Object;
            }
            set
            {
                Properties["Amount"].Object = value;
            }
        }

        public gESkill Skill
        {
            get
            {
                return (gESkill)Properties["Skill"].Object;
            }
            set
            {
                Properties["Skill"].Object = value;
            }
        }
    }

    public class gCModifySkill : bCObjectBase
    {
        public gCModifySkill()
        {
            Properties.addProperty("Modifier", "gESkillModifier", gESkillModifier.gESkillModifier_AddValue);
            Properties.addProperty("Amount", "long", 1);
            Properties.addProperty("Skill", "gESkill", gESkill.gESkill_Atrib_HP);
        }

        public gCModifySkill(IFile S)
        {
            deSerialize(S);
        }

        public override string ToString()
        {
            return Modifier.ToString() + " " + Skill.ToString() + " " + Amount.ToString();
        }

        public gESkillModifier Modifier
        {
            get
            {
                return (gESkillModifier)Properties["Modifier"].Object;
            }
            set
            {
                Properties["Modifier"].Object = value;
            }
        }

        public int Amount
        {
            get
            {
                return (int)Properties["Amount"].Object;
            }
            set
            {
                Properties["Amount"].Object = value;
            }
        }

        public gESkill Skill
        {
            get
            {
                return (gESkill)Properties["Skill"].Object;
            }
            set
            {
                Properties["Skill"].Object = value;
            }
        }
    }

    public class gCCraftIngredient : bCObjectBase
    {
        public gCCraftIngredient()
        {
            Properties.addProperty("ItemTemplate", "eCTemplateEntityProxy", new eCTemplateEntityProxy());
            Properties.addProperty("ItemAmount", "long", 1);
        }

        public gCCraftIngredient(IFile S)
        {
            deSerialize(S);
        }

        public override string ToString()
        {
            return "Item : " + ItemTemplate.ObjectName + " | Amount : " + ItemAmount.ToString();
        }

        public eCTemplateEntityProxy ItemTemplate
        {
            get
            {
                return (eCTemplateEntityProxy)Properties["ItemTemplate"].Object;
            }
            set
            {
                Properties["ItemTemplate"].Object = value;
            }
        }

        public int ItemAmount
        {
            get
            {
                return (int)Properties["ItemAmount"].Object;
            }
            set
            {
                Properties["ItemAmount"].Object = value;
            }
        }
    }

    public class bCMotion : BinaryFileBlock
    {
        public Vector3 pos;
        public Quaternion rot;

        public bCMotion(IFile S)
        {
            deSerialize(S);
        }

        public bCMotion(Vector3 p, Quaternion q)
        {
            pos = p;
            rot = q;
        }

        public override BinaryFileBlock Clone()
        {
            return new bCMotion(pos, rot);
        }

        public override void deSerialize(IFile a_File)
        {
            pos = a_File.Read<Vector3>();
            rot = a_File.Read<Quaternion>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<Vector3>(pos);
            s.Write<Quaternion>(rot);
        }

        public override int Size
        {
            get { return 28; }
        }

        public override string ToString()
        {
            return "Position : " + pos.ToString() + " | Rotation : " + rot.ToString();
        }
    }

    public class gCInteractionSlot : bCObjectBase
    {
        public gCInteractionSlot(IFile S)
        {
            deSerialize(S);
        }

        public gCInteractionSlot()
        {

        }

        public Matrix Offset
        {
            get
            {
                return (Matrix)Properties["Offset"].Object;
            }
            set
            {
                Properties["Offset"].Object = value;
            }
        }

        public eCEntityProxy Item
        {
            get
            {
                return (eCEntityProxy)Properties["Item"].Object;
            }
            set
            {
                Properties["Item"].Object = value;
            }
        }

        public override string ToString()
        {
            return "Item : " + Item.ToString(); 
        }
    }

    public class gCSkillRange : bCObjectBase
    {
        public gCSkillRange()
        {
            Properties.addProperty("MinValue", "long", 0);
            Properties.addProperty("MaxValue", "long", 1);
            Properties.addProperty("Skill", "gESkill", gESkill.gESkill_Atrib_HP);
        }

        public gCSkillRange(IFile S)
        {
            deSerialize(S);
        }

        public int MinValue
        {
            get
            {
                return (int)Properties["MinValue"].Object;
            }
            set
            {
                Properties["MinValue"].Object = value;
            }
        }

        public int MaxValue
        {
            get
            {
                return (int)Properties["MaxValue"].Object;
            }
            set
            {
                Properties["MaxValue"].Object = value;
            }
        }

        public gESkill Skill
        {
            get
            {
                return (gESkill)Properties["Skill"].Object;
            }
            set
            {
                Properties["Skill"].Object = value;
            }
        }

        public override string ToString()
        {
            return Skill.ToString();
        }
    }

    public class gCWaterSubZone_PS : BinaryFileBlock
    {
        short Version;
        Vector3 v0;
        bTValArray<Vector3> B0;
        float f0;
        Vector3 v1;
        Vector4 v2;

        public gCWaterSubZone_PS(IFile S)
        {
            deSerialize(S);
        }

        private gCWaterSubZone_PS() { }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
            if (Version >= 2)
            {
                v0 = a_File.Read<Vector3>();
                B0 = new bTValArray<Vector3>(a_File);
                f0 = a_File.Read<float>();
                v1 = a_File.Read<Vector3>();
                v2 = a_File.Read<Vector4>();
            }
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            if (Version >= 2)
            {
                s.Write<Vector3>(v0);
                B0.Serialize(s);
                s.Write<float>(f0);
                s.Write<Vector3>(v1);
                s.Write<Vector4>(v2);
            }
        }

        public override int Size
        {
            get
            {
                if (Version < 2)
                    return 2;
                else return 2 + 12 + B0.Size + 4 + 12 + 16;
            }
        }

        public override BinaryFileBlock Clone()
        {
            gCWaterSubZone_PS p = new gCWaterSubZone_PS();
            p.Version = Version;
            if (Version >= 2)
            {
                p.v0 = v0;
                p.v1 = v1;
                p.v2 = v2;
                p.f0 = f0;
                p.B0 = (bTValArray<Vector3>)B0.Clone();
            }
            return p;
        }
    }

    public class gCInteraction : bCObjectRefBase
    {
        public gCInteraction(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCInteraction()
        {

        }
    }

    public class eCMoverAnimationSkeletal : bCObjectRefBase
    {
        public eCMoverAnimationSkeletal(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCMoverAnimationSkeletal()
        {

        }
    }

    public class eCMoverAnimationLinear : bCObjectRefBase
    {
        public eCMoverAnimationLinear(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCMoverAnimationLinear()
        {

        }
    }

    public class eCMoverAnimationBezier : bCObjectRefBase
    {
        public eCMoverAnimationBezier(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCMoverAnimationBezier()
        {

        }
    }

    public class eCMoverAnimationFile : bCObjectRefBase
    {
        public eCMoverAnimationFile(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCMoverAnimationFile()
        {

        }
    }

    public class gCMapEntry : bCObjectBase
    {
        public gCMapEntry(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCMapEntry()
        {

        }
    }

    public class eCGuiCursor2 : bCObjectRefBase
    {
        public eCGuiCursor2(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCGuiCursor2()
        {

        }
    }

    public class gCEffectCommand : bCObjectRefBase
    {
        public gCEffectCommand(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCEffectCommand()
        {

        }
    }

    public class gCEffectCommandSequence2 : bCObjectRefBase
    {
        public gCEffectCommandSequence2(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCEffectCommandSequence2()
        {

        }
    }

    public class eCAudioRollOffPreset : bCObjectRefBase
    {
        public eCAudioRollOffPreset(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCAudioRollOffPreset()
        {

        }
    }

    public class eCAudioReverbPreset : bCObjectRefBase
    {
        public eCAudioReverbPreset(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCAudioReverbPreset()
        {

        }
    }

    public class gCCameraTypeConstSolver : bCObjectRefBase
    {
        public gCCameraTypeConstSolver(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCameraTypeConstSolver()
        {

        }
    }

    public class gCCameraEffectSeismicArray : bCObjectRefBase
    {
        public gCCameraEffectSeismicArray(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCCameraEffectSeismicArray()
        {

        }
    }

    public class eCFixedGuiBitmapFont2 : bCObjectRefBase
    {
        public eCFixedGuiBitmapFont2(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCFixedGuiBitmapFont2()
        {

        }
    }

    public class eCGuiSystemFontLanguage2 : bCObjectBase
    {
        public eCGuiSystemFontLanguage2(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCGuiSystemFontLanguage2()
        {

        }
    }

    public class eCGuiSystemFont2 : bCObjectRefBase
    {
        public eCGuiSystemFont2(IFile a_File)
        {
            deSerialize(a_File);
        }

        public eCGuiSystemFont2()
        {

        }
    }

    public class gCMusicLocation2 : bCObjectRefBase
    {
        public gCMusicLocation2(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCMusicLocation2()
        {

        }
    }

    public class gCMusicTransition2 : bCObjectRefBase
    {
        public gCMusicTransition2(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCMusicTransition2()
        {

        }
    }

    public class gCTutorialEvent : bCObjectRefBase
    {
        public gCTutorialEvent(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCTutorialEvent()
        {

        }
    }

    public class gCTutorialHint : bCObjectBase
    {
        public gCTutorialHint(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCTutorialHint()
        {

        }
    }

    public class gCTutorialHintCollection : bCObjectRefBase
    {
        public gCTutorialHintCollection(IFile a_File)
        {
            deSerialize(a_File);
        }

        public gCTutorialHintCollection()
        {

        }
    }

    public class eCImageEffect : classData
    {
        struct int4
        {
            public int i0, i1, i2, i3;
            public int4(int i)
            {
                i0 = i1 = i2 = i3 = i;
            }
        }
        short Version;
        bTValArray<int4> data0;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
            data0 = new bTValArray<int4>(a_File);
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(Version);
            data0.Serialize(a_File);
        }

        public override int Size
        {
            get { return 2 + data0.Size; }
        }
    }
    #endregion

    [TypeConverter(typeof(bCFloatColor_TypeConverter))]
    public class bCFloatColor : BinaryFileBlock
    {
        public class bCFloatColor_TypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(System.Drawing.Color))
                    return true;
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is System.Drawing.Color)
                {
                    return new bCFloatColor((System.Drawing.Color)value);
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(System.Drawing.Color))
                    return (System.Drawing.Color)(value as bCFloatColor);
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        protected int alignment;
        protected Color3 Color { get; set; }

        public bCFloatColor(Color3 c)
        {
            Color = c;
        }

        public bCFloatColor(System.Drawing.Color c)
        {
            Color = new Color3((float)c.R / 255.0f, (float)c.G / 255.0f, (float)c.B / 255.0f);
        }

        public bCFloatColor(IFile s)
        {
            deSerialize(s);
        }

        public override void deSerialize(IFile a_File)
        {
            alignment = a_File.Read<int>();
            Color = a_File.Read<Color3>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<int>(alignment);
            s.Write<Color3>(Color);
        }

        public override int Size
        {
            get { return 16; }
        }

        public override string ToString()
        {
            return Color.ToString();
        }

        public override BinaryFileBlock Clone()
        {
            return new bCFloatColor(Color);
        }

        public static implicit operator Color3(bCFloatColor d)
        {
            return d.Color;
        }

        public static implicit operator Vector3(bCFloatColor d)
        {
            return new Vector3(d.Color.Red, d.Color.Green, d.Color.Blue);
        }

        public static implicit operator System.Drawing.Color(bCFloatColor d)
        {
            return System.Drawing.Color.FromArgb((int)(d.Color.Red * 255.0f), (int)(d.Color.Green * 255.0f), (int)(d.Color.Blue * 255.0f));
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCFloatAlphaColor : bCFloatColor
    {
        public float Alpha { get; set; }

        public bCFloatAlphaColor(Color3 c, float a)
            : base(c)
        {
            Alpha = a;
        }

        public bCFloatAlphaColor(IFile s)
            : base(s)
        {
            Alpha = s.Read<float>();
        }

        public override void Serialize(IFile s)
        {
            base.Serialize(s);
            s.Write<float>(Alpha);
        }

        public override int Size
        {
            get { return 20; }
        }

        public override string ToString()
        {
            return base.Color.ToString() + " Alpha : " + Alpha.ToString();
        }

        public new object Clone()
        {
            return new bCFloatAlphaColor(Color, Alpha);
        }

        public static implicit operator Color3(bCFloatAlphaColor d)
        {
            return d.Color;
        }

        public static implicit operator Color4(bCFloatAlphaColor d)
        {
            Color4 q = new Color4((Color3)d);
            q.Alpha = d.Alpha;
            return q;
        }

        public static implicit operator Vector4(bCFloatAlphaColor d)
        {
            return new Vector4(d.Color.Red, d.Color.Green, d.Color.Blue, d.Alpha);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCByteColor : BinaryFileBlock
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public bCByteColor(Color3 C)
        {
            Color = new Color4(C).ToColor();
        }

        public bCByteColor(System.Drawing.Color C)
        {
            Color = C;
        }

        public bCByteColor(IFile a_File)
        {
            deSerialize(a_File);
        }

        public override BinaryFileBlock Clone()
        {
            return new bCByteColor(Color);
        }

        public override void deSerialize(IFile a_File)
        {
            Red = a_File.Read<byte>();
            Green = a_File.Read<byte>();
            Blue = a_File.Read<byte>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write<byte>(Red);
            a_File.Write<byte>(Green);
            a_File.Write<byte>(Blue);
        }

        public override int Size
        {
            get { return 3; }
        }

        public virtual System.Drawing.Color Color
        {
            get
            {
                return System.Drawing.Color.FromArgb(Red, Green, Blue);
            }
            set
            {
                Red = value.R;
                Green = value.G;
                Blue = value.B;
            }
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class bCByteAlphaColor : bCByteColor
    {
        public byte Alpha { get; set; }

        public bCByteAlphaColor(Color4 C)
            : base(C.ToColor3())
        {
        }

        public bCByteAlphaColor(System.Drawing.Color C)
            : base(C)
        {
        }

        public bCByteAlphaColor(IFile a_File)
            : base(a_File)
        {
            Alpha = a_File.Read<byte>();
        }

        public override int Size
        {
            get
            {
                return 4;
            }
        }

        public override void Serialize(IFile a_File)
        {
            base.Serialize(a_File);
            a_File.Write<byte>(Alpha);
        }

        public override BinaryFileBlock Clone()
        {
            return new bCByteAlphaColor(Color);
        }

        public override System.Drawing.Color Color
        {
            get
            {
                return System.Drawing.Color.FromArgb(Alpha, Red, Green, Blue);
            }
            set
            {
                Red = value.R;
                Green = value.G;
                Blue = value.B;
                Alpha = value.A;
            }
        }
    }

    public class bTInterpolatorLinear<T, U> : BinaryFileBlock
    {
        BUFFER B;

        public bTInterpolatorLinear(IFile S, string[] trueNames)
        {/*
            byte enabled = S.Read<byte>();
            int i0 = S.Read<int>();
            if (enabled != 0 && i0 != 0)
            {
                
                int i1 = S.Read<int>();
                (i0 + i1).ToString();
                o0 = bCProperty.getObject(trueNames[0], S, false);
                byte enabled2 = S.Read<byte>();
                int i2 = S.Read<int>();
                if (enabled2 != 0 && i2 != 0)
                {
                    o1 = bCProperty.getObject(trueNames[1], S, false);
                }
            }
            */
            
            B = new BUFFER(S);
        }

        private bTInterpolatorLinear() { }

        public override void Serialize(IFile s)
        {
            B.Serialize(s);
        }

        public override void deSerialize(IFile a_File)
        {
            throw new NotImplementedException();
        }

        public override int Size
        {
            get { return B.Size; }
        }

        public override BinaryFileBlock Clone()
        {
            bTInterpolatorLinear<T, U> b = new bTInterpolatorLinear<T, U>();
            b.B = (BUFFER)B.Clone();
            return B;
        }
    }

    public class bTInterpolatorBezier<T, U> : BinaryFileBlock
    {
        BUFFER B;

        public bTInterpolatorBezier(IFile S, string[] trueNames)
        {
            B = new BUFFER(S);
        }

        private bTInterpolatorBezier() { }

        public override void deSerialize(IFile a_File)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(IFile s)
        {
            B.Serialize(s);
        }

        public override int Size
        {
            get { return B.Size; }
        }

        public override BinaryFileBlock Clone()
        {
            bTInterpolatorBezier<T, U> b = new bTInterpolatorBezier<T, U>();
            b.B = (BUFFER)B.Clone();
            return B;
        }
    }

    public class bSKeyFrameLinearEx<T> : BinaryFileBlock
    {
        public bSKeyFrameLinearEx(IFile a_File)
        {
            1.ToString();
        }

        public override void deSerialize(IFile a_File)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(IFile s)
        {
            throw new NotImplementedException();
        }

        public override int Size
        {
            get { throw new NotImplementedException(); }
        }

        public override BinaryFileBlock Clone()
        {
            throw new NotImplementedException();
        }
    }

    public class bSKeyFrameBezier<T> : BinaryFileBlock
    {
        public bSKeyFrameBezier(IFile a_File)
        {

        }

        public override void deSerialize(IFile a_File)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(IFile s)
        {
            throw new NotImplementedException();
        }

        public override int Size
        {
            get { throw new NotImplementedException(); }
        }

        public override BinaryFileBlock Clone()
        {
            throw new NotImplementedException();
        }
    }

    public class gSRoutine : BinaryFileBlock
    {
        public bCString m_strName;
        public eCEntityProxy m_WorkingPoint;
        public eCEntityProxy m_RelaxingPoint;
        public eCEntityProxy m_SleepingPoint;

        static gSRoutine dummyRoutine;

        static gSRoutine()
        {
            dummyRoutine = new gSRoutine(1);
            dummyRoutine.m_strName = new bCString("Start");
            dummyRoutine.m_WorkingPoint = new eCEntityProxy(new bCPropertyID(new Guid("{1fc6626d-9de6-4d0f-a4b7-d1f8c9290a7b}")));
            dummyRoutine.m_RelaxingPoint = new eCEntityProxy(new bCPropertyID(new Guid("{55bc0eef-b519-4f56-9168-63d1c2f652db}")));
            dummyRoutine.m_SleepingPoint = new eCEntityProxy(new bCPropertyID(new Guid("{f115f5ae-de71-4cc6-82c1-e5fe41729569}")));
        }

        public gSRoutine(IFile S)
        {
            deSerialize(S);
        }

        public gSRoutine()
        {
            m_strName = (bCString)dummyRoutine.m_strName.Clone();
            m_WorkingPoint = (eCEntityProxy)dummyRoutine.m_WorkingPoint.Clone();
            m_RelaxingPoint = (eCEntityProxy)dummyRoutine.m_RelaxingPoint.Clone();
            m_SleepingPoint = (eCEntityProxy)dummyRoutine.m_SleepingPoint.Clone();
        }

        private gSRoutine(int i) { }

        public override BinaryFileBlock Clone()
        {
            gSRoutine e = new gSRoutine(-1);
            e.m_strName = (bCString)m_strName.Clone();
            e.m_WorkingPoint = (eCEntityProxy)m_WorkingPoint.Clone();
            e.m_RelaxingPoint = (eCEntityProxy)m_RelaxingPoint.Clone();
            e.m_SleepingPoint = (eCEntityProxy)m_SleepingPoint.Clone();
            return e;
        }

        public override void deSerialize(IFile a_File)
        {
            a_File.Position += 2L;
            m_strName = new bCString(a_File);
            m_WorkingPoint = new eCEntityProxy(a_File);
            m_RelaxingPoint = new eCEntityProxy(a_File);
            m_SleepingPoint = new eCEntityProxy(a_File);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(1);
            m_strName.Serialize(s);
            m_WorkingPoint.Serialize(s);
            m_RelaxingPoint.Serialize(s);
            m_SleepingPoint.Serialize(s);
        }

        public override int Size
        {
            get { return 4 + m_WorkingPoint.Size + m_RelaxingPoint.Size + m_SleepingPoint.Size; }
        }

        public void Clear()
        {
            //e0.ID = e1.ID = e2.ID = new bCPropertyID();
            if (m_WorkingPoint.Entity == null)
                m_WorkingPoint = (eCEntityProxy)dummyRoutine.m_WorkingPoint.Clone();
            if (m_RelaxingPoint.Entity == null)
                m_RelaxingPoint = (eCEntityProxy)dummyRoutine.m_RelaxingPoint.Clone();
            if (m_SleepingPoint.Entity == null)
                m_SleepingPoint = (eCEntityProxy)dummyRoutine.m_SleepingPoint.Clone();
        }

        public void SetFreePoints(ILrentObject a_Point)
        {
            SetFreePoints(a_Point, a_Point, a_Point);
        }

        public void SetFreePoints(ILrentObject a_WorkingPoint, ILrentObject a_RelaxingPoint, ILrentObject a_SleepingPoint)
        {
            if (a_WorkingPoint != null)
                m_WorkingPoint.ID = new bCPropertyID(a_WorkingPoint.GUID);
            else m_WorkingPoint.ID = new bCPropertyID();

            if (a_RelaxingPoint != null)
                m_RelaxingPoint.ID = new bCPropertyID(a_RelaxingPoint.GUID);
            else m_RelaxingPoint.ID = new bCPropertyID();

            if (a_SleepingPoint != null)
                m_SleepingPoint.ID = new bCPropertyID(a_SleepingPoint.GUID);
            else m_SleepingPoint.ID = new bCPropertyID();
        }

        public override string ToString()
        {
            return m_strName.pString;
        }
    }

    public class gSVisitedNavCluster : BinaryFileBlock
    {
        int __FIXME_0000;
        Vector3 __FIXME_0004;

        public gSVisitedNavCluster(IFile S)
        {
            deSerialize(S);
        }

        private gSVisitedNavCluster() { }

        public override BinaryFileBlock Clone()
        {
            gSVisitedNavCluster e = new gSVisitedNavCluster();
            e.__FIXME_0000 = __FIXME_0000;
            e.__FIXME_0004 = __FIXME_0004;
            return e;
        }

        public override void deSerialize(IFile a_File)
        {
            __FIXME_0000 = a_File.Read<int>();
            __FIXME_0004 = a_File.Read<Vector3>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<int>(__FIXME_0000);
            s.Write<Vector3>(__FIXME_0004);
        }

        public override int Size
        {
            get { return 16; }
        }
    }

    public class gSTrailElement : BinaryFileBlock
    {
        bCMotion __FIXME_0000;
        int __FIXME_001C;
        byte __FIXME_0020;

        public gSTrailElement(IFile S)
        {
            deSerialize(S);
        }

        private gSTrailElement() { }

        public override BinaryFileBlock Clone()
        {
            gSTrailElement e = new gSTrailElement();
            e.__FIXME_0000 = (bCMotion)__FIXME_0000.Clone();
            e.__FIXME_001C = __FIXME_001C;
            e.__FIXME_0020 = __FIXME_0020;
            return e;
        }

        public override void deSerialize(IFile a_File)
        {
            __FIXME_0000 = new bCMotion(a_File);
            __FIXME_001C = a_File.Read<int>();
            __FIXME_0020 = a_File.Read<byte>();
        }

        public override void Serialize(IFile s)
        {
            __FIXME_0000.Serialize(s);
            s.Write<int>(__FIXME_001C);
            s.Write<byte>(__FIXME_0020);
        }

        public override int Size
        {
            get { return __FIXME_0000.Size + 5; }
        }
    }
    #endregion
}
