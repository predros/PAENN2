using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PAENN2.Models;

namespace PAENN2.ViewModels
{

    public class NodeInterface : Observable
    {
        // Every NodeInterface instance should be associated with a parent Node instance
        private Node Parent;

        // The node's canvas representation (5x5 square)
        public Image img = new Image();

        // Nodal force arrows
        public Image img_Fx = new Image { RenderTransform = new RotateTransform() };
        public Image img_Fy = new Image { RenderTransform = new RotateTransform() };
        public Image img_Mz = new Image { RenderTransform = new ScaleTransform() };


        // Support icons
        public Image img_Rx = new Image();
        public Image img_Ry = new Image();
        public Image img_Rz = new Image();


        private string text_Fx = "";
        public string Text_Fx { get => text_Fx; set => ChangeProperty(ref text_Fx, value); }

        private string text_Fy = "";
        public string Text_Fy { get => text_Fy; set => ChangeProperty(ref text_Fy, value); }

        private string text_Mz = "";
        public string Text_Mz { get => text_Mz; set => ChangeProperty(ref text_Mz, value); }

        public TextBlock textblock_Fx = new TextBlock
        {
            Foreground = Brushes.Red,
            Background = Brushes.White,
            RenderTransform = new TransformGroup { Children = { new ScaleTransform(), new RotateTransform() } }
        };

        public TextBlock textblock_Fy = new TextBlock
        {
            Foreground = Brushes.Red,
            Background = Brushes.White,
            RenderTransform = new TransformGroup { Children = { new ScaleTransform(), new RotateTransform() } }
        };

        public TextBlock textblock_Mz = new TextBlock
        {
            Foreground = Brushes.Blue,
            Background = Brushes.White,
            RenderTransform = new ScaleTransform()
        };

