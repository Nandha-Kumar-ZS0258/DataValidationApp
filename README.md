# TruStage.Adaptor.Validation

[![CI](https://github.com/TruStage/DataValidationApp/actions/workflows/ci.yml/badge.svg)](https://github.com/TruStage/DataValidationApp/actions/workflows/ci.yml)

A production-grade data validation library for the TruStage Adaptor pipeline, implementing a three-gate reconciliation strategy for credit union data migrations.

## Features

- **Three-gate pipeline**: Source → Transformed → ReadyForProd → Prod
- **Pluggable check architecture** via `IConsistencyCheck<TEntity>`
- **Error vs. Warning severity**: Error blocks a record; Warning passes through with a flag
- **Snapshot reconciliation**: count-gap detection at each pipeline stage
- **DI-ready**: single `AddAdaptorValidation()` extension registers everything

## Getting Started

### Installation

```bash
dotnet add package TruStage.Adaptor.Validation
```

### Registration

```csharp
builder.Services.AddAdaptorValidation();
```

### Usage

```csharp
// Inject the service
public class MyAdaptor(IPipelineValidationService validation) { ... }

// Gate 1 & 2 — run after transformation
var gates = validation.RunGate1And2(sourceSnapshot, transformedResult);

// Gate 3 — run after prod load, then build the full report
var report = validation.RunGate3AndBuildReport(
    source, transformed, readyForProd, prod,
    gates.Gate1, gates.Gate2,
    ingestionBatchId, cuId, sourceFileName);
```

## Project Structure

```
src/TruStage.Adaptor.Validation/
  Checks/
    Members/        – Member-level consistency rules
    Accounts/       – Account-level consistency rules
    Loans/          – Loan-level consistency rules
    Transactions/   – Transaction-level consistency rules
    CrossEntity/    – Referential integrity and duplicate checks
  Extensions/       – IServiceCollection extension
  Models/           – ValidationGate, ReconciliationGap, CheckResult, …
tests/TruStage.Adaptor.Validation.Tests/
  Checks/           – Per-check unit tests
  Helpers/          – Builders (test data factory helpers)
```

## Gates

| Gate | From → To | Checks |
|------|-----------|--------|
| Gate 1 | Source → Transformed | Count gaps + entity consistency (Error/Warning rules) |
| Gate 2 | Source → ReadyForProd | Count gaps after DQ blocking |
| Gate 3 | Source → Prod | Count gaps post-ingestion (transaction dedup = Info) |

## Contributing

Pull requests are welcome. Please open an issue first for major changes.
