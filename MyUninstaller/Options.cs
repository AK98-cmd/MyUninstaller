using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//
using System.Drawing.Drawing2D;

namespace MyUninstaller
{
    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            int node = treeView1.SelectedNode.Index;
            if (node == 0)
            {
                panel3.Location = new Point(1200, 1200);
                panel1.Location = new Point(153, 7);
            }
            if (node == 1)
            {
                panel1.Location = new Point(600, 600);
                panel3.Location = new Point(153, 7);
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            Brush brush = new LinearGradientBrush(panel2.ClientRectangle, System.Drawing.ColorTranslator.FromHtml("#BFCDDB"), Color.White, LinearGradientMode.Horizontal);
            panel2.CreateGraphics().FillRectangle(brush, panel2.ClientRectangle);
            String drawString = "General";
            Font drawFont = new Font("Arial", 8);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            Point drawPoint = new Point(2, 2);
            e.Graphics.DrawString(drawString, drawFont, drawBrush, drawPoint);
        }


        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            Brush brush = new LinearGradientBrush(panel4.ClientRectangle, System.Drawing.ColorTranslator.FromHtml("#BFCDDB"), Color.White, LinearGradientMode.Horizontal);
            panel4.CreateGraphics().FillRectangle(brush, panel4.ClientRectangle);
            String drawString = "Options";
            Font drawFont = new Font("Arial", 8);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            Point drawPoint = new Point(2, 2);
            e.Graphics.DrawString(drawString, drawFont, drawBrush, drawPoint);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataContainer.CurrentLanguage = comboBox1.Text;
            if (checkBox1.Checked == true)
                DataContainer.SystemUpdates = true;
            else
                DataContainer.SystemUpdates = false;
            if (checkBox2.Checked == true)
                DataContainer.SystemComponents = true;
            else
                DataContainer.SystemComponents = false;
            if (checkBox3.Checked == true)
                DataContainer.BuiltIn = true;
            else
                DataContainer.BuiltIn = false;
            if (checkBox4.Checked == true)
                DataContainer.MakeSystemRestorePoint = true;
            else
                DataContainer.MakeSystemRestorePoint = false;
            if (checkBox5.Checked == true)
                DataContainer.DeleteToTheRecycleBin = true;
            else
                DataContainer.DeleteToTheRecycleBin = false;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}