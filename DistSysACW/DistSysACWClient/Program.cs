using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
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

        private static string userName = "UserOne";
        private static string apiKey = "5cb07b45-041a-49ac-8920-b9085c25b94f";
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
                        if (attribute.ToString().Contains(routeAttribute))
                            api.Route = attribute.ConstructorArguments.First().ToString();
                        else if (attribute.ToString().Contains(actionAttribute))
                            api.Action = attribute.ConstructorArguments.First().ToString().Replace("\"", "");
                        else if (attribute.ToString().Contains(httpAttribute))
                            api.Http = attribute.ToString().Replace(httpAttribute, "").Replace("Attribute()", "");

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
                Api api;
                getQuery = "";
                postQuery = "";
                postObject = null;
                
                if (input.Contains(' ')) //Check for a space so we don't IndexOutOfRange
                {
                    if (((api = apis.Where(x => x.Controller.ToUpper().ToString() == input.Split(' ')[0].ToUpper())
                            .FirstOrDefault(x => x.Action.ToUpper().ToString() == input.Split(' ')[1].ToUpper())) != null
                        || (api = apis.Where(x => x.Controller.ToUpper().ToString() == input.Split(' ')[0].ToUpper())
                            .FirstOrDefault(x => x.Http.ToUpper().Contains(input.Split(' ')[1].ToUpper()))) != null)
                        
                        ||
                        
                        (input.Split(' ')[0].ToUpper() == "USER" && input.Split(' ')[1].ToUpper() == "SET")
                        ||
                        (input.Split(' ')[0].ToUpper() == "USER" && input.Split(' ')[1].ToUpper() == "ROLE")
                        && (api = apis.Where(x => x.Controller.ToUpper().ToString() == input.Split(' ')[0].ToUpper())
                        .FirstOrDefault(x => x.Action.ToUpper().ToString() == "CHANGEROLE")) != null
                        ||
                        (input.Split(' ')[0].ToUpper() == "PROTECTED" && input.Split(' ')[1].ToUpper() == "HELLO")
                        && (api = apis.Where(x => x.Controller.ToUpper().ToString() == "USER")
                            .FirstOrDefault(x => x.Action.ToUpper().ToString() == input.Split(' ')[1].ToUpper())) != null
                        )
                        
                    {
                        //Remove the command
                         input = input.Replace($"{input.Split(' ')[0]} {input.Split(' ')[1]}", "").Trim();

                    try
                    {
                        //If we have been correctly able to reflect the api out
                        if (api != null)
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
                        else//If we are doing User Set
                        {
                            userName = input.Split(' ')[0];
                            apiKey = input.Split(' ')[1];

                            Console.WriteLine("Stored");
                        }

                        if (api != null)
                        {
                            var client = new HttpClient
                            {
                                BaseAddress = api.Url
                            };
                            client.DefaultRequestHeaders.Add("ApiKey", apiKey);

                            Console.WriteLine("...please wait...");
                            Task<HttpResponseMessage> result;
                            switch (api.Http.ToUpper())
                            {
                                case "[GET]":
                                    result = client.GetAsync(api.Url + "?" + getQuery);
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
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.ReadLine();
                    }
                    if (isSuccess && api.Controller == "User" && api.Http == "[Post]" && api.Action == "new")
                    {
                        userName = postQuery;
                        apiKey = retunedData;
                        Console.WriteLine("Got API Key");
                    } else
                    {
                        Console.WriteLine(retunedData);
                    }
                    }
                }

                Console.WriteLine("What would you like to do next?");
                input = Console.ReadLine();
                Console.Clear();
            }
            Environment.Exit(0);
        }
    }

    #endregion
}