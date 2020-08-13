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
using System.Windows.Interop;
using System.Drawing;

namespace StructureTools
{
    /// <summary>
    /// Interaction logic for SimWindow.xaml
    /// </summary>
    public partial class SimWindow : Window
    {
        private Project pCopy;
        private Boolean isSettingsMenu, changedSimType, changedMCSteps, changedMCEnable, changedName, isImported;
        private StructureTools.ProgramWindow pw;
        private ImportHelper ih;
        public Project PCopy
        {
            get { return this.pCopy; }
            set { this.pCopy = value; }
        }
        public Boolean IsSettingsMenu
        {
            get { return this.isSettingsMenu; }
            set { this.isSettingsMenu = value; }
        }
        public Boolean ChangedSimType
        {
            get { return this.changedSimType; }
            set { this.changedSimType = value; }
        }
        public Boolean ChangedMCSteps
        {
            get { return this.changedMCSteps; }
            set { this.changedMCSteps = value; }
        }
        public Boolean ChangedMCEnable
        {
            get { return this.changedMCEnable; }
            set { this.changedMCEnable = value; }
        }
        public Boolean ChangedName
        {
            get { return this.changedName; }
            set { this.changedName = value; }
        }
        public Boolean IsImported
        {
            get { return this.isImported; }
            set { this.isImported = value; }
        }

        public SimWindow(Project p, Boolean settings)
        {
            InitializeComponent();
            this.IsSettingsMenu = settings;
            this.PCopy = p;
            this.IsImported = false;
            this.ChangedMCSteps = false;
            if (this.IsSettingsMenu)
            {
                this.setCheckedRadioButton((Int32)this.PCopy.ProjectType);
                this.Title = "Editing project '" + this.PCopy.ProjectName + "' settings";
                this.cbox_Periodic.IsChecked = this.PCopy.IsPeriodic;
                this.cbox_EnableMC.IsChecked = this.PCopy.IsMCEnabled;
                this.tbox_ProjName.Text = this.PCopy.ProjectName;
                this.tbox_AreaWidth.Text = this.PCopy.ImageX.ToString();
                if(this.PCopy.IsMCEnabled)
                    this.tbox_MCSteps.Text = this.PCopy.FinalMCStep.ToString();
                else
                    this.tbox_MCSteps.Text = "";
                this.tbox_AreaWidth.IsEnabled = false;
                this.tbox_AreaHeight.Text = this.PCopy.ImageY.ToString();
                this.tbox_AreaHeight.IsEnabled = false;
                this.btn_ClearImport.Visibility = Visibility.Hidden;
                this.btn_ImportExistingImage.Visibility = Visibility.Hidden;
                this.btn_ImportFromProgram.Visibility = Visibility.Hidden;
                if (this.PCopy.IsImported)
                {
                    this.img_PreviewDisplay.Source = MainWindow.convertToBitmapImage(this.PCopy.ProjectImage);
                    this.gbox_projectType.IsEnabled = false;
                }
            }
            this.ih = new ImportHelper();
        }

