using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
    public static class Serial_COM_Data
    {
        public static bool Connected = false;
        public static bool WindowOpen = false;

        //Source Device Info
        public static string Source_Model;
        public static double Source_Max_Voltage;
        public static double Source_Max_Current;
        public static string Source_COM_Port;
        public static int Source_BaudRate;
        public static int Source_Parity;
        public static int Source_StopBits;
        public static int Source_DataBits;
        public static int Source_Handshake;
        public static int Source_WriteTimeout;
        public static int Source_ReadTimeout;
        public static bool Source_RtsEnable;

        //Sink Device Info
        public static string Sink_Model;
        public static double Sink_Max_Voltage;
        public static double Sink_Max_Current;
        public static string Sink_COM_Port;
        public static int Sink_BaudRate;
        public static int Sink_Parity;
        public static int Sink_StopBits;
        public static int Sink_DataBits;
        public static int Sink_Handshake;
        public static int Sink_WriteTimeout;
        public static int Sink_ReadTimeout;
        public static bool Sink_RtsEnable;

        public static string folder_Directory;
    }

    public static class SCPI_Commands
    {
        //To be replaced by Custom Commands by User
        public static string Input_setVolt = "VOLT";
        public static string Input_setCurr = "CURR";
        public static string Input_measVolt = "MEAS:VOLT?";
        public static string Input_measCurr = "MEAS:CURR?";
        public static string Input_outputON = "OUTPut ON";
        public static string Input_outputOFF = "OUTPut OFF";
        public static int Input_minChars = 10;
        public static int Input_maxChars = 12;
        public static int Input_ScientificNotation = 1;
        public static bool customPower_Commands = false;

        public static string Output_setVolt = "VOLT";
        public static string Output_setCurr = "CURR";
        public static string Output_measVolt = "MEAS:VOLT?";
        public static string Output_measCurr = "MEAS:CURR?";
        public static string Output_outputON = "OUTPut ON";
        public static string Output_outputOFF = "OUTPut OFF";
        public static int Output_minChars = 10;
        public static int Output_maxChars = 12;
        public static int Output_ScientificNotation = 1;
        public static bool customLoad_Commands = false;

    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Source = Power Supply = Input
        //Sink = DC Load = Output

        //[Power Supply] Initially Loaded with SCPI Commands, to be replaced by user
        string Input_setVolt = "VOLT"; 
        string Input_setCurr = "CURR"; 
        string Input_measVolt = "MEAS:VOLT?";
        string Input_measCurr = "MEAS:CURR?";
        string Input_outputON = "OUTPut ON";
        string Input_outputOFF = "OUTPut OFF";
        int Input_minChars = 10;
        int Input_maxChars = 12;
        int Input_ScientificNotation = 1;

        //[DC Load] Initially Loaded with SCPI Commands, to be replaced by user
        string Output_setVolt = "VOLT";
        string Output_setCurr = "CURR";
        string Output_measVolt = "MEAS:VOLT?";
        string Output_measCurr = "MEAS:CURR?";
        string Output_outputON = "OUTPut ON";
        string Output_outputOFF = "OUTPut OFF";
        int Output_minChars = 10;
        int Output_maxChars = 12;
        int Output_ScientificNotation = 1;
        bool allowSetVoltage = false;

        //Various Codes to set Output Log Color
        int Success_Code = 0;
        int Error_Code = 1;
        int Warning_Code = 2;
        int Config_Code = 3;
        int Message_Code = 4;
        int Source_message = 5;
        int Sink_message = 6;
        int Average_message = 7;

        Select_COM_Ports Select_COM_Window;

        //Data will be added to graphs and table if their window is opened by user
        bool AddTest_Graph = true;
        bool AddTest_Table = true;

        Graph_Window PowerEfficiency_OutputCurrent;
        Graph_Window PowerEfficiency_OutputPower;
        Graph_Window OutputPower_InputPower;
        Graph_Window InputVoltage_InputCurrent;
        Graph_Window OutputVoltage_OutputCurrent;
        Graph_Window InputCurrent_InputVoltage;
        Graph_Window OutputCurrent_OutputVoltage;
        Graph_Window PowerLoss_OutputCurrent;
        Graph_Window PowerLoss_OutputPower;

        Table DataTable;

        Random randomNum = new Random();

        Queue<string> Colours = new Queue<string>();

        private DispatcherTimer timer_update;

        bool testStart = false;
        int seconds;
        TimeSpan time;
        string Time;


        //All data is saved to text file by default, unitl user disables it.
        bool saveOutputLog = true;
        bool saveMeasurements = true;
        bool saveResults = true;
        string OutputLogFile = "Output Log.txt";
        string TestFolderPath = "";

        double minProgramVoltage = 0.05; //50mV minimum programming voltage for power supply
        double minProgramCurrent = 0.005; //5mA minimum programming current for DC Load
        int VoltageResolution = 3;//volateg resolution is 3 decimal places
        int CurrentResolution = 4;//current resolution is 4 decimal places

        //Initial values for power supply
        double startVoltage = 0;
        double stopVoltage = 0;
        double incrementVoltage = 0;
        double currentLimit = 0;

        //Initial values for DC Load
        double startCurrent = 0;
        double stopCurrent = 0;
        double incrementCurrent = 0;
        double setVoltage = 0;

        //Default values for Test Config.
        int MeasSamples = 20;
        int Max_MeasSamples = 50000; //Software will only allow 50000 measurement to be taken at a time.
        double offsetEfficiency = 0; //Usually set to 0.

        string testName = ""; //Test Name is empty.

        int total_SourceInput_Voltage_Values;
        int max_total_SourceInput_Voltage_Values = 50; //Software will only allow user to perform 50 tests at a time
        int total_SinkOutput_Current_Values;
        int total_Measurements;
        int total_Tests;

        bool SingleSweep = false;
        bool Actual_Test = false;
        bool Cancel_Test = false;


        //Various list, data is stored in them
        List<string> Measurements = new List<string>(); //All Source and Sink Measurements

        List<double> Source_Measure_Voltage = new List<double>();
        List<double> Source_Measure_Current = new List<double>();
        List<double> Sink_Measure_Current = new List<double>();
        List<double> Sink_Measure_Voltage = new List<double>();

        List<double> Test_Efficiency = new List<double>();

        List<double> Sink_Voltage = new List<double>();

        List<double> Sink_Current = new List<double>();

        List<double> Source_Voltage = new List<double>();

        List<double> Source_Current = new List<double>();

        List<double> Output_Power = new List<double>();

        List<double> Input_Power = new List<double>();

        List<double> Test_power_Loss = new List<double>();

        List<double> CircuitResistance = new List<double>();

        List<double> SourceResistance = new List<double>();

        List<double> SinkResistance = new List<double>();

        int maxDigitsEfficiency = 3;
        double Source_MeasVolt; //Measure Source Voltage
        double Source_MeasCurr; //Measure Source Current
        double Sink_MeasVolt; //Measure Sink Voltage
        double Sink_MeasCurr; //Measure Sink Current

        bool skipStep = false; //Skip Calculating Efficiency if bad sample.
        int invalidSamples; //Invalid Samples

        public MainWindow()
        {
            InitializeComponent();
            saveOutputLog = false;
            Color_Palette(); 
            insert_Log("Welcome " + getUserName() + ", to my DC Power Efficiency Measurement Software.", Config_Code);
            insert_Log("To use this software you will need a power supply and a DC Electronic Load.", Config_Code);
            insert_Log("If you don't have a DC Electronic Load then you may use a 2/4 Quadrant Power Supply that has programmable current sink capability.", Config_Code);
            insert_Log("Click on Config Menu and then click Connect to get started.", Config_Code);
        }

        public string getUserName() 
        {
            try
            {
                string WelcomeName = System.Environment.UserName;
                return (char.ToUpper(WelcomeName[0]) + WelcomeName.Substring(1));
            }
            catch (Exception) 
            {
                
            }
            return "Unknown";
        }

        //Timer, runs every 1 second
        private void timerStart(object sender, RoutedEventArgs e) 
        {
            timer_update = new DispatcherTimer();
            timer_update.Interval = TimeSpan.FromSeconds(1);
            timer_update.Tick += runtime_Update;
            timer_update.Start();
        }

        //updates the runtime timer when tests starts
        private void runtime_Update(object sender, EventArgs e) 
        {
            if (Serial_COM_Data.Connected == true) 
            {
                Connect.IsEnabled = false;
                unlockControls();
                if (SCPI_Commands.customPower_Commands == true) 
                {
                    setPowerSupplyCommands();
                    printPowerSupplyCommands();
                }
                if (SCPI_Commands.customLoad_Commands == true)
                {
                    setLoadCommands();
                    printLoadCommands();
                }
                Serial_COM_Data.Connected = false;
            }

            if (testStart == true) 
            {
                seconds++;
                time = TimeSpan.FromSeconds(seconds);
                Time = time.ToString(@"hh\:mm\:ss");
                timer.Content = Time;
            }

        }

        //Once power supply and DC load are connected, all UI controls are enable
        private void unlockControls() 
        {
            SourceMenu.IsEnabled = true;
            SinkMenu.IsEnabled = true;
            SourceInput.IsEnabled = true;
            SinkOutput.IsEnabled = true;
            Test_Config_Box.IsEnabled = true;
            StartInput.IsEnabled = true;
            SourceMenu.Header = "Source (" + Serial_COM_Data.Source_COM_Port + ")";
            SinkMenu.Header = "Sink (" + Serial_COM_Data.Sink_COM_Port + ")";
            SourceInputModel.Text = Serial_COM_Data.Source_Model;
            SinkInputModel.Text = Serial_COM_Data.Sink_Model;
            this.Title = "Power Efficiency Measurement Software (Source: " + Serial_COM_Data.Source_Model + ")" + " (Sink: " + Serial_COM_Data.Sink_Model + ")";
            saveOutputLog = true;
            insert_Log("[Source] " + "Model: " + Serial_COM_Data.Source_Model + "  Max Voltage: " + Serial_COM_Data.Source_Max_Voltage + "V" + "  Max Current: " + Serial_COM_Data.Source_Max_Current + "A", Success_Code);
            insert_Log("[Sink] " + "Model: " + Serial_COM_Data.Sink_Model + "  Max Voltage: " + Serial_COM_Data.Sink_Max_Voltage + "V" + "  Max Current: " + Serial_COM_Data.Sink_Max_Current + "A", Success_Code);
            insert_Log("Test Data will be saved here: " + Serial_COM_Data.folder_Directory, Config_Code);
            insert_Log("You may open graphs by going to the Graphs menu, then Show Graphs and select the graphs you want.", Config_Code);
            insert_Log("You may open the measurement table by going to Table menu, then click show table.", Config_Code);
            insert_Log("For accurate results, your power supply must operate in constant voltage (CV) mode.", Warning_Code);
            insert_Log("For accurate results, your dc electronic load must operate in constant current (CC) mode.", Warning_Code);
        }

        //Load power supply command, if they were loaded
        private void setPowerSupplyCommands()
        {
            Input_setVolt = SCPI_Commands.Input_setVolt;  
            Input_setCurr = SCPI_Commands.Input_setCurr;
            Input_measVolt = SCPI_Commands.Input_measVolt;
            Input_measCurr = SCPI_Commands.Input_measCurr;
            Input_outputON = SCPI_Commands.Input_outputON;
            Input_outputOFF = SCPI_Commands.Input_outputOFF;
            Input_minChars = SCPI_Commands.Input_minChars;
            Input_maxChars = SCPI_Commands.Input_maxChars;
            Input_ScientificNotation = SCPI_Commands.Input_ScientificNotation;
            insert_Log("Custom Power Supply Commands applied.", Config_Code);
        }

        //Prints the custom power supply command
        private void printPowerSupplyCommands() 
        {
            insert_Log("Set Voltage: " + Input_setVolt, Config_Code);
            insert_Log("Set Current: " + Input_setCurr, Config_Code);
            insert_Log("Measure Voltage: " + Input_measVolt, Config_Code);
            insert_Log("Measure Current: " + Input_measCurr, Config_Code);
            insert_Log("Output On: " + Input_outputON, Config_Code);
            insert_Log("Output Off: " + Input_outputOFF, Config_Code);
            insert_Log("Expected Min Data Length: " + Input_minChars.ToString(), Config_Code);
            insert_Log("Expected Max Data Length: " + Input_maxChars.ToString(), Config_Code);
            insert_Log("Scientific Notation: " + Input_ScientificNotation.ToString(), Config_Code);
            insert_Log("Power Supply Commands Loaded.", Success_Code);
        }

        //loads dc load commands, if they were loaded
        private void setLoadCommands() 
        {
            Output_setVolt = SCPI_Commands.Output_setVolt;
            Output_setCurr = SCPI_Commands.Output_setCurr;
            Output_measVolt = SCPI_Commands.Output_measVolt;
            Output_measCurr = SCPI_Commands.Output_measCurr;
            Output_outputON = SCPI_Commands.Output_outputON;
            Output_outputOFF = SCPI_Commands.Output_outputOFF;
            Output_minChars = SCPI_Commands.Output_minChars;
            Output_maxChars = SCPI_Commands.Output_maxChars;
            Output_ScientificNotation = SCPI_Commands.Output_ScientificNotation;
            insert_Log("Custom DC Load Commands applied.", Config_Code);
        }

        //prints dc load commands
        private void printLoadCommands()
        {
            insert_Log("Set Voltage: " + Output_setVolt, Config_Code);
            insert_Log("Set Current: " + Output_setCurr, Config_Code);
            insert_Log("Measure Voltage: " + Output_measVolt, Config_Code);
            insert_Log("Measure Current: " + Output_measCurr, Config_Code);
            insert_Log("Output On: " + Output_outputON, Config_Code);
            insert_Log("Output Off: " + Output_outputOFF, Config_Code);
            insert_Log("Expected Min Data Length: " + Output_minChars.ToString(), Config_Code);
            insert_Log("Expected Max Data Length: " + Output_maxChars.ToString(), Config_Code);
            insert_Log("Scientific Notation: " + Output_ScientificNotation.ToString(), Config_Code);
            insert_Log("DC Load Commands Loaded.", Success_Code);
        }

        //Opens serial COM window
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (Serial_COM_Data.WindowOpen == false)
            {
                Select_COM_Window = new Select_COM_Ports();
                if (Select_COM_Window.IsActive == false)
                {
                    Select_COM_Window.Show();
                }
            }
            else 
            {
                insert_Log("COM Select Window is already open.", Warning_Code);
            }
        }

        //30 color palettes, after that random color are assigned
        private void Color_Palette() 
        {
            try
            {
                Colours.Clear();
                Colours = new Queue<string>(new string[] { "220,20,60", "34,139,34", "0,191,255", "255,20,147", "255,165,0", "255,255,0", "128,0,128", "240,230,140", "124,252,0", "233,150,122", "255,0,255", "0,0,205", "255,69,0", "85,107,47", "127,0,0", "72,61,139", "0,139,139", "210,105,30", "154,205,50", "143,188,143", "176,48,96", "0,255,127", "0,255,255", "218,112,214", "176,196,222", "30,144,255", "123,104,238", "105,105,105", "0,0,128", "144,238,144" });
            }
            catch (Exception) 
            {
                insert_Log("Could not initialized/reset color palette.", Error_Code);
            }
        }

        //write commands to power supply
        private void sendSourceCommand_Click(object sender, RoutedEventArgs e)
        {
            SourceWrite(SourceCommand.Text);
            insert_Log("Command send to power supply: " + SourceCommand.Text, Message_Code);
            SourceCommand.Text = string.Empty;
        }

        //Read write commands to power supply
        private void querySourceCommand_Click(object sender, RoutedEventArgs e)
        {
            SourceWriteRead(SourceQuery.Text);
            insert_Log("Command send to power supply: " + SourceQuery.Text, Message_Code);
            SourceQuery.Text = string.Empty;
        }

        //write commands to dc load
        private void sendSinkCommand_Click(object sender, RoutedEventArgs e)
        {
            SinkWrite(SinkCommand.Text);
            insert_Log("Command send to power supply: " + SinkCommand.Text, Message_Code);
            SinkCommand.Text = string.Empty;
        }

        //read write commands to dc load
        private void querySinkCommand_Click(object sender, RoutedEventArgs e)
        {
            SinkWriteRead(SinkQuery.Text);
            insert_Log("Command send to power supply: " + SinkQuery.Text, Message_Code);
            SinkQuery.Text = string.Empty;
        }

        //serial code for dc laod, write and read
        private string SinkWriteRead(string Command) 
        {
            try
            {
                using (var Si = new SerialPort(Serial_COM_Data.Sink_COM_Port, Serial_COM_Data.Sink_BaudRate, (Parity)Serial_COM_Data.Sink_Parity, Serial_COM_Data.Sink_DataBits, (StopBits)Serial_COM_Data.Sink_StopBits))
                {
                    Si.ReadTimeout = Serial_COM_Data.Sink_ReadTimeout;
                    Si.WriteTimeout = Serial_COM_Data.Sink_WriteTimeout;
                    Si.Handshake = (Handshake)Serial_COM_Data.Sink_Handshake;
                    Si.RtsEnable = Serial_COM_Data.Sink_RtsEnable;
                    try
                    {
                        Si.Open();
                    }
                    catch (Exception)
                    {
                        insert_Log("[Sink] COM Port in use or does not exists", Error_Code);
                        return ("Null");
                    }
                    try
                    {
                        Si.WriteLine(Command);
                        string temp = Si.ReadLine();
                        insert_Log("[Sink] Data Recieved: " + temp, Success_Code);
                        Si.Close();
                        return (temp);
                    }
                    catch (Exception)
                    {
                        insert_Log("[Sink]" + " " + "Device not found or cannot read device.", Error_Code);
                        Si.Close();
                        return ("Null");
                    }
                }
            }
            catch
            {
                insert_Log("[Sink]" + " " + "Device not found or cannot read device.", Error_Code);
                return ("Null");
            }
        }

        //write only to dc load, serial code
        private void SinkWrite(string Command) 
        {
            try
            {
                using (var Si = new SerialPort(Serial_COM_Data.Sink_COM_Port, Serial_COM_Data.Sink_BaudRate, (Parity)Serial_COM_Data.Sink_Parity, Serial_COM_Data.Sink_DataBits, (StopBits)Serial_COM_Data.Sink_StopBits))
                {
                    Si.ReadTimeout = Serial_COM_Data.Sink_ReadTimeout;
                    Si.WriteTimeout = Serial_COM_Data.Sink_WriteTimeout;
                    Si.Handshake = (Handshake)Serial_COM_Data.Sink_Handshake;
                    Si.RtsEnable = Serial_COM_Data.Sink_RtsEnable;
                    try
                    {
                        Si.Open();
                    }
                    catch (Exception)
                    {
                        insert_Log("[Sink] COM Port in use or does not exists", Error_Code);
                    }
                    try
                    {
                        Si.WriteLine(Command);
                        Si.Close();
                    }
                    catch (Exception)
                    {
                        insert_Log("[Sink]" + " " + "Device not found or cannot read device.", Error_Code);
                        Si.Close();
                    }
                }
            }
            catch
            {
                insert_Log("[Sink]" + " " + "Device not found or cannot read device.", Error_Code);
            }
        }

        //inserts message to the output log
        private void insert_Log(string Message, int Code) 
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt");
            SolidColorBrush Color = Brushes.Black;
            string Status = "";
            if (Code == Error_Code) //Error Message
            {
                Status = "[Error]";
                Color = Brushes.Red;
            }
            else if (Code == Success_Code) //Success Message
            {
                Status = "[Success]";
                Color = Brushes.Green;
            }
            else if (Code == Warning_Code) //Warning Message
            {
                Status = "[Warning]";
                Color = Brushes.Orange;
            }
            else if (Code == Config_Code) //Config Message
            {
                Status = "";
                Color = Brushes.Blue;
            }
            else if (Code == Message_Code)//Standard Message
            {
                Status = "";
                Color = Brushes.Black;
            }
            else if (Code == Source_message) //Standard Message
            {
                Status = "";
                Color = Brushes.Magenta;
            }
            else if (Code == Sink_message) //Standard Message
            {
                Status = "";
                Color = Brushes.DodgerBlue;
            }
            else if (Code == Average_message) //Standard Message
            {
                Status = "[Result]";
                Color = Brushes.BlueViolet;
            }
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                Output_Log.Inlines.Add(new Run("[" + date + "]" + " " + Status + " " + Message + "\n") { Foreground = Color });
                Output_Log_Scroll.ScrollToBottom();
            }));
            if (saveOutputLog == true) 
            {
                writeToFile("[" + date + "]" + Message, Serial_COM_Data.folder_Directory, OutputLogFile, true);
            }
        }

        //Close the software
        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        //Close the software by pressing the exit menu option
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private string SourceWriteRead(string Command)
        {
            try
            {
                using (var So = new SerialPort(Serial_COM_Data.Source_COM_Port, Serial_COM_Data.Source_BaudRate, (Parity)Serial_COM_Data.Source_Parity, Serial_COM_Data.Source_DataBits, (StopBits)Serial_COM_Data.Source_StopBits))
                {
                    So.ReadTimeout = Serial_COM_Data.Source_ReadTimeout;
                    So.WriteTimeout = Serial_COM_Data.Source_WriteTimeout;
                    So.Handshake = (Handshake)Serial_COM_Data.Source_Handshake;
                    So.RtsEnable = Serial_COM_Data.Source_RtsEnable;
                    try
                    {
                        So.Open();
                    }
                    catch (Exception)
                    {
                        insert_Log("[Source] COM Port in use or does not exists", Error_Code);
                        return ("Null");
                    }
                    try
                    {
                        So.WriteLine(Command);
                        string temp = So.ReadLine();
                        insert_Log("[Source] Data Recieved: " + temp, Success_Code);
                        So.Close();
                        return (temp);
                    }
                    catch (Exception)
                    {
                        insert_Log("[Source]" + " " + "Device not found or cannot read device.", Error_Code);
                        So.Close();
                        return ("Null");
                    }
                }
            }
            catch
            {
                insert_Log("[Source]" + " " + "Device not found or cannot read device.", Error_Code);
                return ("Null");
            }
        }

        private void SourceWrite(string Command)
        {
            try
            {
                using (var So = new SerialPort(Serial_COM_Data.Source_COM_Port, Serial_COM_Data.Source_BaudRate, (Parity)Serial_COM_Data.Source_Parity, Serial_COM_Data.Source_DataBits, (StopBits)Serial_COM_Data.Source_StopBits))
                {
                    So.ReadTimeout = Serial_COM_Data.Source_ReadTimeout;
                    So.WriteTimeout = Serial_COM_Data.Source_WriteTimeout;
                    So.Handshake = (Handshake)Serial_COM_Data.Source_Handshake;
                    So.RtsEnable = Serial_COM_Data.Source_RtsEnable;
                    try
                    {
                        So.Open();
                    }
                    catch (Exception)
                    {
                        insert_Log("[Source] COM Port in use or does not exists", Error_Code);
                    }
                    try
                    {
                        So.WriteLine(Command);
                        So.Close();
                    }
                    catch (Exception)
                    {
                        insert_Log("[Source]" + " " + "Device not found or cannot read device.", Error_Code);
                        So.Close();
                    }
                }
            }
            catch
            {
                insert_Log("[Source]" + " " + "Device not found or cannot read device.", Error_Code);
            }
        }

        private void Source_BorderColor(string HexValue)
        {
            SolidColorBrush Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HexValue));
            Source_Border.BorderBrush = Color;
        }

        private void Source_PanelColor(string HexValue) 
        {
            SolidColorBrush Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HexValue));
            Source_GroupBox.Foreground = Color;
            So_text1.Foreground = Color;
            So_text2.Foreground = Color;
            So_text3.Foreground = Color;
            So_text4.Foreground = Color;
            So_text5.Foreground = Color;
            So_text6.Foreground = Color;
            So_text7.Foreground = Color;
            So_text8.Foreground = Color;
            So_Volt_Val.Foreground = Color;
            So_Curr_Val.Foreground = Color;
            So_SetVolt_Val.Foreground = Color;
            So_SetCurr_Val.Foreground = Color;
            So_Power_Val.Foreground = Color;
        }

        private void Sink_BorderColor(string HexValue)
        {
            SolidColorBrush Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HexValue));
            Sink_Border.BorderBrush = Color;
        }

        private void Sink_PanelColor(string HexValue)
        {
            SolidColorBrush Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HexValue));
            Sink_GroupBox.Foreground = Color;
            Si_text1.Foreground = Color;
            Si_text2.Foreground = Color;
            Si_text3.Foreground = Color;
            Si_text4.Foreground = Color;
            Si_text5.Foreground = Color;
            Si_text6.Foreground = Color;
            Si_text7.Foreground = Color;
            Si_text8.Foreground = Color;
            Si_Volt_Val.Foreground = Color;
            Si_Curr_Val.Foreground = Color;
            Si_SetVolt_Val.Foreground = Color;
            Si_SetCurr_Val.Foreground = Color;
            Si_Power_Val.Foreground = Color;
        }

        private void SoBC_Green_Click(object sender, RoutedEventArgs e)
        {
            Source_BorderColor("#FF00FF17");
            SoBC_Green.IsChecked = true;
            SoBC_Blue.IsChecked = false;
            SoBC_Red.IsChecked = false;
            SoBC_Yellow.IsChecked = false;
            SoBC_Orange.IsChecked = false;
            SoBC_Pink.IsChecked = false;
            SoBC_White.IsChecked = false;
            SoBC_Black.IsChecked = false;
        }

        private void SoBC_Blue_Click(object sender, RoutedEventArgs e)
        {
            Source_BorderColor("#FF00C0FF");
            SoBC_Green.IsChecked = false;
            SoBC_Blue.IsChecked = true;
            SoBC_Red.IsChecked = false;
            SoBC_Yellow.IsChecked = false;
            SoBC_Orange.IsChecked = false;
            SoBC_Pink.IsChecked = false;
            SoBC_White.IsChecked = false;
            SoBC_Black.IsChecked = false;
        }

        private void SoBC_Red_Click(object sender, RoutedEventArgs e)
        {
            Source_BorderColor("Red");
            SoBC_Green.IsChecked = false;
            SoBC_Blue.IsChecked = false;
            SoBC_Red.IsChecked = true;
            SoBC_Yellow.IsChecked = false;
            SoBC_Orange.IsChecked = false;
            SoBC_Pink.IsChecked = false;
            SoBC_White.IsChecked = false;
            SoBC_Black.IsChecked = false;
        }

        private void SoBC_Yellow_Click(object sender, RoutedEventArgs e)
        {
            Source_BorderColor("#FFFFFF00");
            SoBC_Green.IsChecked = false;
            SoBC_Blue.IsChecked = false;
            SoBC_Red.IsChecked = false;
            SoBC_Yellow.IsChecked = true;
            SoBC_Orange.IsChecked = false;
            SoBC_Pink.IsChecked = false;
            SoBC_White.IsChecked = false;
            SoBC_Black.IsChecked = false;
        }

        private void SoBC_Orange_Click(object sender, RoutedEventArgs e)
        {
            Source_BorderColor("DarkOrange");
            SoBC_Green.IsChecked = false;
            SoBC_Blue.IsChecked = false;
            SoBC_Red.IsChecked = false;
            SoBC_Yellow.IsChecked = false;
            SoBC_Orange.IsChecked = true;
            SoBC_Pink.IsChecked = false;
            SoBC_White.IsChecked = false;
            SoBC_Black.IsChecked = false;
        }

        private void SoBC_Pink_Click(object sender, RoutedEventArgs e)
        {
            Source_BorderColor("DeepPink");
            SoBC_Green.IsChecked = false;
            SoBC_Blue.IsChecked = false;
            SoBC_Red.IsChecked = false;
            SoBC_Yellow.IsChecked = false;
            SoBC_Orange.IsChecked = false;
            SoBC_Pink.IsChecked = true;
            SoBC_White.IsChecked = false;
            SoBC_Black.IsChecked = false;
        }

        private void SoBC_White_Click(object sender, RoutedEventArgs e)
        {
            Source_BorderColor("White");
            SoBC_Green.IsChecked = false;
            SoBC_Blue.IsChecked = false;
            SoBC_Red.IsChecked = false;
            SoBC_Yellow.IsChecked = false;
            SoBC_Orange.IsChecked = false;
            SoBC_Pink.IsChecked = false;
            SoBC_White.IsChecked = true;
            SoBC_Black.IsChecked = false;
        }

        private void SoBC_Black_Click(object sender, RoutedEventArgs e)
        {
            Source_BorderColor("Black");
            SoBC_Green.IsChecked = false;
            SoBC_Blue.IsChecked = false;
            SoBC_Red.IsChecked = false;
            SoBC_Yellow.IsChecked = false;
            SoBC_Orange.IsChecked = false;
            SoBC_Pink.IsChecked = false;
            SoBC_White.IsChecked = false;
            SoBC_Black.IsChecked = true;
        }

        private void So_Green_Click(object sender, RoutedEventArgs e)
        {
            Source_PanelColor("#FF00FF17");
            So_Green.IsChecked = true;
            So_Blue.IsChecked = false;
            So_Red.IsChecked = false;
            So_Yellow.IsChecked = false;
            So_Orange.IsChecked = false;
            So_Pink.IsChecked = false;
            So_White.IsChecked = false;
        }

        private void So_Blue_Click(object sender, RoutedEventArgs e)
        {
            Source_PanelColor("#FF00C0FF");
            So_Green.IsChecked = false;
            So_Blue.IsChecked = true;
            So_Red.IsChecked = false;
            So_Yellow.IsChecked = false;
            So_Orange.IsChecked = false;
            So_Pink.IsChecked = false;
            So_White.IsChecked = false;
        }

        private void So_Red_Click(object sender, RoutedEventArgs e)
        {
            Source_PanelColor("Red");
            So_Green.IsChecked = false;
            So_Blue.IsChecked = false;
            So_Red.IsChecked = true;
            So_Yellow.IsChecked = false;
            So_Orange.IsChecked = false;
            So_Pink.IsChecked = false;
            So_White.IsChecked = false;
        }

        private void So_Yellow_Click(object sender, RoutedEventArgs e)
        {
            Source_PanelColor("#FFFFFF00");
            So_Green.IsChecked = false;
            So_Blue.IsChecked = false;
            So_Red.IsChecked = false;
            So_Yellow.IsChecked = true;
            So_Orange.IsChecked = false;
            So_Pink.IsChecked = false;
            So_White.IsChecked = false;
        }

        private void So_Orange_Click(object sender, RoutedEventArgs e)
        {
            Source_PanelColor("DarkOrange");
            So_Green.IsChecked = false;
            So_Blue.IsChecked = false;
            So_Red.IsChecked = false;
            So_Yellow.IsChecked = false;
            So_Orange.IsChecked = true;
            So_Pink.IsChecked = false;
            So_White.IsChecked = false;
        }

        private void So_Pink_Click(object sender, RoutedEventArgs e)
        {
            Source_PanelColor("DeepPink");
            So_Green.IsChecked = false;
            So_Blue.IsChecked = false;
            So_Red.IsChecked = false;
            So_Yellow.IsChecked = false;
            So_Orange.IsChecked = false;
            So_Pink.IsChecked = true;
            So_White.IsChecked = false;
        }

        private void So_White_Click(object sender, RoutedEventArgs e)
        {
            Source_PanelColor("White");
            So_Green.IsChecked = false;
            So_Blue.IsChecked = false;
            So_Red.IsChecked = false;
            So_Yellow.IsChecked = false;
            So_Orange.IsChecked = false;
            So_Pink.IsChecked = false;
            So_White.IsChecked = true;
        }

        private void SiBC_Green_Click(object sender, RoutedEventArgs e)
        {
            Sink_BorderColor("#FF00FF17");
            SiBC_Green.IsChecked = true;
            SiBC_Blue.IsChecked = false;
            SiBC_Red.IsChecked = false;
            SiBC_Yellow.IsChecked = false;
            SiBC_Orange.IsChecked = false;
            SiBC_Pink.IsChecked = false;
            SiBC_White.IsChecked = false;
            SiBC_Black.IsChecked = false;
        }

        private void SiBC_Blue_Click(object sender, RoutedEventArgs e)
        {
            Sink_BorderColor("#FF00C0FF");
            SiBC_Green.IsChecked = false;
            SiBC_Blue.IsChecked = true;
            SiBC_Red.IsChecked = false;
            SiBC_Yellow.IsChecked = false;
            SiBC_Orange.IsChecked = false;
            SiBC_Pink.IsChecked = false;
            SiBC_White.IsChecked = false;
            SiBC_Black.IsChecked = false;
        }

        private void SiBC_Red_Click(object sender, RoutedEventArgs e)
        {
            Sink_BorderColor("Red");
            SiBC_Green.IsChecked = false;
            SiBC_Blue.IsChecked = false;
            SiBC_Red.IsChecked = true;
            SiBC_Yellow.IsChecked = false;
            SiBC_Orange.IsChecked = false;
            SiBC_Pink.IsChecked = false;
            SiBC_White.IsChecked = false;
            SiBC_Black.IsChecked = false;
        }

        private void SiBC_Yellow_Click(object sender, RoutedEventArgs e)
        {
            Sink_BorderColor("#FFFFFF00");
            SiBC_Green.IsChecked = false;
            SiBC_Blue.IsChecked = false;
            SiBC_Red.IsChecked = false;
            SiBC_Yellow.IsChecked = true;
            SiBC_Orange.IsChecked = false;
            SiBC_Pink.IsChecked = false;
            SiBC_White.IsChecked = false;
            SiBC_Black.IsChecked = false;
        }

        private void SiBC_Orange_Click(object sender, RoutedEventArgs e)
        {
            Sink_BorderColor("DarkOrange");
            SiBC_Green.IsChecked = false;
            SiBC_Blue.IsChecked = false;
            SiBC_Red.IsChecked = false;
            SiBC_Yellow.IsChecked = false;
            SiBC_Orange.IsChecked = true;
            SiBC_Pink.IsChecked = false;
            SiBC_White.IsChecked = false;
            SiBC_Black.IsChecked = false;
        }

        private void SiBC_Pink_Click(object sender, RoutedEventArgs e)
        {
            Sink_BorderColor("DeepPink");
            SiBC_Green.IsChecked = false;
            SiBC_Blue.IsChecked = false;
            SiBC_Red.IsChecked = false;
            SiBC_Yellow.IsChecked = false;
            SiBC_Orange.IsChecked = false;
            SiBC_Pink.IsChecked = true;
            SiBC_White.IsChecked = false;
            SiBC_Black.IsChecked = false;
        }

        private void SiBC_White_Click(object sender, RoutedEventArgs e)
        {
            Sink_BorderColor("White");
            SiBC_Green.IsChecked = false;
            SiBC_Blue.IsChecked = false;
            SiBC_Red.IsChecked = false;
            SiBC_Yellow.IsChecked = false;
            SiBC_Orange.IsChecked = false;
            SiBC_Pink.IsChecked = false;
            SiBC_White.IsChecked = true;
            SiBC_Black.IsChecked = false;
        }

        private void SiBC_Black_Click(object sender, RoutedEventArgs e)
        {
            Sink_BorderColor("Black");
            SiBC_Green.IsChecked = false;
            SiBC_Blue.IsChecked = false;
            SiBC_Red.IsChecked = false;
            SiBC_Yellow.IsChecked = false;
            SiBC_Orange.IsChecked = false;
            SiBC_Pink.IsChecked = false;
            SiBC_White.IsChecked = false;
            SiBC_Black.IsChecked = true;
        }

        private void Si_Green_Click(object sender, RoutedEventArgs e)
        {
            Sink_PanelColor("#FF00FF17");
            Si_Green.IsChecked = true;
            Si_Blue.IsChecked = false;
            Si_Red.IsChecked = false;
            Si_Yellow.IsChecked = false;
            Si_Orange.IsChecked = false;
            Si_Pink.IsChecked = false;
            Si_White.IsChecked = false;
        }

        private void Si_Blue_Click(object sender, RoutedEventArgs e)
        {
            Sink_PanelColor("#FF00C0FF");
            Si_Green.IsChecked = false;
            Si_Blue.IsChecked = true;
            Si_Red.IsChecked = false;
            Si_Yellow.IsChecked = false;
            Si_Orange.IsChecked = false;
            Si_Pink.IsChecked = false;
            Si_White.IsChecked = false;
        }

        private void Si_Red_Click(object sender, RoutedEventArgs e)
        {
            Sink_PanelColor("Red");
            Si_Green.IsChecked = false;
            Si_Blue.IsChecked = false;
            Si_Red.IsChecked = true;
            Si_Yellow.IsChecked = false;
            Si_Orange.IsChecked = false;
            Si_Pink.IsChecked = false;
            Si_White.IsChecked = false;
        }

        private void Si_Yellow_Click(object sender, RoutedEventArgs e)
        {
            Sink_PanelColor("#FFFFFF00");
            Si_Green.IsChecked = false;
            Si_Blue.IsChecked = false;
            Si_Red.IsChecked = false;
            Si_Yellow.IsChecked = true;
            Si_Orange.IsChecked = false;
            Si_Pink.IsChecked = false;
            Si_White.IsChecked = false;
        }

        private void Si_Orange_Click(object sender, RoutedEventArgs e)
        {
            Sink_PanelColor("DarkOrange");
            Si_Green.IsChecked = false;
            Si_Blue.IsChecked = false;
            Si_Red.IsChecked = false;
            Si_Yellow.IsChecked = false;
            Si_Orange.IsChecked = true;
            Si_Pink.IsChecked = false;
            Si_White.IsChecked = false;
        }

        private void Si_Pink_Click(object sender, RoutedEventArgs e)
        {
            Sink_PanelColor("DeepPink");
            Si_Green.IsChecked = false;
            Si_Blue.IsChecked = false;
            Si_Red.IsChecked = false;
            Si_Yellow.IsChecked = false;
            Si_Orange.IsChecked = false;
            Si_Pink.IsChecked = true;
            Si_White.IsChecked = false;
        }

        private void Si_White_Click(object sender, RoutedEventArgs e)
        {
            Sink_PanelColor("White");
            Si_Green.IsChecked = false;
            Si_Blue.IsChecked = false;
            Si_Red.IsChecked = false;
            Si_Yellow.IsChecked = false;
            Si_Orange.IsChecked = false;
            Si_Pink.IsChecked = false;
            Si_White.IsChecked = true;
        }

        private void Supported_Devices_Click(object sender, RoutedEventArgs e)
        {
            insert_Log("Supported Source Power Supplies: Any power supply with serial interface that allows you to set voltage, current and measure voltage, current.", Message_Code);
            insert_Log("Supported DC Electronic Load and 2/4 Quadrant Power Supplies: Any Device with serial interface that allows programmable sink current, set sink current and measure voltage, current.", Message_Code);
        }

        private void Credits_Click(object sender, RoutedEventArgs e)
        {
            insert_Log("Created by Niravk Patel.", Message_Code);
            insert_Log("Need help/support? Email: nirav360n@gmail.com", Message_Code);
        }

        private void folderOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", Serial_COM_Data.folder_Directory);
            }
            catch (Exception)
            {
                insert_Log("Cannot open test files directory.", Error_Code);
            }
        }

        private void writeToFile(string data, string filePath, string fileName, bool append)
        {
            try
            {
                using (TextWriter datatotxt = new StreamWriter(filePath + @"\" + fileName, append))
                {
                    datatotxt.WriteLine(data.Trim());
                }
            }
            catch (Exception) 
            {
                insert_Log("Cannot write to text file.", Error_Code);
            }
        }

        private bool folderCreation(string folderPath)
        {
            try
            {
                Directory.CreateDirectory(folderPath);
                TestFolderPath = folderPath;
                return (true);
            }
            catch (Exception)
            {
                insert_Log("Cannot create test data folder. Choose another file directory.", Error_Code);
                return (false);
            }
        }

        private void SaveOutputLog_Click(object sender, RoutedEventArgs e)
        {
            saveOutputLog = !saveOutputLog;
        }

        private void ClearOutputLog_Click(object sender, RoutedEventArgs e)
        {
            Output_Log.Text = String.Empty;
            Output_Log.Inlines.Clear();
        }

        private void ClearInputs_Click(object sender, RoutedEventArgs e)
        {
            StartV.Text = string.Empty;
            StopV.Text = string.Empty;
            IncV.Text = string.Empty;
            CurrLimit.Text = string.Empty;
            StartC.Text = string.Empty;
            StopC.Text = string.Empty;
            IncC.Text = string.Empty;
            SetV.Text = "0";
            offsetNum.Text = "0";
            insert_Log("All input fields reset to default state.", Message_Code);
        }

        private void VerifyInputs_Click(object sender, RoutedEventArgs e)
        {
            Actual_Test = false;
            if (verifyConfig() == 0) 
            {
                insert_Log("All Input fields are valid. Starting Verification Testing.", Success_Code);
                if (mockTest() == 0)
                {
                    insert_Log("Verification Test Successful. Press Start when you are ready.", Success_Code);
                }
                else 
                {
                    insert_Log("Verification Test Failed. Please check your Test Settings.", Error_Code);
                }
            }
        }

        private int verifyConfig() 
        {
            Tests_status(2);
            if (SingleSweep == false)
            {
                return (verifyConfig_Sweep());
            }
            else
            {
                return (verifyConfig_NoSweep());
            }
        }

        private int verifyConfig_Sweep() 
        {
            if (checkStartVoltage() == 1)
            {
                return (1);
            }
            else if (checkStopVoltage() == 1)
            {
                return (1);
            }
            else if (checkIncrementVoltage() == 1)
            {
                return (1);
            }
            else if (checkCurrentLimit() == 1)
            {
                return (1);
            }
            else if (checkStartCurrent() == 1)
            {
                return (1);
            }
            else if (checkStopCurrent() == 1)
            {
                return (1);
            }
            else if (checkIncrementCurrent() == 1)
            {
                return (1);
            }
            else if (checkSetVoltage() == 1)
            {
                return (1);
            }
            else if (checkMeasSamples() == 1)
            {
                return (1);
            }
            else if (checkOffsetValue() == 1)
            {
                return (1);
            }
            else if (checkTestName() == 1)
            {
                return (1);
            }
            else
            {
                return (0);
            }
        }

        private int verifyConfig_NoSweep() 
        {
            if (checkStartVoltage() == 1)
            {
                return (1);
            }
            else if (checkCurrentLimit() == 1)
            {
                return (1);
            }
            else if (checkStartCurrent() == 1)
            {
                return (1);
            }
            else if (checkStopCurrent() == 1)
            {
                return (1);
            }
            else if (checkIncrementCurrent() == 1)
            {
                return (1);
            }
            else if (checkSetVoltage() == 1)
            {
                return (1);
            }
            else if (checkMeasSamples() == 1)
            {
                return (1);
            }
            else if (checkOffsetValue() == 1)
            {
                return (1);
            }
            else if (checkTestName() == 1)
            {
                return (1);
            }
            else
            {
                return (0);
            }
        }

        private int isPositiveNumber(string Inputtext) 
        {
            try
            {
                if (double.Parse(Inputtext, System.Globalization.NumberStyles.Float) < 0)
                {
                    return (1);
                }
                else 
                {
                    return (0);
                }
            }
            catch (Exception) 
            {
                return (1);
            }
        }

        private double convertTodouble(string number_text) 
        {
            return (double.Parse(number_text, System.Globalization.NumberStyles.Float));
        }

        private int checkStartVoltage() 
        {
            if (isPositiveNumber(StartV.Text) == 1)
            {
                insert_Log("[Source] Start Voltage must be a positive number.", Error_Code);
                return (1);
            } 
            else if (convertTodouble(StartV.Text) > Serial_COM_Data.Source_Max_Voltage) 
            {
                insert_Log("[Source] Start Voltage must be less than " + Serial_COM_Data.Source_Max_Voltage + "V", Error_Code);
                return (1);
            }
            else
            {
                startVoltage = Math.Round(convertTodouble(StartV.Text), VoltageResolution);
                StartV.Text = startVoltage.ToString();
                return (0);
            }
        }

        private int checkStopVoltage()
        {
            if (isPositiveNumber(StopV.Text) == 1)
            {
                insert_Log("[Source] Stop Voltage must be a positive number.", Error_Code);
                return (1);
            }
            else if (convertTodouble(StopV.Text) > Serial_COM_Data.Source_Max_Voltage)
            {
                insert_Log("[Source] Stop Voltage must be less than " + Serial_COM_Data.Source_Max_Voltage + "V", Error_Code);
                return (1);
            }
            else if (convertTodouble(StopV.Text) < startVoltage)
            {
                insert_Log("[Source] Stop Voltage must be greater than Start Voltage", Error_Code);
                return (1);
            }
            else
            {
                stopVoltage = Math.Round(convertTodouble(StopV.Text), VoltageResolution);
                StopV.Text = stopVoltage.ToString();
                return (0);
            }
        }

        private int checkIncrementVoltage()
        {
            if (isPositiveNumber(IncV.Text) == 1)
            {
                insert_Log("[Source] Increment Voltage must be a positive number.", Error_Code);
                return (1);
            }
            else if (convertTodouble(IncV.Text) > Serial_COM_Data.Source_Max_Voltage)
            {
                insert_Log("[Source] Increment Voltage must be less than " + Serial_COM_Data.Source_Max_Voltage + "V", Error_Code);
                return (1);
            }
            else if (convertTodouble(IncV.Text) < minProgramVoltage)
            {
                insert_Log("[Source] Increment Voltage must be greater than or equal to " + minProgramVoltage.ToString() + "V", Error_Code);
                return (1);
            }
            else if (convertTodouble(IncV.Text) > Math.Round((stopVoltage - startVoltage), VoltageResolution))
            {
                insert_Log("[Source] Increment Voltage must be less than or equal to " + Math.Round((stopVoltage - startVoltage), VoltageResolution).ToString() + "V", Error_Code);
                insert_Log("[Source] Start and Stop Voltage values may be too close to each other.", Warning_Code);
                return (1);
            }
            else
            {
                incrementVoltage = Math.Round(convertTodouble(IncV.Text), VoltageResolution);
                IncV.Text = incrementVoltage.ToString();
                return (0);
            }
        }

        private int checkCurrentLimit()
        {
            if (isPositiveNumber(CurrLimit.Text) == 1)
            {
                insert_Log("[Source] Current Limit must be a positive number.", Error_Code);
                return (1);
            }
            else if (convertTodouble(CurrLimit.Text) > Serial_COM_Data.Source_Max_Current)
            {
                insert_Log("[Source] Current Limit must be less than or equal to  " + Serial_COM_Data.Source_Max_Current.ToString() + "A", Error_Code);
                return (1);
            }
            else
            {
                currentLimit = Math.Round(convertTodouble(CurrLimit.Text), CurrentResolution);
                CurrLimit.Text = currentLimit.ToString();
                return (0);
            }
        }

        private int checkStartCurrent()
        {
            if (isPositiveNumber(StartC.Text) == 1)
            {
                insert_Log("[Sink] Start Current must be a positive number.", Error_Code);
                return (1);
            }
            else if (convertTodouble(StartC.Text) > Serial_COM_Data.Sink_Max_Current) 
            {
                insert_Log("[Sink] Start Current must be a less than " + Serial_COM_Data.Sink_Max_Current + "A", Error_Code);
                return (1);
            }
            else
            {
                startCurrent = Math.Round(convertTodouble(StartC.Text), CurrentResolution);
                StartC.Text = startCurrent.ToString();
                return (0);
            }
        }

        private int checkStopCurrent()
        {
            if (isPositiveNumber(StopC.Text) == 1)
            {
                insert_Log("[Sink] Stop Current must be a positive number.", Error_Code);
                return (1);
            }
            else if (convertTodouble(StopC.Text) > Serial_COM_Data.Sink_Max_Current)
            {
                insert_Log("[Sink] Stop Current must be a less than " + Serial_COM_Data.Sink_Max_Current + "A", Error_Code);
                return (1);
            }
            else if (convertTodouble(StopC.Text) < startCurrent)
            {
                insert_Log("[Sink] Stop Current must be a greater than Start Current", Error_Code);
                return (1);
            }
            else
            {
                stopCurrent = Math.Round(convertTodouble(StopC.Text), CurrentResolution);
                StopC.Text = stopCurrent.ToString();
                return (0);
            }
        }

        private int checkIncrementCurrent()
        {
            if (isPositiveNumber(IncC.Text) == 1)
            {
                insert_Log("[Sink] Increment Current must be a positive number.", Error_Code);
                return (1);
            }
            else if (convertTodouble(IncC.Text) > Serial_COM_Data.Sink_Max_Current)
            {
                insert_Log("[Sink] Increment Current must be a less than " + Serial_COM_Data.Sink_Max_Current + "A", Error_Code);
                return (1);
            }
            else if (convertTodouble(IncC.Text) < minProgramCurrent)
            {
                insert_Log("[Sink] Increment Current must be a greater than " + minProgramCurrent.ToString() + "A", Error_Code);
                return (1);
            }
            else if (convertTodouble(IncC.Text) > Math.Round((stopCurrent - startCurrent),CurrentResolution))
            {
                insert_Log("[Sink] Increment Current must be less than or equal to " + Math.Round((stopCurrent - startCurrent), CurrentResolution).ToString() + "A", Error_Code);
                insert_Log("[Sink] Start and Stop Current values may be too close to each other", Warning_Code);
                return (1);
            }
            else
            {
                incrementCurrent = Math.Round(convertTodouble(IncC.Text), CurrentResolution);
                IncC.Text = incrementCurrent.ToString();
                return (0);
            }
        }

        private int checkSetVoltage()
        {
            if (isPositiveNumber(SetV.Text) == 1)
            {
                insert_Log("[Sink] Set Voltage must be a positive number.", Error_Code);
                return (1);
            }
            else if (convertTodouble(SetV.Text) > Serial_COM_Data.Sink_Max_Voltage) 
            {
                insert_Log("[Sink] Set Voltage must be less than " + Serial_COM_Data.Sink_Max_Voltage, Error_Code);
                return (1);
            }
            else
            {
                setVoltage = Math.Round(convertTodouble(SetV.Text), VoltageResolution);
                SetV.Text = setVoltage.ToString();
                if (setVoltage > 0) 
                {
                    insert_Log("[Sink] Set Voltage is usually set to 0V.", Warning_Code);
                    insert_Log("[Sink] May cause damage to Device Under Test (DUT) if not set to 0V.", Warning_Code);
                }
                return (0);
            }
        }

        private int checkMeasSamples()
        {
            try 
            {
                MeasSamples = int.Parse(sampleNum.Text, System.Globalization.NumberStyles.Float);
                if (MeasSamples <= 0)
                {
                    insert_Log("Measurement Samples per Test Value must be a positive integer.", Error_Code);
                    return (1);
                }
                else 
                {
                    return (0);
                }
            }
            catch (Exception)
            {
                insert_Log("Measurement Samples per Test Value must be a positive integer.", Error_Code);
                return (1);
            }
        }

        private int checkOffsetValue()
        {
            try
            {
                offsetEfficiency = double.Parse(offsetNum.Text, System.Globalization.NumberStyles.Float);
                offsetEfficiency = Math.Round(offsetEfficiency, 2);
                offsetNum.Text = offsetEfficiency.ToString();
                if (offsetEfficiency != 0) 
                {
                    insert_Log("Offset Efficiency Value is usually set to 0", Warning_Code);
                }
                return (0);
            }
            catch (Exception)
            {
                insert_Log("Offset Efficiency Value must a number.", Error_Code);
                return (1);
            }
        }

        private int checkTestName() 
        {
            Regex check = new Regex(@"^[a-zA-Z0-9\s,]*$");
            if (check.IsMatch(test_Name.Text) == false)
            {
                insert_Log("Test Name must be alphanumeric. Must only contain letters and numbers.", Error_Code);
                return (1);
            }
            else 
            {
                testName = test_Name.Text;
                return (0);
            }
        }

        private void NoSweep_Unchecked(object sender, RoutedEventArgs e)
        {
            SingleSweep = false;
            stopVoltage = 0;
            incrementVoltage = 0;
            StopV.Text = string.Empty;
            IncV.Text = string.Empty;
            StopVLabel_1.IsEnabled = true;
            StopVLabel_2.IsEnabled = true;
            StopV.IsEnabled = true;
            IncV_Label_1.IsEnabled = true;
            IncV_Label_2.IsEnabled = true;
            IncV.IsEnabled = true;
        }

        private void NoSweep_Checked(object sender, RoutedEventArgs e)
        {
            SingleSweep = true;
            stopVoltage = 0;
            incrementVoltage = 0;
            StopV.Text = string.Empty;
            IncV.Text = string.Empty;
            StopVLabel_1.IsEnabled = false;
            StopVLabel_2.IsEnabled = false;
            StopV.IsEnabled = false;
            IncV_Label_1.IsEnabled = false;
            IncV_Label_2.IsEnabled = false;
            IncV.IsEnabled = false;

        }

        private int mockTest() 
        {
            total_SourceInput_Voltage_Values = 0;
            total_SinkOutput_Current_Values = 0;
            total_Measurements = 0;
            total_Tests = 0;
            if (SingleSweep == true) 
            {
                stopVoltage = startVoltage + 0.1;
            }
            for (double i = startVoltage; i <= stopVoltage; i = Math.Round((i + incrementVoltage), VoltageResolution))
            {
                //Set Source Voltage
                if (Actual_Test == false) { 
                    insert_Log("[Source] Set Voltage: " + i.ToString() + "V", Message_Code); 
                }
                total_SourceInput_Voltage_Values++;
                for (double j = startCurrent; j <= stopCurrent; j = Math.Round((j + incrementCurrent), CurrentResolution))
                {
                    //Set Sink Current
                    total_SinkOutput_Current_Values++;
                    if (Actual_Test == false)
                    {
                        insert_Log("[Sink] Set Current: " + j.ToString() + "A", Message_Code);
                    }
                    for (int k = 0; k <= MeasSamples; k++)
                    {
                        total_Measurements++;
                    }
                    if (total_Measurements > Max_MeasSamples)
                    {
                        break;
                    }
                }
                if ((total_Measurements > Max_MeasSamples) || (total_SourceInput_Voltage_Values > max_total_SourceInput_Voltage_Values))
                {
                    break;
                }
                total_Tests++;
                if (Actual_Test == false)
                {
                    insert_Log("Tests Completed: " + total_Tests.ToString(), Message_Code);
                }
                if (SingleSweep == true) 
                {
                    break;
                }
            }
            if ((total_Measurements > Max_MeasSamples) || (total_SourceInput_Voltage_Values > max_total_SourceInput_Voltage_Values))
            {
                if ((total_Measurements > Max_MeasSamples)) 
                {
                    insert_Log("Verification Failed. Your Voltage and/or Current Sweep Range might be too large.", Error_Code);
                    insert_Log("Check Measurement Samples per Test Value, it might also be too large.", Error_Code);
                    insert_Log("Maximum allowed Total Measurement Samples of " + Max_MeasSamples.ToString() + " has been reached.", Error_Code);
                }
                if ((total_SourceInput_Voltage_Values > max_total_SourceInput_Voltage_Values))
                {
                    insert_Log("Verification Failed. Your Voltage Sweep Range is too large", Error_Code);
                    insert_Log("Maximum allowed Total Source Input Voltage Values of " + max_total_SourceInput_Voltage_Values.ToString() + " has been reached.", Error_Code);
                }
                insert_Log("Total Source Input Voltage Values before fail: " + total_SourceInput_Voltage_Values.ToString(), Warning_Code);
                insert_Log("Total Sink Output Current Values before fail: " + total_SinkOutput_Current_Values.ToString(), Warning_Code);
                insert_Log("Total Measurement Samples Taken before fail: " + total_Measurements.ToString(), Warning_Code);
                insert_Log("Total Tests before fail: " + total_Tests.ToString(), Warning_Code);
                return (1);
            }
            else 
            {
                if (Actual_Test == false)
                {
                    insert_Log("Total Source Input Voltage Values: " + total_SourceInput_Voltage_Values.ToString(), Config_Code);
                    insert_Log("Total Sink Output Current Values: " + total_SinkOutput_Current_Values.ToString(), Config_Code);
                    insert_Log("Total Measurements: " + (total_Measurements * 4).ToString(), Config_Code);
                    insert_Log("Total Measurement Samples Taken: " + total_Measurements.ToString(), Config_Code);
                    insert_Log("Total Tests: " + total_Tests.ToString(), Config_Code);
                }
                Total_Tests();
                Completed_Tests(0);
                return (0);
            }
        }

        private void EfficiencyTest() 
        {
            bool folderCreated = folderCreation(Serial_COM_Data.folder_Directory + @"\" + DateTime.Now.ToString("yyyy-MM-dd h.mm.ss tt") + "-" + testName);
            int testCompleted = 0;
            invalidSamples = 0;
            Tests_status(0);
            using (var Source = new SerialPort(Serial_COM_Data.Source_COM_Port, Serial_COM_Data.Source_BaudRate, (Parity)Serial_COM_Data.Source_Parity, Serial_COM_Data.Source_DataBits, (StopBits)Serial_COM_Data.Source_StopBits))
            {
                Source.ReadTimeout = Serial_COM_Data.Source_ReadTimeout;
                Source.WriteTimeout = Serial_COM_Data.Source_WriteTimeout;
                Source.Handshake = (Handshake)Serial_COM_Data.Source_Handshake;
                Source.RtsEnable = Serial_COM_Data.Source_RtsEnable;
                Source.Open();
                using (var Sink = new SerialPort(Serial_COM_Data.Sink_COM_Port, Serial_COM_Data.Sink_BaudRate, (Parity)Serial_COM_Data.Sink_Parity, Serial_COM_Data.Sink_DataBits, (StopBits)Serial_COM_Data.Sink_StopBits))
                {
                    Sink.ReadTimeout = Serial_COM_Data.Sink_ReadTimeout;
                    Sink.WriteTimeout = Serial_COM_Data.Sink_WriteTimeout;
                    Sink.Handshake = (Handshake)Serial_COM_Data.Sink_Handshake;
                    Sink.RtsEnable = Serial_COM_Data.Sink_RtsEnable;
                    Sink.Open();

                    testStart = true;
                    Cancel_Test = false;
                    Source.WriteLine(Input_setVolt + " " + "0");
                    Source.WriteLine(Input_setCurr + " " + currentLimit.ToString());
                    set_Source_Current_Display(currentLimit);
                    Source.WriteLine(Input_outputON);

                    if (allowSetVoltage == true)
                    {
                        Sink.WriteLine(Output_setVolt + " " + setVoltage.ToString());
                        set_Sink_Voltage_Display(setVoltage);
                    }
                    Sink.WriteLine(Output_setCurr + " " + "0");
                    Sink.WriteLine(Output_outputON);
                    if (SingleSweep == true) 
                    {
                        stopVoltage = (startVoltage + 0.1);
                    }
                    for (double i = startVoltage; i <= stopVoltage; i = Math.Round((i + incrementVoltage), VoltageResolution))
                    {
                        //Set Source Voltage
                        Source.WriteLine(Input_setVolt + " " + i);
                        insert_Log("[Source] Set Input Voltage: " + i.ToString() + "V", Source_message);
                        set_Source_Voltage_Display(i);
                        Measurements.Add("Input Voltage (V), Input Current (A), Output Voltage (V), Output Current (A), Power Efficiency (%)");
                        for (double j = startCurrent; j <= stopCurrent; j = Math.Round((j + incrementCurrent), CurrentResolution))
                        {
                            //Set Sink Current
                            Sink.WriteLine(Output_setCurr + " " + j.ToString());
                            Measurements.Add("[Sink] Set Current: " + j.ToString() + "A");
                            insert_Log("[Sink] Set Output Load Current: " + j.ToString() + "A", Sink_message);
                            set_Sink_Current_Display(j);
                            for (int k = 0; k < MeasSamples; k++)
                            {
                                try
                                {
                                    //Take Measurements
                                    Source.WriteLine(Input_measVolt);
                                    Source_MeasVolt = Math.Abs(Input_measureVoltage(Source.ReadLine()));

                                    Source.WriteLine(Input_measCurr);
                                    Source_MeasCurr = Math.Abs(Input_measureCurrent(Source.ReadLine()));

                                    Sink.WriteLine(Output_measVolt);
                                    Sink_MeasVolt = Math.Abs(Output_measureVoltage(Sink.ReadLine()));

                                    Sink.WriteLine(Output_measCurr);
                                    Sink_MeasCurr = Math.Abs(Output_measureCurrent(Sink.ReadLine()));
                                }
                                catch (Exception)
                                {
                                    skipStep = true;
                                    insert_Log("Cannot take voltage and/or current reading from power supply.", Warning_Code);
                                    insert_Log("Nothing to worry about. If problem still persists then check Serial Connection.", Warning_Code);
                                }

                                if (skipStep == false)
                                {
                                    Source_Measure_Voltage.Add(Source_MeasVolt);
                                    Source_Measure_Current.Add(Source_MeasCurr);
                                    Sink_Measure_Voltage.Add(Sink_MeasVolt);
                                    Sink_Measure_Current.Add(Sink_MeasCurr);
                                    Measure_VC_Display();
                                }
                                else 
                                {
                                    invalidSamples++;
                                    skipStep = false;
                                }
                                if (Cancel_Test == true) 
                                {
                                    insert_Log("Tests Canceled.", Warning_Code);
                                    break;
                                }
                            }
                            Test_Measurements(i, j);
                            if (Cancel_Test == true)
                            {
                                insert_Log("Tests Canceled.", Warning_Code);
                                break;
                            }
                        }
                        Output_Collected_Data(i);
                        if ((folderCreated == true) & (saveMeasurements == true))
                        {
                            saveMeasurements_toFile(i.ToString());
                        }
                        if ((folderCreated == true) & (saveResults == true))
                        {
                            saveFinalResults_toFile(i.ToString());
                        }
                        Clear_Output_Collected_Data();
                        Measurements.Clear();
                        testCompleted++;
                        Completed_Tests(testCompleted);
                        if (SingleSweep == true)
                        {
                            stopVoltage = 0;
                            break;
                        }
                        if (Cancel_Test == true)
                        {
                            insert_Log("Tests Canceled.", Warning_Code);
                            break;
                        }
                    }
                    Source.WriteLine(Input_setVolt + " " + "0");
                    Source.WriteLine(Input_setCurr + " " + "0");
                    Sink.WriteLine(Output_setVolt + " " + "0");
                    Sink.WriteLine(Output_setCurr + " " + "0");
                    Source.WriteLine(Input_outputOFF);
                    Sink.WriteLine(Output_outputOFF);
                    Source.Close();
                    Sink.Close();
                }
            }
            if (invalidSamples > 0) 
            {
                insert_Log("Invalid Samples Recieved: " + invalidSamples.ToString(), Warning_Code);
                insert_Log("Nothing to worry about. Read the User Manual.", Warning_Code);
            }
            Tests_status(1);
            Access_Control(true);
            Display_Reset();
            testStart = false;
            Cancel_Test = false;
            seconds = 0;
        }

        private void Test_Measurements(double setInputVoltage, double setOutputCurrent) 
        {
            int Size = Source_Measure_Voltage.Count();

            for (int i = 0; i < Size; i++) 
            {
                Measurements.Add(Source_Measure_Voltage[i].ToString() + "," + Source_Measure_Current[i].ToString() + "," + Sink_Measure_Voltage[i].ToString() + "," + Sink_Measure_Current[i].ToString());
            }

            double So_Voltage = Math.Round((Source_Measure_Voltage.Average()), VoltageResolution);
            double So_Current = Math.Round((Source_Measure_Current.Average()), CurrentResolution);
            double Si_Voltage = Math.Round((Sink_Measure_Voltage.Average()), VoltageResolution);
            double Si_Current = Math.Round((Sink_Measure_Current.Average()), CurrentResolution);

            double Sink_Power = (Si_Voltage * Si_Current);
            double Source_Power = (So_Voltage * So_Current);

            double input_resistance = Math.Round(((So_Voltage / So_Current)), maxDigitsEfficiency);
            double output_resistance = Math.Round(((Si_Voltage / Si_Current)), maxDigitsEfficiency);
            double circuit_resistnace = Math.Round(((input_resistance - output_resistance)), maxDigitsEfficiency);

            double Test_Efficiency_Sample = Math.Round(((((Sink_Power) / (Source_Power)) * 100) + (offsetEfficiency)), maxDigitsEfficiency);
            Test_Efficiency.Add(Test_Efficiency_Sample);

            double power_loss = Math.Round((100 - Test_Efficiency_Sample), maxDigitsEfficiency);
            Test_power_Loss.Add(power_loss);

            double Input_Power_Sample = Math.Round(Source_Power, maxDigitsEfficiency);
            Input_Power.Add(Input_Power_Sample);

            double Output_Power_Sample = Math.Round(Sink_Power, maxDigitsEfficiency);
            Output_Power.Add(Output_Power_Sample);

            
            Sink_Voltage.Add(Si_Voltage);

            Sink_Current.Add(Si_Current);

            Source_Voltage.Add(So_Voltage);

            Source_Current.Add(So_Current);

            CircuitResistance.Add(circuit_resistnace);

            SourceResistance.Add(input_resistance);

            SinkResistance.Add(output_resistance);

            Source_Measure_Voltage.Clear();
            Source_Measure_Current.Clear();
            Sink_Measure_Voltage.Clear();
            Sink_Measure_Current.Clear();
            insert_Log(" Input Voltage: "
                           + So_Voltage.ToString()
                           + "V, Input Current: "
                           + So_Current.ToString()
                           + "A, Output Voltage: "
                           + Si_Voltage.ToString()
                           + "V, Output Load Current: "
                           + Si_Current.ToString()
                           + "A, Input Power: "
                           + Input_Power_Sample.ToString()
                           + "W, Output Power: "
                           + Output_Power_Sample.ToString()
                           + "W, Power Efficiency: "
                           + Test_Efficiency_Sample.ToString()
                           + "%, Power Loss: "
                           + power_loss.ToString()
                           + "%, Input Resistance: "
                           + input_resistance.ToString()
                           + "Ω, Output Resistance: "
                           + output_resistance
                           + "Ω, Circuit Resistance: "
                           + circuit_resistnace + "Ω", Average_message);
            if (AddTest_Table == true & DataTable != null)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    DataTable.addTest(testName, So_Voltage, So_Current, Si_Voltage, Si_Current, Input_Power_Sample, Output_Power_Sample, Test_Efficiency_Sample, power_loss, input_resistance, output_resistance, circuit_resistnace);
                }));
            }
        }

        private void Output_Collected_Data(double setInputVoltage)
        {
            if (AddTest_Graph == true)
            {
                string averageSinkVolt = Math.Round(Sink_Voltage.Average(), VoltageResolution).ToString();
                string averageSourceVolt = Math.Round(Source_Voltage.Average(), VoltageResolution).ToString();
                string Colour = "0,0,0";
                if (Colours.Count() < 1)
                {
                    Colour = randomNum.Next(0, 255).ToString() + "," + randomNum.Next(0, 255).ToString() + "," + randomNum.Next(0, 255).ToString();
                    insert_Log("Ran out of predefined Color Palettes, random colors will be chosen.", Warning_Code);
                }
                else
                {
                    Colour = Colours.Dequeue();
                }
                try
                {
                    if (PowerEfficiency_OutputCurrent != null)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            PowerEfficiency_OutputCurrent.insertGraph(("Vi: " + averageSourceVolt + "V, Vo: " + averageSinkVolt + "V, " + testName), Sink_Current, Test_Efficiency, Colour);
                            PowerEfficiency_OutputCurrent.insert_Log(("Test Name: " + testName + " [Input] Source Voltage: " + averageSourceVolt + "V, [Output] Sink Voltage: " + averageSinkVolt + "V"), Colour);
                        }));
                    }
                }
                catch (Exception)
                {
                    insert_Log("Could not add test data to Power Efficiency vs Output Current Graph", Error_Code);
                }
                try
                {
                    if (PowerEfficiency_OutputPower != null)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            PowerEfficiency_OutputPower.insertGraph(("Vi: " + averageSourceVolt + "V, Vo: " + averageSinkVolt + "V, " + testName), Output_Power, Test_Efficiency, Colour);
                            PowerEfficiency_OutputPower.insert_Log(("Test Name: " + testName + " [Input] Source Voltage: " + averageSourceVolt + "V, [Output] Sink Voltage: " + averageSinkVolt + "V"), Colour);
                        }));
                    }
                }
                catch (Exception)
                {
                    insert_Log("Could not add test data to Power Efficiency vs Output Power Graph", Error_Code);
                }
                try
                {
                    if (OutputPower_InputPower != null)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            OutputPower_InputPower.insertGraph(("Vi: " + averageSourceVolt + "V, Vo: " + averageSinkVolt + "V, " + testName), Input_Power, Output_Power, Colour);
                            OutputPower_InputPower.insert_Log(("Test Name: " + testName + " [Input] Source Voltage: " + averageSourceVolt + "V, [Output] Sink Voltage: " + averageSinkVolt + "V"), Colour);
                        }));
                    }
                }
                catch (Exception)
                {
                    insert_Log("Could not add test data to Input Power vs Output Power Graph", Error_Code);
                }
                try
                {
                    if (InputCurrent_InputVoltage != null)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            InputCurrent_InputVoltage.insertGraph(("Vi: " + averageSourceVolt + "V, Vo: " + averageSinkVolt + "V, " + testName), Source_Voltage, Source_Current, Colour);
                            InputCurrent_InputVoltage.insert_Log(("Test Name: " + testName + " [Input] Source Voltage: " + averageSourceVolt + "V, [Output] Sink Voltage: " + averageSinkVolt + "V"), Colour);
                        }));
                    }
                }
                catch (Exception)
                {
                    insert_Log("Could not add test data to Input Current vs Input Voltage Graph", Error_Code);
                }
                try
                {
                    if (OutputCurrent_OutputVoltage != null)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            OutputCurrent_OutputVoltage.insertGraph(("Vi: " + averageSourceVolt + "V, Vo: " + averageSinkVolt + "V, " + testName), Sink_Voltage, Sink_Current, Colour);
                            OutputCurrent_OutputVoltage.insert_Log(("Test Name: " + testName + " [Input] Source Voltage: " + averageSourceVolt + "V, [Output] Sink Voltage: " + averageSinkVolt + "V"), Colour);
                        }));
                    }
                }
                catch (Exception)
                {
                    insert_Log("Could not add test data to Output Load Current vs Output Voltage Graph", Error_Code);
                }
                try
                {
                    if (InputVoltage_InputCurrent != null)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            InputVoltage_InputCurrent.insertGraph(("Vi: " + averageSourceVolt + "V, Vo: " + averageSinkVolt + "V, " + testName), Source_Current, Source_Voltage, Colour);
                            InputVoltage_InputCurrent.insert_Log(("Test Name: " + testName + " [Input] Source Voltage: " + averageSourceVolt + "V, [Output] Sink Voltage: " + averageSinkVolt + "V"), Colour);
                        }));
                    }
                }
                catch (Exception)
                {
                    insert_Log("Could not add test data to Input Voltage vs Input Current Graph", Error_Code);
                }
                try
                {
                    if (OutputVoltage_OutputCurrent != null)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            OutputVoltage_OutputCurrent.insertGraph(("Vi: " + averageSourceVolt + "V, Vo: " + averageSinkVolt + "V, " + testName), Sink_Current, Sink_Voltage, Colour);
                            OutputVoltage_OutputCurrent.insert_Log(("Test Name: " + testName + " [Input] Source Voltage: " + averageSourceVolt + "V, [Output] Sink Voltage: " + averageSinkVolt + "V"), Colour);
                        }));
                    }
                }
                catch (Exception)
                {
                    insert_Log("Could not add test data to Input Voltage vs Input Current Graph", Error_Code);
                }
                try
                {
                    if (PowerLoss_OutputCurrent != null)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            PowerLoss_OutputCurrent.insertGraph(("Vi: " + averageSourceVolt + "V, Vo: " + averageSinkVolt + "V, " + testName), Sink_Current, Test_power_Loss, Colour);
                            PowerLoss_OutputCurrent.insert_Log(("Test Name: " + testName + " [Input] Source Voltage: " + averageSourceVolt + "V, [Output] Sink Voltage: " + averageSinkVolt + "V"), Colour);
                        }));
                    }
                }
                catch (Exception)
                {
                    insert_Log("Could not add test data to Power Loss vs Output Load Current Graph", Error_Code);
                }
                try
                {
                    if (PowerLoss_OutputPower != null)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                        {
                            PowerLoss_OutputPower.insertGraph(("Vi: " + averageSourceVolt + "V, Vo: " + averageSinkVolt + "V, " + testName), Output_Power, Test_power_Loss, Colour);
                            PowerLoss_OutputPower.insert_Log(("Test Name: " + testName + " [Input] Source Voltage: " + averageSourceVolt + "V, [Output] Sink Voltage: " + averageSinkVolt + "V"), Colour);
                        }));
                    }
                }
                catch (Exception)
                {
                    insert_Log("Could not add test data to Power Loss vs Output Load Power Graph", Error_Code);
                }
            }
        }

        private void Clear_Output_Collected_Data() 
        {
            Source_Voltage.Clear();
            Source_Current.Clear();
            Sink_Voltage.Clear();
            Sink_Current.Clear();
            Test_Efficiency.Clear();
            Input_Power.Clear();
            Output_Power.Clear();
            Test_power_Loss.Clear();
            CircuitResistance.Clear();
            SourceResistance.Clear();
            SinkResistance.Clear();
        }

        private void saveFinalResults_toFile(string inputVoltage)
        {
            string textName = TestFolderPath + @"\" + "[Final Results][Source] Input Voltage " + inputVoltage + "V" + ".txt";
            try
            {
                using (TextWriter datatotxt = new StreamWriter(textName, true))
                {
                    int Size = Sink_Current.Count();
                    datatotxt.WriteLine("Input Voltage (V), Input Current (A), Output Voltage (V), Output Load Current (A), Input Power (W), Output Load Power (W), Power Efficiency (%), Power Loss (%), Input Resistance(Ω), Output Resistance (Ω), Circuit Resistance (Ω)");
                    for (int i = 0; i < Size; i++)
                    {
                        datatotxt.WriteLine(Source_Voltage[i].ToString() + "," + Source_Current[i].ToString() + "," + Sink_Voltage[i].ToString() + "," + Sink_Current[i].ToString() + "," + Input_Power[i].ToString() + "," + Output_Power[i].ToString() + "," + Test_Efficiency[i].ToString() + "," + Test_power_Loss[i].ToString() + "," + SourceResistance[i].ToString() + "," + SinkResistance[i].ToString() + "," + CircuitResistance[i].ToString());
                    }
                }
                insert_Log("Saved Final Results to File: " + textName, Success_Code);
            }
            catch (Exception)
            {
                insert_Log("Failed to save Final Results to text file. Check File Directory.", Error_Code);
            }
            Source_Voltage.Clear();
            Source_Current.Clear();
            Sink_Voltage.Clear();
            Sink_Current.Clear();
            Test_Efficiency.Clear();
            Input_Power.Clear();
            Output_Power.Clear();
            Test_power_Loss.Clear();
            CircuitResistance.Clear();
            SourceResistance.Clear();
            SinkResistance.Clear();
        }

        private void saveMeasurements_toFile(string inputVoltage) 
        {
            string textName = TestFolderPath + @"\" + "[Source] Input Voltage " + inputVoltage + "V" + ".txt";
            try
            {
                using (TextWriter datatotxt = new StreamWriter(textName, true))
                {
                    foreach (string measurement in Measurements)
                    {
                        datatotxt.WriteLine(measurement);
                    }
                }
                insert_Log("Saved Measurements to File: " + textName, Success_Code);
            }
            catch (Exception) 
            {
                insert_Log("Failed to save measurements to text file. Check File Directory.", Error_Code);
            }
        }

        private void set_Source_Voltage_Display(double voltage) 
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                So_SetVolt_Val.Content = voltage.ToString();
            }));
        }

        private void set_Source_Current_Display(double current) 
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                So_SetCurr_Val.Content = current.ToString();
            }));
        }

        private void set_Sink_Voltage_Display(double voltage)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                Si_SetVolt_Val.Content = voltage.ToString();
            }));
        }

        private void set_Sink_Current_Display(double current)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                Si_SetCurr_Val.Content = current.ToString();
            }));
        }

        private void Measure_VC_Display() 
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                So_Volt_Val.Content = Source_MeasVolt.ToString();
                So_Curr_Val.Content = Source_MeasCurr.ToString();
                So_Power_Val.Content = (Math.Round((Source_MeasVolt*Source_MeasCurr), VoltageResolution)).ToString();

                Si_Volt_Val.Content = Sink_MeasVolt.ToString();
                Si_Curr_Val.Content = Sink_MeasCurr.ToString();
                Si_Power_Val.Content = (Math.Round((Sink_MeasVolt * Sink_MeasCurr), VoltageResolution)).ToString();
            }));
        }

        private void Display_Reset() 
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                So_Volt_Val.Content = "0";
                So_Curr_Val.Content = "0";
                So_Power_Val.Content = "0";
                So_SetVolt_Val.Content = "0";
                So_SetCurr_Val.Content = "0";

                Si_Volt_Val.Content = "0";
                Si_Curr_Val.Content = "0";
                Si_Power_Val.Content = "0";
                Si_SetVolt_Val.Content = "0";
                Si_SetCurr_Val.Content = "0";
            }));
        }

        private void Tests_status(int Status) 
        {
            if (Status == 0)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    status.Content = "Running";
                }));
            }
            else if (Status == 1)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    status.Content = "Completed";
                }));
            }
            else if (Status == 2) 
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    status.Content = "Not Running";
                }));
            }
        }

        private void Completed_Tests(int Number) 
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                testNum.Content = Number.ToString();
                testProgress.Value = Number;
            }));
        }

        private void Total_Tests() 
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                testTotal.Content = total_Tests.ToString();
                testProgress.Maximum = total_Tests;
            }));
        }

        private double Input_measureVoltage(string temp) 
        {
            if (Input_ScientificNotation == 1)
            {
                try
                {
                    if ((temp.Length >= Input_minChars) && (temp.Length <= Input_maxChars) && (temp.Contains("E")) && (temp.Contains(".")))
                    {
                        return (Math.Round(double.Parse(temp, System.Globalization.NumberStyles.Float), VoltageResolution));
                    }
                    else
                    {
                        skipStep = true;
                        return (0);
                    }
                }
                catch (Exception)
                {
                    skipStep = true;
                    insert_Log("Cannot measure Input Voltage. Check Serial Port Connection or bad command.", Error_Code);
                    return (0);
                }
            }
            else if (Input_ScientificNotation == 0)
            {
                try
                {
                    if ((temp.Length >= Input_minChars) && (temp.Length <= Input_maxChars))
                    {
                        return (Math.Round(double.Parse(temp, System.Globalization.NumberStyles.Float), VoltageResolution));
                    }
                    else
                    {
                        skipStep = true;
                        return (0);
                    }
                }
                catch (Exception)
                {
                    skipStep = true;
                    insert_Log("Cannot measure Input Voltage. Check Serial Port Connection or bad command.", Error_Code);
                    return (0);
                }
            }
            else 
            {
                skipStep = true;
                insert_Log("Cannot measure Input Voltage. Check Serial Port Connection or bad command.", Error_Code);
                return (0);
            }
        }

        private double Output_measureVoltage(string temp)
        {
            if (Output_ScientificNotation == 1)
            {
                try
                {
                    if ((temp.Length >= Output_minChars) && (temp.Length <= Output_maxChars) && (temp.Contains("E")) && (temp.Contains(".")))
                    {
                        return (Math.Round(double.Parse(temp, System.Globalization.NumberStyles.Float), VoltageResolution));
                    }
                    else
                    {
                        skipStep = true;
                        return (0);
                    }
                }
                catch (Exception)
                {
                    skipStep = true;
                    insert_Log("Cannot measure Output Voltage. Check Serial Port Connection or bad command.", Error_Code);
                    return (0);
                }
            }
            else if (Output_ScientificNotation == 0)
            {
                try
                {
                    if ((temp.Length >= Output_minChars) && (temp.Length <= Output_maxChars))
                    {
                        return (Math.Round(double.Parse(temp, System.Globalization.NumberStyles.Float), VoltageResolution));
                    }
                    else
                    {
                        skipStep = true;
                        return (0);
                    }
                }
                catch (Exception)
                {
                    skipStep = true;
                    insert_Log("Cannot measure Output Voltage. Check Serial Port Connection or bad command.", Error_Code);
                    return (0);
                }
            }
            else 
            {
                skipStep = true;
                insert_Log("Cannot measure Output Voltage. Check Serial Port Connection or bad command.", Error_Code);
                return (0);
            }
        }

        private double Input_measureCurrent(string temp)
        {
            if (Input_ScientificNotation == 1)
            {
                try
                {
                    if ((temp.Length >= Input_minChars) && (temp.Length <= Input_maxChars) && (temp.Contains("E")) && (temp.Contains(".")))
                    {
                        return (Math.Round(double.Parse(temp, System.Globalization.NumberStyles.Float), CurrentResolution));
                    }
                    else
                    {
                        skipStep = true;
                        return (0);
                    }
                }
                catch (Exception)
                {
                    skipStep = true;
                    insert_Log("Cannot measure Input Current. Check Serial Port Connection or bad command.", Error_Code);
                    return (0);
                }
            }
            else if (Input_ScientificNotation == 0)
            {
                try
                {
                    if ((temp.Length >= Input_minChars) && (temp.Length <= Input_maxChars))
                    {
                        return (Math.Round(double.Parse(temp, System.Globalization.NumberStyles.Float), CurrentResolution));
                    }
                    else
                    {
                        skipStep = true;
                        return (0);
                    }
                }
                catch (Exception)
                {
                    skipStep = true;
                    insert_Log("Cannot measure Input Current. Check Serial Port Connection or bad command.", Error_Code);
                    return (0);
                }
            }
            else 
            {
                skipStep = true;
                insert_Log("Cannot measure Input Current. Check Serial Port Connection or bad command.", Error_Code);
                return (0);
            }
        }

        private double Output_measureCurrent(string temp)
        {
            if (Output_ScientificNotation == 1)
            {
                try
                {
                    if ((temp.Length >= Output_minChars) && (temp.Length <= Output_maxChars) && (temp.Contains("E")) && (temp.Contains(".")))
                    {
                        return (Math.Round(double.Parse(temp, System.Globalization.NumberStyles.Float), CurrentResolution));
                    }
                    else
                    {
                        skipStep = true;
                        return (0);
                    }
                }
                catch (Exception)
                {
                    skipStep = true;
                    insert_Log("Cannot measure Output Load Current. Check Serial Port Connection or bad command.", Error_Code);
                    return (0);
                }
            }
            else if (Output_ScientificNotation == 0)
            {
                try
                {
                    if ((temp.Length >= Output_minChars) && (temp.Length <= Output_maxChars))
                    {
                        return (Math.Round(double.Parse(temp, System.Globalization.NumberStyles.Float), CurrentResolution));
                    }
                    else
                    {
                        skipStep = true;
                        return (0);
                    }
                }
                catch (Exception)
                {
                    skipStep = true;
                    insert_Log("Cannot measure Output Load Current. Check Serial Port Connection or bad command.", Error_Code);
                    return (0);
                }
            }
            else 
            {
                skipStep = true;
                insert_Log("Cannot measure Output Load Current. Check Serial Port Connection or bad command.", Error_Code);
                return (0);
            }
        }

        private void Start_Test_Click(object sender, RoutedEventArgs e)
        {
            Actual_Test = true;
            if (verifyConfig() == 0)
            {
                insert_Log("All Input fields are valid. Starting Verification Testing.", Success_Code);
                if (mockTest() == 0)
                {
                    insert_Log("Verification Test Successful. Starting Test.", Success_Code);
                    Access_Control(false);
                    EfficiencyTest();
                }
                else
                {
                    insert_Log("Verification Test Failed. Please check your Test Settings.", Error_Code);
                }
            }
        }

        private void Access_Control(bool Access) 
        {
            if (Access == false)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    SourceMenu.IsEnabled = false;
                    SinkMenu.IsEnabled = false;
                    SourceInput.IsEnabled = false;
                    SinkOutput.IsEnabled = false;
                    Test_Config_Box.IsEnabled = false;
                    StartInput.IsEnabled = false;
                }));
            }
            else if (Access == true)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    SourceMenu.IsEnabled = true;
                    SinkMenu.IsEnabled = true;
                    SourceInput.IsEnabled = true;
                    SinkOutput.IsEnabled = true;
                    Test_Config_Box.IsEnabled = true;
                    StartInput.IsEnabled = true;
                }));
            }
        }

        private void SaveMeasData_Click(object sender, RoutedEventArgs e)
        {
            saveMeasurements = !saveMeasurements;
        }

        private void SaveFinalData_Click(object sender, RoutedEventArgs e)
        {
            saveResults = !saveResults;
        }

        private void PE_OC_Click(object sender, RoutedEventArgs e)
        {
            if (PowerEfficiency_OutputCurrent == null)
            {
                PowerEfficiency_OutputCurrent = new Graph_Window();
                PowerEfficiency_OutputCurrent.Closed += (a, b) => PowerEfficiency_OutputCurrent = null;
                PowerEfficiency_OutputCurrent.setWindowTitle("Power Efficiency (%) vs [Sink] Output Load Current (A)");
                PowerEfficiency_OutputCurrent.setXYLabels("Output Load Current (A)", "Power Efficiency (%)");
                PowerEfficiency_OutputCurrent.setXYUnits("A", "%");
                PowerEfficiency_OutputCurrent.Show();
            }
            else
            {
                PowerEfficiency_OutputCurrent.Show();
                insert_Log("PowerEfficiency vs Output Current Window is already open.", Warning_Code);
            }
        }

        private void PE_OP_Click(object sender, RoutedEventArgs e)
        {
            if (PowerEfficiency_OutputPower == null)
            {
                PowerEfficiency_OutputPower = new Graph_Window();
                PowerEfficiency_OutputPower.Closed += (a, b) => PowerEfficiency_OutputPower = null;
                PowerEfficiency_OutputPower.setWindowTitle("Power Efficiency (%) vs [Sink] Output Load Power (W)");
                PowerEfficiency_OutputPower.setXYLabels("Output Load Power (W)", "Power Efficiency (%)");
                PowerEfficiency_OutputPower.setXYUnits("W", "%");
                PowerEfficiency_OutputPower.Show();
            }
            else
            {
                PowerEfficiency_OutputPower.Show();
                insert_Log("Power Efficiency vs Output Power Window is already open.", Warning_Code);
            }
        }

        private void IP_OP_Click(object sender, RoutedEventArgs e)
        {
            if (OutputPower_InputPower == null)
            {
                OutputPower_InputPower = new Graph_Window();
                OutputPower_InputPower.Closed += (a, b) => OutputPower_InputPower = null;
                OutputPower_InputPower.setWindowTitle("[Sink] Output Load Power (W) vs [Source] Input Power (W)");
                OutputPower_InputPower.setXYLabels("Input Power (W)", "Output Load Power (W)");
                OutputPower_InputPower.setXYUnits("W", "W");
                OutputPower_InputPower.Show();
            }
            else
            {
                OutputPower_InputPower.Show();
                insert_Log("Input Power vs Output Power Window is already open.", Warning_Code);
            }
        }

        private void Cancel_Tests_Click(object sender, RoutedEventArgs e)
        {
            Cancel_Test = true;
        }

        private void Reset_Colors_Click(object sender, RoutedEventArgs e)
        {
            Color_Palette();
            insert_Log("Colour Palette Reset.", Warning_Code);
        }

        private void IC_IV_Click(object sender, RoutedEventArgs e)
        {
            if (InputCurrent_InputVoltage == null)
            {
                InputCurrent_InputVoltage = new Graph_Window();
                InputCurrent_InputVoltage.Closed += (a, b) => InputCurrent_InputVoltage = null;
                InputCurrent_InputVoltage.setWindowTitle("[Source] Input Current (A) vs [Source] Input Voltage (V)");
                InputCurrent_InputVoltage.setXYLabels("Input Voltage (V)", "Input Current (A)");
                InputCurrent_InputVoltage.setXYUnits("V", "A");
                InputCurrent_InputVoltage.Show();
            }
            else
            {
                InputCurrent_InputVoltage.Show();
                insert_Log("Input Current vs Input Voltage Window is already open.", Warning_Code);
            }
        }

        private void OC_OV_Click(object sender, RoutedEventArgs e)
        {
            if (OutputCurrent_OutputVoltage == null)
            {
                OutputCurrent_OutputVoltage = new Graph_Window();
                OutputCurrent_OutputVoltage.Closed += (a, b) => OutputCurrent_OutputVoltage = null;
                OutputCurrent_OutputVoltage.setWindowTitle("[Sink] Output Load Current (A) vs [Sink] Output Voltage (V)");
                OutputCurrent_OutputVoltage.setXYLabels("Output Voltage (V)", "Output Load Current (A)");
                OutputCurrent_OutputVoltage.setXYUnits("V", "A");
                OutputCurrent_OutputVoltage.Show();
            }
            else
            {
                OutputCurrent_OutputVoltage.Show();
                insert_Log("Output Current vs Output Voltage Window is already open.", Warning_Code);
            }
        }

        private void IV_IC_Click(object sender, RoutedEventArgs e)
        {
            if (InputVoltage_InputCurrent == null)
            {
                InputVoltage_InputCurrent = new Graph_Window();
                InputVoltage_InputCurrent.Closed += (a, b) => InputVoltage_InputCurrent = null;
                InputVoltage_InputCurrent.setWindowTitle("[Source] Input Voltage (V) vs [Source] Input Current (A)");
                InputVoltage_InputCurrent.setXYLabels("Input Current (A)", "Input Voltage (V)");
                InputVoltage_InputCurrent.setXYUnits("A", "V");
                InputVoltage_InputCurrent.Show();
            }
            else
            {
                InputVoltage_InputCurrent.Show();
                insert_Log("Input Current vs [Source] Input Voltage Window is already open.", Warning_Code);
            }
        }

        private void OV_OC_Click(object sender, RoutedEventArgs e)
        {
            if (OutputVoltage_OutputCurrent == null)
            {
                OutputVoltage_OutputCurrent = new Graph_Window();
                OutputVoltage_OutputCurrent.Closed += (a, b) => OutputVoltage_OutputCurrent = null;
                OutputVoltage_OutputCurrent.setWindowTitle("[Sink] Output Voltage (V) vs [Sink] Output Load Current (A)");
                OutputVoltage_OutputCurrent.setXYLabels("Output Load Current (A)", "Output Voltage (V)");
                OutputVoltage_OutputCurrent.setXYUnits("A", "V");
                OutputVoltage_OutputCurrent.Show();
            }
            else
            {
                OutputVoltage_OutputCurrent.Show();
                insert_Log("Output Voltage vs Output Current Window is already open.", Warning_Code);
            }
        }

        private void Add_test_graph_Click(object sender, RoutedEventArgs e)
        {
            if (Add_test_graph.IsChecked == true)
            {
                AddTest_Graph = true;
                insert_Log("Tests Data will be added to Graphs.", Message_Code);
            }
            else 
            {
                AddTest_Graph = false;
                insert_Log("Tests Data will not be added to Graphs.", Warning_Code);
            }
        }

        private void showTable_Click(object sender, RoutedEventArgs e)
        {
            if (DataTable == null)
            {
                DataTable = new Table();
                DataTable.Closed += (a, b) => DataTable = null;
                DataTable.Show();
            }
            else
            {
                DataTable.Show();
                insert_Log("Table is already open.", Warning_Code);
            }
        }

        private void addtoTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddTest_Table = !AddTest_Table;
                if (AddTest_Table == true)
                {
                    addtoTable.IsChecked = true;
                }
                else 
                {
                    addtoTable.IsChecked = false;
                }
            }
            catch (Exception) 
            {
                
            }
        }

        private void PL_OC_Click(object sender, RoutedEventArgs e)
        {
            if (PowerLoss_OutputCurrent == null)
            {
                PowerLoss_OutputCurrent = new Graph_Window();
                PowerLoss_OutputCurrent.Closed += (a, b) => PowerLoss_OutputCurrent = null;
                PowerLoss_OutputCurrent.setWindowTitle("Power Loss (%) vs Output Load Current (A)");
                PowerLoss_OutputCurrent.setXYLabels("Output Load Current (A)", "Power Loss (%)");
                PowerLoss_OutputCurrent.setXYUnits("A", "%");
                PowerLoss_OutputCurrent.Show();
            }
            else
            {
                PowerLoss_OutputCurrent.Show();
                insert_Log("Power Loss vs Output Current Window is already open.", Warning_Code);
            }
        }

        private void PL_OP_Click(object sender, RoutedEventArgs e)
        {
            if (PowerLoss_OutputPower == null)
            {
                PowerLoss_OutputPower = new Graph_Window();
                PowerLoss_OutputPower.Closed += (a, b) => PowerLoss_OutputPower = null;
                PowerLoss_OutputPower.setWindowTitle("Power Loss (%) vs Output Load Power (W)");
                PowerLoss_OutputPower.setXYLabels("Output Load Power (W)", "Power Loss (%)");
                PowerLoss_OutputPower.setXYUnits("W", "%");
                PowerLoss_OutputPower.Show();
            }
            else
            {
                PowerLoss_OutputPower.Show();
                insert_Log("Power Loss vs Output Load Power Window is already open.", Warning_Code);
            }
        }

        private void SinkVsetallow_Click(object sender, RoutedEventArgs e)
        {
            if (SinkVsetallow.IsChecked == true)
            {
                allowSetVoltage = true;
                sinkvolttext.IsEnabled = true;
                SetV.IsEnabled = true;
                sinkvoltUnit.IsEnabled = true;
            }
            else 
            {
                allowSetVoltage = false;
                sinkvolttext.IsEnabled = false;
                SetV.IsEnabled = false;
                sinkvoltUnit.IsEnabled = false;
            }
        }
    }
}
