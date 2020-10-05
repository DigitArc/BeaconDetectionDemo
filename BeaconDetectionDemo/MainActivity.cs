using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using Android.OS;
using Android.App;
using Android.Graphics;
using Android.Widget;
using Android.Support.V7.App;
using Java.Net;
using Java.Util;
using Org.Altbeacon.Beacon;
using Region = Org.Altbeacon.Beacon.Region;
using Uri = Android.Net.Uri;


namespace BeaconDetectionDemo
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IBeaconConsumer, IRangeNotifier
    {
        private TextView tvDistance;
        private BeaconManager beaconManager;
        private ImageView image;
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

            tvDistance = FindViewById<TextView>(Resource.Id.tvDistance);
            image = FindViewById<ImageView>(Resource.Id.digitarc);
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
                beaconManager.StartRangingBeaconsInRegion(new Region("2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6",
                    Identifier.FromUuid(UUID.FromString("2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6")),
                    Identifier.FromInt(1),
                    Identifier.FromInt(1)));
                //beaconManager.StartRangingBeaconsInRegion(new Region(Guid.NewGuid().ToString(), new List<Identifier>
                //{
                //    Identifier.FromUuid(UUID.FromString("0000ffe0-0000-1000-8000-00805f9b34fb")),
                //    Identifier.FromUuid(UUID.FromString("5276d744-90c9-40a7-960e-cc1c3764bc40")),
                //    Identifier.FromUuid(UUID.FromString("dd2c373f-ea1b-4b7f-bee0-e07110d2ae06"))
                //}));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private bool isShowToastMessage = false;
        public void DidRangeBeaconsInRegion(ICollection<Beacon> p0, Region p1)
        {
            if (!p0.Any())
                return;
            var toast = Toast.MakeText(Application.Context, "Beacon bulundu", ToastLength.Short);
            if (isShowToastMessage == false)
            {
                toast.Show();
                isShowToastMessage = true;
            }

            RunOnUiThread(() =>
            {
                tvDistance.Text = p0.First().Distance.ToString("0.00");
            });
        }
    }
}

