using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xapp2.Models;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Net;

namespace Xapp2.Data
{   
    class APIServer
    {
        const string Url = "http://192.168.1.42:5555/api/";
        //const string Url = "https://localhost:44303/api/";
        //const string Url = "http://localhost:59438/api/";


        static public async Task RegClient(Credentials tempCred)
        {
            var response = await Globals.client.PostAsync(Url + "Account/", new StringContent(JsonConvert.SerializeObject(tempCred), Encoding.UTF8, "application/json"));
            string content = await response.Content.ReadAsStringAsync();
            var newcontent = JsonConvert.DeserializeObject<string>(content);
            /*            var newcontent1 = newcontent.Values;
                        var newcontent2 = newcontent.Keys;
                        var newcontent3 = newcontent["access_token"];
            
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadToken(newcontent);
             */

            Globals.JWTkey = newcontent;
            var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", Globals.JWTkey);
            Globals.client.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            return ;
        }
        static public async Task<string> AdminClient(Credentials tempCred)
        {
            var response = await Globals.client.PostAsync(Url + "Account/Admin", new StringContent(JsonConvert.SerializeObject(tempCred), Encoding.UTF8, "application/json"));
            string content = await response.Content.ReadAsStringAsync();
            var newcontent = JsonConvert.DeserializeObject<string>(content);

            if (newcontent == "Unknown Error" | newcontent == "Database Request Failed" | newcontent == "User Not Found" | newcontent == "Invalid Credentials")
            {
                return newcontent;
            }

            return await JWTDecode(newcontent);
        }
        static public async Task<string> SEClient(Worker tempWorker)
        {
            var response = await Globals.client.PostAsync(Url + "Account/SEBadge/", new StringContent(JsonConvert.SerializeObject(tempWorker), Encoding.UTF8, "application/json"));
            string content = await response.Content.ReadAsStringAsync();
            var newcontent = JsonConvert.DeserializeObject<string>(content);

            //Error Handler
            if (newcontent == "Card Not Active" | newcontent == "Unable to Sign In" | newcontent == "Invalid Card Used" | newcontent == "Server Error")
            {
                return newcontent;
            }

            //Decode JWT Token
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(newcontent);

            
            //Update globals with claims from JWT
            Globals.SELevel = Int32.Parse(jsonToken.Claims.First(claim => claim.Type == "SELevel").Value);
            string UFirst = jsonToken.Claims.First(claim => claim.Type == "FirstName").Value;
            string ULast = jsonToken.Claims.First(claim => claim.Type == "LastName").Value;
            Globals.UserDisplay = UFirst.FirstOrDefault().ToString() + ". " + ULast;
            Globals.ServerName = jsonToken.Claims.First(claim => claim.Type == "ServerName").Value;

            Globals.JWTkey = newcontent;
            Globals.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Globals.JWTkey);
            return "Complete";
        }
        static public async Task<string> JWTDecode(string content)
        {
            //Decode JWT Token
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(content);


            //Update globals with claims from JWT
            Globals.SELevel = Int32.Parse(jsonToken.Claims.First(claim => claim.Type == "SELevel").Value);
            string UFirst = jsonToken.Claims.First(claim => claim.Type == "FirstName").Value;
            string ULast = jsonToken.Claims.First(claim => claim.Type == "LastName").Value;
            Globals.UserDisplay = UFirst.FirstOrDefault().ToString() + ". " + ULast;
            Globals.ServerName = jsonToken.Claims.First(claim => claim.Type == "ServerName").Value;

            Globals.JWTkey = content;
            Globals.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Globals.JWTkey);
            return "Complete";
        }

        public static async Task<IEnumerable<Unit>> GetAllUnits(string tag)
        {
            string result = await Globals.client.GetStringAsync(Url + "DataRequest/UnitReq/" + tag);
            return JsonConvert.DeserializeObject<IEnumerable<Unit>>(result);
        }
        public static async Task<IEnumerable<Vessel>> GetAllVessels(string tag)
        {
            string result = await Globals.client.GetStringAsync(Url + "DataRequest/VesselReq/" + tag);
            return JsonConvert.DeserializeObject<IEnumerable<Vessel>>(result);
        }
        public static async Task<IEnumerable<Worker>> GetAllWorkers(string tag)
        {
            try
            {

                string result = await Globals.client.GetStringAsync(Url + "DataRequest/WorkerReq/" + tag);
                return JsonConvert.DeserializeObject<IEnumerable<Worker>>(result);
            }
            catch
            {
                string result = await Globals.client.GetStringAsync(Url + "DataRequest/WorkerReq/" + tag);
                Exception temp = JsonConvert.DeserializeObject<Exception>(result);
                return JsonConvert.DeserializeObject<IEnumerable<Worker>>(result);
            }
        }
        public static async Task<IEnumerable<EntryLog>> GetAllEntryLogs(string tag)
        {

            try
            {
                string result = await Globals.client.GetStringAsync(Url + "DataRequest/EntryLogReq/" + tag);
                return JsonConvert.DeserializeObject<IEnumerable<EntryLog>>(result);
            }
            catch (Exception ex)
            {
                string result = await Globals.client.GetStringAsync(Url + "DataRequest/EntryLogReq/" + tag);
                return JsonConvert.DeserializeObject<IEnumerable<EntryLog>>(result);
            }
            
        }
        public static async Task<IEnumerable<AnalyticsLog>> GetAllAnalyticsLogs(string tag)
        {
            try
            {
                string result = await Globals.client.GetStringAsync(Url + "DataRequest/RecordLogReq/" + tag);
                return JsonConvert.DeserializeObject<IEnumerable<AnalyticsLog>>(result);
            }
            catch (Exception ex)
            {
                string result = await Globals.client.GetStringAsync(Url + "DataRequest/RecordLogReq/" + tag);
                return JsonConvert.DeserializeObject<IEnumerable<AnalyticsLog>>(result);
            }
        }
        static async public Task<int> Add(Tuple <Unit,Vessel> newdata, string tag)
        {
            var response = await Globals.client.PostAsync(Url + "units/" + tag, new StringContent(JsonConvert.SerializeObject(newdata), Encoding.UTF8, "application/json"));
            int newID = (int)JsonConvert.DeserializeObject<int>(response.Content.ReadAsStringAsync().Result);
            return newID;

        }
        static async public Task<int> AddWorker(Worker newdata)
        {
            var response = await Globals.client.PostAsync(Url + "units/PostWorker/", new StringContent(JsonConvert.SerializeObject(newdata), Encoding.UTF8, "application/json"));
            int newID;
            if (response.IsSuccessStatusCode)
            {
                 newID = (int)JsonConvert.DeserializeObject<int>(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                Exception ex = (Exception)JsonConvert.DeserializeObject<Exception>(response.Content.ReadAsStringAsync().Result);
                newID = -5;
            }
            return newID;

        }
        static async public Task<int> AddLog(EntryLog newdata)
        {
             var response = await Globals.client.PostAsync(Url + "units/PostLog/", new StringContent(JsonConvert.SerializeObject(newdata), Encoding.UTF8, "application/json"));
            int newID = (int)JsonConvert.DeserializeObject<int>(response.Content.ReadAsStringAsync().Result);
            return newID;
        }
        static async public Task<int> AddRecord(AnalyticsLog newdata)
        {
            var response = await Globals.client.PostAsync(Url + "units/PostRecord/", new StringContent(JsonConvert.SerializeObject(newdata), Encoding.UTF8, "application/json"));
            int newID = (int)JsonConvert.DeserializeObject<int>(response.Content.ReadAsStringAsync().Result);
            return newID;
        }
        static public async Task Delete(string tag)
        {
            var response = await Globals.client.DeleteAsync(Url + "units/" + tag);
            return; 
        }
        static public async Task DeleteLog(int tag)
        {
            var response = await Globals.client.DeleteAsync(Url + "units/DeleteLog/" + tag);
            return;
        }

        public async Task Update(Unit unit)
        {
            await Globals.client.PutAsync(Url + "/" + unit.UnitID, new StringContent(JsonConvert.SerializeObject(unit), Encoding.UTF8, "application/json"));
        }
    }
}
