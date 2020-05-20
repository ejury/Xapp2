//using System;

//using Xamarin.Forms;
//using SQLite;
//using System.IO;
//using Xapp2.iOS;
//using Xapp2.Data;

//[assembly: Dependency(typeof(ISQLiteDbInterface_iOS))]
//namespace Xapp2.iOS
//{
//    public class ISQLiteDbInterface_iOS: ISQLiteInterface
//    {
//              public SQLite.SQLiteConnection GetConnection ()  

//        {  

//            var fileName = "xappios.db3";  

//            var documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);  

//            var libraryPath = Path.Combine (documentsPath, "..", "Library");  

//            var path = Path.Combine (libraryPath, fileName);  



//           // var platform = new SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS ();  

//            var connection = new SQLite.SQLiteConnection ( path);  



//            return connection;  

//        }  
//    }
//}