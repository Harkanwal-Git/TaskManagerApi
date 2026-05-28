using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using TaskManagerApi.Consumers;
using TaskManagerApi.Data;
using TaskManagerApi.EventPublisher;
using TaskManagerApi.Interceptors;
using TaskManagerApi.Middleware;
using TaskManagerApi.OutboxWorker;
using TaskManagerApi.Producers;
using TaskManagerApi.Producers.EventPublisher;
using TaskManagerApi.Repository;
using TaskManagerApi.Service;
using TaskManagerApi.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(context.Configuration);
        });
        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddJWTAuthentication(builder.Configuration);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
 {
     options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
     {
         Name = "Authorization",
         Type = SecuritySchemeType.Http,
         Scheme = "Bearer",
         BearerFormat = "JWT",
         In = ParameterLocation.Header,
         Description = "Enter your JWT token here"
     });

     options.AddSecurityRequirement(new OpenApiSecurityRequirement
     {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
     });
 });
        builder.Services.AddControllers();  //Add controllers
        builder.Services.AddScoped<ITaskRepository, TaskRepository>();
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ITagRepository, TagRepository>();
        builder.Services.AddScoped<ITagService, TagService>();

        builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        builder.Services.AddScoped<IIdempotencyRecordRepository, IdempotencyRecordRepository>();

        builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
        builder.Services.AddScoped<IProcessedMessageRepository, ProcessedMessageRepository>();

        builder.Services.AddKeyedScoped<IEventPublisher, TaskCreatedEventPublisher>(EventType.TaskCreated);
        builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")).AddInterceptors(new UpdatesAtInterceptor()));

        builder.Services.AddSingleton<IDataConnectionFactory, SQLConnectionFactory>();

        var useRedis = builder.Configuration.GetValue<bool>("CacheSettings:UseRedis");
        if (useRedis)
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            options.Configuration = builder.Configuration.GetConnectionString("Redis"));
            builder.Services.AddSingleton<ICacheRepository, CacheRedisRepository>();
        }
        else
        {
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<ICacheRepository, CacheInMemoryRepository>();

        }
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly));

        builder.Services.AddSingleton<ITaskProducer, TaskProducer>();

        builder.Services.AddHostedService<TaskConsumer>();
        builder.Services.AddHostedService<OutboxWorker>();

        var app = builder.Build();
        app.UseSerilogRequestLogging();
        // Configure the HTTP request pipeline.
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseTimingMiddleware();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseIdempotencyMiddleware();
        app.MapControllers();
        app.Run();
    }
}


