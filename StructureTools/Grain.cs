using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StructureTools
{
    [Serializable]
    public class Grain : Cell
    {
        private Int32 x, y;
        private String hexLabel;

        public Int32 X
        {
            get { return this.x; }
            set { this.x = value; }
        }
        public Int32 Y
        {
            get { return this.y; }
            set { this.y = value; }
        }
        public String HexLabel
        {
            get { return this.hexLabel; }
            set { this.hexLabel = value; }
        }

        public Grain(Int32 x, Int32 y, Byte r, Byte g, Byte b)
        {
            this.X = x;
            this.Y = y;
            this.R = r;
            this.G = g;
            this.B = b;
            this.State = this.NState = true;
            this.HexLabel = "#" + BitConverter.ToString(new Byte[] { this.A, this.R, this.G, this.B }).Replace("-", String.Empty);
        }
        public void modifyGrain(int x, int y, Byte r, Byte g, Byte b)
        {
            this.X = x;
            this.Y = y;
            this.R = r;
            this.G = g;
            this.B = b;
            this.HexLabel = "#" + BitConverter.ToString(new Byte[] { this.A, this.R, this.G, this.B }).Replace("-", String.Empty);
        }
    }
}
