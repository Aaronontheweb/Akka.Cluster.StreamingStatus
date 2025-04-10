using Akka.Cluster.Hosting;
using Akka.Remote.Hosting;
using Microsoft.Extensions.Options;

namespace Akka.Cluster.StreamingStatus;

public class AkkaSettings
{
    public RemoteOptions? RemoteOptions { get; set; } 
    public ClusterOptions? ClusterOptions { get; set; }
}

public class AkkaSettingsValidator : IValidateOptions<AkkaSettings>
{
    public ValidateOptionsResult Validate(string? name, AkkaSettings options)
    {
        var errors = new List<string>();

        if (options.RemoteOptions is null)
        {
            errors.Add("RemoteOptions must not be null.");
        }

        if (options.ClusterOptions is null)
        {
            errors.Add("ClusterOptions must not be null.");
        }
        
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}

public static class AkkaSettingsExtensions
{
    public static IServiceCollection AddAkkaSettings(this IServiceCollection services)
    {
        services.AddSingleton<IValidateOptions<AkkaSettings>, AkkaSettingsValidator>();
        services.AddOptionsWithValidateOnStart<AkkaSettings>()
            .BindConfiguration(nameof(AkkaSettings));
        return services;
    }
}