        public NodeInterface()
        {
            WriteableBitmap bmp = BitmapFactory.New(12, 12);
            bmp.FillRectangle(3, 3, 9, 9, Colors.Black);

            img.Source = bmp;
            img.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(NodeClick);

            Panel.SetZIndex(img, 10);
            Panel.SetZIndex(img_Fx, 9);
            Panel.SetZIndex(img_Fy, 9);
            Panel.SetZIndex(img_Mz, 9);

            Helper.AddToCanvas(VarHolder.Content, img, img_Fx, img_Fy, img_Mz,
                               img_Rx, img_Ry, img_Rz,
                               textblock_Fx, textblock_Fy, textblock_Mz);


            Binding binding_Fx = new Binding
            {
                Source = this,
                Path = new PropertyPath("Text_Fx"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            Binding binding_Fy = new Binding
            {
                Source = this,
                Path = new PropertyPath("Text_Fy"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            Binding binding_Mz = new Binding
            {
                Source = this,
                Path = new PropertyPath("Text_Mz"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            _ = textblock_Fx.SetBinding(TextBlock.TextProperty, binding_Fx);
            _ = textblock_Fy.SetBinding(TextBlock.TextProperty, binding_Fy);
            _ = textblock_Mz.SetBinding(TextBlock.TextProperty, binding_Mz);
        }

        public void SetParent(Node node)
        {
            Parent = node;
            Reposition();
        }


        public void Reposition()
        {
            Point p = VarHolder.Transfmatrix.Transform(Parent.Coords);
            string loadcase = VarHolder.CurrentLoadcase;


            int restr = 0;

            if (Parent.Restr["Rx"]) restr += 1;
            if (Parent.Restr["Ry"]) restr += 2;
            if (Parent.Restr["Rz"]) restr += 4;

            Canvas.SetTop(img, p.Y - 6);
            Canvas.SetLeft(img, p.X - 6);

            if (VarHolder.ClickType != "ResultsDispl")
            {
                int hx = (int)Helper.LinInterp(VarHolder.MinForce, 25, VarHolder.MaxForce,
                                               50, Math.Abs(Parent.Forces[loadcase]["Fx"]));
                int hy = (int)Helper.LinInterp(VarHolder.MinForce, 25, VarHolder.MaxForce,
                                               50, Math.Abs(Parent.Forces[loadcase]["Fy"]));

                ((RotateTransform)((TransformGroup)textblock_Fx.RenderTransform).Children[1]).CenterX = 20 - hx;
                ((RotateTransform)((TransformGroup)textblock_Fy.RenderTransform).Children[1]).CenterY = hy + 10;

                Canvas.SetLeft(img_Fy, p.X - 4);
                Canvas.SetTop(img_Fy, p.Y - hy - 2);
                Canvas.SetLeft(textblock_Fy, p.X + 10);
                Canvas.SetTop(textblock_Fy, p.Y - hy - 10);

                Canvas.SetLeft(img_Fx, p.X + 6);
                Canvas.SetTop(img_Fx, p.Y - 4);
                Canvas.SetLeft(textblock_Fx, p.X + hx - 20);
                Canvas.SetTop(textblock_Fx, p.Y + 20);

                Canvas.SetLeft(img_Mz, p.X - 20.5);
                Canvas.SetTop(img_Mz, p.Y - 20.5);
                Canvas.SetLeft(textblock_Mz, p.X + 15);
                Canvas.SetTop(textblock_Mz, p.Y - 10);
            }
            else
            {
                img_Fx.Source = null;
                img_Fy.Source = null;
                img_Mz.Source = null;
                textblock_Fx.Text = "";
                textblock_Fy.Text = "";
                textblock_Mz.Text = "";
            }


            switch (restr)
            {
                case 1: // Rx only ("001")
                    Canvas.SetLeft(img_Rx, p.X - 15);
                    Canvas.SetTop(img_Rx, p.Y - 30);
                    break;
                case 2: // Ry only ("010")
                    Canvas.SetLeft(img_Ry, p.X - 15);
                    Canvas.SetTop(img_Ry, p.Y - 30);
                    break;
                case 3: // Rx and Ry ("011")
                    Canvas.SetLeft(img_Rx, p.X - 15);
                    Canvas.SetTop(img_Rx, p.Y - 30);
                    break;
                case 4: // Rz only ("100")
                    Canvas.SetLeft(img_Rz, p.X - (15 / 2));
                    Canvas.SetTop(img_Rz, p.Y - (15 / 2));
                    break;
                case 5: // Rx and Rz ("101")
                    Canvas.SetLeft(img_Rx, p.X - 15);
                    Canvas.SetTop(img_Rx, p.Y - 13);
                    break;
                case 6: // Ry and Rz ("110")
                    Canvas.SetLeft(img_Ry, p.X - 15);
                    Canvas.SetTop(img_Ry, p.Y - 13);
                    break;
                case 7: // Rx, Ry and Rz ("111")
                    Canvas.SetLeft(img_Rx, p.X - 15);
                    Canvas.SetTop(img_Rx, p.Y - 8);
                    break;
                default:
                    break;
            }

            if (!Parent.Springs["Kx"].CloseEnough(0))
            {
                Canvas.SetLeft(img_Rx, p.X - 15);
                Canvas.SetTop(img_Rx, p.Y - 30);
            }


            if (!Parent.Springs["Ky"].CloseEnough(0))
            {
                Canvas.SetLeft(img_Ry, p.X - 15);
                Canvas.SetTop(img_Ry, p.Y - 30);
            }


            if (!Parent.Springs["Kz"].CloseEnough(0))
            {
                Canvas.SetLeft(img_Rz, p.X - 17);
                Canvas.SetTop(img_Rz, p.Y - 26);
            }
        }


        public void RemoveFromCanvas() =>
            Helper.RemoveFromCanvas(VarHolder.Content, img, img_Fx, img_Fy, img_Mz,
                                    img_Rx, img_Ry, textblock_Fx, textblock_Fy, textblock_Mz);


        public void AddToCanvas()
        {

            Helper.AddToCanvas(VarHolder.Content, img, img_Fx, img_Fy, img_Mz,
                               img_Rx, img_Ry, textblock_Fx, textblock_Fy, textblock_Mz);
            ApplyForces();
            ApplySupports();

            ToggleSelect(false);
        }


        public void ToggleSelect(bool IsSelected)
        {
            Color color;
            if (IsSelected)
            {
                color = Colors.Red;
            }
            else
            {
                color = Colors.Black;
            }

            WriteableBitmap bmp = (WriteableBitmap)img.Source;
            bmp.Clear();

            if (Parent.IsHinged)
            {
                bmp.FillEllipse(1, 1, 10, 10, Colors.White);
                bmp.DrawEllipse(0, 0, 11, 11, color);
            }
            else
            {
                bmp.FillRectangle(3, 3, 9, 9, color);
            }
        }


        public void ApplyForces()
        {
            string format = Units.Formats["Force"];

            double fx = Parent.Forces[VarHolder.CurrentLoadcase]["Fx"];
            double fy = Parent.Forces[VarHolder.CurrentLoadcase]["Fy"];
            double mz = Parent.Forces[VarHolder.CurrentLoadcase]["Mz"];
            double theta = Parent.Forces[VarHolder.CurrentLoadcase]["Angle"];


            if (!fx.CloseEnough(0))
            {
                int h = (int)Helper.LinInterp(VarHolder.MinForce, 25, VarHolder.MaxForce, 50, Math.Abs(fx));

                Text_Fx = Units.ConvertForce(Math.Abs(fx), Units.Force, true).ToString(format) + " " + Units.Force;
                double angle = fx > 0 ? theta : theta + 180;

                WriteableBitmap bmp = BitmapFactory.New(h, 8);
                bmp.DrawLineAa(0, 4, h - 6, 4, Colors.Red, 1);
                bmp.FillTriangle(h - 6, 0, h - 6, 8, h, 4, Colors.Red);

                img_Fx.Source = bmp;

                RotateTransform r = (RotateTransform)img_Fx.RenderTransform;
                r.Angle = angle;
                r.CenterX = -6;
                r.CenterY = 4;

                ScaleTransform stext = (ScaleTransform)((TransformGroup)textblock_Fx.RenderTransform).Children[0];
                stext.ScaleX = 1;
                stext.ScaleY = -1;

                RotateTransform rtext = (RotateTransform)((TransformGroup)textblock_Fx.RenderTransform).Children[1];
                rtext.Angle = angle;
                rtext.CenterY = -10;
            }
            else
            {
                img_Fx.Source = null;
                Text_Fx = "";
            }


            if (!fy.CloseEnough(0))
            {
                int h = (int)Helper.LinInterp(VarHolder.MinForce, 25, VarHolder.MaxForce, 50, Math.Abs(fy));
                Text_Fy = Units.ConvertForce(Math.Abs(fy), Units.Force, true).ToString(format) + " " + Units.Force;
                double angle = fy < 0 ? theta : theta - 180;

                WriteableBitmap bmp = BitmapFactory.New(8, h);
                bmp.DrawLineAa(4, 0, 4, h - 6, Colors.Red, 1);
                bmp.FillTriangle(0, 6, 8, 6, 4, 0, Colors.Red);

                img_Fy.Source = bmp;

                RotateTransform r = (RotateTransform)img_Fy.RenderTransform;
                r.Angle = angle;
                r.CenterX = 4;
                r.CenterY = h + 2;

                ScaleTransform stext = (ScaleTransform)((TransformGroup)textblock_Fy.RenderTransform).Children[0];
                stext.ScaleX = 1;
                stext.ScaleY = -1;

                RotateTransform rtext = (RotateTransform)((TransformGroup)textblock_Fy.RenderTransform).Children[1];
                rtext.Angle = angle;
                rtext.CenterX = -10;

            }

            else
            {
                img_Fy.Source = null;
                Text_Fy = "";
            }


            if (!mz.CloseEnough(0))
            {
                WriteableBitmap bmp = BitmapFactory.FromResource("Pictures\\Icon_Moment.png");
                Text_Mz = Units.ConvertMoment(Math.Abs(mz), Units.Moment, true).ToString(Units.Formats["Moment"]) + " " + Units.Moment;
                img_Mz.Source = bmp;

                ScaleTransform s = (ScaleTransform)img_Mz.RenderTransform;
                s.CenterX = 20.5;
                s.CenterY = 20.5;
                s.ScaleX = -1;
                s.ScaleY = Math.Sign(mz);

                ScaleTransform stext = (ScaleTransform)textblock_Mz.RenderTransform;
                stext.ScaleX = 1;
                stext.ScaleY = -1;
            }
            else
            {
                img_Mz.Source = null;
                Text_Mz = "";
            }

            Reposition();
        }


        public void ApplySupports()
        {
            int restr = 0;
            if (Parent.Restr["Rx"]) restr += 1;
            if (Parent.Restr["Ry"]) restr += 2;
            if (Parent.Restr["Rz"]) restr += 4;

            img_Ry.Source = img_Rx.Source = img_Rz.Source = null;
            switch (restr)
            {
                case 1: // Rx only ("001")
                    img_Rx.Source = BitmapFactory.FromResource("Pictures\\Icon_Uy.png");
                    img_Rx.RenderTransform = new RotateTransform { Angle = Parent.R_angle - 90, CenterX = 15, CenterY = 30 };
                    break;
                case 2: // Ry only ("010")
                    img_Ry.Source = BitmapFactory.FromResource("Pictures\\Icon_Uy.png");
                    img_Ry.RenderTransform = new RotateTransform { Angle = Parent.R_angle, CenterX = 15, CenterY = 30 };
                    break;
                case 3: // Rx and Ry ("011")
                    img_Rx.Source = BitmapFactory.FromResource("Pictures\\Icon_UxUy.png");
                    img_Rx.RenderTransform = new RotateTransform { Angle = Parent.R_angle, CenterX = 15, CenterY = 30 };
                    break;
                case 4: // Rz only ("100")
                    img_Rz.Source = BitmapFactory.FromResource("Pictures\\Icon_Rz.png");
                    break;
                case 5: // Rx and Rz ("101")
                    img_Rx.Source = BitmapFactory.FromResource("Pictures\\Icon_UyRz.png");
                    img_Rx.RenderTransform = new RotateTransform { Angle = Parent.R_angle - 90, CenterX = 15, CenterY = 13 };
                    break;
                case 6: // Ry and Rz ("110")
                    img_Ry.Source = BitmapFactory.FromResource("Pictures\\Icon_UyRz.png");
                    img_Ry.RenderTransform = new RotateTransform { Angle = Parent.R_angle, CenterX = 15, CenterY = 13 };
                    break;
                case 7: // Rx, Ry and Rz ("111")
                    img_Rx.Source = BitmapFactory.FromResource("Pictures\\Icon_UxUyRz.png");
                    img_Rx.RenderTransform = new RotateTransform { Angle = Parent.R_angle, CenterX = 15, CenterY = 8 };
                    break;
            }


            if (!Parent.Springs["Kx"].CloseEnough(0))
            {
                img_Rx.Source = BitmapFactory.FromResource("Pictures\\Icon_Kx.png");
                img_Rx.RenderTransform = new RotateTransform { Angle = -90, CenterX = 15, CenterY = 30 };
            }

            if (!Parent.Springs["Ky"].CloseEnough(0))
            {
                img_Ry.Source = BitmapFactory.FromResource("Pictures\\Icon_Kx.png");
                img_Ry.RenderTransform = null;
            }

            if (!Parent.Springs["Kz"].CloseEnough(0))
            {
                img_Rz.Source = BitmapFactory.FromResource("Pictures\\Icon_Kz.png");
                img_Rz.RenderTransform = null;
            }

            Reposition();
        }


        public void NodeClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Parent.NodeClick();
        }


        public void DrawResults()
        {
            img_Fx.Source = img_Fy.Source = img_Mz.Source = null;
            text_Fx = text_Fy = text_Mz = "";
        }
    }


    public class MemberInterface : Observable
    {
        // Every MemberInterface instance should be associated with a parent Member instance
        private Member Parent;


        #region Canvas elements
        // The member's canvas representation (1x1 square, to be stretched as needed)
        public Image img = new Image
        {
            RenderTransform = new TransformGroup
            {
                Children = { new ScaleTransform(), new RotateTransform() }
            }
        };

        // The member load's canvas representation
        public Image img_loadx = new Image();
        public Image img_loady = new Image();

        // The member's end-hinges
        public Image img_hinge0 = new Image { RenderTransform = new RotateTransform() };
        public Image img_hinge1 = new Image { RenderTransform = new RotateTransform() };

        // The result-linking line
        public Image img_resline = new Image
        {
            RenderTransform = new TransformGroup
            {
                Children = { new ScaleTransform(), new RotateTransform() }
            }
        };



        // The member's result-display
        public Path resultpath = new Path
        {
            RenderTransform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(),
                    new RotateTransform()
                }
            }
        };

        // Event handler for mouse clicks
        private System.Windows.Input.MouseButtonEventHandler mouseclick;


        // Storage dicts for bitmap properties
        private Dictionary<string, int> lxp = new Dictionary<string, int> { { "H", 0 }, { "axis", 0 }, { "h0", 0 }, { "h1", 0 } };
        private Dictionary<string, int> lyp = new Dictionary<string, int> { { "H", 0 }, { "axis", 0 }, { "h0", 0 }, { "h1", 0 } };
        private Dictionary<string, double> lres = new Dictionary<string, double> { { "axis", 0 }, { "h0", 0 }, { "h1", 0 }, { "h2", 0 }, { "ux0", 0 }, { "ux1", 0 } };
        #endregion


        #region Textblocks
        private string text_qx0 = "";
        public string Text_Qx0 { get => text_qx0; set => ChangeProperty(ref text_qx0, value); }

        private string text_qx1 = "";
        public string Text_Qx1 { get => text_qx1; set => ChangeProperty(ref text_qx1, value); }

        private string text_qy0 = "";
        public string Text_Qy0 { get => text_qy0; set => ChangeProperty(ref text_qy0, value); }

        private string text_qy1 = "";
        public string Text_Qy1 { get => text_qy1; set => ChangeProperty(ref text_qy1, value); }

        private string text_r1 = "";
        public string Text_R1 { get => text_r1; set => ChangeProperty(ref text_r1, value); }

        private string text_r2 = "";
        public string Text_R2 { get => text_r2; set => ChangeProperty(ref text_r2, value); }

        private string text_r3 = "";
        public string Text_R3 { get => text_r3; set => ChangeProperty(ref text_r3, value); }



        public TextBlock textblock_Qx0 = new TextBlock
        {
            Foreground = Brushes.Blue,
            Background = Brushes.White,
            RenderTransform = new TransformGroup { Children = { new ScaleTransform(), new RotateTransform() } }
        };

        public TextBlock textblock_Qx1 = new TextBlock
        {
            Foreground = Brushes.Blue,
            Background = Brushes.White,
            RenderTransform = new TransformGroup { Children = { new ScaleTransform(), new RotateTransform() } }
        };

        public TextBlock textblock_Qy0 = new TextBlock
        {
            Foreground = Brushes.Blue,
            Background = Brushes.White,
            RenderTransform = new TransformGroup { Children = { new ScaleTransform(), new RotateTransform() } }
        };

        public TextBlock textblock_Qy1 = new TextBlock
        {
            Foreground = Brushes.Blue,
            Background = Brushes.White,
            RenderTransform = new TransformGroup { Children = { new ScaleTransform(), new RotateTransform() } }
        };

        public TextBlock textblock_R1 = new TextBlock
        {
            Background = Brushes.White,
            RenderTransform = new TransformGroup { Children = { new ScaleTransform(), new RotateTransform() } }
        };

        public TextBlock textblock_R2 = new TextBlock
        {
            Background = Brushes.White,
            RenderTransform = new TransformGroup { Children = { new ScaleTransform(), new RotateTransform() } }
        };



        public TextBlock textblock_R3 = new TextBlock
        {
            Background = Brushes.White,
            Foreground = Brushes.Red,
            RenderTransform = new TransformGroup { Children = { new ScaleTransform(), new RotateTransform() } }
        };
        #endregion

        public MemberInterface()
        {
            mouseclick = new System.Windows.Input.MouseButtonEventHandler(MemberClick);

            VarHolder.Content.AddToCanvas(img, img_loadx, img_loady, img_hinge0, img_hinge1, resultpath, textblock_Qx0,
                                          textblock_Qx1, textblock_Qy0, textblock_Qy1,
                                          textblock_R1, textblock_R2);

            Binding binding_Qx0 = new Binding { Source = this, Path = new PropertyPath("Text_Qx0"), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            Binding binding_Qx1 = new Binding { Source = this, Path = new PropertyPath("Text_Qx1"), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            Binding binding_Qy0 = new Binding { Source = this, Path = new PropertyPath("Text_Qy0"), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            Binding binding_Qy1 = new Binding { Source = this, Path = new PropertyPath("Text_Qy1"), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            Binding binding_R1 = new Binding { Source = this, Path = new PropertyPath("Text_R1"), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            Binding binding_R2 = new Binding { Source = this, Path = new PropertyPath("Text_R2"), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            Binding binding_R3 = new Binding { Source = this, Path = new PropertyPath("Text_R3"), UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };

            _ = textblock_Qx0.SetBinding(TextBlock.TextProperty, binding_Qx0);
            _ = textblock_Qx1.SetBinding(TextBlock.TextProperty, binding_Qx1);
            _ = textblock_Qy0.SetBinding(TextBlock.TextProperty, binding_Qy0);
            _ = textblock_Qy1.SetBinding(TextBlock.TextProperty, binding_Qy1);
            _ = textblock_R1.SetBinding(TextBlock.TextProperty, binding_R1);
            _ = textblock_R2.SetBinding(TextBlock.TextProperty, binding_R2);
            _ = textblock_R3.SetBinding(TextBlock.TextProperty, binding_R3);


            WriteableBitmap bmp = BitmapFactory.New(1, 6);
            bmp.FillRectangle(0, 2, 1, 4, Colors.SteelBlue);
            Canvas.SetZIndex(img, 8);
            Canvas.SetZIndex(img_hinge0, 10);
            Canvas.SetZIndex(img_hinge1, 10);
            img.Source = bmp;
            img.MouseLeftButtonUp += mouseclick;
        }

        public void SetParent(Member member)
        {
            Parent = member;
            Reposition();
        }

        public void ToggleSelect(bool IsSelected)
        {
            Color color;
            Color hinge_color;

            if (IsSelected)
            {
                color = Colors.Magenta;
                hinge_color = Colors.OrangeRed;
            }
            else
            {
                color = Colors.SteelBlue;
                hinge_color = Colors.DarkBlue;
            }
            
            ((WriteableBitmap)img.Source).FillRectangle(0, 2, 1, 4, color);


            if (Parent.Hinge_Start || Parent.Hinge_End)
            {
                WriteableBitmap bmp = BitmapFactory.New(10, 10);
                bmp.FillEllipse(1, 1, 8, 8, Colors.White);
                bmp.DrawEllipse(0, 0, 9, 9, hinge_color);


                img_hinge0.Source = (Parent.Hinge_Start) ? bmp : null;
                img_hinge1.Source = (Parent.Hinge_End) ? bmp : null;
            }
            else
                img_hinge0.Source = img_hinge1.Source = null;
        }


        public void AddToCanvas()
        {

            Helper.AddToCanvas(VarHolder.Content, img, img_loadx, img_loady, img_resline,
                                                       img_hinge0, img_hinge1, textblock_Qx0, textblock_Qx1,
                                                       textblock_Qy0, textblock_Qy1, textblock_R1, textblock_R2, textblock_R3);
            Reposition();
        }


        public void RemoveFromCanvas() => Helper.RemoveFromCanvas(VarHolder.Content, img, img_loadx, img_loady, img_resline,
                                                                  img_hinge0, img_hinge1, textblock_Qx0, textblock_Qx1, textblock_Qy0,
                                                                  textblock_Qy1, textblock_R1, textblock_R2, textblock_R3);

        public void MemberClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => Parent.MemberClick(sender, e);


        public void Reposition()
        {
            #region Basic stuff
            string loadcase = VarHolder.CurrentLoadcase;
            double loadtype = Parent.Loads[loadcase]["Type"];
            double qx0 = Parent.Loads[loadcase]["Qx0"];
            double qx1 = Parent.Loads[loadcase]["Qx1"];
            double qy0 = Parent.Loads[loadcase]["Qy0"];
            double qy1 = Parent.Loads[loadcase]["Qy1"];


            Point P1 = VarHolder.Transfmatrix.Transform(Parent.N1.Coords);
            Point P2 = VarHolder.Transfmatrix.Transform(Parent.N2.Coords);
            double L = (P2 - P1).Length;
            double l = (Parent.N2.Coords - Parent.N1.Coords).Length;

            int k_angle = (Parent.Angle <= 90 || Parent.Angle > 270 && Parent.Angle < 360) ? 1 : -1;
            #endregion


            #region Member positioning
            TransformGroup transf = (TransformGroup)img.RenderTransform;
            transf.AssignToChildren(L, 1, 0, 3, Parent.Angle, L / 2, 3);

            Canvas.SetLeft(img, ((P1.X + P2.X) / 2) - (L / 2));
            Canvas.SetTop(img, ((P1.Y + P2.Y) / 2) - 3);

            Canvas.SetLeft(img_hinge0, P1.X + 3);
            Canvas.SetTop(img_hinge0, P1.Y - 5);

            Canvas.SetLeft(img_hinge1, P1.X + L - 13);
            Canvas.SetTop(img_hinge1, P1.Y - 5);

            RotateTransform hinge0_transf = (RotateTransform)img_hinge0.RenderTransform;
            RotateTransform hinge1_transf = (RotateTransform)img_hinge1.RenderTransform;

            hinge0_transf.Angle = hinge1_transf.Angle = Parent.Angle;
            hinge0_transf.CenterY = hinge1_transf.CenterY = 5;
            hinge0_transf.CenterX = -3;
            hinge1_transf.CenterX = -L + 13;

            #endregion


            #region Result positioning
            if (VarHolder.ClickType.Length > 7 &&
                VarHolder.ClickType.Substring(0, 7) == "Results")
            {
                double f = L / l;

                resultpath.StrokeThickness = 1 / f;
                TransformGroup transform = (TransformGroup)resultpath.RenderTransform;

                Point q0 = new Point(qx0, qy0);
                Point q1 = new Point(qx1, qy1);

                q0 = loadtype == 0 ? q0 : q0.Rotate(new Point(), Parent.Angle);
                q1 = loadtype == 0 ? q1 : q1.Rotate(new Point(), Parent.Angle);

                double deriv0 = 0;
                double deriv1 = 0;



                switch (VarHolder.ClickType)
                {
                    case "ResultsDispl":
                        break;

                    case "ResultsBM":
                        deriv0 = Parent.Shear[loadcase][0];
                        deriv1 = Parent.Shear[loadcase].Last();
                        break;

                    case "ResultsSF":
                        deriv0 = q0.Y;
                        deriv1 = q1.Y;
                        break;

                    case "ResultsNF":
                        deriv0 = q0.X;
                        deriv1 = q1.X;
                        break;
                }

                Canvas.SetLeft(resultpath, P1.X);
                Canvas.SetTop(resultpath, P1.Y);

                transform.AssignToChildren(f, f, -lres["ux0"], 0, Parent.Angle, 0, 0);


                Size size_r1 = textblock_R1.MeasureString(Text_R1);
                Size size_r2 = textblock_R2.MeasureString(Text_R2);

                double yr1 = (lres["h0"] * k_angle > 0) ? size_r1.Height : 0;
                double yr2 = (lres["h1"] * k_angle > 0) ? size_r2.Height : 0;

                if (deriv0.CloseEnough(deriv1))
                {
                    if (deriv0.CloseEnough(0))
                    {
                        Point p = new Point(P1.X + (L / 2) - (size_r1.Width / 2), P1.Y + (f * lres["h0"]) + (Math.Sign(lres["h0"]) * (5 + yr1)));

                        TransformGroup t = (TransformGroup)textblock_R1.RenderTransform;
                        t.AssignToChildren(k_angle, -k_angle, size_r1.Width / 2, 0, Parent.Angle, -p.X + P1.X, -p.Y + P1.Y);

                        Canvas.SetLeft(textblock_R1, p.X);
                        Canvas.SetTop(textblock_R1, p.Y);
                    }
                    else
                    {
                        Point pa = new Point(P1.X, P1.Y + (f * lres["h0"]) + (Math.Sign(lres["h0"]) * (5 + yr1)));
                        Point pb = new Point(P1.X + L - size_r2.Width, P1.Y + (f * lres["h1"]) + (Math.Sign(lres["h1"]) * (5 + yr2)));

                        TransformGroup t1 = (TransformGroup)textblock_R1.RenderTransform;
                        TransformGroup t2 = (TransformGroup)textblock_R2.RenderTransform;

                        t1.AssignToChildren(k_angle, -k_angle, size_r1.Width / 2, 0, Parent.Angle, -pa.X + P1.X, -pa.Y + P1.Y);
                        t2.AssignToChildren(k_angle, -k_angle, size_r2.Width / 2, 0, Parent.Angle, -pb.X + P1.X, -pb.Y + P1.Y);

                        Canvas.SetLeft(textblock_R1, pa.X);
                        Canvas.SetTop(textblock_R1, pa.Y);

                        Canvas.SetLeft(textblock_R2, pb.X);
                        Canvas.SetTop(textblock_R2, pb.Y);
                    }
                }

                else
                {
                    Point pa = new Point(P1.X, P1.Y + (f * lres["h0"]) + (Math.Sign(lres["h0"]) * (5 + yr1)));
                    Point pb = new Point(P1.X + L - size_r2.Width, P1.Y + (f * lres["h1"]) + (Math.Sign(lres["h1"]) * (5 + yr2)));

                    TransformGroup t1 = (TransformGroup)textblock_R1.RenderTransform;
                    TransformGroup t2 = (TransformGroup)textblock_R2.RenderTransform;

                    t1.AssignToChildren(k_angle, -k_angle, size_r1.Width / 2, 0, Parent.Angle, -pa.X + P1.X, -pa.Y + P1.Y);
                    t2.AssignToChildren(k_angle, -k_angle, size_r2.Width / 2, 0, Parent.Angle, -pb.X + P1.X, -pb.Y + P1.Y);

                    Canvas.SetLeft(textblock_R1, pa.X);
                    Canvas.SetTop(textblock_R1, pa.Y);

                    Canvas.SetLeft(textblock_R2, pb.X);
                    Canvas.SetTop(textblock_R2, pb.Y);
                }
                return;
            }
            #endregion


            #region Load positioning
            resultpath.Data = null;
            ApplyLoads();

            Size size_x0 = textblock_Qx0.MeasureString(Text_Qx0);
            Size size_x1 = textblock_Qx1.MeasureString(Text_Qx1);
            Size size_y0 = textblock_Qy0.MeasureString(Text_Qy0);
            Size size_y1 = textblock_Qy1.MeasureString(Text_Qy1);


            // It's finally working -- for the love of God, do not touch it!!!
            if (loadtype == 0)
            {

                if (!qx0.CloseEnough(0) || !qx1.CloseEnough(0))
                {
                    double x0 = (qx0 * k_angle < 0) ? size_x0.Height : 0;
                    double x1 = (qx1 * k_angle < 0) ? size_x1.Height : 0;

                    if (qx0.CloseEnough(qx1))
                    {
                        Point p = new Point(P1.X + (L / 2) - (size_x0.Width / 2), P1.Y - (5 + x0));

                        Canvas.SetLeft(textblock_Qx0, p.X);
                        Canvas.SetTop(textblock_Qx0, p.Y);

                        TransformGroup t = (TransformGroup)textblock_Qx0.RenderTransform;
                        t.AssignToChildren(k_angle, -k_angle, size_x0.Width / 2, 0, Parent.Angle, -p.X + P1.X, -p.Y + P1.Y); ;
                    }

                    else
                    {
                        Point pa = new Point(P1.X, P1.Y - (5 + x0));
                        Point pb = new Point(P1.X + L - size_x0.Width, P1.Y - (5 + x1));

                        Canvas.SetLeft(textblock_Qx0, pa.X);
                        Canvas.SetTop(textblock_Qx0, pa.Y);

                        Canvas.SetLeft(textblock_Qx1, pb.X);
                        Canvas.SetTop(textblock_Qx1, pb.Y);

                        TransformGroup t1 = (TransformGroup)textblock_Qx0.RenderTransform;
                        TransformGroup t2 = (TransformGroup)textblock_Qx1.RenderTransform;

                        t1.AssignToChildren(k_angle, -k_angle, size_x0.Width / 2, 0, Parent.Angle, -pa.X + P1.X, -pa.Y + P1.Y);
                        t2.AssignToChildren(k_angle, -k_angle, size_x1.Width / 2, 0, Parent.Angle, -pb.X + P1.X, -pb.Y + P1.Y);
                    }

                }


                if (!qy0.CloseEnough(0) || !qy1.CloseEnough(0))
                {
                    double y0 = (qy0 * k_angle < 0) ? size_y0.Height : 0;
                    double y1 = (qy1 * k_angle < 0) ? size_y1.Height : 0;

                    if (qy0.CloseEnough(qy1))
                    {
                        Point p = new Point(P1.X + (L / 2) - (size_y0.Width / 2), P1.Y - lyp["h0"] - (Math.Sign(lyp["h0"]) * (5 + y0)));

                        TransformGroup t = (TransformGroup)textblock_Qy0.RenderTransform;
                        t.AssignToChildren(k_angle, -k_angle, size_y0.Width / 2, 0, Parent.Angle, -p.X + P1.X, -p.Y + P1.Y);

                        Canvas.SetLeft(textblock_Qy0, p.X);
                        Canvas.SetTop(textblock_Qy0, p.Y);
                    }

                    else
                    {
                        Point pa = new Point(P1.X, P1.Y - lyp["h0"] - (Math.Sign(lyp["h0"]) * (5 + y0)));
                        Point pb = new Point(P1.X + L - size_y1.Width, P1.Y - lyp["h1"] - (Math.Sign(lyp["h1"]) * (5 + y1)));

                        TransformGroup t1 = (TransformGroup)textblock_Qy0.RenderTransform;
                        TransformGroup t2 = (TransformGroup)textblock_Qy1.RenderTransform;

                        t1.AssignToChildren(k_angle, -k_angle, size_y0.Width / 2, 0, Parent.Angle, -pa.X + P1.X, -pa.Y + P1.Y);
                        t2.AssignToChildren(k_angle, -k_angle, size_y1.Width / 2, 0, Parent.Angle, -pb.X + P1.X, -pb.Y + P1.Y);

                        Canvas.SetLeft(textblock_Qy0, pa.X);
                        Canvas.SetTop(textblock_Qy0, pa.Y);

                        Canvas.SetLeft(textblock_Qy1, pb.X);
                        Canvas.SetTop(textblock_Qy1, pb.Y);

                    }

                }
            }


            else
            {

                if (!qx0.CloseEnough(0) || !qx1.CloseEnough(0))
                {

                    if (qx0.CloseEnough(qx1))
                    {
                        Canvas.SetLeft(textblock_Qx0, Math.Min(P1.X, P2.X) - lxp["H"] - size_x0.Height - 4);
                        Canvas.SetTop(textblock_Qx0, ((P1.Y + P2.Y) / 2) - (size_x0.Width / 2));

                        if (qx0 * qx1 > 0 && qx0 < 0)
                        {
                            Canvas.SetLeft(textblock_Qx0, Math.Max(P1.X, P2.X) + lxp["H"] + 5);
                        }

                        TransformGroup t = (TransformGroup)textblock_Qx0.RenderTransform;
                        t.AssignToChildren(1, -1, 0, 0, 90, 0, 0);
                    }

                    else
                    {
                        double x1 = 5 + lxp["H"] - lxp["axis"] + (Math.Max(Math.Sign(lxp["h0"]), 0) * size_x0.Width);
                        double x2 = 5 + lxp["H"] - lxp["axis"] + (Math.Max(Math.Sign(lxp["h1"]), 0) * size_x1.Width);
                        double y1 = (P1.Y < P2.Y) ? -5 : 5 + size_x0.Height;
                        double y2 = (P1.Y < P2.Y) ? 5 + size_x1.Height : -5;

                        if (qx0 * qx1 > 0 && qx0 < 0)
                        {
                            Canvas.SetLeft(textblock_Qx0, Math.Max(P1.X, P2.X) + 5 - lxp["h0"] - size_x0.Width);
                            Canvas.SetLeft(textblock_Qx1, Math.Max(P1.X, P2.X) + 5 - lxp["h1"] - size_x1.Width);
                        }

                        else
                        {
                            Canvas.SetLeft(textblock_Qx0, Math.Min(P1.X, P2.X) - x1);
                            Canvas.SetLeft(textblock_Qx1, Math.Min(P1.X, P2.X) - x2);
                        }

                        Canvas.SetTop(textblock_Qx0, P1.Y + y1);
                        Canvas.SetTop(textblock_Qx1, P2.Y + y2);

                        TransformGroup t1 = (TransformGroup)textblock_Qx0.RenderTransform;
                        TransformGroup t2 = (TransformGroup)textblock_Qx1.RenderTransform;

                        t1.AssignToChildren(1, -1, 0, 0, Parent.Angle, 0, 0);
                        t2.AssignToChildren(1, -1, 0, 0, Parent.Angle, 0, 0);

                    }
                }


                if (!qy0.CloseEnough(0) || !qy1.CloseEnough(0))
                {
                    if (qy0.CloseEnough(qy1))
                    {
                        Canvas.SetLeft(textblock_Qy0, ((P1.X + P2.X) / 2) - (size_y0.Width / 2));
                        Canvas.SetTop(textblock_Qy0, Math.Min(P1.Y, P2.Y) - lyp["H"] - 5);

                        if (qy0 * qy1 > 0 && qy0 < 0)
                        {
                            Canvas.SetTop(textblock_Qy0, Math.Max(P1.Y, P2.Y) + lyp["H"] + size_y0.Height + 5);
                        }

                        TransformGroup t = (TransformGroup)textblock_Qy0.RenderTransform;
                        t.AssignToChildren(1, -1, size_y0.Width / 2, 0, 0, 0, 0);

                    }

                    else
                    {
                        if (qy0 * qy1 > 0 && qy0 > 0)
                        {
                            Canvas.SetTop(textblock_Qy0, Math.Min(P1.Y, P2.Y) - lyp["h0"] - 5);
                            Canvas.SetTop(textblock_Qy1, Math.Min(P1.Y, P2.Y) - lyp["h1"] - 5);
                        }

                        else
                        {
                            Canvas.SetTop(textblock_Qy0, Math.Max(P1.Y, P2.Y) + lyp["axis"] - lyp["h0"] + ((lyp["h0"] > 0) ? -5 : 5 + size_y0.Height));
                            Canvas.SetTop(textblock_Qy1, Math.Max(P1.Y, P2.Y) + lyp["axis"] - lyp["h1"] + ((lyp["h1"] > 0) ? -5 : 5 + size_y1.Height));
                        }

                        Canvas.SetLeft(textblock_Qy0, P1.X - (size_y0.Width / 2));
                        Canvas.SetLeft(textblock_Qy1, P2.X - (size_y1.Width / 2));

                        TransformGroup t1 = (TransformGroup)textblock_Qy0.RenderTransform;
                        TransformGroup t2 = (TransformGroup)textblock_Qy1.RenderTransform;

                        t1.AssignToChildren(1, -1, 0, 0, 0, 0, 0);
                        t2.AssignToChildren(1, -1, 0, 0, 0, 0, 0);
                    }

                }
            }
            #endregion
        }

        #region Load-drawing methods
        public void ApplyLoads()
        {
            string loadcase = VarHolder.CurrentLoadcase;
            double qx0 = Parent.Loads[loadcase]["Qx0"];
            double qx1 = Parent.Loads[loadcase]["Qx1"];
            double qy0 = Parent.Loads[loadcase]["Qy0"];
            double qy1 = Parent.Loads[loadcase]["Qy1"];
            double type = Parent.Loads[loadcase]["Type"];


            if (type == 0)
            {
                if (!qx0.CloseEnough(0) || !qx1.CloseEnough(0))
                {
                    DrawLoads("XL");
                }
                else
                {
                    ClearLoads("X");
                }

                if (!qy0.CloseEnough(0) || !qy1.CloseEnough(0))
                {
                    DrawLoads("YL");
                }
                else
                {
                    ClearLoads("Y");
                }
            }

            else
            {
                if (!qx0.CloseEnough(0) || !qx1.CloseEnough(0))
                {
                    DrawLoads("XG");
                }
                else
                {
                    ClearLoads("X");
                }

                if (!qy0.CloseEnough(0) || !qy1.CloseEnough(0))
                {
                    DrawLoads("YG");
                }
                else
                {
                    ClearLoads("Y");
                }
            }


        }


        private void DrawLoads(string type)
        {
            string loadcase = VarHolder.CurrentLoadcase;

            double qx0 = Parent.Loads[loadcase]["Qx0"];
            double qx1 = Parent.Loads[loadcase]["Qx1"];
            double qy0 = Parent.Loads[loadcase]["Qy0"];
            double qy1 = Parent.Loads[loadcase]["Qy1"];
            string format = Units.Formats["Load"];

            string string_qx0 = Units.ConvertLoad(Math.Abs(qx0), Units.Load, true).ToString(format);
            string string_qx1 = Units.ConvertLoad(Math.Abs(qx1), Units.Load, true).ToString(format);
            string string_qy0 = Units.ConvertLoad(Math.Abs(qy0), Units.Load, true).ToString(format);
            string string_qy1 = Units.ConvertLoad(Math.Abs(qy1), Units.Load, true).ToString(format);

            switch (type)
            {
                case "XL":
                    LocalX(ref img_loadx, qx0, qx1);

                    Text_Qx0 = $"{string_qx0} {Units.Load}";
                    Text_Qx1 = qx1.CloseEnough(qx0) ? "" : $"{string_qx1} {Units.Load}";
                    break;

                case "YL":
                    LocalY(ref img_loady, qy0, qy1);

                    Text_Qy0 = qy0.CloseEnough(0) ? "" : $"{string_qy0} {Units.Load}";
                    Text_Qy1 = (qy1.CloseEnough(0) || qy1.CloseEnough(qy0)) ? "" : $"{string_qy1} {Units.Load}";
                    break;

                case "XG":
                    if (Parent.Angle == 0 || Parent.Angle == 180)
                    {
                        LocalX(ref img_loadx, qx0, qx1);
                    }
                    else if (Parent.Angle == 90 || Parent.Angle == 270)
                    {
                        LocalY(ref img_loadx, qy0, qy1);
                    }
                    else
                    {
                        GlobalX(ref img_loadx, qx0, qx1);
                    }

                    Text_Qx0 = qx0.CloseEnough(0) ? "" : $"{string_qx0} {Units.Load}";
                    Text_Qx1 = (qx1.CloseEnough(0) || qx1.CloseEnough(qx0)) ? "" : $"{string_qx1} {Units.Load}";
                    break;

                case "YG":
                    if (Parent.Angle == 90 || Parent.Angle == 270)
                    {
                        LocalX(ref img_loady, qy0, qy1);
                    }
                    else if (Parent.Angle == 0 || Parent.Angle == 180)
                    {
                        LocalY(ref img_loady, qy0, qy1);
                    }
                    else
                    {
                        GlobalY(ref img_loady, qy0, qy1);
                    }

                    Text_Qy0 = qy0.CloseEnough(0) ? "" : $"{string_qy0} {Units.Load}";
                    Text_Qy1 = (qy1.CloseEnough(0) || qy1.CloseEnough(qy0)) ? "" : $"{string_qy1} {Units.Load}";
                    break;

                default:
                    throw new ArgumentException("Invalid load-type passed as argument.");
            }
            return;
        }


        private void ClearLoads(string type)
        {
            switch (type)
            {
                case "X":
                    img_loadx.MouseLeftButtonUp -= mouseclick;
                    img_loadx.Source = null;
                    Text_Qx0 = Text_Qx1 = "";
                    Canvas.SetZIndex(img_loadx, 0);
                    break;
                case "Y":
                    img_loady.Source = null;
                    Text_Qy0 = Text_Qy1 = "";
                    break;
                default:
                    throw new ArgumentException("Invalid load-type passed as argument.");

            }

        }


        private void LocalX(ref Image image, double q0, double q1)
        {
            // Basic Canvas transform stuff
            Point P1 = VarHolder.Transfmatrix.Transform(Parent.N1.Coords);
            Point P2 = VarHolder.Transfmatrix.Transform(Parent.N2.Coords);
            int L = (int)(P2 - P1).Length;

            lxp["H"] = lxp["axis"] = lxp["h0"] = lxp["h1"] = 0;

            // Creating the bitmap and populating it with arrows
            WriteableBitmap bmp = BitmapFactory.New((int)L, 8);
            for (int x = 0; x < (int)L; x += 20)
            {
                double h = q0 + ((q1 - q0) * x / L);
                bmp.FillTriangle(x, 0, x, 8, x + (Math.Sign(h) * 6), 4, Colors.Blue);
            }

            // Setting up the Image control
            image.Source = bmp;
            image.RenderTransform = new RotateTransform { Angle = Parent.Angle, CenterX = bmp.Width / 2, CenterY = bmp.Height / 2 };
            image.MouseLeftButtonUp += mouseclick;
            bmp.Freeze();

            // Positioning the Image in the Canvas
            Canvas.SetZIndex(image, 9);
            Canvas.SetLeft(image, ((P1.X + P2.X) / 2) - (bmp.Width / 2));
            Canvas.SetTop(image, ((P1.Y + P2.Y) / 2) - (bmp.Height / 2));
        }


        private void LocalY(ref Image image, double q0, double q1)
        {
            // Basic Canvas transform stuff
            Point P1 = VarHolder.Transfmatrix.Transform(Parent.N1.Coords);
            Point P2 = VarHolder.Transfmatrix.Transform(Parent.N2.Coords);
            int L = (int)(P2 - P1).Length;


            // Interpolate between the max and min current loads in order to find the adequate arrow length
            double maxL = VarHolder.MaxLoad;
            double minL = VarHolder.MinLoad;
            lyp["h0"] = q0.CloseEnough(0) ? 0 : (int)Helper.LinInterp(minL, 30, maxL, 50, Math.Abs(q0)) * Math.Sign(q0);
            lyp["h1"] = q1.CloseEnough(0) ? 0 : (int)Helper.LinInterp(minL, 30, maxL, 50, Math.Abs(q1)) * Math.Sign(q1);

            // Calculate the bitmap height and the y-coordinate of the member's axis (in bitmap coordinates)
            lyp["H"] = lyp["h0"] * lyp["h1"] < 0 ? Math.Abs(lyp["h0"] - lyp["h1"]) : Math.Max(Math.Abs(lyp["h0"]), Math.Abs(lyp["h1"]));
            lyp["axis"] = lyp["h0"] * lyp["h1"] < 0 ? Math.Max(lyp["h0"], lyp["h1"]) : Math.Max(0, Math.Max(lyp["h0"], lyp["h1"]));


            // Create the bmp and draw the line between start and end loads
            WriteableBitmap bmp = BitmapFactory.New((int)L + 8, lyp["H"]);
            bmp.DrawLineAa(4, lyp["axis"] - lyp["h0"], (int)L + 4, lyp["axis"] - lyp["h1"], Colors.Blue);


            // Draw each arrow
            int n = Math.Max((int)(L / 20), 3);
            for (int i = 0; i <= n; i++)
            {
                int x = 4 + (int)(i * L / n);       // Find the current x-coordinate
                int h = (int)(lyp["h0"] + ((lyp["h1"] - lyp["h0"]) * x / L));      // Find the current arrow height
                int s = Math.Sign(h);
                int s2 = (h < 0) ? 0 : 1;   // For some reason the positive arrows keep getting hidden behind the member while the 
                                            // negative ones don't, so I used this dummy variable in order to slightly shift them up a bit (awful solution)

                // If the height is enough, draw the arrow -- no idea why this overflow exception catch is here
                try
                {
                    if (Math.Abs(h) > 10)
                    {
                        bmp.FillTriangle(x, lyp["axis"] - (4 * s * s2),
                                         x + 4, lyp["axis"] - (4 * s * (1 + s2)), x - 4, lyp["axis"] - (4 * s * (1 + s2)), Colors.Blue);
                        bmp.DrawLineAa(x, lyp["axis"] - (4 * s * s2), x, lyp["axis"] - h, Colors.Blue);
                    }
                }
                catch (System.OverflowException) { return; }

            }

            // Setting up the Image control
            RotateTransform rotate = new RotateTransform { Angle = Parent.Angle, CenterX = bmp.Width / 2, CenterY = lyp["axis"] };
            ScaleTransform scaleload = new ScaleTransform { CenterX = bmp.Width / 2, CenterY = lyp["axis"], ScaleY = 1 };

            image.Source = bmp;
            image.RenderTransform = new TransformGroup { Children = { scaleload, rotate } };

            // Positioning the Image control
            Canvas.SetLeft(image, ((P1.X + P2.X) / 2) - (bmp.Width / 2));
            Canvas.SetTop(image, ((P1.Y + P2.Y) / 2) - lyp["axis"]);
        }


        private void GlobalX(ref Image image, double q0, double q1)
        {
            // Basic Canvas transform stuff
            Point P1 = VarHolder.Transfmatrix.Transform(Parent.N1.Coords);
            Point P2 = VarHolder.Transfmatrix.Transform(Parent.N2.Coords);
            int L = (int)(P2 - P1).Length;


            // Interpolating the arrow lengths
            double maxL = VarHolder.MaxLoad;
            double minL = VarHolder.MinLoad;
            lxp["hx0"] = q0.CloseEnough(0) ? 0 : (int)Helper.LinInterp(minL, 30, maxL, 50, Math.Abs(q0)) * Math.Sign(q0);
            lxp["hx1"] = q1.CloseEnough(0) ? 0 : (int)Helper.LinInterp(minL, 30, maxL, 50, Math.Abs(q1)) * Math.Sign(q1);

            // Calculating the bitmap's dimensions and axis position
            double Ld = L * Math.Abs(Math.Sin(Parent.Angle * Math.PI / 180));

            lxp["H"] = lxp["hx0"] * lxp["hx1"] < 0 ? Math.Abs(lxp["hx0"] - lxp["hx1"]) : Math.Max(Math.Abs(lxp["hx0"]), Math.Abs(lxp["hx1"]));
            lxp["axis"] = lxp["hx0"] * lxp["hx1"] < 0 ? Math.Max(lxp["hx0"], lxp["hx1"]) : Math.Max(0, Math.Max(lxp["hx0"], lxp["hx1"]));


            // Creating the bmp and drawing the line between the start and end loads
            WriteableBitmap bmp = BitmapFactory.New(lxp["H"], (int)Ld + 8);

            bmp.DrawLineAa(lxp["axis"], 4, lxp["axis"], (int)Ld + 4, Colors.Gray);
            bmp.DrawLineAa(lxp["axis"] - lxp["hx0"], 4, lxp["axis"] - lxp["hx1"], (int)Ld + 4, Colors.Blue);

            // Drawing each arrow
            int n = Math.Max((int)(Ld / 20), 3);
            for (int i = 0; i <= n; i++)
            {
                double x = 4 + (i * Ld / n);
                int h = (int)(lxp["hx0"] + ((lxp["hx1"] - lxp["hx0"]) * (x - 4) / Ld));
                int s = Math.Sign(h);

                if (Math.Abs(h) > 10)
                {
                    bmp.FillTriangle(lxp["axis"], (int)x, lxp["axis"] - (4 * s), (int)x + (4 * s), lxp["axis"] - (4 * s), (int)x - (4 * s), Colors.Blue);
                    bmp.DrawLineAa(lxp["axis"], (int)x, lxp["axis"] - h, (int)x, Colors.Blue);
                }
            }

            // Setting up the Image control
            image.MouseLeftButtonUp -= mouseclick;
            image.Source = bmp;
            image.RenderTransform = null;
            bmp.Freeze();

            // Positioning the Image control
            Canvas.SetLeft(image, Math.Min(P1.X, P2.X) - lxp["H"] - 5);
            Canvas.SetTop(image, Math.Min(P1.Y, P2.Y));

            if (q0 * q1 > 0 && q0 < 0)
            {
                Canvas.SetLeft(image, Math.Max(P1.X, P2.X) + 5);
            }
        }


        private void GlobalY(ref Image image, double q0, double q1)
        {
            // Basic Canvas transform stuff
            Point P1 = VarHolder.Transfmatrix.Transform(Parent.N1.Coords);
            Point P2 = VarHolder.Transfmatrix.Transform(Parent.N2.Coords);
            int L = (int)(P2 - P1).Length;


            // Interpolating the arrow lengths
            double maxL = VarHolder.MaxLoad;
            double minL = VarHolder.MinLoad;
            lyp["h0"] = q0.CloseEnough(0) ? 0 : (int)Helper.LinInterp(minL, 30, maxL, 50, Math.Abs(q0)) * Math.Sign(q0);
            lyp["h1"] = q1.CloseEnough(0) ? 0 : (int)Helper.LinInterp(minL, 30, maxL, 50, Math.Abs(q1)) * Math.Sign(q1);

            // Calculating the bitmap's dimensions and axis position
            double Ld = L * Math.Abs(Math.Cos(Parent.Angle * Math.PI / 180));

            lyp["H"] = lyp["h0"] * lyp["h1"] < 0 ? Math.Abs(lyp["h0"] - lyp["h1"]) : Math.Max(Math.Abs(lyp["h0"]), Math.Abs(lyp["h1"]));
            lyp["axis"] = lyp["h0"] * lyp["h1"] < 0 ? Math.Max(lyp["h0"], lyp["h1"]) : Math.Max(0, Math.Max(lyp["h0"], lyp["h1"]));


            // Creating the bmp and drawing the line between the start and end loads
            WriteableBitmap bmp = BitmapFactory.New((int)Ld + 8, (int)lyp["H"]);

            bmp.DrawLineAa(4, lyp["axis"], (int)Ld + 4, lyp["axis"], Colors.Gray);
            bmp.DrawLineAa(4, lyp["axis"] - lyp["h0"], (int)Ld + 4, lyp["axis"] - lyp["h1"], Colors.Blue);

            // Drawing each arrow
            int n = Math.Max((int)(Ld / 20), 3);
            for (int i = 0; i <= n; i++)
            {
                double x = 4 + (i * Ld / n);
                int h = (int)(lyp["h0"] + ((lyp["h1"] - lyp["h0"]) * (x - 4) / Ld));
                int s = Math.Sign(h);

                if (Math.Abs(h) > 10)
                {
                    bmp.FillTriangle((int)x, lyp["axis"], (int)x + (4 * s), lyp["axis"] - (4 * s), (int)x - (4 * s), lyp["axis"] - (4 * s), Colors.Blue);
                    bmp.DrawLineAa((int)x, lyp["axis"], (int)x, lyp["axis"] - h, Colors.Blue);
                }
            }

            // Setting up the Image control
            image.MouseLeftButtonUp -= mouseclick;
            image.Source = bmp;
            image.RenderTransform = null;
            bmp.Freeze();

            // Positioning the Image control
            Canvas.SetLeft(image, Math.Min(P1.X, P2.X));
            Canvas.SetTop(image, Math.Max(P1.Y, P2.Y) + 5);

            if (q0 * q1 > 0 && q0 > 0)
            {
                Canvas.SetTop(image, Math.Min(P1.Y, P2.Y) - 5 - lyp["H"]);
            }
        }
        #endregion


        #region Result-drawing methods
        public void ShowResults()
        {
            string loadcase = VarHolder.CurrentLoadcase;
            double[] defarray = new double[1];
            double T1 = 0;
            double T2 = 0;
            string format = "";
            Brush textcolor = Brushes.Magenta;

            switch (VarHolder.ClickType)
            {
                case "ResultsDispl":
                    DrawResults(Parent.Uy[loadcase], Parent.Ux[loadcase], Brushes.Magenta, false, VarHolder.ResultScale, VarHolder.ResultScale);
                    break;
                case "ResultsBM":
                    DrawResults(Parent.Moment[loadcase], defarray, Brushes.DarkGreen, true, -0.01 * VarHolder.ResultScale);
                    T1 = Math.Abs(Units.ConvertMoment(Parent.Moment[loadcase][0], Units.Moment, true));
                    T2 = Math.Abs(Units.ConvertMoment(Parent.Moment[loadcase].Last(), Units.Moment, true));
                    format = "Moment";
                    textcolor = Brushes.DarkGreen;
                    break;
                case "ResultsSF":
                    DrawResults(Parent.Shear[loadcase], defarray, Brushes.DarkRed, true, VarHolder.ResultScale);
                    T1 = Units.ConvertForce(Parent.Shear[loadcase][0], Units.Force, true);
                    T2 = Units.ConvertForce(Parent.Shear[loadcase].Last(), Units.Force, true);
                    format = "Force";
                    textcolor = Brushes.DarkRed;
                    break;
                case "ResultsNF":
                    DrawResults(Parent.Axial[loadcase], defarray, Brushes.DarkBlue, true, VarHolder.ResultScale);
                    T1 = Units.ConvertForce(Parent.Axial[loadcase][0], Units.Force, true);
                    T2 = Units.ConvertForce(Parent.Axial[loadcase].Last(), Units.Force, true);
                    format = "Force";
                    textcolor = Brushes.DarkBlue;
                    break;
            }

            Text_R1 = T1.CloseEnough(0) ? "" : T1.ToString(Units.Formats[format]);
            Text_R2 = T2.CloseEnough(0) ? "" : T2.ToString(Units.Formats[format]);
            textblock_R1.Foreground = textblock_R2.Foreground = textcolor;


            img_loadx.Source = img_loady.Source = null;
            Text_Qx0 = Text_Qx1 = Text_Qy0 = Text_Qy1 = "";

            Reposition();

        }


        private void DrawResults(double[] YValues, double[] XValues, Brush ResultColor, bool ClosedAtEnds, double fy, double fx = 1)
        {
            // Basic stuff
            double L = (Parent.N2.Coords - Parent.N1.Coords).Length;
            int n = YValues.Length;
            XValues = (XValues.Length == n) ? XValues : new double[n];

            lres["ux0"] = XValues[0];
            lres["ux1"] = XValues.Last();

            StreamGeometry stg = new StreamGeometry();
            lres["xmax"] = -1;

            lres["h0"] = fy * YValues[0];
            lres["h1"] = fy * YValues.Last();

            using (StreamGeometryContext ctx = stg.Open())
            {
                double y = ClosedAtEnds ? 0 : YValues[0];

                Point P = new Point(fx * XValues[0], fy * y);
                ctx.BeginFigure(P, true, false);

                for (int i = ClosedAtEnds ? 0 : 1; i < n; i++)
                {
                    P.X = (L * i / (n - 1)) + (fx * XValues[i]);
                    P.Y = lres["axis"] + (fy * YValues[i]);
                    ctx.LineTo(P, true, false);
                }
                if (ClosedAtEnds)
                {
                    P.Y = lres["axis"];
                    ctx.LineTo(P, true, false);
                }
            }

            resultpath.Data = stg;
            resultpath.Stroke = ResultColor;
        }
        #endregion
    }


    public class NodeState
    {
        public Node SavedNode;

        public bool SavedHinge;
        public double SavedAngle;

        public Dictionary<string, Dictionary<string, double>> SavedForces = new Dictionary<string, Dictionary<string, double>>();
        public Dictionary<string, double> SavedSprings;
        public Dictionary<string, bool> SavedRestr;
        public Dictionary<string, double> SavedPdispl;


        public NodeState(Node node)
        {
            SavedNode = node;
            SavedHinge = node.IsHinged;
            SavedAngle = node.R_angle;

            SavedSprings = new Dictionary<string, double> { { "Kx", node.Springs["Kx"] }, { "Ky", node.Springs["Ky"] }, { "Kz", node.Springs["Kz"] } };
            SavedRestr = new Dictionary<string, bool> { { "Rx", node.Restr["Rx"] }, { "Ry", node.Restr["Ry"] }, { "Rz", node.Restr["Rz"] } };
            SavedPdispl = new Dictionary<string, double> { { "Ux", node.Pdispl["Ux"] }, { "Uy", node.Pdispl["Uy"] }, { "Rz", node.Pdispl["Rz"] } };


            foreach (KeyValuePair<string, Dictionary<string, double>> kvp in node.Forces)
            {
                SavedForces[kvp.Key] = new Dictionary<string, double> { { "Fx", node.Forces[kvp.Key]["Fx"] }, { "Fy", node.Forces[kvp.Key]["Fy"] },
                                                                        { "Mz", node.Forces[kvp.Key]["Mz"] },  { "Angle", node.Forces[kvp.Key]["Angle"] }};
            }
        }

        public void ApplyBack()
        {
            SavedNode.IsHinged = SavedHinge;
            SavedNode.R_angle = SavedAngle;

            foreach (KeyValuePair<string, double> kvp in SavedSprings)
            {
                SavedNode.Springs[kvp.Key] = SavedSprings[kvp.Key];
            }

            foreach (KeyValuePair<string, bool> kvp in SavedRestr)
            {
                SavedNode.Restr[kvp.Key] = SavedRestr[kvp.Key];
            }

            foreach (KeyValuePair<string, double> kvp in SavedPdispl)
            {
                SavedNode.Pdispl[kvp.Key] = SavedPdispl[kvp.Key];
            }

            SavedNode.Forces.Clear();

            foreach (KeyValuePair<string, Dictionary<string, double>> kvp in SavedForces)
            {
                SavedNode.Forces[kvp.Key] = new Dictionary<string, double> { { "Fx", SavedForces[kvp.Key]["Fx"] }, { "Fy", SavedForces[kvp.Key]["Fy"] },
                                                                        { "Mz", SavedForces[kvp.Key]["Mz"] }, { "Angle", SavedForces[kvp.Key]["Angle"] } };
            }

            VarHolder.NodesList.Add(SavedNode);

            SavedNode.AddToCanvas();
            SavedNode.ApplyForces();
            SavedNode.ApplySupports();
        }


    }


    public class MemberState
    {
        public Member SavedMember;
        public Node SavedN1;
        public Node SavedN2;

        public MemberMat SavedMaterial;
        public MemberSec SavedSection;

        public bool SavedStiffness;
        public bool SavedHinge0;
        public bool SavedHinge1;

        public Dictionary<string, Dictionary<string, double>> SavedLoads = new Dictionary<string, Dictionary<string, double>>();

        public Dictionary<string, double[]> SavedTemperature = new Dictionary<string, double[]>();


        public double SavedLength;
        public double SavedAngle;

        public MemberState(Member member)
        {
            SavedMember = member;
            SavedN1 = member.N1;
            SavedN2 = member.N2;

            SavedLength = member.Length;
            SavedAngle = member.Angle;
            SavedStiffness = member.IsRigid;

            SavedMaterial = member.Material;
            SavedSection = member.Section;

            SavedHinge0 = member.Hinge_Start;
            SavedHinge1 = member.Hinge_End;

            foreach (KeyValuePair<string, Dictionary<string, double>> kvp in member.Loads)
            {
                SavedLoads[kvp.Key] = new Dictionary<string, double> { { "Qx0", member.Loads[kvp.Key]["Qx0"] }, { "Qx1", member.Loads[kvp.Key]["Qx1"] },
                                                                       { "Qy0", member.Loads[kvp.Key]["Qy0"] },  { "Qy1", member.Loads[kvp.Key]["Qy1"] },
                                                                       { "Type", member.Loads[kvp.Key]["Type"] } };
            }
        }

        public void ApplyBack()
        {
            SavedMember.N1 = SavedN1;
            SavedMember.N2 = SavedN2;


            SavedMember.Length = SavedLength;
            SavedMember.Angle = SavedAngle;

            SavedMember.IsRigid = SavedStiffness;

            SavedMember.Material = SavedMaterial;
            SavedMember.Section = SavedSection;

            SavedMember.Hinge_Start = SavedHinge0;
            SavedMember.Hinge_End = SavedHinge1;

            foreach (KeyValuePair<string, Dictionary<string, double>> kvp in SavedLoads)
            {
                SavedMember.Loads[kvp.Key] = new Dictionary<string, double> { { "Qx0", SavedLoads[kvp.Key]["Qx0"] }, { "Qx1", SavedLoads[kvp.Key]["Qx1"] },
                                                                              { "Qy0", SavedLoads[kvp.Key]["Qy0"] },  { "Qy1", SavedLoads[kvp.Key]["Qy1"] },
                                                                              { "Type", SavedLoads[kvp.Key]["Type"] } };
            }

            VarHolder.MembersList.Add(SavedMember);
            SavedMember.AddToCanvas();
            SavedMember.Reposition();
        }
    }

}
