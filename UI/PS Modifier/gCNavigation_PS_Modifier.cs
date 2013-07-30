using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RisenEditor.Code;
using RisenEditor.Code.RisenTypes;
using GameLibrary.Objekte;
using GameLibrary;
using SlimDX;

namespace RisenEditor.UI.PS_Modifier
{
    public partial class gCNavigation_PS_Modifier : PS_Modifier
    {
        Label lastLabel;

        public gCNavigation_PS_Modifier()
        {
            InitializeComponent();
        }

        public override void Activate()
        {
            comboBox1.Items.Clear();
            base.Activate();
            bTObjArray<gSRoutine> R = P.CurrNode.getSet<gCNavigation_PS_Wrapper>().Routines;
            foreach (gSRoutine r in R)
                comboBox1.Items.Add(r);
            if (R.Length > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void label4_DoubleClick(object sender, EventArgs e2)
        {
            eCEntityProxy e = (sender as Label).Tag as eCEntityProxy;
            if (e.Entity != null)
            {
                P.CurrNode = e.Entity;
                ILrentObject.MoveCameraTo(e.Entity);
            }
        }

        private void label4_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                label8.Visible = true;
                label8.Text = "Please select an entity which contains a gCAIHelper_FreePoint_PS set.";
                lastLabel = sender as Label;
                P.OnBufferClick = handleMouseDown;
                (Parent as PoperContainer).CAN_CLOSE = false;
            }
        }

        bool handleMouseDown(MouseEventArgs e, Vector3 v, Node q)
        {
            if (q.Tag == null || (q.Tag as ILrentObject.ObjectTagger).Object == null)
                return false;
            label8.Visible = false;
            ((lastLabel as Label).Tag as eCEntityProxy).Entity = (q.Tag as ILrentObject.ObjectTagger).Object;
            lastLabel.Text = ((lastLabel as Label).Tag as eCEntityProxy).Entity.ToString();
            (Parent as PoperContainer).CAN_CLOSE = true;
            P.OnBufferClick = null;
            return false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel1.Visible = true;
            gSRoutine r = comboBox1.SelectedItem as gSRoutine; 
            label4.Text = r.m_WorkingPoint.ToString();
            label4.Tag = r.m_WorkingPoint;
            label5.Text = r.m_RelaxingPoint.ToString();
            label5.Tag = r.m_RelaxingPoint;
            label6.Text = r.m_SleepingPoint.ToString();
            label6.Tag = r.m_SleepingPoint;
            textBox1.Text = r.m_strName.pString;
            button2.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            gSRoutine r = new gSRoutine();
            comboBox1.Items.Add(r);
            P.CurrNode.getSet<gCNavigation_PS_Wrapper>().Routines.Add(r);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            gSRoutine r = comboBox1.SelectedItem as gSRoutine;
            P.CurrNode.getSet<gCNavigation_PS_Wrapper>().Routines.Remove(r);
            panel1.Visible = false;
            button2.Visible = false;
            comboBox1.Items.Remove(r);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            (comboBox1.SelectedItem as gSRoutine).m_strName.pString = textBox1.Text;
            comboBox1.Items[comboBox1.SelectedIndex] = comboBox1.Items[comboBox1.SelectedIndex];
        }

        private void label4_Click(object sender, EventArgs e2)
        {
            eCEntityProxy e = (sender as Label).Tag as eCEntityProxy;
            if (e.Entity != null)
            {
                P.CurrNode = e.Entity;
            }
        }
    }
}
