using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Hardware;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Xamarin.Essentials;
using Android.Util;
using Android.Gms.Tasks;

namespace DataCollection.Droid.Services
{

    static class Globals
    {
        public static int CurrentActivity = DetectedActivity.Unknown;
        public static int OnFootConfidence = 0;
        public static bool Running = false;
    }

    [Service]
    class DataRecorderService : Service
    {
        private const int NotifID = 9000;
        List<IFormattable[]> dataList;
        NotificationManager notificationManager = (NotificationManager)Application.Context.GetSystemService(NotificationService);
        private NotificationCompat.Builder builder;
        private CountDownTimer timer;

        private SensorManager manager;
        private Sensor linearAccelerometer;

        string dataFile = "accelerometer_data.json";
        string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        string dataPath;
        string idFile = "unique_id.txt";
        string userID;
        Vibrator vibrator = (Vibrator) Application.Context.GetSystemService(Context.VibratorService);

        ActivityRecognitionClient client;
        LinearAccelerationSensor listener;


        private static long[] recordVibration = { 0, 600, 100, 200, 100, 600 };
        private static long[] waitVibration = { 0, 600, 100, 200, 100, 200 };
        private static long[] walkingVibration = { 0, 100, 100, 100 };

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override void OnCreate()
        {
            if (Globals.Running)
            {
                return;
            }
            base.OnCreate();
            Globals.Running = true;
            manager = (SensorManager)GetSystemService(SensorService);
            linearAccelerometer = manager.GetDefaultSensor(SensorType.LinearAcceleration);
            //manager.RegisterListener(linearSensor, SensorDelay.Ui);
            //sensorManager.UnregisterListener(linearSensor);
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            dataPath = Path.Combine(documentsPath, dataFile);
            string idPath = Path.Combine(documentsPath, idFile);
            userID = File.ReadAllText(idPath);
            client = new ActivityRecognitionClient(Application.Context);
            listener = new LinearAccelerationSensor(LinearAccelerationChanged);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            Globals.Running = false;
            timer.Cancel();
            if (accelerometerMonitoring)
            {
                manager.UnregisterListener(listener);
            }
            if(activityMonitoring)
            {
                client.RemoveActivityUpdates(pIntent);
            }
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            
            builder = getNotif();
            StartForeground(NotifID, builder.Build());

            StartTimer(10*1000);

             return StartCommandResult.Sticky;
        }

        public void StopService()
        {
            StopForeground(true);
            StopSelf();
        }

        public void StartTimer(int time)
        {
            TimeSpan start = new TimeSpan(5, 0, 0);
            TimeSpan stop = new TimeSpan(7, 0, 0);
            TimeSpan now = DateTime.Now.TimeOfDay;
            if ((now > start) && (now < stop))
            {
                // if it is between 5 and 7 in the morning,
                // end the service, I'm assuming that 
                // the person has gone to bed by now
                StopSelf();
                return;
            }
            Action<long> onTick = new Action<long>((long m) =>
            {
                UpdateNotification(m);
            });
            Action onFinish = new Action(() =>
            {
                timer.Cancel();
                StartRecordingData();
            });
            timer = new NotificationTimer(time, 1000, onTick, onFinish);
            builder.SetContentTitle("Not collecting");
            notificationManager.Notify(NotifID, builder.Build());
            Vibrate(waitVibration);
            timer.Start();
        }

        PendingIntent pIntent
        {
            get
            {
                Intent intent = new Intent(this, typeof(DetectedActivitiesIntentService));
                PendingIntent _pIntent = PendingIntent.GetService(
                    Application.Context,
                    0,
                    intent,
                    PendingIntentFlags.UpdateCurrent);
                return _pIntent;
            }
        }
        private int count = 0;
        private bool activityMonitoring = false;
        private bool accelerometerMonitoring = false;
        private int timerLength = 10 *60* 1000;
        public void StartRecordingData()
        {
            Task task = client.RequestActivityUpdates(0, pIntent);
            task.AddOnSuccessListener(new OnSuccessListener { Activity=this});
            task.AddOnFailureListener(new OnFailureListener { Activity = this });
            activityMonitoring = true;
            Action<long> onTick = new Action<long>((long m) =>
            {
                count += 1;
                if (count==10)
                {
                    //if (Globals.CurrentActivity == DetectedActivity.OnFoot)
                    if(true)
                    {
                        Vibrate(walkingVibration);
                        manager.RegisterListener(listener, linearAccelerometer, SensorDelay.Game);
                        accelerometerMonitoring = true;
                        timerLength = 10 * 60 * 1000;
                    }
                    else
                    {
                        timerLength = 2 * 60 * 1000;
                    }
                    
                    //Accelerometer.Start(SensorSpeed.Fastest);
                }
                UpdateNotification(m);
            });
            Action onFinish = new Action(() =>
            {
                count = 0;
                manager.UnregisterListener(listener);
                accelerometerMonitoring = false;
                client.RemoveActivityUpdates(pIntent);
                activityMonitoring = false;
                //Accelerometer.Stop();
                timer.Cancel();
                SaveData();
                //StartTimer(timerLength);
                StartRecordingData();
            });
            timer = new NotificationTimer(40000, 1000, onTick, onFinish);
            builder.SetContentTitle("Collecting, please walk around");
            notificationManager.Notify(NotifID, builder.Build());
            dataList = new List<IFormattable[]>();
            Vibrate(recordVibration);
            timer.Start();
        }

