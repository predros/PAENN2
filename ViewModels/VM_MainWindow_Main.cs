using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PAENN2.Models;


namespace PAENN2.ViewModels
{
    public partial class VM_MainWindow : Observable
    {
        #region DelegateCommand declaration
        private readonly DelegateCommand toolbar_undo;
        private readonly DelegateCommand toolbar_redo;
        private readonly DelegateCommand toolbar_run;
        private readonly DelegateCommand comm_delete;

        private readonly DelegateCommand open_select;
        private readonly DelegateCommand open_newnode;
        private readonly DelegateCommand open_newmember;
        private readonly DelegateCommand open_materials;
        private readonly DelegateCommand open_sections;
        private readonly DelegateCommand open_supports;
        private readonly DelegateCommand open_hinges;
        private readonly DelegateCommand open_nodal;
        private readonly DelegateCommand open_loads;
        private readonly DelegateCommand open_thermal;
        #endregion


        #region ICommand declaration
        public ICommand Toolbar_Undo => toolbar_undo;
        public ICommand Toolbar_Redo => toolbar_redo;
        public ICommand Toolbar_Run => toolbar_run;
        public ICommand Comm_Delete => comm_delete;


        public ICommand Open_Select => open_select;
        public ICommand Open_NewNode => open_newnode;
        public ICommand Open_NewMember => open_newmember;
        public ICommand Open_Materials => open_materials;
        public ICommand Open_Sections => open_sections;
        public ICommand Open_Supports => open_supports;
        public ICommand Open_Hinges => open_hinges;
        public ICommand Open_Nodal => open_nodal;
        public ICommand Open_Loads => open_loads;
        public ICommand Open_Thermal => open_thermal;
        #endregion


        #region Toolbox startup methods
        private void DisableAllToolboxes()
        {
            Vis_NewNode = false;
            Vis_NewMember = false;
            Vis_Supports = false;
            Vis_Hinges = false;
            Vis_Nodal = false;
            Vis_Loads = false;
            Vis_Thermal = false;
            Vis_Results = false;

        }

        private void FN_Select(object param)
        {
            ToolboxTitle = "";

            VarHolder.ClickType = "Select";

            DisableAllToolboxes();
        }


        private void FN_NewNode(object param)
        {
            ToolboxTitle = "Adicionar pontos";
            Toolbox_NewNode_X = $"Coord. X ({Units.Length}):";
            Toolbox_NewNode_Y = $"Coord. Y ({Units.Length }):";

            VarHolder.ClickType = "NewNode";

            DisableAllToolboxes();
            Vis_NewNode = true;
        }

        private void FN_NewMember(object param)
        {
            ToolboxTitle = "Adicionar barra";

            Toolbox_NewMember_X1 = $"X1 {Units.Length}):";
            Toolbox_NewMember_X2 = $"X2 ({Units.Length}):";
            Toolbox_NewMember_Y1 = $"Y1 ({Units.Length}):";
            Toolbox_NewMember_Y2 = $"Y2 ({Units.Length}):";

            VarHolder.ClickType = "NewMember_Start";

            MatList.Clear();
            SecList.Clear();

            foreach (MemberMat material in VarHolder.MaterialsList)
            {
                MatList.Add(material.Name);
            }

            foreach (MemberSec section in VarHolder.SectionsList)
            {
                SecList.Add(section.Name);
            }

            if (MatList.Count > 0)
            {
                CBMaterial = MatList[0];
            }

            if (SecList.Count > 0)
            {
                CBSection = SecList[0];
            }

            DisableAllToolboxes();
            Vis_NewMember = true;
        }

        private void FN_Materials(object param)
        {
            ToolboxTitle = "";
            VarHolder.ClickType = "Select";

            DisableAllToolboxes();
        }

        private void FN_Sections(object param)
        {
            ToolboxTitle = "";
            VarHolder.ClickType = "Select";


            DisableAllToolboxes();
        }

        private void FN_Supports(object param)
        {
            ToolboxTitle = "Restrições nodais";
            VarHolder.ClickType = "Select";

            Toolbox_Supports_PdisplLabel = $"Desloc. ({Units.Displacement})";
            Toolbox_Supports_SpringLabel = $"Constantes ({Units.Spring})";
            Toolbox_Supports_AngleLabel = $"Ângulo ({Units.Angle}):";

            Toolbox_Supports_RestrX = Toolbox_Supports_RestrY = Toolbox_Supports_RestrZ = false;

            DisableAllToolboxes();
            Vis_Supports = true;
        }

        private void FN_Hinges(object param)
        {
            ToolboxTitle = "Adicionar rótulas";
            VarHolder.ClickType = "Select";

            DisableAllToolboxes();
            Vis_Hinges = true;
        }

        private void FN_Nodal(object param)
        {
            ToolboxTitle = "Forças nodais";
            VarHolder.ClickType = "Select";

            Toolbox_Nodal_Fx = $"Fx ({Units.Force}):";
            Toolbox_Nodal_Fy = $"Fy ({Units.Force}):";
            Toolbox_Nodal_Mz = $"Mz ({Units.Force}):";
            Toolbox_Nodal_Angle = $"Ângulo β ({Units.Angle}):";

            DisableAllToolboxes();
            Vis_Nodal = true;
        }

