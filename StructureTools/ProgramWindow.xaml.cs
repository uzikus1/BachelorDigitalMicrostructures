using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StructureTools
{
    /// <summary>
    /// Interaction logic for ProgramWindow.xaml
    /// </summary>
    public partial class ProgramWindow : Window
    {
        List<StructureTools.GenericProgramData> programs;
        Boolean fromSim, hasExternalExited;
        public Process pr;
        Timer t;

        public ProgramWindow(String programPath, String header, Boolean sim)
        {
            this.t = new Timer();
            this.t.Interval = 100;
            this.t.Tick += new EventHandler(timerEvent);
            this.hasExternalExited = false;
            InitializeComponent();
            this.Title = header;
            this.fromSim = sim;
            this.programs = new List<StructureTools.GenericProgramData>();
            this.processDirectory(programPath, "*.exe");
            this.processDirectory(programPath, "*.jar");
            this.processDirectory(programPath, "*.lnk");
            for (int i = 0; i < programs.Count; i++)
                this.lbox_ProgramList.Items.Add(programs[i].FileName);
        }
        void processDirectory(String targetDirectory, String targetExtension)
        {
            // process the list of files found in the directory
            String[] fileEntries = Directory.GetFiles(targetDirectory, targetExtension);
            foreach (String fileName in fileEntries)
                this.programs.Add(new GenericProgramData(fileName));

            // recurse into subdirectories of this directory
            String[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (String subdirectory in subdirectoryEntries)
                this.processDirectory(subdirectory, targetExtension);
        }
        void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void lbox_ProgramList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int selectedIndex = this.lbox_ProgramList.Items.IndexOf(this.lbox_ProgramList.SelectedItem.ToString());
                if (this.programs[selectedIndex].FileExt == "lnk")
                    this.pr = Process.Start(new ProcessStartInfo(this.programs[selectedIndex].FilePath));
                else
                    this.pr = Process.Start(programs[selectedIndex].FilePath);
                if(fromSim)
                {
                    this.t.Start();
                    this.pr.EnableRaisingEvents = true;
                    this.pr.Exited += new EventHandler(p_Exited);
                }
            }
            catch (Exception) { }
        }
        void timerEvent(object sender, EventArgs e)
        {
            if(this.hasExternalExited)
            {
                this.t.Stop();
                this.hasExternalExited = false;
                this.Close();
            }  
        }
        void p_Exited(object sender, EventArgs e)
        {
            this.hasExternalExited = true;
        }
        
    }
}
