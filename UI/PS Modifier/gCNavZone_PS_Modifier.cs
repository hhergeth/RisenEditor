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
    public partial class gCNavZone_PS_Modifier : PS_Modifier
    {
        ILrentObject hashedObject;

        public gCNavZone_PS_Modifier()
        {
            InitializeComponent();
        }

        public override void Activate()
        {
            hashedObject = P.CurrNode;
            button1.Visible = P.CurrNode.getSet<gCNavZone_PS_Wrapper>() != null || P.CurrNode.getSet<gCNegZone_PS_Wrapper>() != null;
            button2.Visible = P.CurrNode is RisenNavStick;
            button3.Visible = P.CurrNode.getSet<gCNavZone_PS_Wrapper>() != null || P.CurrNode.getSet<gCNegZone_PS_Wrapper>() != null;
            button4.Visible = P.CurrNode is RisenNavStick;
            label1.Visible = false;
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
            if (hashedObject.getSet<gCNavZone_PS_Wrapper>() != null)
                hashedObject.getSet<gCNavZone_PS_Wrapper>().AddPoint(v, true);
            else hashedObject.getSet<gCNegZone_PS_Wrapper>().AddPoint(v, true);
            P.CurrNode = hashedObject;
            P.OnBufferClick = null;
            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ILrentObject O = (P.CurrNode as RisenNavStick).P;
            if (O.getSet<gCNavZone_PS_Wrapper>() != null)
                O.getSet<gCNavZone_PS_Wrapper>().RemovePoint(P.CurrNode as RisenNavStick);
            else O.getSet<gCNegZone_PS_Wrapper>().RemovePoint(P.CurrNode as RisenNavStick);
            P.CurrNode = null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (hashedObject.getSet<gCNavZone_PS_Wrapper>() != null)
                hashedObject.getSet<gCNavZone_PS_Wrapper>().CalculateCenter();
            else hashedObject.getSet<gCNegZone_PS_Wrapper>().CalculateCenter();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            P.CurrNode = (hashedObject as RisenNavStick).P;
        }
    }
}
