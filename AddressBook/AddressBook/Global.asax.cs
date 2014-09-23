using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using RhoconnectNET;
using RhoconnectNET.Controllers;
using System.Collections;

namespace AddressBook
{
    public class MvcApplication : System.Web.HttpApplication
    {

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Address", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            init_rhoconnect();
        }

        // implement init_rhoconnect() method to establish
        // communication link between `Rhoconnect` server
        // and ASP.NET MVC application
        private void init_rhoconnect()
        {
            // this call allows parsing JSON structures into Objects
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());

            RhoconnectNET.Client.set_app_endpoint("http://localhost:9292",
                                                  "http://localhost:3000",
                                                  "my-rhoconnect-token",
                                                  rhoconnect_authenticate);
        }

        private bool rhoconnect_authenticate(ref String username, String password, Hashtable auth_attrs)
        {
            return true;
        }
    }
}
