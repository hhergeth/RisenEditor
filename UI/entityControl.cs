using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GameLibrary;
using RisenEditor.Code;

namespace RisenEditor.UI
{
    public partial class entityControl : UserControl
    {
        Form1 P;
        PoperContainer popup;
        Dictionary<ILrentObject, PropertySetWrapper> hashedWrapper;
        ILrentObject currNode;

        public entityControl()
        {
            InitializeComponent();
            hashedWrapper = new Dictionary<ILrentObject, PropertySetWrapper>();
        }

        public void Init(Form1 F)
        {
            P = F;
            F.ObjectSelected += new EventHandler(F_ObjectSelected);
        }

        void F_ObjectSelected(object sender, EventArgs e)
        {
            button2.Visible = false;
            if (popup != null)
            {
                popup.Dispose();
            }
            propertyGrid1.SelectedObject = P.CurrNode;
            if (P.CurrNode is RisenNavStick)
                propertyGrid1.SelectedObject = (P.CurrNode as RisenNavStick).P;
            currNode = propertyGrid1.SelectedObject as ILrentObject;

            button1.Visible = false;
            comboBox1.Items.Clear();
            comboBox1.SelectedText = "Empty";
            propertyGrid2.SelectedObject = null;
            if (currNode is ILrentObject)
                foreach (PropertySetWrapper s in currNode.SetWrappers)
                    comboBox1.Items.Add(s);

            if(currNode != null)
                if (hashedWrapper.ContainsKey(currNode))
                {
                    PropertySetWrapper p = hashedWrapper[currNode];
                    if (p != null)
                    {
                        int i = comboBox1.Items.IndexOf(p);
                        if (i != -1)
                            comboBox1.SelectedIndex = i;
                    }
                }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (hashedWrapper.ContainsKey(currNode))
                hashedWrapper[currNode] = comboBox1.SelectedItem as PropertySetWrapper;
            else hashedWrapper.Add(currNode, comboBox1.SelectedItem as PropertySetWrapper);

            button2.Visible = true;
            button1.Visible = false;
            propertyGrid2.SelectedObject = comboBox1.SelectedItem;
            bool b = PS_Modifier.PS_Modifier.canfetchModifier((comboBox1.SelectedItem as PropertySetWrapper).Set);
            if (b)
                button1.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            showModifier();
        }

        PS_Modifier.PS_Modifier showModifier()
        {
            PS_Modifier.PS_Modifier M = PS_Modifier.PS_Modifier.fetchModifier((comboBox1.SelectedItem as PropertySetWrapper).Set, P);
            if (popup != null)
                popup.Dispose();
            popup = new PoperContainer(M);
            popup.Closing += new ToolStripDropDownClosingEventHandler(popup_Closing);
            Point p = new Point(button1.Location.X - M.Size.Width, button1.Location.Y - M.Size.Height);
            p = PointToScreen(p);
            M.Activate();
            popup.Show(P, p);
            M.Visible = true;
            return M;
        }

        void popup_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            popup.Closing -= popup_Closing;
            (popup.DisplayedContainer as PS_Modifier.PS_Modifier).Closeing();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            currNode.RemoveSet(comboBox1.SelectedItem as PropertySetWrapper);
            comboBox1.Items.RemoveAt(comboBox1.SelectedIndex);
            propertyGrid2.SelectedObject = null;
        }

        public T ActivateModifier<T>() where T : PS_Modifier.PS_Modifier, new()
        {
            T t = new T();
            string s = t.SetName;
            for(int i = 0; i < comboBox1.Items.Count; i++)
                if ((comboBox1.Items[i] as PropertySetWrapper).Set == s)
                {
                    comboBox1.SelectedIndex = i;
                    if (PS_Modifier.PS_Modifier.canfetchModifier((comboBox1.SelectedItem as PropertySetWrapper).Set))
                    {
                        tabControl1.SelectedTab = tabControl1.TabPages[1];
                        return showModifier() as T;
                    }
                }
            return null;
        }
    }
}
