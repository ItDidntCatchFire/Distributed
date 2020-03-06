using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;

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
            new Uri($"{Domain}/{Route.Replace("[Controller]", Controller).Replace("[Action]", Action)}/");


        public class Paramater
        {
            public string Name { get; set; }
            public string From { get; set; }
        }
    }

    class Client
    {
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
                //Make sure there is a space
                //Are they calling a method in the API?
                Api api;
                if (input.Contains(' ') &&
                    ((api = apis.Where(x => x.Controller.ToUpper().ToString() == input.Split(' ')[0].ToUpper())
                        .FirstOrDefault(x => x.Action.ToUpper().ToString() == input.Split(' ')[1].ToUpper())) != null))
                {
                    try
                    {
                        //They are calling an API method
                        Console.WriteLine(api.Url);
                        foreach (var param in api.Paramaters)
                            Console.WriteLine($"\t{param.Name}");

                        var client = new HttpClient
                        {
                            BaseAddress = api.Url
                        };

                        Console.WriteLine($"!{api.Http}!");
                    
                        Console.WriteLine("...please wait...");
                        HttpResponseMessage result;
                        switch (api.Http)
                        {
                            case "[Get]":
                                Console.WriteLine("1");
                                result = client.GetAsync(api.Url).Result;
                                Console.WriteLine("1.5");
                                break;
                            case "[POST]":
                                Console.WriteLine("2");
                                result = await client.PostAsync("", new StringContent(""));
                                break;
                            case "[Delete]":
                                Console.WriteLine("3");
                                result = await client.DeleteAsync("");
                                break;
                            default:
                                Console.WriteLine("4");
                                result = await client.GetAsync("");
                                Console.WriteLine($"Unknown Type: {api.Http}");
                                Console.ReadKey(true);
                                Environment.Exit(0);
                                break;
                        }

                        Console.WriteLine("Hey");
                        if (result.IsSuccessStatusCode)
                        {
                            Console.WriteLine(result.Content);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        throw;
                    }
                   
                }

                Console.WriteLine("What would you like to do next?");
                input = Console.ReadLine();
                Console.Clear();
            }
            Console.WriteLine("Over Here");
            Environment.Exit(0);
        }
    }

    #endregion
}