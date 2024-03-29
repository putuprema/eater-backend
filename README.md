# Eater

[![Deploy Account Service](https://github.com/putuprema/eater-backend/actions/workflows/account-service-deploy.yml/badge.svg)](https://github.com/putuprema/eater-backend/actions/workflows/account-service-deploy.yml)
[![Deploy Notification Service](https://github.com/putuprema/eater-backend/actions/workflows/notification-service-deploy.yml/badge.svg)](https://github.com/putuprema/eater-backend/actions/workflows/notification-service-deploy.yml)
[![Deploy Order Service](https://github.com/putuprema/eater-backend/actions/workflows/order-service-deploy.yml/badge.svg)](https://github.com/putuprema/eater-backend/actions/workflows/order-service-deploy.yml)
[![Deploy Payment Service](https://github.com/putuprema/eater-backend/actions/workflows/payment-service-deploy.yml/badge.svg)](https://github.com/putuprema/eater-backend/actions/workflows/payment-service-deploy.yml)
[![Deploy Product Service](https://github.com/putuprema/eater-backend/actions/workflows/product-service-deploy.yml/badge.svg)](https://github.com/putuprema/eater-backend/actions/workflows/product-service-deploy.yml)
[![Deploy Table Service](https://github.com/putuprema/eater-backend/actions/workflows/table-service-deploy.yml/badge.svg)](https://github.com/putuprema/eater-backend/actions/workflows/table-service-deploy.yml)

Eater is a restaurant food ordering application for contactless dining experience. This repository is the backend for the application, employing event-driven microservices with Clean Architecture using CQRS (Command Query Responsibility Segregation) and Saga design pattern.

### Architecture
![Eater Architecture Diagram](Architecture.jpg)

### Tech Stack
- Azure API Management for API gateway
- Azure Function on .NET 6 for each microservice
- Azure Cosmos DB for database
- Azure Event Grid and Service Bus for messaging
- Azure Blob Storage for storing files
- Stripe as Payment Gateway

### Libraries
- [MediatR](https://github.com/jbogard/MediatR)
- [FluentValidation](https://fluentvalidation.net/)
- [Mapster](https://github.com/MapsterMapper/Mapster)
- [ImageSharp](https://github.com/SixLabors/ImageSharp)
- [QRCoder](https://github.com/codebude/QRCoder)
- [Stripe.net](https://github.com/stripe/stripe-dotnet)

### TODO
- [x] Account microservice
- [x] Product microservice
- [x] Table microservice
- [x] Payment microservice
- [x] Order microservice
- [x] Notification microservice
- [ ] CI/CD
