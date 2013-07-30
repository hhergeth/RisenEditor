using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GameLibrary;
using SlimDX;
using System.Windows.Forms.Design;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.UI.PS_Modifier
{
    public partial class gCInventory_PS_Modifier : PS_Modifier
    {
        class C1 : ListView
        {
            static Bitmap main;
            static Font F = new Font("Arial", 8, FontStyle.Regular);

            public void Init(API_Device D)
            {
                this.OwnerDraw = true;
                this.LargeImageList = this.SmallImageList = new ImageList();
                this.LargeImageList.ImageSize = new Size(22, 22);
                this.LargeImageList.Images.Add(new Bitmap(this.LargeImageList.ImageSize.Width, this.LargeImageList.ImageSize.Height));
                if (main != null)
                    return;
                FileManager.g_pApp.PauseRendering();
                GameLibrary.ShaderResourceTexture mT = new ShaderResourceTexture("GUI_IconSets01._ximg", D);
                System.IO.Stream S = new System.IO.MemoryStream();
                SlimDX.Direct3D11.Texture2D.ToStream(D.Context, mT.Texture, SlimDX.Direct3D11.ImageFileFormat.Jpg, S);
                FileManager.g_pApp.ResumeRendering();
                main = new Bitmap(S);
                mT.Dispose();
                S.Dispose();
            }

            protected override void OnDrawItem(DrawListViewItemEventArgs e)
            {
                C1_DrawItem(this, e);
                base.OnDrawItem(e);
            }

            void C1_DrawItem(object sender, DrawListViewItemEventArgs e)
            {
                if (main == null)
                    return;
                e.DrawBackground();
                e.DrawFocusRectangle();
                RectangleF E = e.Bounds;
                SizeF Q = e.Graphics.MeasureString(e.Item.Text, F);
                Guid g = Guid.Empty;
                if (e.Item.Tag is gCInventory_PS_Wrapper.InventoryItem)
                    g = (e.Item.Tag as gCInventory_PS_Wrapper.InventoryItem).ItemGuid;
                else g = (Guid)e.Item.Tag;
                if (ResourceManager.Rects.ContainsKey(g))
                {
                    RectangleF r0 = ResourceManager.Rects[g];
                    RectangleF r1 = E;
                    r1.Height -= Q.Height;
                    float sh = (r1.Height / r0.Height);
                    float sw = (r1.Width / r0.Width);
                    if (sw > sh)//new box not heigh enough
                    {
                        float h = r0.Height * sh;
                        float w = r0.Width * sh;
                        float q0 = (r1.Width - w) / 2.0f;
                        r1.X += q0;
                        r1.Width = w;
                        r1.Height = h;
                    }
                    else
                    {

                    }
                    e.Graphics.DrawImage(main, r1, r0, GraphicsUnit.Pixel);
                }
                e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
            }
        }

        static Form sF;
        long lastHit;

        public gCInventory_PS_Modifier()
        {
            InitializeComponent();
        }

        public override void Activate()
        {
            base.Activate();
            (listView1 as C1).Init(P.Device);
            if (sF == null)
            {
                sF = new Form();
                sF.Controls.Add(new C1());
                (sF.Controls[0] as C1).Init(P.Device);
                sF.Controls[0].Enabled = true;
                sF.Controls[0].Dock = DockStyle.Fill;
                foreach (KeyValuePair<Guid, Rectangle> kvp in ResourceManager.Rects)
                {
                    string s = ResourceManager.GetlName(kvp.Key);
                    ListViewItem q = new ListViewItem(s);
                    q.Tag = kvp.Key;
                    (sF.Controls[0] as ListView).Items.Add(q);
                }
            }

            _clear();
            gCInventory_PS_Wrapper S = P.CurrNode.getSet<gCInventory_PS_Wrapper>();
            foreach (gCInventory_PS_Wrapper.InventoryItem i in S.CharacterInventory.InventoryItems)
                _add(i);
        }

        void _add(gCInventory_PS_Wrapper.InventoryItem I)
        {
            ListViewItem N = new ListViewItem(I.TypeName);
            N.Tag = I;
            N.ImageIndex = 0;
            listView1.Items.Add(N);
        }

        void _clear()
        {
            listView1.Clear();
        }

        int _selected()
        {
            return listView1.SelectedIndices.Count == 0 ? -1 : listView1.SelectedIndices[0];
        }

        void _delete(int index)
        {
            listView1.Items.RemoveAt(index);
            gCInventory_PS_Wrapper S = P.CurrNode.getSet<gCInventory_PS_Wrapper>();
            S.CharacterInventory.removeItem(S.CharacterInventory.InventoryItems[index]);
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && _selected() != -1)
                _delete(_selected());
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            lastHit = Environment.TickCount;
            propertyGrid1.SelectedObject = e.Item.Tag;
            contextMenuStrip1.Items[1].Visible = true;
        }

        private void addItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (sF.Controls[0] as ListView).MouseDoubleClick += CharacterForm_MouseDoubleClick;
            sF.ShowDialog(this);
        }

        void CharacterForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            (sF.Controls[0] as ListView).MouseDoubleClick -= CharacterForm_MouseDoubleClick;
            if ((sF.Controls[0] as ListView).SelectedItems.Count == 0)
                return;
            Guid g = (Guid)(sF.Controls[0] as ListView).SelectedItems[0].Tag;
            sF.Visible = false;

            gCInventory_PS_Wrapper S = P.CurrNode.getSet<gCInventory_PS_Wrapper>();
            gCInventory_PS_Wrapper.InventoryItem nItem = S.CharacterInventory.addItem(g, 1, gEStackType.gEStackType_Normal, gEEquipSlot.gEEquipSlot_None);
            _add(nItem);
        }

        private void deleteItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(_selected() != -1)
                _delete(_selected());
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            if (Environment.TickCount - lastHit < 100 || _selected() != -1)
                return;
            propertyGrid1.SelectedObject = null;
            contextMenuStrip1.Items[1].Visible = false;
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                (Parent as ToolStripDropDown).AutoClose = false;
        }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            (Parent as ToolStripDropDown).AutoClose = true;
        }
    }
}
