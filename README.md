# CQRS.NET
#### https://www.nuget.org/packages/Cqrs

## The framework
A lightweight enterprise framework to help write CQRS and Eventsourcing applications in C#, with a special focus on deploying to Azure. Currently published as nuget packages @ http://www.nuget.org. It is written in C# and targets .NET 4.0, with the exception of some Azure packages which target .NET 4.5. CQRS.NET borrows heavily from CQRSlite (https://github.com/gautema/cqrslite), from some point in 2013.

CQRS.NET has been designed with modularity in mind... see the number of technology packages below you can chose from. Every package and design choice made should be interchangeable with custom code if needed.

## Tutorials and getting started
For those wanting to take a plunge into using the CQRS.NET, we strongly suggest you have a look at our basic [Northwind Tutorial](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Tutorial-0:-Quick-Northwind-sample.).

And for those looking for a more thorough tutorial we suggest you start with the [getting started](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Getting-Started) section in the [wiki](https://github.com/Chinchilla-Software-Com/CQRS/wiki)

## Documentation
Documentation is starting off for now in the [wiki section](https://github.com/Chinchilla-Software-Com/CQRS/wiki) of this project. We strongly invite people to post questions and issues which we'll answer and work on.

## Sample projects.
Several sample projects are located within the code, these show two approaches/common usage scenarios of the framework. One sample is the original sample from CQRSlite, ported to use CQRS.NET, the other is Modelled in UML and uses T4 template generation to heavily reduce the amount of code written.

The project should compile without any setup in .NET 4.0. The Azure project should compile without any setup in .NET 4.5.2. I'm guessing Mono 3.10.0 should work too.

## Commerical Support
Commercial support is now available through our partner company [Chinchilla Software](http://www.chinchillasoftware.com).

## Continual improvements
This project is actively developed, however we sometimes have specific feature requests that are outside of our roadmap and plan. We're always open to new ideas and requests for new modules and technology connectors that you need. The biggest requests in version 2.0 where to have better [akka.net](http://getakka.net) modules as well as support for Azure blob/table storage and performance/telemetry.

## Features
* Command sending and event publishing
* Unit of work through session with aggregate tracking (I'm tempted to remove the UOW)
* Repository for getting and saving aggregates and process managers
* Strategy/Specification pattern for querying
* Optimistic concurrency checking
* In process bus with autoregistration of handlers
* Azure service bus for event bus'ing
* Azure service bus for command bus'ing
* Greg Youngs EventStore for event sourcing
* MongoDB for entity, view and project persistence
* Azure DocumentDB for event sourcing, entity, view and project persistence
* SqlServer for entity, view and project persistence... Strongly don't recommend you use this though.

## Installation
See [Installation](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Installation) in the wiki.


## Projects / Nuget packages:

See [Nuget Packages](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Nuget-Packages) in the wiki.
