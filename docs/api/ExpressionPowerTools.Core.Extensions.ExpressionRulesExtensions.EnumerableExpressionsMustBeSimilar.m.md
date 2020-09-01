﻿# ExpressionRulesExtensions.EnumerableExpressionsMustBeSimilar Method

[Index](../index.md) > [ExpressionPowerTools.Core](ExpressionPowerTools.Core.a.md) > [ExpressionPowerTools.Core.Extensions](ExpressionPowerTools.Core.Extensions.n.md) > [ExpressionRulesExtensions](ExpressionPowerTools.Core.Extensions.ExpressionRulesExtensions.cs.md) > **EnumerableExpressionsMustBeSimilar**

Expression in each enumerable must be similar.

## Overloads

| Overload | Description |
| :-- | :-- |
| [EnumerableExpressionsMustBeSimilar&lt;T>(Func&lt;T, IEnumerable&lt;Expression>> member)](#enumerableexpressionsmustbesimilartfunct-ienumerableexpression-member) | Expression in each enumerable must be similar. |
## EnumerableExpressionsMustBeSimilar&lt;T>(Func&lt;T, IEnumerable&lt;Expression>> member)

Expression in each enumerable must be similar.

```csharp
public static Expression<Func<T, T, Boolean>> EnumerableExpressionsMustBeSimilar<T>(Func<T, IEnumerable<Expression>> member)
```

### Return Type

 [Expression&lt;Func&lt;T, T, Boolean>>](https://docs.microsoft.com/dotnet/api/system.linq.expressions.expression-1)  - A value indicating whether or not the enumerables are similar.

### Parameters

| Parameter | Type | Description |
| :-- | :-- | :-- |
| `member` | [Func&lt;T, IEnumerable&lt;Expression>>](https://docs.microsoft.com/dotnet/api/system.func-2) | The enumerable child expressions. |



---

| Generated | Copyright | Version |
| :-- | :-: | --: |
| 8/27/2020 11:30:52 PM | (c) Copyright 2020 Jeremy Likness. | 0.8.2-alpha |