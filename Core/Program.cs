using Core;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Engine.Services;
using InputManager.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//--- Services ---//
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IGameLoop, GameLoop>();
builder.Services.AddSingleton<IInputHandler, InputHandler>();

// --- Builder ---//
var host = builder.Build();



await host.RunAsync();
