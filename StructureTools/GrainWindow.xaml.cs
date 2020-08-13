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
using Xceed.Wpf.Toolkit;

namespace StructureTools
{
    /// <summary>
    /// Interaction logic for GrainWindow.xaml
    /// </summary>
    public partial class GrainWindow : Window
    {
        private Project pCopy;
        private Boolean isImageClickable, wasListModified;
        private Random rand;

        public Project PCopy
        {
            get { return this.pCopy; }
            set { this.pCopy = value; }
        }
        public Boolean IsImageClickable
        {
            get { return this.isImageClickable; }
            set { this.isImageClickable = value; }
        }
        public Boolean WasListModified
        {
            get { return this.wasListModified; }
            set { this.wasListModified = value; }
        }

        public GrainWindow(Project p)
        {
            InitializeComponent();
            this.pCopy = p;
            this.isImageClickable = false;
            this.wasListModified = false;
            for (int i = 0; i < pCopy.ProjectGrains.Count; i++)
            {
                String displayLabel = "X = " + this.pCopy.ProjectGrains[i].X + ", Y = " + this.pCopy.ProjectGrains[i].Y + ", C = " + this.pCopy.ProjectGrains[i].HexLabel;
                this.lbox_GrainsList.Items.Add(displayLabel);
            }
            this.lab_GrainCount.Content = this.pCopy.ProjectGrains.Count.ToString();
            this.rand = new Random();
        }

