# TaskManager

## Overview

**TaskManager** is a .NET microservice responsible for task and workflow management within the Five Degrees **Neo** core banking platform. It powers the task management features displayed on the platform’s main dashboard, enabling users to track and manage tasks related to various banking processes.

Developed by Five Degrees, TaskManager integrates with the broader Neo suite — a cloud-native, Azure-based microservices platform — to provide a responsive and modular banking solution.

> 📖 TaskManager and the Neo platform have been featured by Microsoft in a customer case study for their innovative use of Azure and microservice architecture.  
> [Read the full Microsoft story](https://www.microsoft.com/en/customers/story/1709320069254495348-five-degrees-azure-netherlands)

## Technologies Used

- **.NET 6 (ASP.NET Core)**  
  The service is built using .NET 6, leveraging ASP.NET Core to expose its functionality through a robust and scalable web API.

- **Entity Framework Core**  
  Acts as the Object-Relational Mapper (ORM) to manage data access. EF Core supports code-first design and migrations to simplify database interactions.

- **MediatR**  
  Implements the mediator pattern to decouple business logic. Supports request/response, commands, and queries using a clean CQRS-style architecture.

- **Domain-Driven Design (DDD)**  
  The solution is structured using DDD principles, with separate Domain, Infrastructure, and API layers. This promotes separation of concerns and aligns with modern microservice architecture best practices.

## Part of the Neo Platform

TaskManager is a core component of **Neo**, a modern SaaS core banking suite developed by Five Degrees. Neo is built on **Microsoft Azure** and follows a modular microservices architecture to deliver flexibility and scalability.

The platform includes modules for:
- Loan and account management
- Payments
- Orchestration
- Task/workflow automation (powered by TaskManager)

Neo empowers banks and fintechs to replace legacy infrastructure with cloud-native solutions that support continuous deployment, high availability, and regulatory compliance.

> 📘 Learn more about Neo and its impact in the official Microsoft case study:  
> [Five Degrees redefines core banking with Azure](https://www.microsoft.com/en/customers/story/1709320069254495348-five-degrees-azure-netherlands)
