using System;
using System.Collections.Generic;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;

namespace PAENN2.Models
{
    public static class Analysis
    {
        /// <summary>
        /// Extracts the data contained in the VarHolder objects and returns it in lists suitable for use in the analysis algorithm.
        /// </summary>
        /// <param name="X">The nodal X-coordinates list.</param>
        /// <param name="Y">The nodal Y-coordinates list.</param>
        /// <param name="Restr">The nodal restrictions list.</param>
        /// <param name="RestrAngle">The nodal support angles list.</param>
        /// <param name="Spring">The nodal spring constants list.</param>
        /// <param name="Pdispl">The nodal prescribed displacements list.</param>
        /// <param name="Nodal">The nodal forces array, sorted by loadcase.</param>
        /// <param name="N1">The member start-node indices list.</param>
        /// <param name="N2">The member end-node indices list.</param>
        /// <param name="Elast">The member elasticity moduli list.</param>
        /// <param name="Thermal">The member thermal expansion coefficients list.</param>
        /// <param name="Inertia">The member cross-sectional inertias list.</param>
        /// <param name="Area">The member cross-section areas list.</param>
        /// <param name="Ysup">The member max y-coordinates (local) list.</param>
        /// <param name="Yinf">The member min y-coordinates (local) list.</param>
        /// <param name="Q0">The member starting loads list, sorted by loadcase.</param>
        /// <param name="Q1">The member ending loads list, sorted by loadcase.</param>
        /// <param name="QIsGlobal">The member load types (local or global coordinates) list, sorted by loadcase.</param>
        /// <param name="T0">The member upper (local y-coord) temperatures list, sorted by loadcase.</param>
        /// <param name="T1">The member lower (local y-coord) temperatures list, sorted by loadcase.</param>
        /// <param name="H1">The member start-node moment release (hinge) configurations list</param>
        /// <param name="H2">The member end-node moment release (hinge) configurations list.</param>
        public static void ExtractData(out double[] X, out double[] Y, out bool[] Restr, out double[] RestrAngle,
                                out double[] Spring, out double[] Pdispl, out Dictionary<string, double[]> Nodal,
                                out Dictionary<string, double[]> NAngle, out int[] N1, out int[] N2, out double[] Elast,
                                out double[] Thermal, out double[] Inertia, out double[] Area, out double[] Ysup,
                                out double[] Yinf, out Dictionary<string, double[]> Q0, out Dictionary<string, double[]> Q1,
                                out Dictionary<string, bool[]> QIsGlobal, out Dictionary<string, double[]> T0,
                                out Dictionary<string, double[]> T1, out bool[] H1, out bool[] H2)
        {
            int nnodes = VarHolder.NodesList.Count;
            int nmembers = VarHolder.MembersList.Count;

            #region Nodal parameter extraction
            X = new double[nnodes];
            Y = new double[nnodes];

            Restr = new bool[3 * nnodes];
            RestrAngle = new double[nnodes];
            Spring = new double[3 * nnodes];
            Pdispl = new double[3 * nnodes];

            Nodal = new Dictionary<string, double[]>();
            NAngle = new Dictionary<string, double[]>();

            foreach (string loadcase in VarHolder.LoadcasesList)
            {
                Nodal[loadcase] = new double[3 * nnodes];
                NAngle[loadcase] = new double[nnodes];
            }


            for (int i = 0; i < nnodes; i++)
            {
                Node node = VarHolder.NodesList[i];
                X[i] = node.Coords.X;
                Y[i] = node.Coords.Y;

                Restr[3 * i] = node.Restr["Rx"];
                Restr[(3 * i) + 1] = node.Restr["Ry"];
                Restr[(3 * i) + 2] = node.Restr["Rz"];

                Spring[3 * i] = node.Springs["Kx"];
                Spring[(3 * i) + 1] = node.Springs["Ky"];
                Spring[(3 * i) + 2] = node.Springs["Kz"];

                Pdispl[3 * i] = node.Pdispl["Ux"];
                Pdispl[(3 * i) + 1] = node.Pdispl["Uy"];
                Pdispl[(3 * i) + 2] = node.Pdispl["Rz"];

                foreach (string loadcase in VarHolder.LoadcasesList)
                {
                    Nodal[loadcase][3 * i] = node.Forces[loadcase]["Fx"];
                    Nodal[loadcase][(3 * i) + 1] = node.Forces[loadcase]["Fy"];
                    Nodal[loadcase][(3 * i) + 2] = node.Forces[loadcase]["Mz"];
                    NAngle[loadcase][i] = node.Forces[loadcase]["Angle"];
                }
            }
            #endregion


            #region Member parameter extraction
            N1 = new int[nmembers];
            N2 = new int[nmembers];
            Elast = new double[nmembers];
            Thermal = new double[nmembers];
            Inertia = new double[nmembers];
            Area = new double[nmembers];
            Ysup = new double[nmembers];
            Yinf = new double[nmembers];
            Q0 = new Dictionary<string, double[]>();
            Q1 = new Dictionary<string, double[]>();
            QIsGlobal = new Dictionary<string, bool[]>();
            T0 = new Dictionary<string, double[]>();
            T1 = new Dictionary<string, double[]>();

            H1 = new bool[nmembers];
            H2 = new bool[nmembers];


            foreach (string loadcase in VarHolder.LoadcasesList)
            {
                Q0[loadcase] = new double[2 * nmembers];
                Q1[loadcase] = new double[2 * nmembers];
                QIsGlobal[loadcase] = new bool[nmembers];
                T0[loadcase] = new double[nmembers];
                T1[loadcase] = new double[nmembers];
            }

            for (int i = 0; i < nmembers; i++)
            {
                Member member = VarHolder.MembersList[i];

                N1[i] = VarHolder.NodesList.IndexOf(member.N1);
                N2[i] = VarHolder.NodesList.IndexOf(member.N2);

                Elast[i] = member.Material.Elasticity;
                Thermal[i] = member.Material.Thermal;

                Inertia[i] = member.Section.Inertia;
                Area[i] = member.Section.Area;
                Ysup[i] = member.Section.Ysup;
                Yinf[i] = member.Section.Yinf;

                H1[i] = member.Hinge_Start;
                H2[i] = member.Hinge_End;


                foreach (string loadcase in VarHolder.LoadcasesList)
                {
                    Q0[loadcase][2 * i] = member.Loads[loadcase]["Qx0"];
                    Q0[loadcase][(2 * i) + 1] = member.Loads[loadcase]["Qy0"];
                    Q1[loadcase][2 * i] = member.Loads[loadcase]["Qx1"];
                    Q1[loadcase][(2 * i) + 1] = member.Loads[loadcase]["Qy1"];

                    T0[loadcase][i] = member.Temperature[loadcase][0];
                    T1[loadcase][i] = member.Temperature[loadcase][1];

                    QIsGlobal[loadcase][i] = member.Loads[loadcase]["Type"] == 0;
                }
            }
            #endregion


            #region Hinge definitions
            // For each node, if it is hinged, change every connected member's respective hinge setting
            // (H1 if it is N1, H2 if N2) to true, except for the first connected member (so as to not
            // cause a singular DOF matrix.
            foreach (Node node in VarHolder.NodesList)
            {
                if (node.IsHinged)
                {
                    bool FirstHinge = true;
                    for (int i = 0; i < VarHolder.MembersList.Count; i++)
                    {
                        if (VarHolder.MembersList[i].N1 == node)
                        {
                            if (FirstHinge)
                            {
                                H1[i] = false;
                                FirstHinge = false;
                            }
                            else
                            {
                                H1[i] = true;
                            }
                        }

                        else if (VarHolder.MembersList[i].N2 == node)
                        {
                            if (FirstHinge)
                            {
                                H2[i] = false;
                                FirstHinge = false;
                            }
                            else
                            {
                                H2[i] = true;
                            }
                        }
                    }
                }
            }
            #endregion

        }



