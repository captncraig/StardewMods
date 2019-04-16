using Nancy;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;
using Nancy.ViewEngines;
using System;
using System.IO;
using Nancy.ViewEngines.Razor;
using System.Collections.Generic;
using System.Net.Http;
using StardewModdingAPI;
using System.Linq;

namespace Graph.Farm.Collector
{
    public class HttpServer
    {
        public HttpServer(string addr)
        {
            var hc = new HostConfiguration {
                UrlReservations = new UrlReservations {
                    CreateAutomatically = true
                }
            };
            var host = new NancyHost(hc, new Uri(addr));
            
            host.Start();
            ModEntry.Instance.Monitor.Log($"Started stats ui at {addr}");
        }
    }

    public class CustomRootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            var path = ModEntry.Instance.config.ViewRoot ?? "";
            return path == "" ? ModEntry.Instance.Helper.DirectoryPath: path;
        }
    }

    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override IRootPathProvider RootPathProvider
        {
            get { return new CustomRootPathProvider(); }
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register<IViewEngine, RazorViewEngine>().AsSingleton();
        }
    }

    public class StatsModule : NancyModule
    {
        public StatsModule()
        {
            Get["/"] = Home;
            Get["/{game}/{view}"] = Dashboard;
            Get["/{game}/api/ts"] = TimeSeries;
        }

        public dynamic Home(dynamic _)
        {
            // TODO: filter to ones that have a db
            var saves = Directory.GetDirectories(Constants.SavesPath).Select(x => Path.GetFileName(x));
            return View["home.cshtml", saves];
        }

        public dynamic Dashboard(dynamic p)
        { 
            return View["dash.cshtml"];
        }

        public dynamic TimeSeries(dynamic p)
        {
           
            var metric = Request.Query.metric;
            if (string.IsNullOrEmpty(metric))
            {
                return ((Response)"Need to supply metric").WithStatusCode(400);
            }
            var db = new Database(Path.Combine(Constants.SavesPath, p.game), true);
            var data = db.GetTimeSeries(metric);
            return data;
        }
        
    }
}
