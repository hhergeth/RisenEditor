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
    public partial class gCDialog_PS_Modifier : PS_Modifier
    {
        public gCDialog_PS_Modifier()
        {
            InitializeComponent();
            Array S = Enum.GetValues(typeof(gESkill));
            foreach (object s in S)
                comboBox2.Items.Add((gESkill)s);
        }

        public override void Activate()
        {
            bTObjArray<gCSkillRange> T = P.CurrNode.getSet<gCDialog_PS_Wrapper>().TeachSkills;
            comboBox1.Items.Clear();
            foreach (gCSkillRange r in T)
                comboBox1.Items.Add(r);
            panel1.Visible = false;
            button2.Visible = false;
        }

        public override void Closeing()
        {
            base.Closeing();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            gCSkillRange r = comboBox1.SelectedItem as gCSkillRange;
            if (sender == numericUpDown1)
                r.MinValue = (int)numericUpDown1.Value;
            else if (sender == numericUpDown2)
                r.MaxValue = (int)numericUpDown2.Value;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            gCSkillRange r = comboBox1.SelectedItem as gCSkillRange;
            r.Skill = (gESkill)comboBox2.SelectedItem;
            comboBox1.Items[comboBox1.Items.IndexOf(r)] = r;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            gCSkillRange r = new gCSkillRange();
            comboBox1.Items.Add(r);
            P.CurrNode.getSet<gCDialog_PS_Wrapper>().TeachSkills.Add(r);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            button2.Visible = false;
            gCSkillRange r = comboBox1.SelectedItem as gCSkillRange;
            comboBox1.Items.Remove(r);
            P.CurrNode.getSet<gCDialog_PS_Wrapper>().TeachSkills.Remove(r);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel1.Visible = true;
            button2.Visible = true;
            gCSkillRange r = comboBox1.SelectedItem as gCSkillRange;
            numericUpDown1.Value = r.MinValue;
            numericUpDown2.Value = r.MaxValue;
            comboBox2.SelectedIndex = comboBox2.Items.IndexOf(r.Skill);
        }
    }
}
