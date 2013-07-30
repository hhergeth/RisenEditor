using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GameLibrary.IO;
using GameLibrary;

namespace RisenEditor.Code.RisenTypes
{
    struct tabheader
    {
        public char4 magic;
        public short version;
        public short format;
        public long dateTime;

        public tabheader(short type)
        {
            magic = new char4("TAB0");
            version = 1;
            format = type;
            dateTime = DateTime.Now.ToFileTime();
        }
    }

    public class tabFile
    {
        tabheader fileheader;
        EFile m_File;
        List<List<string>> data;
        List<string> header;
        bool changed;
        const int m_pKeyIndex = 0;
        byte[] m_pTmp = new byte[4096];

        public tabFile(EFile a, params string[] columns)
        {
            fileheader = new tabheader(1);
            header = new List<string>(columns);
            changed = true;
            data = new List<List<string>>();
            for (int i = 0; i < columns.Length; i++)
                data.Add(new List<string>());
            m_File = a;
        }

        public tabFile(EFile a_File)
        {
            m_File = a_File;
            changed = false;
            Stream MS = a_File.Open(FileAccess.Read);
            fileheader = new BinaryStreamReader(MS).Read<tabheader>();
            uint Columns = readUint(MS);
            header = new List<string>((int)Columns);
            data = new List<List<string>>();
            for (int i = 0; i < Columns; i++)
            {
                byte b = (byte)MS.ReadByte();
                if (b != 0)
                {
                    ushort tmp = readShort(MS);
                    string q = readString(MS);
                    header.Add(q);
                    int rows = (int)readUint(MS);
                    List<string> d = new List<string>(rows);
                    for (int j = 0; j < rows; j++)
                        d.Add(readString(MS));
                    data.Add(d);
                }
            }
            a_File.Close();
        }

        uint readUint(Stream MS)
        {
            byte[] b = new byte[4];
            MS.Read(b, 0, 4);
            return b.GetStructure<uint>();
        }

        ushort readShort(Stream MS)
        {
            byte[] b = new byte[2];
            MS.Read(b, 0, 2);
            return b.GetStructure<ushort>();
        }

        string readString(Stream MS)
        {
            int i = (int)readShort(MS);
            byte[] b = new byte[i * 2];
            MS.Read(b, 0, i * 2);
            return new string(Encoding.Unicode.GetChars(b));
        }

        public bool ContainsKey(string key)
        {
            return data[m_pKeyIndex].IndexOf(key) != -1;
        }

        public string[] getString(string key)
        {
            string[] A = new string[Columns];
            int i = data[m_pKeyIndex].IndexOf(key);
            if(i == -1)
                return new string[0];
            for (int a = 0; a < A.Length; a++)
                A[a] = data[a][i];
            return A;
        }

        public void setString(string key, string s, int index)
        {
            if (index < header.Count)
            {
                int i = data[m_pKeyIndex].IndexOf(key);
                if (i == -1)
                {
                    addString(key);
                    i = data[m_pKeyIndex].Count - 1;
                }
                data[index][i] = s;
            }
            else
            {
                throw new Exception();
            }
            changed = true;
        }

        public void addString(string key, params string[] vals)
        {
            for (int i = 0; i < header.Count; i++)
                data[i].Add(vals.Length > i ? vals[i] : "NOT_SET");
            data[m_pKeyIndex][data[m_pKeyIndex].Count - 1] = key;
            changed = true;
        }

        public void addColumn(string header)
        {
            this.header.Add(header);
            data.Add(GENOMEMath.Repeated<string>("", Rows));
        }

        public void removeString(string key)
        {
            int i = data[m_pKeyIndex].IndexOf(key);
            for (int j = 0; j < header.Count; j++)
                data[j].RemoveAt(i);
            changed = true;
        }

        public void serialize()
        {
            if (!changed)
                return;
            changed = false;
            Stream MS = m_File.Open(FileAccess.Write);
            MS.Position = 0L;
            MS.SetLength(MS.Length);
            byte[] A = fileheader.GetBytes();
            MS.Write(A, 0, A.Length);
            writeUInt((uint)header.Count, MS);
            for (int i = 0; i < header.Count; i++)
            {
                MS.WriteByte(1);
                writeUShort(1, MS);
                writeString(header[i], MS);
                writeUInt((uint)data[i].Count, MS);
                for (int j = 0; j < data[i].Count; j++)
                    writeString(data[i][j], MS);
            }
            m_File.Close();
        }

        void writeUInt(uint i, Stream MS)
        {
            MS.Write(i.GetBytes(), 0, 4);
        }

        void writeUShort(ushort s, Stream MS)
        {
            MS.Write(s.GetBytes(), 0, 2);
        }

        void writeString(string s, Stream MS)
        {
            writeUShort((ushort)s.Length, MS);
            int a = Encoding.Unicode.GetBytes(s, 0, s.Length, m_pTmp, 0);
            MS.Write(m_pTmp, 0, a);
        }

        public string Name
        {
            get
            {
                return m_File.Name;
            }
        }

        public string this[int row, int column]
        {
            get
            {
                return data[column][row];
            }
            set
            {
                data[column][row] = value;
            }
        }

        public int Columns
        {
            get
            {
                return data.Count;
            }
        }

        public int Rows
        {
            get
            {
                if (data.Count > 0)
                    return data[0].Count;
                else return 0;
            }
        }

        public ICollection<string> ColumnHeaders
        {
            get
            {
                return header;
            }
        }
    }
}
