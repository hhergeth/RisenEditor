using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.UI
{
    public partial class TabViewer : Form
    {
        tabFile T;
        bool notice;

        public TabViewer()
        {
            InitializeComponent();
            foreach (tabFile k in FileManager.GetTabFiles())
            {
                listView1.Items.Add(k.Name);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileManager.SaveTabFiles();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string f = listView1.SelectedItems[0].Text;
            selectTab(FileManager.GetTabFile(f));
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (!notice)
                return;
            string k = dataGridView1.Rows[e.RowIndex].Cells[0].Value as string;
            if (k == null)
                k = string.Empty;
            T.addString(k);
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string v = (string)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            T.setString((string)dataGridView1.Rows[e.RowIndex].Cells[0].Value, v, e.ColumnIndex);
        }

        private void addColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddColumnForm A = new AddColumnForm();
            DialogResult dr = A.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                T.addColumn(A.ColumnHeader);
                dataGridView1.Columns.Add(A.ColumnHeader, A.ColumnHeader);
            }
        }

        private void newTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewTabForm A = new NewTabForm();
            DialogResult dr = A.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                T = FileManager.CreateNewTabFile(A.TabName, A.TabPrefix);
                listView1.Items.Add(A.TabName + ".tab");
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            cloneToolStripMenuItem.Enabled = listView1.SelectedIndices.Count > 0;
            if(listView1.SelectedIndices.Count > 0)
                cloneToolStripMenuItem.Text = "Clone(" + listView1.SelectedItems[0].Text + ")";
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string f = listView1.SelectedItems[0].Text;
            tabFile old = FileManager.GetTabFile(f);
            NewTabForm A = new NewTabForm();
            DialogResult dr = A.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                T = FileManager.CreateNewTabFile(A.TabName, A.TabPrefix, old.ColumnHeaders.ToArray());
                listView1.Items.Add(A.TabName + ".tab");
                selectTab(T);
            }
        }

        void selectTab(tabFile t)
        {
            notice = false;
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            this.T = t;
            foreach (string s in T.ColumnHeaders)
                dataGridView1.Columns.Add(s, s);
            for (int i = 0; i < T.Rows; i++)
                dataGridView1.Rows.Add();

            for (int i = 0; i < T.Columns; i++)
                for (int j = 0; j < T.Rows; j++)
                    dataGridView1.Rows[j].Cells[i].Value = T[j, i];

            notice = true;
        }
    }
} 
