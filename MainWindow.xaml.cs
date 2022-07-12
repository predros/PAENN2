using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PAENN2
{
    /// <summary>
    /// Interação lógica para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModels.VM_MainWindow VM = new ViewModels.VM_MainWindow();
        private Point AnchorPoint_Window = new Point();
        private Point AnchorPoint_Canvas = new Point();
        private System.Windows.Shapes.Rectangle SelectionBox = new System.Windows.Shapes.Rectangle();
        private System.Windows.Shapes.Line NewMember = new System.Windows.Shapes.Line { Stroke = Brushes.LightSteelBlue, StrokeThickness = 2 };

        public MainWindow()
        {
            DataContext = VM;
            InitializeComponent();
            VarHolder.ClickType = "Select";

            Scroll.ScrollToVerticalOffset((CanvasBorder.Height / 2) - 400);
            Scroll.ScrollToHorizontalOffset((CanvasBorder.Width / 2) - 200);


            CanvasBorder.Background = VarHolder.CanvasBG;
            CanvasBorder.Background.Transform = new MatrixTransform();
            Statusbar_GridToggle();
        }



        private void Cnv_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            e.Handled = true;

            Point p = e.GetPosition(Cnv);
            Point q = e.GetPosition(CanvasBorder);
            double f = e.Delta > 0 ? 1.15 : 1 / 1.15;
            VarHolder.Transfmatrix.ScaleAt(f, f, p.X, p.Y);

            Matrix matrix = ((MatrixTransform)CanvasBorder.Background.Transform).Value;
            matrix.ScaleAt(f, f, q.X, q.Y);
            ((MatrixTransform)CanvasBorder.Background.Transform).Matrix = matrix;

            VarHolder.GridPen.Thickness /= f;

            VM.Zoom();
        }

        private void Cnv_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() != typeof(Border) && e.OriginalSource != NewMember)
            {
                e.Handled = true;
                return;
            }

            AnchorPoint_Window = e.GetPosition(this);
            Trace.WriteLine(AnchorPoint_Window);
            Trace.WriteLine(VarHolder.Transfmatrix.M11);


            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Matrix matrix = VarHolder.Transfmatrix;
                matrix.Invert();

                AnchorPoint_Canvas = e.GetPosition(Cnv);
                Point p = matrix.Transform(AnchorPoint_Canvas);



                if (VM.Statusbar_SnapToggle)
                {
                    double X_Round = Math.Round(p.X / VM.Grid_X) * VM.Grid_X;
                    double Y_Round = Math.Round(p.Y / VM.Grid_Y) * VM.Grid_Y;

                    Point RoundPoint = VarHolder.Transfmatrix.Transform(new Point(X_Round, Y_Round));

                    double deltax = RoundPoint.X - AnchorPoint_Canvas.X;
                    double deltay = RoundPoint.Y - AnchorPoint_Canvas.Y;

                    Native.SetCursor((int)(AnchorPoint_Window.X + 8 + deltax), (int)(AnchorPoint_Window.Y + 31 - deltay));

                    p = new Point(X_Round, Y_Round);
                }



                switch (VarHolder.ClickType)
                {
                    case "Select":
                        VM.ClearSelection();
                        Helper.AddToCanvas(VarHolder.Content, SelectionBox);

                        SelectionBox.Width = 0;
                        SelectionBox.Height = 0;
                        break;
                    case "NewNode":
                        try { VM.FN_NewNodeClick(p, 8); }
                        catch (ArgumentException) { return; }
                        break;
                    case "NewMember_Start":
                        VarHolder.NewMemberStart = p;
                        VarHolder.ClickType = "NewMember_End";
                        break;
                    case "NewMember_End":
                        try
                        {
                            VM.FN_NewMemberClick(p, 8);
                            VarHolder.ClickType = "NewMember_Start";
                        }
                        catch (ArgumentException) { return; }
                        break;
                    default:
                        return;
                }
            }
            else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                switch (VarHolder.ClickType)
                {
                    case "NewMember_End":
                        VarHolder.NewMemberStart = new Point();
                        VarHolder.ClickType = "NewMember_Start";
                        break;
                }
            }
        }

        private void Cnv_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            e.Handled = true;

            if (VarHolder.ClickType == "NewMember_End" && VarHolder.NewMemberStart != null)
            {
                Point p = e.GetPosition(Cnv);
                Point p0 = VarHolder.Transfmatrix.Transform(VarHolder.NewMemberStart);

                NewMember.X1 = p.X;
                NewMember.Y1 = p.Y;
                NewMember.X2 = p0.X;
                NewMember.Y2 = p0.Y;

                if (!VarHolder.Content.Children.Contains(NewMember))
                {
                    _ = VarHolder.Content.Children.Add(NewMember);
                }
            }
            else
            {
                VarHolder.Content.Children.Remove(NewMember);
            }


            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Vector delta = e.GetPosition(this) - AnchorPoint_Window;
                Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset - delta.Y);
                Scroll.ScrollToHorizontalOffset(Scroll.HorizontalOffset - delta.X);
                AnchorPoint_Window = e.GetPosition(this);

            }

            else if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if (VarHolder.ClickType == "Select")
                {
                    Point p = e.GetPosition(Cnv);

                    Canvas.SetLeft(SelectionBox, Math.Min(p.X, AnchorPoint_Canvas.X));
                    Canvas.SetTop(SelectionBox, Math.Min(p.Y, AnchorPoint_Canvas.Y));

                    SelectionBox.Width = Math.Abs(p.X - AnchorPoint_Canvas.X);
                    SelectionBox.Height = Math.Abs(p.Y - AnchorPoint_Canvas.Y);

                    if (p.X > AnchorPoint_Canvas.X)
                    {
                        SelectionBox.Stroke = Brushes.Blue;
                    }
                    else
                    {
                        SelectionBox.Stroke = Brushes.DarkGreen;
                    }
                }
            }

        }

        private void Cnv_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                if (VarHolder.Content.Children.Contains(SelectionBox))
                {
                    VarHolder.Content.Children.Remove(SelectionBox);
                }

                Matrix matrix = VarHolder.Transfmatrix;
                matrix.Invert();

                Point p1 = matrix.Transform(e.GetPosition(Cnv));
                Point p2 = matrix.Transform(AnchorPoint_Canvas);
                bool type = (p1.X > p2.X) ? true : false;



                if (VarHolder.ClickType == "Select")
                {
                    VM.SelectionBox(p1, p2, type);
                }
            }
        }

        private void Cnv_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (VarHolder.Content.Children.Contains(SelectionBox))
            {
                VarHolder.Content.Children.Remove(SelectionBox);
            }
        }

        private void CB_NewMember_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = CB_Materials.SelectedIndex;
            int j = CB_Sections.SelectedIndex;
            if (i >= 0)
            {
                VarHolder.CurrentMaterial = VarHolder.MaterialsList[i];
            }

            if (j >= 0)
            {
                VarHolder.CurrentSection = VarHolder.SectionsList[j];
            }
        }

        private void RB_Results_Checked(object sender, RoutedEventArgs e)
        {
            int check = 1;

            RadioButton rb = (RadioButton)sender;

            if (rb == RB_Results_Displacement)
            {
                check = 1;
            }
            else if (rb == RB_Results_Normal)
            {
                check = 2;
            }
            else if (rb == RB_Results_Shear)
            {
                check = 3;
            }
            else
            {
                check = 4;
            }

            VM.ShowResults(check);
        }

        private void Slider_Results_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            double f = e.NewValue / slider.Maximum;

            VarHolder.ResultScale = (0.333 * Math.Exp(2.773 * f)) - 0.333;

            VM.ShowResults();
        }

        private void Statusbar_GridToggle()
        {
            if (VM.Statusbar_GridToggle)
            {
                VarHolder.GridPen.Brush = Brushes.Gray;
                GeometryDrawing geo = new GeometryDrawing {
                    Geometry = new RectangleGeometry { Rect = new Rect(0, 0, VM.Grid_X, VM.Grid_Y) },
                    Brush = Brushes.White,
                    Pen = VarHolder.GridPen
                };
                VarHolder.CanvasBG.Drawing = geo;
                VarHolder.CanvasBG.Viewport = new Rect(0, 0, VM.Grid_X, VM.Grid_Y);
            }
            else
            {
                VarHolder.GridPen.Brush = Brushes.White;
            }
        }

        private void Statusbar_GridCB_Checked(object sender, RoutedEventArgs e) => Statusbar_GridToggle();
    }
}
