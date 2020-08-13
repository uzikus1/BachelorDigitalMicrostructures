using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureTools
{
    [Serializable]
    public class Cell
    {
        private Boolean state, nstate;
        private Byte nr, ng, nb, r, g, b, a;

        public Boolean State
        {
            get { return this.state; }
            set { this.state = value; }
        }
        public Boolean NState
        {
            get { return this.nstate; }
            set { this.nstate = value; }
        }
        public Byte nR 
        {
            get { return this.nr; }
            set { this.nr = value; }
        }
        public Byte nG 
        {
            get { return this.ng; }
            set { this.ng = value; }
        }
        public Byte nB 
        {
            get { return this.nb; }
            set { this.nb = value; }
        }
        public Byte R 
        {
            get { return this.r; }
            set { this.r = value; }
        }
        public Byte G 
        {
            get { return this.g; }
            set { this.g = value; }
        }
        public Byte B 
        {
            get { return this.b; }
            set { this.b = value; }
        }
        public Byte A
        {
            get { return this.a; }
            set { this.a = value; }
        }

        public Cell() 
        {
            this.A = 255;
            this.initOrClear();
        }
        public void initOrClear()
        {
            this.State = this.NState = false;
            this.R = this.nR = System.Drawing.Color.White.R;
            this.G = this.nG = System.Drawing.Color.White.G;
            this.B = this.nB = System.Drawing.Color.White.B;
        }
        public void setAsInitGrain(Byte r, Byte g, Byte b)
        {
            this.R = this.nR = r;
            this.G = this.nG = g;
            this.B = this.nB = b;
            this.State = this.NState = true;
        }
    }

   
}
