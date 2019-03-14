/**
 * Jameson Price
 * This is the section of the program that deals with managing the database including the remove selected and remove all functionality.
 * Additionally it has ways to query the database and opens a new window with that query or notifies the user that no records were found.
 * This file also has the components to dynamically create a ListView to add to a new window that will be populated with query results.
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SQLite;

namespace WPFMenusAndToolBar
{
    /// <summary>
    /// Interaction logic for DBWindow.xaml
    /// </summary>
    public partial class DBWindow : Window
    {
        private int counter;
        private SQLiteConnection conn;
        private SQLiteCommand cmd;
        private SQLiteDataReader rdr;
        private int progId;
        public DBWindow()
        {
            InitializeComponent();
            this.conn = new SQLiteConnection("Data Source=WPF_Menu_Toolbar_Filelog.db;Version=3;New=True;Compress=True;");
            this.cmd = conn.CreateCommand();
            this.counter = NumberOfRows();
            this.progId = NumberOfRows();
            InitializeListView();
        }

        private int NumberOfRows()
        {
            int res = 1;
            try
            {
                this.conn.Open();
                this.cmd.CommandText = "SELECT count(*) AS res from File_Log;";
                this.rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    this.rdr.Read();
                    res = Convert.ToInt32(rdr["res"].ToString()) + 1;
                }
                this.rdr.Close();
                this.conn.Close();
                return res;
            }
            catch (Exception err)
            {
                if (!rdr.IsClosed)
                    this.rdr.Close();
                this.conn.Close();
                throw new DataMisalignedException();
            }
        }

        private void InitializeListView()
        {
            try
            {
                this.conn.Open();
                this.cmd.CommandText = "SELECT * from File_Log;";
                this.rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    this.lvDBInfo.Items.Add(new { First = rdr["Id"].ToString(), Second = rdr["FileName"].ToString(), Third = rdr["Extension"].ToString(), Fourth = rdr["Event"].ToString(), Fifth = rdr["ProgNumber"].ToString() });
                }
                this.rdr.Close();
                this.conn.Close();
            }
            catch (Exception err)
            {
                if (!rdr.IsClosed)
                    this.rdr.Close();
                this.conn.Close();
                throw new DataMisalignedException();
            }
        }

        private ListView getListViewTemplate()
        {
            ListView ret = new ListView();
            var grid = new GridView();
            var qItem1 = new GridViewColumn();
            qItem1.Header = "Row Id";
            qItem1.Width = 40;
            qItem1.DisplayMemberBinding = new Binding("First");
            var qItem2 = new GridViewColumn();
            qItem2.Header = "File Name";
            qItem2.Width = 208;
            qItem2.DisplayMemberBinding = new Binding("Second");
            var qItem3 = new GridViewColumn();
            qItem3.Header = "Extension";
            qItem3.Width = 60;
            qItem3.DisplayMemberBinding = new Binding("Third");
            var qItem4 = new GridViewColumn();
            qItem4.Header = "Event";
            qItem4.Width = 300;
            qItem4.DisplayMemberBinding = new Binding("Fourth");
            var qItem5 = new GridViewColumn();
            qItem5.Header = "Prog Id";
            qItem5.Width = 60;
            qItem5.DisplayMemberBinding = new Binding("Fifth");
            grid.Columns.Add(qItem1);
            grid.Columns.Add(qItem2);
            grid.Columns.Add(qItem3);
            grid.Columns.Add(qItem4);
            grid.Columns.Add(qItem5);
            ret.View = grid;
            ret.HorizontalAlignment = HorizontalAlignment.Left;
            ret.VerticalAlignment = VerticalAlignment.Top;
            var thick = new Thickness(0, 0, 0, 0);
            ret.Margin = thick;
            return ret;
        }

        private void searchExt_Click(object sender, RoutedEventArgs e)
        {
            if (this.extSpecified.Text == "")
                MessageBox.Show("Please enter an extension to query");
            else
            {
                try
                {
                    this.conn.Open();
                    this.cmd.CommandText = "SELECT * FROM File_Log WHERE Extension like @ext";
                    this.cmd.Parameters.AddWithValue("@ext", this.extSpecified.Text);
                    this.rdr = cmd.ExecuteReader();
                    if(rdr.HasRows)
                    {
                        var win = new Window();
                        ListView list = getListViewTemplate();
                        while(rdr.Read())
                        {
                            list.Items.Add(new {  First = rdr["Id"].ToString(), Second = rdr["FileName"].ToString(), Third = rdr["Extension"].ToString(), Fourth = rdr["Event"].ToString(), Fifth = rdr["ProgNumber"].ToString() });
                        }
                        win.Content = list;
                        win.Height = 330;
                        win.Width = 728;
                        win.Show();
                    }
                    else
                        MessageBox.Show("No results found.");
                    this.rdr.Close();
                    this.conn.Close();
                }
                catch(Exception err)
                {
                    if (!rdr.IsClosed)
                        this.rdr.Close();
                    this.conn.Close();
                    MessageBox.Show("No results found");
                }
            }
        }

        private void rmvSelected_Click(object sender, RoutedEventArgs e)
        {
            Remove_Selected(this.lvDBInfo);
        }

        private void rmvAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.conn.Open();
                this.cmd.CommandText = "DROP TABLE File_Log";
                this.cmd.ExecuteNonQuery();
                this.conn.Close();
                CreateTable();
                this.lvDBInfo.Items.Clear();
                MessageBox.Show("Database has been cleared.");
            }
            catch (Exception err)
            {
                this.conn.Close();
            }
        }

        private void CreateTable()
        {
            try
            {
                this.conn.Open();
                this.cmd.CommandText = "CREATE TABLE File_Log (Id integer primary key, Extension varchar(32), FileName varchar(256), Event varchar(512), ProgNumber varchar(256));";
                this.cmd.ExecuteNonQuery();
                this.conn.Close();
            }
            catch (Exception err)
            {
                this.conn.Close();
            }
        }

        private void Remove_Selected(ListView e)
        {
            string rowId;
            List<object> removed = new List<object>();
            foreach (object queued in e.SelectedItems)
            {
                rowId = GetRowId(queued.ToString());
                DeleteRow(rowId);
                removed.Add(queued);
            }
            string str = "Removed the following entries to database:\n";
            foreach (object item in removed)
            {
                str += "RowId: " + GetRowId(item.ToString()) + "\n";
                e.Items.Remove(item);
            }
            MessageBox.Show(str);
        }

        private string GetRowId(string input)
        {
            string[] equals = input.Split('=');
            string[] commas = equals[1].Split(',');
            return commas[0];
        }

        private string GetFname(string input)
        {
            string[] subs = input.Split('\\');
            return subs[subs.Length - 1];
        }

        private void DeleteRow(string rowId)
        {
            try
            {
                this.conn.Open();
                this.cmd.CommandText = "DELETE FROM File_Log WHERE Id = " + rowId;
                this.cmd.ExecuteNonQuery();
                this.conn.Close();
            }
            catch (Exception err)
            {
                this.conn.Close();
            }
        }

        private void searchFname_Click(object sender, RoutedEventArgs e)
        {
            if (this.fnameSpecified.Text == "")
                MessageBox.Show("Please enter an extension to query");
            else
            {
                try
                {
                    this.conn.Open();
                    this.cmd.CommandText = "SELECT * FROM File_Log WHERE FileName like @fName";
                    this.cmd.Parameters.AddWithValue("@fName", this.fnameSpecified.Text);
                    this.rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        var win = new Window();
                        ListView list = getListViewTemplate();
                        while (rdr.Read())
                        {
                            list.Items.Add(new { First = rdr["Id"].ToString(), Second = rdr["FileName"].ToString(), Third = rdr["Extension"].ToString(), Fourth = rdr["Event"].ToString(), Fifth = rdr["ProgNumber"].ToString() });
                        }
                        win.Content = list;
                        win.Height = 330;
                        win.Width = 728;
                        win.Show();
                    }
                    else
                        MessageBox.Show("No results found.");
                    this.rdr.Close();
                    this.conn.Close();
                }
                catch (Exception err)
                {
                    if (!rdr.IsClosed)
                        this.rdr.Close();
                    this.conn.Close();
                    MessageBox.Show("No results found");
                }
            }
        }

        private void searchEvent_Click(object sender, RoutedEventArgs e)
        {
            if (this.eventSpecified.Text == "")
                MessageBox.Show("Please enter an extension to query");
            else
            {
                try
                {
                    this.conn.Open();
                    this.cmd.CommandText = "SELECT * FROM File_Log WHERE Event like @evt";
                    this.cmd.Parameters.AddWithValue("@evt", this.eventSpecified.Text);
                    this.rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        var win = new Window();
                        ListView list = getListViewTemplate();
                        while (rdr.Read())
                        {
                            list.Items.Add(new { First = rdr["Id"].ToString(), Second = rdr["FileName"].ToString(), Third = rdr["Extension"].ToString(), Fourth = rdr["Event"].ToString(), Fifth = rdr["ProgNumber"].ToString() });
                        }
                        win.Content = list;
                        win.Height = 330;
                        win.Width = 728;
                        win.Show();
                    }
                    else
                        MessageBox.Show("No results found.");
                    this.rdr.Close();
                    this.conn.Close();
                }
                catch (Exception err)
                {
                    if (!rdr.IsClosed)
                        this.rdr.Close();
                    this.conn.Close();
                    MessageBox.Show("No results found");
                }
            }
        }
    }
}

