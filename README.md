# Magic Home Light SDK for .NET
This unofficial library lets you control Magic Home enabled lights that are connected to the same local area network.

## Requirements
- .NET Standard 2.1 or above
- Magic Home enabled smart home light device that runs on LEDENET

## Features
- Connect to lights in network
- Read properties of light
- Set various properties of light such as power state, color
- Easy to use

## Show me the code
```cs
await light.ConnectAsync("192.168.0.10");

await light.TurnOnAsync();

await light.SetColorAsync(Color.Red);
await Task.Delay(500);

await light.SetColorAsync(255, 255, 255);
await Task.Delay(500);

await light.RestoreAsync();
```

### Credit where credit is due
This project was heavily inspired by
- [nathanielxd/magic-home](https://github.com/nathanielxd/magic-home)
- [magic-home @ npm](https://www.npmjs.com/package/magic-home)
