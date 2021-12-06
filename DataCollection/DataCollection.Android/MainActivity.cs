using System;
using System.IO;

using Xamarin.Forms;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using DataCollection.Services;
using System.Collections.Generic;
using DataCollection.Droid;

[assembly:Dependency(typeof(PublicFiles))]
namespace DataCollection.Droid
{
    [Activity(Label = "DataCollection", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class PublicFiles : IPublicFiles
    {
        Context context = Android.App.Application.Context;
        string dataFile = "accelerometer_data.json";
        string dataPath;
       
        void IPublicFiles.SaveData(bool drunk, List<float[]> data)
        {
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            dataPath = Path.Combine(documentsPath, dataFile);
            if (!File.Exists(dataPath))
            {
                File.WriteAllText(dataPath, "");
            }
            using (StreamWriter sw = File.AppendText(dataPath))
            {
                sw.WriteLine(drunk.ToString());
                sw.WriteLine("{");
                int i = 0;
                foreach (float[] v in data)
                {
                    i += 1;
                    sw.Write($"{{ {v[0]}, {v[1]}, {v[2]} }}");
                    if (i != data.Count)
                    {
                        sw.WriteLine(",");
                    }
                    else
                    {
                        sw.WriteLine("");
                    }
                }
                sw.WriteLine("}");
                sw.WriteLine("");
            }
        }


    }
}