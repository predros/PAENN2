using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PAENN2.ViewModels;

namespace PAENN2.Models
{
    public static class ActionHandler
    {
        // Lists containing every undoable saved state
        private static List<List<NodeState>> Undo_Nodes = new List<List<NodeState>> { new List<NodeState>() };
        private static List<List<MemberState>> Undo_Members = new List<List<MemberState>> { new List<MemberState>() };

        // Lists containing every redoable saved state
        private static List<List<NodeState>> Redo_Nodes = new List<List<NodeState>> { new List<NodeState>() };
        private static List<List<MemberState>> Redo_Members = new List<List<MemberState>> { new List<MemberState>() };


        #region Undo/Redo and Action handling methods
        public static void ClearStates()
        {
            Undo_Nodes = new List<List<NodeState>> { new List<NodeState>() };
            Undo_Members = new List<List<MemberState>> { new List<MemberState>() };
            Redo_Nodes = new List<List<NodeState>> { new List<NodeState>() };
            Redo_Members = new List<List<MemberState>> { new List<MemberState>() };
        }


        /// <summary>
        /// Saves the current state of the structure.
        /// </summary>
        public static void SaveState()
        {
            List<NodeState> nodestate = new List<NodeState>();
            List<MemberState> memberstate = new List<MemberState>();

            foreach (Node node in VarHolder.NodesList)
            {
                nodestate.Add(new NodeState(node));
            }

            foreach (Member member in VarHolder.MembersList)
            {
                memberstate.Add(new MemberState(member));
            }

            Undo_Nodes.Add(nodestate);
            Undo_Members.Add(memberstate);

            Redo_Nodes.RemoveRange(1, Redo_Nodes.Count - 1);
            Redo_Members.RemoveRange(1, Redo_Members.Count - 1);
        }



        /// <summary>
        /// Reverts to the second-to-last saved state of the structure.
        /// </summary>
        public static void Undo()
        {
            int n = Undo_Nodes.Count - 1;
            if (n < 1)
            {
                return;
            }

            foreach (Node node in VarHolder.NodesList)
            {
                node.RemoveFromCanvas();
            }

            foreach (Member member in VarHolder.MembersList)
            {
                member.RemoveFromCanvas();
            }

            VarHolder.NodesList.Clear();
            VarHolder.MembersList.Clear();

            foreach (NodeState state in Undo_Nodes[n - 1])
            {
                state.ApplyBack();
            }

            foreach (MemberState state in Undo_Members[n - 1])
            {
                state.ApplyBack();
            }

            Redo_Nodes.Add(Undo_Nodes[n]);
            Redo_Members.Add(Undo_Members[n]);

            Undo_Nodes.RemoveAt(n);
            Undo_Members.RemoveAt(n);
        }



        /// <summary>
        /// Reverts to the last undone state of the structure.
        /// </summary>
        public static void Redo()
        {
            int n = Redo_Nodes.Count - 1;
            if (n < 1)
            {
                return;
            }

            foreach (Node node in VarHolder.NodesList)
            {
                node.RemoveFromCanvas();
            }

            foreach (Member member in VarHolder.MembersList)
            {
                member.RemoveFromCanvas();
            }

            VarHolder.NodesList.Clear();
            VarHolder.MembersList.Clear();

            foreach (NodeState state in Redo_Nodes[n])
            {
                state.ApplyBack();
            }

            foreach (MemberState state in Redo_Members[n])
            {
                state.ApplyBack();
            }

            Undo_Nodes.Add(Redo_Nodes[n]);
            Undo_Members.Add(Redo_Members[n]);

            Redo_Nodes.RemoveAt(n);
            Redo_Members.RemoveAt(n);
        }
        #endregion



        #region Item selection methods
        /// <summary>
        /// Selects a single member.
        /// </summary>
        /// <param name="member">The member.</param>
        public static void Select(Member member)
        {
            VarHolder.SelectedMembers.Add(member);

            foreach (Member m in VarHolder.MembersList)
            {
                if (VarHolder.SelectedMembers.Contains(m))
                {
                    m.Select();
                }
                else
                {
                    m.Unselect();
                }
            }
        }