        public static void LinearAnalysis(double[] X, double[] Y, bool[] Restr, double[] RestrAngle, double[] Spring,
                                   double[] Pdispl, Dictionary<string, double[]> Nodal, Dictionary<string, double[]> NAngle,
                                   int[] N1, int[] N2, double[] Elast, double[] Thermal, double[] Inertia,
                                   double[] Area, double[] Ysup, double[] Yinf, Dictionary<string, double[]> Q0,
                                   Dictionary<string, double[]> Q1, Dictionary<string, bool[]> QIsGlobal, Dictionary<string, double[]> T0,
                                   Dictionary<string, double[]> T1, bool[] H1, bool[] H2,
                                   List<string> Loadcases, out Dictionary<string, List<double[]>> Displacements,
                                   out Dictionary<string, List<double[]>> Forces, out Dictionary<string, double[]> Reactions)
        {
            MatrixBuilder<double> MDouble = Matrix<double>.Build;
            VectorBuilder<double> VDouble = Vector<double>.Build;

            Displacements = new Dictionary<string, List<double[]>>();
            Forces = new Dictionary<string, List<double[]>>();
            Reactions = new Dictionary<string, double[]>();


            int nnodes = X.Length;
            int nmembers = N1.Length;

            int[] DOF = new int[3 * nnodes];
            List<int[]> MDOF = new List<int[]>();
            int ndof = 0;

            for (int i = 0; i < 3 * nnodes; i++)
            {
                if (Restr[i])
                {
                    DOF[i] = -1;
                }
                else
                {
                    DOF[i] = ndof;
                    ndof += 1;
                }
            }


            List<Matrix<double>> RList = new List<Matrix<double>>();
            List<Matrix<double>> KList = new List<Matrix<double>>();
            List<Matrix<double>> RIList = new List<Matrix<double>>();
            List<Vector<double>> FdList = new List<Vector<double>>();


            Matrix<double> K = MDouble.Dense(ndof, ndof);

            for (int i = 0; i < 3 * nnodes; i++)
            {
                if (DOF[i] >= 0)
                {
                    K[DOF[i], DOF[i]] += Spring[i];
                }
            }


            for (int i = 0; i < nmembers; i++)
            {
                int n1 = N1[i];
                int n2 = N2[i];

                double Lx = X[n2] - X[n1];
                double Ly = Y[n2] - Y[n1];
                double L = Math.Sqrt((Lx * Lx) + (Ly * Ly));

                double cos = Lx / L;
                double sin = Ly / L;

                double E = Elast[i];
                double I = Inertia[i];
                double A = Area[i];


                double theta1 = (Restr[3 * n1] ^ Restr[(3 * n1) + 1]) ? -RestrAngle[n1] : 0;
                double theta2 = (Restr[3 * n2] ^ Restr[(3 * n2) + 1]) ? -RestrAngle[n2] : 0;

                double cos1 = Math.Cos(theta1);
                double cos2 = Math.Cos(theta2);

                double sin1 = Math.Sin(theta1);
                double sin2 = Math.Sin(theta2);

                Matrix<double> R = MDouble.DenseOfArray(new double[,] { { cos, sin, 0, 0, 0, 0 },
                                                             { -sin, cos, 0, 0, 0, 0 },
                                                             { 0, 0, 1, 0, 0, 0 },
                                                             { 0, 0, 0, cos, sin, 0 },
                                                             { 0, 0, 0, -sin, cos, 0 },
                                                             { 0, 0, 0, 0, 0, 1 } });

                Matrix<double> RI = MDouble.DenseOfArray(new double[,] { { cos1, sin1, 0, 0, 0, 0 },
                                                              { -sin1, cos1, 0, 0, 0, 0 },
                                                              { 0, 0, 1, 0, 0, 0 },
                                                              { 0, 0, 0, cos2, sin2, 0 },
                                                              { 0, 0, 0, -sin2, cos2, 0 },
                                                              { 0, 0, 0, 0, 0, 1 } });

                int[] mdof = new int[] { DOF[3 * n1], DOF[(3 * n1) + 1], DOF[(3 * n1) + 2],
                                          DOF[3 * n2], DOF[(3 * n2) + 1], DOF[(3 * n2) + 2] };

                Vector<double> pd = VDouble.DenseOfArray(new double[] { Pdispl[3 * n1], Pdispl[(3 * n1) + 1], Pdispl[(3 * n1) + 2],
                                                              Pdispl[3 * n2], Pdispl[(3 * n2) + 1], Pdispl[(3 * n2) + 2] });
                pd = RI * pd;

                Matrix<double> KL;

                if (H1[i] && H2[i])
                {
                    double EAL = E * A / L;
                    KL = MDouble.DenseOfArray(new double[,] { { EAL, 0, 0, -EAL, 0, 0 },
                                                              { 0, 0, 0, 0, 0, 0 },
                                                              { 0, 0, 0, 0, 0, 0 },
                                                              { -EAL, 0, 0, EAL, 0, 0 },
                                                              { 0, 0, 0, 0, 0, 0 },
                                                              { 0, 0, 0, 0, 0, 0 } });
                }

                else if (H1[i])
                {
                    double[] a = { E * A / L, 3 * E * I / L / L / L, 3 * E * I / L / L, 3 * E * I / L };
                    KL = MDouble.DenseOfArray(new double[,] { { a[0], 0, 0, -a[0], 0, 0 },
                                                              { 0, a[1], 0, 0, -a[1], a[2] },
                                                              { 0, 0, 0, 0, 0, 0 },
                                                              { -a[0], 0, 0, a[0], 0, 0 },
                                                              { 0, -a[1], 0, 0, a[1], -a[2] },
                                                              { 0, a[2], 0, 0, -a[2], a[3] } });

                }

                else if (H2[i])
                {
                    double[] a = { E * A / L, 3 * E * I / L / L / L, 3 * E * I / L / L, 3 * E * I / L };
                    KL = MDouble.DenseOfArray(new double[,] { { a[0], 0, 0, -a[0], 0, 0 },
                                                              { 0, a[1], a[2], 0, -a[1], 0 },
                                                              { 0, a[2], a[3], 0, -a[2], 0 },
                                                              { -a[0], 0, 0, a[0], 0, 0 },
                                                              { 0, -a[1], -a[2], 0, a[1], 0 },
                                                              { 0, 0, 0, 0, 0, 0 } });
                }

                else
                {
                    double[] a = { E * A / L, 12 * E * I / L / L / L, 6 * E * I / L / L, 4 * E * I / L, 2 * E * I / L };
                    KL = MDouble.DenseOfArray(new double[,] { { a[0], 0, 0, -a[0], 0, 0 },
                                                              { 0, a[1], a[2], 0, -a[1], a[2] },
                                                              { 0, a[2], a[3], 0, -a[2], a[4] },
                                                              { -a[0], 0, 0, a[0], 0, 0 },
                                                              { 0, -a[1], -a[2], 0, a[1], -a[2] },
                                                              { 0, a[2], a[4], 0, -a[2], a[3] } });
                }

                Matrix<double> KG = RI.Transpose() * R.Transpose() * KL * R * RI;
                Vector<double> Fpd = KG * pd;

                KList.Add(KG);
                MDOF.Add(mdof);
                RList.Add(R);
                RIList.Add(RI);
                FdList.Add(Fpd);

                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        if (mdof[j] >= 0 && mdof[k] >= 0)
                        {
                            K[mdof[j], mdof[k]] += KG[j, k];
                        }
                    }
                }
            }