        void LinearAccelerationChanged(SensorEvent e)
        {
            float x = e.Values[0];
            float y = e.Values[1];
            float z = e.Values[2];
            long time = e.Timestamp;
            IFormattable[] vector = { time, x, y, z };
            dataList.Add(vector);
            
        }
        void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            float x = data.Acceleration.X;
            float y = data.Acceleration.Y;
            float z = data.Acceleration.Z;
            IFormattable[] vector = { x, y, z };
            dataList.Add(vector);
            // Process Acceleration X, Y, and Z
        }

        void UpdateNotification(long time)
        {
            string text = "";
            long seconds = (time / 1000);
            long minutes = seconds / 60;
            if (minutes < 10)
            {
                text += "0";
            }
            text += minutes.ToString();
            text += ":";
            seconds = seconds % 60;
            if (seconds < 10)
            {
                text += "0";
            }
            text += seconds.ToString();

            builder.SetContentText(text);
            Notification notif = builder.Build();
            notificationManager.Notify(NotifID, notif);
        }

        /// <summary>
        /// executes the vibration pattern given
        /// </summary>
        /// <param name="waveform">a sequence of off/on time values starting with off</param>
        void Vibrate(long[] waveform)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                int[] amplitudes = new int[waveform.Length];
                for (int i = 0; i < amplitudes.Length; i++)
                {
                    amplitudes[i] = 255;
                }
                // create VibrationEffect instance and createWaveform of vibrationWaveFormDurationPattern
                // -1 here is the parameter which indicates that the vibration shouldn't be repeated.
                VibrationEffect vibrationEffect = VibrationEffect.CreateWaveform(waveform,amplitudes, -1);

                // it is safe to cancel all the vibration taking place currently
                vibrator.Cancel();