        /// <summary>
        /// Selects a single node.
        /// </summary>
        /// <param name="node">The node.</param>
        public static void Select(Node node)
        {
            VarHolder.SelectedNodes.Add(node);

            foreach (Node n in VarHolder.NodesList)
            {
                if (VarHolder.SelectedNodes.Contains(n))
                {
                    n.Select();
                }
                else
                {
                    n.Unselect();
                }
            }
        }



        /// <summary>
        /// Selects multiple members, given by a list.
        /// </summary>
        /// <param name="members">The list containing the members to be selected.</param>
        public static void SelectMultiple(List<Member> members)
        {
            foreach (Member m in members)
            {
                VarHolder.SelectedMembers.Add(m);
            }

            foreach (Member m in VarHolder.MembersList)
            {
                if (VarHolder.SelectedMembers.Contains(m))
                {
                    m.Select();
                }
                else
                {
                    m.Unselect();
                }
            }
        }



        /// <summary>
        /// Selects multiple nodes, given by a list.
        /// </summary>
        /// <param name="nodes">The list of nodes to be selected.</param>
        public static void SelectMultiple(List<Node> nodes)
        {
            foreach (Node n in nodes)
            {
                VarHolder.SelectedNodes.Add(n);
            }

            foreach (Node n in VarHolder.NodesList)
            {
                if (VarHolder.SelectedNodes.Contains(n))
                {
                    n.Select();
                }
                else
                {
                    n.Unselect();
                }
            }
        }



        /// <summary>
        /// Clears all selected nodes and members.
        /// </summary>
        public static void ClearSelection()
        {
            VarHolder.SelectedMembers.Clear();
            VarHolder.SelectedNodes.Clear();

            foreach (Member m in VarHolder.MembersList)
            {
                m.Unselect();
            }

            foreach (Node n in VarHolder.NodesList)
            {
                n.Unselect();
            }
        }



        /// <summary>
        /// Finds all nodes and members intersecting/contained in a given selection box.
        /// </summary>
        /// <param name="P1">The selection box's starting point.</param>
        /// <param name="P2">The selection box's ending point.</param>
        /// <param name="IsBlueSelection">True if it is a "blue" selection box (only selecting elements completely contained within), 
        /// and false if a "green" box (selects any element intersecting the rectangle).</param>
        public static void SelectionBox(Point P1, Point P2, bool IsBlueSelection)
        {
            Rect rectangle = new Rect(P1, P2);
            List<Node> nodes = new List<Node>();
            List<Member> members = new List<Member>();

            foreach (Node node in VarHolder.NodesList)
            {
                if (rectangle.Contains(node.Coords))
                {
                    nodes.Add(node);
                }
            }

            if (IsBlueSelection)
            {
                foreach (Member member in VarHolder.MembersList)
                {
                    if (rectangle.Contains(member.N1.Coords) && rectangle.Contains(member.N2.Coords))
                    {
                        members.Add(member);
                    }
                }
            }

            else
            {
                Point P3 = new Point(P2.X, P1.Y);
                Point P4 = new Point(P1.X, P2.Y);

                foreach (Member member in VarHolder.MembersList)
                {
                    Point n1 = member.N1.Coords;
                    Point n2 = member.N2.Coords;
                    if (Helper.DoTheyIntersect(n1, n2, P1, P3) || Helper.DoTheyIntersect(n1, n2, P3, P2) ||
                        Helper.DoTheyIntersect(n1, n2, P3, P4) || Helper.DoTheyIntersect(n1, n2, P4, P1))
                    {
                        members.Add(member);
                    }
                }
            }

            SelectMultiple(nodes);
            SelectMultiple(members);
        }
        #endregion



