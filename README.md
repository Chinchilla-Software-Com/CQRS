# CQRS
#### https://www.nuget.org/packages/Cqrs
## Documentation and sample projects are a work in progress.

## The framework
A lightweight framework to help write CQRS and Eventsourcing applications in C#. Currently published as nuget packages @ http://www.nuget.org. It is written in C# and targets .NET 4.0, with the exception of some Azure packages which target .NET 4.5. CQRS borrows heavily from CQRSlite (https://github.com/gautema/cqrslite), from some point in 2013.

CQRS has been made designed with modularity in mind... just look at the number of packages below you can chose from. Every package and design choice I've ade should be interchangeable with custom code if needed.

## Commerical Support
Commercial support will be available from October 2015. Details to come.

##Getting started
Two sample projects are located within the code (soon), these show two approaches/common usage scenarios of the framework. One sample is the original sample from CQRSlite, just ported to use CQRS, the other is Modelled in UML and uses T4 template generation.

The project should compile without any setup in .NET 4.0. The Azure project should compile without any setup in .NET 4.5.2. I'm guessing Mono 3.10.0 should work too.

##Features
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

##Installation
To install Cqrs,  you can either download the files and copy whats needed into your project, you can clone this project and reference it, or you can download the below published packages  from nuget. You can currently choose to use either Greg Youngs EventStore or Azure Document DB as own eventstore, or writing an adapter to use an existing eventstore should be trivial.

At this point Ninject is the IOC container of choice.

## Projects / Nuget packages:

* Cqrs
* Cqrs.Mongo - Includes MongoDB requirements for persiting entities, views and projections.
* Cqrs.EventStore - Includes Greg Youngs EventStore requirements for persiting events.
* Cqrs.Azure.ServiceBus - Includes Azure Service requirements for using the Azure ServiceBus as a command and event bus.
* Cqrs.Azure.DocumentDb - Includes Azure DocumentDB requirements for persiting events, entities, views and projections.
* Cqrs.Azure.ConfigurationManager Includes Azure ConfigurationManager requirements for setting configuration via cloud service files etc. Read up on the nuget package Microsoft.WindowsAzure.ConfigurationManager.
* Cqrs.Ninject.Mongo - Includes Ninject configuration for the Cqrs.Mongo package.
* Cqrs.Ninject.EventStore - Includes Ninject configuration for the Cqrs.EventStore package.
* Cqrs.Ninject.Azure.ServiceBus.CommandBus - Includes Ninject configuration for the Cqrs.Azure.ServiceBus package for command bus'ing only.
* Cqrs.Ninject.Azure.ServiceBus.EventBus - Includes Ninject configuration for the Cqrs.Azure.ServiceBus package for event bus'ing only
* Cqrs.Ninject.Azure.DocumentDb - Includes Ninject configuration for the Cqrs.Azure.DocumentDb package.
* Cqrs.Ninject.InProcess.CommandBus - Includes Ninject configuration for the using an in-process for command bus.
* Cqrs.Ninject.InProcess.EventBus - Includes Ninject configuration for the using an in-process for event bus.
* Cqrs.Ninject.InProcess.EventStore - Includes Ninject configuration for the using an in-process for event store... Surprise... it doesn't persist, so it's probably only good for unit and integration tests.
* Cqrs.Modelling.UmlProfiles - Includes T4 templates and a VISX extension so you can use the built-in UML modelling. features of Visual Studio 2012 or above... Sort of works for Visual Studio 2010, but not really.
