﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Matrix.Web.Controllers;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using System.Reflection;
using Matrix.Core.FrameworkCore;
using Matrix.Web.Areas.Sales.Controllers;
using Matrix.Core.MongoCore;
using System.Configuration;
using Matrix.Core.QueueCore;
using Matrix.DAL.MongoRepositoriesCustom;
using Matrix.DAL.SearchRepositoriesBase;
using Matrix.Core.SearchCore;
using Matrix.DAL.MongoRepositoriesBase;
using Matrix.Core.ConfigurationsCore;
using Matrix.Core.CacheCore;


namespace Matrix.Web
{
    public static class AutofacBootstrapper
    {
        public static void Initialise()
        {
            var builder = new ContainerBuilder();

            BuildContainer(builder);
        }

        static void BuildContainer(ContainerBuilder builder)
        {
            //Register MVC controllers first
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            //then the base repo types
            builder.RegisterType<MXBusinessMongoRepository>().As<IMXBusinessMongoRepository>();
            builder.RegisterType<MXProductCatalogMongoRepository>().As<IMXProductCatalogMongoRepository>();
            builder.RegisterType<MXConfigurationMongoRepository>().As<IMXConfigurationMongoRepository>();


            //-------------------------Named types(for my reference only, there are better ways though, look at the registrations abovea and below this block)---------
            builder.RegisterType<ClientRepository>().Named<IMXBusinessMongoRepository>("ClientRepository");
            //inject specific implementation of IRepository Interface. A better approach though is to create a separate interface as it's done with Books and then inject.
            //I'll keep this for reference purpose though.
            builder.Register(c => new ClientController(c.ResolveNamed<IMXBusinessMongoRepository>("ClientRepository")));
            //-------------------------END - Named types----------------------------------------------------

            //register rabbitMQ client as a singleton
            builder.RegisterType<MXRabbitClient>().As<IMXRabbitClient>()
                .WithParameter(new NamedParameter("connectionString", ConfigurationManager.AppSettings["rabbitMQConnectionString"]))
                .SingleInstance();
            
            //register rediscache repository; well, no need for this as a single repo here can handle mutiple databases
            //builder.RegisterType<MXRedisCacheRepository>().As<IMXCacheRepository>()
            //    .WithParameter(new NamedParameter("connectionString", ConfigurationManager.AppSettings["redisConnectionString"]))
            //    .WithParameter(new NamedParameter("dbName", MXRedisDatabaseName.FlagSettings));
            
            //Custom repos            
            builder.RegisterType<BookRepository>().As<IBookRepository>();
            builder.RegisterType<BookSearchRepository>().As<IBookSearchRepository>();
            builder.RegisterType<InitialConfigurationRepository>().As<IInitialConfigurationRepository>();
            builder.RegisterType<FlagSettingRepository>().As<IFlagSettingRepository>();


            
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
        
    }//End of class
}