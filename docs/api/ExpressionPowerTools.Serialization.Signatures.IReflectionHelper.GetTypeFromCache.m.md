﻿# IReflectionHelper.GetTypeFromCache Method

[Index](../index.md) > [ExpressionPowerTools.Serialization](ExpressionPowerTools.Serialization.a.md) > [ExpressionPowerTools.Serialization.Signatures](ExpressionPowerTools.Serialization.Signatures.n.md) > [IReflectionHelper](ExpressionPowerTools.Serialization.Signatures.IReflectionHelper.i.md) > **GetTypeFromCache**

Get a [Type](https://docs.microsoft.com/dotnet/api/system.type) based on full name.

## Overloads

| Overload | Description |
| :-- | :-- |
| [GetTypeFromCache(String name)](#gettypefromcachestring-name) | Get a [Type](https://docs.microsoft.com/dotnet/api/system.type) based on full name. |
## GetTypeFromCache(String name)

Get a [Type](https://docs.microsoft.com/dotnet/api/system.type) based on full name.

```csharp
public virtual Type GetTypeFromCache(String name)
```

### Return Type

 [Type](https://docs.microsoft.com/dotnet/api/system.type)  - The [Type](https://docs.microsoft.com/dotnet/api/system.type) or `null` .

### Parameters

| Parameter | Type | Description |
| :-- | :-- | :-- |
| `name` | [String](https://docs.microsoft.com/dotnet/api/system.string) | The full name of the [Type](https://docs.microsoft.com/dotnet/api/system.type) . |


## Remarks

This will check the cache first, then try to create the type, and
            finally will scan all assemblies for the type.


---

| Generated | Copyright | Version |
| :-- | :-: | --: |
| 9/3/2020 10:27:04 PM | (c) Copyright 2020 Jeremy Likness. | 0.8.4-alpha |