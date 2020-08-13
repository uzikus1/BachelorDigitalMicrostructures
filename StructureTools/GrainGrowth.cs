using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace StructureTools
{
    class GrainGrowth
    {
        public void oneStep(Project p)
        {
            Boolean[] neighborhood = this.selectNeighborhood((Int32)p.ProjectType);
            if (neighborhood.Length == 1) //voronoi
            {
                Parallel.For(0, p.ImageX, i =>
                {
                    Double regularLength, periodicLength, shortestLength = Double.MaxValue;
                    Int32 shortestP = 0;
                    for (int j = 0; j < p.ImageY; j++)
                    {
                        if (!p.ProjectCells[i, j].State)
                        {
                            for (int m = 0; m < p.ProjectGrains.Count; m++)
                            {
                                Point p1 = new Point(p.ProjectGrains[m].X, p.ProjectGrains[m].Y);
                                Point p2 = new Point(i, j);
                                regularLength = calculateLength(p1, p2);
                                if (p.IsPeriodic)
                                {
                                    periodicLength = calculateMinPeriodVoronoi(p1, p2, p.ImageX, p.ImageY);
                                    if (periodicLength < regularLength)
                                        regularLength = periodicLength;
                                }
                                if (regularLength < shortestLength)
                                {
                                    shortestLength = regularLength;
                                    shortestP = m;
                                }
                            }
                            p.ProjectCells[i, j].State = p.ProjectCells[i, j].NState = true;
                            p.ProjectCells[i, j].R = p.ProjectCells[i, j].nR = p.ProjectGrains[shortestP].R;
                            p.ProjectCells[i, j].G = p.ProjectCells[i, j].nG = p.ProjectGrains[shortestP].G;
                            p.ProjectCells[i, j].B = p.ProjectCells[i, j].nB = p.ProjectGrains[shortestP].B;
                            shortestLength = Double.MaxValue;
                        }
                    }
                });
                for (int i = 0; i < p.ImageX; i++)
                    for (int j = 0; j < p.ImageY; j++)
                        p.ProjectImage.SetPixel(i, j, Color.FromArgb(255, p.ProjectCells[i, j].R, p.ProjectCells[i, j].G, p.ProjectCells[i, j].B));
            }
            else //CA
            {
                Parallel.For(0, p.ImageX, i =>
                {
                    List<System.Drawing.Color> tempColors = new List<System.Drawing.Color>();
                    Int32 iter = 0;
                        for (int j = 0; j < p.ImageY; j++)
                            if (!p.ProjectCells[i, j].State)
                            {
                                for (int m = i - 1; m <= i + 1; m++)
                                    for (int n = j - 1; n <= j + 1; n++)
                                    {
                                        if (p.IsPeriodic)
                                        {
                                            if (neighborhood[iter])
                                                if (p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].State)
                                                    tempColors.Add(System.Drawing.Color.FromArgb(255,   
                                                        p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].R,                                                                                                         
                                                        p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].G,                                                                                                      
                                                        p.ProjectCells[this.mod(m, p.ImageX), this.mod(n, p.ImageY)].B));
                                        }
                                        else
                                        {
                                            if (m >= 0 && m < p.ImageX && n >= 0 && n < p.ImageY)
                                                if (neighborhood[iter])
                                                    if (p.ProjectCells[m, n].State)
                                                        tempColors.Add(System.Drawing.Color.FromArgb(255, 
                                                            p.ProjectCells[m, n].R, 
                                                            p.ProjectCells[m, n].G, 
                                                            p.ProjectCells[m, n].B));
                                        }
                                        iter++;
                                    }
                                tempColors.RemoveAll(c => c.Equals(System.Drawing.Color.White));
                                if (Convert.ToBoolean(tempColors.Count))
                                {
                                    System.Drawing.Color mode = tempColors.GroupBy(v => v).OrderByDescending(g => g.Count()).First().Key;
                                    p.ProjectCells[i, j].NState = true;
                                    p.ProjectCells[i, j].nR = mode.R;
                                    p.ProjectCells[i, j].nG = mode.G;
                                    p.ProjectCells[i, j].nB = mode.B;
                                }
                                tempColors.Clear();
                                iter = 0;
                            }
                });
                for (int i = 0; i < p.ImageX; i++)
                    for (int j = 0; j < p.ImageY; j++)
                    {
                        p.ProjectCells[i, j].State = p.ProjectCells[i, j].NState;
                        p.ProjectCells[i, j].R = p.ProjectCells[i, j].nR;
                        p.ProjectCells[i, j].G = p.ProjectCells[i, j].nG;
                        p.ProjectCells[i, j].B = p.ProjectCells[i, j].nB;
                        p.ProjectImage.SetPixel(i, j, Color.FromArgb(255, p.ProjectCells[i, j].R, p.ProjectCells[i, j].G, p.ProjectCells[i, j].B));
                    }
            }
        }
        private Boolean[] selectNeighborhood(Int32 type)
        {
            Boolean[] neigh1;
            Boolean[,] neigh2;
            Random r;
            Int32 current;
            switch (type)
            {
                case 2:
                    return new Boolean[]{
                            true, true, true,
                            true, false, true,
                            true, true, true
                    };


                case 3:
                    return new Boolean[]{
                            false, true, false,
                            true, false, true,
                            false, true, false
                    };

                case 4:
                    neigh2 = new Boolean[,]
                    {
                            {
                            false, true, true,
                            false, false, true,
                            false, true, true
                            },

                            {
                            true, true, false,
                            true, false, false,
                            true, true, false
                            },

                            {
                            false, false, false,
                            true, false, true,
                            true, true, true
                            },

                            {
                            true, true, true,
                            true, false, true,
                            false, false, false
                            }
                    };
                    r = new Random();
                    current = Convert.ToInt32(mod(r.Next(0, 1000), 4));
                    neigh1 = new Boolean[9];
                    for (int i = 0; i < 9; i++)
                        neigh1[i] = neigh2[current, i];
                    return neigh1;

                case 5:
                    return new Boolean[]
                    {
                            true, true, false,
                            true, false, true,
                            false, true, true
                    };

                case 6:

                    return new Boolean[]
                    {
                            false, true, true,
                            true, false, true,
                            true, true, false
                    };

                case 7:
                    neigh2 = new Boolean[,]
                    {
                            { // left
                            true, true, false,
                            true, false, true,
                            false, true, true
                            },

                            { // right
                            false, true, true,
                            true, false, true,
                            true, true, false
                            }
                   };
                    r = new Random();
                    current = Convert.ToInt32(mod(r.Next(0, 1000), 2));
                    neigh1 = new Boolean[9];
                    for (int i = 0; i < 9; i++)
                        neigh1[i] = neigh2[current, i];
                    return neigh1;

                default:
                    return new Boolean[] { false };
            }
        }
        private Int32 mod(Int32 x, Int32 m)
        {
            return ((x % m) + m) % m;
        }
        private Int32 calculateLength(Point p1, Point p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }
        private Int32 calculateMinPeriodVoronoi(Point p1, Point p2, Int32 translateX, Int32 translateY)
        {
            Point[] dirs = {
                               new Point(-1, -1), new Point(0, -1), new Point(1, -1),
                               new Point(1, 0), new Point(1, 1), new Point(0, 1),
                               new Point(-1, 1), new Point(-1, 0)
                           };
            List<Int32> values = new List<Int32>();
            for (int i = 0; i < dirs.Length; i++)
            {
                dirs[i].X *= translateX;
                dirs[i].X += p2.X;
                dirs[i].Y *= translateY;
                dirs[i].Y += p2.Y;
                values.Add(calculateLength(p1, dirs[i]));
            }
            return values.Min();
        }
    }
}
