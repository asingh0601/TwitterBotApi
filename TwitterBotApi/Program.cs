using Hangfire;
using Hangfire.SqlServer;
using Serilog;
using TwitterBotApi.Helpers;
using TwitterBotApi.Repos;
using TwitterBotApi.Services;

namespace TwitterBotApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			var env = builder.Environment.EnvironmentName;
			IConfigurationBuilder config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", true)
				.AddJsonFile($"appsettings.{env}.json", true, true)
				.AddEnvironmentVariables();

			builder.Services.AddRouting().AddControllers().AddNewtonsoftJson();
			builder.Services.AddDbContext<BotDbContext>(ServiceLifetime.Transient);
			builder.Services.AddScoped<IProcessHelper, ProcessHelper>();
			builder.Services.AddScoped<IBotRepo, BotRepo>();
			builder.Services.AddScoped<IBotHelper, BotHelper>();
			builder.Services.AddScoped<ITelegramHelper, TelegramHelper>();
			builder.Services.AddScoped<IArgumentHelper, ArgumentHelper>();
			builder.Services.AddScoped<IBotService, BotService>();
			builder.Services.AddScoped<IRecurringTasks, RecurringTasks>();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			builder.Services.AddHangfire(configuration => configuration
				   .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
				   .UseSimpleAssemblyNameTypeSerializer()
				   .UseRecommendedSerializerSettings()
				   .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
				   {
					   CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
					   SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
					   QueuePollInterval = TimeSpan.Zero,
					   UseRecommendedIsolationLevel = true,
					   DisableGlobalLocks = true
				   })
				   .UseSerilogLogProvider()
				   .UseColouredConsoleLogProvider());
			builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
			builder.Services.AddHangfireServer();
			var app = builder.Build();
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}
			// Configure the HTTP request pipeline.

			app.UseHttpsRedirection();
			app.UseHangfireDashboard("/hangfire", new DashboardOptions
			{
				Authorization = new[] { new HangfireAuthorizationFilter() }
			});
			app.UseAuthorization();
			app.UseSerilogRequestLogging();
			app.UseRouting();
			app.MapControllers();
			StartRecurringJobs();
			app.Run();
		}

		private static void StartRecurringJobs()
		{
			RecurringJob.AddOrUpdate<IRecurringTasks>("kill_orphan_process", x => x.KillOrphanProcesses(), "*/10 * * * *");
			RecurringJob.AddOrUpdate<IRecurringTasks>("kill_old_process", x => x.KillOldProcesses(), "0 */1 * * *");
			RecurringJob.AddOrUpdate<IRecurringTasks>("db_cleanup", x => x.CleanupDB(), "0 */2 * * *");
		}
	}
}
