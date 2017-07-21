# CQRS.NET
#### https://www.nuget.org/packages/Cqrs

## The framework
A lightweight framework to help write CQRS and Eventsourcing applications in C#, with a special focus on deploying to Azure. Currently published as nuget packages @ http://www.nuget.org. It is written in C# and targets .NET 4.0 (where possible) and .NET 4.5. CQRS.NET borrows heavily from CQRSlite (https://github.com/gautema/cqrslite), from some point in 2013.

CQRS.NET has been designed with modularity in mind... see the number of technology packages below you can chose from. Every package and design choice made should be interchangeable with custom code if needed.

![CQRS.NET](https://raw.githubusercontent.com/Chinchilla-Software-Com/CQRS/master/wiki/CQRS-Process-Flow-Level-2.png)

## Tutorials and getting started
For those new to CQRS.NET, we strongly suggest you have a look at our basic [Hello World Tutorial](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Hello-World-Example-1) that covers basic commands and events.
Or you have a look at our basic [Northwind Tutorial](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Tutorial-1:-Step-1:-Quick-Northwind-sample.) which takes a look into queries, views, read stores and read models, then commands, eents aggregates and event handlers.

And for those looking for a more thorough tutorial we suggest you start with the [getting started](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Getting-Started) section in the [wiki](https://github.com/Chinchilla-Software-Com/CQRS/wiki)

## Documentation
[CQRS.NET API reference documentation](http://chinchilla-software-com.github.io/CQRS/wiki/docs) is available to browse. We are adding more and more documentation as we edit each file. By version 3.0 we aim to have all public methods and classes documented.
User documentation is starting off for now in the [wiki section](https://github.com/Chinchilla-Software-Com/CQRS/wiki) of this project. We strongly invite people to post questions and issues which we'll answer and work on.

## Sample projects.
Several sample projects are located within the code and on our [wiki](https://github.com/Chinchilla-Software-Com/CQRS/wiki/getting-started). These show a few different approaches/common usage scenarios of the framework.
* One sample is the original sample from CQRSlite, ported to use CQRS.NET - this is not documented and found within the main Visual Studio solution in our code base.
* The second is a rather basic [Hello World sample](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Hello-World-Example-1) that covers basic commands and events, but not queries. 
* The third is a more traditional mixed-mode scenario, [Northwind Tutorial](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Tutorial-1:-Step-1:-Quick-Northwind-sample.) thats starts off by replacing one query, then one create, update and delete operation out of several in an existing website. 

The project should compile without any setup in .NET 4.5.2. I'm guessing Mono 3.10.0 should work too.

## Commerical Support
Commercial support is now available through our partner company [Chinchilla Software](http://www.chinchillasoftware.com).

## Continual improvements
This project is actively developed, however we sometimes have specific feature requests that are outside of our roadmap and plan. We're always open to new ideas and requests for new modules and technology connectors that you need. The biggest requests in version 2.0 were to have better [akka.net](http://getakka.net) modules as well as support for Azure blob/table storage and performance/telemetry.

## Features
* Command sending and event publishing.
* Unit of work through session with aggregate tracking (I'm tempted to remove the UOW).
* Repository for getting and saving aggregates and process managers.
* Strategy/Specification pattern for querying.
* Optimistic concurrency checking.
* In process bus with autoregistration of handlers.
* Azure service bus/event hub for event bus'ing.
* Azure service bus/event hub for command bus'ing.
* Greg Youngs EventStore for event sourcing.
* MongoDB for entity, view and project persistence and event sourcing.
* Azure CosmosDB for event sourcing, entity, view and project persistence.
* Azure Blob/Table Storage for event sourcing, entity, view and project persistence.
* Sql for event sourcing, entity, view and project persistence with built-in mirroring.
* Advanced concurrency support via [akka.net](http://getakka.net/)

## Projects / Nuget packages:

See [Nuget Packages](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Nuget-Packages) in the wiki.
