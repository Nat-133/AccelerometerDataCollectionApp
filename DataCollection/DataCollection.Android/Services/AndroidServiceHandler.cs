using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DataCollection.Droid.Services;
using DataCollection.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidServiceHandler))]
namespace DataCollection.Droid.Services
{
    class AndroidServiceHandler : IServiceHandler
    {
        public void StartService()
        {
            Intent intent = new Intent(Application.Context, typeof(DataRecorderService));
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Application.Context.StartForegroundService(intent);
            }
            else
            {
                Application.Context.StartService(intent);
            }
        }

        public void StopService()
        {
            Intent intent = new Intent(Application.Context, typeof(DataRecorderService));
            Application.Context.StopService(intent);
        }
    }
}