﻿# ExceptionHelper.AsInvalidOperationException Method

[Index](../index.md) > [ExpressionPowerTools.Core](ExpressionPowerTools.Core.a.md) > [ExpressionPowerTools.Core.Resources](ExpressionPowerTools.Core.Resources.n.md) > [ExceptionHelper](ExpressionPowerTools.Core.Resources.ExceptionHelper.cs.md) > **AsInvalidOperationException**

Invalid operation messages.

## Overloads

| Overload | Description |
| :-- | :-- |
| [AsInvalidOperationException(String message, String[] parameters)](#asinvalidoperationexceptionstring-message-string[]-parameters) | Invalid operation messages. |
## AsInvalidOperationException(String message, String[] parameters)

Invalid operation messages.

```csharp
public static InvalidOperationException AsInvalidOperationException(String message, String[] parameters)
```

### Return Type

 [InvalidOperationException](https://docs.microsoft.com/dotnet/api/system.invalidoperationexception)  - The invalid operation.

### Parameters

| Parameter | Type | Description |
| :-- | :-- | :-- |
| `message` | [String](https://docs.microsoft.com/dotnet/api/system.string) | The message key. |
| `parameters` | [String[]](https://docs.microsoft.com/dotnet/api/system.string[]) | The parameters. |



---

| Generated | Copyright | Version |
| :-- | :-: | --: |
| 8/27/2020 11:30:52 PM | (c) Copyright 2020 Jeremy Likness. | 0.8.2-alpha |