public static partial class Program
{
    static partial void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMyTestService, MyTestService>();
        services.AddSingleton(new Action<IScriptHost>(
                                host => host.Task("IOC-Task")
                                            .IsDependeeOf("Clean")
                                            .Does(() => Information("Hello from IOC-Task"))));
    }
}