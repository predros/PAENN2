using PAENN2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PAENN2
{
    public static class VarHolder
    {
        public static bool FileChanged = false;

        public static Matrix Transfmatrix = Matrix.Identity;
        public static string ClickType = "Select";


        public static string CurrentLoadcase = "Caso 01";
        public static MemberMat CurrentMaterial;
        public static MemberSec CurrentSection;

        public static double MaxForce = 1;
        public static double MinForce = 0;

        public static double MaxLoad = 0;
        public static double MinLoad = 0;


        public static List<string> LoadcasesList = new List<string> { "Caso 01" };
        public static ObservableCollection<Node> NodesList = new ObservableCollection<Node>();
        public static ObservableCollection<Member> MembersList = new ObservableCollection<Member>();
        public static ObservableCollection<MemberMat> MaterialsList = new ObservableCollection<MemberMat>();
        public static ObservableCollection<MemberSec> SectionsList = new ObservableCollection<MemberSec>();

        public static double[] AppliedNodal = { 0, 0, 0, 0 };
        public static double[] AppliedLoads = { 0, 0, 0, 0, 0 };
        public static double[] AppliedRestr = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static ObservableCollection<Member> SelectedMembers = new ObservableCollection<Member>();
        public static ObservableCollection<Node> SelectedNodes = new ObservableCollection<Node>();

        public static Point NewMemberStart;


        public static System.Windows.Controls.Canvas Content = new System.Windows.Controls.Canvas();
        public static DrawingBrush CanvasBG = new DrawingBrush { ViewportUnits = BrushMappingMode.Absolute, TileMode = TileMode.Tile };
        public static Pen GridPen = new Pen { Brush = Brushes.Gray, Thickness = 0.5 };

        public static Dictionary<string, List<double[]>> DisplacementResults = new Dictionary<string, List<double[]>>();
        public static Dictionary<string, List<double[]>> ForceResults = new Dictionary<string, List<double[]>>();
        public static Dictionary<string, double[]> ReactionResults = new Dictionary<string, double[]>();


        public static bool ResultsAvailable = false;
        public static double ResultScale = 2;
        public static double[] ResultDisplayValue = new double[4];
        public static Member ResultDisplayMember;


        public static Path ResultDisplayPath = new Path
        {
            Stroke = Brushes.Red,
            RenderTransform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(),
                    new RotateTransform()
                }
            }
        };
        public static System.Windows.Controls.TextBlock ResultDisplayText = new System.Windows.Controls.TextBlock
        {
            Foreground = Brushes.Red,
            Background = Brushes.White,
            RenderTransform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(),
                    new RotateTransform()
                }
            }
        };

    }

}
