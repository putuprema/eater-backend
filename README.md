# Eater

Eater is a restaurant food ordering application for contactless dining experience. This repository is the backend for the application, employing event-driven microservices with Clean Architecture using CQRS (Command Query Responsibility Segregation) design pattern.

### Architecture
TBD

### Tech Stack
- Azure API Management for API gateway
- Azure Function on .NET 6 for each microservice
- Azure Cosmos DB for database
- Azure Event Grid for messaging
- Azure Blob Storage for storing files

### Libraries
- [MediatR](https://github.com/jbogard/MediatR)
- [FluentValidation](https://fluentvalidation.net/)
- [Mapster](https://github.com/MapsterMapper/Mapster)
- [ImageSharp](https://github.com/SixLabors/ImageSharp)
- [QRCoder](https://github.com/codebude/QRCoder)

### TODO
- [x] Account microservice
- [x] Product microservice
- [x] Table microservice
- [x] Payment microservice
- [x] Order microservice
- [ ] Notification microservice
- [ ] CI/CD
