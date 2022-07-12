
using PAENN2.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace PAENN2.ViewModels
{
    public partial class VM_MainWindow
    {
        // Toolbox title property
        private string toolboxtitle = "";
        public string ToolboxTitle { get => toolboxtitle; set => ChangeProperty(ref toolboxtitle, value); }

        #region "New File" parameters/methods
        // Command declaration
        private readonly DelegateCommand toolbar_newfile;
        public ICommand Toolbar_Newfile => toolbar_newfile;

        public void FN_Newfile(object param)
        {
            if (VarHolder.FileChanged)
            {
                MessageBoxResult save = MessageBox.Show("Salvar as alterações na estrutura?", "Salvar arquivo", MessageBoxButton.YesNoCancel);
                if (save == MessageBoxResult.Yes)
                {

                }
                else if (save == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            FileIO.Newfile();
        }
        #endregion


        #region Statusbar parameters/methods
        private bool statusbar_gridtoggle = true;
        public bool Statusbar_GridToggle { get => statusbar_gridtoggle; set => ChangeProperty(ref statusbar_gridtoggle, value); }

        private bool statusbar_snaptoggle = false;
        public bool Statusbar_SnapToggle { get => statusbar_snaptoggle; set => ChangeProperty(ref statusbar_snaptoggle, value); }


        private double grid_y = 100;
        public double Grid_Y { get => grid_y; set => ChangeProperty(ref grid_y, value); }

        private double grid_x = 100;
        public double Grid_X { get => grid_x; set => ChangeProperty(ref grid_x, value); }
        #endregion


        #region "New Node" toolbox parameters/methods
        // Command declaration
        private readonly DelegateCommand toolbox_newnode_apply;
        public ICommand Toolbox_NewNode_Apply => toolbox_newnode_apply;


        // Labels
        private string toolbox_newnode_x = "";
        public string Toolbox_NewNode_X { get => toolbox_newnode_x; set => ChangeProperty(ref toolbox_newnode_x, value); }

        private string toolbox_newnode_y = "";
        public string Toolbox_NewNode_Y { get => toolbox_newnode_y; set => ChangeProperty(ref toolbox_newnode_y, value); }

        // Textboxes
        private string textbox_newnode_x = "";
        public string Textbox_NewNode_X { get => textbox_newnode_x; set => ChangeProperty(ref textbox_newnode_x, value); }

        private string textbox_newnode_y = "";
        public string Textbox_NewNode_Y { get => textbox_newnode_y; set => ChangeProperty(ref textbox_newnode_y, value); }


        private void FN_NewNode_Apply(object param)
        {
            try
            {
                double x = Textbox_NewNode_X.StringToDouble();
                double y = Textbox_NewNode_Y.StringToDouble();

                x = Units.ConvertLength(x, Units.Length, false);
                y = Units.ConvertLength(y, Units.Length, false);

                try
                {
                    FN_NewNodeClick(new Point(x, y));
                }
                catch (ArgumentException)
                {
                    _ = MessageBox.Show("Já existe um ponto nas coordenadas fornecidas.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Textbox_NewNode_X = "";
                Textbox_NewNode_Y = "";

            }
            catch (System.FormatException)
            {
                _ = MessageBox.Show("Insira coordenadas válidas.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void FN_NewNodeClick(Point P, double tol = 1e-5) => ActionHandler.NewNode(P, tol);

        public void FN_NewMemberClick(Point P, double tol = 1e-5) => ActionHandler.NewMember(VarHolder.NewMemberStart, P, null, null, tol);


        #endregion


        #region "New Member" toolbox parameters/methods
        // Command declaration
        private readonly DelegateCommand toolbox_newmember_apply;
        public ICommand Toolbox_NewMember_Apply => toolbox_newmember_apply;


        // Labels
        private string toolbox_newmember_x1 = "";
        public string Toolbox_NewMember_X1 { get => toolbox_newmember_x1; set => ChangeProperty(ref toolbox_newmember_x1, value); }

        private string toolbox_newmember_x2 = "";
        public string Toolbox_NewMember_X2 { get => toolbox_newmember_x2; set => ChangeProperty(ref toolbox_newmember_x2, value); }

        private string toolbox_newmember_y1 = "";
        public string Toolbox_NewMember_Y1 { get => toolbox_newmember_y1; set => ChangeProperty(ref toolbox_newmember_y1, value); }

        private string toolbox_newmember_y2 = "";
        public string Toolbox_NewMember_Y2 { get => toolbox_newmember_y2; set => ChangeProperty(ref toolbox_newmember_y2, value); }

        // Textboxes
        private string textbox_newmember_x1 = "";
        public string Textbox_NewMember_X1 { get => textbox_newmember_x1; set => ChangeProperty(ref textbox_newmember_x1, value); }

        private string textbox_newmember_x2 = "";
        public string Textbox_NewMember_X2 { get => textbox_newmember_x2; set => ChangeProperty(ref textbox_newmember_x2, value); }

        private string textbox_newmember_y1 = "";
        public string Textbox_NewMember_Y1 { get => textbox_newmember_y1; set => ChangeProperty(ref textbox_newmember_y1, value); }

        private string textbox_newmember_y2 = "";
        public string Textbox_NewMember_Y2 { get => textbox_newmember_y2; set => ChangeProperty(ref textbox_newmember_y2, value); }

        // Checkbox variables
        public ObservableCollection<string> MatList { get; set; }
        public ObservableCollection<string> SecList { get; set; }

        private string cbmaterial = "";
        public string CBMaterial { get => cbmaterial; set => ChangeProperty(ref cbmaterial, value); }

        private string cbsection = "";
        public string CBSection { get => cbsection; set => ChangeProperty(ref cbsection, value); }



        public void FN_NewMember_Apply(object param)
        {
            try
            {
                double x1 = Textbox_NewMember_X1.StringToDouble();
                double y1 = Textbox_NewMember_Y1.StringToDouble();
                double x2 = Textbox_NewMember_X2.StringToDouble();
                double y2 = Textbox_NewMember_Y2.StringToDouble();

                if (x1.CloseEnough(x2) && y1.CloseEnough(y2))
                {
                    _ = MessageBox.Show("Os pontos inicial e final devem ser diferentes.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                x1 = Units.ConvertLength(x1, Units.Length, false);
                y1 = Units.ConvertLength(y1, Units.Length, false);
                x2 = Units.ConvertLength(x2, Units.Length, false);
                y2 = Units.ConvertLength(y2, Units.Length, false);

                int matindex = 0;
                int secindex = 0;

                if (MatList.Contains(CBMaterial))
                {
                    matindex = MatList.IndexOf(CBMaterial);
                }
                else
                {
                    _ = MessageBox.Show("Selecione um material válido.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (SecList.Contains(CBSection))
                {
                    secindex = SecList.IndexOf(CBSection);
                }
                else
                {
                    _ = MessageBox.Show("Selecione uma seção válida.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                ActionHandler.NewMember(new Point(x1, y1), new Point(x2, y2), VarHolder.MaterialsList[matindex], VarHolder.SectionsList[secindex]);

                Textbox_NewMember_X1 = x2.ToString();
                Textbox_NewMember_Y1 = y2.ToString();

                Textbox_NewMember_X2 = (x2 + (x2 - x1)).ToString();
                Textbox_NewMember_Y2 = (y2 + (y2 - y1)).ToString();
            }
            catch (System.FormatException)
            {
                _ = MessageBox.Show("Insira coordenadas válidas.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (System.ArgumentException)
            {
                _ = MessageBox.Show("Já existe uma barra nas coordenadas fornecidas.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        #endregion


        #region "Manage supports" toolbox parameters
        // Boolean variables
        private bool toolbox_supports_restrx = false;
        public bool Toolbox_Supports_RestrX { get => toolbox_supports_restrx; set => ChangeProperty(ref toolbox_supports_restrx, value); }

        private bool toolbox_supports_restry = false;
        public bool Toolbox_Supports_RestrY { get => toolbox_supports_restry; set => ChangeProperty(ref toolbox_supports_restry, value); }

        private bool toolbox_supports_restrz = false;
        public bool Toolbox_Supports_RestrZ { get => toolbox_supports_restrz; set => ChangeProperty(ref toolbox_supports_restrz, value); }

        // Labels
        private string toolbox_supports_springlabel = "";
        public string Toolbox_Supports_SpringLabel { get => toolbox_supports_springlabel; set => ChangeProperty(ref toolbox_supports_springlabel, value); }

        private string toolbox_supports_pdispllabel = "";
        public string Toolbox_Supports_PdisplLabel { get => toolbox_supports_pdispllabel; set => ChangeProperty(ref toolbox_supports_pdispllabel, value); }

        private string toolbox_supports_anglelabel = "";
        public string Toolbox_Supports_AngleLabel { get => toolbox_supports_anglelabel; set => ChangeProperty(ref toolbox_supports_anglelabel, value); }


        // Textbox strings
        private string textbox_supports_angle = "";
        public string Textbox_Supports_Angle { get => textbox_supports_angle; set => ChangeProperty(ref textbox_supports_angle, value); }

        private string textbox_supports_springx = "";
        public string Textbox_Supports_SpringX { get => textbox_supports_springx; set => ChangeProperty(ref textbox_supports_springx, value); }

        private string textbox_supports_springy = "";
        public string Textbox_Supports_SpringY { get => textbox_supports_springy; set => ChangeProperty(ref textbox_supports_springy, value); }

        private string textbox_supports_springz = "";
        public string Textbox_Supports_SpringZ { get => textbox_supports_springz; set => ChangeProperty(ref textbox_supports_springz, value); }

        private string textbox_supports_pdisplx = "";
        public string Textbox_Supports_PdisplX { get => textbox_supports_pdisplx; set => ChangeProperty(ref textbox_supports_pdisplx, value); }

        private string textbox_supports_pdisply = "";
        public string Textbox_Supports_PdisplY { get => textbox_supports_pdisply; set => ChangeProperty(ref textbox_supports_pdisply, value); }

        private string textbox_supports_pdisplz = "";
        public string Textbox_Supports_PdisplZ { get => textbox_supports_pdisplz; set => ChangeProperty(ref textbox_supports_pdisplz, value); }


        // Command declaration
        private DelegateCommand toolbox_supports_apply;
        public ICommand Toolbox_Supports_Apply => toolbox_supports_apply;


        private void FN_Supports_Apply(object param)
        {
            try
            {
                double rx = Toolbox_Supports_RestrX ? 1d : 0d;
                double ry = Toolbox_Supports_RestrY ? 1d : 0d;
                double rz = Toolbox_Supports_RestrZ ? 1d : 0d;
                double angle = Textbox_Supports_Angle.StringToDouble();

                double kx = Toolbox_Supports_RestrX ? 0d : Textbox_Supports_SpringX.StringToDouble();
                double ky = Toolbox_Supports_RestrY ? 0d : Textbox_Supports_SpringY.StringToDouble();
                double kz = Toolbox_Supports_RestrZ ? 0d : Textbox_Supports_SpringZ.StringToDouble();

                double pdx = (!Toolbox_Supports_RestrX) ? 0d : Textbox_Supports_PdisplX.StringToDouble();
                double pdy = (!Toolbox_Supports_RestrY) ? 0d : Textbox_Supports_PdisplY.StringToDouble();
                double pdz = (!Toolbox_Supports_RestrZ) ? 0d : Textbox_Supports_PdisplZ.StringToDouble();



                VarHolder.AppliedRestr = new double[] { rx, ry, rz, angle, kx, ky, kz, pdx, pdy, pdz };
                VarHolder.ClickType = "Supports";
            }
            catch (System.FormatException)
            {
                _ = MessageBox.Show("Digite valores válidos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region "Add Hinges" toolbox parameters
        private int hinge_type = 0;
        public int Hinge_Type { get => hinge_type; set => ChangeProperty(ref hinge_type, value); }


        // Command declaration
        private DelegateCommand toolbox_hinge_apply;
        public ICommand Toolbox_Hinge_Apply => toolbox_hinge_apply;

        private DelegateCommand toolbox_hinge_remove;
        public ICommand Toolbox_Hinge_Remove => toolbox_hinge_remove;



        public void FN_Hinge_Apply(object param)
        {
            switch (Hinge_Type)
            {
                case 0:
                    VarHolder.ClickType = "NodeHinge";
                    break;
                case 1:
                    VarHolder.ClickType = "StartHinge";
                    break;
                case 2:
                    VarHolder.ClickType = "EndHinge";
                    break;
                case 3:
                    VarHolder.ClickType = "BothHinge";
                    break;
                default:
                    break;
            }
        }

        public void FN_Hinge_Remove(object param)
        {
            VarHolder.ClickType = "RemoveHinge";
        }
        #endregion

        #region "Nodal forces" toolbox parameters
        // Command declaration
        private readonly DelegateCommand toolbox_nodal_apply;
        public ICommand Toolbox_Nodal_Apply => toolbox_nodal_apply;


        // Labels
        private string toolbox_nodal_fx = "";
        public string Toolbox_Nodal_Fx { get => toolbox_nodal_fx; set => ChangeProperty(ref toolbox_nodal_fx, value); }

        private string toolbox_nodal_fy = "";
        public string Toolbox_Nodal_Fy { get => toolbox_nodal_fy; set => ChangeProperty(ref toolbox_nodal_fy, value); }

        private string toolbox_nodal_mz = "";
        public string Toolbox_Nodal_Mz { get => toolbox_nodal_mz; set => ChangeProperty(ref toolbox_nodal_mz, value); }

        private string toolbox_nodal_angle = "";
        public string Toolbox_Nodal_Angle { get => toolbox_nodal_angle; set => ChangeProperty(ref toolbox_nodal_angle, value); }

        // Textboxes
        private string textbox_nodal_fx = "";
        public string Textbox_Nodal_Fx { get => textbox_nodal_fx; set => ChangeProperty(ref textbox_nodal_fx, value); }

        private string textbox_nodal_fy = "";
        public string Textbox_Nodal_Fy { get => textbox_nodal_fy; set => ChangeProperty(ref textbox_nodal_fy, value); }


        private string textbox_nodal_mz = "";
        public string Textbox_Nodal_Mz { get => textbox_nodal_mz; set => ChangeProperty(ref textbox_nodal_mz, value); }

        private string textbox_nodal_angle = "";
        public string Textbox_Nodal_Angle { get => textbox_nodal_angle; set => ChangeProperty(ref textbox_nodal_angle, value); }

        private void FN_Nodal_Apply(object param)
        {
            try
            {
                double fx = Units.ConvertForce(Textbox_Nodal_Fx.StringToDouble(), Units.Force, false);
                double fy = Units.ConvertForce(Textbox_Nodal_Fy.StringToDouble(), Units.Force, false);
                double mz = Units.ConvertMoment(Textbox_Nodal_Mz.StringToDouble(), Units.Moment, false);
                double angle = Units.ConvertAngle(Textbox_Nodal_Angle.StringToDouble(), Units.Angle, false);

                if (angle >= 360)
                {
                    while (angle >= 360)
                    {
                        angle -= 360;
                    }
                }

                if (angle < 0)
                {
                    while (angle < 0)
                    {
                        angle += 360;
                    }
                }

                VarHolder.AppliedNodal = new double[] { fx, fy, mz, angle };
                VarHolder.ClickType = "Nodal";
            }
            catch (System.FormatException)
            {
                _ = MessageBox.Show("Digite valores válidos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion


        #region "Member loads" toolbox parameters
        // Boolean variables
        private bool toolbox_loads_isglobal = true;
        public bool Toolbox_Loads_IsGlobal { get => toolbox_loads_isglobal; set => ChangeProperty(ref toolbox_loads_isglobal, value); }

        private bool toolbox_loads_isxlinear = false;
        public bool Toolbox_Loads_IsXLinear { get => toolbox_loads_isxlinear; set => ChangeProperty(ref toolbox_loads_isxlinear, value); }

        private bool toolbox_loads_isylinear = false;
        public bool Toolbox_Loads_IsYLinear { get => toolbox_loads_isylinear; set => ChangeProperty(ref toolbox_loads_isylinear, value); }


        // Labels
        private string toolbox_loads_q0 = "";
        public string Toolbox_Loads_Q0 { get => toolbox_loads_q0; set => ChangeProperty(ref toolbox_loads_q0, value); }

        private string toolbox_loads_q1 = "";
        public string Toolbox_Loads_Q1 { get => toolbox_loads_q1; set => ChangeProperty(ref toolbox_loads_q1, value); }


        // Textbox strings
        private string textbox_loads_qx0 = "";
        public string Textbox_Loads_Qx0 { get => textbox_loads_qx0; set => ChangeProperty(ref textbox_loads_qx0, value); }


        private string textbox_loads_qx1 = "";
        public string Textbox_Loads_Qx1 { get => textbox_loads_qx1; set => ChangeProperty(ref textbox_loads_qx1, value); }


        private string textbox_loads_qy0 = "";
        public string Textbox_Loads_Qy0 { get => textbox_loads_qy0; set => ChangeProperty(ref textbox_loads_qy0, value); }

        private string textbox_loads_qy1 = "";
        public string Textbox_Loads_Qy1 { get => textbox_loads_qy1; set => ChangeProperty(ref textbox_loads_qy1, value); }




        // Command declaration
        private DelegateCommand toolbox_loads_apply;
        public ICommand Toolbox_Loads_Apply => toolbox_loads_apply;

        private void FN_Loads_Apply(object param)
        {
            try
            {
                double qx0 = Units.ConvertLoad(Textbox_Loads_Qx0.StringToDouble(), Units.Load, false);
                double qx1 = Units.ConvertLoad(Textbox_Loads_Qx1.StringToDouble(), Units.Load, false);
                double qy0 = Units.ConvertLoad(Textbox_Loads_Qy0.StringToDouble(), Units.Load, false);
                double qy1 = Units.ConvertLoad(Textbox_Loads_Qy1.StringToDouble(), Units.Load, false);
                double loadtype = 0d;

                if (!Toolbox_Loads_IsXLinear)
                {
                    qx1 = qx0;
                }

                if (!Toolbox_Loads_IsYLinear)
                {
                    qy1 = qy0;
                }

                if (Toolbox_Loads_IsGlobal)
                {
                    loadtype = 1d;
                }

                VarHolder.AppliedLoads = new double[] { qx0, qx1, qy0, qy1, loadtype };
                VarHolder.ClickType = "Loads";
            }
            catch (System.FormatException)
            {
                _ = MessageBox.Show("Digite valores válidos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
