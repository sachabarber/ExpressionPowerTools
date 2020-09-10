﻿# IRouteProcessor.ParseRoute Method

[Index](../index.md) > [ExpressionPowerTools.Serialization.EFCore.AspNetCore](ExpressionPowerTools.Serialization.EFCore.AspNetCore.a.md) > [ExpressionPowerTools.Serialization.EFCore.AspNetCore.Signatures](ExpressionPowerTools.Serialization.EFCore.AspNetCore.Signatures.n.md) > [IRouteProcessor](ExpressionPowerTools.Serialization.EFCore.AspNetCore.Signatures.IRouteProcessor.i.md) > **ParseRoute**

Parse the path for segments.

## Overloads

| Overload | Description |
| :-- | :-- |
| [ParseRoute(RouteValueDictionary route)](#parserouteroutevaluedictionary-route) | Parse the path for segments. |
## ParseRoute(RouteValueDictionary route)

Parse the path for segments.

```csharp
public virtual ValueTuple<String, String> ParseRoute(RouteValueDictionary route)
```

### Return Type

 [ValueTuple&lt;String, String>](https://docs.microsoft.com/dotnet/api/system.valuetuple-2)  - A tuple with the name of the `DbContext` and the collection.

### Parameters

| Parameter | Type | Description |
| :-- | :-- | :-- |
| `route` | [RouteValueDictionary](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.routing.routevaluedictionary) | The route. |


## Remarks

Expects it in the format `/root/contextName/collectionName` .


---

| Generated | Copyright | Version |
| :-- | :-: | --: |
| 9/10/2020 10:31:18 PM | (c) Copyright 2020 Jeremy Likness. | 0.8.7-alpha |