using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Nivaes.App.Rpc.SourceGenerator;

namespace Nivaes.App.Rpc.Tests
{
    public class RegisterRpcDataModelsActionsGeneratoTests
    {
        [Fact]
        public void RegisterRpcDataModelsActionsGenerator1()
        {
            var source = """
            using Nivaes.App.Rpc;

            namespace Tests

            public partial class TestRpcDataModel : Nivaes.App.Rpc.IRpcDataModel
            {
                    Guid Id { get; }

                    long TimeStampTicks { get; set; }
            }
            """;

            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var compilation = CSharpCompilation.Create(
                "TestAssembly",
                new[] { syntaxTree },
                GetReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var type = compilation.GetTypeByMetadataName("Nivaes.App.Rpc.IRpcDataModel");
            type.ShouldNotBeNull();

            IIncrementalGenerator generator = new RegisterRpcDataModelsActionsGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            driver = driver.RunGenerators(compilation);

            var result = driver.GetRunResult();

            result.Diagnostics.ShouldBeEmpty();

            result.GeneratedTrees.ShouldHaveSingleItem();

            var generated = result.GeneratedTrees[0]
                .GetText()
                .ToString();

            generated.ShouldContain("RegisterRpcDataModelsActions");
            generated.ShouldContain("GeneratedRegisterRpcDataModelsExtensions");
            generated.ShouldContain("RpcDataModelTypeContainerHelper.New");
        }

        [Fact]
        public void RegisterRpcDataModelsActionsGenerator2()
        {
            var source = """
            using Nivaes.App.Rpc;

            namespace Tests

            public abstract class BaseRpcDataModel : IRpcDataModel
            {
                Guid Id { get; }
            
                long TimeStampTicks { get; set; }
            }

            public class TestRpcDataModel : BaseRpcDataModel
            {
            }
            """;

            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var compilation = CSharpCompilation.Create(
                "TestAssembly",
                new[] { syntaxTree },
                GetReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var type = compilation.GetTypeByMetadataName("Nivaes.App.Rpc.IRpcDataModel");
            type.ShouldNotBeNull();

            IIncrementalGenerator generator = new RegisterRpcDataModelsActionsGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            driver = driver.RunGenerators(compilation);

            var result = driver.GetRunResult();

            result.Diagnostics.ShouldBeEmpty();

            result.GeneratedTrees.ShouldHaveSingleItem();

            var generated = result.GeneratedTrees[0]
                .GetText()
                .ToString();

            generated.ShouldContain("RegisterRpcDataModelsActions");
            generated.ShouldContain("GeneratedRegisterRpcDataModelsExtensions");
            generated.ShouldContain("RpcDataModelTypeContainerHelper.New");
        }

        private static IEnumerable<MetadataReference> GetReferences()
        {
            yield return MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location);

            yield return MetadataReference.CreateFromFile(
               typeof(Guid).Assembly.Location);

            yield return MetadataReference.CreateFromFile(
                typeof(IRpcDataModel).Assembly.Location);

            //yield return MetadataReference.CreateFromFile(
            //    typeof(RpcServiceAttribute).Assembly.Location);
        }
    }
}
