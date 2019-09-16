# CQRS.NET
#### https://www.nuget.org/packages/Cqrs
Nightly build packages and symbols available at http://nightlies.chinchillasoftware.co.nz/

## The framework
A lightweight enterprise Function as a service (FaaS) framework to write function based serverless and micro-service applications in hybrid multi-datacentre, on-premise and Azure environments, offering modern patterns such as CQRS and event-sourcing. Offering a superior combination of serverless, micro-service and traditional deployments both in the cloud and on-premise to suit any business. Deployments can be inter-connectected with each other sharing data and resourcing or independant and isolated while providing a consistent framework and guideline for both development, deployment, DevOps and administration.

CQRS.NET has been designed with modularity in mind... see the number of technology packages below you can choose from. Modularity applies to both development concerns like storage as well as operational modularity such as serverless or micro-service deployment, PaaS, VMs or container packaging. Every package and design choice made should be interchangeable with custom code if needed.

![CQRS.NET](https://raw.githubusercontent.com/Chinchilla-Software-Com/CQRS/master/wiki/stack/FaaS.png)

## Tutorials and getting started
For those new to CQRS.NET, we strongly suggest you have a look at our basic [Hello World Tutorial](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Hello-World-Example-1) that covers the basics of sending messages between a website and a function via commands and events, including pushing real-time messages back to the browser without refreshing your screen in a very simplcity micro-service kinda of way.

Or you can have a look at our basic [Northwind Tutorial](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Tutorial-1:-Step-1:-Quick-Northwind-sample.) which takes you through  converting an existing legacy website covering some queries, views, read stores and read models, then getting into actioning function with commands, events aggregates and event handlers (the function itself). This takes you on the micro-service path as well while keeping it very simple and running all the services in one website to keep the tutorial simple. At any stage you can start to break up the well structured code into separate API endpoints.

And for those looking for a more thorough tutorial including a deep, deep dive into best practises we suggest you start with the [A Beginner's Guide](https://github.com/Chinchilla-Software-Com/CQRS/wiki/A-Beginner's-Guide-To-Implementing-CQRS-ES) section in the [wiki](https://github.com/Chinchilla-Software-Com/CQRS/wiki). This covers much, much more than just functions, but modern coding practises that really do work in the cloud.

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
