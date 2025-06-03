using GoogleLimitedInputAuth.Net.UsageExample;
using GoogleLimitedInputAuth.Net.UsageExample.options;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IOptions<GApiOptions>>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var options = new GApiOptions();
    configuration.Bind(GApiOptions.SectionName, options);
    return Options.Create(options);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();