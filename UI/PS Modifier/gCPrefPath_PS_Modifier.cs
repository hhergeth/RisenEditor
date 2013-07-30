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
    public partial class gCPrefPath_PS_Modifier : PS_Modifier
    {
        const float stdRadius = 30;
        ILrentObject hashedObject;

        public gCPrefPath_PS_Modifier()
        {
            InitializeComponent();
        }

        public override void Activate()
        {
            hashedObject = P.CurrNode;
            button1.Visible = P.CurrNode.getSet<gCPrefPath_PS_Wrapper>() != null || P.CurrNode.getSet<gCNavPath_PS_Wrapper>() != null;
            button2.Visible = P.CurrNode is RisenNavStick;
            button4.Visible = P.CurrNode is RisenNavStick;
            label1.Visible = false;
            if (P.CurrNode is RisenNavStick)
            {
                numericUpDown1.Visible = true;
                numericUpDown1.Value = (decimal)(P.CurrNode as RisenNavStick).radius;
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
            Vector3 v = P.GetNewStartPosition(hashedObject.Position);
            if (hashedObject.getSet<gCPrefPath_PS_Wrapper>() != null)
                hashedObject.getSet<gCPrefPath_PS_Wrapper>().AddPoint(v, stdRadius, true);
            else hashedObject.getSet<gCNavPath_PS_Wrapper>().AddPoint(v, stdRadius, true);
            P.OnBufferClick = null;
            return false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ILrentObject O = (P.CurrNode as RisenNavStick).P;
            if (O.getSet<gCPrefPath_PS_Wrapper>() != null)
                O.getSet<gCPrefPath_PS_Wrapper>().RemovePoint(P.CurrNode as RisenNavStick);
            else O.getSet<gCNavPath_PS_Wrapper>().RemovePoint(P.CurrNode as RisenNavStick);
            P.CurrNode = null;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            P.CurrNode = (hashedObject as RisenNavStick).P;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            float f = (float)numericUpDown1.Value;
            (P.CurrNode as RisenNavStick).radius = f;
            ILrentObject O = (P.CurrNode as RisenNavStick).P;
            if (O.getSet<gCPrefPath_PS_Wrapper>() != null)
                O.getSet<gCPrefPath_PS_Wrapper>().SetRadius(P.CurrNode as RisenNavStick, f);
            else O.getSet<gCNavPath_PS_Wrapper>().SetRadius(P.CurrNode as RisenNavStick, f);
        }
    }
}
