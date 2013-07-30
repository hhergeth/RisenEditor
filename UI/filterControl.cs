using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.UI
{
    public partial class filterControl : UserControl
    {
        public event EventHandler UpdateEvent;

        public filterControl()
        {
            InitializeComponent();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("gCInventory_PS");
            comboBox1.Items.Add("eCPortalRoom_PS");
            comboBox1.Items.Add("eCOccluder_PS");
            comboBox1.Items.Add("gCInteraction_PS");
            comboBox1.Items.Add("eCMover_PS");
            comboBox1.Items.Add("eCParticle_PS");
            comboBox1.Items.Add("eCVegetation_PS");
            comboBox1.Items.Add("gCLock_PS");
            comboBox1.Items.Add("gCParty_PS");
            comboBox1.Items.Add("gCSkills_PS");
            comboBox1.Items.Add("eCDecal_PS");
            comboBox1.Items.Add("eCAudioEmitter_PS");
            comboBox1.Items.Add("eCSpeedTree_PS");
            comboBox1.Items.Add("eCWeatherZone_PS");
            comboBox1.Items.Add("gCAnchor_PS");
            comboBox1.Items.Add("gCArena_PS");
            comboBox1.Items.Add("gCBook_P");
            comboBox1.Items.Add("gCDoor_PS");
            comboBox1.Items.Add("gCItem_PS");
            comboBox1.Items.Add("gCLetter_PS");
            comboBox1.Items.Add("gCMapInfo_PS");
            comboBox1.Items.Add("gCNPC_PS");
            comboBox1.Items.Add("gCProjectile2_PS");
            comboBox1.Items.Add("gCRecipe_PS");
            comboBox1.Items.Add("gCWaterZone_PS");
            comboBox1.Items.Add("gCNavZone_PS");
            comboBox1.Items.Add("gCNegZone_PS");
            comboBox1.Items.Add("gCNavPath_PS");
            comboBox1.Items.Add("gCPrefPath_PS");
        }

        public string pSFilter = null;
        public string filterNamePart = null;

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            filterNamePart = (textBox1.Text.Length == 0) ? null : textBox1.Text;
            if (UpdateEvent != null)
                UpdateEvent(this, null);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pSFilter = (comboBox1.SelectedIndex == 0) ? null : (string)comboBox1.SelectedItem;
            if (UpdateEvent != null)
                UpdateEvent(this, null);
        }
    }
}