            Matrix<double> Kinv = K.Inverse();

            foreach (string loadcase in Loadcases)
            {
                Displacements[loadcase] = new List<double[]>();
                Forces[loadcase] = new List<double[]>();
                Reactions[loadcase] = new double[3 * nnodes];

                Vector<double> FN = VDouble.Dense(ndof);
                Vector<double> F0 = VDouble.Dense(ndof);
                List<Vector<double>> F0List = new List<Vector<double>>();


                for (int i = 0; i < nnodes; i++)
                {
                    double cos = Math.Cos(NAngle[loadcase][i]);
                    double sin = Math.Sin(NAngle[loadcase][i]);
                    double Fx = Nodal[loadcase][3 * i];
                    double Fy = Nodal[loadcase][(3 * i) + 1];


                    if (DOF[3 * i] >= 0)
                    {
                        FN[DOF[3 * i]] = (Fx * cos) + (Fy * sin);
                    }

                    if (DOF[(3 * i) + 1] >= 0)
                    {
                        FN[DOF[(3 * i) + 1]] = (Fx * (-sin)) + (Fx * cos);
                    }

                    if (DOF[(3 * i) + 2] >= 0)
                    {
                        FN[DOF[(3 * i) + 2]] = Nodal[loadcase][(3 * i) + 2];
                    }
                }

                for (int i = 0; i < nmembers; i++)
                {
                    int n1 = N1[i];
                    int n2 = N2[i];
                    double Lx = X[n2] - X[n1];
                    double Ly = Y[n2] - Y[n1];
                    double L = Math.Sqrt((Lx * Lx) + (Ly * Ly));

                    double E = Elast[i];
                    double alpha = Thermal[i];
                    double I = Inertia[i];
                    double A = Area[i];
                    double y0 = Ysup[i];
                    double y1 = Yinf[i];


                    double QX0 = Q0[loadcase][2 * i];
                    double QX1 = Q1[loadcase][2 * i];

                    double QY0 = Q0[loadcase][(2 * i) + 1];
                    double QY1 = Q1[loadcase][(2 * i) + 1];
                    bool Qtype = QIsGlobal[loadcase][i];



                    double Qx0 = QX0;
                    double Qx1 = QX1;
                    double Qy0 = QY0;
                    double Qy1 = QY1;

                    if (Qtype)
                    {
                        double cos = Lx / L;
                        double sin = Ly / L;

                        Qx0 = (QX0 * cos) + (QY0 * sin);
                        Qy0 = (QX0 * (-sin)) + (QY0 * cos);

                        Qx1 = (QX1 * cos) + (QY1 * sin);
                        Qy1 = (QX1 * (-sin)) + (QY1 * cos);
                    }

                    double dQ = Qy1 - Qy0;

                    double t0 = T0[loadcase][i];
                    double t1 = T1[loadcase][i];
                    double tg = ((t0 * y0) + (t1 * y1)) / (y0 + y1);
                    double dt = t0 - t1;

                    Vector<double> Fq;
                    Vector<double> FT;

                    if (H1[i] && H2[i])
                    {
                        Fq = VDouble.DenseOfArray(new double[] { ((2 * Qx0) + Qx1) * L / 6,
                                                                 0,
                                                                 0,
                                                                 ((2 * Qx1) + Qx0) * L / 6,
                                                                 0,
                                                                 0 });

                        FT = VDouble.DenseOfArray(new double[] {-E * A * alpha * tg,
                                                                 0,
                                                                 0,
                                                                 E * A * alpha * tg,
                                                                 0,
                                                                 0 });
                    }

                    else if (H1[i])
                    {
                        Fq = VDouble.DenseOfArray(new double[] { ((2 * Qx0) + Qx1) * L / 6,
                                                                  (3 * Qy0 * L / 8) + (dQ * L / 10),
                                                                  0,
                                                                  ((2 * Qx1) + Qx0) * L / 6,
                                                                  (5 * Qy0 * L / 8) + (2 * dQ * L / 5),
                                                                  (- Qy0 * L * L / 8) - (dQ * L * L / 15) });

                        FT = VDouble.DenseOfArray(new double[] { -E * A * alpha * tg,
                                                                 3 * E * I * alpha * dt / 2 / L / (y0 + y1),
                                                                 0,
                                                                 E * A * alpha * tg,
                                                                 -3 * E * I * alpha * dt / 2 / L / (y0 + y1),
                                                                 3 * E * I * alpha * dt / 2 / (y0 + y1) });
                    }

                    else if (H2[i])
                    {
                        Fq = VDouble.DenseOfArray(new double[] { ((2 * Qx0) + Qx1) * L / 6,
                                                                  (5 * Qy0 * L / 8) + (2 * dQ  * L/ 5),
                                                                  (Qy0 * L * L / 8) + (7 * dQ * L * L / 120),
                                                                  ((2 * Qx1) + Qx0) * L / 6,
                                                                  (3 * Qy0 * L / 8) + (dQ * L / 10),
                                                                  0 });

                        FT = VDouble.DenseOfArray(new double[] { -E * A * alpha * tg,
                                                                 -3 * E * I * alpha * dt / 2 / L / (y0 + y1),
                                                                 -3 * E * I * alpha * dt / 2 / (y0 + y1),
                                                                 E * A * alpha * tg,
                                                                 3 * E * I * alpha * dt / 2 / L / (y0 + y1),
                                                                 0 });
                    }

                    else
                    {
                        Fq = VDouble.DenseOfArray(new double[] { ((2 * Qx0) + Qx1) * L / 6,
                                                                  (Qy0 * L / 2) + (3 * dQ * L/ 20),
                                                                  (Qy0 * L * L / 12) + (dQ * L * L / 30),
                                                                  ((2 * Qx1) + Qx0) * L / 6,
                                                                  (Qy0 * L / 2) + (7 * dQ * L / 20),
                                                                  (-Qy0 * L * L / 12) - (dQ * L * L / 20) });

                        FT = VDouble.DenseOfArray(new double[] { -E * A * alpha * tg,
                                                                 0,
                                                                 E * I * alpha * dt / (y0 + y1),
                                                                 E * A * alpha * tg,
                                                                 0,
                                                                 -E * I * alpha * dt / (y0 + y1) });
                    }

                    Vector<double> F0L = Fq + FT;
                    Vector<double> F0G = (RIList[i].Transpose() * RList[i].Transpose() * F0L) - FdList[i];

                    F0List.Add(F0G);


                    for (int j = 0; j < 6; j++)
                    {
                        if (MDOF[i][j] >= 0)
                        {
                            F0[MDOF[i][j]] += F0G[j];
                        }
                    }
                }

                Vector<double> u = Kinv * (F0 + FN);
                double[] U = new double[3 * nnodes];

                for (int i = 0; i < 3 * nnodes; i++)
                {
                    if (DOF[i] >= 0)
                    {
                        U[i] = u[DOF[i]];
                    }
                    else
                    {
                        U[i] = Pdispl[i];
                    }
                }

                for (int i = 0; i < nmembers; i++)
                {
                    int n1 = N1[i];
                    int n2 = N2[i];
                    Vector<double> UmG = VDouble.DenseOfArray(new double[] {U[3 * n1], U[(3 * n1) + 1], U[(3 * n1) + 2],
                                                                U[3 * n2], U[(3 * n2) + 1], U[(3 * n2) + 2]});

                    Vector<double> FeG = (KList[i] * UmG) - F0List[i];

                    Vector<double> UmL = RList[i] * RIList[i] * UmG;
                    Vector<double> FeL = RList[i] * RIList[i] * FeG;

                    Displacements[loadcase].Add(UmL.ToArray());
                    Forces[loadcase].Add(FeL.ToArray());

                    for (int j = 0; j < 3; j++)
                    {

                        Reactions[loadcase][(3 * n1) + j] += FeG[j];
                        Reactions[loadcase][(3 * n2) + j] += FeG[3 + j];
                    }
                }

                for (int i = 0; i < nnodes; i++)
                {
                    double cos = Math.Cos(RestrAngle[i]);
                    double sin = Math.Sin(RestrAngle[i]);

                    double Px = Nodal[loadcase][3 * i];
                    double Py = Nodal[loadcase][(3 * i) + 1];

                    Reactions[loadcase][3 * i] -= (Px * cos) + (Py * sin);
                    Reactions[loadcase][(3 * i) + 1] -= (Px * (-sin)) + (Py * cos);
                    Reactions[loadcase][(3 * i) + 2] -= Nodal[loadcase][(3 * i) + 2];
                }
            }

        }


        public static void InternalResults(double[] X, double[] Y, int[] N1, int[] N2, double[] Elast, double[] Inertia, double[] Area,
                                           bool[] H1, bool[] H2, Dictionary<string, double[]> Q0, Dictionary<string, double[]> Q1,
                                           Dictionary<string, bool[]> QIsGlobal, Dictionary<string, List<double[]>> Displacements,
                                           Dictionary<string, List<double[]>> Forces, List<string> Loadcases,
                                           out Dictionary<string, List<double[]>> InUx, out Dictionary<string, List<double[]>> InUy,
                                           out Dictionary<string, List<double[]>> InRz, out Dictionary<string, List<double[]>> InN,
                                           out Dictionary<string, List<double[]>> InQ, out Dictionary<string, List<double[]>> InM,
                                           out Dictionary<string, List<int[]>> InMaxM, out Dictionary<string, List<int>> InMaxQ)
        {
            InUx = new Dictionary<string, List<double[]>>();
            InUy = new Dictionary<string, List<double[]>>();
            InRz = new Dictionary<string, List<double[]>>();

            InN = new Dictionary<string, List<double[]>>();
            InQ = new Dictionary<string, List<double[]>>();
            InM = new Dictionary<string, List<double[]>>();

            InMaxM = new Dictionary<string, List<int[]>>();
            InMaxQ = new Dictionary<string, List<int>>();

            int nmembers = N1.Length;

            foreach (string loadcase in Loadcases)
            {
                InUx[loadcase] = new List<double[]>();
                InUy[loadcase] = new List<double[]>();
                InRz[loadcase] = new List<double[]>();

                InN[loadcase] = new List<double[]>();
                InQ[loadcase] = new List<double[]>();
                InM[loadcase] = new List<double[]>();
                InMaxM[loadcase] = new List<int[]>();
                InMaxQ[loadcase] = new List<int>();
            }


            for (int m = 0; m < nmembers; m++)
            {

                int n1 = N1[m];
                int n2 = N2[m];

                double Lx = X[n2] - X[n1];
                double Ly = Y[n2] - Y[n1];
                double L = Math.Sqrt((Lx * Lx) + (Ly * Ly));
                double cos = Lx / L;
                double sin = Ly / L;

                double EI = Elast[m] * Inertia[m];
                double EA = Elast[m] * Area[m];

                int npoints = (int)Math.Max(1.5 * L, 25);

                foreach (string loadcase in Loadcases)
                {
                    double[] displ = new double[npoints];

                    double Qx0 = Q0[loadcase][2 * m];
                    double Qx1 = Q1[loadcase][2 * m];
                    double Qy0 = Q0[loadcase][(2 * m) + 1];
                    double Qy1 = Q1[loadcase][(2 * m) + 1];
                    bool qtype = QIsGlobal[loadcase][m];


                    double qx0 = Qx0;
                    double qx1 = Qx1;
                    double qy0 = Qy0;
                    double qy1 = Qy1;

                    if (qtype)
                    {
                        qx0 = (Qx0 * cos) + (Qy0 * sin);
                        qy0 = (Qx0 * (-sin)) + (Qy0 * cos);

                        qx1 = (Qx1 * cos) + (Qy1 * sin);
                        qy1 = (Qx1 * (-sin)) + (Qy1 * cos);
                    }

                    double[] U = Displacements[loadcase][m];
                    double[] F = Forces[loadcase][m];

                    InN[loadcase].Add(new double[npoints]);
                    InQ[loadcase].Add(new double[npoints]);
                    InM[loadcase].Add(new double[npoints]);

                    InRz[loadcase].Add(new double[npoints]);
                    InUy[loadcase].Add(new double[npoints]);
                    InUx[loadcase].Add(new double[npoints]);

                    int imaxM0, imaxM1, imaxQ;

                    if (qy0.CloseEnough(qy1))
                    {
                        double xmaxM = -F[1] / qy0;

                        imaxM0 = (xmaxM < 0 || xmaxM > L) ? -1 : (int)(xmaxM / L * (npoints - 1));
                        imaxM1 = imaxQ = -1;
                    }
                    else
                    {
                        double xmaxQ = qy0 * L / (qy0 - qy1);
                        imaxQ = (xmaxQ < 0 || xmaxQ > L)? -1 : (int)(xmaxQ / L * (npoints - 1));

                        double delta = L * L * qy0 * qy0 - (qy1 - qy0) * 2 * L * F[1];
                        
                        if (delta < 0)
                        {
                            imaxM0 = imaxM1 = -1;
                        }
                        else
                        {
                            double minusbOverTwoa = -L * qy0 / (qy1 - qy0);
                            double sqrtDeltaOverTwoa = Math.Sqrt(delta) / 2 / (qy1 - qy0);
                            
                            double xmaxM0 = minusbOverTwoa + sqrtDeltaOverTwoa;
                            double xmaxM1 = minusbOverTwoa - sqrtDeltaOverTwoa;

                            imaxM0 = (xmaxM0 < 0 || xmaxM1 > L) ? -1 : (int)(xmaxM0 / L * (npoints - 1));
                            imaxM1 = (xmaxM1 < 0 || xmaxM1 > L) ? -1 : (int)(xmaxM1 / L * (npoints - 1));
                        }
                    }


                    InMaxM[loadcase].Add(new int[2] { imaxM0, imaxM1 });
                    InMaxQ[loadcase].Add(imaxQ);

                    for (int i = 0; i < npoints; i++)
                    {
                        double x = i * L / (npoints - 1);

                        InN[loadcase][m][i] = -F[0] - (((2 * qx0) + ((qx1 - qx0) * x / L)) * x / 2);
                        InQ[loadcase][m][i] = F[1] + (qy0 * x) + ((qy1 - qy0) * x * x / 2 / L);
                        InM[loadcase][m][i] = -F[2] + (F[1] * x) + (qy0 * x * x / 2) + ((qy1 - qy0) * x * x * x / 6 / L);

                        InRz[loadcase][m][i] = (((-F[2] * x) + (F[1] * x * x / 2) + (qy0 * x * x * x / 6) + ((qy1 - qy0) * x * x * x * x / 24 / L)) / EI) + U[2];
                        InUy[loadcase][m][i] = (((-F[2] * x * x / 2) + (F[1] * x * x * x / 6) + (qy0 * x * x * x * x / 24) + ((qy1 - qy0) * x * x * x * x * x / 120 / L)) / EI) + (U[2] * x) + U[1];
                        InUx[loadcase][m][i] = (((-F[0] * x) - (qx0 * x * x / 2) - ((qx1 - qx0) * x * x * x / 6 / L)) / EA) + U[0];
                    }
                }


            }
        }
    }
}
