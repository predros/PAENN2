using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Shapes;
using PAENN2.ViewModels;


namespace PAENN2.Models
{

    public class Node : Observable
    {
        // The node's coordinates.
        private Point coords = new Point();
        public Point Coords { get => coords; set => ChangeProperty(ref coords, value); }

        public bool IsHinged { get; set; } = false;

        // The node's applied forces.
        public Dictionary<string, Dictionary<string, double>> Forces { get; set; } = new Dictionary<string, Dictionary<string, double>>();

        // The node's spring constants.
        public Dictionary<string, double> Springs { get; set; } = new Dictionary<string, double> { { "Kx", 0 }, { "Ky", 0 }, { "Kz", 0 } };

        // The node's displacement restrictions.
        public Dictionary<string, bool> Restr { get; set; } = new Dictionary<string, bool> { { "Rx", false }, { "Ry", false }, { "Rz", false } };
        public double R_angle { get; set; } = 0;

        // The node's prescribed displacements.
        public Dictionary<string, double> Pdispl { get; set; } = new Dictionary<string, double> { { "Ux", 0 }, { "Uy", 0 }, { "Rz", 0 } };


        // The node's GUI representation is handled by this object
        public PAENN2.ViewModels.NodeInterface NodeView = new PAENN2.ViewModels.NodeInterface();


        public Node(Point point)
        {
            Coords = point;

            foreach (string loadcase in VarHolder.LoadcasesList)
            {
                Forces[loadcase] = new Dictionary<string, double> { { "Fx", 0 }, { "Fy", 0 }, { "Mz", 0 }, { "Angle", 0 } };
            }

            NodeView.SetParent(this);
        }


        public void Reposition() => NodeView.Reposition();
        public void AddToCanvas() => NodeView.AddToCanvas();
        public void RemoveFromCanvas() => NodeView.RemoveFromCanvas();

        public void DrawResults() => NodeView.DrawResults();

        public void Select() => NodeView.ToggleSelect(true);
        public void Unselect() => NodeView.ToggleSelect(false);

        public void ApplyForces() => NodeView.ApplyForces();


        public void ApplySupports() => NodeView.ApplySupports();

        public void NodeClick()
        {
            ActionHandler.ClearSelection();
            ActionHandler.Select(this);

            switch (VarHolder.ClickType)
            {
                default:
                    ActionHandler.ClearSelection();
                    break;
                case "Select":
                    break;
                case "Nodal":
                    ActionHandler.NodalForces(this);
                    break;
                case "Supports":
                    ActionHandler.AddSupports(this);
                    break;
                case "NodeHinge":
                    ActionHandler.AddNodeHinge(this);
                    break;
                case "RemoveHinge":
                    ActionHandler.AddNodeHinge(this, true);
                    break;
                case "NewMember_Start":
                    VarHolder.NewMemberStart = Coords;
                    VarHolder.ClickType = "NewMember_End";
                    break;
                case "NewMember_End":
                    try
                    {
                        ActionHandler.NewMember(VarHolder.NewMemberStart, Coords);
                    }
                    catch (ArgumentException)
                    {
                        break;
                    }

                    VarHolder.ClickType = "NewMember_Start";
                    ActionHandler.ClearSelection();
                    break;
            }
        }

    }


    public class Member : Observable
    {
        public Node N1;
        public Node N2;

        public MemberMat Material;
        public MemberSec Section;

        public bool IsRigid = false;
        public bool Hinge_Start = false;
        public bool Hinge_End = false;

        public Dictionary<string, Dictionary<string, double>> Loads = new Dictionary<string, Dictionary<string, double>>();

        public Dictionary<string, double[]> Temperature = new Dictionary<string, double[]>();

        public Dictionary<string, double[]> Axial = new Dictionary<string, double[]>();
        public Dictionary<string, double[]> Shear = new Dictionary<string, double[]>();
        public Dictionary<string, double[]> Moment = new Dictionary<string, double[]>();

        public Dictionary<string, double[]> Ux = new Dictionary<string, double[]>();
        public Dictionary<string, double[]> Uy = new Dictionary<string, double[]>();
        public Dictionary<string, double[]> Rz = new Dictionary<string, double[]>();

