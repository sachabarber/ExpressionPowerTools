﻿# CustomQueryProvider&lt;T> Class

[Index](../index.md) > [ExpressionPowerTools.Core](ExpressionPowerTools.Core.a.md) > [ExpressionPowerTools.Core.Providers](ExpressionPowerTools.Core.Providers.n.md) > **CustomQueryProvider<T>**

Base query provider class.

```csharp
public abstract class CustomQueryProvider<T> : ICustomQueryProvider<T>
```

### Type Parameters

| Parameter Name | Constraints | Description |
| :-- | :-- | :-- |
| `T` | None. | The entity type. |

Inheritance [Object](https://docs.microsoft.com/dotnet/api/system.object) → **CustomQueryProvider&lt;T>**

Implements  [ICustomQueryProvider&lt;T>](ExpressionPowerTools.Core.Signatures.ICustomQueryProvider`1.i.md) ,  [IQueryProvider](https://docs.microsoft.com/dotnet/api/system.linq.iqueryprovider) 

Derived  [QueryInterceptingProvider&lt;T>](ExpressionPowerTools.Core.Providers.QueryInterceptingProvider`1.cs.md) ,  [QuerySnapshotProvider&lt;T>](ExpressionPowerTools.Core.Providers.QuerySnapshotProvider`1.cs.md) 

# Constructors

| Ctor | Description |
| :-- | :-- |
| [CustomQueryProvider(IQueryable sourceQuery)](ExpressionPowerTools.Core.Providers.CustomQueryProvider`1.ctor.md#customqueryprovideriqueryable-sourcequery) | Initializes a new instance of the [CustomQueryProvider&lt;T>](ExpressionPowerTools.Core.Providers.CustomQueryProvider`1.cs.md) class. |