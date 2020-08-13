using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StructureTools
{
    class SaveLoadHelper
    {
        public Boolean wasContentChanged;
        private Boolean WasContentChanged
        {
            get { return this.wasContentChanged; }
            set { this.wasContentChanged = value; }
        }

        public SaveLoadHelper()
        {
            this.WasContentChanged = false;
        }
        public Int32 unsavedChangesPrompt(Project p)
        {
            // 1 - will save
            // 0 - will not save
            // 2 - cancel
            if (this.wasContentChanged) //if any content-changing action was taken
            {
                var result = System.Windows.MessageBox.Show("You have unsaved changes. Would you like to save?", "Unsaved changes", System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Warning);
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    //this.MenuItem_Save_Click(null, null);
                    this.saveProject(p);
                    return 1;
                }
                else if (result == System.Windows.MessageBoxResult.No)
                    return 0;
                else
                    return 2;
            }
            else return 0;
        }
        public void saveProject(Project p)
        {
            System.IO.Directory.CreateDirectory(p.ProjectPath);
            using (Bitmap bmp = new Bitmap(p.ProjectImage))
            {
                bmp.Save(p.ProjectPath + p.ProjectName + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                this.writeToBinaryFile<Project>(p.ProjectPath + p.ProjectName + ".bin", p);
                System.Windows.MessageBox.Show("The file has been saved to " + p.ProjectPath);
            }
            this.wasContentChanged = false;
        }
        private void writeToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = System.IO.File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }
        public T readFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = System.IO.File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
    }
}
