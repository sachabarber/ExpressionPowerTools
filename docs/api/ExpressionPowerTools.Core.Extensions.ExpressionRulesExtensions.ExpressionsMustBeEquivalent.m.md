﻿# ExpressionRulesExtensions.ExpressionsMustBeEquivalent Method

[Index](../index.md) > [ExpressionPowerTools.Core](ExpressionPowerTools.Core.a.md) > [ExpressionPowerTools.Core.Extensions](ExpressionPowerTools.Core.Extensions.n.md) > [ExpressionRulesExtensions](ExpressionPowerTools.Core.Extensions.ExpressionRulesExtensions.cs.md) > **ExpressionsMustBeEquivalent**

Expressions must be equivalent.

## Overloads

| Overload | Description |
| :-- | :-- |
| [ExpressionsMustBeEquivalent&lt;T>(Func&lt;T, Expression> member)](#expressionsmustbeequivalenttfunct-expression-member) | Expressions must be equivalent. |
## ExpressionsMustBeEquivalent&lt;T>(Func&lt;T, Expression> member)

Expressions must be equivalent.

```csharp
public static Expression<Func<T, T, Boolean>> ExpressionsMustBeEquivalent<T>(Func<T, Expression> member)
```

### Return Type

 [Expression&lt;Func&lt;T, T, Boolean>>](https://docs.microsoft.com/dotnet/api/system.linq.expressions.expression-1)  - A value indicating whether the expressions are equivalent.

### Parameters

| Parameter | Type | Description |
| :-- | :-- | :-- |
| `member` | [Func&lt;T, Expression>](https://docs.microsoft.com/dotnet/api/system.func-2) | Reference the property that is an expression. |



---

| Generated | Copyright | Version |
| :-- | :-: | --: |
| 09/21/2020 19:07:57 | (c) Copyright 2020 Jeremy Likness. | 0.9.2-alpha |
