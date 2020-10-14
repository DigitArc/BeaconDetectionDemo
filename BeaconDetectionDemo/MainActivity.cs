using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using Android.OS;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Locations;
using Android.Media;
using Android.Support.V4.App;
using Android.Widget;
using Android.Support.V7.App;
using Java.Net;
using Java.Util;
using Org.Altbeacon.Beacon;
using Org.Altbeacon.Beacon.Startup;
using Region = Org.Altbeacon.Beacon.Region;
using Uri = Android.Net.Uri;


namespace BeaconDetectionDemo
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IBeaconConsumer, IRangeNotifier
    {
        private TextView tvDistance;
        private TextView beaconCount;
        private TextView beaconIds;
        private BeaconManager beaconManager;
        private TextView tableName;
        private ImageView image;
        private TextView beaconsTextView;

        private static readonly Dictionary<string, string> BeaconList = new Dictionary<string, string>
        {
            {"F3:01:9A:8A:21:FF","MASA 1"},
            {"C6:26:C7:DD:D6:CA","MASA 2"},
            {"F0:6E:83:15:61:72","MASA 3"}
        };
        private static Dictionary<string, Beacon> DetectedBeacons = new Dictionary<string, Beacon>();

        private Beacon _selectedBeacon = null;
        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            beaconIds = FindViewById<TextView>(Resource.Id.beaconCount);
            //tvDistance = FindViewById<TextView>(Resource.Id.tvDistance);
            image = FindViewById<ImageView>(Resource.Id.digitarc);
            //tableName = FindViewById<TextView>(Resource.Id.tableName);
            var imageBitmap = GetImageBitmapFromUrl("https://digitarc.net/assets/img/digitarc-logo-gri.png");
            image.SetImageBitmap(imageBitmap);
            beaconManager = BeaconManager.GetInstanceForApplication(this);

            //-------------------------
            // To detect proprietary beacons, you must add a line like below corresponding to your beacon
            // type.  Do a web search for "setBeaconLayout" to get the proper expression.
            //var beaconParser = new BeaconParser();
            //beaconParser.SetBeaconLayout("m:2-3=beac,i:4-19,i:20-21,i:22-23,p:24-24,d:25-25");
            //beaconManager.BeaconParsers.Add(beaconParser);
            //-------------------------

            var beaconParser = new BeaconParser();
            beaconParser.SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24");

            var beaconParser2 = new BeaconParser();
            beaconParser2.SetBeaconLayout("m:2-3=beac,i:4-19,i:20-21,i:22-23,p:24-24,d:25-25");
            CreateNotificationChannel();
            beaconManager.BeaconParsers.Add(beaconParser);
            beaconManager.BeaconParsers.Add(beaconParser2);
            beaconManager.AddRangeNotifier(this);
            beaconManager.Bind(this);
        }

        protected override void OnDestroy()
        {
            beaconManager.RemoveAllRangeNotifiers();
            beaconManager.Unbind(this);

            base.OnDestroy();
        }

        public void OnBeaconServiceConnect()
        {
            try
            {
                var region = new Region("SarayMuhallebicisi", new List<Identifier>
                {
                    Identifier.FromUuid(UUID.FromString("adc2050d-3dce-4700-adf9-f42c8f70e5a4"))
                });
                //beaconManager.ForegroundScanPeriod = 1;
                //beaconManager.ForegroundBetweenScanPeriod = 1;
                //beaconManager.BackgroundBetweenScanPeriod= 1;
                //beaconManager.BackgroundScanPeriod = 1;
                beaconManager.StartRangingBeaconsInRegion(region);
              
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification 
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel("Digitarc", "Digitarc", NotificationImportance.Default)
            {
                Description = "Digitarc"
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
        private bool isShowToastMessage = false;
        public void DidRangeBeaconsInRegion(ICollection<Beacon> p0, Region p1)
        {

            if (!p0.Any())
                return;
            var beacons = p0.ToList().OrderBy(p => p.Distance).ToList();

            var first = beacons.First();
            if (_selectedBeacon == null)
            {
                _selectedBeacon = first;
            }

            var updateBeacon = beacons.FirstOrDefault(p => p.BluetoothAddress == _selectedBeacon.BluetoothAddress);
            if (updateBeacon != null)
            {
                _selectedBeacon = updateBeacon;
            }
            else
            {
                if (first.Distance < _selectedBeacon.Distance)
                {
                    _selectedBeacon = first;
                }
            }

            if (_selectedBeacon.BluetoothAddress != first.BluetoothAddress)
            {
                if (first.Distance < _selectedBeacon.Distance)
                {
                    _selectedBeacon = first;
                }
            }

            beaconIds.Text = BeaconList[_selectedBeacon.BluetoothAddress];
           
        }


    }
}

