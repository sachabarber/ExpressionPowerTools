﻿# ICustomQueryProvider&lt;T> Interface

[Index](../index.md) > [ExpressionPowerTools.Core](ExpressionPowerTools.Core.a.md) > [ExpressionPowerTools.Core.Signatures](ExpressionPowerTools.Core.Signatures.n.md) > **ICustomQueryProvider<T>**

Interface for a custom implementation of [IQueryProvider](https://docs.microsoft.com/dotnet/api/system.linq.iqueryprovider) .

```csharp
public interface ICustomQueryProvider<T> : IQueryProvider
```

### Type Parameters

| Parameter Name | Constraints | Description |
| :-- | :-- | :-- |
| `T` | None. | The entity type. |

Implements  [IQueryProvider](https://docs.microsoft.com/dotnet/api/system.linq.iqueryprovider) 

Derived  [CustomQueryProvider&lt;T>](ExpressionPowerTools.Core.Providers.CustomQueryProvider`1.cs.md) ,  [IQueryInterceptingProvider&lt;T>](ExpressionPowerTools.Core.Signatures.IQueryInterceptingProvider`1.i.md) ,  [IQuerySnapshotProvider&lt;T>](ExpressionPowerTools.Core.Signatures.IQuerySnapshotProvider`1.i.md) ,  [QueryInterceptingProvider&lt;T>](ExpressionPowerTools.Core.Providers.QueryInterceptingProvider`1.cs.md) ,  [QuerySnapshotProvider&lt;T>](ExpressionPowerTools.Core.Providers.QuerySnapshotProvider`1.cs.md) 

## Methods

| Method | Description |
| :-- | :-- |
| [IEnumerable&lt;T> ExecuteEnumerable(Expression expression)](ExpressionPowerTools.Core.Signatures.ICustomQueryProvider`1.ExecuteEnumerable.m.md) | Execute enumeration from the [Expression](https://docs.microsoft.com/dotnet/api/system.linq.expressions.expression) . |

---

| Generated | Copyright | Version |
| :-- | :-: | --: |
| 09/21/2020 19:07:57 | (c) Copyright 2020 Jeremy Likness. | 0.9.2-alpha |
