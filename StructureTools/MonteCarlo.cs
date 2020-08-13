using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StructureTools
{
    class MonteCarlo
    {
        private Boolean isActive;
        private Boolean[] neighborhood;
        public Boolean IsActive
        {
            get { return this.isActive; }
            set { this.isActive = value; }
        }

        public MonteCarlo()
        {
            this.isActive = false;
            this.neighborhood = new Boolean[]{
                            true, true, true,
                            true, false, true,
                            true, true, true}; //Moore
        }
        public Boolean oneMCStep(Project p)
        {
            //randomize x,y
            Random rand = new Random();
            System.Windows.Point moveable = p.AllMCPoints[rand.Next(p.AllMCPoints.Count)];
            Int32 x = Convert.ToInt32(moveable.X);
            Int32 y = Convert.ToInt32(moveable.Y);
            //get all the colors in a 3x3 neighborhood
            List<Color> tempColors = new List<Color>();
            List<Color> distinctColors = new List<Color>();
            for (int m = x - 1; m < x + 2; m++)
            {
                for (int n = y - 1; n < y + 2; n++)
                {
                    if (p.IsPeriodic)
                        tempColors.Add(Color.FromArgb(255, p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].R, p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].G, p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].B));
                    else if (m >= 0 && m < p.ImageX && n >= 0 && n < p.ImageY)
                        tempColors.Add(Color.FromArgb(255, p.ProjectCells[m, n].R, p.ProjectCells[m, n].G, p.ProjectCells[m, n].B));                  
                }
            }
            //get distinct colors
            distinctColors = tempColors.Distinct().ToList();
            //check if on border (1 distinct color = not on border)
            Boolean onBorder;
            if (distinctColors.Count == 1)
                onBorder = false;
            else
                onBorder = true;
            distinctColors.Clear();
            p.AllMCPoints.Remove(moveable);
            p.CheckedMCPoints.Add(moveable);
            if (p.AllMCPoints.Count == 0)
            {
                p.CurrentMCStep++;
                if (p.CurrentMCStep == p.FinalMCStep + 1)
                    return false;
                foreach (System.Windows.Point pt in p.CheckedMCPoints)
                    p.AllMCPoints.Add(pt);
                p.CheckedMCPoints.Clear();
            }
            //if rand x,y on border, continue
            if (onBorder)
            {
                Int32 iter = 0;
                Int32 preEnergy, postEnergy;
                preEnergy = postEnergy = 8;
                //calculate current energy
                for (int m = x - 1; m < x + 2; m++)
                {
                    for (int n = y - 1; n < y + 2; n++)
                    {
                        if(p.IsPeriodic)
                        {
                            if (this.neighborhood[iter])
                                if (p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].R == p.ProjectCells[x, y].R
                                        && p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].G == p.ProjectCells[x, y].G
                                        && p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].B == p.ProjectCells[x, y].B)
                                    preEnergy--;
                        }
                        else 
                        {
                            if (m >= 0 && m < p.ImageX && n >= 0 && n < p.ImageY)
                                if (this.neighborhood[iter])
                                    if (p.ProjectCells[m, n].R == p.ProjectCells[x, y].R
                                        && p.ProjectCells[m, n].G == p.ProjectCells[x, y].G
                                        && p.ProjectCells[m, n].B == p.ProjectCells[x, y].B)
                                        preEnergy--;                         
                        }          
                        iter++;
                    }
                }
                iter = 0;

                //select new color
                Color newColor = tempColors[rand.Next(0, tempColors.Count)];
                tempColors.Clear();

                //calculate new energy
                for (int m = x - 1; m <x + 2; m++)
                {
                    for (int n = y - 1; n < y + 2; n++)
                    {
                        if(p.IsPeriodic)
                        {
                            if (this.neighborhood[iter])
                                if (newColor.R == p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].R
                                        && newColor.G == p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].G
                                        && newColor.B == p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].B)
                                    postEnergy--;
                        }
                        else
                        {
                            if (m >= 0 && m < p.ImageX && n >= 0 && n < p.ImageY)
                                if (this.neighborhood[iter])
                                    if (newColor.R == p.ProjectCells[m, n].R
                                        && newColor.G == p.ProjectCells[m, n].G
                                        && newColor.B == p.ProjectCells[m, n].B)
                                        postEnergy--;
                        }                       
                        iter++;
                    }
                }
                iter = 0;

                //check if color should flip
                if(postEnergy <= preEnergy)
                {
                    p.ProjectCells[x, y].R = newColor.R;
                    p.ProjectCells[x, y].G = newColor.G;
                    p.ProjectCells[x, y].B = newColor.B;
                    p.ProjectImage.SetPixel(x, y, Color.FromArgb(newColor.R, newColor.G, newColor.B));
                    return true;
                }
                preEnergy = 8;
                postEnergy = 8;
            }
            tempColors.Clear();
            return false;
        }
        public void resetMCLists(Project p)
        {
            foreach (System.Windows.Point pt in p.CheckedMCPoints)
                p.AllMCPoints.Add(pt);
            p.CheckedMCPoints.Clear();
        }
        private Int32 mod(Int32 x, Int32 m)
        {
            return ((x % m) + m) % m;
        }
    }
}
