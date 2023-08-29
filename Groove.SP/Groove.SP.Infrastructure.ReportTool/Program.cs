using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Groove.SP.Infrastructure.ReportTool
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        const string reportServerAddress = "http://localhost:83/";
        const string serverREStAPI = reportServerAddress + "api/reportserver/";

        static void Main(string[] args)
        {
            // Our .NET Client
            client.BaseAddress = new Uri(serverREStAPI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //log the user to allow further operations
            var userToken = LogIn("local-report-admin", "Pass@123456");

            //authorize further requests
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var reports = GetReports().Result;
            var categories = GetCategories().Result;

            var environments = new[] { "QC-SP" }; // "QC-SP", "Staging-SP", "Production-SP"
            foreach (var env in environments)
            {
                var manager = new ReportConnectionStringManager($"{env}Database", client, env, reports, categories);

                var files = DirSearch(manager.reportFolder);
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            manager.UpdateConnectionString(file);

                            Console.WriteLine($"Updated: {file}!");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Failed: {file}! Exception: {e.Message}");
                        }
                    }
                }
            }

            Console.ReadLine();
        }

        //log user
        static string LogIn(string usernameInput, string passwordInput)
        {
            var data = new FormUrlEncodedContent(new[]{
              new KeyValuePair<string, string>( "grant_type" ,"password" ),
               new KeyValuePair<string, string>( "username" , usernameInput ),
               new KeyValuePair<string, string>( "password" , passwordInput )
           });
            // "grant_type=password&username=" + usernameInput + "&password=" + passwordInput;

            HttpResponseMessage response = client.PostAsync(reportServerAddress + "Token", data).Result;
            response.EnsureSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            return result["access_token"];
        }

        static async Task<List<ReportModel>> GetReports()
        {
            var response = await client.GetAsync(Path.Combine($"{reportServerAddress}api/reportserver/", "Reports"));
            response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ReportModel>>(jsonResult);
        }

        static async Task<List<CategoryModel>> GetCategories()
        {
            var response = await client.GetAsync(Path.Combine($"{reportServerAddress}api/reportserver/", "Categories"));
            response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CategoryModel>>(jsonResult);
        }

        private static List<String> DirSearch(string sDir)
        {
            List<String> files = new List<String>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d));
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }

            return files;
        }
    }
}
