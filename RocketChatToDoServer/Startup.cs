using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using RocketChatToDoServer.TodoBot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RocketChatToDoServer
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
            services.AddSingleton(Configuration);
            services.AddSingleton(provider => new BotService(provider.GetService<ILogger<BotService>>(), Configuration, services));
            services.AddScoped<TaskParser.TaskParserService>();
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            // create connection string for sqlite database using the appsettings.json section database
            string connection = Configuration.GetSection("database").GetChildren()
                .Select(x => x.Key + "=" + x.Value)
                .Aggregate((a, b) => a + ";" + b);
            services.AddDbContext<TaskContext>
                (options => options.UseSqlite(connection));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddScoped(svcs => new Database.TaskParser(svcs.GetService<ILogger<Database.TaskParser>>(),
                    (username, logger) =>
                    {
                        // no using here, as the context is managed by the di scope
                        TaskContext ct = svcs.GetService<TaskContext>();
                        User user = ct.Users.FirstOrDefault(u => u.Name == username);
                        if (user == null)
                        {
                            user = ct.Users.Add(new User()
                            {
                                Name = username
                            }).Entity;
                            ct.SaveChanges();
                        }
                        return user;
                    }, (User owner, IEnumerable<User> assignees, string taskDescription, ILogger logger, DateTime? dueDate) =>
                    {
                        // no using here, as the context is managed by the di scope
                        TaskContext tc = svcs.GetService<TaskContext>();
                        Task t = tc.Tasks.Add(new Task()
                        {
                            CreationDate = DateTime.Now,
                            DueDate = dueDate ?? default,
                            Title = taskDescription.Trim(),
                            Initiator = owner
                        }).Entity;
                        tc.SaveChanges();
                        tc.UserTaskMaps.AddRange(assignees.Select(a => new UserTaskMap
                        {
                            TaskID = t.ID,
                            UserID = a.ID
                        }));
                        tc.SaveChanges();
                        return t;
                    }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Connect Bot
            app.ApplicationServices.GetService<BotService>().LoginAsync();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