                // now initiate the vibration of the device
                vibrator.Vibrate(vibrationEffect);
            }
        }

        void SaveData()
        {
            if (dataList.Count == 0)
            {
                return;
            }
            string json = GetDataJson();
            bool sent = SendData(json);
            if (sent)
            {
                return;
            }
            long fileLength = new FileInfo(dataPath).Length;
            if (fileLength == 0)
            {
                json = "[\n" + json;
            }
            else if (fileLength > 1)
            {
                json = ",\n" + json;
            }
            using (StreamWriter sw = File.AppendText(dataPath))
            {
                sw.Write(json);
            }
        }

        private string GetDataJson()
        {
            string json = "{\n";
            //json += "  \"id\": " + userID + ",\n";
            json += "  \"id\": \"" + "DISCARD" + "\",\n";
            json += "  \"drunk\": true,\n";
            json += "\"walking\": " + Globals.OnFootConfidence.ToString() + ",\n";
            json += "  \"data\": [\n";
            int i = 0;
            foreach (IFormattable[] v in dataList)
            {
                i += 1;
                // writes each vector
                json += $"    [{v[0]}, {v[1]}, {v[2]}, {v[3]}]";
                if (i != dataList.Count)
                {
                    json += ",";
                }
                json += "\n";
            }
            json += "  ]\n";
            json += "}";

            return json;
        }

        private bool SendData(string json)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://aitsaiac.dcs.warwick.ac.uk/cgi-bin/data_handler.cgi");
            request.ContentType = "application/json";
            request.Method = "POST";
            request.ServerCertificateValidationCallback = delegate { return true; };
            HttpWebResponse response;
            try
            { 
                using(var streamWriter=new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write("["+json+"]");
                }

                response = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();
                }
            } 
            catch (WebException _)
            {
                response = null;
            }
            return !(response is null);
        }

        private NotificationCompat.Builder getNotif()
        {
            string foregroundChannelId = "9001";
            Intent intent = new Intent(Application.Context, typeof(DataRecorderService));
            intent.AddFlags(ActivityFlags.SingleTop);
            intent.PutExtra("Title", "Message");
            PendingIntent pendingIntent = PendingIntent.GetActivity(Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);

            Action stop = new Action(() => { StopService(); });

            Intent stopIntent = new Intent("StopDataCollectionBackground");
            PendingIntent stopPI = PendingIntent.GetBroadcast(this, 4, stopIntent, PendingIntentFlags.CancelCurrent);

            BroadcastReceiver receiver = new CallMethodBroadcastReceiver(stop);
            RegisterReceiver(receiver, new IntentFilter("StopDataCollectionBackground"));

        var notifBuilder = new NotificationCompat.Builder(Application.Context, foregroundChannelId)
                .SetContentTitle("it's time")
                .SetContentText("to do some shit")
                .SetOnlyAlertOnce(true)
                .SetSmallIcon(Resource.Drawable.notification_template_icon_low_bg)
                .SetOngoing(true)
                //.SetContentIntent(pendingIntent)
                .SetContentIntent(stopPI)
                .AddAction(0, "Stop Recording", stopPI);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel(foregroundChannelId, "Title", NotificationImportance.High);
                notificationChannel.Importance = NotificationImportance.High;
                notificationChannel.EnableLights(true);
                notificationChannel.EnableVibration(true);
                notificationChannel.SetShowBadge(true);
                notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300, 400, 500, 400, 300, 200, 400 });
                
                var notifManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
                if (notifManager != null)
                {
                    notifBuilder.SetChannelId(foregroundChannelId);
                    notifManager.CreateNotificationChannel(notificationChannel);
                }
            }
            return notifBuilder;
        }

        public class LinearAccelerationSensor : Activity, ISensorEventListener
        {
            private Action<SensorEvent> onChange;

            public LinearAccelerationSensor(Action<SensorEvent> _onChange)
            {
                onChange = _onChange;
            }

            public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
            {
            }

            public void OnSensorChanged(SensorEvent e)
            {
                onChange(e);
            }
        }

        public class NotificationTimer : CountDownTimer
        {
            private Action<long> tickEvent;
            private Action finishEvent;

            public NotificationTimer(long totaltime, long interval, 
                Action<long> onTick, Action onFinish)
                : base(totaltime, interval)
            {
                tickEvent = onTick;
                finishEvent = onFinish;
            }

            public override void OnFinish()
            {
                finishEvent();
            }
            
            public override void OnTick(long millisUntilFinished)
            {
                tickEvent(millisUntilFinished);
            }
        }

        public class OnSuccessListener : Java.Lang.Object, IOnSuccessListener
        {
            public DataRecorderService Activity { get; set; }

            public void OnSuccess(Java.Lang.Object result)
            {

            }
        }
        public class OnFailureListener : Java.Lang.Object, IOnFailureListener
        {
            public DataRecorderService Activity { get; set; }
            public void OnFailure(Java.Lang.Exception e)
            {

            }
        }
        
    }

    //[BroadcastReceiver]
    //public class DetectedActivitiesIntentService : BroadcastReceiver
    //{
    //    protected const string TAG = "DetectedActivitiesIS";

    //    public DetectedActivitiesIntentService()
    //        : base()
    //    {
    //    }

    //    public DetectedActivitiesIntentService(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    //    {
    //    }

    //    public override void OnReceive(Context context, Intent intent)
    //    {
    //        var result = ActivityRecognitionResult.ExtractResult(intent);

    //        IList<DetectedActivity> detectedActivities = result.ProbableActivities;
    //        int mostLikelyConfidence = 0;
    //        int mostLikely = DetectedActivity.Unknown;
    //        foreach (DetectedActivity a in detectedActivities)
    //        {
    //            if (a.Confidence > mostLikelyConfidence)
    //            {
    //                mostLikely = a.Type;
    //                mostLikelyConfidence = a.Confidence;
    //            }
    //        }
    //        Globals.CurrentActivity = mostLikely;
    //    }
    //}
    class CallMethodBroadcastReceiver : BroadcastReceiver
    {
        private Action toCall;
        public CallMethodBroadcastReceiver(Action actionToCall)
        {
            toCall = actionToCall;
        }
        public override void OnReceive(Context context, Intent intent)
        {
            toCall();
        }
    }


    [Service(Exported = false)]
    public class DetectedActivitiesIntentService : IntentService
    {
        protected const string TAG = "DetectedActivitiesIS";

        public DetectedActivitiesIntentService()
            : base(TAG)
        {
        }

        protected override void OnHandleIntent(Intent intent)
        {
            var result = ActivityRecognitionResult.ExtractResult(intent);

            IList<DetectedActivity> detectedActivities = result.ProbableActivities;
            int mostLikelyConfidence = 0;
            int mostLikely = DetectedActivity.Unknown;
            foreach (DetectedActivity a in detectedActivities)
            {
                if (a.Confidence > mostLikelyConfidence)
                {
                    mostLikely = a.Type;
                    mostLikelyConfidence = a.Confidence;
                }
                if (a.Type == DetectedActivity.OnFoot && a.Confidence >= 60) 
                {
                    Globals.OnFootConfidence = a.Confidence;
                    break;
                }
            }
            Globals.CurrentActivity = mostLikely;
        }
    }
}