        #region Undoable action methods
        /// <summary>
        /// Creates a new Node at the given Point.
        /// </summary>
        /// <param name="P">The Point containing the Node's coordinates.</param>
        /// <param name="tol">Tolerance used for checking whether any other Node is too close.</param>
        public static void NewNode(Point P, double tol)
        {
            foreach (Node node in VarHolder.NodesList)
            {
                if (P.X.CloseEnough(node.Coords.X, tol) && P.Y.CloseEnough(node.Coords.Y, tol))
                {
                    throw new ArgumentException("There is already a node in the given coordinates.");
                }
            }

            Node n = new Node(P);
            VarHolder.NodesList.Add(n);
            VarHolder.FileChanged = true;
            SaveState();
        }



        /// <summary>
        /// Applies the currently stored nodal forces array to a given Node (in the current Loadcase).
        /// </summary>
        /// <param name="node">The Node.</param>
        public static void NodalForces(Node node)
        {
            string loadcase = VarHolder.CurrentLoadcase;

            node.Forces[loadcase]["Fx"] = VarHolder.AppliedNodal[0];
            node.Forces[loadcase]["Fy"] = VarHolder.AppliedNodal[1];
            node.Forces[loadcase]["Mz"] = VarHolder.AppliedNodal[2];
            node.Forces[loadcase]["Angle"] = VarHolder.AppliedNodal[3];

            VarHolder.MaxForce = 0;
            foreach (Node n in VarHolder.NodesList)
            {
                if (!n.Forces[loadcase]["Fx"].CloseEnough(0))
                {
                    VarHolder.MaxForce = Math.Max(VarHolder.MaxForce, Math.Abs(n.Forces[loadcase]["Fx"]));
                }

                if (!n.Forces[loadcase]["Fy"].CloseEnough(0))
                {
                    VarHolder.MaxForce = Math.Max(VarHolder.MaxForce, Math.Abs(n.Forces[loadcase]["Fy"]));
                }
            }

            VarHolder.MinForce = VarHolder.MaxForce;
            foreach (Node n in VarHolder.NodesList)
            {
                if (!n.Forces[loadcase]["Fx"].CloseEnough(0))
                {
                    VarHolder.MinForce = Math.Min(VarHolder.MinForce, Math.Abs(n.Forces[loadcase]["Fx"]));
                }

                if (!n.Forces[loadcase]["Fy"].CloseEnough(0))
                {
                    VarHolder.MinForce = Math.Min(VarHolder.MinForce, Math.Abs(n.Forces[loadcase]["Fy"]));
                }
            }

            if (VarHolder.MaxForce == 0)
            {
                VarHolder.MaxForce = 1;
            }

            if (VarHolder.MinForce == 0)
            {
                VarHolder.MinForce = 1;
            }

            if (VarHolder.MaxForce.CloseEnough(VarHolder.MinForce))
            {
                VarHolder.MinForce /= 2;
            }

            foreach (Node n in VarHolder.NodesList)
            {
                n.ApplyForces();
            }
            VarHolder.FileChanged = true;
            SaveState();
        }



        /// <summary>
        /// Applies the currently stored support conditions array to a given Node.
        /// </summary>
        /// <param name="node">The Node.</param>
        public static void AddSupports(Node node)
        {

            node.Restr["Rx"] = (VarHolder.AppliedRestr[0] == 0) ? false : true;
            node.Restr["Ry"] = (VarHolder.AppliedRestr[1] == 0) ? false : true;
            node.Restr["Rz"] = (VarHolder.AppliedRestr[2] == 0) ? false : true;
            node.R_angle = VarHolder.AppliedRestr[3];

            node.Springs["Kx"] = VarHolder.AppliedRestr[4];
            node.Springs["Ky"] = VarHolder.AppliedRestr[5];
            node.Springs["Kz"] = VarHolder.AppliedRestr[6];

            node.Pdispl["Ux"] = VarHolder.AppliedRestr[7];
            node.Pdispl["Uy"] = VarHolder.AppliedRestr[8];
            node.Pdispl["Rz"] = VarHolder.AppliedRestr[9];

            node.ApplySupports();
            VarHolder.FileChanged = true;
            SaveState();
        }

        public static void AddNodeHinge(Node node, bool RemoveHinge = false)
        {
            node.IsHinged = !RemoveHinge;
            node.Select();
            VarHolder.FileChanged = true;
            SaveState();
        }

