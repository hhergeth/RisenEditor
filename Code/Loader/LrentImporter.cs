using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using GameLibrary.Objekte;
using SlimDX;
using GameLibrary.IO;
using GameLibrary;
using System.Threading;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.Code.Loader
{
    public static class secReader
    {
        public static void Read(EFile _File, Form1 C, List<EFile> _Files)
        {
            SystemLog.Append(LogImportance.Information, "Sec file imported : " + _File.Name);
            SecFile S = new SecFile(_File);
            foreach (string s in S.LrentFiles)
                _Files.Add(FileManager.GetFile(s));
        }
    }

    public class LrentImporter
    {
        public static bool DynamicNodes = true;

        public List<LrentFile> Read(Form1 C, params EFile[] a_Files)
        {
            if (a_Files.Length == 0) return new List<LrentFile>();

            C.startLoading();
            ManagedWorld.NodeLibrary.AddToOcTree = false;
            List<LrentFile> files = new List<LrentFile>();
            foreach (EFile f in a_Files)
            {
                LrentFile f2 = Read(f, C.Device);
                if (f2 != null)
                    files.Add(f2);
            }
            if (files.Count == 0)
            {
                C.endLoading();
                return files;
            }
            RisenWorld.AddLrents(files);
            lrentNodeLoader.loadGraphicNodes(files, C);
            if (files[0].Objects.Count > 1)
                ILrentObject.MoveCameraTo(files[0][1]);
            return files;
        }

        void DoThreading(EFile[] a_Files, List<LrentFile> a_FilesFin, API_Device D)
        {
            HWND v_MainBlock = ThreadManager.CreateSemaphore(0, 1);
            Queue<EFile> m_Files = new Queue<EFile>(a_Files.Length);
            foreach (EFile f in a_Files)
                m_Files.Enqueue(f);
            int c = Math.Max(Environment.ProcessorCount - 2, 1);
            c = Math.Min(c, m_Files.Count);
            c = 1;
            HWND[] m_Threads = new HWND[c];
            object[] O = new object[] { m_Files, a_FilesFin, v_MainBlock, D};
            for (int i = 0; i < m_Threads.Length; i++)
            {
                m_Threads[i] = ThreadManager.CreateThread(DoThreading2);
                ThreadManager.StartThread(m_Threads[i], O);
            }
            ThreadManager.WaitForSingleObject(v_MainBlock);
            ThreadManager.AbortObject(v_MainBlock);
        }

        void DoThreading2(object o)
        {
            Queue<EFile> m_Files = (Queue<EFile>)(o as object[])[0];
            List<LrentFile> a_FilesFin = (List<LrentFile>)(o as object[])[1];
            HWND v_MainBlock = (HWND)(o as object[])[2];
            API_Device C = (API_Device)(o as object[])[3];

            while (m_Files.Count > 0)
            {
                EFile nF = null;
                bool isLast = false;
                lock (m_Files)
                {
                    isLast = m_Files.Count == 1;
                    nF = m_Files.Dequeue();
                }
                LrentFile f = Read(nF, C);
                lock (a_FilesFin)
                {
                    if(f != null)
                        a_FilesFin.Add(f);
                }
                if (isLast)
                    ThreadManager.ReleaseObject(v_MainBlock);
            }
        }

        LrentFile Read(EFile file, API_Device D)
        {
            SystemLog.Append(LogImportance.Information, "Lrent file imported : " + file.Name);
            if (file.Name.Contains("Levelmesh_Water_Rivermesh.lrent") || file.Name.Contains("OutMain_Nav.lrent") || !file.IsOpenable)
                return null;
            return new LrentFile(file, D);
        }
    }

    internal static class lrentNodeLoader
    {
        static System.Collections.Queue rObjects;
        static Form1 form;
        static int maxCount;
        internal static IList<LrentFile> pFiles;

        static internal void loadGraphicNodes(IList<LrentFile> files, Form1 a_Form)
        {
            ManagedWorld.NodeLibrary.AddToOcTree = false;
            pFiles = files;
            form = a_Form;
            rObjects = new System.Collections.Queue();
            rObjects = System.Collections.Queue.Synchronized(rObjects);
            foreach (LrentFile lr in files)
            {
                foreach (ILrentObject ro in lr.Objects)
                        rObjects.Enqueue(ro);
            }
            maxCount = rObjects.Count;

            HWND H = ThreadManager.CreateThread(new ParameterizedThreadStart(batch));
            ThreadManager.StartThread(H, null);
        }

        static private ILrentObject getNext()
        {
            if (rObjects.Count == 0)
            {
                ManagedWorld.NodeLibrary.AddToOcTree = true;
                ManagedWorld.NodeLibrary.OcTree.Build();
                form.endLoading();
                RisenWorld.OnLoadingFinished();
                Thread.CurrentThread.Abort();
            }
            if(rObjects.Count % 5 == 0)
                form.setPercentage(100 - (int)(((float)rObjects.Count / (float)maxCount) * 100.0f));
            ILrentObject r = rObjects.Dequeue() as ILrentObject;
            return r;
        }

        static private void batch(object o)
        {
            while (true)
            {
                ILrentObject ro = getNext();
                ro.LoadModels(form.Device);
            }
        }
    }
}
