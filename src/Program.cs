using System.Reflection;
using AppAny.HotChocolate.FluentValidation;
using HotChocolate.Types;
using Planara.Accounts.Data;
using Planara.Accounts.GraphQL;
using Planara.Accounts.Workers;
using Planara.Common.Auth.Jwt;
using Planara.Common.Configuration;
using Planara.Common.Database;
using Planara.Common.GraphQL.Filters;
using Planara.Common.Host;
using Planara.Common.Kafka;
using Planara.Common.Validators;
using Planara.Kafka.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddSettingsJson();
builder.Services
    .AddValidators(Assembly.GetExecutingAssembly())
    .AddJwtAuth(builder.Configuration)
    // .AddCors()
    .AddLogging();

builder.Services
    .AddRouting()
    .AddGraphQLServer()
    .AddErrorFilter<ErrorFilter>()
    .AddQueryType(m => m.Name(OperationTypeNames.Query))
    .AddType<Query>()
    .AddMutationType(m => m.Name(OperationTypeNames.Mutation))
    .AddType<Mutation>()
    .AddAuthorization() 
    .AddFluentValidation(options =>
    {
        options.UseInputValidators();
        options.UseDefaultErrorMapper();
    })
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = builder.Environment.IsDevelopment())
    .InitializeOnStartup();

builder.Services.AddDataContext<DataContext>(
    builder.Configuration.GetValue<string>("DbConnections:Postgres:ConnectionString")!,
    builder.Configuration.GetValue<int>("DbConnections:Postgres:MaxRetry"),
    builder.Configuration.GetValue<int>("DbConnections:Postgres:MaxDelaySec")
);

builder.Services
    .AddKafkaConsumer<UserCreatedMessage>(builder.Configuration);

builder.Services.AddScoped<KafkaConsumerWorker>();
if (!builder.Environment.IsEnvironment("Test"))
    builder.Services.AddHostedService<KafkaConsumerWorker>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();

app.PrepareAndRun<DataContext>(args);