        public static void AddMemberHinge(Member member, int hinge_type)
        {
            switch (hinge_type)
            {
                case 0:
                    member.Hinge_Start = true;
                    member.Hinge_End = false;
                    break;
                case 1:
                    member.Hinge_Start = false;
                    member.Hinge_End = true;
                    break;
                case 2:
                    member.Hinge_Start = true;
                    member.Hinge_End = true;
                    break;
                case 3:
                    member.Hinge_Start = false;
                    member.Hinge_End = false;
                    break;
            }
            member.Select();
            member.Reposition();
            VarHolder.FileChanged = true;
            SaveState();
        }


        /// <summary>
        /// Adds a new Member at the given endpoints, with given Material and Section.
        /// </summary>
        /// <param name="start">Point containing the Member's starting coordinates.</param>
        /// <param name="end">Point containing the Member's ending coordinates.</param>
        /// <param name="material">The Member's Material.</param>
        /// <param name="section">The Member's Section.</param>
        public static void NewMember(Point start, Point end, MemberMat material = null, MemberSec section = null, double tol = 1e-5)
        {
            // If the input material/section is null, use the one stored as current 
            material = material ?? VarHolder.CurrentMaterial;
            section = section ?? VarHolder.CurrentSection;

            Node n1 = VarHolder.NodesList.FirstOrDefault(n =>
                                                         n.Coords.X.CloseEnough(start.X, tol) &&
                                                         n.Coords.Y.CloseEnough(start.Y, tol));
            Node n2 = VarHolder.NodesList.FirstOrDefault(n =>
                                                         n.Coords.X.CloseEnough(end.X, tol) &&
                                                         n.Coords.Y.CloseEnough(end.Y, tol));

            foreach (Member member in VarHolder.MembersList)
            {
                if ((n1 == member.N1 && n2 == member.N2) || (n2 == member.N1 && n1 == member.N2))
                {
                    throw new ArgumentException("There is already a member at the given endpoints.");
                }
            }

            if (n1 == default(Node))
            {
                n1 = new Node(start);
                VarHolder.NodesList.Add(n1);
            }

            if (n2 == default(Node))
            {
                n2 = new Node(end);
                VarHolder.NodesList.Add(n2);
            }


            Member m = new Member(n1, n2, material, section);
            VarHolder.MembersList.Add(m);
            VarHolder.FileChanged = true;
            SaveState();
        }



