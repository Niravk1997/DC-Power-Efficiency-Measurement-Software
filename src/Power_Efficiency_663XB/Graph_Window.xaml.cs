using Microsoft.Win32;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Power_Efficiency_663XB
{
    
    public partial class Graph_Window : Window
    {
        Boolean showLegend = true;
        Boolean AutoAxis = true;

        Boolean MouseTrack = false;

        PlottableVLine Mouse_VLine;
        PlottableHLine Mouse_HLine;
        PlottableText Coord_text;

        string XUnits = "";
        string YUnits = "";

        public Graph_Window()
        {
            InitializeComponent();
            Graph.plt.Legend(enableLegend: true);
        }

        public void insertGraph(string GraphName, List<double> Xvalues, List<double> Yvalues, string Colour)
        {
            try
            {
                string[] Color_Parts = Colour.Split(',');
                System.Drawing.Color Plot_Color = System.Drawing.Color.FromArgb(Convert.ToInt32(Color_Parts[0]), Convert.ToInt32(Color_Parts[1]), Convert.ToInt32(Color_Parts[2]));
                Graph.plt.PlotScatter(Xvalues.ToArray(), Yvalues.ToArray(), color: Plot_Color, label: GraphName);
                if (AutoAxis == true & MouseTrack == false)
                {
                    Graph.plt.AxisAuto();
                }
                Graph.Render(skipIfCurrentlyRendering: true);
            }
            catch (Exception)
            {
                insert_Log("Error: Could not add Test to Graph.", "0,0,0");
            }
        }

        public void insert_Log(string Message, string Colour)
        {
            string[] Color_Parts = Colour.Split(',');
            SolidColorBrush Plot_Color = new SolidColorBrush(Color.FromArgb(255, (byte)Convert.ToInt32(Color_Parts[0]), (byte)Convert.ToInt32(Color_Parts[1]), (byte)Convert.ToInt32(Color_Parts[2])));
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                Output_Log.Inlines.Add(new Run(Message + "\n") { Foreground = Plot_Color });
                Output_Log_Scroll.ScrollToBottom();
            }));
        }

        private void bgGreen_Text_Click(object sender, RoutedEventArgs e)
        {
            bgGreen_Text.IsChecked = true;
            bgBlue_Text.IsChecked = false;
            bgRed_Text.IsChecked = false;
            bgYellow_Text.IsChecked = false;
            bgOrange_Text.IsChecked = false;
            bgBlack_Text.IsChecked = false;
            bgPink_Text.IsChecked = false;
            bgViolet_Text.IsChecked = false;
            bgWhite_Text.IsChecked = false;
            Graph.plt.Style(dataBg: System.Drawing.ColorTranslator.FromHtml("#FFAAFFB2"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void bgBlue_Text_Click(object sender, RoutedEventArgs e)
        {
            bgGreen_Text.IsChecked = false;
            bgBlue_Text.IsChecked = true;
            bgRed_Text.IsChecked = false;
            bgYellow_Text.IsChecked = false;
            bgOrange_Text.IsChecked = false;
            bgBlack_Text.IsChecked = false;
            bgPink_Text.IsChecked = false;
            bgViolet_Text.IsChecked = false;
            bgWhite_Text.IsChecked = false;
            Graph.plt.Style(dataBg: System.Drawing.ColorTranslator.FromHtml("#FFA1E7FF"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void bgRed_Text_Click(object sender, RoutedEventArgs e)
        {
            bgGreen_Text.IsChecked = false;
            bgBlue_Text.IsChecked = false;
            bgRed_Text.IsChecked = true;
            bgYellow_Text.IsChecked = false;
            bgOrange_Text.IsChecked = false;
            bgBlack_Text.IsChecked = false;
            bgPink_Text.IsChecked = false;
            bgViolet_Text.IsChecked = false;
            bgWhite_Text.IsChecked = false;
            Graph.plt.Style(dataBg: System.Drawing.ColorTranslator.FromHtml("#FFFF8989"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void bgYellow_Text_Click(object sender, RoutedEventArgs e)
        {
            bgGreen_Text.IsChecked = false;
            bgBlue_Text.IsChecked = false;
            bgRed_Text.IsChecked = false;
            bgYellow_Text.IsChecked = true;
            bgOrange_Text.IsChecked = false;
            bgBlack_Text.IsChecked = false;
            bgPink_Text.IsChecked = false;
            bgViolet_Text.IsChecked = false;
            bgWhite_Text.IsChecked = false;
            Graph.plt.Style(dataBg: System.Drawing.ColorTranslator.FromHtml("#FFFFFF93"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void bgOrange_Text_Click(object sender, RoutedEventArgs e)
        {
            bgGreen_Text.IsChecked = false;
            bgBlue_Text.IsChecked = false;
            bgRed_Text.IsChecked = false;
            bgYellow_Text.IsChecked = false;
            bgOrange_Text.IsChecked = true;
            bgBlack_Text.IsChecked = false;
            bgPink_Text.IsChecked = false;
            bgViolet_Text.IsChecked = false;
            bgWhite_Text.IsChecked = false;
            Graph.plt.Style(dataBg: System.Drawing.ColorTranslator.FromHtml("#FFFFCB8C"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void bgBlack_Text_Click(object sender, RoutedEventArgs e)
        {
            bgGreen_Text.IsChecked = false;
            bgBlue_Text.IsChecked = false;
            bgRed_Text.IsChecked = false;
            bgYellow_Text.IsChecked = false;
            bgOrange_Text.IsChecked = false;
            bgBlack_Text.IsChecked = true;
            bgPink_Text.IsChecked = false;
            bgViolet_Text.IsChecked = false;
            bgWhite_Text.IsChecked = false;
            Graph.plt.Style(dataBg: System.Drawing.ColorTranslator.FromHtml("#FF6E6E6E"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void bgPink_Text_Click(object sender, RoutedEventArgs e)
        {
            bgGreen_Text.IsChecked = false;
            bgBlue_Text.IsChecked = false;
            bgRed_Text.IsChecked = false;
            bgYellow_Text.IsChecked = false;
            bgOrange_Text.IsChecked = false;
            bgBlack_Text.IsChecked = false;
            bgPink_Text.IsChecked = true;
            bgViolet_Text.IsChecked = false;
            bgWhite_Text.IsChecked = false;
            Graph.plt.Style(dataBg: System.Drawing.ColorTranslator.FromHtml("#FFFF9ED2"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void bgViolet_Text_Click(object sender, RoutedEventArgs e)
        {
            bgGreen_Text.IsChecked = false;
            bgBlue_Text.IsChecked = false;
            bgRed_Text.IsChecked = false;
            bgYellow_Text.IsChecked = false;
            bgOrange_Text.IsChecked = false;
            bgBlack_Text.IsChecked = false;
            bgPink_Text.IsChecked = false;
            bgViolet_Text.IsChecked = true;
            bgWhite_Text.IsChecked = false;
            Graph.plt.Style(dataBg: System.Drawing.ColorTranslator.FromHtml("#FFE6ACFF"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void bgWhite_Text_Click(object sender, RoutedEventArgs e)
        {
            bgGreen_Text.IsChecked = false;
            bgBlue_Text.IsChecked = false;
            bgRed_Text.IsChecked = false;
            bgYellow_Text.IsChecked = false;
            bgOrange_Text.IsChecked = false;
            bgBlack_Text.IsChecked = false;
            bgPink_Text.IsChecked = false;
            bgViolet_Text.IsChecked = false;
            bgWhite_Text.IsChecked = true;
            Graph.plt.Style(dataBg: System.Drawing.ColorTranslator.FromHtml("#FFFFFFFF"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void fgGreen_Text_Click(object sender, RoutedEventArgs e)
        {
            fgGreen_Text.IsChecked = true;
            fgBlue_Text.IsChecked = false;
            fgRed_Text.IsChecked = false;
            fgYellow_Text.IsChecked = false;
            fgOrange_Text.IsChecked = false;
            fgBlack_Text.IsChecked = false;
            fgPink_Text.IsChecked = false;
            fgViolet_Text.IsChecked = false;
            fgWhite_Text.IsChecked = false;
            Graph.plt.Style(figBg: System.Drawing.ColorTranslator.FromHtml("#FFAAFFB2"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void fgBlue_Text_Click(object sender, RoutedEventArgs e)
        {
            fgGreen_Text.IsChecked = false;
            fgBlue_Text.IsChecked = true;
            fgRed_Text.IsChecked = false;
            fgYellow_Text.IsChecked = false;
            fgOrange_Text.IsChecked = false;
            fgBlack_Text.IsChecked = false;
            fgPink_Text.IsChecked = false;
            fgViolet_Text.IsChecked = false;
            fgWhite_Text.IsChecked = false;
            Graph.plt.Style(figBg: System.Drawing.ColorTranslator.FromHtml("#FFA1E7FF"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void fgRed_Text_Click(object sender, RoutedEventArgs e)
        {
            fgGreen_Text.IsChecked = false;
            fgBlue_Text.IsChecked = false;
            fgRed_Text.IsChecked = true;
            fgYellow_Text.IsChecked = false;
            fgOrange_Text.IsChecked = false;
            fgBlack_Text.IsChecked = false;
            fgPink_Text.IsChecked = false;
            fgViolet_Text.IsChecked = false;
            fgWhite_Text.IsChecked = false;
            Graph.plt.Style(figBg: System.Drawing.ColorTranslator.FromHtml("#FFFF8989"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void fgYellow_Text_Click(object sender, RoutedEventArgs e)
        {
            fgGreen_Text.IsChecked = false;
            fgBlue_Text.IsChecked = false;
            fgRed_Text.IsChecked = false;
            fgYellow_Text.IsChecked = true;
            fgOrange_Text.IsChecked = false;
            fgBlack_Text.IsChecked = false;
            fgPink_Text.IsChecked = false;
            fgViolet_Text.IsChecked = false;
            fgWhite_Text.IsChecked = false;
            Graph.plt.Style(figBg: System.Drawing.ColorTranslator.FromHtml("#FFFFFF93"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void fgOrange_Text_Click(object sender, RoutedEventArgs e)
        {
            fgGreen_Text.IsChecked = false;
            fgBlue_Text.IsChecked = false;
            fgRed_Text.IsChecked = false;
            fgYellow_Text.IsChecked = false;
            fgOrange_Text.IsChecked = true;
            fgBlack_Text.IsChecked = false;
            fgPink_Text.IsChecked = false;
            fgViolet_Text.IsChecked = false;
            fgWhite_Text.IsChecked = false;
            Graph.plt.Style(figBg: System.Drawing.ColorTranslator.FromHtml("#FFFFCB8C"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void fgBlack_Text_Click(object sender, RoutedEventArgs e)
        {
            fgGreen_Text.IsChecked = false;
            fgBlue_Text.IsChecked = false;
            fgRed_Text.IsChecked = false;
            fgYellow_Text.IsChecked = false;
            fgOrange_Text.IsChecked = false;
            fgBlack_Text.IsChecked = true;
            fgPink_Text.IsChecked = false;
            fgViolet_Text.IsChecked = false;
            fgWhite_Text.IsChecked = false;
            Graph.plt.Style(figBg: System.Drawing.ColorTranslator.FromHtml("#FF6E6E6E"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }


        private void fgPink_Text_Click(object sender, RoutedEventArgs e)
        {
            fgGreen_Text.IsChecked = false;
            fgBlue_Text.IsChecked = false;
            fgRed_Text.IsChecked = false;
            fgYellow_Text.IsChecked = false;
            fgOrange_Text.IsChecked = false;
            fgBlack_Text.IsChecked = false;
            fgPink_Text.IsChecked = true;
            fgViolet_Text.IsChecked = false;
            fgWhite_Text.IsChecked = false;
            Graph.plt.Style(figBg: System.Drawing.ColorTranslator.FromHtml("#FFFF9ED2"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void fgViolet_Text_Click(object sender, RoutedEventArgs e)
        {
            fgGreen_Text.IsChecked = false;
            fgBlue_Text.IsChecked = false;
            fgRed_Text.IsChecked = false;
            fgYellow_Text.IsChecked = false;
            fgOrange_Text.IsChecked = false;
            fgBlack_Text.IsChecked = false;
            fgPink_Text.IsChecked = false;
            fgViolet_Text.IsChecked = true;
            fgWhite_Text.IsChecked = false;
            Graph.plt.Style(figBg: System.Drawing.ColorTranslator.FromHtml("#FFE6ACFF"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void fgWhite_Text_Click(object sender, RoutedEventArgs e)
        {
            fgGreen_Text.IsChecked = false;
            fgBlue_Text.IsChecked = false;
            fgRed_Text.IsChecked = false;
            fgYellow_Text.IsChecked = false;
            fgOrange_Text.IsChecked = false;
            fgBlack_Text.IsChecked = false;
            fgPink_Text.IsChecked = false;
            fgViolet_Text.IsChecked = false;
            fgWhite_Text.IsChecked = true;
            Graph.plt.Style(figBg: System.Drawing.ColorTranslator.FromHtml("#FFFFFFFF"));
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Default_Theme_Click(object sender, RoutedEventArgs e)
        {
            Default_Theme.IsChecked = true;
            Black_Theme.IsChecked = false;
            Blue_Theme.IsChecked = false;
            Gray_Theme.IsChecked = false;
            GrayBlack_Theme.IsChecked = false;
            Graph.plt.Style(ScottPlot.Style.Default);
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Black_Theme_Click(object sender, RoutedEventArgs e)
        {
            Default_Theme.IsChecked = false;
            Black_Theme.IsChecked = true;
            Blue_Theme.IsChecked = false;
            Gray_Theme.IsChecked = false;
            GrayBlack_Theme.IsChecked = false;
            Graph.plt.Style(ScottPlot.Style.Black);
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Blue_Theme_Click(object sender, RoutedEventArgs e)
        {
            Default_Theme.IsChecked = false;
            Black_Theme.IsChecked = false;
            Blue_Theme.IsChecked = true;
            Gray_Theme.IsChecked = false;
            GrayBlack_Theme.IsChecked = false;
            Graph.plt.Style(ScottPlot.Style.Blue1);
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Gray_Theme_Click(object sender, RoutedEventArgs e)
        {
            Default_Theme.IsChecked = false;
            Black_Theme.IsChecked = false;
            Blue_Theme.IsChecked = false;
            Gray_Theme.IsChecked = true;
            GrayBlack_Theme.IsChecked = false;
            Graph.plt.Style(ScottPlot.Style.Gray1);
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void GrayBlack_Theme_Click(object sender, RoutedEventArgs e)
        {
            Default_Theme.IsChecked = false;
            Black_Theme.IsChecked = false;
            Blue_Theme.IsChecked = false;
            Gray_Theme.IsChecked = false;
            GrayBlack_Theme.IsChecked = true;
            Graph.plt.Style(ScottPlot.Style.Gray2);
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Graph_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PNG files (*.png)|*.png"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    Graph.plt.SaveFig(saveFileDialog.FileName);
                }
            }
            catch (Exception) { insert_Log("Error: Could not save file.", "0,0,0"); }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Log_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (TextWriter datatotxt = new StreamWriter(saveFileDialog.FileName, true))
                    {
                        datatotxt.WriteLine(String.Join(String.Empty, Output_Log.Inlines.Select(line => line.ContentStart.GetTextInRun(LogicalDirection.Forward))).ToString());
                    }
                }
            }
            catch (Exception)
            {
                insert_Log("Error: Could not save file.", "0,0,0");
            }
        }

        private void Clear_Log_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Output_Log.Inlines.Clear();
                Output_Log.Text = string.Empty;
            }
            catch (Exception)
            {
                insert_Log("Error: Could not clear output log.", "0,0,0");
            }
        }

        private void Clear_Plots_Click(object sender, RoutedEventArgs e)
        {
            Graph.plt.Clear();
            MouseTrack = false;
            Mouse_Tracker.IsChecked = false;
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Show_legend_Click(object sender, RoutedEventArgs e)
        {
            showLegend = !showLegend;
            Show_legend.IsChecked = showLegend;
            Graph.plt.Legend(enableLegend: showLegend);
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Legend_TopLeft_Click(object sender, RoutedEventArgs e)
        {
            Legend_TopLeft.IsChecked = true;
            Legend_TopRight.IsChecked = false;
            Legend_BottomLeft.IsChecked = false;
            Legend_BottomRight.IsChecked = false;
            Show_legend.IsChecked = true;
            Graph.plt.Legend(location: legendLocation.upperLeft);
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Legend_TopRight_Click(object sender, RoutedEventArgs e)
        {
            Legend_TopLeft.IsChecked = false;
            Legend_TopRight.IsChecked = true;
            Legend_BottomLeft.IsChecked = false;
            Legend_BottomRight.IsChecked = false;
            Show_legend.IsChecked = true;
            Graph.plt.Legend(location: legendLocation.upperRight);
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Legend_BottomLeft_Click(object sender, RoutedEventArgs e)
        {
            Legend_TopLeft.IsChecked = false;
            Legend_TopRight.IsChecked = false;
            Legend_BottomLeft.IsChecked = true;
            Legend_BottomRight.IsChecked = false;
            Show_legend.IsChecked = true;
            Graph.plt.Legend(location: legendLocation.lowerLeft);
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Legend_BottomRight_Click(object sender, RoutedEventArgs e)
        {
            Legend_TopLeft.IsChecked = false;
            Legend_TopRight.IsChecked = false;
            Legend_BottomLeft.IsChecked = false;
            Legend_BottomRight.IsChecked = true;
            Show_legend.IsChecked = true;
            Graph.plt.Legend(location: legendLocation.lowerRight);
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Auto_Axis_Disable_Click(object sender, RoutedEventArgs e)
        {
            AutoAxis = false;
            Auto_Axis_Disable.IsChecked = true;
            Auto_Axix_Enable.IsChecked = false;
        }

        private void Auto_Axix_Enable_Click(object sender, RoutedEventArgs e)
        {
            AutoAxis = true;
            Auto_Axis_Disable.IsChecked = false;
            Auto_Axix_Enable.IsChecked = true;
        }

        private void Force_AutoAxis_Click(object sender, RoutedEventArgs e)
        {
            Graph.plt.AxisAuto();
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Drag_H_Marker_Click(object sender, RoutedEventArgs e)
        {
            Graph.plt.PlotHLine(0, draggable: true, lineStyle: LineStyle.Dash);
            if (AutoAxis == true & MouseTrack == false)
            {
                Graph.plt.AxisAuto();
            }
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Drag_V_Marker_Click(object sender, RoutedEventArgs e)
        {
            Graph.plt.PlotVLine(0, draggable: true, lineStyle: LineStyle.Dash);
            if (AutoAxis == true & MouseTrack == false)
            {
                Graph.plt.AxisAuto();
            }
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Add_H_Marker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double HValue = Double.Parse(H_Marker_Name.Text, System.Globalization.NumberStyles.Float);
                Graph.plt.PlotHLine(HValue, label: "H Marker: " + HValue.ToString(), lineStyle: LineStyle.Dash);
                if (AutoAxis == true & MouseTrack == false)
                {
                    Graph.plt.AxisAuto();
                }
                Graph.Render(skipIfCurrentlyRendering: true);
                H_Marker_Name.Text = string.Empty;
            }
            catch (Exception)
            {
                H_Marker_Name.Text = string.Empty;
                insert_Log("Error: Could not add Horizontal Marker, value must be a number.", "0,0,0");
            }
        }

        private void Add_V_Marker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double VValue = Double.Parse(V_Marker_Name.Text, System.Globalization.NumberStyles.Float);
                Graph.plt.PlotVLine(VValue, label: "V Marker: " + VValue.ToString(), lineStyle: LineStyle.Dash);
                if (AutoAxis == true & MouseTrack == false)
                {
                    Graph.plt.AxisAuto();
                }
                Graph.Render(skipIfCurrentlyRendering: true);
                V_Marker_Name.Text = string.Empty;
            }
            catch (Exception)
            {
                V_Marker_Name.Text = string.Empty;
                insert_Log("Error: Could not add Vertical Marker, value must be a number.", "0,0,0");
            }
        }

        public void setXYLabels(string Xlabel, string Ylabel)
        {
            try
            {
                Graph.plt.XLabel(Xlabel);
                Graph.plt.YLabel(Ylabel);
            }
            catch (Exception)
            {
                insert_Log("Error: Could not add Graph X and Y Labels", "0,0,0");
            }
        }

        public void setXYUnits(string XUnit, string YUnit)
        {
            try
            {
                XUnits = XUnit;
                YUnits = YUnit;
            }
            catch (Exception)
            {
                insert_Log("Error: Could not add X and Y Units.", "0,0,0");
            }
        }

        public void setWindowTitle(string title)
        {
            try
            {
                this.Title = title;
                Graph_Log_Name.Text = title;
            }
            catch (Exception)
            {
                insert_Log("Error: Could not add Window Title", "0,0,0");
            }
        }

        public void resizeWindow(int Height, int Width)
        {
            try
            {
                this.Height = Height;
                this.Width = Width;
            }
            catch (Exception)
            {
                insert_Log("Error: Could not Resize Window", "0,0,0");
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            insert_Log("If window becomes slow and sluggish, clear output log and plots.", "0,0,0");
        }

        private void Credits_Click(object sender, RoutedEventArgs e)
        {
            insert_Log("Created by Niravk Patel, using open-source ScottPlot Graphing Library.", "0,0,0");
        }

        private void Diagonal_Line_Click(object sender, RoutedEventArgs e)
        {
            Graph.plt.PlotLine(0, 0, 100, 100, label: "Diagonal Line", lineStyle: LineStyle.Dash);
            if (AutoAxis == true & MouseTrack == false)
            {
                Graph.plt.AxisAuto();
            }
            Graph.Render(skipIfCurrentlyRendering: true);
        }

        private void Mouse_Tracker_Click(object sender, RoutedEventArgs e)
        {
            MouseTrack = !MouseTrack;
            if (MouseTrack == true)
            {
                Mouse_VLine = Graph.plt.PlotVLine(0, color: System.Drawing.Color.Red, lineStyle: LineStyle.DashDot);
                Mouse_HLine = Graph.plt.PlotHLine(0, color: System.Drawing.Color.Red, lineStyle: LineStyle.DashDot);
                Coord_text = Graph.plt.PlotText("", 0, 0, color: System.Drawing.Color.Red, fontSize: 18, alignment: ScottPlot.TextAlignment.lowerLeft);
            }
            else
            {
                Graph.plt.Clear(Mouse_VLine);
                Graph.plt.Clear(Mouse_HLine);
                Graph.plt.Clear(Coord_text);
                if (AutoAxis == true)
                {
                    Graph.plt.AxisAuto();
                }
                Graph.Render(skipIfCurrentlyRendering: true);
            }
        }

        private void Graph_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseTrack == true)
            {
                (double coordinateX, double coordinateY) = Graph.GetMouseCoordinates();

                Coord_text.text = Math.Round(coordinateX, 3) + XUnits + ", " + Math.Round(coordinateY, 3) + YUnits;
                Coord_text.x = coordinateX;
                Coord_text.y = coordinateY;

                Mouse_VLine.position = coordinateX;
                Mouse_HLine.position = coordinateY;

                Graph.Render(skipIfCurrentlyRendering: true);
            }
        }
    }
}
