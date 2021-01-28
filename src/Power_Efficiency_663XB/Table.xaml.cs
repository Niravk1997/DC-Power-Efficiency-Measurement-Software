using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Shapes;

namespace Power_Efficiency_663XB
{
    public class TestData
    {
        public TestData(int test_number, string testName, double sourceVoltage, double sourceCurrent, double sinkVoltage, double sinkCurrent, double inputPower, double outputPower, double powerEfficiency, double powerLoss, double sourceResistance, double sinkResistance, double circuitResistance)
        {
            Test_Number = test_number;
            TestName = testName;
            SourceVoltage = sourceVoltage;
            SourceCurrent = sourceCurrent;
            SinkVoltage = sinkVoltage;
            SinkCurrent = sinkCurrent;
            InputPower = inputPower;
            OutputPower = outputPower;
            PowerEfficiency = powerEfficiency;
            PowerLoss = powerLoss;
            SourceResistance = sourceResistance;
            SinkResistance = sinkResistance;
            CircuitResistance = circuitResistance;
        }
        public int Test_Number { get; set; }
        public string TestName { get; set; }
        public double SourceVoltage { get; set; }
        public double SourceCurrent { get; set; }
        public double SinkVoltage { get; set; }
        public double SinkCurrent { get; set; }
        public double InputPower { get; set; }
        public double OutputPower { get; set; }
        public double PowerEfficiency { get; set; }
        public double PowerLoss { get; set; }
        public double SourceResistance { get; set; }
        public double SinkResistance { get; set; }
        public double CircuitResistance { get; set; }
    }

    public partial class Table : Window
    {
        ObservableCollection<TestData> testData = new ObservableCollection<TestData>();
        int total_Tests = 0;

        SolidColorBrush Green = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF17"));
        SolidColorBrush Blue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00C0FF"));
        SolidColorBrush Red = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
        SolidColorBrush Yellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF00"));
        SolidColorBrush Orange = new SolidColorBrush((Color)ColorConverter.ConvertFromString("DarkOrange"));
        SolidColorBrush White = new SolidColorBrush(Colors.White);
        SolidColorBrush Gray = new SolidColorBrush(Colors.Gray);
        SolidColorBrush Black = new SolidColorBrush(Colors.Black);

        public Table()
        {
            InitializeComponent();
            TestTable.ItemsSource = testData;
        }

        public void addTest(string Name, double SourceVoltage, double SourceCurrent, double SinkVoltage, double SinkCurrent, double InputPower, double OutputPower, double PowerEfficiency, double powerLoss, double sourceResistance, double sinkResistance, double circuitResistnace)
        {
            try
            {
                total_Tests++;
                testData.Add(new TestData(total_Tests, Name, SourceVoltage, SourceCurrent, SinkVoltage, SinkCurrent, InputPower, OutputPower, PowerEfficiency, powerLoss, sourceResistance, sinkResistance, circuitResistnace));
                totalTests.Content = total_Tests.ToString();
            }
            catch (Exception)
            {
                total_Tests--;
                totalTests.Content = total_Tests.ToString();
            }
        }