        /// <summary>
        /// Applies the currently stored distributed loads array to a given Member (in the current Loadcase).
        /// </summary>
        /// <param name="member">The Member.</param>
        public static void MemberLoads(Member member)
        {
            string loadcase = VarHolder.CurrentLoadcase;

            member.Loads[loadcase]["Qx0"] = VarHolder.AppliedLoads[0];
            member.Loads[loadcase]["Qx1"] = VarHolder.AppliedLoads[1];
            member.Loads[loadcase]["Qy0"] = VarHolder.AppliedLoads[2];
            member.Loads[loadcase]["Qy1"] = VarHolder.AppliedLoads[3];
            member.Loads[loadcase]["Type"] = VarHolder.AppliedLoads[4];

            VarHolder.MaxLoad = 0;

            foreach (Member m in VarHolder.MembersList)
            {
                VarHolder.MaxLoad = Math.Max(VarHolder.MaxLoad, Math.Abs(m.Loads[loadcase]["Qy0"]));
                VarHolder.MaxLoad = Math.Max(VarHolder.MaxLoad, Math.Abs(m.Loads[loadcase]["Qy1"]));

                if (m.Loads[loadcase]["Type"].CloseEnough(1))
                {
                    VarHolder.MaxLoad = Math.Max(VarHolder.MaxLoad, Math.Abs(m.Loads[loadcase]["Qx0"]));
                    VarHolder.MaxLoad = Math.Max(VarHolder.MaxLoad, Math.Abs(m.Loads[loadcase]["Qx1"]));
                }

            }

            VarHolder.MinLoad = VarHolder.MaxLoad;
            foreach (Member m in VarHolder.MembersList)
            {
                VarHolder.MinLoad = m.Loads[loadcase]["Qy0"].CloseEnough(0) ? VarHolder.MinLoad : Math.Min(VarHolder.MinLoad, Math.Abs(m.Loads[loadcase]["Qy0"]));
                VarHolder.MinLoad = m.Loads[loadcase]["Qy1"].CloseEnough(0) ? VarHolder.MinLoad : Math.Min(VarHolder.MinLoad, Math.Abs(m.Loads[loadcase]["Qy1"]));

                if (m.Loads[loadcase]["Type"].CloseEnough(1))
                {
                    VarHolder.MinLoad = m.Loads[loadcase]["Qx0"].CloseEnough(0) ? VarHolder.MinLoad : Math.Min(VarHolder.MinLoad, Math.Abs(m.Loads[loadcase]["Qx0"]));
                    VarHolder.MinLoad = m.Loads[loadcase]["Qx1"].CloseEnough(0) ? VarHolder.MinLoad : Math.Min(VarHolder.MinLoad, Math.Abs(m.Loads[loadcase]["Qx1"]));
                }


            }

            if (VarHolder.MaxLoad == 0)
            {
                VarHolder.MaxLoad = 0.2;
            }

            if (VarHolder.MinLoad == 0)
            {
                VarHolder.MinLoad = 0.1;
            }

            if (VarHolder.MinLoad >= VarHolder.MaxLoad)
            {
                VarHolder.MinLoad = VarHolder.MaxLoad - 0.1;
            }

            foreach (Member m in VarHolder.MembersList)
            {
                m.Reposition();
            }
            VarHolder.FileChanged = true;
            SaveState();
        }



        /// <summary>
        /// Removes every selected member and node from the structure.
        /// </summary>
        public static void DeleteSelection()
        {
            foreach (Member member in VarHolder.SelectedMembers)
            {
                _ = VarHolder.MembersList.Remove(member);
                member.RemoveFromCanvas();
            }

            foreach (Node node in VarHolder.SelectedNodes)
            {

                for (int i = VarHolder.MembersList.Count - 1; i >= 0; i--)
                {
                    Member member = VarHolder.MembersList[i];

                    if (member.N1 == node || member.N2 == node)
                    {
                        _ = VarHolder.MembersList.Remove(member);
                        member.RemoveFromCanvas();
                    }
                }

                _ = VarHolder.NodesList.Remove(node);
                node.RemoveFromCanvas();
            }
            VarHolder.FileChanged = true;
            SaveState();
        }
        #endregion


        public static void Run()
        {
            Analysis.ExtractData(out double[] X, out double[] Y, out bool[] Restr, out double[] RestrAngle, out double[] Spring,
                                 out double[] Pdispl, out Dictionary<string, double[]> Nodal, out Dictionary<string, double[]> NAngle,
                                 out int[] N1, out int[] N2, out double[] Elast, out double[] Thermal, out double[] Inertia,
                                 out double[] Area, out double[] Ysup, out double[] Yinf, out Dictionary<string, double[]> Q1,
                                 out Dictionary<string, double[]> Q2, out Dictionary<string, bool[]> QIsGlobal,
                                 out Dictionary<string, double[]> T1, out Dictionary<string, double[]> T2, out bool[] H1, out bool[] H2);

            Analysis.LinearAnalysis(X, Y, Restr, RestrAngle, Spring, Pdispl, Nodal, NAngle, N1, N2, Elast, Thermal, Inertia,
                                    Area, Ysup, Yinf, Q1, Q2, QIsGlobal, T1, T2, H1, H2, VarHolder.LoadcasesList,
                                    out Dictionary<string, List<double[]>> U, out Dictionary<string, List<double[]>> F,
                                    out Dictionary<string, double[]> R);

            Analysis.InternalResults(X, Y, N1, N2, Elast, Inertia, Area, H1, H2, Q1, Q2, QIsGlobal, U, F, VarHolder.LoadcasesList,
                                     out Dictionary<string, List<double[]>> InUx, out Dictionary<string, List<double[]>> InUy,
                                     out Dictionary<string, List<double[]>> InRz, out Dictionary<string, List<double[]>> InN,
                                     out Dictionary<string, List<double[]>> InQ, out Dictionary<string, List<double[]>> InM,
                                     out Dictionary<string, List<int[]>> InMaxM, out Dictionary<string, List<int>> InMaxQ);


            foreach (string loadcase in VarHolder.LoadcasesList)
            {
                for (int i = 0; i < VarHolder.MembersList.Count; i++)
                {
                    Member member = VarHolder.MembersList[i];

                    member.Uy[loadcase] = InUy[loadcase][i];
                    member.Ux[loadcase] = InUx[loadcase][i];
                    member.Rz[loadcase] = InRz[loadcase][i];

                    member.Axial[loadcase] = InN[loadcase][i];
                    member.Shear[loadcase] = InQ[loadcase][i];
                    member.Moment[loadcase] = InM[loadcase][i];
                    member.MomentMax[loadcase] = InMaxM[loadcase][i];
                    member.ShearMax[loadcase] = InMaxQ[loadcase][i];
                }
            }

            VarHolder.ResultsAvailable = true;
            VarHolder.ClickType = "ResultsDispl";
            ResultShow(true);
        }


