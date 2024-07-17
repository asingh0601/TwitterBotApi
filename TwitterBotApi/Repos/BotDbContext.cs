using Microsoft.EntityFrameworkCore;
using TwitterBotApi.Models;

namespace TwitterBotApi.Repos
{
	public class BotDbContext(IConfiguration configuration) : DbContext
	{
		protected readonly IConfiguration Configuration = configuration;

		public DbSet<AuthorisedUser> AuthorisedUsers { get; set; }
		public DbSet<ProcessedUpdate> ProcessedUpdates { get; set; }
		public DbSet<BotDetails> BotDetails { get; set; }

		public string ConnectionString
		{
			get
			{
				return Configuration.GetConnectionString("DbConnectionString") ?? string.Empty;
			}
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DblConnectionString"), options => options.EnableRetryOnFailure());
		}
	}
}