        //buttons
        private void btn_SimCancel_Click(object sender, RoutedEventArgs e)
        {
            ih.clearImport();
            this.Close();
        }
        private void btn_SimOK_Click(object sender, RoutedEventArgs e)
        {
            if(this.tbox_ProjName.Text == "")
            {
                System.Windows.MessageBox.Show("The value in the Name text field is missing.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Int32 type = findCheckedRadioButton();
            String name = this.tbox_ProjName.Text;
            Boolean periodic = Convert.ToBoolean(this.cbox_Periodic.IsChecked);
            Boolean enable = Convert.ToBoolean(this.cbox_EnableMC.IsChecked);
            Int32 mcSteps;
            if (this.isSettingsMenu) //if invoked from settings menu
            {
                //check if name was changed
                if (name != this.pCopy.ProjectName)
                {
                    this.pCopy.ProjectName = name;
                    this.pCopy.ProjectPath = @".\_WORKSPACE\projects\" + this.pCopy.ProjectName + @"\";
                    this.changedName = true;
                }
                else
                    this.changedName = false;
                    
                //check if type was changed
                if (type == (Int32)this.pCopy.ProjectType)
                    this.changedSimType = false;
                else
                {
                    this.changedSimType = true;
                    this.pCopy.ProjectType = (projectType)type;
                }
                
                //change periodic according to checkbox
                this.pCopy.IsPeriodic = periodic;

                //check if mc enable was changed
                if (enable != this.pCopy.IsMCEnabled)
                {
                    this.changedMCEnable = true;
                    if(enable)//if was changed and enabled
                    {
                        //CHECK MC STEPS
                        try
                        {
                            mcSteps = Convert.ToInt32(this.tbox_MCSteps.Text);
                            if (mcSteps > 0)
                            {
                                if (mcSteps > this.pCopy.CurrentMCStep)
                                {
                                    if (mcSteps != this.pCopy.FinalMCStep)
                                    {
                                        this.ChangedMCSteps = true;
                                        pCopy.enableMC(mcSteps);
                                    }
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("The value in the MC Steps cannot be lower or equal to the current step value, which is " + this.pCopy.CurrentMCStep + ".", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("The value in the MC Steps text field cannot be lower or equal to 0.", "Input errror", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }         
                        }
                        catch (Exception)
                        {
                            System.Windows.MessageBox.Show("The value in the MC Steps text field is incorrect or missing.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else //if was changed and disabled
                    {
                        this.pCopy.disableMC();
                        this.changedMCEnable = false;
                    }
                }
                else //if enable wasn't changed but steps may have been
                {
                    if(enable)
                    {
                        try
                        {
                            mcSteps = Convert.ToInt32(this.tbox_MCSteps.Text);
                            if(mcSteps != this.PCopy.FinalMCStep)
                            {
                                if (mcSteps > 0)
                                {
                                    if (mcSteps > this.pCopy.CurrentMCStep /*&& this.pCopy.CurrentMCStep == this.pCopy.FinalMCStep*/)
                                    {
                                        if (mcSteps != this.pCopy.FinalMCStep)
                                        {
                                            this.ChangedMCSteps = true;
                                            this.PCopy.FinalMCStep = mcSteps;
                                        }
                                    }
                                    else
                                    {
                                        System.Windows.MessageBox.Show("The value in the MC Steps cannot be lower or equal to the current step value, which is " + this.pCopy.CurrentMCStep + ".", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("The value in the MC Steps text field cannot be lower or equal to 0.", "Input errror", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            System.Windows.MessageBox.Show("The value in the MC Steps text field is incorrect or missing.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }               
            }
            else //if project is new
            {
                Int32 x, y;
                try
                {
                    x = Convert.ToInt32(this.tbox_AreaWidth.Text);
                    y = Convert.ToInt32(this.tbox_AreaHeight.Text);
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("The value in the Width and/or Height text field is incorrect or missing.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (enable)
                {
                    try
                    {
                        if (Convert.ToInt32(this.tbox_MCSteps.Text) > 0)
                        {
                            mcSteps = Convert.ToInt32(this.tbox_MCSteps.Text);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("The value in the MC Steps text field cannot be lower or equal to 0.", "Input errror", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        System.Windows.MessageBox.Show("The value in the MC Steps text field is incorrect or missing.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    mcSteps = 0;
                }
                if (this.isImported)
                    this.pCopy = new Project(name, x, y, type, this.isImported, ih.ImportPath, periodic, enable, mcSteps);
                else
                    this.pCopy = new Project(name, x, y, type, this.isImported, "", periodic, enable, mcSteps);
            }
            this.pCopy.IsLoaded = true;
            this.Close();
        }
        private void btn_ImportExistingImage_Click(object sender, RoutedEventArgs e)
        {
            ih.clearImport();
            ih.ImportPath = ih.openImportDialog();
            Bitmap bmp = ih.importImageFromFile(ih.ImportPath);
            if (bmp.Width == 1 && bmp.Height == 1)
            {
                ih.clearImport();
                return;
            }
            this.isImported = true;
            this.tbox_AreaWidth.Text = bmp.Width.ToString();
            this.tbox_AreaWidth.IsEnabled = false;
            this.tbox_AreaHeight.Text = bmp.Height.ToString();
            this.tbox_AreaHeight.IsEnabled = false;
            this.gbox_projectType.IsEnabled = false;
            this.img_PreviewDisplay.Visibility = Visibility.Visible;
            this.img_PreviewDisplay.Source = MainWindow.convertToBitmapImage(bmp);
            this.setCheckedRadioButton((Int32)projectType.projImported);
        }
        private void btn_ImportFromProgram_Click(object sender, RoutedEventArgs e)
        {
            pw = new StructureTools.ProgramWindow(@".\_WORKSPACE\apps\dmr", "DMR applications", true);
            pw.Show();
            pw.Closed += new EventHandler(this.ProgramWindow_Closed);
        }
        private void btn_ClearImport_Click(object sender, RoutedEventArgs e)
        {
            ih.clearImport();
            this.img_PreviewDisplay.Source = MainWindow.convertToBitmapImage(new Bitmap(100, 100));
            this.img_PreviewDisplay.Visibility = Visibility.Hidden;
            this.tbox_AreaWidth.Text = "";
            this.tbox_AreaWidth.IsEnabled = true;
            this.tbox_AreaHeight.Text = "";
            this.tbox_AreaHeight.IsEnabled = true;
            this.gbox_projectType.IsEnabled = true;
        }

        //misc
        private Int32 findCheckedRadioButton()
        {
            //this is really shameful
            if (Convert.ToBoolean(this.rad_1.IsChecked))
                return 1;
            else if (Convert.ToBoolean(this.rad_2.IsChecked))
                return 2;
            else if (Convert.ToBoolean(this.rad_3.IsChecked))
                return 3;
            else if (Convert.ToBoolean(this.rad_4.IsChecked))
                return 4;
            else if (Convert.ToBoolean(this.rad_5.IsChecked))
                return 5;
            else if (Convert.ToBoolean(this.rad_6.IsChecked))
                return 6;
            else if (Convert.ToBoolean(this.rad_7.IsChecked))
                return 7;
            else return 8;
        }
        private void setCheckedRadioButton(Int32 value)
        {
            //god forgive me for i have sinned
            if (value == 1)
                this.rad_1.IsChecked = true;
            else if (value == 2)
                this.rad_2.IsChecked = true;
            else if (value == 3)
                this.rad_3.IsChecked = true;
            else if (value == 4)
                this.rad_4.IsChecked = true;
            else if (value == 5)
                this.rad_5.IsChecked = true;
            else if (value == 6)
                this.rad_6.IsChecked = true;
            else if (value == 7)
                this.rad_7.IsChecked = true;
            else if (value == 8)
                this.rad_8.IsChecked = true;
        }

        //import     
        private void ProgramWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                if (this.pw.pr.HasExited)
                    btn_ImportExistingImage_Click(null, null);
            }
            catch (Exception) { }
        }
        private void cbox_EnableMC_Checked(object sender, RoutedEventArgs e)
        {
            this.tbox_MCSteps.IsEnabled = true;
        }
        private void cbox_EnableMC_Unchecked(object sender, RoutedEventArgs e)
        {
            this.tbox_MCSteps.IsEnabled = false;
        }
    }
}
