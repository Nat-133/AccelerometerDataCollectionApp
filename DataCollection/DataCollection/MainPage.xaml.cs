using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using DataCollection.Services;
using Xamarin.Forms;
using Xamarin.Essentials;
using DataCollection.Interfaces;
using System.Net;

namespace DataCollection
{
    public partial class MainPage : ContentPage 
    {
        bool collectionActive = false;
        bool drunk = false;
        readonly string dataPath;
        List<float[]> dataList = new List<float[]>();

        public MainPage() 
        {
            InitializeComponent();
            string dataFile = "accelerometer_data.json";
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            dataPath = Path.Combine(documentsPath, dataFile);
            string idFile = "unique_id.txt";
            string idPath = Path.Combine(documentsPath, idFile);
            if (!File.Exists(dataPath))
            {
                File.WriteAllText(dataPath, "[");
            }
            if (!File.Exists(idPath))
            {
                Random r = new Random();
                int id = r.Next(int.MaxValue);
                File.WriteAllText(idPath, id.ToString());
            }
            Userid.Text = "user id: "+File.ReadAllText(idPath);
        }

        void ToggleCollection(object sender, System.EventArgs e)
        {
            if (collectionActive)
            {
                DependencyService.Get<IServiceHandler>().StopService();
                ((Button)sender).Text = "Activate Data Collection";
            }
            else
            {
                DependencyService.Get<IServiceHandler>().StartService();
                ((Button)sender).Text = "Dectivate Data Collection";
            }
            collectionActive = !collectionActive;
        }
        void DeleteData(object sender, EventArgs e)
        {
            File.WriteAllText(dataPath, "[");
            ReadData(sender, e);
        }

        void ReadData(object sender, EventArgs e)
        {
            //string result = File.ReadAllText(dataPath);
            long fileSize = new System.IO.FileInfo(dataPath).Length;
            string result = fileSize.ToString();// + "\n" + result;
            Readings.Text = result;
        }

        void ResendData(object sender, EventArgs e)
        {
            
            string json = File.ReadAllText(dataPath);
            var request = (HttpWebRequest)WebRequest.Create("https://aitsaiac.dcs.warwick.ac.uk/cgi-bin/test.cgi");
            request.ContentType = "application/json";
            request.Method = "POST";
            request.ServerCertificateValidationCallback = delegate { return true; };
            HttpWebResponse response;
            try
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json+"]");
                }

                response = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();
                }
                DeleteData(sender, e);
            }
            catch (Exception _)
            {
                response = null;
            }
            ReadData(new object(), new EventArgs());
        }

        void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            float x = data.Acceleration.X;
            float y = data.Acceleration.Y;
            float z = data.Acceleration.Z;
            float[] vector = { x, y, z };
            dataList.Add(vector);
            // Process Acceleration X, Y, and Z
        }
    }
}

