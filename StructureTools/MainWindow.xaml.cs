using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Drawing;
using System.Timers;
using System.Windows.Media.Imaging;

namespace StructureTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private Project p;
        private SimWindow sw;
        private GrainWindow gw;
        private ProgramWindow pw;
        private FileSystemWatcher watcher;
        private MonteCarlo mc;
        private GrainGrowth gg;
        private Thread th;
        private SaveLoadHelper slh;
        private System.Windows.Forms.Timer timerGrown, timerMCLabels, timerMCEnd;

        public MainWindow()
        {
            //timers needed for main thread content changes by other threads
            this.timerGrown = new System.Windows.Forms.Timer();
            this.timerGrown.Interval = 10;
            this.timerGrown.Tick += new EventHandler(timerGrownEvent);

            this.timerMCLabels = new System.Windows.Forms.Timer();
            this.timerMCLabels.Interval = 10;
            this.timerMCLabels.Tick += new EventHandler(timerMCLabelEvent);

            this.timerMCEnd = new System.Windows.Forms.Timer();
            this.timerMCEnd.Interval = 10;
            this.timerMCEnd.Tick += new EventHandler(timerMCEndEvent);
 
            this.watcher = new FileSystemWatcher();
            this.watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            this.watcher.Changed += new FileSystemEventHandler(this.OnChanged);

            this.p = new Project();
            this.mc = new MonteCarlo();
            this.gg = new GrainGrowth();
            this.slh = new SaveLoadHelper();
        }

        //simulation window functions

        private void MenuItem_New_Click(object sender, RoutedEventArgs e)
        {
            if (this.slh.unsavedChangesPrompt(this.p) == 2)
                return;
            this.sw = new StructureTools.SimWindow(this.p, false);
            this.sw.Show();
            this.sw.Closed += new EventHandler(SimWindow_Closed);
        }
        private void MenuItem_Settings_Click(object sender, RoutedEventArgs e)
        {
            this.sw = new StructureTools.SimWindow(this.p, true);
            this.sw.Show();
            this.sw.Closed += new EventHandler(this.SimWindow_Closed);
        }
        private void SimWindow_Closed(object sender, EventArgs e)
        {
            if(this.sw.PCopy.IsLoaded)
            {
                this.p = this.sw.PCopy;
                System.IO.Directory.CreateDirectory(this.p.ProjectPath);
                this.watcher.Path = System.IO.Path.GetDirectoryName(this.p.ProjectPath + this.p.ProjectName);
                this.watcher.Filter = System.IO.Path.GetFileName(this.p.ProjectPath + this.p.ProjectName + ".bmp");
                this.watcher.EnableRaisingEvents = true;
                this.displayLoadedImage();
                this.setControlsAvailability(true);
                if (this.sw.IsSettingsMenu)
                {
                    this.slh.wasContentChanged = false;

                    //if name was changed
                    if(this.sw.ChangedName)
                        this.slh.wasContentChanged = true;

                    //if sim type was changed
                    if (this.sw.ChangedSimType)
                    {
                        this.slh.wasContentChanged = true;
                        this.MenuItem_Restart_Click(null, null);
                    }

                    //if MC enable was changed
                    if (this.sw.ChangedMCEnable)
                    {
                        this.slh.wasContentChanged = true;
                        if(this.p.IsMCEnabled) //if MC is now enabled
                        {
                            if(this.p.IsGrown)
                                this.MenuItem_Sub_RunMC.IsEnabled = true;
                            else
                                this.MenuItem_Sub_RunMC.IsEnabled = false;
                        }
                        else
                            this.MenuItem_Sub_RunMC.IsEnabled = false;
                    }
                        
                    if (this.sw.ChangedMCSteps && this.sw.ChangedSimType)
                    {
                        this.slh.wasContentChanged = true;
                        if (this.p.IsMCEnabled) //if MC is now enabled
                        {
                            if (this.p.IsGrown)
                                this.MenuItem_Sub_RunMC.IsEnabled = true;
                            else
                                this.MenuItem_Sub_RunMC.IsEnabled = false;
                        }
                        else
                            this.MenuItem_Sub_RunMC.IsEnabled = false;
                        this.MenuItem_Sub_RunMC.Header = "Enable MC, finished steps: " + this.p.CurrentMCStep;
                        this.p.IsMCFinished = false;
                        this.mc.IsActive = false;
                        if(this.p.IsMCEnabled)
                            this.mc.resetMCLists(this.p);       
                    }
                    else if (this.sw.ChangedMCSteps)
                    {
                        this.slh.wasContentChanged = true;
                        if (this.p.IsMCEnabled) //if MC is now enabled
                        {
                            if (this.p.IsGrown)
                                this.MenuItem_Sub_RunMC.IsEnabled = true;
                            else
                                this.MenuItem_Sub_RunMC.IsEnabled = false;
                        }
                        else
                            this.MenuItem_Sub_RunMC.IsEnabled = false;
                        this.MenuItem_Sub_RunMC.Header = "Enable MC, finished steps: " + this.p.CurrentMCStep;
                        this.p.IsMCFinished = false;
                        this.mc.IsActive = false;
                        if (this.p.IsMCEnabled)
                            this.mc.resetMCLists(this.p);
                    }
                }
                else
                {
                    this.slh.wasContentChanged = true;
                    this.p.IsMCFinished = false;
                    this.mc.IsActive = false;                   
                }
            }
        }

        //grain window functions

        private void MenuItem_Grains_Click(object sender, RoutedEventArgs e)
        {
            this.gw = new StructureTools.GrainWindow(this.p);
            this.gw.Show();
            this.gw.Closed += new EventHandler(this.GrainWindow_Closed);
            this.MenuItem_Sub_Run.IsEnabled = false;
            this.MenuItem_Sub_Restart.IsEnabled = false;
            this.MenuItem_Sub_RunMC.IsEnabled = false;
        }
        private void GrainWindow_Closed(object sender, EventArgs e)
        {
            if(this.gw.WasListModified)
                this.slh.wasContentChanged = true;
            else
                this.slh.wasContentChanged = false;
            this.p = this.gw.PCopy;
            this.gw.IsImageClickable = false;
            this.gw.WasListModified = false;
            if(this.p.ProjectGrains.Count != 0)
            {
                this.MenuItem_Sub_Run.IsEnabled = true;
                this.MenuItem_Sub_Restart.IsEnabled = true;
                this.MenuItem_Sub_RunMC.IsEnabled = false;
            }
            this.img_MainDisplay.Source = MainWindow.convertToBitmapImage(this.p.ProjectImage);
        }
        private void img_MainDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var clickedPoint = e.GetPosition((System.Windows.Controls.Image)sender);
            Boolean clickable = false;
            try
            {
                clickable = this.gw.IsImageClickable;
            }
            catch (Exception) {}
            if (clickable)
            {
                Int32 x = Convert.ToInt32(clickedPoint.X);
                Int32 y = Convert.ToInt32(clickedPoint.Y);
                if (!this.gw.doesPointExistWithinArea(x, y))
                    if (!this.gw.doesPointExistOnList(x, y, -1))
                    {
                        Random rand = new Random();
                        System.Drawing.Color c;
                        Byte r, g, b;
                        do
                        {
                            r = Convert.ToByte(rand.Next(0, 255));
                            g = Convert.ToByte(rand.Next(0, 255));
                            b = Convert.ToByte(rand.Next(0, 255));
                            c = System.Drawing.Color.FromArgb(255, r, g, b);
                        }
                        while (this.gw.doesColorExist(new byte[] { r, g, b }));
                        this.gw.addNewGrain(x, y, r, g, b, false);
                        String displayLabel = "X = " + this.p.ProjectGrains[this.p.ProjectGrains.Count - 1].X + ", Y = " + this.p.ProjectGrains[this.p.ProjectGrains.Count - 1].Y + ", C = " + this.p.ProjectGrains[this.p.ProjectGrains.Count - 1].HexLabel;
                        this.gw.lbox_GrainsList.Items.Add(displayLabel);
                        this.img_MainDisplay.Source = MainWindow.convertToBitmapImage(this.p.ProjectImage);
                        this.gw.WasListModified = true;
                    }
            }
        }

        //program window functions

        private void MenuItem_Other_Click(object sender, RoutedEventArgs e)
        {
            if (this.slh.unsavedChangesPrompt(this.p) == 2)
                return;
            this.pw = new StructureTools.ProgramWindow(@".\_WORKSPACE\apps\other", "Other applications", false);
            this.pw.Show();  
        }
        private void MenuItem_Materials_Click(object sender, RoutedEventArgs e)
        {
            if (this.slh.unsavedChangesPrompt(this.p) == 2)
                return;
            this.pw = new StructureTools.ProgramWindow(@".\_WORKSPACE\apps\material", "Material applications", false);
            this.pw.Show();
        }
        private void MenuItem_Meshers_Click(object sender, RoutedEventArgs e)
        {
            if (this.slh.unsavedChangesPrompt(this.p) == 2)
                return;
            this.pw = new StructureTools.ProgramWindow(@".\_WORKSPACE\apps\mesher", "Mesher applications", false);
            this.pw.Show();
        }

        //growth & MC related functions

        private void runGrowth()
        {
            Stopwatch stop = new Stopwatch();
            stop.Start();
            Boolean shouldBeEnded = false;
            switch (p.ProjectType)
            {
                case projectType.projVoronoi:
                    this.gg.oneStep(this.p);
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => this.img_MainDisplay.Source = MainWindow.convertToBitmapImage(this.p.ProjectImage)));
                    break;

                default:
                    while (!shouldBeEnded)
                    {
                        shouldBeEnded = true;
                        foreach (Cell c in p.ProjectCells)
                        {
                            if (!c.State)
                            {
                                shouldBeEnded = false;
                                break;
                            }
                        }
                        this.gg.oneStep(this.p);
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => this.img_MainDisplay.Source = MainWindow.convertToBitmapImage(this.p.ProjectImage)));
                    }
                    break;
            }
            this.p.IsGrown = true;
            System.Windows.MessageBox.Show("Simulation time: " + stop.Elapsed.ToString());
        }
        private void runMC()
        {
            Boolean isChanged = false;
            while (this.mc.IsActive && !this.p.IsMCFinished)
            {
                if (this.p.CurrentMCStep == this.p.FinalMCStep)
                {
                    this.p.IsMCFinished = true;
                    this.mc.IsActive = false;
                    System.Windows.MessageBox.Show("Monte Carlo has finished after " + this.p.CurrentMCStep + " steps. If you wish to continue the MC process, increase the number of steps in the Settings menu.", "Monte Carlo has finished", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                }
                isChanged = this.mc.oneMCStep(this.p);
                if (isChanged)
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => this.img_MainDisplay.Source = MainWindow.convertToBitmapImage(p.ProjectImage)));
                
                
            }
        }
        private void timerGrownEvent(object sender, EventArgs e)
        {
            if (this.p.IsGrown)
            {
                this.timerGrown.Stop();
                this.MenuItem_Sub_Run.IsEnabled = false;
                this.MenuItem_Sub_Restart.IsEnabled = true;
                if (this.p.IsMCEnabled)//if monte carlo is enabled
                    this.MenuItem_Sub_RunMC.IsEnabled = true;
                else // if monte carlo is disabled
                    this.MenuItem_Sub_RunMC.IsEnabled = false;
                this.mi_Sub_Sim_Settings.IsEnabled = true;
            }
        }
        private void timerMCLabelEvent(object sender, EventArgs e)
        {
            this.MenuItem_Sub_RunMC.Header = "Disable MC, finished steps: " + this.p.CurrentMCStep;
        }
        private void timerMCEndEvent(object sender, EventArgs e)
        {
            if(this.p.IsMCFinished)
            {
                this.timerMCLabels.Stop();
                this.timerMCEnd.Stop();
                this.MenuItem_RunMC_Click(null, null);
                this.MenuItem_Sub_RunMC.IsEnabled = false;                
            }
        }

        //other functions

        private void win_MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.slh.unsavedChangesPrompt(this.p) == 2)
                return;
            System.Windows.Application.Current.Shutdown();
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            this.Dispatcher.Invoke(() =>
            {
                using (Bitmap bmp = new Bitmap(this.p.ProjectPath + this.p.ProjectName + ".bmp"))
                {
                    this.p.ProjectImage = new Bitmap(bmp);
                    this.img_MainDisplay.Source = MainWindow.convertToBitmapImage(this.p.ProjectImage);
                }
                for (int i = 0; i < this.p.ImageX; i++)
                    for (int j = 0; j < this.p.ImageY; j++)
                    {
                        this.p.ProjectCells[i, j].R = this.p.ProjectImage.GetPixel(i, j).R;
                        this.p.ProjectCells[i, j].G = this.p.ProjectImage.GetPixel(i, j).G;
                        this.p.ProjectCells[i, j].B = this.p.ProjectImage.GetPixel(i, j).B;
                    } 
            });
            this.slh.wasContentChanged = true;
            
        }
        private void setControlsAvailability(Boolean isSimWindow)
        {
            //always make visible and enable!
            this.mi_Main_Sim.Visibility = Visibility.Visible;
            this.mi_Main_Other.Visibility = Visibility.Visible;
            this.mi_Sub_File_Save.Visibility = Visibility.Visible;

            this.MenuItem_Sub_Run.Visibility = Visibility.Visible;
            this.MenuItem_Sub_Restart.Visibility = Visibility.Visible;
            this.MenuItem_Sub_RunMC.Visibility = Visibility.Visible;

            this.mi_Sub_File_New.IsEnabled = true;
            this.mi_Sub_File_Save.IsEnabled = true;
            this.mi_Sub_File_Load.IsEnabled = true;
            this.mi_Main_Sim.IsEnabled = true;

            if (isSimWindow)//invoking from simulation window closing function
            {
                if (!this.sw.IsSettingsMenu)//is simwindow is not a settings-type simwindow
                {
                    if (this.p.ProjectType == projectType.projImported)//if project is imported
                    {
                        this.MenuItem_Sub_Run.IsEnabled = false;
                        this.MenuItem_Sub_Restart.IsEnabled = false;
                        this.mi_Sub_Sim_Grains.IsEnabled = false;
                        this.mi_Sub_Sim_Settings.IsEnabled = true;
                        if (this.p.IsMCEnabled)//if monte carlo is enabled
                            this.MenuItem_Sub_RunMC.IsEnabled = true;
                        else// if monte carlo is disabled
                            this.MenuItem_Sub_RunMC.IsEnabled = false;      
                    }
                    else if(!this.p.IsGrown && this.p.ProjectGrains.Count > 0)//if project is new but another one exists already AND it's NOT grown (cancel button press)
                    {
                        this.MenuItem_Sub_Run.IsEnabled = true;
                        this.MenuItem_Sub_Restart.IsEnabled = true;
                        this.mi_Sub_Sim_Grains.IsEnabled = true;
                        this.mi_Sub_Sim_Settings.IsEnabled = true;
                        this.MenuItem_Sub_RunMC.IsEnabled = false;
                    }
                    else if (this.p.IsGrown)//if project is new but another one exists already AND it's grown (cancel button press)
                    {
                        this.MenuItem_Sub_Run.IsEnabled = false;
                        this.MenuItem_Sub_Restart.IsEnabled = true;
                        this.mi_Sub_Sim_Grains.IsEnabled = true;
                        this.mi_Sub_Sim_Settings.IsEnabled = true;
                        if (this.p.IsMCEnabled)//if MC is enabled
                            this.MenuItem_Sub_RunMC.IsEnabled = true;
                        else // if MC is disabled
                            this.MenuItem_Sub_RunMC.IsEnabled = false;                          
                    }
                    else //if project is totally new
                    {
                        this.MenuItem_Sub_Run.IsEnabled = false;
                        this.MenuItem_Sub_Restart.IsEnabled = false;
                        this.mi_Sub_Sim_Grains.IsEnabled = true;
                        this.mi_Sub_Sim_Settings.IsEnabled = true;
                        this.MenuItem_Sub_RunMC.IsEnabled = false;
                    }
                }
                else //if this is a settings-type simwindow
                {
                    if(this.p.IsGrown)// if project is grown
                    {
                        if(this.p.IsMCFinished)//if MC is finished (
                        {
                            this.MenuItem_Sub_RunMC.IsEnabled = false;
                        }
                        else //if MC is NOT finished
                        {
                            if (this.p.IsMCEnabled)//if MC is enabled
                                this.MenuItem_Sub_RunMC.IsEnabled = true;
                            else // if MC is disabled
                                this.MenuItem_Sub_RunMC.IsEnabled = false;
                        }      
                    }
                    else
                        this.MenuItem_Sub_RunMC.IsEnabled = false;              
                }
            }
            else//invoking this function from loading function
            {
                //below line happens whichever project is loaded
                this.MenuItem_Sub_RunMC.Header = "Enable MC, finished steps: " + this.p.CurrentMCStep;               
                if (this.p.ProjectType == projectType.projImported) //if loaded project is imported
                {
                    this.mi_Sub_Sim_Grains.IsEnabled = false;
                    this.mi_Sub_Sim_Settings.IsEnabled = true;

                    this.MenuItem_Sub_Run.IsEnabled = false;
                    this.MenuItem_Sub_Restart.IsEnabled = false;
                    //if project has enabled MC
                    if(this.p.IsMCEnabled)
                    {
                        //if loaded project has finished MC
                        if (this.p.AllMCPoints.Count == 0 && this.p.IsMCFinished)
                            this.MenuItem_Sub_RunMC.IsEnabled = false;
                        else //project has NOT finished MC
                            this.MenuItem_Sub_RunMC.IsEnabled = true;
                    }
                    else //project has NOT enabled MC
                        this.MenuItem_Sub_RunMC.IsEnabled = false;             
                }
                else //if loaded project is graingrowth
                {              
                    this.mi_Sub_Sim_Settings.IsEnabled = true;
                    this.MenuItem_Sub_Restart.IsEnabled = true;

                    if (this.p.IsGrown) //if loaded project is grown
                    {
                        this.mi_Sub_Sim_Grains.IsEnabled = false;
                        this.MenuItem_Sub_Run.IsEnabled = false;
                        //if project has enabled MC
                        if (this.p.IsMCEnabled)
                        {
                            if(this.p.IsMCFinished)
                                this.MenuItem_Sub_RunMC.IsEnabled = false;
                            else //project has NOT finished MC
                                this.MenuItem_Sub_RunMC.IsEnabled = true;
                        }
                        else //project has NOT enabled MC
                            this.MenuItem_Sub_RunMC.IsEnabled = false;     
                    }
                    else //if loaded project is not yet grown
                    {
                        this.mi_Sub_Sim_Grains.IsEnabled = true;
                        this.MenuItem_Sub_RunMC.IsEnabled = false;
                        if (this.p.ProjectGrains.Count == 0)//if project does not contain grains
                        {
                            this.MenuItem_Sub_Restart.IsEnabled = false;
                            this.MenuItem_Sub_Run.IsEnabled = false;
                        }
                        else // if project contains grains
                        {
                            this.MenuItem_Sub_Restart.IsEnabled = true;
                            this.MenuItem_Sub_Run.IsEnabled = true;
                        }
                    }
                }
            }
        }
        private void displayLoadedImage()
        {
            if (this.p.ProjectImage.Width > this.Width)
                this.Width = 1.05 * this.p.ProjectImage.Width;
            if (this.p.ProjectImage.Height > this.Height)
                this.Height = 1.2 * this.p.ProjectImage.Height;
            this.img_MainDisplay.Width = this.p.ProjectImage.Width;
            this.img_MainDisplay.Height = this.p.ProjectImage.Height;
            this.img_MainDisplay.Source = MainWindow.convertToBitmapImage(this.p.ProjectImage);
        }
        public static BitmapImage convertToBitmapImage(Bitmap bmp)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

        //main menu functions

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            this.slh.wasContentChanged = false;
            this.slh.saveProject(this.p);
        }
        private void MenuItem_Load_Click(object sender, RoutedEventArgs e)
        {
            //cannot be inserted into SaveLoadHelper
            if (this.slh.unsavedChangesPrompt(this.p) == 2)
                return;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = "*.bin";
            dlg.Filter = "Binary files (.bin)|*.bin";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                this.p = this.slh.readFromBinaryFile<Project>(dlg.FileName);
                try
                {
                    using (Bitmap bmp = new Bitmap(this.p.ProjectPath + this.p.ProjectName + ".bmp"))
                    {
                        this.p.ProjectImage = new Bitmap(bmp);
                        this.displayLoadedImage();
                        this.setControlsAvailability(false);
                        this.watcher.Path = System.IO.Path.GetDirectoryName(this.p.ProjectPath + this.p.ProjectName);
                        this.watcher.Filter = System.IO.Path.GetFileName(this.p.ProjectPath + this.p.ProjectName + ".bmp");
                        this.watcher.EnableRaisingEvents = true;
                    }
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show(@"Could not load the project. Make sure the project is located in the _WORKSPACE\projects\[project name]\ folder.", "Load error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            this.slh.wasContentChanged = false;
        }
        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            if (this.slh.unsavedChangesPrompt(this.p) == 2)
                return;
            System.Windows.Application.Current.Shutdown();
        }

        //sub menu functions

        private void MenuItem_Restart_Click(object sender, RoutedEventArgs e)
        {
            this.slh.wasContentChanged = true;
            for (int i = 0; i < this.p.ImageX; i++)
                for (int j = 0; j < this.p.ImageY; j++)
                {
                    this.p.ProjectCells[i, j].initOrClear();
                    this.p.ProjectImage.SetPixel(i, j, Color.White);
                }        
            for (int i = 0; i < this.p.ProjectGrains.Count; i++)
            {
                this.p.ProjectCells[this.p.ProjectGrains[i].X, this.p.ProjectGrains[i].Y].setAsInitGrain(this.p.ProjectGrains[i].R, this.p.ProjectGrains[i].G, this.p.ProjectGrains[i].B);
                this.p.ProjectImage.SetPixel(this.p.ProjectGrains[i].X, this.p.ProjectGrains[i].Y, Color.FromArgb(255, this.p.ProjectGrains[i].R, this.p.ProjectGrains[i].G, this.p.ProjectGrains[i].B));
            }
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => this.img_MainDisplay.Source = MainWindow.convertToBitmapImage(this.p.ProjectImage)));
            this.p.CurrentMCStep = 0;
            if(this.p.IsMCEnabled)
                this.mc.resetMCLists(this.p);
            this.p.IsMCFinished = false;
            this.mc.IsActive = false;
            this.MenuItem_Sub_Run.IsEnabled = true;
            this.mi_Sub_Sim_Grains.IsEnabled = true;
            this.MenuItem_Sub_RunMC.IsEnabled = false;
            this.MenuItem_Sub_RunMC.Header = "Enable MC, finished steps: " + this.p.CurrentMCStep;
            this.p.IsGrown = false;
        }
        private void MenuItem_Run_Click(object sender, RoutedEventArgs e)
        {
            this.slh.wasContentChanged = true;
            this.MenuItem_Sub_Run.IsEnabled = false;
            this.MenuItem_Sub_Restart.IsEnabled = false;
            this.mi_Sub_Sim_Grains.IsEnabled = false;
            this.mi_Sub_Sim_Settings.IsEnabled = false;
            this.th = new Thread(runGrowth);
            this.th.Start();
            this.timerGrown.Start();
        }
        private void MenuItem_RunMC_Click(object sender, RoutedEventArgs e)
        {
            this.slh.wasContentChanged = true;
            if (th == null || !th.IsAlive)
            {
                if(this.p.IsMCEnabled)
                {
                    this.mc.IsActive = true;
                    if (!(this.p.ProjectType == projectType.projImported))
                        this.MenuItem_Sub_Restart.IsEnabled = false;
                    this.mi_Main_Sim.IsEnabled = false;
                    this.mi_Sub_File_New.IsEnabled = false;
                    this.mi_Sub_File_Save.IsEnabled = false;
                    this.mi_Sub_File_Load.IsEnabled = false;
                    this.MenuItem_Sub_RunMC.Header = "Disable MC, finished steps: " + this.p.CurrentMCStep;
                    this.th = new Thread(runMC);
                    this.timerMCLabels.Start();
                    this.timerMCEnd.Start();
                    th.Start();
                } 
            }
            else
            {
                if(this.p.IsMCEnabled)
                {
                    this.mc.IsActive = false;
                    if (!(this.p.ProjectType == projectType.projImported))
                        this.MenuItem_Sub_Restart.IsEnabled = true;
                    this.mi_Main_Sim.IsEnabled = true;
                    this.mi_Sub_File_New.IsEnabled = true;
                    this.mi_Sub_File_Save.IsEnabled = true;
                    this.mi_Sub_File_Load.IsEnabled = true;
                    this.timerMCLabels.Stop();
                    this.MenuItem_Sub_RunMC.Header = "Enable MC, finished steps: " + this.p.CurrentMCStep;
                }  
            }
        }
    }
}
