// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Schema;
using SharedBaton.Firebase;
using SharedBaton.Commands;
using DevEnvironmentBot.Cards;
using SharedBaton.BatonServices;
using SharedBaton.Card;
using SharedBaton.CommandHandlers;
using SharedBaton.Interfaces;

namespace BatonBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create a global hashset for our ConversationReferences
            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, Bots.BatonBot>();

            services.AddSingleton<IFirebaseService, FirebaseService>();
            services.AddSingleton<ICardCreator, Card>();
            services.AddSingleton<ITakeCommandHandler, TakeCommandHandler>();
            services.AddSingleton<IReleaseCommandHandler, ReleaseCommandHandler>();
            services.AddSingleton<IAdminReleaseCommandHandler, AdminReleaseCommandHandler>();
            services.AddSingleton<IShowCommandHandler, ShowCommandHandler>();
            services.AddSingleton<ICommandHandler, CommandHandler>(); 
            services.AddSingleton<IBatonService, BatonService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}
