//using System;
//using System.IO;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Support.V4.Content;
//using Google.Api;
//using Xamarin.Forms;
//using Xamarin.Forms.PlatformConfiguration;

//namespace SALONXAMARIN.Class
//{
//    public class DroidFileHelper
//    {

//        public string GetLocalFilePath(string filename)
//        {
//            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
//            return Path.Combine(path, filename);
//        }

//        public async Task SaveFileToDefaultLocation(string fileName, byte[] bytes, bool showFile = false)
//        {
//            Context currentContext = Android. App.Application.Context;
//            string directory = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
//            if (!Directory.Exists(directory))
//            {
//                Directory.CreateDirectory(directory);
//            }
//            string file = Path.Combine(directory, fileName);
//            System.IO.File.WriteAllBytes(file, bytes);

//            //If you want to open up the file after download the use below code
//            if (showFile)
//            {

//                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
//                {

//                    string externalStorageState = global::Android.OS.Environment.ExternalStorageState;
//                    var externalPath = global::Android.OS.Environment.ExternalStorageDirectory.Path + "/" + global::Android.OS.Environment.DirectoryDownloads + "/" + fileName;
//                    File.WriteAllBytes(externalPath, bytes);

//                    Java.IO.File files = new Java.IO.File(externalPath);
//                    files.SetReadable(true);

//                    string application = "application/pdf";
//                    Intent intent = new Intent(Intent.ActionView);
//                    Android.Net.Uri uri = FileProvider.GetUriForFile(currentContext, "com.companyname.appname.provider", files);
//                    intent.SetDataAndType(uri, application);
//                    intent.SetFlags(ActivityFlags.GrantReadUriPermission);
//                    Forms.Context.StartActivity(intent);
//                }
//                else
//                {
//                    Intent promptInstall = new Intent(Intent.ActionView);
//                    promptInstall.SetDataAndType(Android.Net.Uri.FromFile(new Java.IO.File(file)), "application/pdf");
//                    promptInstall.SetFlags(ActivityFlags.NewTask);
//                    Forms.Context.StartActivity(promptInstall);
//                }
//            }
//        }
//    }
//}
