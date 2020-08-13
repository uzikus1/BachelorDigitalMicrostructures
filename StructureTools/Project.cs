using System;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;
using System.Xml.Serialization;

namespace StructureTools
{
    [Serializable]
    public class Project
    {        
        private String projectPath, projectName;
        [NonSerialized]
        private Bitmap projectImage;
        private Int32 imageX, imageY, currentMCStep, finalMCStep;
        private Boolean isImported, isLoaded, isGrown, isPeriodic, isMCEnabled, isMCFinished;
        private projectType projectType;
        private List<Grain> projectGrains;
        private List<System.Windows.Point> allMCPoints, checkedMCPoints;
        private Cell[,] projectCells;

        //getters and setters
        public String ProjectPath
        {
            get { return this.projectPath; }
            set { this.projectPath = value; }
        }
        public String ProjectName
        {
            get { return this.projectName; }
            set { this.projectName = value; }
        }
        public Bitmap ProjectImage
        {
            get { return this.projectImage; }
            set { this.projectImage = value; }
        }
        public Int32 ImageX
        {
            get { return this.imageX; }
            set { this.imageX = value; }
        }
        public Int32 ImageY
        {
            get { return this.imageY; }
            set { this.imageY = value; }
        }
        public Int32 CurrentMCStep
        {
            get { return this.currentMCStep; }
            set { this.currentMCStep = value; }
        }
        public Int32 FinalMCStep
        {
            get { return this.finalMCStep; }
            set { this.finalMCStep = value; }
        }
        public Boolean IsImported
        {
            get { return this.isImported; }
            set { this.isImported = value; }
        }
        public Boolean IsLoaded
        {
            get { return this.isLoaded; }
            set { this.isLoaded = value; }
        }
        public Boolean IsGrown
        {
            get { return this.isGrown; }
            set { this.isGrown = value; }
        }
        public Boolean IsPeriodic
        {
            get { return this.isPeriodic; }
            set { this.isPeriodic = value; }
        }
        public Boolean IsMCEnabled
        {
            get { return this.isMCEnabled; }
            set { this.isMCEnabled = value; }
        }
        public Boolean IsMCFinished
        {
            get { return this.isMCFinished; }
            set { this.isMCFinished = value; }
        }
        public projectType ProjectType
        {
            get { return this.projectType; }
            set { this.projectType = value; }
        }
        public List<Grain> ProjectGrains 
        {
            get { return this.projectGrains; }
            set { this.projectGrains = value; }
        }
        public List<System.Windows.Point> AllMCPoints
        {
            get { return this.allMCPoints; }
            set { this.allMCPoints = value; }
        }
        public List<System.Windows.Point> CheckedMCPoints
        {
            get { return this.checkedMCPoints; }
            set { this.checkedMCPoints = value; }
        }
        public Cell[,] ProjectCells
        {
            get { return this.projectCells; }
            set { this.projectCells = value; }
        }

        public Project() { }
        public Project(String name, Int32 width, Int32 height, Int32 projectInt, Boolean import, String importPath, Boolean periodic, Boolean mcEnable, Int32 mcSteps)
        {
            this.ProjectName = name;
            this.ImageX = width;
            this.ImageY = height;
            this.IsLoaded = true;
            this.ProjectPath = @".\_WORKSPACE\projects\" + this.ProjectName + @"\";
            Bitmap bmp;
            this.ProjectCells = new Cell[this.ImageX, this.ImageY];
            this.IsPeriodic = periodic;
            if (mcEnable)
                this.enableMC(mcSteps);
            else
                this.disableMC();  
            if (import)
            {
                this.ProjectImage = new Bitmap(importPath);
                this.ProjectType = projectType.projImported;
                this.IsGrown = true;
                this.IsImported = true;
                for (int i = 0; i < this.ImageX; i++)
                    for (int j = 0; j < this.ImageY; j++)
                    {
                        this.ProjectCells[i, j] = new Cell();
                        this.ProjectCells[i, j].R = this.ProjectImage.GetPixel(i, j).R;
                        this.ProjectCells[i, j].G = this.ProjectImage.GetPixel(i, j).G;
                        this.ProjectCells[i, j].B = this.ProjectImage.GetPixel(i, j).B;
                    } 
            }
            else 
            {
                    bmp = new Bitmap(this.ImageX, this.ImageY);
                    this.ProjectImage = bmp;
                    this.ProjectType = (projectType)projectInt;
                    this.IsGrown = false;
                    this.IsImported = false;
                    this.ProjectGrains = new List<Grain>();                           
                    for (int i = 0; i < this.ImageX; i++)
                        for (int j = 0; j < this.ImageY; j++)
                        {   
                            this.ProjectCells[i, j] = new Cell();
                            this.ProjectCells[i, j].initOrClear();
                            this.ProjectImage.SetPixel(i, j, System.Drawing.Color.White);
                        }
            }
        }      
        public void enableMC(Int32 mcSteps)
        {
            this.IsMCEnabled = true;
            this.AllMCPoints = new List<System.Windows.Point>();
            for (int i = 0; i < this.ImageX; i++)
                for (int j = 0; j < this.ImageY; j++)
                    this.AllMCPoints.Add(new System.Windows.Point(i, j));
            this.CheckedMCPoints = new List<System.Windows.Point>();
            this.IsMCFinished = false;
            this.CurrentMCStep = 0;
            this.FinalMCStep = mcSteps;
        }
        public void disableMC()
        {
            this.IsMCEnabled = false;
            if(this.AllMCPoints != null)
                this.AllMCPoints.Clear();
            if(this.CheckedMCPoints != null)
                this.CheckedMCPoints.Clear();
            this.IsMCFinished = true;
            this.CurrentMCStep = 0;
            this.FinalMCStep = 0;
        }
    }

    public enum projectType
    {
        projVoronoi = 1,
        projCAMoore = 2, projCAVN = 3,
        projCAPentRand = 4,
        projCAHexL = 5, projCAHexR = 6, projCAHexRand = 7,
        projImported = 8
    };
}
