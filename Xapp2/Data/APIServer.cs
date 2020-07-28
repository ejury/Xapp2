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

namespace Xapp2.Data
{   
    class APIServer
    {
        const string Url = "http://192.168.1.42:5555/api/";
        //const string Url = "https://localhost:44303/api/";
        //const string Url = "http://localhost:59438/api/";


/*        static public async Task GetClient(SELogin tempLogin)
        {
            //HttpClient client = new HttpClient();
            if (string.IsNullOrEmpty(Globals.AKey))
            {
                //Globals.AKey = await Globals.client.GetStringAsync(Url + "units/login", new StringContent(JsonConvert.SerializeObject(tempLogin), Encoding.UTF8, "application/json"));
                //Globals.client.DefaultRequestHeaders.Add("UN", tempLogin.Username);
                //Globals.client.DefaultRequestHeaders.Add("PW", tempLogin.Password);

                Globals.AKey = await Globals.client.GetStringAsync(Url + "units/login");
                Globals.AKey = JsonConvert.DeserializeObject<string>(Globals.AKey);
                Console.WriteLine(Globals.AKey);

                Globals.client.DefaultRequestHeaders.Add("Authorization", Globals.AKey);
                Globals.client.DefaultRequestHeaders.Add("Accept", "application/json");
                
            }


            return ;
        }*/

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
        static public async Task<string> SEClient(Worker tempWorker)
        {
            var response = await Globals.client.PostAsync(Url + "Account/SEBadge/", new StringContent(JsonConvert.SerializeObject(tempWorker), Encoding.UTF8, "application/json"));
            string content = await response.Content.ReadAsStringAsync();
            var newcontent = JsonConvert.DeserializeObject<string>(content);


            //Error Handler
            if (newcontent == "Card Not Active")
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

            Globals.JWTkey = newcontent;
            var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", Globals.JWTkey);
            Globals.client.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            return "Complete";
        }

        public static async Task<IEnumerable<Unit>> GetAllUnits()
        {

            string result = await Globals.client.GetStringAsync(Url + "DataRequest/UnitReq/");
            return JsonConvert.DeserializeObject<IEnumerable<Unit>>(result);
        }
        public static async Task<IEnumerable<Vessel>> GetAllVessels()
        {

            string result = await Globals.client.GetStringAsync(Url + "DataRequest/VesselReq/");
            return JsonConvert.DeserializeObject<IEnumerable<Vessel>>(result);
        }
        public static async Task<IEnumerable<Worker>> GetAllWorkers()
        {

            string result = await Globals.client.GetStringAsync(Url + "DataRequest/WorkerReq/");
            return JsonConvert.DeserializeObject<IEnumerable<Worker>>(result);
        }

        static async public Task Add(Tuple <Unit,Vessel,Worker> newdata, string tag)
        {

            var response = await Globals.client.PostAsync(Url+"units/" + tag, new StringContent(JsonConvert.SerializeObject(newdata), Encoding.UTF8, "application/json"));
            return;
        }
        static async public Task AddWorker(Worker newdata)
        {

            var response = await Globals.client.PostAsync(Url + "units/Worker/", new StringContent(JsonConvert.SerializeObject(newdata), Encoding.UTF8, "application/json"));
            return;
        }


        static public async Task Delete(string tag)
        {
            var response = await Globals.client.DeleteAsync(Url + "units/" + tag);

            return;
        
        }

        public async Task Update(Unit unit)
        {
            await Globals.client.PutAsync(Url + "/" + unit.UnitID, new StringContent(JsonConvert.SerializeObject(unit), Encoding.UTF8, "application/json"));
        }
    }
}
