## 📌 ETPackages.Mediator

[![Build & Publish](https://github.com/ET-NuGet-Packages/ETPackages.Mediator/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/ET-NuGet-Packages/ETPackages.Mediator/actions)
[![NuGet Version](https://img.shields.io/nuget/v/ETPackages.Mediator.svg?logo=nuget)](https://www.nuget.org/packages/ETPackages.Mediator/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ETPackages.Mediator.svg)](https://www.nuget.org/packages/ETPackages.Mediator/)
[![Target Frameworks](https://img.shields.io/badge/.NET-8%20%7C%209-blue?logo=dotnet)](https://dotnet.microsoft.com/)

**ETPackages.Mediator** is a simple and performance-oriented library for .NET. It supports `IRequest`, `INotification`, `IPipelineBehavior`, and works seamlessly with Dependency Injection.

---

## 📦 Features

- ✅ Request/Response (`IRequest`, `IRequest<TResponse>`)
- ✅ Notifications (`INotification`)
- ✅ Pipeline behaviors (`IPipelineBehavior`)
- ✅ Dependency Injection ready
- ✅ Fully async support

---

## 📋 Requirements

- .NET 8.0 or higher  

---

## 📦 Installation

Install via [NuGet](https://www.nuget.org/packages/ETPackages.Mediator):

```dash
Install-Package ETPackages.Mediator
```

Or via the .NET Core command line interface:

```dash
dotnet add package ETPackages.Mediator
```

---

## 🚀 Getting Started

```csharp
using ETPackages.Mediator;

services.AddMediator(options =>
{
    options.AddRegisterAssemblies(typeof(Program).Assembly);
    options.AddOpenBehavior(typeof(LoggingBehavior<,>)); //with response
    options.AddOpenBehavior(typeof(ValidationBehavior<,>)); //with response
    options.AddOpenBehavior(typeof(ValidationBehavior<>)); //no response
});
```

---

## 🧩 IRequest / IRequestHandler

Define a request and its corresponding handler:

`With Response`

```csharp
public class GetProductQuery : IRequest<ProductDto>
{
    public int Id { get; set; }
}

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    public Task<ProductDto> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = new ProductDto { Id = request.Id, Title = "Domain-Driven Design: Tackling Complexity in the Heart of Software" };
        return Task.FromResult(product);
    }
}
```

`No Response`

```csharp
public class CreateProductCommand : IRequest
{
    public string Title { get; set; }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
{
    public Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // db record

        await Task.CompletedTask;
    }
}
```

## 📤 Sending with IMediator

Use `IMediator` to send the request:

`With Response`

```csharp
var result = await mediator.Send(new GetProductQuery { Id = 1 });
Console.WriteLine(result.Title);
```

`No Response`

```csharp
await mediator.Send(new CreateProductCommand { Title = "Domain-Driven Design: Tackling Complexity in the Heart of Software" });
```

## 🔁 Pipeline Behavior

Use `IPipelineBehavior` for cross-cutting concerns like logging, validation, caching, etc.

`With Response`

```csharp
public interface ILoggableRequest
{
}

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ILoggableRequest
{
    public async Task<TResponse> Handle(TRequest request, RequesteHandlerDelete<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation($"Request received: {requestName}");
        
        var response = await next();
        
        logger.LogInformation($"Response returned: {requestName}");
        
        return response;
    }
}
```

`No Response`

```csharp
public class ValidationBehavior<TRequest>(ILogger<ValidationBehavior<TRequest>> logger) : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    public async Task Handle(TRequest request, RequesteHandlerDelete<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation($"Request validation started: {requestName}");

        await next();

        logger.LogInformation($"Request validation finished: {requestName}");
    }
}
```

## 📣 Notification Handling

Define a notification and its handler:

```csharp
public class ProductCreatedEvent : INotification
{
    public ProductCreatedEvent(int id)
    {
        Id = id;
    }

    public int Id { get; set; }
}

public class SendEmailHandler(ILogger<SendEmailHandler> logger) : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Id sent: {notification.Id}");

        // process send email

        return Task.CompletedTask;
    }
}
```

Publish a notification:

```csharp
await mediator.Publish(new ProductCreatedEvent(product.Id));
```

---

## 📁 License

This project is licensed under the MIT License.