        private void FN_Loads(object param)
        {
            ToolboxTitle = "Cargas distribuídas";
            VarHolder.ClickType = "Select";

            Toolbox_Loads_Q0 = $"   Q0\n({Units.Load})";
            Toolbox_Loads_Q1 = $"   Q1\n({Units.Load})";

            Toolbox_Loads_IsGlobal = true;
            Toolbox_Loads_IsXLinear = Toolbox_Loads_IsYLinear = false;

            DisableAllToolboxes();
            Vis_Loads = true;
        }

        private void FN_Thermal(object param)
        {
            ToolboxTitle = "Cargas térmicas";
            VarHolder.ClickType = "Select";


            DisableAllToolboxes();
            Vis_Thermal = true;
        }

        private void FN_Run(object param)
        {
            ActionHandler.Run();

            ToolboxTitle = "Resultados";

            DisableAllToolboxes();
            Vis_Results = true;

            ShowResults(0);
        }
        #endregion


        #region Visibility booleans
        private bool vis_newnode = false;
        public bool Vis_NewNode { get => vis_newnode; set => ChangeProperty(ref vis_newnode, value); }

        private bool vis_newmember = false;
        public bool Vis_NewMember { get => vis_newmember; set => ChangeProperty(ref vis_newmember, value); }

        private bool vis_supports = false;
        public bool Vis_Supports { get => vis_supports; set => ChangeProperty(ref vis_supports, value); }

        private bool vis_hinges = false;
        public bool Vis_Hinges { get => vis_hinges; set => ChangeProperty(ref vis_hinges, value); }

        private bool vis_nodal = false;
        public bool Vis_Nodal { get => vis_nodal; set => ChangeProperty(ref vis_nodal, value); }

        private bool vis_loads = false;
        public bool Vis_Loads { get => vis_loads; set => ChangeProperty(ref vis_loads, value); }

        private bool vis_thermal = false;
        public bool Vis_Thermal { get => vis_thermal; set => ChangeProperty(ref vis_thermal, value); }

        private bool vis_results = false;
        public bool Vis_Results { get => vis_results; set => ChangeProperty(ref vis_results, value); }
        #endregion


        #region Toolbar ICommand methods
        public void FN_Undo(object param) => ActionHandler.Undo();

        public void FN_Redo(object param) => ActionHandler.Redo();

        public void FN_Delete(object param) => ActionHandler.DeleteSelection();
        #endregion


        public VM_MainWindow()
        {
            #region DelegateCommand initialization
            toolbar_undo = new DelegateCommand(FN_Undo);
            toolbar_redo = new DelegateCommand(FN_Redo);
            toolbar_run = new DelegateCommand(FN_Run);
            comm_delete = new DelegateCommand(FN_Delete);

            open_select = new DelegateCommand(FN_Select);
            open_newnode = new DelegateCommand(FN_NewNode);
            open_newmember = new DelegateCommand(FN_NewMember);
            open_materials = new DelegateCommand(FN_Materials);
            open_sections = new DelegateCommand(FN_Sections);
            open_supports = new DelegateCommand(FN_Supports);
            open_hinges = new DelegateCommand(FN_Hinges);
            open_nodal = new DelegateCommand(FN_Nodal);
            open_loads = new DelegateCommand(FN_Loads);
            open_thermal = new DelegateCommand(FN_Thermal);

            toolbox_newnode_apply = new DelegateCommand(FN_NewNode_Apply);
            toolbox_newmember_apply = new DelegateCommand(FN_NewMember_Apply);
            toolbox_nodal_apply = new DelegateCommand(FN_Nodal_Apply);
            toolbox_loads_apply = new DelegateCommand(FN_Loads_Apply);
            toolbox_supports_apply = new DelegateCommand(FN_Supports_Apply);
            toolbox_hinge_apply = new DelegateCommand(FN_Hinge_Apply);
            toolbox_hinge_remove = new DelegateCommand(FN_Hinge_Remove);

            toolbar_newfile = new DelegateCommand(FN_Newfile);
            #endregion


            MemberMat c = new MemberMat("dababy", 2380, 0.000001);
            MemberSec d = new MemberSec("bed");
            d.Circle(20, 0);
            VarHolder.MaterialsList.Add(c);
            VarHolder.SectionsList.Add(d);

            MatList = new ObservableCollection<string>();
            SecList = new ObservableCollection<string>();

        }


        public void Zoom()
        {
            foreach (Node node in VarHolder.NodesList)
            {
                node.Reposition();
            }

            foreach (Member member in VarHolder.MembersList)
            {
                member.Reposition();
            }

            if (VarHolder.ClickType.Length > 7 && VarHolder.ClickType.Substring(0, 7) == "Results")
            {
                ActionHandler.ResultPosition();
            }
        }


        public void SelectionBox(Point P1, Point P2, bool IsBlueSelection) => ActionHandler.SelectionBox(P1, P2, IsBlueSelection);


        public void ClearSelection() => ActionHandler.ClearSelection();


        public void ShowResults(int ChangeType = 0)
        {
            if (VarHolder.ResultsAvailable)
            {
                switch (ChangeType)
                {
                    case 0:
                        break;
                    case 1:
                        VarHolder.ClickType = "ResultsDispl";
                        break;
                    case 2:
                        VarHolder.ClickType = "ResultsNF";
                        break;
                    case 3:
                        VarHolder.ClickType = "ResultsSF";
                        break;
                    case 4:
                        VarHolder.ClickType = "ResultsBM";
                        break;
                    default:
                        throw new System.ArgumentException("Invalid click-type passed as parameter.");
                }
                ActionHandler.ResultShow((ChangeType != 0));

            }

        }

    }
}
