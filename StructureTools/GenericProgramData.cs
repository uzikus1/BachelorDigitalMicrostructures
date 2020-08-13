using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureTools
{
    [Serializable]
    public class GenericProgramData
    {
        private String fileName, filePath, fileExt;

        public String FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }
        public String FilePath
        {
            get { return this.filePath; }
            set { this.filePath = value; }
        }
        public String FileExt
        {
            get { return this.fileExt; }
            set { this.fileExt = value; }
        }

        public GenericProgramData(String s)
        {
            this.FilePath = s;
            this.FileName = System.IO.Path.GetFileNameWithoutExtension(s);
            this.FileExt = System.IO.Path.GetExtension(s);
        }
    }
}