        public static void ResultClick(Member member, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!VarHolder.Content.Children.Contains(VarHolder.ResultDisplayPath))
            {
                Helper.AddToCanvas(VarHolder.Content, VarHolder.ResultDisplayPath, VarHolder.ResultDisplayText);
            }

            string loadcase = VarHolder.CurrentLoadcase;
            int n = member.Ux[loadcase].Length;

            Point P1 = VarHolder.Transfmatrix.Transform(member.N1.Coords);
            Point P2 = VarHolder.Transfmatrix.Transform(member.N2.Coords);
            Point P = e.GetPosition(VarHolder.Content);

            double L = (P2 - P1).Length;
            double l = member.Length;
            double x = (P - P1).Length * l / L;

            int i1 = (int)Math.Floor(x * (n - 1) / l);
            int i2 = (int)Math.Ceiling(x * (n - 1) / l);
            double x1 = i1 * l / (n - 1);
            double x2 = i2 * l / (n - 1);

            double[] Hy = new double[2];
            double[] Hx = new double[2];
            double[] Hz = new double[2];
            string text = "";
            double f = 1.0;

            switch (VarHolder.ClickType)
            {
                case "ResultsDispl":
                    Hy = new double[] { member.Uy[loadcase][i1], member.Uy[loadcase][i2] };
                    Hx = new double[] { member.Ux[loadcase][i1], member.Ux[loadcase][i2] };
                    Hz = new double[] { member.Rz[loadcase][i1], member.Rz[loadcase][i2] };
                    break;
                case "ResultsBM":
                    Hy = new double[] { member.Moment[loadcase][i1], member.Moment[loadcase][i2] };
                    f = -0.01;
                    break;
                case "ResultsSF":
                    Hy = new double[] { member.Shear[loadcase][i1], member.Shear[loadcase][i2] };
                    break;
                case "ResultsNF":
                    Hy = new double[] { member.Axial[loadcase][i1], member.Axial[loadcase][i2] };
                    break;
                default:
                    throw new Exception("Result-displaying method called in edit mode.");
            }

            double hx = Helper.LinInterp(x1, Hx[0], x2, Hx[1], x);
            double hy = Helper.LinInterp(x1, Hy[0], x2, Hy[1], x);
            double hz = Helper.LinInterp(x1, Hz[0], x2, Hz[1], x);

            StreamGeometry stg = new StreamGeometry();

            using (StreamGeometryContext ctx = stg.Open())
            {
                Point p = new Point(x, 0);
                ctx.BeginFigure(p, true, false);
                p.X += hx;
                p.Y += f * Hy[0];
                ctx.LineTo(p, true, false);
            }

