﻿// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using ExpressionPowerTools.Core.Contract;
using ExpressionPowerTools.Core.Extensions;
using ExpressionPowerTools.Core.Signatures;

namespace ExpressionPowerTools.Core.Members
{
    /// <summary>
    /// Handles translation and comparison of members.
    /// </summary>
    public class MemberAdapter : IMemberAdapter
    {
        /// <summary>
        /// Code for constructor.
        /// </summary>
        public const string Ctor = "ctor";

        /// <summary>
        /// Code for static constructor.
        /// </summary>
        public const string StaticCtor = "cctor";

        /// <summary>
        /// Indicates a type parameter.
        /// </summary>
        public const string TypeTick = "`";

        /// <summary>
        /// Indicates the generic parameter for a method.
        /// </summary>
        public const string MethodTick = "``";

        /// <summary>
        /// Property prefix.
        /// </summary>
        public const char PropertyCode = 'P';

        /// <summary>
        /// Type prefix.
        /// </summary>
        public const char TypeCode = 'T';

        /// <summary>
        /// Event code.
        /// </summary>
        public const char EventCode = 'E';

        /// <summary>
        /// Field code.
        /// </summary>
        public const char FieldCode = 'F';

        /// <summary>
        /// Method code.
        /// </summary>
        public const char MethodCode = 'M';

        /// <summary>
        /// All flags.
        /// </summary>
        private readonly BindingFlags all = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Cache for mapping keys to members.
        /// </summary>
        private readonly ConcurrentDictionary<string, MemberInfo> cache =
            new ConcurrentDictionary<string, MemberInfo>();

        /// <summary>
        /// Maps the hash code of the member to its key.
        /// </summary>
        private readonly ConcurrentDictionary<int, string> reverseCache =
            new ConcurrentDictionary<int, string>();

