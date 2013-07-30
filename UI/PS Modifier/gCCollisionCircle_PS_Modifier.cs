using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.UI.PS_Modifier
{
    public partial class gCCollisionCircle_PS_Modifier : PS_Modifier
    {

        public gCCollisionCircle_PS_Modifier()
        {
            InitializeComponent();
        }

        public override void Activate()
        {
            base.Activate();
            button1.Visible = button2.Visible = button3.Visible = label1.Visible = numericUpDown1.Visible = false;
            if (P.CurrNode is RisenNavStick)
            {
                button3.Visible = button2.Visible = numericUpDown1.Visible = true;
                numericUpDown1.Value = (decimal)(P.CurrNode as RisenNavStick).radius;
            }
            else
            {
                button1.Visible = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Visible = true;
            label1.Text = "Please click somwhere to specify the location.";
            P.OnBufferClick = handleMouseDown;
        }

        bool handleMouseDown(MouseEventArgs a, Vector3 v2, GameLibrary.Objekte.Node q)
        {
            Vector3 v = P.GetNewStartPosition(P.CurrNode.Position);
            float f = 100;
            P.CurrNode.getSet<gIColl>().AddCircle(v, f, true);
            P.OnBufferClick = null;
            return false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ILrentObject O = (P.CurrNode as RisenNavStick).P;
            O.getSet<gIColl>().RemoveCircle(P.CurrNode as RisenNavStick);
            P.CurrNode = null;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            float f = (float)numericUpDown1.Value;
            (P.CurrNode as RisenNavStick).radius = f;
            ILrentObject O = (P.CurrNode as RisenNavStick).P;
            O.getSet<gIColl>().SetRadius(P.CurrNode as RisenNavStick, f);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            P.CurrNode = (P.CurrNode as RisenNavStick).P;
        }
    }
}
