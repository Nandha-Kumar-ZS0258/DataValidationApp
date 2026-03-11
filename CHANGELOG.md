# Changelog

All notable changes to **TruStage.Adaptor.Validation** will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Three-gate validation pipeline (`Gate1`, `Gate2`, `Gate3`)
- `IPipelineValidationService` / `PipelineValidationService` orchestrator
- `IConsistencyCheck<TEntity>` plug-in interface for entity-level rules
- Member checks: required fields, status, date-of-birth, credit score, contact
- Account checks: balance, status, interest rate, maturity date
- Loan checks: amount, origination date, delinquency, charge-off, rate
- Transaction checks: amount, date, running balance
- Cross-entity checks: referential integrity, transaction-to-account, joint-owner self-reference, duplicate key detection
- `DataSnapshot`, `ValidationGate`, `ReconciliationGap`, `CheckResult` models
- `IServiceCollection` extension `AddAdaptorValidation`
- xUnit test project with FluentAssertions and NSubstitute
- GitHub Actions CI workflow (build + pack)

[Unreleased]: https://github.com/TruStage/DataValidationApp/compare/HEAD...HEAD
