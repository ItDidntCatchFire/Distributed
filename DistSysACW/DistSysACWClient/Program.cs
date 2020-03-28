using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CoreExtensions;
using DistSysACW.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DistSysACWClient
{
    #region Task 10 and beyond

    class Api
    {
        private string _route;

        public string Route
        {
            get => this._route;
            set => _route = value.Replace("\"", "");
        }

        private string _controller;

        public string Controller
        {
            get => this._controller;
            set => _controller = value.Replace("Controller", "");
        }

        public string Domain { get; set; }
        public string Action { get; set; }
        public string Http { get; set; }

        public bool Authorized { get; set; } = false;

        public List<Paramater> Paramaters { get; set; }

        public System.Uri Url =>
            new Uri($"{Domain}/{Route.Replace("[Controller]", Controller).Replace("[Action]", Action).TrimStart('/')}");


        public class Paramater
        {
            public string Name { get; set; }
            public string From { get; set; }
            public string Type { get; set; }
        }
    }

    class Client
    {

        private static string userName = "";
        private static string apiKey = "";
        private static string publicKey = "";
        private static string getQuery = "", postQuery = "";
        private static object postObject;
        private static string retunedData = "";
        private static bool isSuccess = false;
        private static void Main(string[] args)
        {
            var apis = GetApis();

            Console.WriteLine("Hello. What would you like to do?");
            var input = "" + Console.ReadLine();
            DoTheThing(apis, input);
        }

        private static List<Api> GetApis()
        {
            const string routeAttribute = "Microsoft.AspNetCore.Mvc.RouteAttribute";
            const string actionAttribute = "Microsoft.AspNetCore.Mvc.ActionNameAttribute";
            const string httpAttribute = "Microsoft.AspNetCore.Mvc.Http";
            const string fromAttribute = "Microsoft.AspNetCore.Mvc.From";
            const string authAttribute = "Microsoft.AspNetCore.Authorization.AuthorizeAttribute";

            var endpoints = new List<Api>();
            var name = AssemblyName.GetAssemblyName(@".\DistSysACW.dll");

            foreach (var myClass in Assembly.Load(name).GetTypes()
                .Where(t => String.Equals(t.Namespace, "DistSysACW.Controllers", StringComparison.Ordinal))
                .Where(t => t.IsClass)
                .Where(t => ((t.Attributes & TypeAttributes.NestedPrivate) != TypeAttributes.NestedPrivate))
                .ToArray())
            {
                //Get the Route for that controller of from its parent
                var route = $"";
                if (myClass.CustomAttributes.Count(t => t.ToString().Contains(routeAttribute)) == 1)
                    route = myClass.CustomAttributes.First().ConstructorArguments.First().ToString();
                else if (myClass.BaseType != null &&
                         myClass.BaseType.CustomAttributes.Count(t => t.ToString().Contains(routeAttribute)) == 1)
                    route = myClass.BaseType.CustomAttributes.First().ConstructorArguments.First().ToString();

                //Get all of our methods
                foreach (var method in myClass.GetTypeInfo().DeclaredMethods)
                {
                    var api = new Api()
                    {
                        Route = route,
                        Controller = myClass.Name,
                        Domain = "http://localhost:5000"
                    };

                    //Get the data from the method attributes
                    foreach (var attribute in method.CustomAttributes)
                    {
                        if (attribute.ToString().Contains(routeAttribute))
                            api.Route = attribute.ConstructorArguments.First().ToString();
                        else if (attribute.ToString().Contains(actionAttribute))
                            api.Action = attribute.ConstructorArguments.First().ToString().Replace("\"", "");
                        else if (attribute.ToString().Contains(httpAttribute))
                            api.Http = attribute.ToString().Replace(httpAttribute, "").Replace("Attribute()", "");
                        else if (attribute.ToString().Contains(authAttribute))
                            api.Authorized = true;
                    }
                    
                    //Do the parameters
                    api.Paramaters = new List<Api.Paramater>();
                    foreach (var parameter in method.GetParameters())
                        api.Paramaters.Add(new Api.Paramater()
                        {
                            Name = parameter.Name,
                            Type = parameter.ParameterType.ToString(),
                            From = parameter.CustomAttributes.FirstOrDefault()?.ToString().Replace(fromAttribute, "")
                                .Replace("Attribute()", "")
                        });

                    endpoints.Add(api);
                }
            }

            return endpoints;
        }

        private static async void DoTheThing(IEnumerable<Api> apis, string input = "")
        {
            while (input != "Exit")
            {
                Api api = null;
                getQuery = "";
                postQuery = "";
                postObject = null;
                retunedData = "";
                isSuccess = false;
                
                if (input.Contains(' ')) //Check for a space so we don't IndexOutOfRange
                {
                    //Lazily set the api whilst checking if it is valid
                    //This looks messy but works kinda well
                    //Filter out all of the options that the reflection could not do beforehand then set the api object up
                    //ToUpper everything as users are bad
                    
                    if (//USER ROLE CHANGE
                        input.Split(' ')[0].ToUpper() == "USER" && input.Split(' ')[1].ToUpper() == "ROLE"
                        && (api = apis.Where(x => x.Controller.ToUpper().ToString() == input.Split(' ')[0].ToUpper())
                            .FirstOrDefault(x => x.Action.ToUpper().ToString() == "CHANGEROLE")) != null
                        ||
                            //PROTECTED HELLO
                        input.Split(' ')[0].ToUpper() == "PROTECTED" && input.Split(' ')[1].ToUpper() == "HELLO"
                        && (api = apis.Where(x => x.Controller.ToUpper().ToString() == "USER")
                            .FirstOrDefault(x => x.Action.ToUpper().ToString() == input.Split(' ')[1].ToUpper())) != null
                        ||
                            //PROTECTED GET PUBLIC KEY
                        (input.Split(' ')[0].ToUpper() == "PROTECTED" && input.Split(' ')[1].ToUpper() == "GET" && input.Split(' ')[2].ToUpper() == "PUBLICKEY")
                        && (api = apis.Where(x => x.Controller.ToUpper().ToString() == input.Split(' ')[0].ToUpper())
                            .FirstOrDefault(x => x.Action.ToUpper().ToString() == "GETPUBLICKEY")) != null
                        ||
                            //SEARCH the reflected API
                        (api = apis.Where(x => x.Controller.ToUpper().ToString() == input.Split(' ')[0].ToUpper())
                            .FirstOrDefault(x => x.Action.ToUpper().ToString() == input.Split(' ')[1].ToUpper())) != null
                        || (api = apis.Where(x => x.Controller.ToUpper().ToString() == input.Split(' ')[0].ToUpper())
                            .FirstOrDefault(x => x.Http.ToUpper().Contains(input.Split(' ')[1].ToUpper()))) != null
                        )
                        //Remove the command from the input string
                        input = input.Replace($"{input.Split(' ')[0]} {input.Split(' ')[1]}", "").Trim();

                    if (input.Split(' ')[0].ToUpper() == "USER" && input.Split(' ')[1].ToUpper() == "SET")
                    {
                        if (input.Split(' ').Length == 4)
                        {
                            userName = input.Split(' ')[2];
                            apiKey = input.Split(' ')[3];
                            Console.WriteLine("Stored");    
                        }
                    }
                    
                    try
                    {
                        //If we have been correctly able to reflect the api out
                        if (api != null)
                        {
                            foreach (var param in api.Paramaters)
                            {
                                if (param.Type.Contains("[]") && param.From == "[Query]")
                                {
                                    //[1,2,3,4,5]
                                    var array = input.Split(' ')[0];
                                    input = input.Replace(array, " ");

                                    foreach (var item in array.Split(','))
                                    {
                                        var fixedItem = item.Replace("[", "").Replace("]", "");
                                        getQuery += param.Name + "=" + fixedItem + "&";
                                    }
                                }
                                else if (param.From == "[Body]" && param.Type == "DistSysACW.Models.User")
                                {
                                    var uName = input.Split(' ')[0];
                                    var role = input.Split(' ')[1];

                                    var user = new DistSysACW.Models.User()
                                    {
                                        UserName = uName,
                                        Role = User.Roles.User
                                    };

                                    //Convert to an object so we can serialize generally
                                    postObject = user;
                                }
                                else
                                {
                                    //delete breaks
                                    var item = input.Split(' ')[0];
                                    if (param.From == "[Query]" && !String.IsNullOrEmpty(item))
                                    {
                                        input = input.Replace(item, " ");
                                        getQuery += param.Name + "=" + item + "&";
                                    }
                                    else if (param.From == "[Body]")
                                        postQuery = item;
                                    
                                    //Convert to an object so we can serialize generally
                                    postObject = postQuery;
                                }
                            }
                        
                            var client = new HttpClient
                            {
                                BaseAddress = api.Url
                            };
                            
                            //If we should have an apiKey but don't
                            if (api.Authorized && apiKey == "")
                                retunedData = "You need to do a User Post or User Set first";
                            else
                            {
                                //If we should need an ApiKey then add it
                                if (api.Authorized)
                                    client.DefaultRequestHeaders.Add("ApiKey", apiKey);

                                Console.WriteLine("...please wait...");
                                Task<HttpResponseMessage> result;
                                switch (api.Http.ToUpper())
                                {
                                    case "[GET]":
                                        result = client.GetAsync(api.Url + "?" + getQuery.TrimEnd('&'));
                                        break;
                                    case "[POST]":
                                        result = client.PostAsJsonAsync("", postObject);
                                        break;
                                    case "[DELETE]":
                                        result = client.DeleteAsync($"{api.Url}?{getQuery}{userName}");
                                        break;
                                    default:
                                        result = client.GetAsync("");
                                        Console.WriteLine($"Unknown Type: {api.Http}");
                                        Console.ReadKey(true);
                                        Environment.Exit(0);
                                        break;
                                }

                                var temp = result.Result;
                                isSuccess = temp.IsSuccessStatusCode;
                                retunedData = await temp.Content.ReadAsStringAsync();
                            }    
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.ReadLine();
                    }
                    if (api != null)
                        if (isSuccess && api.Controller == "User" && api.Http == "[Post]" && api.Action == "new")
                        {
                            userName = postQuery;
                            apiKey = retunedData;
                            Console.WriteLine("Got API Key");
                        }
                        else if (api.Controller == "Protected" && api.Http == "[Get]" && api.Action == "GetPublicKey")
                        {
                            if (isSuccess)
                            {
                                Console.WriteLine("Got Public Key");
                                publicKey = retunedData;
                            }
                            else
                                Console.WriteLine("Couldn't Get the Public Key");
                        }
                        else if (isSuccess && api.Controller == "Protected" && api.Http == "[Get]" && api.Action == "Sign")
                        {
                            if (publicKey == "")
                                Console.WriteLine("Client doesn’t yet have the public key");
                            else
                            {
                                //message=Hello&
                                var message = getQuery.Replace("message=", "").TrimEnd('&');
                                var byteConverter = new ASCIIEncoding();
                                var originalData = byteConverter.GetBytes(message);
                                
                                var hexReturned = StringToByteArray(retunedData.Replace("-",""));
                                
                                var cspAsymmetric = new RSACryptoServiceProvider();
                                cspAsymmetric.FromXmlStringCore22(publicKey);
                                var correct = cspAsymmetric.VerifyData(originalData,CryptoConfig.MapNameToOID("SHA1"), hexReturned);

                                Console.WriteLine(correct
                                    ? "Message was successfully signed"
                                    : "Message was not successfully signed");
                            }
                        }
                        else
                            Console.WriteLine(retunedData);
                }

                Console.WriteLine("What would you like to do next?");
                input = Console.ReadLine();
                Console.Clear();
            }
            Environment.Exit(0);
        }
        
        public static byte[] StringToByteArray(string hex)
        {
            var NumberChars = hex.Length;
            var bytes = new byte[NumberChars / 2];
            for (var i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
    #endregion
}