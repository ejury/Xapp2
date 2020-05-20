
//using System.IO;


//using SQLite;

//using Xamarin.Forms;
//using Xamarin.Forms;
//using Xapp2.Data;
//using Xapp2.Android;

//[assembly: Dependency(typeof(GetSQLiteConnection))]

//namespace Xapp2.Android

//{
//    public class GetSQLiteConnection : ISQLiteInterface

//    {
//        public GetSQLiteConnection()

//        {

//        }
    
//        public SQLite.SQLiteConnection GetConnection()

//        {

//            var fileName = "UserDatabase.db3";

//            var documentPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

//            var path = Path.Combine(documentPath, fileName);

//      //      var platform = new SQLite.Platform.XamarinAndroid.SQLitePlatformAndroid();

//            var connection = new SQLiteConnection( path,true);

//            return connection;

//        }

//    }

//}