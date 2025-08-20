# ProductAPI Documentation

## 📋 Overview

This documentation covers the architectural decisions, business rules, and implementation patterns for the ProductAPI project built with Hexagonal Architecture and Domain-Driven Design (DDD).

## 📁 Documentation Structure

- **[Architecture](./architecture/)** - Architectural decisions and patterns
- **[Business Rules](./business-rules/)** - BDD-style business requirements and tests
- **[Patterns](./patterns/)** - Implementation patterns and trade-offs

## 🏗️ Project Structure

```
ProductAPI/
├── ProductAPI.Api/          # API Layer (Controllers, Swagger)
├── ProductAPI.Application/  # Application Layer (Use Cases, DTOs)
├── ProductAPI.Domain/       # Domain Layer (Entities, Specifications, Business Rules)
├── ProductAPI.Infrastructure/ # Infrastructure Layer (Repositories, Data)
├── ProductAPI.Tests/        # Unit Tests (30 tests)
├── ProductAPI.IntegrationTests/ # Integration Tests (27 tests)
└── docs/                   # Project Documentation
```

## 🎯 Key Features

- ✅ **Hexagonal Architecture** with clear separation of concerns
- ✅ **Domain-Driven Design** with business rules in domain layer
- ✅ **Specification Pattern** for domain-driven queries
- ✅ **Comprehensive Testing** (57 total tests)
- ✅ **Clean Architecture** principles
- ✅ **Business Rule Validation** enforced at domain level

## 🚀 Quick Start

1. **Clone the repository**
2. **Run tests**: `dotnet test`
3. **Start API**: `dotnet run --project ProductAPI.Api`
4. **View Swagger**: `https://localhost:7xxx/swagger`

## 📚 Further Reading

- [Architectural Decision Records](./architecture/ADRs.md)
- [Business Rules Specification](./business-rules/product-rules.md)
- [Testing Strategy](./architecture/testing-strategy.md)