        //buttons
        private void btn_GrainAdd_Click(object sender, RoutedEventArgs e)
        {
            
            if(Convert.ToBoolean(this.rad_Input_Manual.IsChecked)) //input a grain manually
            {
                Int32 x,y;
                try
                {
                    x = Convert.ToInt32(this.tbox_GrainX.Text);
                    y = Convert.ToInt32(this.tbox_GrainY.Text);
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("The value in the X and/or Y text field is incorrect or missing.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (!doesColorExist(new byte[] { this.colc_Picker.R, this.colc_Picker.G, this.colc_Picker.B }))
                    if (!doesPointExistWithinArea(x, y))
                        if (!doesPointExistOnList(x, y, -1))
                        {
                            this.addNewGrain(
                                x,
                                y,
                                this.colc_Picker.R,
                                this.colc_Picker.G,
                                this.colc_Picker.B,
                                false);
                            String displayLabel = "X = " + this.pCopy.ProjectGrains[this.pCopy.ProjectGrains.Count - 1].X + ", Y = " + this.pCopy.ProjectGrains[this.pCopy.ProjectGrains.Count - 1].Y + ", C = " + this.pCopy.ProjectGrains[this.pCopy.ProjectGrains.Count - 1].HexLabel;
                            this.lbox_GrainsList.Items.Add(displayLabel);
                            this.tbox_GrainX.Text = "";
                            this.tbox_GrainY.Text = "";
                            this.colc_Picker.HexadecimalString = "#FFFFFFFF";
                            this.wasListModified = true;
                        }
                        else
                            System.Windows.MessageBox.Show("This point already exists, select another.", "Point Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        System.Windows.MessageBox.Show("This point is out of bounds, select another.", "Bounds Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    System.Windows.MessageBox.Show("This color already exists or selected color is white (#FFFFFF), select another.", "Color Warning", MessageBoxButton.OK, MessageBoxImage.Error);   
            }
            else if(Convert.ToBoolean(this.rad_Input_Random.IsChecked)) //input a # of random grains
            {
                Int32 batchCount, noOfTries;
                try
                {
                    batchCount = Convert.ToInt32(this.tbox_RandGrain.Text);
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("The value in the Random text field is incorrect or missing.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Random rand = new Random();
                int x, y;
                byte r, g, b;
                System.Drawing.Color c;
                if (batchCount + this.pCopy.ProjectGrains.Count > this.pCopy.ImageX * this.pCopy.ImageY)
                {
                    System.Windows.MessageBox.Show("The number of grains exceeds the area size.", "Input error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                for(int i=0; i<batchCount;i++)
                {
                    noOfTries = 0;
                    do
                    {
                        if (noOfTries > batchCount * batchCount)
                        {
                            System.Windows.MessageBox.Show("The number of tries exceeded the maximum number.", "Random error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        x = rand.Next(0, this.pCopy.ImageX);
                        y = rand.Next(0, this.pCopy.ImageY);
                        r = Convert.ToByte(rand.Next(0, 255));
                        g = Convert.ToByte(rand.Next(0, 255));
                        b = Convert.ToByte(rand.Next(0, 255));
                        c = System.Drawing.Color.FromArgb(255, r, g, b);
                        noOfTries++;
                    }
                    while (this.doesPointExistOnList(x, y, -1) || this.doesPointExistWithinArea(x, y) || this.doesColorExist(new byte[] { r, g, b }));
                    this.addNewGrain(x, y, r, g, b, false);
                    this.wasListModified = true;
                    String displayLabel = "X = " + this.pCopy.ProjectGrains[this.pCopy.ProjectGrains.Count - 1].X + ", Y = " + this.pCopy.ProjectGrains[this.pCopy.ProjectGrains.Count - 1].Y + ", C = " + this.pCopy.ProjectGrains[this.pCopy.ProjectGrains.Count - 1].HexLabel;
                    this.lbox_GrainsList.Items.Add(displayLabel);
                    if (this.pCopy.ProjectGrains.Count == this.pCopy.ImageX * this.pCopy.ImageY)
                    {
                        System.Windows.MessageBox.Show("Maximum grain count reached.", "Max grain count", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
            }
        } 
        private void btn_GrainEdit_Click(object sender, RoutedEventArgs e)
        {
            int selected = this.lbox_GrainsList.Items.IndexOf(lbox_GrainsList.SelectedItem.ToString());
            if (!doesColorExist(new byte[] {this.colc_Picker.R, this.colc_Picker.G, this.colc_Picker.B}))
                if (!doesPointExistWithinArea(Convert.ToInt32(this.tbox_GrainX.Text), Convert.ToInt32(this.tbox_GrainY.Text)))
                    if (!doesPointExistOnList(Convert.ToInt32(this.tbox_GrainX.Text), Convert.ToInt32(this.tbox_GrainY.Text), selected))
                    {
                        this.pCopy.ProjectCells[this.pCopy.ProjectGrains[selected].X, this.pCopy.ProjectGrains[selected].Y].initOrClear(); //delete old grain from field
                        this.pCopy.ProjectImage.SetPixel(this.pCopy.ProjectGrains[selected].X, this.pCopy.ProjectGrains[selected].Y, System.Drawing.Color.White);
                        this.pCopy.ProjectGrains[selected].modifyGrain( //modify grain list
                            Convert.ToInt32(this.tbox_GrainX.Text),
                            Convert.ToInt32(this.tbox_GrainY.Text),
                            this.colc_Picker.R,
                            this.colc_Picker.G,
                            this.colc_Picker.B);
                        addNewGrain(Convert.ToInt32(this.tbox_GrainX.Text), //add new grain to field
                            Convert.ToInt32(this.tbox_GrainY.Text),
                            this.colc_Picker.R,
                            this.colc_Picker.G,
                            this.colc_Picker.B,
                            true);
                        String displayLabel = "X = " + this.pCopy.ProjectGrains[selected].X + ", Y = " + this.pCopy.ProjectGrains[selected].Y + ", C = " + this.pCopy.ProjectGrains[selected].HexLabel;
                        this.lbox_GrainsList.Items[selected] = displayLabel;
                        this.btn_GrainEdit.IsEnabled = false;
                        this.btn_GrainDelete.IsEnabled = false;
                        this.tbox_GrainX.Text = "";
                        this.tbox_GrainY.Text = "";
                        this.wasListModified = true;
                    }
                    else
                        System.Windows.MessageBox.Show("This point already exists, select another.", "Point Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    System.Windows.MessageBox.Show("This point is out of bounds, select another.", "Bounds Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                System.Windows.MessageBox.Show("This color already exists or selected color is white (#FFFFFF), select another.", "Color Warning", MessageBoxButton.OK, MessageBoxImage.Error);  
        }
        private void btn_GrainDelete_Click(object sender, RoutedEventArgs e)
        {
            int selected = this.lbox_GrainsList.Items.IndexOf(this.lbox_GrainsList.SelectedItem.ToString());
            this.pCopy.ProjectImage.SetPixel(this.pCopy.ProjectGrains[selected].X, this.pCopy.ProjectGrains[selected].Y, System.Drawing.Color.White);
            this.pCopy.ProjectCells[this.pCopy.ProjectGrains[selected].X, this.pCopy.ProjectGrains[selected].Y].initOrClear();
            this.pCopy.ProjectGrains.RemoveAt(selected);
            this.lbox_GrainsList.Items.RemoveAt(selected);
            this.btn_GrainEdit.IsEnabled = false;
            this.btn_GrainDelete.IsEnabled = false;
            this.tbox_GrainX.Text = "";
            this.tbox_GrainY.Text = "";
            this.colc_Picker.HexadecimalString = "#FFFFFFFF";
            this.lab_GrainCount.Content = this.pCopy.ProjectGrains.Count.ToString();
            this.wasListModified = true;
        }
        private void btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            this.pCopy.ProjectGrains.Clear();
            this.lbox_GrainsList.Items.Clear();
            for (int i = 0; i < this.pCopy.ImageX; i++)
                for (int j = 0; j < this.pCopy.ImageY; j++)
                {
                    this.pCopy.ProjectImage.SetPixel(i, j, System.Drawing.Color.White);
                    this.pCopy.ProjectCells[i, j].initOrClear();
                }
            this.btn_GrainEdit.IsEnabled = false;
            this.btn_GrainDelete.IsEnabled = false;
            this.tbox_GrainX.Text = "";
            this.tbox_GrainY.Text = "";
            this.colc_Picker.HexadecimalString = "#FFFFFFFF";
            this.lab_GrainCount.Content = this.pCopy.ProjectGrains.Count.ToString();
            this.wasListModified = true;
        }
        private void btn_Done_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btn_RandomColor_Click(object sender, RoutedEventArgs e)
        {
            do
            {
                this.colc_Picker.HexadecimalString = "#" + BitConverter.ToString(new Byte[] { 255, Convert.ToByte(this.rand.Next(0, 255)), Convert.ToByte(this.rand.Next(0, 255)), Convert.ToByte(this.rand.Next(0, 255)) }).Replace("-", String.Empty);
            }
            while (this.colc_Picker.HexadecimalString == "#FFFFFFFF");
        }

        //boundary and existence checks
        public Boolean doesColorExist(Byte[] b)
        {
            if(b[0] == 255 && b[0] == 255 && b[1] == 255)
                return true; //white cannot be used
            else
            {
                for(int i = 0; i<this.PCopy.ProjectGrains.Count;i++)
                    if (b[0] == this.PCopy.ProjectGrains[i].R && b[1] == this.PCopy.ProjectGrains[i].G && b[2] == this.PCopy.ProjectGrains[i].B) 
                        return true;
            }
            return false;
        }
        public Boolean doesPointExistWithinArea(Int32 x, Int32 y)
        {
            if (x < 0 || x >= this.PCopy.ImageX || y < 0 || y >= this.PCopy.ImageY)
                return true;
            return false;
        }
        public Boolean doesPointExistOnList(Int32 x, Int32 y, Int32 currentIndex)
        { //index is specifically used for editing grain on a list - for cases where X,Y persists, while color changes
            for (int i = 0; i < this.PCopy.ProjectGrains.Count; i++)
                if (this.PCopy.ProjectGrains[i].X == x && this.PCopy.ProjectGrains[i].Y == y)
                    if(i != currentIndex)
                        return true;
            return false;
        }

        //other functions
        private void rad_Input_Click_Checked(object sender, RoutedEventArgs e)
        {
            this.isImageClickable = true;
        }
        private void rad_Input_Click_Unchecked(object sender, RoutedEventArgs e)
        {
            this.isImageClickable = false;
        }
        private void lbox_GrainsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int selected = this.lbox_GrainsList.Items.IndexOf(this.lbox_GrainsList.SelectedItem.ToString());
                this.tbox_GrainX.Text = this.pCopy.ProjectGrains[selected].X.ToString();
                this.tbox_GrainY.Text = this.pCopy.ProjectGrains[selected].Y.ToString();
                this.colc_Picker.A = this.pCopy.ProjectGrains[selected].A;
                this.colc_Picker.R = this.pCopy.ProjectGrains[selected].R;
                this.colc_Picker.G = this.pCopy.ProjectGrains[selected].G;
                this.colc_Picker.B = this.pCopy.ProjectGrains[selected].B;
                this.btn_GrainEdit.IsEnabled = true;
                this.btn_GrainDelete.IsEnabled = true;
            }
            catch (Exception) { }
        }
        public void addNewGrain(Int32 x, Int32 y, Byte r, Byte g, Byte b, Boolean isEdited)
        {
            this.PCopy.ProjectCells[x, y].setAsInitGrain(r, g, b);
            if (isEdited == false)
                this.PCopy.ProjectGrains.Add(new Grain(x, y, r, g, b));
            System.Drawing.Color c;
            c = System.Drawing.Color.FromArgb(255, r, g, b);
            this.PCopy.ProjectImage.SetPixel(x, y, c);
            this.lab_GrainCount.Content = this.PCopy.ProjectGrains.Count.ToString();
        }
    }
}
