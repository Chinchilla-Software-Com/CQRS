# CS CQRS
#### https://www.nuget.org/packages/Cqrs

## Continual improvements
This project is actively developed, however we sometimes have specific feature requests that are outside of our roadmap and plan. We're always open to new ideas and requests for new modules and technology connectors that you need. Right now we've got an unplanned request to have better [akka.net](http://getakka.net) modules. To help out this we've got a [kickstarter project](https://www.kickstarter.com/projects/chinchilla-software/cqrs-for-akk.net) running to help get the funding to bring this work forward.

## Documentation
Documentation is starting off for now in the [wiki section](https://github.com/Chinchilla-Software-Com/CQRS/wiki) of this project. We strongly invite people to post questions and issues which we'll answer and work on.

## Sample projects.
We have a very simple example which, for now, is too CRUD like for us. 

## Tutorials
As we're not happy with the sample project, we are starting to build a tutorial around a bank teller and ATM sample project which you can follow in the [Getting Started](https://github.com/Chinchilla-Software-Com/CQRS/wiki/Getting-Started) section of the wiki.

## The framework
A lightweight framework to help write CQRS and Eventsourcing applications in C#. Currently published as nuget packages @ http://www.nuget.org. It is written in C# and targets .NET 4.0, with the exception of some Azure packages which target .NET 4.5. CQRS borrows heavily from CQRSlite (https://github.com/gautema/cqrslite), from some point in 2013.

CQRS has been made designed with modularity in mind... just look at the number of packages below you can chose from. Every package and design choice I've ade should be interchangeable with custom code if needed.

## Commerical Support
Commercial support will be available in 2016. Details to come, but in the meantime message us to register your interest in comercial support.

## Getting started
Two sample projects are located within the code (soon), these show two approaches/common usage scenarios of the framework. One sample is the original sample from CQRSlite, just ported to use CQRS, the other is Modelled in UML and uses T4 template generation.

The project should compile without any setup in .NET 4.0. The Azure project should compile without any setup in .NET 4.5.2. I'm guessing Mono 3.10.0 should work too.

## Features
* Command sending and event publishing
* Unit of work through session with aggregate tracking (I'm tempted to remove the UOW)
* Repository for getting and saving aggregates
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