        private void Exit_Table_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ClearTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                testData.Clear();
                total_Tests = 0;
                totalTests.Content = total_Tests.ToString();
            }
            catch (Exception)
            {

            }
        }

        private void SaveTable_csv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (TextWriter datatotxt = new StreamWriter(saveFileDialog.FileName, false))
                    {
                        TestTable.SelectAllCells();
                        TestTable.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                        ApplicationCommands.Copy.Execute(null, TestTable);
                        TestTable.UnselectAllCells();
                        datatotxt.Write((string)Clipboard.GetData(DataFormats.CommaSeparatedValue));
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void SaveTable_text_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (TextWriter datatotxt = new StreamWriter(saveFileDialog.FileName, false))
                    {
                        TestTable.SelectAllCells();
                        TestTable.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                        ApplicationCommands.Copy.Execute(null, TestTable);
                        TestTable.UnselectAllCells();
                        datatotxt.Write((string)Clipboard.GetData(DataFormats.CommaSeparatedValue));
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void Table_12_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontSize = 12;
            Table_12.IsChecked = true;
            Table_14.IsChecked = false;
            Table_16.IsChecked = false;
            Table_18.IsChecked = false;
            TableContentFit(1);
            TableContentFit(2);
        }

        private void Table_14_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontSize = 14;
            Table_12.IsChecked = false;
            Table_14.IsChecked = true;
            Table_16.IsChecked = false;
            Table_18.IsChecked = false;
            TableContentFit(1);
            TableContentFit(2);
        }

        private void Table_16_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontSize = 16;
            Table_12.IsChecked = false;
            Table_14.IsChecked = false;
            Table_16.IsChecked = true;
            Table_18.IsChecked = false;
            TableContentFit(1);
            TableContentFit(2);
        }

        private void Table_18_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontSize = 18;
            Table_12.IsChecked = false;
            Table_14.IsChecked = false;
            Table_16.IsChecked = false;
            Table_18.IsChecked = true;
            TableContentFit(1);
            TableContentFit(2);
        }

        private void TableContentFit(int resizePer)
        {
            foreach (DataGridColumn column in TestTable.Columns)
            {
                if (resizePer == 0)
                {
                    column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToHeader);
                }
                else if (resizePer == 1)
                {
                    column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
                }
                else
                {
                    column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Auto);
                }
            }
        }

        private void resize_perHeader_Click(object sender, RoutedEventArgs e)
        {
            TableContentFit(0);
            resize_perHeader.IsChecked = true;
            resize_perColumn.IsChecked = false;
            resize_perBoth.IsChecked = false;
        }

        private void resize_perColumn_Click(object sender, RoutedEventArgs e)
        {
            TableContentFit(1);
            resize_perHeader.IsChecked = false;
            resize_perColumn.IsChecked = true;
            resize_perBoth.IsChecked = false;
        }

        private void resize_perBoth_Click(object sender, RoutedEventArgs e)
        {
            TableContentFit(2);
            resize_perHeader.IsChecked = false;
            resize_perColumn.IsChecked = false;
            resize_perBoth.IsChecked = true;
        }

        private void Arial_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontFamily = new FontFamily("Arial");
            Arial.IsChecked = true;
            Arial_Black.IsChecked = false;
            Courier_New.IsChecked = false;
            Comic_Sans_MS.IsChecked = false;
            Ink_Free.IsChecked = false;
            Segoe_UI.IsChecked = false;
            Segoe_UI_Black.IsChecked = false;
            Times_New_Roman.IsChecked = false;
        }

        private void Arial_Black_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontFamily = new FontFamily("Arial Black");
            Arial.IsChecked = false;
            Arial_Black.IsChecked = true;
            Courier_New.IsChecked = false;
            Comic_Sans_MS.IsChecked = false;
            Ink_Free.IsChecked = false;
            Segoe_UI.IsChecked = false;
            Segoe_UI_Black.IsChecked = false;
            Times_New_Roman.IsChecked = false;
        }

        private void Courier_New_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontFamily = new FontFamily("Courier New");
            Arial.IsChecked = false;
            Arial_Black.IsChecked = false;
            Courier_New.IsChecked = true;
            Comic_Sans_MS.IsChecked = false;
            Ink_Free.IsChecked = false;
            Segoe_UI.IsChecked = false;
            Segoe_UI_Black.IsChecked = false;
            Times_New_Roman.IsChecked = false;
        }

        private void Comic_Sans_MS_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontFamily = new FontFamily("Comic Sans MS");
            Arial.IsChecked = false;
            Arial_Black.IsChecked = false;
            Courier_New.IsChecked = false;
            Comic_Sans_MS.IsChecked = true;
            Ink_Free.IsChecked = false;
            Segoe_UI.IsChecked = false;
            Segoe_UI_Black.IsChecked = false;
            Times_New_Roman.IsChecked = false;
        }

        private void Ink_Free_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontFamily = new FontFamily("Ink Free");
            Arial.IsChecked = false;
            Arial_Black.IsChecked = false;
            Courier_New.IsChecked = false;
            Comic_Sans_MS.IsChecked = false;
            Ink_Free.IsChecked = true;
            Segoe_UI.IsChecked = false;
            Segoe_UI_Black.IsChecked = false;
            Times_New_Roman.IsChecked = false;
        }

        private void Segoe_UI_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontFamily = new FontFamily("Segoe UI");
            Arial.IsChecked = false;
            Arial_Black.IsChecked = false;
            Courier_New.IsChecked = false;
            Comic_Sans_MS.IsChecked = false;
            Ink_Free.IsChecked = false;
            Segoe_UI.IsChecked = true;
            Segoe_UI_Black.IsChecked = false;
            Times_New_Roman.IsChecked = false;
        }

        private void Segoe_UI_Black_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontFamily = new FontFamily("Segoe UI Black");
            Arial.IsChecked = false;
            Arial_Black.IsChecked = false;
            Courier_New.IsChecked = false;
            Comic_Sans_MS.IsChecked = false;
            Ink_Free.IsChecked = false;
            Segoe_UI.IsChecked = false;
            Segoe_UI_Black.IsChecked = true;
            Times_New_Roman.IsChecked = false;
        }

        private void Times_New_Roman_Click(object sender, RoutedEventArgs e)
        {
            TestTable.FontFamily = new FontFamily("Times New Roman");
            Arial.IsChecked = false;
            Arial_Black.IsChecked = false;
            Courier_New.IsChecked = false;
            Comic_Sans_MS.IsChecked = false;
            Ink_Free.IsChecked = false;
            Segoe_UI.IsChecked = false;
            Segoe_UI_Black.IsChecked = false;
            Times_New_Roman.IsChecked = true;
        }

        private void BackgroundGreen_Click(object sender, RoutedEventArgs e)
        {
            TestTable.RowBackground = Green;
            BackgroundGreen.IsChecked = true;
            BackgroundBlue.IsChecked = false;
            BackgroundRed.IsChecked = false;
            BackgroundYellow.IsChecked = false;
            BackgroundOrange.IsChecked = false;
            BackgroundWhite.IsChecked = false;
            BackgroundGray.IsChecked = false;
            BackgroundBlack.IsChecked = false;
            BackgroundGray.IsChecked = false;
        }

        private void BackgroundBlue_Click(object sender, RoutedEventArgs e)
        {
            TestTable.RowBackground = Blue;
            BackgroundGreen.IsChecked = false;
            BackgroundBlue.IsChecked = true;
            BackgroundRed.IsChecked = false;
            BackgroundYellow.IsChecked = false;
            BackgroundOrange.IsChecked = false;
            BackgroundWhite.IsChecked = false;
            BackgroundGray.IsChecked = false;
            BackgroundBlack.IsChecked = false;
        }

        private void BackgroundRed_Click(object sender, RoutedEventArgs e)
        {
            TestTable.RowBackground = Red;
            BackgroundGreen.IsChecked = false;
            BackgroundBlue.IsChecked = false;
            BackgroundRed.IsChecked = true;
            BackgroundYellow.IsChecked = false;
            BackgroundOrange.IsChecked = false;
            BackgroundWhite.IsChecked = false;
            BackgroundGray.IsChecked = false;
            BackgroundBlack.IsChecked = false;
        }

        private void BackgroundYellow_Click(object sender, RoutedEventArgs e)
        {
            TestTable.RowBackground = Yellow;
            BackgroundGreen.IsChecked = false;
            BackgroundBlue.IsChecked = false;
            BackgroundRed.IsChecked = false;
            BackgroundYellow.IsChecked = true;
            BackgroundOrange.IsChecked = false;
            BackgroundWhite.IsChecked = false;
            BackgroundGray.IsChecked = false;
            BackgroundBlack.IsChecked = false;
        }

        private void BackgroundOrange_Click(object sender, RoutedEventArgs e)
        {
            TestTable.RowBackground = Orange;
            BackgroundGreen.IsChecked = false;
            BackgroundBlue.IsChecked = false;
            BackgroundRed.IsChecked = false;
            BackgroundYellow.IsChecked = false;
            BackgroundOrange.IsChecked = true;
            BackgroundWhite.IsChecked = false;
            BackgroundGray.IsChecked = false;
            BackgroundBlack.IsChecked = false;
        }

        private void BackgroundWhite_Click(object sender, RoutedEventArgs e)
        {
            TestTable.RowBackground = White;
            BackgroundGreen.IsChecked = false;
            BackgroundBlue.IsChecked = false;
            BackgroundRed.IsChecked = false;
            BackgroundYellow.IsChecked = false;
            BackgroundOrange.IsChecked = false;
            BackgroundWhite.IsChecked = true;
            BackgroundGray.IsChecked = false;
            BackgroundBlack.IsChecked = false;
        }

        private void BackgroundGray_Click(object sender, RoutedEventArgs e)
        {
            TestTable.RowBackground = Gray;
            BackgroundGreen.IsChecked = false;
            BackgroundBlue.IsChecked = false;
            BackgroundRed.IsChecked = false;
            BackgroundYellow.IsChecked = false;
            BackgroundOrange.IsChecked = false;
            BackgroundWhite.IsChecked = false;
            BackgroundGray.IsChecked = true;
            BackgroundBlack.IsChecked = false;
        }

        private void BackgroundBlack_Click(object sender, RoutedEventArgs e)
        {
            TestTable.RowBackground = Black;
            BackgroundGreen.IsChecked = false;
            BackgroundBlue.IsChecked = false;
            BackgroundRed.IsChecked = false;
            BackgroundYellow.IsChecked = false;
            BackgroundOrange.IsChecked = false;
            BackgroundWhite.IsChecked = false;
            BackgroundGray.IsChecked = false;
            BackgroundBlack.IsChecked = true;
        }

        private void AltBackgroundGreen_Click(object sender, RoutedEventArgs e)
        {
            TestTable.AlternatingRowBackground = Green;
            AltBackgroundGreen.IsChecked = true;
            AltBackgroundBlue.IsChecked = false;
            AltBackgroundRed.IsChecked = false;
            AltBackgroundYellow.IsChecked = false;
            AltBackgroundOrange.IsChecked = false;
            AltBackgroundWhite.IsChecked = false;
            AltBackgroundGray.IsChecked = false;
            AltBackgroundBlack.IsChecked = false;

        }

        private void AltBackgroundBlue_Click(object sender, RoutedEventArgs e)
        {
            TestTable.AlternatingRowBackground = Blue;
            AltBackgroundGreen.IsChecked = false;
            AltBackgroundBlue.IsChecked = true;
            AltBackgroundRed.IsChecked = false;
            AltBackgroundYellow.IsChecked = false;
            AltBackgroundOrange.IsChecked = false;
            AltBackgroundWhite.IsChecked = false;
            AltBackgroundGray.IsChecked = false;
            AltBackgroundBlack.IsChecked = false;

        }

        private void AltBackgroundRed_Click(object sender, RoutedEventArgs e)
        {
            TestTable.AlternatingRowBackground = Red;
            AltBackgroundGreen.IsChecked = false;
            AltBackgroundBlue.IsChecked = false;
            AltBackgroundRed.IsChecked = true;
            AltBackgroundYellow.IsChecked = false;
            AltBackgroundOrange.IsChecked = false;
            AltBackgroundWhite.IsChecked = false;
            AltBackgroundGray.IsChecked = false;
            AltBackgroundBlack.IsChecked = false;

        }

        private void AltBackgroundYellow_Click(object sender, RoutedEventArgs e)
        {
            TestTable.AlternatingRowBackground = Yellow;
            AltBackgroundGreen.IsChecked = false;
            AltBackgroundBlue.IsChecked = false;
            AltBackgroundRed.IsChecked = false;
            AltBackgroundYellow.IsChecked = true;
            AltBackgroundOrange.IsChecked = false;
            AltBackgroundWhite.IsChecked = false;
            AltBackgroundGray.IsChecked = false;
            AltBackgroundBlack.IsChecked = false;

        }

        private void AltBackgroundOrange_Click(object sender, RoutedEventArgs e)
        {
            TestTable.AlternatingRowBackground = Orange;
            AltBackgroundGreen.IsChecked = false;
            AltBackgroundBlue.IsChecked = false;
            AltBackgroundRed.IsChecked = false;
            AltBackgroundYellow.IsChecked = false;
            AltBackgroundOrange.IsChecked = true;
            AltBackgroundWhite.IsChecked = false;
            AltBackgroundGray.IsChecked = false;
            AltBackgroundBlack.IsChecked = false;

        }

        private void AltBackgroundWhite_Click(object sender, RoutedEventArgs e)
        {
            TestTable.AlternatingRowBackground = White;
            AltBackgroundGreen.IsChecked = false;
            AltBackgroundBlue.IsChecked = false;
            AltBackgroundRed.IsChecked = false;
            AltBackgroundYellow.IsChecked = false;
            AltBackgroundOrange.IsChecked = false;
            AltBackgroundWhite.IsChecked = true;
            AltBackgroundGray.IsChecked = false;
            AltBackgroundBlack.IsChecked = false;

        }

        private void AltBackgroundGray_Click(object sender, RoutedEventArgs e)
        {
            TestTable.AlternatingRowBackground = Gray;
            AltBackgroundGreen.IsChecked = false;
            AltBackgroundBlue.IsChecked = false;
            AltBackgroundRed.IsChecked = false;
            AltBackgroundYellow.IsChecked = false;
            AltBackgroundOrange.IsChecked = false;
            AltBackgroundWhite.IsChecked = false;
            AltBackgroundGray.IsChecked = true;
            AltBackgroundBlack.IsChecked = false;

        }

        private void AltBackgroundBlack_Click(object sender, RoutedEventArgs e)
        {
            TestTable.AlternatingRowBackground = Black;
            AltBackgroundGreen.IsChecked = false;
            AltBackgroundBlue.IsChecked = false;
            AltBackgroundRed.IsChecked = false;
            AltBackgroundYellow.IsChecked = false;
            AltBackgroundOrange.IsChecked = false;
            AltBackgroundWhite.IsChecked = false;
            AltBackgroundGray.IsChecked = false;
            AltBackgroundBlack.IsChecked = true;

        }
    }
}
