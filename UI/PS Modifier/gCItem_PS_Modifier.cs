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

namespace RisenEditor.UI.PS_Modifier
{
    public partial class gCItem_PS_Modifier : PS_Modifier
    {
        ListView lastView;
        ListViewItem lastViewItem;

        public gCItem_PS_Modifier()
        {
            InitializeComponent();
        }

        public override void Activate()
        {
            base.Activate();
            listView1.Items.Clear();
            listView2.Items.Clear();
            propertyGrid1.SelectedObject = propertyGrid2.SelectedObject = null;
            gCItem_PS_Wrapper I = P.CurrNode.getSet<gCItem_PS_Wrapper>();
            foreach (gCSkillValue o in I.RequiredSkills)
            {
                ListViewItem l = new ListViewItem(o.Skill.ToString());
                l.Tag = o;
                listView1.Items.Add(l);
            }
            foreach (gCModifySkill o in I.ModifySkills)
            {
                ListViewItem l = new ListViewItem(o.Skill.ToString());
                l.Tag = o;
                listView2.Items.Add(l);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((sender as ListView).SelectedItems.Count != 1)
                return;
            if(sender == listView1)
                propertyGrid1.SelectedObject = (sender as ListView).SelectedItems[0].Tag;
            else if(sender == listView2)
                propertyGrid2.SelectedObject = (sender as ListView).SelectedItems[0].Tag;
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;
            lastView = sender as ListView;
            lastViewItem = (sender as ListView).GetItemAt(e.X, e.Y);
            (Parent as ToolStripDropDown).AutoClose = false;
            if (lastViewItem == null)
                contextMenuStrip1.Show((sender as ListView).PointToScreen(e.Location));
            else contextMenuStrip2.Show((sender as ListView).PointToScreen(e.Location));
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lastView == listView1)
            {
                gCSkillValue o = new gCSkillValue();
                ListViewItem l = new ListViewItem(o.Skill.ToString());
                l.Tag = o;
                listView1.Items.Add(l);
                P.CurrNode.getSet<gCItem_PS_Wrapper>().RequiredSkills.Add(o);
            }
            else if (lastView == listView2)
            {
                gCModifySkill o = new gCModifySkill();
                ListViewItem l = new ListViewItem(o.Skill.ToString());
                l.Tag = o;
                listView2.Items.Add(l);
                P.CurrNode.getSet<gCItem_PS_Wrapper>().ModifySkills.Add(o);
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lastView.Items.Remove(lastViewItem);
            if (lastView == listView1)
            {
                gCSkillValue d = lastViewItem.Tag as gCSkillValue;
                P.CurrNode.getSet<gCItem_PS_Wrapper>().RequiredSkills.Remove(d);
                propertyGrid1.SelectedObject = null;
            }
            else if (lastView == listView2)
            {
                gCModifySkill d = lastViewItem.Tag as gCModifySkill;
                P.CurrNode.getSet<gCItem_PS_Wrapper>().ModifySkills.Remove(d);
                propertyGrid2.SelectedObject = null;
            } 
        }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            (Parent as ToolStripDropDown).AutoClose = true;
        }
    }
}