        /// <summary>
        /// Gets a unique string that identifies the member.
        /// </summary>
        /// <param name="member">The <see cref="MemberInfo"/> to parse.</param>
        /// <returns>the unique string.</returns>
        public string GetKeyForMember(MemberInfo member)
        {
            Ensure.NotNull(() => member);

            var hash = member.GetHashCode();
            if (reverseCache.ContainsKey(hash) &&
                reverseCache[hash].Contains(member.Name))
            {
                return reverseCache[hash];
            }

            var typeGenericMap = new Dictionary<string, int>();
            var methodGenericMap = new Dictionary<string, int>();

            char prefixCode;

            var key = new StringBuilder();
            if (member is Type mbr)
            {
                if (mbr.IsAnonymousType())
                {
                    var name = mbr.Name.Substring(0, mbr.Name.IndexOf(TypeTick[0]));
                    var ctor = mbr.GetConstructors().Single();
                    key.Append($"{name}{{{ParseAnonymousTypes(ctor)}}}");
                }
                else
                {
                    if (mbr.IsGenericType && mbr.GenericTypeArguments.Length > 0)
                    {
                        var idx = 0;
                        foreach (var typeArg in mbr.GetGenericTypeDefinition().GetGenericArguments())
                        {
                            typeGenericMap[typeArg.Name] = idx++;
                        }

                        var processedType = ParseTypeParameters(methodGenericMap, typeGenericMap, mbr);
                        key.Append(processedType);
                    }
                    else
                    {
                        key.Append(NameOfType(mbr));
                    }
                }
            }
            else
            {
                var parentType = GetKeyForMember(member.DeclaringType).Split(':')[1];
                key.Append($"{parentType}.{member.Name}");
            }

            Type[] genericArguments = null;
            var parameterInfos = new ParameterInfo[0];
            var isMethod = member is MethodInfo;
            var isStatic = false;

            if (member.DeclaringType != null && member.DeclaringType.IsGenericType)
            {
                var tempTypeGeneric = 0;
                foreach (var genericArg in member.DeclaringType.GetGenericArguments())
                {
                    typeGenericMap[genericArg.Name] = tempTypeGeneric++;
                }
            }

            if (member is MethodInfo method)
            {
                isStatic = method.IsStatic;
                if (method.IsGenericMethod)
                {
                    genericArguments = method.GetGenericArguments();
                }

                parameterInfos = method.GetParameters();

                if (genericArguments != null)
                {
                    var tempMethodGeneric = 0;
                    foreach (var methodGenericArg in genericArguments)
                    {
                        methodGenericMap[methodGenericArg.Name] = tempMethodGeneric++;
                    }

                    if (method.IsGenericMethodDefinition ||
                        !method.GetGenericMethodDefinition().ReturnParameter.ParameterType
                        .IsGenericParameter)
                    {
                        key.Append($"{MethodTick}{genericArguments.Length}");
                    }
                    else
                    {
                        var closure = ParseTypeParameters(
                                methodGenericMap,
                                typeGenericMap,
                                method.ReturnParameter.ParameterType);
                        key.Append($"{{{closure}}}");
                    }
                }
            }
            else if (member is ConstructorInfo ctor)
            {
                isStatic = ctor.IsStatic;
                parameterInfos = ctor.GetParameters();
            }

            var parameters = ParseParameters(methodGenericMap, typeGenericMap, parameterInfos);

            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                    if (isStatic)
                    {
                        key.Replace($".{StaticCtor}", $"#{StaticCtor}");
                    }
                    else
                    {
                        key.Replace($".{Ctor}", $"#{Ctor}");
                    }

                    goto case MemberTypes.Method;

                case MemberTypes.Method:
                    prefixCode = MethodCode;
                    string paramTypesList = string.Join(
                        ",",
                        parameters.ToArray());
                    if (!string.IsNullOrEmpty(paramTypesList))
                    {
                        key.Append($"({paramTypesList})");
                    }

                    break;

                case MemberTypes.Event: prefixCode = EventCode; break;

                case MemberTypes.Field: prefixCode = FieldCode; break;

                case MemberTypes.NestedType:
                    goto case MemberTypes.TypeInfo;

                case MemberTypes.TypeInfo:
                    prefixCode = TypeCode;
                    break;

                case MemberTypes.Property:
                    if ((member as PropertyInfo).GetIndexParameters().Any())
                    {
                        var prop = member as PropertyInfo;
                        var empty = new Dictionary<string, int>();
                        var indexerParms = ParseParameters(empty, empty, prop.GetIndexParameters());
                        var indexTypesList = string.Join(
                            ",",
                            indexerParms.ToArray());
                        if (!string.IsNullOrEmpty(indexTypesList))
                        {
                            key.Append($"({indexTypesList})");
                        }
                    }

                    prefixCode = PropertyCode;
                    break;

                default:
                    throw new ArgumentException("Unknown member type", "member");
            }

            var memberName = $"{prefixCode}:{key}";

            if (!cache.ContainsKey(memberName))
            {
                SafeAdd(memberName, member);
            }

            return memberName;
        }

        /// <summary>
        /// Clears all caches. Primary for testing.
        /// </summary>
        public void Reset()
        {
            cache.Clear();
            reverseCache.Clear();
        }

        /// <summary>
        /// Uses the key to build the proper <see cref="MemberInfo"/> reference.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="MemberInfo"/>.</returns>
        public MemberInfo GetMemberForKey(string key)
        {
            Ensure.NotNullOrWhitespace(() => key);

            var result = default(MemberInfo);

            if (TryGetMemberInfo(key, out MemberInfo member))
            {
                result = member;
            }

            return result;
        }

        /// <summary>
        /// Translate key to specific type.
        /// </summary>
        /// <typeparam name="TMemberInfo">The type of <see cref="MemberInfo"/> to build.</typeparam>
        /// <param name="key">The key to use.</param>
        /// <returns>The <see cref="MemberInfo"/>.</returns>
        public TMemberInfo GetMemberForKey<TMemberInfo>(string key)
            where TMemberInfo : MemberInfo =>
            GetMemberForKey(key) as TMemberInfo;

