using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Reflection;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using Ookii.Dialogs;

namespace Power_Efficiency_663XB
{
    /// <summary>
    /// Interaction logic for Select_COM_Ports.xaml
    /// </summary>
    public partial class Select_COM_Ports : Window
    {
        int Success_Code = 0;
        int Error_Code = 1;
        int Warning_Code = 2;
        int Config_Code = 3;
        int Message_Code = 4;

        string folder_Directory;

        string So_COM_Port = "";
        int So_BaudRate = 9600;
        int So_Parity = 0;
        int So_StopBits = 1;
        int So_DataBits = 8;
        int So_Handshake = 0;
        int So_WriteTimeout = 3000;
        int So_ReadTimeout = 3000;
        bool So_RtsEnable = false;

        string Si_COM_Port = "";
        int Si_BaudRate = 9600;
        int Si_Parity = 0;
        int Si_StopBits = 1;
        int Si_DataBits = 8;
        int Si_Handshake = 0;
        int Si_WriteTimeout = 3000;
        int Si_ReadTimeout = 3000;
        bool Si_RtsEnable = false;

        string Source_Model;
        double Source_Max_Voltage = -1;
        double Source_Max_Current = -1;

        string Sink_Model;
        double Sink_Max_Voltage = -1;
        double Sink_Max_Current = -1;

        List<string> portList;

        public Select_COM_Ports()
        {
            InitializeComponent();
            if (Thread.CurrentThread.CurrentCulture.Name != "en-US")
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");
            }
            Get_COM_List();
            Serial_COM_Data.WindowOpen = true;
            getSoftwarePath();
            insert_Log("Set your power supply parameters and load the commands file.", Config_Code);
            insert_Log("Set your DC Electronic Load parameters and load the commands file.", Config_Code);
            insert_Log("Click the Connect button when you are ready.", Config_Code);
        }

        private void getSoftwarePath()
        {

            try
            {
                folder_Directory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\" + "Test Data";
                insert_Log("Test Data will be saved inside the software directory.", Config_Code);
                insert_Log(folder_Directory, Config_Code);
                insert_Log("Click the Data Files Directory button to select another directory.", Config_Code);
            }
            catch (Exception)
            {
                insert_Log("Cannot get software directory path. Choose a new directory.", Error_Code);
            }
        }

