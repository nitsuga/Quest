using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Quest.HEMSLinkTest.Properties;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Quest.Lib.Utils;
using MessageBroker.Objects;
using Quest.Lib.HEMS;
using Quest.Lib.HEMS.Message;

namespace Quest.HEMSLinkTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MessageHelper _msgSource = new MessageHelper();
        private ExceptionManager exManager;

        public MainWindow()
        {
            exManager = SetupLogging();

            InitializeComponent();

            LastUpdate.Text = DateTime.Now.ToString();
            Dispatched.Text = DateTime.Now.ToString();
            Origin.Text = DateTime.Now.ToString();
            Updated.Text = DateTime.Now.ToString();

            // set up rabbit
            _msgSource.Initialise("Quest.XCReader", "amqp://guest:guest@localhost:5672", "CAC", "", 10000); // Telephony
            _msgSource.NewMessage += _msgSource_NewMessage;

        }

        ExceptionManager SetupLogging()
        {
            try
            {
                Logger.SetLogWriter(new LogWriterFactory().Create());
                ExceptionPolicyFactory policyFactory = new ExceptionPolicyFactory();
                return policyFactory.CreateManager();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Quest", ex.Message);
            }
            return null; // failed
        }

        void AddLog(TextBox target, string message)
        {
            target.Text += "\n" + message;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EventUpdate evnt = new EventUpdate()
                {
                    Address = Address.Text,
                    AZGrid = AZ.Text,
                    CallOrigin = DateTime.Parse(Origin.Text),
                    Callsign = Callsign.Text,
                    Determinant = Determinant.Text,
                    EventId = EventId.Text,
                    Dispatched = DateTime.Parse(Dispatched.Text),
                    Updated = DateTime.Parse(Updated.Text),
                    Easting = int.Parse(Easting.Text),
                    Latitude = float.Parse(Latitude.Text),
                    Longitude = float.Parse(Longitude.Text),
                    Northing = int.Parse(Northing.Text),
                    Age  = Age.Text,
                    Sex = Sex.Text
                };

                App.server.Send(evnt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ooops: Something broke.." + ex.ToString(), "Oh no", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoLogon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logon evnt = new Logon()
                {
                     AppId = AppId.Text,
                     Callsign = LogonCallsign.Text,
                     LastUpdate = DateTime.Parse(LastUpdate.Text),
                     MaxEvents= int.Parse(Easting.Text)
                };


                App.client.Send(evnt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ooops: Something broke.." + ex.ToString(), "Oh no", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoServerJSON_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.server.SendJSON(JSON.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ooops: Something broke.." + ex.ToString(), "Oh no", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoClientJSON_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.client.SendJSON(JSON.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ooops: Something broke.." + ex.ToString(), "Oh no", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void SendResUpdate(object sender, RoutedEventArgs e)
        {
            ResourceUpdate msg = new ResourceUpdate()
            {
                Agency = "LAS",
                Callsign =  RU_Callsign.Text,
                Destination = "24 High Beeches",
                Class="E",
                Direction="45",
                Emergency="A",
                EventType="T", 
                FleetNo = 1000,
                Incident = RU_Inc.Text,
                LastUpdate = DateTime.UtcNow.Subtract(new TimeSpan(1,0,0)),
                ResourceType="HEM",
                Status = RU_Status.Text,
                Sector="A",
                Skill="H",
                Speed=120
            };
            _msgSource.BroadcastMessage(msg);
        }

        void _msgSource_NewMessage(object sender, MessageBroker.NewMessageArgs e)
        {
            
        }

        private void ClientCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            App.client = new HEMSLinkServer();
            App.client.Initialise("handover.londonambulance.nhs.uk", Settings.Default.Port, true, 120, "LASLAA", "", "", "", "", 0, "", "", false, "", "", true, "default", "default");
            App.client.NewMessage += client_NewMessage;
        }

        void client_NewMessage(object sender, Lib.HEMS.HEMSEventArgs e)
        {
            if (e.HEMSMessage != null)
                Dispatcher.BeginInvoke(new Action<TextBox, String>(AddLog), Log, "Client:  " + e.HEMSMessage.MessageBody.ToString());
            Dispatcher.BeginInvoke(new Action<TextBox, String>(AddLog), Log, "Client:  " + e.RawMessage);
            if (e.ErrorMessage != null && e.ErrorMessage.Length > 0)
                Dispatcher.BeginInvoke(new Action<TextBox, String>(AddLog), Log, "Client:  " + e.ErrorMessage);
        }

        private void ServerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            App.server = new HEMSLinkServer();
            App.server.Initialise(null, Settings.Default.Port, true, 120, "HEMSAPPID123456789", "Quest.HEMS", "amqp://guest:guest@localhost:5672", "CAC", "", 10000, "", "", false, "", "", true, "default", "default");
            App.server.NewMessage += server_NewMessage;
        }

        private void ClientUnchecked(object sender, RoutedEventArgs e)
        {
            App.client.Close();
        }

        private void ServerUnchecked(object sender, RoutedEventArgs e)
        {
            App.server.Close();
        }

        void server_NewMessage(object sender, HEMSEventArgs e)
        {
            if (e.HEMSMessage != null)
                Dispatcher.BeginInvoke(new Action<TextBox, String>(AddLog), Log, "Server:  " + e.HEMSMessage.MessageBody.ToString());
            Dispatcher.BeginInvoke(new Action<TextBox, String>(AddLog), Log, "Server:  " + e.RawMessage);
            if (e.ErrorMessage != null && e.ErrorMessage.Length > 0)
                Dispatcher.BeginInvoke(new Action<TextBox, String>(AddLog), Log, "Server:  " + e.ErrorMessage);
        }

    }
}
