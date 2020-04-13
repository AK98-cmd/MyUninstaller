using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//
using System.Management;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyUninstaller
{
    public partial class MainForm : Form
    {
        const string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        List<List<string>> myList = new List<List<string>>();
        List<List<string>> myList2 = new List<List<string>>();
        List<List<string>> myCurrentList = new List<List<string>>();
        int col = 0;
        bool ord=true;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Main();
        }

        private void Main()
        {   
            myList = new List<List<string>>();
            toolStripButton4.Enabled = false;
            PreluareDate(RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(registry_key));
            PreluareDate(RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(registry_key));
            ModificareDatePreluate();
            OrdonareMatriceInformation();
            StergereDubluri();
            PreluareIcons(ref imageList1, ref imageList2, ref myList);
            IncarcareDateInListView(imageList1, imageList2, myList);
        }

        private void PreluareDate(RegistryKey key)
        {
            foreach (string subkey_name in key.GetSubKeyNames())
            {
                RegistryKey subkey = key.OpenSubKey(subkey_name);
                string[] Values = new string[] { "DisplayName", "EstimatedSize", "DisplayVersion", "InstallDate", "Publisher", "HelpLink", "Comments", "URLInfoAbout", "URLUpdateInfo", "InstallLocation", "DisplayIcon", "UninstallString" };
                if (subkey.GetValue("DisplayName") != null && VerificareConditiiPentruPreluare(subkey))
                {
                    List<string> Details = new List<string>();
                    for (int i = 0; i < Values.Length; i++)
                        if (subkey.GetValue(Values[i]) != null)
                            Details.Add(subkey.GetValue(Values[i]).ToString());
                        else
                            Details.Add(string.Empty);
                    Details.Add(subkey.ToString());
                    myList.Add(Details);
                }
            }
        }

        private void ModificareDatePreluate()
        {
            for (int i = 0; i < myList.Count; i++)
            {
                if (!String.IsNullOrEmpty(myList[i][3]))
                    myList[i][3] = myList[i][3].Substring(6, 2) +"."+ myList[i][3].Substring(4, 2) +"."+ myList[i][3].Substring(0, 4);
            }
            try
            {
                for (int i = 0; i < myList.Count; i++)
                {
                    if (!String.IsNullOrEmpty(myList[i][1]))
                    {
                        double number = (Math.Round(Convert.ToDouble(myList[i][1]) / 1024, 2));
                        string s=String.Format("{0:0.00}", number);
                        myList[i][1] = s + " MB";
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void OrdonareMatriceInformation()
        {
            for (int i = 0; i < myList.Count-1; i++)
                for (int j = i + 1; j < myList.Count; j++)
                    if (String.Compare(myList[i][0], myList[j][0]) > 0)
                        for (int k = 0; k < myList[0].Count; k++)
                        {
                            string swap; swap = myList[i][k]; myList[i][k] = myList[j][k]; myList[j][k] = swap;
                        }
        }

        private void StergereDubluri()
        {
            for (int i = 0; i < myList.Count - 1; i++)
                for (int j = i + 1; j < myList.Count; j++)
                    if (String.Compare(myList[i][0], myList[j][0]) == 0)
                        myList.RemoveAt(i);
            toolStripStatusLabel1.Text = "Installations: " + myList.Count.ToString();
        }

        private void IncarcareDateInListView(ImageList SmallImageList, ImageList LargeImageList, List<List<string>> myList)
        {
            listView1.Items.Clear();
            listView1.SmallImageList = SmallImageList;
            listView1.LargeImageList = LargeImageList;
            for (int i = 0; i < myList.Count; i++)
            {
                ListViewItem item = new ListViewItem();
                item.ImageIndex = i;
                item.Text = myList[i][0];
                for (int l = 1; l <= 6; l++)
                {
                    if (myList[i][l] != null)
                        item.SubItems.Add(myList[i][l]);
                    else
                        item.SubItems.Add("");
                }
                listView1.Items.Add(item);
            }
            label2.Text = "Found: " + myList.Count.ToString();
            myCurrentList = myList.ToList();
            OrdonareListViewDetailsDupaColoane(col,ord);
            MyUninstaller.ListViewExtensions.SetSortIcon(listView1, 0, SortOrder.Ascending);
        }

        private static bool VerificareConditiiPentruPreluare(RegistryKey subkey)
        {
            var name = (string)subkey.GetValue("DisplayName");
            var releaseType = (string)subkey.GetValue("ReleaseType");
            var SystemComponent = subkey.GetValue("SystemComponent");
            var parentName = (string)subkey.GetValue("ParentDisplayName");
            if (DataContainer.SystemComponents == true && DataContainer.SystemUpdates == true)
                return !string.IsNullOrEmpty(name) 
                    && string.IsNullOrEmpty(releaseType);
            else
                if (DataContainer.SystemComponents == true)
                    return !string.IsNullOrEmpty(name) 
                        && string.IsNullOrEmpty(releaseType) 
                        && string.IsNullOrEmpty(parentName);
                else
                    if (DataContainer.SystemUpdates == true)
                        return !string.IsNullOrEmpty(name) 
                            && string.IsNullOrEmpty(releaseType) 
                            && (SystemComponent == null);
                    else
                        return !string.IsNullOrEmpty(name) 
                            && string.IsNullOrEmpty(releaseType) 
                            && string.IsNullOrEmpty(parentName) 
                            && (SystemComponent == null);
        }

        private bool Uninstall(int ind)
        {
            string args;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            ProcessStartInfo info = new ProcessStartInfo();
            string uninstallValue = myCurrentList[ind][11];
            if ((uninstallValue.Contains("MsiExec.exe")) && (!string.IsNullOrEmpty(uninstallValue)))
                args = "/x{" + uninstallValue.Split("/".ToCharArray())[1].Split("I{".ToCharArray())[2];
            else
                args = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(args))
                    info.FileName = uninstallValue.Split("/".ToCharArray())[0];
                else
                {
                    info.FileName = uninstallValue;
                    info.Arguments = args;
                }
                info.RedirectStandardError = false;
                info.RedirectStandardOutput = false;
                info.RedirectStandardInput = false;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                process.EnableRaisingEvents = true;
                process.StartInfo = info;
                process.Start();
                process.WaitForInputIdle();
                process.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure that you want to uninstall the selected programs ?" + Environment.NewLine + Environment.NewLine + myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][0], "Uninstaller", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                Uninstall(listView1.Items.IndexOf(listView1.SelectedItems[0]));
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                if (listView1.SelectedItems.Count != 0)
                    toolStripButton4.Enabled = true;   
            if (e.Button == MouseButtons.Right)
                if (listView1.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    contextMenuStrip1.Show(Cursor.Position);
                    if (!String.IsNullOrEmpty(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][11]))
                    {
                        uninstallToolStripMenuItem.Enabled = true;
                        toolStripButton4.Enabled = true;
                    }
                    else
                    {
                        uninstallToolStripMenuItem.Enabled = false;
                        toolStripButton4.Enabled = false;
                    }            
                    nameToolStripMenuItem.Visible=false;
                    companyToolStripMenuItem.Visible=false;
                    if (!String.IsNullOrEmpty(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][0]) || !String.IsNullOrEmpty(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][4]))
                    {
                        searchtoolStripMenuItem1.Enabled = true;

                        if (!String.IsNullOrEmpty(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][0]))
                        {
                            nameToolStripMenuItem.Text = myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][0];
                            nameToolStripMenuItem.Visible = true;

                        }
                        if (!String.IsNullOrEmpty(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][4]))
                        {
                            companyToolStripMenuItem.Text = myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][4];
                            companyToolStripMenuItem.Visible = true;
                        }
                    }
                    else
                        searchtoolStripMenuItem1.Enabled = false;
                    if (!String.IsNullOrEmpty(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][7]))
                        openAboutLinkToolStripMenuItem.Enabled = true;
                    else
                        openAboutLinkToolStripMenuItem.Enabled = false;
                    if (!String.IsNullOrEmpty(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][5]))
                        openHelpLinkToolStripMenuItem.Enabled=true;
                    else
                        openHelpLinkToolStripMenuItem.Enabled = false;
                    if (!String.IsNullOrEmpty(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][8]))
                        openUpdateLinkToolStripMenuItem.Enabled=true;
                    else
                        openUpdateLinkToolStripMenuItem.Enabled = false;
                    if (!String.IsNullOrEmpty(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][9]))
                        installLocationToolStripMenuItem.Enabled = true;
                    else
                        installLocationToolStripMenuItem.Enabled = false;
                }          
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Options form = new Options();
            form.ShowDialog();
        }

        private void StergerePotriviri(string searching)
        {
            int i = 0;
            while (i < myList2.Count)
                if (!myList2[i][0].ToLower().Contains(searching.ToLower()))
                    myList2.RemoveAt(i);
                else
                    i++;
        }

        private void Initializare()
        {
            imageList3.Images.Clear();
            imageList4.Images.Clear();
            myList2 = myList.ToList();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        { 
            Initializare();
            StergerePotriviri(textBox1.Text);
            PreluareIcons(ref imageList3, ref imageList4, ref myList2);
            IncarcareDateInListView(imageList3, imageList4, myList2);
            OrdonareListViewDetailsDupaColoane(col, ord);
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            MyUninstaller.ListViewExtensions.SetSortIcon(listView1, col, SortOrder.None);
            if (col == e.Column)
                ord = !ord;
            else
                ord = true;
            col = e.Column;
            OrdonareListViewDetailsDupaColoane(col,ord);
        }

        private void InterschimbareIntreDouaListViewItems(int indexA, int indexB)
        {
            ListViewItem item = listView1.Items[indexA];
            ListViewItem item2 = listView1.Items[indexB];
            listView1.Items.Remove(item);
            listView1.Items.Insert(indexB, item);
        }

        private void OrdonareListViewDetailsDupaColoane(int col,bool ord)
        {
            for (int i = 0; i <listView1.Items.Count-1; i++)
                    for (int j = i + 1; j < listView1.Items.Count; j++)
                        if(ord==true)
                        {
                            if (string.Compare(listView1.Items[i].SubItems[col].Text,listView1.Items[j].SubItems[col].Text) > 0)
                                InterschimbareIntreDouaListViewItems(j, i);
                        }
                        else
                        {
                            if (string.Compare(listView1.Items[i].SubItems[col].Text, listView1.Items[j].SubItems[col].Text) < 0)
                                InterschimbareIntreDouaListViewItems(j, i);
                        }
            if (ord == true)
                MyUninstaller.ListViewExtensions.SetSortIcon(listView1, col, SortOrder.Ascending);
            else
                MyUninstaller.ListViewExtensions.SetSortIcon(listView1, col, SortOrder.Descending);
        }

        private void StandardIcon(ref ImageList imageList1,ref ImageList imageList2,int i)
        {
            Image image = Image.FromFile("Stan19.png");
            imageList1.Images.Add(i.ToString(), image);
            imageList2.Images.Add(i.ToString(), image);
        }

        private void PreluareIcons(ref ImageList imageList1, ref ImageList imageList2, ref List<List<string>> myList)
        {
            string path;
            for (int i = 0; i < myList.Count; i++)
            {
                Image image;
                 if (!String.IsNullOrEmpty(myList[i][10]))
                 {
                     int l = myList[i][10].IndexOf(',');
                     if (l > 0)
                     {
                         path = myList[i][10].Substring(0, l);
                     }
                     else
                     {
                         path = myList[i][10];
                     }
                     if (path.Substring(path.Length - 4, 4) == ".exe")
                     {
                         if (File.Exists(path))
                         {
                             Icon result = Icon.ExtractAssociatedIcon(path);
                             imageList1.Images.Add(i.ToString(), result.ToBitmap());
                             imageList2.Images.Add(i.ToString(), result.ToBitmap());
                         }
                         else
                             StandardIcon(ref imageList1, ref imageList2, i);
                     }
                     else
                     {
                         if (path.Substring(path.Length - 4, 4) == ".ico")
                         {
                             if (File.Exists(path))
                             {
                                 image = Image.FromFile(path);
                                 imageList1.Images.Add(i.ToString(), image);
                                 imageList2.Images.Add(i.ToString(), image);
                             }
                             else
                                 StandardIcon(ref imageList1, ref imageList2, i);
                         }
                         else
                             StandardIcon(ref imageList1, ref imageList2, i);
                     }
                 }
                 else
                     StandardIcon(ref imageList1, ref imageList2, i);
            }
        }

        private void iconsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
        }

        private void listToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listView1.View = View.List;
        }

        private void detailsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            Main();
            Initializare();
            StergerePotriviri(textBox1.Text);
            PreluareIcons(ref imageList3, ref imageList4, ref myList2);
            IncarcareDateInListView(imageList3, imageList4, myList2);
            OrdonareListViewDetailsDupaColoane(col, ord);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            uninstallToolStripMenuItem.PerformClick();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Options form = new Options();
            form.ShowDialog();
        }

        private void openAboutLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][7]);
        }

        private void openHelpLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][5]);
        }

        private void openUpdateLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][8]);
        }

        private void installLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][9]);
        }

        private void openRegistryKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][12]);
        }

        private void nameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.google.ro/search?q=" + myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][0]);
        }

        private void companyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.google.ro/search?q=" + myCurrentList[listView1.Items.IndexOf(listView1.SelectedItems[0])][4]);
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}