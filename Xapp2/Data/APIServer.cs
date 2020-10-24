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
        //const string Url = "https://msi";
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

        //Login Queries
        static public async Task<string> AdminClient(Credentials tempCred)
        {
            try
            {
                var response = await Globals.client.PostAsync(Url + "Account/Admin", new StringContent(JsonConvert.SerializeObject(tempCred), Encoding.UTF8, "application/json"));
                string content = await response.Content.ReadAsStringAsync();
                var newcontent = JsonConvert.DeserializeObject<string>(content);
                return await JWTDecode(newcontent);
            }
            catch
            {
                return "Server Connection Error";
            }
        }
        static public async Task<string> SEClient(Worker tempWorker)
        {
            try
            {
                var response = await Globals.client.PostAsync(Url + "Account/SEBadge", new StringContent(JsonConvert.SerializeObject(tempWorker), Encoding.UTF8, "application/json"));
                string content = await response.Content.ReadAsStringAsync();
                var newcontent = JsonConvert.DeserializeObject<string>(content);
                return await JWTDecode(newcontent);
            }
            catch (Exception ex)
            {
                var response = await Globals.client.PostAsync(Url + "Account/SEBadge/", new StringContent(JsonConvert.SerializeObject(tempWorker), Encoding.UTF8, "application/json"));
                Exception test = (Exception)JsonConvert.DeserializeObject<Exception>(response.Content.ReadAsStringAsync().Result);
              
                return "Server Connection Error";
            }
        }
        
        //Login Queries - Decode JWT token after login
        static public async Task<string> JWTDecode(string content)
        {
            //Error Handler
            if (content == "Card Not Active" | content == "Unable to Sign In" | content == "Invalid Card Used" | content == "Server Error")
            {
                return content;
            }

            //Decode JWT Token
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(content);

            //Update globals with claims from JWT
            Globals.SELevel = Int32.Parse(jsonToken.Claims.First(claim => claim.Type == "SELevel").Value);
            string UFirst = jsonToken.Claims.First(claim => claim.Type == "FirstName").Value;
            string ULast = jsonToken.Claims.First(claim => claim.Type == "LastName").Value;
            Globals.UserDisplay = UFirst.FirstOrDefault().ToString() + ". " + ULast;
            Globals.ServerName = jsonToken.Claims.First(claim => claim.Type == "ServerName").Value;
            Globals.OfflineMode = false;
            Globals.JWTkey = content;
            Globals.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Globals.JWTkey);
            return "Complete";
        }

        //Database Table Querys 
        public static async Task<IEnumerable<Unit>> GetAllUnits(string tag)
        {
            string result = await GetAllAPI("UnitReq", tag);
            return JsonConvert.DeserializeObject<IEnumerable<Unit>>(result);
        }
        public static async Task<IEnumerable<Vessel>> GetAllVessels(string tag)
        {
            string result = await GetAllAPI("VesselReq", tag);
            return JsonConvert.DeserializeObject<IEnumerable<Vessel>>(result);
        }
        public static async Task<IEnumerable<Worker>> GetAllWorkers(string tag)
        {
            string result = await GetAllAPI("WorkerReq", tag);
            return JsonConvert.DeserializeObject<IEnumerable<Worker>>(result);
        }
        public static async Task<IEnumerable<EntryLog>> GetAllEntryLogs(string tag)
        {
            string result = await GetAllAPI("EntryLogReq", tag);
            return JsonConvert.DeserializeObject<IEnumerable<EntryLog>>(result);           
        }
        public static async Task<IEnumerable<AnalyticsLog>> GetAllAnalyticsLogs(string tag)
        {
            string result = await GetAllAPI("RecordLogReq", tag);
            return JsonConvert.DeserializeObject<IEnumerable<AnalyticsLog>>(result);
        }

        //Database Table Querys - Error Handler
        static public async Task<string> GetAllAPI(string URIsuffix, string tag)
        {
            try
            {
                return await Globals.client.GetStringAsync(Url + "DataRequest/" + URIsuffix + "/" + tag);
            }
            catch (Exception ex)
            {
                var test = ex;
                if(ex.Message == "temp")
                {
                    return "1";
                }
                return "2";
            }
        }

/*        static async public Task<int> Add(Tuple <Unit,Vessel> newdata, string tag)
        {
            var response = await Globals.client.PostAsync(Url + "units/" + tag, new StringContent(JsonConvert.SerializeObject(newdata), Encoding.UTF8, "application/json"));
            int newID = (int)JsonConvert.DeserializeObject<int>(response.Content.ReadAsStringAsync().Result);
            return newID;

        }*/

        //Add Database Entry
        static async public Task<int> AddUnit(Unit newdata)
        {
            string payload = JsonConvert.SerializeObject(newdata);
            int newID = await AddAPI("PostUnit", payload);
            return newID;
        }
        static async public Task<int> AddVessel(Vessel newdata)
        {
            string payload = JsonConvert.SerializeObject(newdata);
            int newID = await AddAPI("PostVessel", payload);
            return newID;
        }
        static async public Task<int> AddWorker(Worker newdata)
        {
            string payload = JsonConvert.SerializeObject(newdata);
            int newID = await AddAPI("PostWorker", payload);
            return newID;
        }
        static async public Task<int> AddLog(EntryLog newdata)
        {
            string payload = JsonConvert.SerializeObject(newdata);
            int newID = await AddAPI("PostLog", payload);
            return newID;
        }
        static async public Task<int> AddRecord(AnalyticsLog newdata)
        {
            string payload = JsonConvert.SerializeObject(newdata);
            int newID = await AddAPI("PostRecord", payload);
            return newID;
        }
        static public async Task<int> DeactivateWorker(string NFCtag)
        {
            string payload = JsonConvert.SerializeObject(NFCtag);
            int newID = await AddAPI("DeactivateWorker", payload);
            return newID;
        }

        //Add Database Entry - Error Handler
        static public async Task<int> AddAPI(string URIsuffix, string payload)
        {  
            try
            {
                var response = await Globals.client.PostAsync(Url + "units/" + URIsuffix + "/", new StringContent(payload, Encoding.UTF8, "application/json"));
                int newID = (int)JsonConvert.DeserializeObject<int>(response.Content.ReadAsStringAsync().Result);
                return newID;
            }
            catch (Exception ex)
            {
                
                var test = ex;
                var response = await Globals.client.PostAsync(Url + "units/" + URIsuffix + "/", new StringContent(payload, Encoding.UTF8, "application/json"));

                return -6;
            }
        }


        static async public Task<int> SyncLogs(List<AnalyticsLog> analyticslogs)
        {
            var response = await Globals.client.PostAsync(Url + "units/SyncRecords/", new StringContent(JsonConvert.SerializeObject(analyticslogs), Encoding.UTF8, "application/json"));
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
