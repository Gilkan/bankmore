# BankMore

**BankMore** is a **.NET 8 backend system** that implements a small but realistic **banking core**, focused on **transaction correctness, clean layering, and deterministic testing**.

This repository is intentionally scoped as a **portfolio-quality project**.  
Its purpose is to demonstrate _how to design and reason about backend architecture_, not to provide a production-ready financial system.

---

## Why This Project Exists

This project exists to demonstrate:

- Explicit and correct transaction handling
- Clean separation between domain, application, and infrastructure
- Safe use of SQLite despite its limitations
- Deterministic, persistence-heavy unit tests
- Optional asynchronous integration (Kafka) without polluting core logic
- Practical use of CQRS-style command handlers

It is meant to be **read and reviewed** as much as it is run.

---

## Technical Stack

- .NET 8
- ASP.NET Core Web API
- MediatR
- Dapper
- SQLite
- JWT Authentication
- Kafka (optional)
- Docker / docker-compose
- xUnit

---

## Architecture Overview

The solution follows a **layered architecture** with strict dependency direction.

### Domain

- Pure business entities and value objects
- Encapsulates business rules and invariants
- No framework, persistence, or infrastructure dependencies
- Uses `Guid` as the native identifier type

Example:

- `Transferencia.Criar` enforces valid value and distinct accounts
- Domain exceptions represent business failures, not technical ones

---

### Application

- Use cases implemented as **MediatR command handlers**
- Orchestrates business workflows
- Coordinates repositories and domain behavior
- **Explicitly controls transaction boundaries** via the Unit of Work

The application layer does **not** hide transactional behavior — commits and rollbacks are intentional and visible in handlers.

This is **CQRS-style command handling**, not a full CQRS system with separate read models.

---

### Infrastructure

- SQLite persistence via Dapper
- Explicit connection and transaction handling
- Repository implementations
- JWT authentication wiring
- Optional Kafka producer
- Exception middleware

Infrastructure concerns are kept out of the domain and application logic whenever possible.

---

### Tests

- Unit tests covering core business scenarios
- SQLite in-memory database
- Shared connection + transaction per test
- Deterministic and isolated execution
- No reliance on ambient transactions or auto-commit behavior

Tests intentionally mirror production transaction flows to surface correctness issues early.

---

## Persistence & Transaction Strategy

### SQLite as a Deliberate Constraint

SQLite is used intentionally for development and testing to enforce discipline:

- No nested transactions
- Weak native GUID support
- Easy to misuse auto-commit

Rather than hiding these limitations, the design **forces explicit handling**.

---

### Core Rules

- All write operations require an explicit `IDbTransaction`
- Repositories never begin, commit, or rollback transactions
- The Unit of Work owns:
  - Connection lifetime
  - Transaction lifecycle
- Command handlers decide **when** to commit or rollback

This makes transactional behavior explicit, predictable, and testable.

---

### GUID Handling

- Domain model uses `Guid`
- SQLite persists GUIDs as `TEXT` when configured
- Conversion happens only at persistence boundaries
- The domain remains infrastructure-agnostic

---

## Unit of Work

- Runtime implementation: `SqliteUnitOfWork`
- Test implementation: `TestUnitOfWork`

Responsibilities:

- Open and own the database connection
- Begin and control the transaction
- Commit or rollback explicitly
- Prevent unsafe transactional behavior under SQLite

Tests reuse an existing transaction to preserve determinism.

---

## Kafka Integration (Optional)

Kafka is included as an **optional integration mechanism**, not a core dependency.

### What Is Implemented

- `KafkaProducer` abstraction over `Confluent.Kafka`
- JSON message serialization
- Topic configuration via options
- Messages published **only after a successful database commit**
- Application runs normally if Kafka is not configured

Kafka is used strictly as an **integration channel**, not as a source of truth.

### Explicit Non-Goals

The following are intentionally **out of scope**:

- Kafka consumers
- Outbox pattern
- Retry policies / DLQs
- Schema registry
- Production-grade Kafka security

---

## API & Security

- ASP.NET Core controllers
- JWT-based authentication
- Swagger configured with Bearer token support
- Stubbed user authentication client

JWT support exists to demonstrate **security wiring**, not to represent a full identity solution.

---

## Containerized Local Runtime

A `docker-compose.yml` is provided for local development.

Included services:

- BankMore API
- Kafka broker
- Zookeeper
- Kowl UI (Kafka inspection)
- SQLite (file-based persistence)

This setup allows:

- Running the API with real infrastructure
- Inspecting Kafka messages visually
- Resetting state via Docker volumes

---

## Tests

### Covered Scenarios

- Account credit and debit
- Transfers between accounts
- Transfer fees
- Idempotency behavior
- Transaction correctness

### Characteristics

- SQLite in-memory database
- Explicit transaction control
- Deterministic execution
- No shared state between tests

The tests are designed to **fail loudly** if transactional rules are violated.

---

## Account Number Generation

Account numbers are **not auto-generated by the database**.

Behavior:

- The repository computes the next number using `MAX(numero) + 1`
- A configurable minimum starting value is enforced

Example configuration:

```json
"ContaCorrente": {
  "NumeroInicial": 1000
}
```