        public double Length;
        public double Angle;

        public MemberInterface MemberView = new MemberInterface();

        public Member(Node start, Node end, MemberMat material, MemberSec section)
        {

            N1 = start;
            N2 = end;
            Material = material;
            Section = section;
            Length = (N2.Coords - N1.Coords).Length;
            Angle = N1.Coords.AngleBetween(N2.Coords);


            foreach (string loadcase in VarHolder.LoadcasesList)
            {
                Loads[loadcase] = new Dictionary<string, double> { { "Qx0", 0 }, { "Qx1", 0 }, { "Qy0", 0 }, { "Qy1", 0 }, { "Type", 0 } };
                Temperature[loadcase] = new double[] { 0, 0 };
            }

            MemberView.SetParent(this);
        }

        public void MemberClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (VarHolder.ClickType)
            {
                case "Select":
                    ActionHandler.Select(this);
                    break;
                case "Loads":
                    ActionHandler.MemberLoads(this);
                    break;
                case "StartHinge":
                    ActionHandler.AddMemberHinge(this, 0);
                    break;
                case "EndHinge":
                    ActionHandler.AddMemberHinge(this, 1);
                    break;
                case "BothHinge":
                    ActionHandler.AddMemberHinge(this, 2);
                    break;
                case "RemoveHinge":
                    ActionHandler.AddMemberHinge(this, 3);
                    break;
                case "ResultsDispl":
                case "ResultsBM":
                case "ResultsSF":
                case "ResultsNF":
                    ActionHandler.ResultClick(this, e);
                    break;
            }
        }


        public void Select() => MemberView.ToggleSelect(true);

        public void Unselect() => MemberView.ToggleSelect(false);


        public void RemoveFromCanvas() => MemberView.RemoveFromCanvas();

        public void AddToCanvas() => MemberView.AddToCanvas();


        public void Reposition() => MemberView.Reposition();

        public void ApplyLoads() => MemberView.ApplyLoads();

        public void ShowResults() => MemberView.ShowResults();
    }


    public class MemberMat : Observable
    {
        private string matname;
        public string Name { get => matname; set => ChangeProperty(ref matname, value); }

        private double elasticity;
        public double Elasticity { get => elasticity; set => ChangeProperty(ref elasticity, value); }

        private double thermal;
        public double Thermal { get => thermal; set => ChangeProperty(ref thermal, value); }


        public MemberMat(string name, double E, double thermal)
        {
            Name = name;
            Elasticity = E;
            Thermal = thermal;
        }
    }


    public class MemberSec : Observable
    {
        private string secname = "";
        public string Name { get => secname; set => ChangeProperty(ref secname, value); }


        private double inertia = 0;
        public double Inertia { get => inertia; set => ChangeProperty(ref inertia, value); }


        private double area = 0;
        public double Area { get => area; set => ChangeProperty(ref area, value); }


        private double ysup = 0;
        public double Ysup { get => ysup; set => ChangeProperty(ref ysup, value); }


        private double yinf = 0;
        public double Yinf { get => yinf; set => ChangeProperty(ref yinf, value); }


        private double[] parameters = { };
        public double[] Parameters { get => parameters; set => ChangeProperty(ref parameters, value); }



        public MemberSec(string name) { Name = name; }

        public void Generic(double I, double A, double y1, double y2)
        {
            Inertia = I;
            Area = A;
            Ysup = y1;
            Yinf = y2;

            Parameters = new double[] { I, A, y1, y2 };
        }

        public void Rectangle(double b, double h)
        {
            Inertia = b * h * h * h / 12;
            Area = b * h;
            Ysup = Yinf = h / 2;

            Parameters = new double[] { b, h };
        }

        public void Circle(double D, double d)
        {
            Inertia = Math.PI * (Math.Pow(D, 4) - Math.Pow(d, 4)) / 64;
            Area = Math.PI * ((D * D) - (d * d)) / 4;
            Ysup = Yinf = D / 2;

            Parameters = new double[] { D, d };
        }

    }

}
