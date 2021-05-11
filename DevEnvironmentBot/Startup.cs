// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.11.1

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DevEnvironmentBot.Bots;
using SharedBaton.Firebase;
using SharedBaton.Card;
using DevEnvironmentBot.Cards;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using SharedBaton.BatonServices;
using SharedBaton.CommandHandlers;
using SharedBaton.Commands;
using SharedBaton.Interfaces;

namespace DevEnvironmentBot
{
    using SharedBaton.GitHubService;

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
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create a global hashset for our ConversationReferences
            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DevBot>();

            services.AddSingleton<ICardCreator, Card>();

            services.AddSingleton<IFirebaseService, FirebaseService>(); 
            services.AddSingleton<IBatonService, BatonService>();

            services.AddSingleton<ICommandHandler, CommandHandler>();
            services.AddSingleton<IShowCommandHandler, ShowCommandHandler>();
            services.AddSingleton<ITakeCommandHandler, TakeCommandHandler>();
            services.AddSingleton<IReleaseCommandHandler, ReleaseCommandHandler>();
            services.AddSingleton<IAdminReleaseCommandHandler, AdminReleaseCommandHandler>();
            services.AddSingleton<IMoveMeCommandHandler, MoveMeCommandHandler>();
            services.AddSingleton<IGithubUpdateHandler, UpdateGithubHandler>();
            services.AddSingleton<IGitHubService, GitHubService>();
            services.AddSingleton<IFirebaseLogger, FirebaseLogger>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
