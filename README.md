TaskManager
Overview

TaskManager is a .NET microservice responsible for task and workflow management within the Five Degrees Neo core banking platform. It powers the task management features displayed on the platform’s main dashboard, allowing users to track and manage tasks related to banking processes. Developed by Five Degrees, TaskManager integrates with the broader Neo suite – a cloud-native, Azure-based microservices platform – to provide a responsive and modular banking solution
microsoft.com
. Five Degrees and the Neo platform have been featured by Microsoft in a customer case study for their innovative use of Azure and microservice architecture
microsoft.com
.

Technologies Used

.NET 6 (ASP.NET Core) – The service is built with .NET 6, leveraging ASP.NET Core to expose its functionality as a web API.

Entity Framework Core – Used as the Object-Relational Mapper (ORM) for data access. EF Core enables database operations through C# objects, supporting code-first design and migrations.

MediatR – Utilized to implement the mediator pattern for decoupling business logic. MediatR is an in-process messaging library that supports handling of requests/responses, commands, and queries in .NET
medium.com
, enabling a CQRS-style separation of concerns.

Domain-Driven Design (DDD) principles – The solution follows a layered architecture with separate Domain, Infrastructure, and API projects, indicating a DDD approach. This separation of concerns helps enforce a clear domain model and persistence ignorance, and together with MediatR supports a clean CQRS pattern for command and query handling.

Part of the Neo Platform

TaskManager operates as one component of the Five Degrees Neo platform. Neo is a state-of-the-art SaaS core banking suite built by Five Degrees
5square.nl
. It runs on Microsoft Azure and employs a modular microservices architecture
microsoft.com
microsoft.com
. The platform offers a range of core banking modules – including loan and account management, payments, orchestration, and a task/workflow management system
microsoft.com
. TaskManager specifically provides the task and workflow functionality within this ecosystem, contributing to Neo’s goal of delivering adaptive and modern banking experiences.

Five Degrees introduced Neo to help banks and fintech companies modernize their legacy infrastructure with cloud-native technology
microsoft.com
. The Neo platform’s design adheres to Azure-first principles and emphasizes scalability, security, and continuous deployment
microsoft.com
. Microsoft highlighted Five Degrees and the Neo platform in an official customer story, showcasing how Neo leverages Azure services to deliver a flexible and resilient core banking solution