            switch (VarHolder.ClickType)
            {
                case "ResultsDispl":
                    string s1 = (Units.ConvertLength(hx, Units.Displacement, true)).ToString(Units.Formats["Displacement"]);
                    string s2 = (Units.ConvertLength(hy, Units.Displacement, true)).ToString(Units.Formats["Displacement"]);
                    string s3 = (Units.ConvertAngle(hx, Units.Rotation, true)).ToString(Units.Formats["Rotation"]);
                    text = $"({s1} {Units.Displacement}, {s2} {Units.Displacement}, {s3} {Units.Rotation})";
                    break;
                case "ResultsBM":
                    s1 = (Units.ConvertMoment(hy, Units.Moment, true)).ToString(Units.Formats["Moment"]);
                    text = $"{s1}";
                    break;
                case "ResultsSF":
                    s1 = (Units.ConvertForce(hy, Units.Force, true)).ToString(Units.Formats["Force"]);
                    text = $"{s1}";
                    break;
                case "ResultsNF":
                    s1 = (Units.ConvertForce(hy, Units.Force, true)).ToString(Units.Formats["Force"]);
                    text = $"{s1}";
                    break;
                default:
                    throw new Exception("Result-displaying method called in edit mode.");
            }


            VarHolder.ResultDisplayValue = new double[] { x, hx, f * hy, hz };
            VarHolder.ResultDisplayMember = member;
            VarHolder.ResultDisplayPath.Data = stg;
            VarHolder.ResultDisplayText.Text = text;

            ResultPosition();
        }

        public static void ResultShow(bool ChangeType = false)
        {
            foreach (Member member in VarHolder.MembersList)
            {
                member.ShowResults();
            }

            foreach (Node node in VarHolder.NodesList)
            {
                node.DrawResults();
            }

            if (ChangeType)
            {
                VarHolder.ResultDisplayMember = null;
                VarHolder.ResultDisplayPath.Data = null;
                VarHolder.ResultDisplayText.Text = "";
            }
            else if (VarHolder.ResultDisplayMember != null)
            {
                ActionHandler.ResultPosition();
            }
        }

        static public void ResultPosition()
        {
            if (VarHolder.ResultDisplayMember == null)
            {
                return;
            }

            Member member = VarHolder.ResultDisplayMember;
            Point P1 = VarHolder.Transfmatrix.Transform(member.N1.Coords);
            Point P2 = VarHolder.Transfmatrix.Transform(member.N2.Coords);

            double L = (P2 - P1).Length;
            double l = member.Length;
            double fx = L / l;
            double fy = VarHolder.ResultScale * fx;

            double x = VarHolder.ResultDisplayValue[0];
            double hx = VarHolder.ResultDisplayValue[1];
            double hy = VarHolder.ResultDisplayValue[2];

            Size size_r = VarHolder.ResultDisplayText.MeasureString(VarHolder.ResultDisplayText.Text);


            int k_angle = (member.Angle <= 90 || (member.Angle > 270 && member.Angle < 360)) ? 1 : -1;

            double yr = (hy * k_angle > 0) ? size_r.Height : 0;

            Point p = new Point(P1.X + (fx * x) + (fy * hx) - (size_r.Width / 2),
                                P1.Y + (fy * hy) + (Math.Sign(hy) * (5 + yr)));

            VarHolder.ResultDisplayPath.StrokeThickness = 1 / fx;


            Helper.SetCanvasPosition(VarHolder.ResultDisplayPath, P1.X, P1.Y);
            Helper.SetCanvasPosition(VarHolder.ResultDisplayPath, p.X, p.Y);

            TransformGroup t_text = (TransformGroup)VarHolder.ResultDisplayText.RenderTransform;
            TransformGroup t = (TransformGroup)VarHolder.ResultDisplayPath.RenderTransform;

            t.AssignToChildren(fx, fy, 0, 0, member.Angle, 0, 0);
            t_text.AssignToChildren(k_angle, -k_angle, size_r.Width / 2, 0, member.Angle, -p.X + P1.X, -p.Y + P1.Y);



        }
    }
}
