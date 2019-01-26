﻿using AmosAndroid;
using AmosShared.Base;
using Android.App;
using Android.Content.PM;
using Android.Widget;
using Android.OS;

namespace Type.Android
{
    [Activity(Label = "Type.Android", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : GameActivity
    {
        public MainActivity() : base(new Game())
        {
        }
    }
}

