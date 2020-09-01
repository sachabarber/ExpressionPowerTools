﻿# ParameterSerializer.Serialize Method

[Index](../index.md) > [ExpressionPowerTools.Serialization](ExpressionPowerTools.Serialization.a.md) > [ExpressionPowerTools.Serialization.Serializers](ExpressionPowerTools.Serialization.Serializers.n.md) > [ParameterSerializer](ExpressionPowerTools.Serialization.Serializers.ParameterSerializer.cs.md) > **Serialize**

Serializes the expression.

## Overloads

| Overload | Description |
| :-- | :-- |
| [Serialize(ParameterExpression expression, JsonSerializerOptions options)](#serializeparameterexpression-expression-jsonserializeroptions-options) | Serializes the expression. |
## Serialize(ParameterExpression expression, JsonSerializerOptions options)

Serializes the expression.

```csharp
public virtual Parameter Serialize(ParameterExpression expression, JsonSerializerOptions options)
```

### Return Type

 [Parameter](ExpressionPowerTools.Serialization.Serializers.Parameter.cs.md)  - The serializable [Constant](ExpressionPowerTools.Serialization.Serializers.Constant.cs.md) .

### Parameters

| Parameter | Type | Description |
| :-- | :-- | :-- |
| `expression` | [ParameterExpression](https://docs.microsoft.com/dotnet/api/system.linq.expressions.parameterexpression) | The [ConstantExpression](https://docs.microsoft.com/dotnet/api/system.linq.expressions.constantexpression) to serialize. |
| `options` | [JsonSerializerOptions](https://docs.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions) | The optional [JsonSerializerOptions](https://docs.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions) . |



---

| Generated | Copyright | Version |
| :-- | :-: | --: |
| 8/27/2020 11:30:52 PM | (c) Copyright 2020 Jeremy Likness. | 0.8.2-alpha |