        private void Get_COM_List()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                var portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
                portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains('(' + n + ')'))).ToList();
                foreach (string p in portList)
                {
                    updateList(p);
                }
            }
        }

        private void updateList(string data)
        {
            ListBoxItem So_itm = new ListBoxItem();
            ListBoxItem Si_itm = new ListBoxItem();
            So_itm.Content = data;
            Si_itm.Content = data;
            Source_List.Items.Add(So_itm);
            Sink_List.Items.Add(Si_itm);
        }

        private void Source_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Source_List.Items.Clear();
            Sink_List.Items.Clear();
            Get_COM_List();
        }

        private void Sink_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Source_List.Items.Clear();
            Sink_List.Items.Clear();
            Get_COM_List();
        }

        private void Select_Dir_Click(object sender, RoutedEventArgs e)
        {
            var Choose_Directory = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (Choose_Directory.ShowDialog() == true)
            {
                folder_Directory = Choose_Directory.SelectedPath + @"\" + "Test Data";
            }
            insert_Log("Test Data will be saved here: " + folder_Directory, Config_Code);
        }

        private void Source_Verify_Click(object sender, RoutedEventArgs e)
        {
            Source_Updater();
            if (check_COM_Match() == 1)
            {
                insert_Log("[Source] Cannot verify. Try again.", Error_Code);
            }
            else
            {
                if (SourceVerify() == 1)
                {
                    insert_Log("[Source] Verify failed. Try again.", Error_Code);
                }
            }
        }

        private int SourceVerify()
        {
            if (verifySourceCOM() == 1) 
            {
                return 1;
            }
            else if (Source_Max_Voltage <= 0)
            {
                insert_Log("Power Supply's max voltage must be greater than 0V.", Error_Code);
                return 1;
            }
            else if (Source_Max_Current <= 0)
            {
                insert_Log("Power Supply's max current must be greater than 0V.", Error_Code);
                return 1;
            }
            else if (SCPI_Commands.Input_ScientificNotation < 0 || SCPI_Commands.Input_ScientificNotation > 1)
            {
                insert_Log("Power Supply's Scientific Notation value must be a 1 (for true) or 0 (for false).", Error_Code);
                return 1;
            }
            else
            {
                if (SCPI_Commands.customPower_Commands == false)
                {
                    insert_Log("Power Supply commands not loaded, default SCPI commands will be used.", Warning_Code);
                }
                insert_Log("Power Supply's parameters are valid.", Success_Code);
                return 0;
            }
        }

        private void Sink_Verify_Click(object sender, RoutedEventArgs e)
        {
            Sink_Updater();
            if (check_COM_Match() == 1)
            {
                insert_Log("[Sink] Cannot verify. Try again.", Error_Code);
            }
            else
            {
                if (SinkVerify() == 1)
                {
                    insert_Log("[Sink] Verify failed. Try again.", Error_Code);
                }
            }
        }

        private int SinkVerify()
        {
            if (verifySinkCOM() == 1)
            {
                return 1;
            }
            else if (Sink_Max_Voltage <= 0)
            {
                insert_Log("DC Load max voltage must be greater than 0V.", Error_Code);
                return 1;
            }
            else if (Sink_Max_Current <= 0)
            {
                insert_Log("DC Load max current must be greater than 0V.", Error_Code);
                return 1;
            }
            else if (SCPI_Commands.Output_ScientificNotation < 0 || SCPI_Commands.Output_ScientificNotation > 1) 
            {
                insert_Log("DC Load's Scientific Notation value must be a 1 (for true) or 0 (for false).", Error_Code);
                return 1;
            }
            else
            {
                if (SCPI_Commands.customLoad_Commands == false)
                {
                    insert_Log("DC Load commands not loaded, default SCPI commands will be used.", Warning_Code);
                }
                insert_Log("DC Load's parameters are valid.", Success_Code);
                return 0;
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            int Source_Good;
            int Sink_Good;

            if (check_COM_Match() == 0)
            {
                Source_Updater();
                Source_Good = SourceVerify();
                if (Source_Good == 1)
                {
                    insert_Log("[Source] Cannot connect. Try Again.", Error_Code);
                }
                else { insert_Log("[Source] Connected.", Success_Code); }

                Sink_Updater();
                Sink_Good = SinkVerify();
                if (Sink_Good == 1)
                {
                    insert_Log("[Sink] Cannot connect. Try Again.", Error_Code);
                }
                else { insert_Log("[Sink] Connected.", Success_Code); }

                if ((Source_Good == 0) & (Sink_Good == 0) & (folderCreation(folder_Directory) == 0)) 
                {
                    DataUpdater();
                    Info_Log.Text = String.Empty;
                    Info_Log.Inlines.Clear();
                    this.Close();
                }
            }
        }

        private void DataUpdater() 
        {
            //Source Device Info
            Serial_COM_Data.Source_Model = Source_Model;
            Serial_COM_Data.Source_Max_Voltage = Source_Max_Voltage;
            Serial_COM_Data.Source_Max_Current = Source_Max_Current;
            Serial_COM_Data.Source_COM_Port = So_COM_Port;
            Serial_COM_Data.Source_BaudRate = So_BaudRate;
            Serial_COM_Data.Source_Parity = So_Parity;
            Serial_COM_Data.Source_StopBits = So_StopBits;
            Serial_COM_Data.Source_DataBits = So_DataBits;
            Serial_COM_Data.Source_Handshake = So_Handshake;
            Serial_COM_Data.Source_WriteTimeout = So_WriteTimeout;
            Serial_COM_Data.Source_ReadTimeout = So_ReadTimeout;
            Serial_COM_Data.Source_RtsEnable = So_RtsEnable;

            //Sink Device Info
            Serial_COM_Data.Sink_Model = Sink_Model;
            Serial_COM_Data.Sink_Max_Voltage = Sink_Max_Voltage;
            Serial_COM_Data.Sink_Max_Current = Sink_Max_Current;
            Serial_COM_Data.Sink_COM_Port = Si_COM_Port;
            Serial_COM_Data.Sink_BaudRate = Si_BaudRate;
            Serial_COM_Data.Sink_Parity = Si_Parity;
            Serial_COM_Data.Sink_StopBits = Si_StopBits;
            Serial_COM_Data.Sink_DataBits = Si_DataBits;
            Serial_COM_Data.Sink_Handshake = Si_Handshake;
            Serial_COM_Data.Sink_WriteTimeout = Si_WriteTimeout;
            Serial_COM_Data.Sink_ReadTimeout = Si_ReadTimeout;
            Serial_COM_Data.Sink_RtsEnable = Si_RtsEnable;

            Serial_COM_Data.folder_Directory = folder_Directory;
            Serial_COM_Data.Connected = true;
    }

        private void Source_List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string temp = Source_List.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
                string COM = temp.Substring(0, temp.IndexOf(" -"));
                Source_COM.Text = COM;
                if (check_COM_Match() == 0)
                {
                    So_COM_Port = COM;
                    check_COM_Match();
                    try
                    {
                        using (var sp = new SerialPort(COM, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One))
                        {
                            sp.WriteTimeout = 500;
                            sp.ReadTimeout = 500;
                            sp.Open();
                            sp.Close();
                            insert_Log("Source: " + COM + " is ready.", Success_Code);
                        }
                    }
                    catch (Exception)
                    {
                        insert_Log("Source: " + COM + " is already in use or does not exists.", Error_Code);
                    }
                }
                else
                {
                    Source_COM.Text = "";
                }
            }
            catch (Exception)
            {
                insert_Log("Select a Valid COM Port.", Warning_Code);
            }
        }

        private int verifySourceCOM() 
        {
            try
            {
                if (check_COM_Match() == 0)
                {
                    So_COM_Port = Source_COM.Text;
                    check_COM_Match();
                    try
                    {
                        using (var sp = new SerialPort(Source_COM.Text, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One))
                        {
                            sp.WriteTimeout = 500;
                            sp.ReadTimeout = 500;
                            sp.Open();
                            sp.Close();
                            insert_Log("Source: " + Source_COM.Text + " is ready.", Success_Code);
                            return 0;
                        }
                    }
                    catch (Exception)
                    {
                        insert_Log("Source: " + Source_COM.Text + " is already in use or does not exists.", Error_Code);
                        return 1;
                    }
                }
                else
                {
                    Source_COM.Text = "";
                }
            }
            catch (Exception)
            {
                insert_Log("Select a Valid COM Port.", Warning_Code);
                return 1;
            }
            return 1;
        }

        private void Sink_List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string temp = Sink_List.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
                string COM = temp.Substring(0, temp.IndexOf(" -"));
                Sink_COM.Text = COM;
                if (check_COM_Match() == 0)
                {
                    Si_COM_Port = COM;
                    check_COM_Match();
                    try
                    {
                        using (var sp = new SerialPort(COM, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One))
                        {
                            sp.WriteTimeout = 500;
                            sp.ReadTimeout = 500;
                            sp.Open();
                            sp.Close();
                            insert_Log("Sink: " + COM + " is ready.", Success_Code);
                        }
                    }
                    catch (Exception)
                    {
                        insert_Log("Sink: " + COM + " is already in use or does not exists.", Error_Code);
                    }
                }
                else
                {
                    Sink_COM.Text = "";
                }
            }
            catch (Exception)
            {
                insert_Log("Select a Valid COM Port.", Warning_Code);
            }
        }

        private int verifySinkCOM() 
        {
            try
            {
                if (check_COM_Match() == 0)
                {
                    Si_COM_Port = Sink_COM.Text;
                    check_COM_Match();
                    try
                    {
                        using (var sp = new SerialPort(Sink_COM.Text, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One))
                        {
                            sp.WriteTimeout = 500;
                            sp.ReadTimeout = 500;
                            sp.Open();
                            sp.Close();
                            insert_Log("Sink: " + Sink_COM.Text + " is ready.", Success_Code);
                            return 0;
                        }
                    }
                    catch (Exception)
                    {
                        insert_Log("Sink: " + Sink_COM.Text + " is already in use  or does not exists.", Error_Code);
                    }
                }
                else
                {
                    Sink_COM.Text = "";
                }
            }
            catch (Exception)
            {
                insert_Log("Select a Valid COM Port.", Warning_Code);
                return 1;
            }
            return 1;
        }

        private int check_COM_Match() 
        {
            string sourceCOM = Source_COM.Text.ToUpper().Trim();
            string sinkCOM = Sink_COM.Text.ToUpper().Trim();
            if (sourceCOM == sinkCOM)
            {
                insert_Log("Source and Sink Power Supplies both cannot have the same COM Port Number.", Warning_Code);
                return (1);
            }
            else 
            {
                return (0);
            }
        }

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
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                Info_Log.Inlines.Add(new Run("[" + date + "]" + " " + Status + " " + Message + "\n") { Foreground = Color });
                Info_Scroll.ScrollToBottom();
            }));
        }

        private void Source_Updater() 
        {
            So_COM_Port = Source_COM.Text.ToUpper().Trim();

            string BaudRate = Source_Bits.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            So_BaudRate = Int32.Parse(BaudRate);

            string DataBits = Source_DataBits.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            So_DataBits = Int32.Parse(DataBits);

            Source_Model = Source_Name.Text;

            bool voltageOK = double.TryParse(Source_maxVolt.Text, out double maxVoltage);
            if (voltageOK == true)
            {
                Source_Max_Voltage = maxVoltage;
            }
            else 
            {
                insert_Log("Power Supply max voltage value is invalid. Please check.", Error_Code);

            }

            bool currentOK = double.TryParse(Source_maxCurr.Text, out double maxCurrent);
            if (currentOK == true)
            {
                Source_Max_Current = maxCurrent;
            }
            else 
            {
                insert_Log("Power Supply max current value is invalid. Please check.", Error_Code);
            }


            try
            {
                So_WriteTimeout = int.Parse(Source_write_timeout.Text.ToUpper().Trim());
            }
            catch (Exception)
            {
                Source_write_timeout.Text = "1000";
                So_WriteTimeout = 1000;
            }

            try
            {
                So_ReadTimeout = int.Parse(Source_read_timeout.Text.ToUpper().Trim());
            }
            catch (Exception)
            {
                Source_read_timeout.Text = "1000";
                So_ReadTimeout = 1000;
            }

            string Parity = Source_Parity.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            switch (Parity)
            {
                case "Even":
                    So_Parity = 2;
                    break;
                case "Odd":
                    So_Parity = 1;
                    break;
                case "None":
                    So_Parity = 0;
                    break;
                case "Mark":
                    So_Parity = 3;
                    break;
                case "Space":
                    So_Parity = 4;
                    break;
                default:
                    So_Parity = 0;
                    break;
            }

            string StopBits = Source_Stop.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            switch (StopBits)
            {
                case "1":
                    So_StopBits = 1;
                    break;
                case "1.5":
                    So_StopBits = 3;
                    break;
                case "2":
                    So_StopBits = 2;
                    break;
                default:
                    So_StopBits = 1;
                    break;
            }

            string Flow = Source_Flow.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            switch (Flow)
            {
                case "Xon/Xoff":
                    So_Handshake = 1;
                    break;
                case "Hardware":
                    So_Handshake = 2;
                    break;
                case "None":
                    So_Handshake = 0;
                    break;
                default:
                    So_Handshake = 1;
                    break;
            }

            string rts = Source_rtsEnable.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            switch (rts)
            {
                case "True":
                    So_RtsEnable = true;
                    break;
                case "False":
                    So_RtsEnable = false;
                    break;
                default:
                    So_RtsEnable = false;
                    break;
            }
        }

        private void Sink_Updater()
        {
            Si_COM_Port = Sink_COM.Text.ToUpper().Trim();

            string BaudRate = Sink_Bits.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            Si_BaudRate = Int32.Parse(BaudRate);

            string DataBits = Sink_DataBits.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last(); ;
            Si_DataBits = Int32.Parse(DataBits);

            Sink_Model = Sink_Name.Text;

            bool voltageOK = double.TryParse(Sink_maxVolt.Text, out double maxVoltage);
            if (voltageOK == true)
            {
                Sink_Max_Voltage = maxVoltage;
            }
            else
            {
                insert_Log("DC Load max voltage value is invalid. Please check.", Error_Code);
            }

            bool currentOK = double.TryParse(Sink_maxCurr.Text, out double maxCurrent);
            if (currentOK == true)
            {
                Sink_Max_Current = maxCurrent;
            }
            else
            {
                insert_Log("DC Load max current value is invalid. Please check.", Error_Code);
            }

            try
            {
                Si_WriteTimeout = int.Parse(Sink_write_timeout.Text.ToUpper().Trim());
            }
            catch (Exception)
            {
                Sink_write_timeout.Text = "1000";
                Si_WriteTimeout = 1000;
            }

            try
            {
                Si_ReadTimeout = int.Parse(Sink_read_timeout.Text.ToUpper().Trim());
            }
            catch (Exception)
            {
                Sink_read_timeout.Text = "1000";
                Si_ReadTimeout = 1000;
            }

            string Parity = Sink_Parity.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            switch (Parity)
            {
                case "Even":
                    Si_Parity = 2;
                    break;
                case "Odd":
                    Si_Parity = 1;
                    break;
                case "None":
                    Si_Parity = 0;
                    break;
                case "Mark":
                    Si_Parity = 3;
                    break;
                case "Space":
                    Si_Parity = 4;
                    break;
                default:
                    Si_Parity = 0;
                    break;
            }

            string StopBits = Sink_Stop.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            switch (StopBits)
            {
                case "1":
                    Si_StopBits = 1;
                    break;
                case "1.5":
                    Si_StopBits = 3;
                    break;
                case "2":
                    Si_StopBits = 2;
                    break;
                default:
                    Si_StopBits = 1;
                    break;
            }

            string Flow = Sink_Flow.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            switch (Flow)
            {
                case "Xon/Xoff":
                    Si_Handshake = 1;
                    break;
                case "Hardware":
                    Si_Handshake = 2;
                    break;
                case "None":
                    Si_Handshake = 0;
                    break;
                default:
                    Si_Handshake = 1;
                    break;
            }

            string rts = Sink_rtsEnable.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            switch (rts)
            {
                case "True":
                    Si_RtsEnable = true;
                    break;
                case "False":
                    Si_RtsEnable = false;
                    break;
                default:
                    So_RtsEnable = false;
                    break;
            }
        }

        private void Info_Clear_Click(object sender, RoutedEventArgs e)
        {
            Info_Log.Text = String.Empty;
            Info_Log.Inlines.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Serial_COM_Data.WindowOpen = false;
        }

        private int folderCreation(string folderPath)
        {
            try
            {
                Directory.CreateDirectory(folderPath);
                return (0);
            }
            catch (Exception)
            {
                insert_Log("Cannot create test data folder. Choose another file directory.", Error_Code);
                return (1);
            }
        }


        private void PowerSupply_commands_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                bool fileExists = openFileDialog.CheckFileExists;
                bool filePathExists = openFileDialog.CheckPathExists;
                string fileExtension = System.IO.Path.GetExtension(openFileDialog.FileName);
                string filePath = openFileDialog.FileName;
                if (fileExists == true & filePathExists == true)
                {
                    if (fileExtension == ".txt")
                    {
                        insertPowerSupplyCommands(filePath);
                    }
                    else
                    {
                        insert_Log("File is invalid. Must be a text file.", Error_Code);
                    }
                }
                else
                {
                    insert_Log("File not found or file path not valid. Try again.", Error_Code);
                }
            }
        }

        private void insertPowerSupplyCommands(string path) 
        {
            try
            {
                using (var readFile = new StreamReader(path))
                {
                    SCPI_Commands.Input_setVolt = readFile.ReadLine().Trim();
                    SCPI_Commands.Input_setCurr = readFile.ReadLine().Trim();
                    SCPI_Commands.Input_measVolt = readFile.ReadLine().Trim();
                    SCPI_Commands.Input_measCurr = readFile.ReadLine().Trim();
                    SCPI_Commands.Input_outputON = readFile.ReadLine().Trim();
                    SCPI_Commands.Input_outputOFF = readFile.ReadLine().Trim();
                    SCPI_Commands.Input_minChars = int.Parse(readFile.ReadLine().Trim());
                    SCPI_Commands.Input_maxChars = int.Parse(readFile.ReadLine().Trim());
                    SCPI_Commands.Input_ScientificNotation = int.Parse(readFile.ReadLine().Trim());
                    SCPI_Commands.customPower_Commands = true;
                    insert_Log("Power Supply Commands File Processed.", Success_Code);
                    printPowerCommands();
                }
            }
            catch (Exception)
            {
                insert_Log("File read failed. Try again.", Error_Code);
                insert_Log("Remember order matters. Refer to user manual.", Error_Code);
                insert_Log("Reverting back to default commands.", Error_Code);
                SCPI_Commands.Input_setVolt = "VOLT";
                SCPI_Commands.Input_setCurr = "CURR";
                SCPI_Commands.Input_measVolt = "MEAS:VOLT?";
                SCPI_Commands.Input_measCurr = "MEAS:CURR?";
                SCPI_Commands.Input_outputON = "OUTPut ON";
                SCPI_Commands.Input_outputOFF = "OUTPut OFF";
                SCPI_Commands.Input_minChars = 10;
                SCPI_Commands.Input_maxChars = 12;
                SCPI_Commands.Input_ScientificNotation = 1;
                SCPI_Commands.customPower_Commands = false;
            }
        }

        private void printPowerCommands()
        {
            insert_Log("Set Voltage: " + SCPI_Commands.Input_setVolt, Config_Code);
            insert_Log("Set Current: " + SCPI_Commands.Input_setCurr, Config_Code);
            insert_Log("Measure Voltage: " + SCPI_Commands.Input_measVolt, Config_Code);
            insert_Log("Measure Current: " + SCPI_Commands.Input_measCurr, Config_Code);
            insert_Log("Output On: " + SCPI_Commands.Input_outputON, Config_Code);
            insert_Log("Output Off: " + SCPI_Commands.Input_outputOFF, Config_Code);
            insert_Log("Expected Min Data Length: " + SCPI_Commands.Input_minChars.ToString(), Config_Code);
            insert_Log("Expected Max Data Length: " + SCPI_Commands.Input_maxChars.ToString(), Config_Code);
            insert_Log("Scientific Notation: " + SCPI_Commands.Input_ScientificNotation.ToString(), Config_Code);
            insert_Log("Power Supply Commands Loaded.", Success_Code);
        }

        private void DCLoad_commands_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                bool fileExists = openFileDialog.CheckFileExists;
                bool filePathExists = openFileDialog.CheckPathExists;
                string fileExtension = System.IO.Path.GetExtension(openFileDialog.FileName);
                string filePath = openFileDialog.FileName;
                if (fileExists == true & filePathExists == true)
                {
                    if (fileExtension == ".txt")
                    {
                        insertDCLoadCommands(filePath);
                    }
                    else
                    {
                        insert_Log("File is invalid. Must be a text file.", Error_Code);
                    }
                }
                else
                {
                    insert_Log("File not found or file path not valid. Try again.", Error_Code);
                }
            }
        }

        private void insertDCLoadCommands(string path)
        {
            try
            {
                using (var readFile = new StreamReader(path))
                {
                    SCPI_Commands.Output_setVolt = readFile.ReadLine().Trim();
                    SCPI_Commands.Output_setCurr = readFile.ReadLine().Trim();
                    SCPI_Commands.Output_measVolt = readFile.ReadLine().Trim();
                    SCPI_Commands.Output_measCurr = readFile.ReadLine().Trim();
                    SCPI_Commands.Output_outputON = readFile.ReadLine().Trim();
                    SCPI_Commands.Output_outputOFF = readFile.ReadLine().Trim();
                    SCPI_Commands.Output_minChars = short.Parse(readFile.ReadLine().Trim());
                    SCPI_Commands.Output_maxChars = short.Parse(readFile.ReadLine().Trim());
                    SCPI_Commands.Output_ScientificNotation = short.Parse(readFile.ReadLine().Trim());
                    SCPI_Commands.customLoad_Commands = true;
                    insert_Log("DC Load Commands File Processed.", Success_Code);
                    printLoadCommands();
                }
            }
            catch (Exception)
            {
                insert_Log("File read failed. Try again.", Error_Code);
                insert_Log("Reverting back to default commands.", Error_Code);
                SCPI_Commands.Output_setVolt = "VOLT";
                SCPI_Commands.Output_setCurr = "CURR";
                SCPI_Commands.Output_measVolt = "MEAS:VOLT?";
                SCPI_Commands.Output_measCurr = "MEAS:CURR?";
                SCPI_Commands.Output_outputON = "OUTPut ON";
                SCPI_Commands.Output_outputOFF = "OUTPut OFF";
                SCPI_Commands.Output_minChars = 10;
                SCPI_Commands.Output_maxChars = 12;
                SCPI_Commands.Output_ScientificNotation = 1;
                SCPI_Commands.customLoad_Commands = false;
            }
        }

        private void printLoadCommands()
        {
            insert_Log("Set Voltage: " + SCPI_Commands.Output_setVolt, Config_Code);
            insert_Log("Set Current: " + SCPI_Commands.Output_setCurr, Config_Code);
            insert_Log("Measure Voltage: " + SCPI_Commands.Output_measVolt, Config_Code);
            insert_Log("Measure Current: " + SCPI_Commands.Output_measCurr, Config_Code);
            insert_Log("Output On: " + SCPI_Commands.Output_outputON, Config_Code);
            insert_Log("Output Off: " + SCPI_Commands.Output_outputOFF, Config_Code);
            insert_Log("Expected Min Data Length: " + SCPI_Commands.Output_minChars.ToString(), Config_Code);
            insert_Log("Expected Max Data Length: " + SCPI_Commands.Output_maxChars.ToString(), Config_Code);
            insert_Log("Scientific Notation: " + SCPI_Commands.Output_ScientificNotation.ToString(), Config_Code);
            insert_Log("DC Load Commands Loaded.", Success_Code);
        }
    }
}