        /// <summary>
        /// Counts closed generics to provide a good type name.
        /// </summary>
        /// <param name="key">The key to parse.</param>
        /// <returns>The generics count.</returns>
        public int ClosedGenericsCount(string key)
        {
            var genericCount = 0;
            var genericLevel = 0;
            foreach (var chr in key.AsEnumerable())
            {
                switch (chr)
                {
                    case '{':
                        genericLevel++;
                        if (genericLevel == 1)
                        {
                            genericCount++;
                        }

                        break;
                    case ',':
                        if (genericLevel == 1)
                        {
                            genericCount++;
                        }

                        break;

                    case '}':
                        genericLevel--;
                        break;
                }
            }

            return genericCount;
        }

        /// <summary>
        /// Processes parameters and annotates based on the type map.
        /// </summary>
        /// <param name="methodGenericMap">The generic map for the method.</param>
        /// <param name="typeGenericMap">The generic map for the type.</param>
        /// <param name="parameters">The list of <see cref="ParameterInfo"/>.</param>
        private IList<string> ParseParameters(
            Dictionary<string, int> methodGenericMap,
            Dictionary<string, int> typeGenericMap,
            ParameterInfo[] parameters)
        {
            var result = new List<string>();

            foreach (var parameter in parameters)
            {
                var paramType = parameter.ParameterType;
                var param = ParseTypeParameters(methodGenericMap, typeGenericMap, paramType);
                result.Add(param);
            }

            return result;
        }

        /// <summary>
        /// Parses the ctor info for an anonymous type.
        /// </summary>
        /// <param name="ctor">The <see cref="ConstructorInfo"/>.</param>
        /// <returns>The anonymous type parameters.</returns>
        private string ParseAnonymousTypes(ConstructorInfo ctor)
        {
            var empty = new Dictionary<string, int>();
            var ctorParams = ctor.GetParameters();
            var parameters = ParseParameters(empty, empty, ctor.GetParameters());
            var result = new List<string>();
            for (var idx = 0; idx < ctorParams.Length; idx += 1)
            {
                result.Add($"{ctorParams[idx].Name}={parameters[idx]}");
            }

            return string.Join(",", result);
        }

        /// <summary>
        /// Parse the type parameters.
        /// </summary>
        /// <param name="methodGenericMap">The method map of generics.</param>
        /// <param name="typeGenericMap">The type map of generics.</param>
        /// <param name="paramType">The <see cref="Type"/> to parse.</param>
        /// <returns>The qualified name with parameters.</returns>
        private string ParseTypeParameters(
            Dictionary<string, int> methodGenericMap,
            Dictionary<string, int> typeGenericMap,
            Type paramType)
        {
            var param = string.Empty;

            if (paramType.HasElementType)
            {
                if (paramType.IsArray)
                {
                    param = $"{paramType.FullName}";
                }
                else if (paramType.IsPointer)
                {
                    param = $"{paramType.FullName}*";
                }
                else if (paramType.IsByRef)
                {
                    param = $"{paramType.FullName}".Replace("&", "@");
                }
            }
            else if (paramType.FullName == null && typeGenericMap.ContainsKey(paramType.Name))
            {
                return $"{TypeTick}{typeGenericMap[paramType.Name]}";
            }
            else if (paramType.FullName == null && methodGenericMap.ContainsKey(paramType.Name))
            {
                return $"{MethodTick}{methodGenericMap[paramType.Name]}";
            }
            else
            {
                var isGeneric = false;
                var fullname = NameOfType(paramType);

                if (paramType.GenericTypeArguments.Length == 0)
                {
                    return fullname;
                }

                if (fullname.IndexOf(TypeTick[0]) > 0)
                {
                    var nameNoTicks = fullname.Substring(0, fullname.IndexOf(TypeTick[0]));
                    var paramBuilder = new StringBuilder(nameNoTicks);

                    paramBuilder.Append("{");
                    isGeneric = true;
                    var first = true;

                    foreach (var parameter in paramType.GetGenericArguments())
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            paramBuilder.Append(",");
                        }

                        if (parameter.IsGenericParameter || parameter.GenericTypeArguments.Length > 0)
                        {
                            var parsed = ParseTypeParameters(methodGenericMap, typeGenericMap, parameter);
                            paramBuilder.Append(parsed);
                        }
                        else
                        {
                            paramBuilder.Append(NameOfType(parameter));
                        }
                    }

                    fullname = paramBuilder.ToString();
                }

