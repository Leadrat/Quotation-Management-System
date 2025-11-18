# CRM Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-11-13

## Active Technologies
- C# 12, .NET 8 Web API + MediatR, Ardalis.Specification, EF Core, FluentValidation, AutoMapper, Serilog (006-client-crud)
- PostgreSQL (UUID PKs, TIMESTAMPTZ); EF Core migrations (006-client-crud)
- C# 12+ (.NET 8.0), TypeScript 5.x, React 19, Next.js 15 (018-system-administration-configuration)
- PostgreSQL (SystemSettings, IntegrationKeys, AuditLog, CustomBranding, DataRetentionPolicy tables), File storage for logos (S3 or local filesystem) (018-system-administration-configuration)

- [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION] + [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION] (master)

## Project Structure

```text
backend/
frontend/
tests/
```

## Commands

cd src; pytest; ruff check .

## Code Style

[e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION]: Follow standard conventions

## Recent Changes
- 018-system-administration-configuration: Added C# 12+ (.NET 8.0), TypeScript 5.x, React 19, Next.js 15
- 006-client-crud: Added C# 12, .NET 8 Web API + MediatR, Ardalis.Specification, EF Core, FluentValidation, AutoMapper, Serilog

- master: Added [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION] + [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
