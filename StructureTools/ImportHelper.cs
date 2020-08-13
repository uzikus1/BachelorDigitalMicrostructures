using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureTools
{
    class ImportHelper
    {
        private String importPath;
        public String ImportPath
        {
            get { return this.importPath; }
            set { this.importPath = value; }
        }

        public String openImportDialog()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = "*.bmp";
            dlg.Filter = "BMP Files (*.bmp)|*.bmp";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
                return dlg.FileName;
            else return null;
        }
        public Bitmap importImageFromFile(String s)
        {
            if (s == null)
                return new Bitmap(1, 1); //cannot return null Bitmap, nobody sane will create 1x1 DMR Bitmap
            Bitmap bmp = new Bitmap(s);
            return bmp;
        }
        public void clearImport()
        {
            this.ImportPath = "";
        }
    }
}