                param = isGeneric ? $"{fullname}}}" : fullname;
            }

            param = param ?? paramType.FullName ?? paramType.Name;

            return param;
        }

        /// <summary>
        /// Try to get member info from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="member">The member.</param>
        /// <returns>A value indicating whether or not the member was found in the cache.</returns>
        private bool TryGetMemberInfo(string key, out MemberInfo member)
        {
            member = null;
            var found = false;

            if (cache.ContainsKey(key))
            {
                member = cache[key];
                found = true;
            }
            else
            {
                var parts = key.Split(':');

                if (parts.Length != 2 ||
                    string.IsNullOrWhiteSpace(parts[0]) ||
                    string.IsNullOrWhiteSpace(parts[1]))
                {
                    throw new ArgumentException(key, nameof(key));
                }

                var memberType = parts[0][0];
                var memberDefinition = parts[1];
                switch (memberType)
                {
                    case TypeCode:
                        found = TryGetType(memberDefinition, out Type type);
                        member = type;
                        break;

                    case MethodCode:
                        if (memberDefinition.Contains($"#{Ctor}") ||
                            memberDefinition.Contains($"#{StaticCtor}"))
                        {
                            found = TryGetCtor(memberDefinition, out ConstructorInfo ctor);
                            member = ctor;
                        }
                        else
                        {
                            found = TryGetMethod(memberDefinition, out MethodInfo method);
                            member = method;
                        }

                        break;

                    case PropertyCode:
                        found = TryGetProperty(memberDefinition, out PropertyInfo property);
                        member = property;
                        break;

                    case FieldCode:
                        found = TryGetField(memberDefinition, out FieldInfo field);
                        member = field;
                        break;
                }
            }

            if (found)
            {
                SafeAdd(key, member);
            }

            return found;
        }

        /// <summary>
        /// Try to resolve a method.
        /// </summary>
        /// <param name="key">The method key.</param>
        /// <param name="method">The <see cref="MethodInfo"/>.</param>
        /// <returns>A value indicating whether the method was found.</returns>
        private bool TryGetMethod(string key, out MethodInfo method)
        {
            method = null;
            var parameterStart = key.IndexOf("(");
            if (parameterStart < 0)
            {
                parameterStart = key.Length;
            }

            var typeAndMethod = key.Substring(0, parameterStart);
            var genericOpen = typeAndMethod.IndexOf('{');
            int methodPos;
            if (genericOpen < 0)
            {
                methodPos = typeAndMethod.LastIndexOf('.');
            }
            else
            {
                methodPos = typeAndMethod.Substring(0, genericOpen).LastIndexOf('.');
            }

            var typeKey = typeAndMethod.Substring(0, methodPos);

            if (TryGetType(typeKey, out Type methodType))
            {
                var typeArgs = GenericArgs(methodType);
                var methodName = typeAndMethod.Substring(methodPos + 1);
                var tick = methodName.IndexOf(MethodTick);

                var genericMethodArgsCount = 0;

                if (genericOpen > 0)
                {
                    genericMethodArgsCount = ClosedGenericsCount(methodName);
                    var methodOpen = methodName.IndexOf('{');
                    if (methodOpen > 0)
                    {
                        methodName = methodName.Substring(0, methodOpen);
                    }
                }

                if (tick > 0)
                {
                    var ticks = methodName.Substring(tick);
                    genericMethodArgsCount += int.Parse(ticks.Replace(MethodTick, string.Empty));
                    methodName = methodName.Substring(0, tick);
                }

                if (parameterStart > 0)
                {
                    var parameters = key.Substring(parameterStart);
                    var methods = methodType.GetMethods(all)
                        .Where(m =>
                        m.Name == methodName).ToList();

                    foreach (var candidate in methods)
                    {
                        var methodCheck = candidate;

                        if (methodCheck.IsGenericMethod && genericMethodArgsCount == 0)
                        {
                            continue;
                        }

                        var methodArgs = candidate.IsGenericMethod ? candidate.GetGenericArguments()
                            : new Type[0];
                        var parameterTypes = ProcessParameters(typeArgs, methodArgs, parameters);

                        var match = true;

                        var methodTypes = methodCheck.GetParameters()
                            .Select(p => p.ParameterType).ToArray();

                        for (var idx = 0; idx < parameterTypes.Length && match; idx++)
                        {
                            if (methodTypes[idx] != parameterTypes[idx])
                            {
                                match = false;
                            }
                        }

                        Type[] methodClosingTypes = new Type[0];
                        if (genericOpen > 0)
                        {
                            var methodParameters = key.Substring(genericOpen);
                            methodParameters = methodParameters.Substring(
                                0,
                                methodParameters.IndexOf('('));
                            methodClosingTypes = ProcessParameters(typeArgs, methodArgs, methodParameters);
                        }

                        if (match)
                        {
                            if (methodClosingTypes.Length > 0)
                            {
                                method = methodCheck.MakeGenericMethod(methodClosingTypes);
                            }
                            else
                            {
                                method = methodCheck;
                            }

                            break;
                        }
                        else if (methodCheck.IsGenericMethod && methodCheck.IsGenericMethodDefinition)
                        {
                            if (genericOpen > 0)
                            {
                                if (methodClosingTypes.Length == methodArgs.Length)
                                {
                                    method = methodCheck.MakeGenericMethod(methodClosingTypes);
                                    match = true;
                                    break;
                                }
                            }

                            parameterTypes = parameterTypes.Union(methodClosingTypes).ToArray();

                            var genericArgs = methodCheck.GetGenericArguments();
                            var typeList = new Type[genericArgs.Length];
                            var methodParameters = methodCheck.GetParameters()
                                .Select(p => p.ParameterType)
                                .Union(new[] { methodCheck.ReturnParameter.ParameterType })
                                .ToArray();
                            RecurseMethodArguments(
                                genericArgs,
                                methodParameters,
                                parameterTypes,
                                typeList);

                            method = methodCheck.MakeGenericMethod(typeList);
                            break;
                        }
                    }
                }
                else
                {
                    method = methodType.GetMethods(all)
                        .Single(c => c.GetParameters().Length == 0);
                }
            }

            return method != null;
        }

        /// <summary>
        /// Recurses the method to match the generic types.
        /// </summary>
        /// <param name="genericArgs">Top-level map for method.</param>
        /// <param name="methodParameters">Set of types to process.</param>
        /// <param name="parameterTypes">Set of resolved types to match.</param>
        /// <param name="typeList">The list of types to close the generic method.</param>
        private void RecurseMethodArguments(
            Type[] genericArgs,
            Type[] methodParameters,
            Type[] parameterTypes,
            Type[] typeList)
        {
            for (var idx = 0; idx < methodParameters.Length; idx += 1)
            {
                if (typeList.Any(t => t == null))
                {
                    if (genericArgs.Contains(methodParameters[idx]))
                    {
                        var genericIdx = Array.IndexOf(genericArgs, methodParameters[idx]);
                        typeList[genericIdx] = parameterTypes[idx];
                        if (!typeList.Any(t => t == null))
                        {
                            break;
                        }
                    }
                    else if (methodParameters[idx].IsGenericType)
                    {
                        RecurseMethodArguments(
                            genericArgs,
                            methodParameters[idx].GetGenericArguments(),
                            parameterTypes[idx].GenericTypeArguments,
                            typeList);
                    }
                }
            }
        }

        /// <summary>
        /// Parse generic arguments.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>The generic arguments list.</returns>
        private Type[] GenericArgs(Type type) =>
            type.IsGenericType ? type.GetGenericArguments() : new Type[0];

        /// <summary>
        /// Tries to get a type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="type">The resolved <see cref="Type"/>.</param>
        /// <returns>A value indicating whether the type was found.</returns>
        private bool TryGetType(string key, out Type type)
        {
            var cacheCheck = $"{TypeCode}:{key}";
            if (cache.ContainsKey($"{TypeCode}:{key}"))
            {
                type = cache[cacheCheck] as TypeInfo;
                return true;
            }

            if (key.StartsWith("<>") && key.Contains("Anonymous"))
            {
                type = typeof(ExpandoObject);
                return true;
            }

            var closedGeneric = key.IndexOf("{");

            var typeName = closedGeneric > 0 ?
                key.Substring(0, closedGeneric)
                : key;

            if (closedGeneric > 0)
            {
                var generics = ClosedGenericsCount(key);
                if (generics > 0)
                {
                    typeName = $"{typeName}{TypeTick}{generics}";
                }
            }

            type = Type.GetType(typeName);

            if (type == null)
            {
                type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName == typeName);
            }

            if (typeName.IndexOf('[') > 0)
            {
                var arrayTypeName = typeName.Substring(0, typeName.IndexOf('['));
                if (TryGetType(arrayTypeName, out Type elementType))
                {
                    type = elementType.MakeArrayType();
                }
            }

            if (closedGeneric > 0)
            {
                var typeArgs = type == null ? new Type[0] : GenericArgs(type);
                var types = ProcessParameters(typeArgs, new Type[0], key.Substring(closedGeneric));
                if (types?.Length > 0)
                {
                    if (type == null)
                    {
                        typeName = $"{typeName}`{types.Length}";
                        TryGetType(typeName, out type);
                    }

                    if (type != null)
                    {
                        type = type.MakeGenericType(types);
                    }
                }
            }

            if (type != null)
            {
                SafeAdd(cacheCheck, type);
            }

            return type != null;
        }

        /// <summary>
        /// Logic to get a field or property.
        /// </summary>
        /// <typeparam name="TMemberInfo">The type of info to get.</typeparam>
        /// <param name="getList">A delegate that returns the desired types.</param>
        /// <param name="memberDefinition">The portion of the key being parsed.</param>
        /// <param name="info">The member to return.</param>
        /// <returns>A value indicating whether the member was found.</returns>
        private bool TryGetFieldOrProperty<TMemberInfo>(
            Func<Type, BindingFlags, MemberInfo[]> getList,
            string memberDefinition,
            out TMemberInfo info)
            where TMemberInfo : MemberInfo
        {
            if (typeof(TMemberInfo) == typeof(PropertyInfo)
                && memberDefinition.IndexOf('(') > 0)
            {
                info = GetIndexerProperty(memberDefinition)
                    as TMemberInfo;
            }
            else
            {
                info = null;
                var pos = memberDefinition.LastIndexOf('.');
                var typeKey = memberDefinition.Substring(0, pos);
                var name = memberDefinition.Substring(pos + 1);
                if (TryGetType(typeKey, out Type owningType))
                {
                    info = (TMemberInfo)getList(
                        owningType,
                        all).Single(m => m.Name == name);
                }
            }

            return info != null;
        }

        /// <summary>
        /// Gets the indexer property.
        /// </summary>
        /// <param name="memberDefinition">The member definition to parse.</param>
        /// <returns>The <see cref="PropertyInfo"/> of the indexer.</returns>
        private PropertyInfo GetIndexerProperty(string memberDefinition)
        {
            var result = default(PropertyInfo);
            var typeAndProperty =
                memberDefinition.Substring(
                    0,
                    memberDefinition.IndexOf('('));
            var pos = typeAndProperty.LastIndexOf('.');
            var typeKey = typeAndProperty.Substring(0, pos);
            if (TryGetType(typeKey, out Type owningType))
            {
                result = owningType.GetProperties()
                    .SingleOrDefault(p => p.GetIndexParameters().Length > 0);
            }

            return result;
        }

        /// <summary>
        /// Logic to resolve a field.
        /// </summary>
        /// <param name="memberDefinition">The key selector.</param>
        /// <param name="field">The <see cref="FieldInfo"/>.</param>
        /// <returns>A value indicating whether the field was found.</returns>
        private bool TryGetField(string memberDefinition, out FieldInfo field) =>
            TryGetFieldOrProperty(
                (type, flags) => type.GetFields(flags),
                memberDefinition,
                out field);

        /// <summary>
        /// Logic to resolve a property.
        /// </summary>
        /// <param name="memberDefinition">The key selector.</param>
        /// <param name="property">The <see cref="PropertyInfo"/>.</param>
        /// <returns>A value indicating whether the property was found.</returns>
        private bool TryGetProperty(string memberDefinition, out PropertyInfo property) =>
            TryGetFieldOrProperty(
                (type, flags) => type.GetProperties(flags),
                memberDefinition,
                out property);

        /// <summary>
        /// Logic to resolve a constructor.
        /// </summary>
        /// <param name="key">The key selector.</param>
        /// <param name="ctor">The <see cref="ConstructorInfo"/>.</param>
        /// <returns>A value indicating whether the constructor was found.</returns>
        private bool TryGetCtor(string key, out ConstructorInfo ctor)
        {
            ctor = null;
            var typeKey = key.Substring(0, key.IndexOf("#") - 1);
            var isStatic = key.Contains(StaticCtor);
            var flags = BindingFlags.Public | BindingFlags.NonPublic |
                (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            if (TryGetType(typeKey, out Type ctorType))
            {
                var parameterStart = key.IndexOf("(");
                if (parameterStart > 0)
                {
                    var parameters = key.Substring(parameterStart);
                    var types = ProcessParameters(GenericArgs(ctorType), new Type[0], parameters);
                    var ctors = ctorType.GetConstructors(flags)
                        .Where(c => c.GetParameters().Length == types.Length);
                    foreach (var candidate in ctors)
                    {
                        var ctorTypes = candidate.GetParameters()
                            .Select(p => p.ParameterType).ToArray();
                        var match = true;
                        for (var idx = 0; idx < types.Length && match; idx++)
                        {
                            if (ctorTypes[idx] != types[idx])
                            {
                                match = false;
                            }
                        }

                        if (match)
                        {
                            ctor = candidate;
                            break;
                        }
                    }
                }
                else
                {
                    ctor = ctorType.GetConstructors(flags)
                        .Single(c => c.GetParameters().Length == 0);
                }
            }

            return ctor != null;
        }

        /// <summary>
        /// Finds the next non-numeric position.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="start">The start for parsing.</param>
        /// <returns>The position of the next non-numeric character.</returns>
        private int IndexOfNextNonNumeric(string str, int start)
        {
            for (var idx = start; idx < str.Length; idx++)
            {
                if ("01234567890".Contains(str[idx]))
                {
                    continue;
                }

                return idx;
            }

            return -1;
        }

        /// <summary>
        /// Finds the closing brace for a generic type definition with handling for nested generics.
        /// </summary>
        /// <param name="str">The string to process.</param>
        /// <returns>The index of the outer closure.</returns>
        private int IndexOfGenericClosure(string str)
        {
            var braceCount = 0;
            for (var idx = 0; idx < str.Length; idx++)
            {
                if (str[idx] == '{')
                {
                    braceCount++;
                }

                if (str[idx] == '}')
                {
                    braceCount--;
                }

                if (braceCount == 0)
                {
                    return idx;
                }
            }

            return str.Length;
        }

        /// <summary>
        /// Processes type parameters to respective types.
        /// </summary>
        /// <param name="typeArgs">Type generic type arguments.</param>
        /// <param name="methodArgs">Method generic type arguments.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The list of types to close the generic.</returns>
        private Type[] ProcessParameters(Type[] typeArgs, Type[] methodArgs, string parameters)
        {
            var result = new List<Type>();

            while (parameters.Length > 0)
            {
                if ("}{(),".Contains(parameters[0]))
                {
                    parameters = parameters.Length > 1 ?
                        parameters.Substring(1) : string.Empty;
                    continue;
                }

                if (parameters.IndexOf(MethodTick) == 0)
                {
                    var next = IndexOfNextNonNumeric(parameters, 2);
                    var end = next > 0 ? next - 2 : parameters.Length - 2;
                    var pos = parameters.Substring(2, end);
                    result.Add(methodArgs[int.Parse(pos)]);
                    parameters = next > 0 ?
                        parameters.Substring(next) : string.Empty;
                    continue;
                }
                else if (parameters.IndexOf(TypeTick) == 0)
                {
                    var next = IndexOfNextNonNumeric(parameters, 1);
                    var end = next > 0 ? next - 1 : parameters.Length - 1;
                    var pos = parameters.Substring(1, end);
                    result.Add(typeArgs[int.Parse(pos)]);
                    parameters = next > 0 ?
                        parameters.Substring(next) : string.Empty;
                    continue;
                }
                else
                {
                    var generic = parameters.IndexOf('{');
                    var next = parameters.IndexOf(',');
                    if (next < 0)
                    {
                        next = parameters.IndexOf(")");
                        if (next < 0)
                        {
                            next = parameters.IndexOf('}');
                        }
                    }

                    var isGeneric = generic > 0 &&
                        (next < 0 || generic < next);
                    var pos = isGeneric ? generic :
                        next < 0 ? parameters.Length : next;

                    var type = parameters.Substring(0, pos);

                    Type[] typeParams = new Type[0];
                    int closure = -1;

                    if (isGeneric)
                    {
                        parameters = parameters.Substring(generic);
                        closure = IndexOfGenericClosure(parameters);
                        var genericParameters = parameters.Substring(0, closure);
                        typeParams = ProcessParameters(typeArgs, methodArgs, genericParameters);
                        if (typeParams.Length > 0)
                        {
                            type = $"{type}{TypeTick}{typeParams.Length}";
                        }
                    }

                    if (TryGetType(type, out Type typeParam))
                    {
                        if (isGeneric && typeParam.GetGenericArguments().Length == typeParams.Length)
                        {
                            result.Add(typeParam.MakeGenericType(typeParams));
                            next = closure;
                        }
                        else
                        {
                            result.Add(typeParam);
                        }
                    }

                    if (next < 0 || next > parameters.Length)
                    {
                        break;
                    }

                    parameters = parameters.Substring(next);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Resolves the type name.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>The functional name to use.</returns>
        private string NameOfType(Type type)
        {
            var fullName = type.FullName;
            if (fullName != null)
            {
                if (type.IsArray)
                {
                    var close = fullName.IndexOf(']');
                    if (close == fullName.Length)
                    {
                        return fullName;
                    }

                    return fullName.Substring(0, close + 1);
                }

                return fullName.IndexOf('[') < 0 ?
                    fullName :
                    fullName.Substring(0, fullName.IndexOf('['));
            }

            if (type.IsGenericType)
            {
                return $"{type.Namespace}.{type.Name}";
            }

            return type.Name;
        }

        /// <summary>
        /// Safely add a key to the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="member">The related <see cref="MemberInfo"/>.</param>
        private void SafeAdd(string key, MemberInfo member)
        {
            if (cache.ContainsKey(key))
            {
                return;
            }

            reverseCache.TryAdd(member.GetHashCode(), key);
            cache.TryAdd(key, member);
        }
    }
}
