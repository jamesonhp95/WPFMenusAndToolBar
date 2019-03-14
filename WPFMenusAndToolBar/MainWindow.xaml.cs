/**
 * Jameson Price
 * This is the section of the program that deals with the main window of the program. It is defaulted to stopped and the button is disabled until you start the program.
 * There are options to specify file extensions, a directory path, add selected items from display box, add all items from display box, and clear the database.
 * The exit button prompts the user to save monitored files to database upon attempting to exit.
 * */

using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;

namespace WPFMenusAndToolBar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileSystemWatcher watcher;
        private DBWindow dbWindow;
        private SQLiteConnection conn;
        private SQLiteCommand cmd;
        private SQLiteDataReader rdr;
        private int progId;
        public MainWindow()
        {
            InitializeComponent();
            this.conn = new SQLiteConnection("Data Source=WPF_Menu_Toolbar_Filelog.db;Version=3;New=True;Compress=True;");
            this.cmd = conn.CreateCommand();
            CreateTable();
            SetupWatcher();
            this.progId = NumberOfRows();
            this.btnStop.IsEnabled = false;
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
            catch(Exception err)
            {
                this.conn.Close();
            }
        }

        private void SetupWatcher()
        {
            watcher = new FileSystemWatcher(@"C:\");
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
          | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {

            Dispatcher.BeginInvoke(
               (Action)(() =>
               {
                   this.txtWatcherEvents.Items.Add("File created: " + e.FullPath);
               }));
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {

            Dispatcher.BeginInvoke(
               (Action)(() =>
               {
                   this.txtWatcherEvents.Items.Add("File deleted: " + e.FullPath);
               }));
            
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {

            Dispatcher.BeginInvoke(
               (Action)(() =>
               {
                   this.txtWatcherEvents.Items.Add("File changed: " + e.FullPath);
               }));
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {

            Dispatcher.BeginInvoke(
               (Action)(() =>
               {
                   this.txtWatcherEvents.Items.Add("File renamed: from " + e.OldFullPath + " to " + e.FullPath);
               }));
        }
        
        private void Add_All(System.Collections.IList e)
        {
            string ext, fname, evt;
            List<object> remove = new List<object>();
            foreach (object queued in e)
            {
                ext = GetExt(queued.ToString());
                fname = GetFname(queued.ToString());
                evt = GetEvt(queued.ToString());
                AddRow(ext, fname, evt);
                remove.Add(queued);
            }
            string str = "Added the following entries to database:\n";
            foreach (object item in e)
            {
                str += item.ToString() + "\n";
            }
            foreach (object obj in remove)
            {
                e.Remove(obj);
            }
            MessageBox.Show(str);
        }

        private void Add_Selected(ListBox e)
        {
            string ext, fname, evt;
            List<object> added = new List<object>();
            foreach (object queued in e.SelectedItems)
            {
                ext = GetExt(queued.ToString());
                fname = GetFname(queued.ToString());
                evt = GetEvt(queued.ToString());
                AddRow(ext, fname, evt);
                added.Add(queued);
            }
            string str = "Added the following entries to database:\n";
            foreach (object item in added)
            {
                str += item.ToString() + "\n";
                e.Items.Remove(item);
            }
            MessageBox.Show(str);
        }

        private string GetExt(string input)
        {
            input = GetFname(input);
            string[] subs = input.Split('.');
            return "." + subs[subs.Length - 1];
        }

        private string GetFname(string input)
        {
            string[] subs = input.Split('\\');
            return subs[subs.Length - 1];
        }

        private string GetEvt(string input)
        {
            string[] subs = input.Split(':');
            return subs[0];
        }

        private void AddRow(string ext, string fname, string evt)
        {
            try
            {
                int rowId = NumberOfRows();
                this.conn.Open();
                this.cmd.CommandText = "INSERT INTO File_Log (Id, Extension, FileName, Event, ProgNumber) VALUES (" + rowId + ", '" + ext + "', '" + fname + "', '" + evt + "', 'fileLogProgId-" + this.progId + "');";
                this.cmd.ExecuteNonQuery();
                this.conn.Close();
            }
            catch (Exception err)
            {
                this.conn.Close();
            }
        }

        private int NumberOfRows()
        {
            int res = 1;
            try
            {
                this.conn.Open();
                this.cmd.CommandText = "SELECT * from File_Log;";
                this.rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    res = Convert.ToInt32(rdr["Id"].ToString()) + 1;
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

        private void mnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            string msg = "Would you like to add unsaved entries to database?";
            string cpt = "Wait! Before you go:";
            MessageBoxButton btns = MessageBoxButton.YesNo;
            MessageBoxResult res;
            res = MessageBox.Show(msg, cpt, btns);
            if(res == System.Windows.MessageBoxResult.Yes)
            {
                Add_All(this.txtWatcherEvents.Items);
            }
            Environment.Exit(0);
        }

        private void addSelected_Click(object sender, RoutedEventArgs e)
        {
            Add_Selected(this.txtWatcherEvents);
        }

        private void addAll_Click(object sender, RoutedEventArgs e)
        {
            Add_All(this.txtWatcherEvents.Items);
        }

        private void clearDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.conn.Open();
                this.cmd.CommandText = "DROP TABLE File_Log";
                this.cmd.ExecuteNonQuery();
                this.conn.Close();
                CreateTable();
                MessageBox.Show("Database has been cleared.");
            }
            catch(Exception err)
            {
                this.conn.Close();
            }
        }

        private void watchSpecificDir_Click(object sender, RoutedEventArgs e)
        {
            if (this.pathSpecified.Text == "")
            {
                watcher.Path = @"C:\";
                MessageBox.Show("Watching" + @"C:\");
            }
            else
            {

                try
                {
                    string path = this.pathSpecified.Text;
                    watcher.Path = path;
                    MessageBox.Show("Watching " + path);
                }
                catch (Exception err)
                {
                    MessageBox.Show("Please Enter a valid path");
                }
            }
        }

        private void watchSpecificExt_Click(object sender, RoutedEventArgs e)
        {
            if (this.extSpecified.Text == "")
            {
                if (string.IsNullOrEmpty(this.cbox.Text))
                {
                    watcher.Filter = "*.*";
                    MessageBox.Show("Watching all extensions");
                }
                else
                {
                    try
                    {
                        string ext = this.cbox.SelectedItem.ToString();
                        ext = GetExt(ext);
                        watcher.Filter = "*" + ext;
                        MessageBox.Show("Watching extension type: " + ext);
                    }
                    catch(Exception err)
                    {
                        MessageBox.Show("Unable to change extension type to watch");
                    }
                }
            }
            else
            {
                try
                {
                    string ext = this.extSpecified.Text;
                    watcher.Filter = "*" + ext;
                    MessageBox.Show("Watching extension type: " + ext);
                }
                catch(Exception error)
                {
                    MessageBox.Show("Invalid Extension Specified");
                }
            }
        }

        private void mnuFileStart_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("FileWatcher initiated. Monitoring of files has begun.");
            watcher.Created += Watcher_Created;
            watcher.Deleted += Watcher_Deleted;
            watcher.Changed += Watcher_Changed;
            watcher.Renamed += Watcher_Renamed;
            this.btnStart.IsEnabled = false;
            this.btnStop.IsEnabled = true;
        }

        private void mnuFileStop_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("FileWatcher stopped. Monitoring of files has ceased.");
            watcher.Created -= Watcher_Created;
            watcher.Changed -= Watcher_Changed;
            watcher.Deleted -= Watcher_Deleted;
            watcher.Renamed -= Watcher_Renamed;
            this.btnStop.IsEnabled = false;
            this.btnStart.IsEnabled = true;
        }

        private void mnuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This program is intended to allow a robust feeling of monitoring files in a " + 
                "specified path. See the Details page for more information about specific tools within the program. " +
                "\n\nVersion: 1.0.0\nAuthor: Jameson Price\nUtilized the template given by Tom Capaul.");
        }

        private void mnuHelpDetails_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a small program that can be used to watch files from a directory, either specified or defaulted at root level. Below you will see an " +
                "Exit, Start, and Stop button. The program begins with it Stopped to avoid unwanted filewatching. To the bottom right of the display box for current watched files you " +
                "will see a Specify Extension Button that allows for a drop down selection or text to be entered into the field for narrowing the parameters for what files will be monitored " +
                "by the program. The manual text entree will be prioritized, thus if you type an extension to watch, then choose from the drop down menu, it will target the manually set extension" +
                " and will ignore the drop down field. Additionally to the left of the extension specification, their is a path specification to change the directory the program will monitor. " +
                "This must be a valid path. Off to the far right there are intuitive buttons to add selected displayed records in the display box, add all from the display box to the database, and an " +
                "option to clear the database entirely.");
        }

        private void mnuFileDBWindow_Click(object sender, RoutedEventArgs e)
        {
            this.dbWindow = new DBWindow();
            this.dbWindow.ShowDialog();
        }
    }
}
