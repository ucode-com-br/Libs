using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace UCode.Extensions.CodeGenerator
{
    public partial class DeepCloneableClass<T> : IDeepCloneable<T>
    {
        /// <summary>
        /// Manual deep clone implementation, mimicking what the generator would produce.
        /// </summary>
        public T? DeepClone()
        {
            // Example approach using the same runtime logic from the generator:
            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            return CloneRuntimeHelper.DeepCloneRuntime<T>((T?)(object?)this, visited);
        }
    }

    public partial record DeepCloneableRecord<T> : IDeepCloneable<T>
    {
        /// <summary>
        /// Manual deep clone implementation, mimicking what the generator would produce.
        /// </summary>
        public T? DeepClone()
        {
            // Example approach using the same runtime logic from the generator:
            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            return CloneRuntimeHelper.DeepCloneRuntime<T>((T?)(object?)this, visited);
        }

    }

    public partial struct DeepCloneableStruct<T> : IDeepCloneable<T>
    {
        /// <summary>
        /// Manual deep clone implementation, mimicking what the generator would produce.
        /// </summary>
        public T? DeepClone()
        {
            // Example approach using the same runtime logic from the generator:
            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            return CloneRuntimeHelper.DeepCloneRuntime<T>((T?)(object?)this, visited);
        }
    }



    /// <summary>
    /// Attribute that indicates a type (class, struct, or interface) should have a deep clone method generated.
    /// </summary>
    /// <remarks>
    /// If you apply this attribute, the <see cref="DeepCloneSourceGenerator"/> will generate code that provides
    /// a deep cloning method (or extension) based on your type's characteristics (partial, sealed, etc.).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
    public sealed class DeepCloneableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeepCloneableAttribute"/> class.
        /// </summary>
        public DeepCloneableAttribute()
        {
        }
    }

    /// <summary>
    /// Non-generic interface used to signal that a type is deep-cloneable.
    /// </summary>
    /// <remarks>
    /// If your type implements this interface, the <see cref="DeepCloneSourceGenerator"/> 
    /// will consider it for code generation of a deep clone method.
    /// </remarks>
    public interface IDeepCloneable : IDeepCloneable<object>
    {
    }

    /// <summary>
    /// Generic interface used to signal that a type is deep-cloneable.
    /// </summary>
    /// <typeparam name="T">The type to be cloned.</typeparam>
    /// <remarks>
    /// If your type implements <c>IDeepCloneable&lt;T&gt;</c>, the <see cref="DeepCloneSourceGenerator"/>
    /// will generate the appropriate cloning code or extension methods for it.
    /// </remarks>
    public interface IDeepCloneable<T>
    {
        // If you want your own explicit "T? Clone();" signature, you can add it here.
        // For demonstration, we leave it empty because the generator will create the clone method.
    }

    #region SOURCE_GENERATOR

    /// <summary>
    /// Roslyn Source Generator that finds types marked with <see cref="DeepCloneableAttribute"/> 
    /// or implementing <see cref="IDeepCloneable"/> or <see cref="IDeepCloneable{T}"/>,
    /// and generates code for a deep clone method.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This generator will inspect your project's syntax trees. When it finds a type that either:
    /// 1) Has the <see cref="DeepCloneableAttribute"/>, or
    /// 2) Implements <see cref="IDeepCloneable"/> (or <c>IDeepCloneable&lt;T&gt;</c>),
    /// it will generate code to provide a deep clone method.
    /// </para>
    /// <para>
    /// If the type is:
    /// <list type="bullet">
    ///   <item><description>an <c>interface</c>, an extension method is generated.</description></item>
    ///   <item><description>a <c>partial class/struct</c>, the method is injected inline as <c>DeepClone()</c>.</description></item>
    ///   <item><description>a <c>sealed</c> class (non-partial), a JSON-based static method is generated.</description></item>
    ///   <item><description>any other non-partial class, a new class is created which inherits from it (if possible) to expose a static <c>Clone()</c> method.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Refer to <see cref="CloneRuntimeHelper"/> for the actual deep cloning logic. 
    /// This logic preserves cyclic references using a <c>Dictionary&lt;object, object&gt;</c> with
    /// <see cref="ReferenceEqualityComparer"/>.
    /// </para>
    /// </remarks>
    [Generator]
    public class DeepCloneSourceGenerator : ISourceGenerator
    {
        /// <summary>
        /// Initializes the source generator. Typically used for registering syntax receivers 
        /// or additional steps. Not used in this example.
        /// </summary>
        /// <param name="context">The <see cref="GeneratorInitializationContext"/> for initialization.</param>
        public void Initialize(GeneratorInitializationContext context)
        {
            // Uncomment if you want to debug the source generator:
            // if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();
        }

        /// <summary>
        /// The main entry point for the source generator. It iterates over syntax trees in the compilation,
        /// finds eligible types, and generates the appropriate code files.
        /// </summary>
        /// <param name="context">The <see cref="GeneratorExecutionContext"/> that provides access to the compilation and other features.</param>
        public void Execute(GeneratorExecutionContext context)
        {
            var allSyntaxTrees = context.Compilation.SyntaxTrees;
            foreach (var syntaxTree in allSyntaxTrees)
            {
                var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
                var root = syntaxTree.GetRoot(context.CancellationToken);

                // Retrieves all type declarations (classes, structs, interfaces).
                var typeDeclarations = root.DescendantNodes()
                                           .OfType<TypeDeclarationSyntax>();

                foreach (var decl in typeDeclarations)
                {
                    // 1) Check if it has [DeepCloneable]
                    // OR
                    // 2) Check if it implements IDeepCloneable / IDeepCloneable<T>
                    if (!IsDeepCloneCandidate(decl, semanticModel, context.CancellationToken))
                        continue;

                    // Obtain the symbol representation of the type
                    var typeSymbol = semanticModel.GetDeclaredSymbol(decl, context.CancellationToken);
                    if (typeSymbol == null)
                        continue;

                    // If it's an interface, generate an extension method
                    if (typeSymbol.TypeKind == TypeKind.Interface)
                    {
                        var generatedCode = GenerateForInterface(decl, typeSymbol);
                        context.AddSource($"{typeSymbol.Name}_DeepCloneInterface.g.cs", generatedCode);
                        continue;
                    }

                    // Otherwise, it's a class or struct
                    bool isPartial = decl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
                    bool isSealed = typeSymbol.IsSealed;

                    if (isPartial)
                    {
                        // If partial => inject method inline
                        var generatedCode = GenerateInlineMethod(decl, typeSymbol);
                        context.AddSource($"{typeSymbol.Name}_DeepClonePartial.g.cs", generatedCode);
                    }
                    else
                    {
                        // Not partial
                        if (isSealed)
                        {
                            // If sealed => fallback to JSON approach
                            var generatedCode = GenerateJsonCloneStaticMethod(decl, typeSymbol);
                            context.AddSource($"{typeSymbol.Name}_DeepCloneSealed.g.cs", generatedCode);
                        }
                        else
                        {
                            // Not partial and not sealed => generate an inherited class
                            var generatedCode = GenerateInheritedCloneClass(decl, typeSymbol);
                            context.AddSource($"{typeSymbol.Name}_DeepCloneInherited.g.cs", generatedCode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the given type declaration is a candidate for deep clone code generation.
        /// </summary>
        /// <param name="typeDecl">The syntax node representing the type declaration.</param>
        /// <param name="semanticModel">The semantic model used to analyze the type.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>True if it has <see cref="DeepCloneableAttribute"/> or implements <see cref="IDeepCloneable"/>; otherwise false.</returns>
        private static bool IsDeepCloneCandidate(
            TypeDeclarationSyntax typeDecl,
            SemanticModel semanticModel,
            CancellationToken ct)
        {
            // 1) Check attribute
            if (HasDeepCloneableAttribute(typeDecl, semanticModel, ct))
                return true;

            // 2) Check implemented interfaces
            var symbol = semanticModel.GetDeclaredSymbol(typeDecl, ct);
            if (symbol == null)
                return false;

            foreach (var iface in symbol.AllInterfaces)
            {
                // E.g. "DeepCloneGenerator.IDeepCloneable"
                // or "DeepCloneGenerator.IDeepCloneable<T>"
                var name = iface.ToDisplayString();
                if (name == "DeepCloneGenerator.IDeepCloneable" ||
                    name.StartsWith("DeepCloneGenerator.IDeepCloneable<"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the type has the <see cref="DeepCloneableAttribute"/>.
        /// </summary>
        private static bool HasDeepCloneableAttribute(
            TypeDeclarationSyntax typeDecl,
            SemanticModel semanticModel,
            CancellationToken ct)
        {
            foreach (var list in typeDecl.AttributeLists)
            {
                foreach (var attr in list.Attributes)
                {
                    var attrSymbol = semanticModel.GetSymbolInfo(attr, ct).Symbol;
                    if (attrSymbol == null)
                        continue;

                    var containingType = attrSymbol.ContainingType;
                    if (containingType == null)
                        continue;

                    var fullName = containingType.ToDisplayString();
                    if (fullName == "DeepCloneGenerator.DeepCloneableAttribute")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #region 1) INTERFACE

        /// <summary>
        /// Generates code for an interface, producing an extension method named DeepClone().
        /// </summary>
        /// <param name="decl">The syntax node for the interface declaration.</param>
        /// <param name="typeSymbol">The symbol for the interface.</param>
        /// <returns>The generated source code as a string.</returns>
        private static string GenerateForInterface(TypeDeclarationSyntax decl, INamedTypeSymbol typeSymbol)
        {
            // Example of generated code:
            // public static class FooDeepCloneExtensions
            // {
            //     public static IFoo? DeepClone(this IFoo? obj)
            //     {
            //         if (obj == null) return default;
            //         ...
            //     }
            // }

            var namespaceName = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : typeSymbol.ContainingNamespace.ToDisplayString();

            var extensionClassName = $"{SanitizeName(typeSymbol.Name)}DeepCloneExtensions";
            var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
            }

            _ = sb.AppendLine($"    public static class {extensionClassName}");
            _ = sb.AppendLine("    {");
            _ = sb.AppendLine($"        /// <summary>");
            _ = sb.AppendLine($"        /// Creates a deep clone of this <see cref=\"{typeName}\"/> instance, preserving reference cycles.");
            _ = sb.AppendLine($"        /// </summary>");
            _ = sb.AppendLine($"        /// <param name=\"obj\">The interface instance to clone.</param>");
            _ = sb.AppendLine($"        /// <returns>A deeply cloned copy of the original instance, or null if <paramref name=\"obj\"/> is null.</returns>");
            _ = sb.AppendLine($"        public static {typeName}? DeepClone(this {typeName}? obj)");
            _ = sb.AppendLine("        {");
            _ = sb.AppendLine("            if (obj == null) return default;");
            _ = sb.AppendLine("            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);");
            _ = sb.AppendLine("            var clone = CloneRuntimeHelper.DeepCloneRuntime(obj, visited);");
            _ = sb.AppendLine($"            return ({typeName}?)clone;");
            _ = sb.AppendLine("        }");
            _ = sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        #endregion

        #region 2) PARTIAL CLASS/STRUCT

        /// <summary>
        /// Generates a deep clone method inline for a partial class or struct.
        /// </summary>
        /// <param name="decl">The syntax node for the class/struct declaration.</param>
        /// <param name="typeSymbol">The symbol representing the class/struct.</param>
        /// <returns>A string containing the generated C# source.</returns>
        private static string GenerateInlineMethod(TypeDeclarationSyntax decl, INamedTypeSymbol typeSymbol)
        {
            // Example of generated code:
            // public partial class Foo
            // {
            //     public Foo? DeepClone() 
            //     {
            //         var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            //         return (Foo?)CloneRuntimeHelper.DeepCloneRuntime(this, visited);
            //     }
            // }

            var namespaceName = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : typeSymbol.ContainingNamespace.ToDisplayString();

            var typeKindKeyword = decl.Keyword.ValueText; // "class" or "struct"
            var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            var accessibility = AccessibilityToString(typeSymbol.DeclaredAccessibility);

            var baseList = decl.BaseList?.ToFullString() ?? "";
            var typeParams = decl.TypeParameterList?.ToFullString() ?? "";
            var typeConstraints = decl.ConstraintClauses.ToFullString() ?? "";

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    {accessibility} partial {typeKindKeyword} {decl.Identifier}{typeParams} {baseList} {typeConstraints}");
            sb.AppendLine("    {");
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// Creates a deep clone of this <see cref=\"{typeName}\"/> instance, preserving reference cycles.");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine($"        /// <returns>A deeply cloned copy of the current instance.</returns>");
            sb.AppendLine($"        /// <example>");
            sb.AppendLine($"        /// var cloned = myInstance.DeepClone();");
            sb.AppendLine($"        /// </example>");
            sb.AppendLine($"        /// <exception cref=\"InvalidOperationException\">If the type cannot be instantiated.</exception>");
            sb.AppendLine($"        public {typeName}? DeepClone()");
            sb.AppendLine("        {");
            sb.AppendLine("            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);");
            sb.AppendLine($"            return ({typeName}?)CloneRuntimeHelper.DeepCloneRuntime(this, visited);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        #endregion

        #region 3) INHERITED CLASS

        /// <summary>
        /// Generates a new class that inherits from the original (non-partial, non-sealed) class 
        /// and provides a static <c>Clone()</c> method.
        /// </summary>
        /// <param name="decl">The original type declaration syntax.</param>
        /// <param name="typeSymbol">The symbol representing the original class.</param>
        /// <returns>A string containing the generated C# source of the derived class.</returns>
        private static string GenerateInheritedCloneClass(TypeDeclarationSyntax decl, INamedTypeSymbol typeSymbol)
        {
            // Example of generated code:
            // public class FooClone : Foo
            // {
            //     public static Foo? Clone(Foo? original)
            //     {
            //         if (original == null) return null;
            //         ...
            //         return (Foo?)CloneRuntimeHelper.DeepCloneRuntime(original, visited);
            //     }
            // }

            var namespaceName = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : typeSymbol.ContainingNamespace.ToDisplayString();

            var originalTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var cloneClassName = $"{SanitizeName(typeSymbol.Name)}Clone";

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    public class {cloneClassName} : {originalTypeName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// Creates a deep clone of the given <see cref=\"{originalTypeName}\"/> instance, preserving reference cycles.");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine($"        /// <param name=\"original\">The original instance to clone.</param>");
            sb.AppendLine($"        /// <returns>A new <see cref=\"{originalTypeName}\"/> instance or null if <paramref name=\"original\"/> is null.</returns>");
            sb.AppendLine($"        public static {originalTypeName}? Clone({originalTypeName}? original)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (original == null) return null;");
            sb.AppendLine("            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);");
            sb.AppendLine("            return ("
                           + originalTypeName
                           + "?)CloneRuntimeHelper.DeepCloneRuntime(original, visited);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

        #endregion

        #region 4) SEALED => JSON

        /// <summary>
        /// Generates a static class that clones a sealed, non-partial class 
        /// via JSON serialization and deserialization (with reference preservation).
        /// </summary>
        /// <param name="decl">The original sealed type declaration syntax.</param>
        /// <param name="typeSymbol">The symbol for the sealed class.</param>
        /// <returns>A string containing the generated static class with a <c>Clone()</c> method.</returns>
        private static string GenerateJsonCloneStaticMethod(TypeDeclarationSyntax decl, INamedTypeSymbol typeSymbol)
        {
            // Example of generated code:
            // public static class FooJsonCloneHelper
            // {
            //     public static Foo? Clone(Foo? original)
            //     {
            //         if (original == null) return null;
            //         var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve };
            //         ...
            //         return JsonSerializer.Deserialize<Foo>(json, options);
            //     }
            // }

            var namespaceName = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : typeSymbol.ContainingNamespace.ToDisplayString();

            var originalTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var helperClassName = $"{SanitizeName(typeSymbol.Name)}JsonCloneHelper";

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    public static class {helperClassName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// Creates a deep clone of a sealed <see cref=\"{originalTypeName}\"/> by serializing and deserializing it as JSON.");
            sb.AppendLine($"        /// References are preserved via <c>ReferenceHandler.Preserve</c>.");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine($"        /// <param name=\"original\">The instance to clone, which can be null.</param>");
            sb.AppendLine($"        /// <returns>A cloned <see cref=\"{originalTypeName}\"/> instance, or null if the original is null.</returns>");
            sb.AppendLine($"        /// <exception cref=\"System.Text.Json.JsonException\">If serialization or deserialization fails.</exception>");
            sb.AppendLine($"        public static {originalTypeName}? Clone({originalTypeName}? original)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (original == null) return null;");
            sb.AppendLine("            var options = new System.Text.Json.JsonSerializerOptions");
            sb.AppendLine("            {");
            sb.AppendLine("                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,");
            sb.AppendLine("                WriteIndented = false");
            sb.AppendLine("            };");
            sb.AppendLine("            var json = System.Text.Json.JsonSerializer.Serialize(original, options);");
            sb.AppendLine($"            return System.Text.Json.JsonSerializer.Deserialize<{originalTypeName}>(json, options);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

        #endregion

        /// <summary>
        /// Converts Roslyn's <see cref="Accessibility"/> to a C# keyword (e.g. "public", "internal", etc.).
        /// </summary>
        private static string AccessibilityToString(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.Public => "public",
                _ => "private"
            };
        }

        /// <summary>
        /// Removes non-alphanumeric characters from a name, to produce a valid identifier for code generation.
        /// </summary>
        private static string SanitizeName(string name)
        {
            var sb = new StringBuilder();
            foreach (var ch in name)
            {
                if (char.IsLetterOrDigit(ch))
                    sb.Append(ch);
            }
            return sb.ToString();
        }
    }
    #endregion

    #region RUNTIME_HELPER

    /// <summary>
    /// Helper class responsible for the actual deep clone logic at runtime.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class preserves references by storing previously cloned objects 
    /// in a <see cref="Dictionary{TKey,TValue}"/> keyed by reference
    /// (<see cref="ReferenceEqualityComparer"/>).
    /// </para>
    /// <para>
    /// It handles:
    /// <list type="bullet">
    ///   <item><description>Primitive/simple types (returns them directly).</description></item>
    ///   <item><description>Streams (copies to a <see cref="MemoryStream"/>).</description></item>
    ///   <item><description><c>Span&lt;T&gt;</c> (calls <c>ToArray()</c> if available).</description></item>
    ///   <item><description><see cref="ExpandoObject"/> (clones as a new <see cref="ExpandoObject"/>).</description></item>
    ///   <item><description>Arrays (including multi-dimensional).</description></item>
    ///   <item><description>Dictionaries (and tries to preserve keys/values deeply cloned).</description></item>
    ///   <item><description>Generic collections (IEnumerable&lt;T&gt;).</description></item>
    ///   <item><description>Complex objects (POCOs) via reflection, including private fields.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If the type cannot be constructed via a public or non-public default constructor, 
    /// it uses <see cref="FormatterServices.GetUninitializedObject"/> as a fallback.
    /// </para>
    /// </remarks>
    public static class CloneRuntimeHelper
    {
        public static object? DeepCloneRuntime(object? obj, Dictionary<object, object> visited) => CloneRuntimeHelper.DeepCloneRuntime<object>(obj, visited);

        /// <summary>
        /// Creates a deep clone of the given <paramref name="obj"/> using reflection and special-case logic 
        /// for streams, arrays, etc. Cyclic references are tracked in the <paramref name="visited"/> dictionary.
        /// </summary>
        /// <param name="obj">The object to clone.</param>
        /// <param name="visited">A dictionary tracking already cloned objects.</param>
        /// <returns>A deep clone of <paramref name="obj"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if it is impossible to instantiate the object's type (e.g., no suitable constructor or fallback).
        /// </exception>
        public static T? DeepCloneRuntime<T>(T? obj, Dictionary<object, object> visited)
        {
            if (obj is null)
                return default;

            // If we've already cloned this object, return the existing copy.
            if (visited.TryGetValue(obj, out var existing))
            {
                return (T)existing;
            }

            var type = obj.GetType();

            // Return immediately for simple types (no reference needed).
            if (IsSimpleType(type))
            {
                // Not added to visited because it's immutable or has no reference complexity.
                return obj;
            }

            // Special case for Stream => copy to MemoryStream
            if (obj is Stream s)
            {
                var ms = new MemoryStream();
                var oldPos = s.CanSeek ? s.Position : (long?)null;
                s.CopyTo(ms);
                if (oldPos.HasValue)
                    s.Position = oldPos.Value;
                ms.Position = 0;
                visited[obj] = ms;
                return (T?)(object?)ms;
            }

            // If it's a Span<T>, we call ToArray() if available
            if (type.IsByRefLike && type.Name.StartsWith("Span"))
            {
                var toArrayMethod = type.GetMethod("ToArray", Type.EmptyTypes);
                if (toArrayMethod != null)
                {
                    var array = toArrayMethod.Invoke(obj, null);
                    visited[obj] = array!;
                    return (T?)(object?)array!;
                }
                return obj;
            }

            // If it's an ExpandoObject
            if (obj is ExpandoObject expando)
            {
                var cloneExp = new ExpandoObject();
                visited[obj] = cloneExp;

                var dictExpando = (IDictionary<string, object?>)expando;
                var cloneDict = (IDictionary<string, object?>)cloneExp;
                foreach (var kvp in dictExpando)
                {
                    cloneDict[kvp.Key] = DeepCloneRuntime(kvp.Value!, visited);
                }
                return (T?)(object?)cloneExp;
            }

            // If it's an array (including multi-dimensional)
            if (type.IsArray)
            {
                var arr = (Array)(object)obj;
                var rank = arr.Rank;
                var lengths = Enumerable.Range(0, rank).Select(d => arr.GetLength(d)).ToArray();
                var cloneArr = Array.CreateInstance(type.GetElementType()!, lengths);
                visited[obj] = cloneArr;

                var indices = new int[rank];
                CopyArrayRecursive(arr, cloneArr, 0, indices, visited);
                return (T?)(object?)cloneArr;
            }

            // If it's a non-generic IDictionary
            if (obj is IDictionary dict)
            {
                var dictClone = (IDictionary)Activator.CreateInstance(type)!;
                visited[obj] = dictClone;

                foreach (var key in dict.Keys)
                {
                    var k2 = DeepCloneRuntime(key!, visited);
                    var v2 = DeepCloneRuntime(dict[key]!, visited);
                    dictClone[k2] = v2;
                }
                return (T?)(object?)dictClone;
            }

            // If it's an IEnumerable<T>
            var ienumerableT = GetIEnumerableT(type);
            if (ienumerableT != null)
            {
                object collection;
                var ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor != null && !type.IsAbstract)
                {
                    collection = Activator.CreateInstance(type)!;
                }
                else
                {
                    var listType = typeof(List<>).MakeGenericType(ienumerableT);
                    collection = Activator.CreateInstance(listType)!;
                }
                visited[obj] = collection;

                var addMethod = collection.GetType().GetMethod("Add", new[] { ienumerableT });
                if (addMethod == null)
                {
                    if (collection is IList listObj)
                    {
                        foreach (var item in (IEnumerable)obj)
                        {
                            listObj.Add(DeepCloneRuntime(item!, visited));
                        }
                        return (T?)(object?)collection;
                    }
                    else
                    {
                        // fallback: return a List<object?>
                        var tmpList = new List<object?>();
                        foreach (var item in (IEnumerable)obj)
                        {
                            tmpList.Add(DeepCloneRuntime(item!, visited));
                        }
                        return (T?)(object?)tmpList;
                    }
                }
                else
                {
                    foreach (var item in (IEnumerable)obj)
                    {
                        addMethod.Invoke(collection, new[] { DeepCloneRuntime(item!, visited) });
                    }
                    return (T?)(object?)collection;
                }
            }

            // Otherwise, it's a complex (POCO) type
            object instance;
            // Try default constructor (including private ones)
            var defaultCtor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);

            if (defaultCtor != null && !type.IsAbstract)
            {
                instance = defaultCtor.Invoke(null);
            }
            else
            {
                // fallback => uses FormatterServices to create an uninitialized object
                instance = FormatterServices.GetUninitializedObject(type);
            }

            visited[obj] = instance;

            // Copy all fields (including private), stepping through the hierarchy
            var current = type;
            while (current != null && current != typeof(object))
            {
                var fields = current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var f in fields)
                {
                    if (f.IsStatic)
                        continue;
                    var fieldVal = f.GetValue(obj);
                    var clonedVal = DeepCloneRuntime(fieldVal!, visited);
                    f.SetValue(instance, clonedVal);
                }
                current = current.BaseType;
            }

            return (T?)(object?)instance;
        }

        /// <summary>
        /// Recursively copies elements of a multi-dimensional array, applying deep clone to each element.
        /// </summary>
        private static void CopyArrayRecursive(Array source, Array dest, int dimension, int[] indices, Dictionary<object, object> visited)
        {
            if (dimension == source.Rank)
            {
                var val = source.GetValue(indices);
                var cloned = DeepCloneRuntime(val!, visited);
                dest.SetValue(cloned, indices);
            }
            else
            {
                for (int i = 0; i < source.GetLength(dimension); i++)
                {
                    indices[dimension] = i;
                    CopyArrayRecursive(source, dest, dimension + 1, indices, visited);
                }
            }
        }

        /// <summary>
        /// Determines if the given <paramref name="type"/> is a "simple" type: primitive, enum, string, decimal, etc.
        /// </summary>
        private static bool IsSimpleType(Type type)
        {
            if (type.IsPrimitive)
                return true;
            if (type.IsEnum)
                return true;
            if (type == typeof(string))
                return true;
            if (type == typeof(decimal))
                return true;
            if (type == typeof(DateTime))
                return true;
            if (type == typeof(DateTimeOffset))
                return true;
            if (type == typeof(TimeSpan))
                return true;
            if (type == typeof(Guid))
                return true;
            return false;
        }

        /// <summary>
        /// If <paramref name="type"/> implements <c>IEnumerable&lt;T&gt;</c>, returns the <c>T</c>; otherwise, null.
        /// </summary>
        private static Type? GetIEnumerableT(Type type)
        {
            if (type.IsInterface
                && type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return type.GetGenericArguments()[0];
            }
            foreach (var it in type.GetInterfaces())
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return it.GetGenericArguments()[0];
            }
            return null;
        }
    }

    /// <summary>
    /// A reference-based equality comparer used to track object instances in the <c>visited</c> dictionary.
    /// </summary>
    /// <remarks>
    /// This ensures that if two objects are the same reference, they are considered equal and share the same hash code.
    /// Helpful for preserving object identity in deep clone scenarios with cycles.
    /// </remarks>
    internal sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        /// <summary>
        /// A singleton instance of <see cref="ReferenceEqualityComparer"/>.
        /// </summary>
        public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

        /// <summary>
        /// Determines whether the specified objects are the same reference.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns><c>true</c> if both objects are the exact same reference; otherwise, <c>false</c>.</returns>
        bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

        /// <summary>
        /// Returns a hash code based on the runtime reference (pointer) of the object.
        /// </summary>
        /// <param name="obj">The object for which to get the hash code.</param>
        /// <returns>An integer representing the hash code of the object's reference.</returns>
        int IEqualityComparer<object>.GetHashCode(object obj)
            => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }

    #endregion
}



















//////using System;
//////using System.Collections;
//////using System.Collections.Concurrent;
//////using System.Collections.Generic;
//////using System.Dynamic;
//////using System.IO;
//////using System.Linq;
//////using System.Linq.Expressions;
//////using System.Reflection;
//////using Microsoft.CodeAnalysis.CSharp.Syntax;
//////using Microsoft.CodeAnalysis.CSharp;
//////using Microsoft.CodeAnalysis;
//////using System.Runtime.Serialization;
//////using System.Text;
//////using System.Threading;

//////namespace UCode.Extensions.CodeGenerator
//////{
//////    /// <summary>
//////    /// Atributo que marca que um tipo deve ter seu método de clonagem gerado.
//////    /// </summary>
//////    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
//////    public sealed class DeepCloneableAttribute : Attribute
//////    {
//////        public DeepCloneableAttribute()
//////        {
//////        }
//////    }

//////    /// <summary>
//////    /// Interface que sinaliza que um tipo é "deep cloneable". 
//////    /// </summary>
//////    public interface IDeepCloneable : IDeepCloneable<object>
//////    {
//////    }

//////    /// <summary>
//////    /// Interface genérica de deep-clone.
//////    /// </summary>
//////    public interface IDeepCloneable<T>
//////    {
//////    }


//////    #region SOURCE_GENERATOR
//////    [Generator]
//////    public class DeepCloneSourceGenerator : ISourceGenerator
//////    {
//////        public void Initialize(GeneratorInitializationContext context)
//////        {
//////            // Debug se necessário
//////            // if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();
//////        }

//////        public void Execute(GeneratorExecutionContext context)
//////        {
//////            var allSyntaxTrees = context.Compilation.SyntaxTrees;
//////            foreach (var syntaxTree in allSyntaxTrees)
//////            {
//////                var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
//////                var root = syntaxTree.GetRoot(context.CancellationToken);

//////                // Pega classes/struct/interfaces
//////                var typeDeclarations = root.DescendantNodes()
//////                                           .OfType<TypeDeclarationSyntax>();

//////                foreach (var decl in typeDeclarations)
//////                {
//////                    // Verifica se esse tipo deve ser processado:
//////                    // 1) Se tiver [DeepCloneable]
//////                    // OU
//////                    // 2) Se implementar IDeepCloneable / IDeepCloneable<T>
//////                    if (!IsDeepCloneCandidate(decl, semanticModel, context.CancellationToken))
//////                        continue;

//////                    // Obtem símbolo do tipo
//////                    var typeSymbol = semanticModel.GetDeclaredSymbol(decl, context.CancellationToken);
//////                    if (typeSymbol == null)
//////                        continue;

//////                    // Se for interface
//////                    if (typeSymbol.TypeKind == TypeKind.Interface)
//////                    {
//////                        // Gera extension method
//////                        var generatedCode = GenerateForInterface(decl, typeSymbol);
//////                        context.AddSource($"{typeSymbol.Name}_DeepCloneInterface.g.cs", generatedCode);
//////                        continue;
//////                    }

//////                    // Caso contrário, é classe ou struct
//////                    bool isPartial = decl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
//////                    bool isSealed = typeSymbol.IsSealed;

//////                    if (isPartial)
//////                    {
//////                        // Injetar método inline
//////                        var generatedCode = GenerateInlineMethod(decl, typeSymbol);
//////                        context.AddSource($"{typeSymbol.Name}_DeepClonePartial.g.cs", generatedCode);
//////                    }
//////                    else
//////                    {
//////                        // Não partial
//////                        if (isSealed)
//////                        {
//////                            // Se sealed, fallback JSON
//////                            var generatedCode = GenerateJsonCloneStaticMethod(decl, typeSymbol);
//////                            context.AddSource($"{typeSymbol.Name}_DeepCloneSealed.g.cs", generatedCode);
//////                        }
//////                        else
//////                        {
//////                            // Não partial e não sealed => gera classe herdada
//////                            var generatedCode = GenerateInheritedCloneClass(decl, typeSymbol);
//////                            context.AddSource($"{typeSymbol.Name}_DeepCloneInherited.g.cs", generatedCode);
//////                        }
//////                    }
//////                }
//////            }
//////        }

//////        /// <summary>
//////        /// Verifica se o tipo declarado tem [DeepCloneable] OU implementa IDeepCloneable.
//////        /// </summary>
//////        private static bool IsDeepCloneCandidate(
//////            TypeDeclarationSyntax typeDecl,
//////            SemanticModel semanticModel,
//////            CancellationToken ct)
//////        {
//////            // 1) Checa atributo
//////            if (HasDeepCloneableAttribute(typeDecl, semanticModel, ct))
//////                return true;

//////            // 2) Checa se implementa IDeepCloneable ou IDeepCloneable<T>
//////            //    Aqui podemos inspecionar as interfaces do símbolo.
//////            var symbol = semanticModel.GetDeclaredSymbol(typeDecl, ct);
//////            if (symbol == null)
//////                return false;

//////            // Percorre interfaces implementadas
//////            foreach (var iface in symbol.AllInterfaces)
//////            {
//////                // "DeepCloneGenerator.IDeepCloneable"
//////                // ou "DeepCloneGenerator.IDeepCloneable<T>"
//////                var name = iface.ToDisplayString();
//////                if (name == "DeepCloneGenerator.IDeepCloneable" ||
//////                    name.StartsWith("DeepCloneGenerator.IDeepCloneable<"))
//////                {
//////                    return true;
//////                }
//////            }

//////            return false;
//////        }

//////        private static bool HasDeepCloneableAttribute(
//////            TypeDeclarationSyntax typeDecl,
//////            SemanticModel semanticModel,
//////            CancellationToken ct)
//////        {
//////            foreach (var list in typeDecl.AttributeLists)
//////            {
//////                foreach (var attr in list.Attributes)
//////                {
//////                    var attrSymbol = semanticModel.GetSymbolInfo(attr, ct).Symbol;
//////                    if (attrSymbol == null)
//////                        continue;

//////                    var containingType = attrSymbol.ContainingType;
//////                    if (containingType == null)
//////                        continue;

//////                    var fullName = containingType.ToDisplayString();
//////                    if (fullName == "DeepCloneGenerator.DeepCloneableAttribute")
//////                    {
//////                        return true;
//////                    }
//////                }
//////            }
//////            return false;
//////        }

//////        #region 1) INTERFACE
//////        private static string GenerateForInterface(TypeDeclarationSyntax decl, INamedTypeSymbol typeSymbol)
//////        {
//////            // Gera extension method 
//////            //
//////            // public static class FooDeepCloneExtensions
//////            // {
//////            //     public static IFoo DeepClone(this IFoo obj)
//////            //     {
//////            //         ...
//////            //     }
//////            // }
//////            var namespaceName = typeSymbol.ContainingNamespace.IsGlobalNamespace
//////                ? null
//////                : typeSymbol.ContainingNamespace.ToDisplayString();

//////            var extensionClassName = $"{SanitizeName(typeSymbol.Name)}DeepCloneExtensions";
//////            var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

//////            var sb = new StringBuilder();

//////            if (!string.IsNullOrEmpty(namespaceName))
//////            {
//////                sb.AppendLine($"namespace {namespaceName}");
//////                sb.AppendLine("{");
//////            }

//////            sb.AppendLine($"    public static class {extensionClassName}");
//////            sb.AppendLine("    {");
//////            sb.AppendLine($"        public static {typeName}? DeepClone(this {typeName}? obj)");
//////            sb.AppendLine("        {");
//////            sb.AppendLine("            if (obj == null) return default;");
//////            sb.AppendLine("            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);");
//////            sb.AppendLine("            var clone = CloneRuntimeHelper.DeepCloneRuntime(obj, visited);");
//////            sb.AppendLine($"            return ({typeName}?)clone;");
//////            sb.AppendLine("        }");
//////            sb.AppendLine("    }");

//////            if (!string.IsNullOrEmpty(namespaceName))
//////            {
//////                sb.AppendLine("}");
//////            }

//////            return sb.ToString();
//////        }
//////        #endregion

//////        #region 2) PARTIAL CLASS/STRUCT
//////        private static string GenerateInlineMethod(TypeDeclarationSyntax decl, INamedTypeSymbol typeSymbol)
//////        {
//////            // injeta no mesmo tipo
//////            //
//////            // public partial class Foo
//////            // {
//////            //     public Foo? DeepClone() { ... }
//////            // }
//////            var namespaceName = typeSymbol.ContainingNamespace.IsGlobalNamespace
//////                ? null
//////                : typeSymbol.ContainingNamespace.ToDisplayString();

//////            var typeKindKeyword = decl.Keyword.ValueText; // "class" / "struct"
//////            var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

//////            var accessibility = AccessibilityToString(typeSymbol.DeclaredAccessibility);

//////            var baseList = decl.BaseList?.ToFullString() ?? "";
//////            var typeParams = decl.TypeParameterList?.ToFullString() ?? "";
//////            var typeConstraints = decl.ConstraintClauses.ToFullString() ?? "";

//////            var sb = new StringBuilder();

//////            if (!string.IsNullOrEmpty(namespaceName))
//////            {
//////                sb.AppendLine($"namespace {namespaceName}");
//////                sb.AppendLine("{");
//////            }

//////            sb.AppendLine($"    {accessibility} partial {typeKindKeyword} {decl.Identifier}{typeParams} {baseList} {typeConstraints}");
//////            sb.AppendLine("    {");
//////            sb.AppendLine($"        public {typeName}? DeepClone()");
//////            sb.AppendLine("        {");
//////            sb.AppendLine("            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);");
//////            sb.AppendLine($"            return ({typeName}?)CloneRuntimeHelper.DeepCloneRuntime(this, visited);");
//////            sb.AppendLine("        }");
//////            sb.AppendLine("    }");

//////            if (!string.IsNullOrEmpty(namespaceName))
//////            {
//////                sb.AppendLine("}");
//////            }

//////            return sb.ToString();
//////        }
//////        #endregion

//////        #region 3) INHERITED CLASS
//////        private static string GenerateInheritedCloneClass(TypeDeclarationSyntax decl, INamedTypeSymbol typeSymbol)
//////        {
//////            // gera class FooClone : Foo
//////            //
//////            // public class FooClone : Foo
//////            // {
//////            //     public static Foo? Clone(Foo? original)
//////            //     {
//////            //         ...
//////            //     }
//////            // }
//////            var namespaceName = typeSymbol.ContainingNamespace.IsGlobalNamespace
//////                ? null
//////                : typeSymbol.ContainingNamespace.ToDisplayString();

//////            var originalTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
//////            var cloneClassName = $"{SanitizeName(typeSymbol.Name)}Clone";

//////            var sb = new StringBuilder();

//////            if (!string.IsNullOrEmpty(namespaceName))
//////            {
//////                sb.AppendLine($"namespace {namespaceName}");
//////                sb.AppendLine("{");
//////            }

//////            sb.AppendLine($"    public class {cloneClassName} : {originalTypeName}");
//////            sb.AppendLine("    {");
//////            sb.AppendLine($"        public static {originalTypeName}? Clone({originalTypeName}? original)");
//////            sb.AppendLine("        {");
//////            sb.AppendLine("            if (original == null) return null;");
//////            sb.AppendLine("            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);");
//////            sb.AppendLine("            return ("
//////                           + originalTypeName
//////                           + "?)CloneRuntimeHelper.DeepCloneRuntime(original, visited);");
//////            sb.AppendLine("        }");
//////            sb.AppendLine("    }");

//////            if (!string.IsNullOrEmpty(namespaceName))
//////            {
//////                sb.AppendLine("}");
//////            }
//////            return sb.ToString();
//////        }
//////        #endregion

//////        #region 4) SEALED => JSON
//////        private static string GenerateJsonCloneStaticMethod(TypeDeclarationSyntax decl, INamedTypeSymbol typeSymbol)
//////        {
//////            // se sealed e nao partial
//////            //
//////            // public static class FooJsonCloneHelper
//////            // {
//////            //     public static Foo? Clone(Foo? original)
//////            //     {
//////            //         ...
//////            //     }
//////            // }
//////            var namespaceName = typeSymbol.ContainingNamespace.IsGlobalNamespace
//////                ? null
//////                : typeSymbol.ContainingNamespace.ToDisplayString();

//////            var originalTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
//////            var helperClassName = $"{SanitizeName(typeSymbol.Name)}JsonCloneHelper";

//////            var sb = new StringBuilder();

//////            if (!string.IsNullOrEmpty(namespaceName))
//////            {
//////                sb.AppendLine($"namespace {namespaceName}");
//////                sb.AppendLine("{");
//////            }

//////            sb.AppendLine($"    public static class {helperClassName}");
//////            sb.AppendLine("    {");
//////            sb.AppendLine($"        public static {originalTypeName}? Clone({originalTypeName}? original)");
//////            sb.AppendLine("        {");
//////            sb.AppendLine("            if (original == null) return null;");
//////            sb.AppendLine("            var options = new JsonSerializerOptions");
//////            sb.AppendLine("            {");
//////            sb.AppendLine("                ReferenceHandler = ReferenceHandler.Preserve,");
//////            sb.AppendLine("                WriteIndented = false");
//////            sb.AppendLine("            };");
//////            sb.AppendLine("            var json = JsonSerializer.Serialize(original, options);");
//////            sb.AppendLine($"            return JsonSerializer.Deserialize<{originalTypeName}>(json, options);");
//////            sb.AppendLine("        }");
//////            sb.AppendLine("    }");

//////            if (!string.IsNullOrEmpty(namespaceName))
//////            {
//////                sb.AppendLine("}");
//////            }
//////            return sb.ToString();
//////        }
//////        #endregion

//////        private static string AccessibilityToString(Accessibility accessibility)
//////        {
//////            return accessibility switch
//////            {
//////                Accessibility.Private => "private",
//////                Accessibility.Protected => "protected",
//////                Accessibility.Internal => "internal",
//////                Accessibility.ProtectedOrInternal => "protected internal",
//////                Accessibility.Public => "public",
//////                _ => "private"
//////            };
//////        }

//////        private static string SanitizeName(string name)
//////        {
//////            // remove caracteres não alfanuméricos
//////            var sb = new StringBuilder();
//////            foreach (var ch in name)
//////            {
//////                if (char.IsLetterOrDigit(ch))
//////                    sb.Append(ch);
//////            }
//////            return sb.ToString();
//////        }
//////    }
//////    #endregion

//////    #region RUNTIME_HELPER
//////    internal static class CloneRuntimeHelper
//////    {
//////        public static object DeepCloneRuntime(object obj, Dictionary<object, object> visited)
//////        {
//////            if (obj is null)
//////                return null!;

//////            // se já foi clonado, retorna
//////            if (visited.TryGetValue(obj, out var existing))
//////            {
//////                return existing;
//////            }

//////            var type = obj.GetType();

//////            // Tipos simples
//////            if (IsSimpleType(type))
//////            {
//////                // não adiciona no visited
//////                return obj;
//////            }

//////            // Stream => copia para MemoryStream
//////            if (obj is Stream s)
//////            {
//////                var ms = new MemoryStream();
//////                var oldPos = s.CanSeek ? s.Position : (long?)null;
//////                s.CopyTo(ms);
//////                if (oldPos.HasValue)
//////                    s.Position = oldPos.Value;
//////                ms.Position = 0;
//////                visited[obj] = ms;
//////                return ms;
//////            }

//////            // Span<T> => chama ToArray() se existir
//////            if (type.IsByRefLike && type.Name.StartsWith("Span"))
//////            {
//////                var toArrayMethod = type.GetMethod("ToArray", Type.EmptyTypes);
//////                if (toArrayMethod != null)
//////                {
//////                    var array = toArrayMethod.Invoke(obj, null);
//////                    visited[obj] = array!;
//////                    return array!;
//////                }
//////                return obj;
//////            }

//////            // ExpandoObject
//////            if (obj is ExpandoObject expando)
//////            {
//////                var cloneExp = new ExpandoObject();
//////                visited[obj] = cloneExp;

//////                var dictExpando = (IDictionary<string, object?>)expando;
//////                var cloneDict = (IDictionary<string, object?>)cloneExp;
//////                foreach (var kvp in dictExpando)
//////                {
//////                    cloneDict[kvp.Key] = DeepCloneRuntime(kvp.Value!, visited);
//////                }
//////                return cloneExp;
//////            }

//////            // Arrays
//////            if (type.IsArray)
//////            {
//////                var arr = (Array)obj;
//////                var rank = arr.Rank;
//////                var lengths = Enumerable.Range(0, rank).Select(d => arr.GetLength(d)).ToArray();
//////                var cloneArr = Array.CreateInstance(type.GetElementType()!, lengths);
//////                visited[obj] = cloneArr;

//////                var indices = new int[rank];
//////                CopyArrayRecursive(arr, cloneArr, 0, indices, visited);
//////                return cloneArr;
//////            }

//////            // IDictionary (não genérico)
//////            if (obj is IDictionary dict)
//////            {
//////                var dictClone = (IDictionary)Activator.CreateInstance(type)!;
//////                visited[obj] = dictClone;

//////                foreach (var key in dict.Keys)
//////                {
//////                    var k2 = DeepCloneRuntime(key!, visited);
//////                    var v2 = DeepCloneRuntime(dict[key]!, visited);
//////                    dictClone[k2] = v2;
//////                }
//////                return dictClone;
//////            }

//////            // IEnumerable<T> genérico
//////            var ienumerableT = GetIEnumerableT(type);
//////            if (ienumerableT != null)
//////            {
//////                object collection;
//////                var ctor = type.GetConstructor(Type.EmptyTypes);
//////                if (ctor != null && !type.IsAbstract)
//////                {
//////                    collection = Activator.CreateInstance(type)!;
//////                }
//////                else
//////                {
//////                    var listType = typeof(List<>).MakeGenericType(ienumerableT);
//////                    collection = Activator.CreateInstance(listType)!;
//////                }
//////                visited[obj] = collection;

//////                var addMethod = collection.GetType().GetMethod("Add", new[] { ienumerableT });
//////                if (addMethod == null)
//////                {
//////                    if (collection is IList listObj)
//////                    {
//////                        foreach (var item in (IEnumerable)obj)
//////                        {
//////                            listObj.Add(DeepCloneRuntime(item!, visited));
//////                        }
//////                        return collection;
//////                    }
//////                    else
//////                    {
//////                        // fallback
//////                        var tmpList = new List<object?>();
//////                        foreach (var item in (IEnumerable)obj)
//////                        {
//////                            tmpList.Add(DeepCloneRuntime(item!, visited));
//////                        }
//////                        return tmpList;
//////                    }
//////                }
//////                else
//////                {
//////                    foreach (var item in (IEnumerable)obj)
//////                    {
//////                        addMethod.Invoke(collection, new[] { DeepCloneRuntime(item!, visited) });
//////                    }
//////                    return collection;
//////                }
//////            }

//////            // Tipo complexo (POCO)
//////            object instance;
//////            // tenta construtor sem parâmetros
//////            var defaultCtor = type.GetConstructor(
//////                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
//////                null,
//////                Type.EmptyTypes,
//////                null);

//////            if (defaultCtor != null && !type.IsAbstract)
//////            {
//////                instance = defaultCtor.Invoke(null);
//////            }
//////            else
//////            {
//////                // fallback => FormatterServices
//////                instance = FormatterServices.GetUninitializedObject(type);
//////            }

//////            visited[obj] = instance;

//////            // Copia campos de toda a hierarquia
//////            var current = type;
//////            while (current != null && current != typeof(object))
//////            {
//////                var fields = current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
//////                foreach (var f in fields)
//////                {
//////                    if (f.IsStatic)
//////                        continue;
//////                    var fieldVal = f.GetValue(obj);
//////                    var clonedVal = DeepCloneRuntime(fieldVal!, visited);
//////                    f.SetValue(instance, clonedVal);
//////                }
//////                current = current.BaseType;
//////            }

//////            return instance;
//////        }

//////        private static void CopyArrayRecursive(Array source, Array dest, int dimension, int[] indices, Dictionary<object, object> visited)
//////        {
//////            if (dimension == source.Rank)
//////            {
//////                var val = source.GetValue(indices);
//////                var cloned = DeepCloneRuntime(val!, visited);
//////                dest.SetValue(cloned, indices);
//////            }
//////            else
//////            {
//////                for (int i = 0; i < source.GetLength(dimension); i++)
//////                {
//////                    indices[dimension] = i;
//////                    CopyArrayRecursive(source, dest, dimension + 1, indices, visited);
//////                }
//////            }
//////        }

//////        private static bool IsSimpleType(Type type)
//////        {
//////            if (type.IsPrimitive)
//////                return true;
//////            if (type.IsEnum)
//////                return true;
//////            if (type == typeof(string))
//////                return true;
//////            if (type == typeof(decimal))
//////                return true;
//////            if (type == typeof(DateTime))
//////                return true;
//////            if (type == typeof(DateTimeOffset))
//////                return true;
//////            if (type == typeof(TimeSpan))
//////                return true;
//////            if (type == typeof(Guid))
//////                return true;
//////            return false;
//////        }

//////        private static Type? GetIEnumerableT(Type type)
//////        {
//////            if (type.IsInterface
//////                && type.IsGenericType
//////                && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
//////            {
//////                return type.GetGenericArguments()[0];
//////            }
//////            foreach (var it in type.GetInterfaces())
//////            {
//////                if (it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IEnumerable<>))
//////                    return it.GetGenericArguments()[0];
//////            }
//////            return null;
//////        }
//////    }

//////    /// <summary>
//////    /// Comparador de referência para uso no visited, preservando referências cíclicas.
//////    /// </summary>
//////    internal sealed class ReferenceEqualityComparer : IEqualityComparer<object>
//////    {
//////        public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

//////        bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

//////        int IEqualityComparer<object>.GetHashCode(object obj)
//////            => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
//////    }
//////    #endregion

//////}
