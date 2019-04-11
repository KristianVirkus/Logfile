# Logfile

The goal of this library is to provide a logging framework which manages all log data in a structured fashion. All information remain strongly typed and distinctively accessible throughout the whole logging pipeline until its finally written to disk or database or any other output medium. This library features a fluent syntax for assembling multiple structured information to a log event.

Please feel free to raise issues and to contribute by starting discussions and creating pull requests.

## Technology

This library requires a .NET Standard 2.0 compatible runtime. It is platform-independent.

## How to get?

The most convenient way to use this library is to get it into your .NET project using NuGet. The package name is `Logfile`. Using the package manager console, the command is:

```
PM> Install-Package Logfile
```

## Principles

This library is based on the `EventRouter` library also available via NuGet for the basic underlying event routing.

The main component of this library is the `Logfile<TLoglevel>` class which implements `Hub<LogEvent<TLoglevel>>` and `ILogfileProxy<TLoglevel>`. While the direct usage of the `Logfile<>` type is required for configuring the logging, the usage as `ILogfileProxy<>` is encouraged for actual logging tasks.

The log events themselves are instances of the `LogEvent<TLoglevel>` class. Any log event stores a loglevel of the type `TLoglevel` which may be any `Enum` type. In addition, it stores an occurrence time and all the structured `Details` the log event represents. These can be text, exceptions, binary data and anything else you can imagine.

This library itself is not capable of saving log events to the disk or perform any other output medium. It requires a log event router to be specified. The accompanying library `Logfile.Structured` implements such a log event router and is capable of writing logfiles to disk and log events to the console and debug console. It defines a textual format which is easily readable by both humans and machines.

Creating log events is done in a fluent manner. The logfile instance itself offers entry points to create new log events with a predefined loglevel (so does the class `StandardLogfile` derived from `Logfile<StandardLoglevel>`). All details of the log event are added by fluently invoking extension methods returning the same log event, e.g. `logfile.Error.Msg("Error message").Exception(ex).Go();` Using the `Go()` method, a log event gets forwarded to the configured routers, internally invoking a callback specified by the logfile instance having created the log event.

## License

This library is licensed under the MIT license. See `LICENSE` in the root directory of this repository.
