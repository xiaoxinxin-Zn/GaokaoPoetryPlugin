using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using GaokaoPoetryPlugin.Components;
using GaokaoPoetryPlugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GaokaoPoetryPlugin;

[PluginEntrance]
public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        // 注册服务
        services.AddSingleton<PoetryService>();

        // 注册组件
        services.AddComponent<PoetryComponent, PoetryComponentSettingsControl>();
    }
}
