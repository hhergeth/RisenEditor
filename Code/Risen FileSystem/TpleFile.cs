using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GameLibrary;
using GameLibrary.IO;
using SlimDX;

namespace RisenEditor.Code.RisenTypes
{
    public class TpleFile
    {
        char[] magic;
        short Version;
        int count;
        List<TempHeader> headers;
        List<TempContent> contents;

        public TpleFile(EFile F)
        {
            eCArchiveFile a_Stream = new eCArchiveFile(F);
            a_Stream.Position = 14;
            magic = a_Stream.Read<char>(8);
            Version = a_Stream.Read<short>();
            count = a_Stream.Read<int>();
            headers = new List<TempHeader>();
            contents = new List<TempContent>();
            for (int i = 0; i < count; i++)
            {
                headers.Add(new TempHeader(a_Stream));
                contents.Add(new TempContent(a_Stream));
            }
        }
    }

    public class TempHeader
    {
        public short Version;
        bCPropertyID ID;
        byte bHasRefTemplateID;
        bCProperty m_pRefTemplateID;
        float m_fRenderAlphaValue;
        float m_fViewRange;
        short enumInsertType;
        bCString st0;
        Vector3 v0;
        Quaternion q0;
        bCDateTime dt0;

        byte b0, b1, bObsolete1, bValue1, bValue2, bValue3, bValue4, m_u8ScaleGridPercentage, bValue5, bValue6, bValue7;

        public TempHeader(IFile a_Stream)
        {
            Version = a_Stream.Read<short>();
            ID = new bCPropertyID(a_Stream);
            b0 = a_Stream.Read<byte>();
            b1 = a_Stream.Read<byte>();
            bObsolete1 = a_Stream.Read<byte>();
            if (Version < 215u)
                bValue1 = a_Stream.Read<byte>();
            bValue2 = a_Stream.Read<byte>();
            bValue3 = a_Stream.Read<byte>();
            bValue1 = a_Stream.Read<byte>();
            bValue4 = a_Stream.Read<byte>();
            if (Version >= 214u)
                bValue1 = a_Stream.Read<byte>();
            bHasRefTemplateID = a_Stream.Read<byte>();
            if (bHasRefTemplateID == 1)
                m_pRefTemplateID = new bCProperty(a_Stream);
            m_fRenderAlphaValue = a_Stream.Read<float>();
            m_fViewRange = a_Stream.Read<float>();
            enumInsertType = a_Stream.Read<short>();
            st0 = new bCString(a_Stream);
            v0 = a_Stream.Read<Vector3>();
            q0 = a_Stream.Read<Quaternion>();
            if (Version < 213u)
            {
                bValue1 = a_Stream.Read<byte>();
                bValue1 = a_Stream.Read<byte>();
            }
            if (Version < 212u)
                bValue1 = a_Stream.Read<byte>();
            if (Version < 213u)
                a_Stream.Read<float>();
            dt0 = new bCDateTime(a_Stream);
            if (Version < 213u)
                a_Stream.Read<byte>();
            if (Version < 217u)
                a_Stream.Read<float>();
            if (Version < 213u)
            {
                a_Stream.Read<byte>();
                bValue1 = a_Stream.Read<byte>();
            }
            bValue5 = a_Stream.Read<byte>();
            bValue6 = a_Stream.Read<byte>();
            bValue7 = a_Stream.Read<byte>();
            if (Version >= 211u)
                m_u8ScaleGridPercentage = a_Stream.Read<byte>();
            if (Version >= 218u)
                a_Stream.Read<byte>(); 
        }
    }

    public class TempContent
    {
        public class c0
        {
            public short s0;
            public bCString st0;
            public int i0;
            public bCAccessorPropertyObject obj;
            public int i1;
        }

        public List<c0> data;

        public TempContent(eCArchiveFile a_Stream)
        {
            int q = a_Stream.Read<int>();
            data = new List<c0>(q);
            for (int i = 0; i < data.Capacity; i++)
            {
                c0 c = new c0();
                c.s0 = a_Stream.Read<short>();
                c.st0 = new bCString(a_Stream);
                c.i0 = a_Stream.Read<int>();
                c.obj = new bCAccessorPropertyObject(a_Stream);
                c.i1 = a_Stream.Read<int>();
                data.Add(c);
            }
        }
    }
}
