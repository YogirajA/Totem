using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Totem.Infrastructure;

namespace Totem.Tests
{
    public static class Testing
    {
        private static readonly IServiceScopeFactory ScopeFactory;
        public static IConfigurationRoot Configuration { get; }

        static Testing()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            Configuration = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables(Program.ApplicationName + ":")
                .Build();

            var startup = new Startup(Configuration);
            var services = new ServiceCollection();
            startup.ConfigureServices(services);

            var rootContainer = services.BuildServiceProvider();
            ScopeFactory = rootContainer.GetService<IServiceScopeFactory>();
            RemoveDataFromTestDatabase();
        }

        public static async Task Send(IRequest message)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var database = serviceProvider.GetService<TotemContext>();

                var validator = Validator(serviceProvider, message);

                if (validator != null)
                {
                    var validationResult = await validator.ValidateAsync(message);
                    validationResult.ShouldBeSuccessful();
                }

                await serviceProvider.GetService<IMediator>().Send(message);
            }
        }

        public static async Task<TResponse> Send<TResponse>(IRequest<TResponse> message)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var database = serviceProvider.GetService<TotemContext>();

                var validator = Validator(serviceProvider, message);

                if (validator != null)
                {
                    var validationResult = await validator.ValidateAsync(message);
                    validationResult.ShouldBeSuccessful();
                }

                var response = await serviceProvider.GetService<IMediator>().Send(message);

                return response;
            }
        }

        public static AutoMapper.IConfigurationProvider MapperConfiguration()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                return scope.ServiceProvider.GetService<AutoMapper.IConfigurationProvider>();
            }
        }

        public static string Json(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented);
        }

        public static T ConvertJsonToObject<T>(T value)
        {
            return JsonConvert.DeserializeObject<T>(Json(value));
        }

        public static ValidationResult Validation<TResult>(IRequest<TResult> message)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var database = serviceProvider.GetService<TotemContext>();

                var validator = Validator(serviceProvider, message);

                if (validator == null)
                    throw new Exception($"There is no validator for {message.GetType()} messages.");

                var validationResult = validator.Validate(message);

                return validationResult;
            }
        }

        private static IValidator Validator<TResult>(IServiceProvider serviceProvider, IRequest<TResult> message)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(message.GetType());
            return serviceProvider.GetService(validatorType) as IValidator;
        }

        public static void Transaction(Action<TotemContext> action)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<TotemContext>();

                action(database);
            }
        }

        public static TEntity Query<TEntity>(params object[] keyValues) where TEntity : class
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<TotemContext>();
                var entity = database.Set<TEntity>();

                var response = entity.Find(keyValues);

                return response;
            }
        }

        public static List<TEntity> Query<TEntity>() where TEntity : class
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<TotemContext>();
                var entity = database.Set<TEntity>();

                var response = entity.ToList();

                return response;
            }
        }

        public static int CountRecords<TEntity>() where TEntity : class
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<TotemContext>();
                var entity = database.Set<TEntity>();

                var response = entity.Count();

                return response;
            }
        }

        public static void RemoveDataFromTestDatabase()
        {
            Transaction(database =>
            {
                database.Database.ExecuteSqlCommand("DELETE FROM [Contract]");
                database.Database.ExecuteSqlCommand("DELETE FROM [ContractSchema] WHERE [SchemaName] NOT IN ('Guid','String','Integer','Integer(Int32)','Integer(Int64)','DateTime')");
            });
        }
    }
}
