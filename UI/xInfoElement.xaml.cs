using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RisenEditor.Code;
using RisenEditor.Code.RisenTypes;
using GraphSharp;
using QuickGraph;
using GraphSharp.Controls;
using System.Xml;
using System.ComponentModel;

//var a = InfoWrapper.ToXml(w);
/*
XmlWriterSettings settings = new XmlWriterSettings();
settings.Indent = true;
settings.NewLineOnAttributes = true;
settings.OmitXmlDeclaration = true;
settings.ConformanceLevel = ConformanceLevel.Document;
using (XmlWriter writer = XmlWriter.Create(w.Name + ".xml", settings))
{
    a.WriteTo(writer);
}
*/
//InfoWrapper w2 = InfoWrapper.FromXml(a);

namespace RisenEditor.UI
{
    /// <summary>
    /// Interaktionslogik für xInfoElement.xaml
    /// </summary>
    public partial class xInfoElement : UserControl
    {
        public static xInfoElement g_Singleton;

        Dictionary<string, InfoWrapper> m_pData = new Dictionary<string, InfoWrapper>();
        List<InfoWrapper> m_pChanged = new List<InfoWrapper>();

        public xInfoElement()
        {
            g_Singleton = this;
            InitializeComponent();

            eCArchiveFile E = new eCArchiveFile(FileManager.GetFile("compiled_infos.bin"));
            E.Position = 14;
            eCDocArchive D = new eCDocArchive(E);
            
            foreach (bCAccessorPropertyObject o in D)
            {
                InfoWrapper w = new InfoWrapper(o.Class as gCInfo);
                m_pData.Add(w.Name.pString, w);     
            }

            System.IO.DirectoryInfo m = new System.IO.DirectoryInfo(FileManager.g_pGamepath + "data\\raw\\infos");
            if (m.Exists)
            {
                foreach (System.IO.FileInfo fi in m.GetFiles("*.xinf"))
                {
                    InfoWrapper w = InfoWrapper.FromXml(System.Xml.Linq.XElement.Load(fi.FullName));
                    m_pData.Add(w.Name.pString, w);
                }
            }

            listView1.ItemsSource = m_pData.Values;
            setElement(m_pData["PANKRATZX2_00647"], 1, 0);
        }

        void setElement(InfoWrapper W, int parent, int children)
        {
            BidirectionalGraph<object, IEdge<object>> L = new BidirectionalGraph<object, IEdge<object>>();
            L.AddVertex(W);
            if (parent != 0)
            {
                InfoWrapper c = W;
                while (c.Parent.pString != "")
                {
                    InfoWrapper p = m_pData[W.Parent.pString];
                    if (!L.ContainsVertex(p))
                        L.AddVertex(p);
                    L.AddEdge(new Edge<object>(p, c));
                    if (parent == 1 || p.Name.pString == c.Name.pString)
                        break;
                    c = p;
                }
            }
            if (children != 0)
            {
                Action<InfoWrapper> T = null;
                T = (p) =>
                {
                    foreach (InfoWrapper a in m_pData.Values)
                        if (a.Parent.pString == p.Name.pString)
                        {
                            if (!L.AddVertex(a))
                                L.AddVertex(a);
                            L.AddEdge(new Edge<object>(p, a));
                            if (children != 1)
                                T(a);
                        }
                };
                T(W);
            }
            graphLayout1.Graph = L;
            foreach (var v in graphLayout1.Children)
            {
                if (v is VertexControl)
                    (v as VertexControl).PreviewMouseDoubleClick += graphLayout1_MouseDown;
            }
            (windowsFormsHost2.Child as System.Windows.Forms.PropertyGrid).SelectedObject = W;
            addWrapper(W);
        }

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listView1.SelectedItem != null)
                setElement(listView1.SelectedItem as InfoWrapper, 2, 2);
        }

        private void graphLayout1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            InfoWrapper o = (sender as VertexControl).Vertex as InfoWrapper;
            setElement(o, 2, 2);
        }

        public void setCmd(InfoCommandWrapper a, InfoWrapper W)
        {
            addWrapper(W);
            (windowsFormsHost1.Child as System.Windows.Forms.PropertyGrid).SelectedObject = a;
            (windowsFormsHost2.Child as System.Windows.Forms.PropertyGrid).SelectedObject = W;
        }

        public void filterNew()
        {
            string q = textBox1.Text == "" ? "*" : textBox1.Text, w = textBox2.Text == "" ? "*" : textBox2.Text, e = textBox3.Text == "" ? "*" : textBox3.Text;
            string ow = "^" + q.Replace(@".", @"\.").Replace(@"*", @".*").Replace("?", "(.{1,1})");
            string qu = "^" + w.Replace(@".", @"\.").Replace(@"*", @".*").Replace("?", "(.{1,1})");
            string na = "^" + e.Replace(@".", @"\.").Replace(@"*", @".*").Replace("?", "(.{1,1})");
            System.Text.RegularExpressions.Regex r0 = new System.Text.RegularExpressions.Regex(ow, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex r1 = new System.Text.RegularExpressions.Regex(qu, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex r2 = new System.Text.RegularExpressions.Regex(na, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            List<InfoWrapper> A = new List<InfoWrapper>();
            foreach (InfoWrapper a in m_pData.Values)
            {
                if ((a.Owner == "" || r0.IsMatch(a.Owner)) && (a.Quest == "" || r1.IsMatch(a.Quest)) && (a.Name == "" || r2.IsMatch(a.Name)))
                    A.Add(a);
            }
            listView1.ItemsSource = A;
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            filterNew();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            foreach (InfoWrapper w in m_pChanged)
                saveWrapper(w);
            m_pChanged.Clear();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            NewInfoForm F = new NewInfoForm();
            if (F.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InfoWrapper w = new InfoWrapper(F.InfoName, F.InfoOwner, F.InfoType, F.InfoCndType);
                m_pData.Add(w.Name, w);
                saveWrapper(w);
            }
        }

        void saveWrapper( InfoWrapper w)
        {
            string n = FileManager.CreateNewPhysicalFile(FileManager.NewFileType.Info, w.Name + ".xinf").Path;
            var a = InfoWrapper.ToXml(w);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Document;
            using (XmlWriter writer = XmlWriter.Create(n, settings))
            {
                a.WriteTo(writer);
            }  
        }

        void addWrapper(InfoWrapper arg1)
        {
            if (!m_pChanged.Contains(arg1))
                m_pChanged.Add(arg1);
        }
    }
}
