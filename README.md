# Magic Home Light SDK for .NET 
[![Build Status](https://dev.azure.com/ginomessmer/MagicHome.NET/_apis/build/status/ginomessmer.magic-home-dotnet?branchName=master)](https://dev.azure.com/ginomessmer/MagicHome.NET/_build/latest?definitionId=4&branchName=master)
![https://www.nuget.org/packages/MagicHome.NET](https://img.shields.io/nuget/v/MagicHome.NET)

This unofficial library lets you control Magic Home enabled lights that are connected to the same local area network.

## Requirements
- .NET Standard 2.0 or above
- Magic Home enabled smart home light device that runs on LEDENET

## Features
- Connect to lights in network
- Read properties of light
- Set various properties of light such as power state, color
- Easy to use

## Install
Get it from [https://www.nuget.org/packages/MagicHome.NET](Nuget) or with your favorite CLI tool:

```ps
Install-Package MagicHome.NET
```
```sh
dotnet add package MagicHome.NET
```


## Show me the code
```cs
var light = new Light();
await light.ConnectAsync("192.168.0.10");

Console.WriteLine(light.Color);

await light.TurnOnAsync();

await light.SetColorAsync(Color.Red);
await Task.Delay(500);

await light.SetColorAsync(255, 255, 255);
await Task.Delay(500);

await light.RestoreAsync();
light.Dispose();
```

### Credit where credit is due
This project was heavily inspired by
- [nathanielxd/magic-home](https://github.com/nathanielxd/magic-home)
- [magic-home @ npm](https://www.npmjs.com/package/magic-home)
