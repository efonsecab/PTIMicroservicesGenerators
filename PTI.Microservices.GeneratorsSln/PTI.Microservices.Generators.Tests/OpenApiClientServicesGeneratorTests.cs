using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;

namespace PTI.Microservices.Generators.Tests
{
    /// <summary>
    /// Check
    /// https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.cookbook.md
    /// </summary>
    [TestClass]
    public class OpenApiClientServicesGeneratorTests
    {
        [TestMethod]
        public void Test_OpenApiClientServicesGenerator()
        {
            var appDirectory = AppContext.BaseDirectory;
            string additionaTextFilePath = Path.Combine(appDirectory, "AdditionalFiles/OpenApiImports.txt");
            // directly create an instance of the generator
            // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
            OpenApiClientServicesGenerator generator = new OpenApiClientServicesGenerator();
            GeneratorAdditionalText generatorAdditionalText =
                new GeneratorAdditionalText(additionaTextFilePath);
            // Create the driver that will control the generation, passing in our generator
            GeneratorAdditionalText[] additionalTexts = new[] { generatorAdditionalText };
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generators:
                new[] { generator }, additionalTexts:
                additionalTexts);
            var inputCompilation = CreateCompilation(null);
            driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);
            // We can now assert things about the resulting compilation:
            Assert.IsTrue(diagnostics.IsEmpty); // there were no diagnostics created by the generators
            var allLines = additionalTexts.SelectMany(p => p.GetText().Lines);
            Assert.AreEqual(outputCompilation.SyntaxTrees.Count(),allLines.Count()); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
            var outputdiagnostics = outputCompilation.GetDiagnostics();
            //Assert.IsTrue(outputdiagnostics.IsEmpty); // verify the compilation with the added source has no diagnostics
        }

        [TestMethod]
        public void Test_AccessGeneratedClasses()
        {
        }

        private static Compilation CreateCompilation(string source)
        {
            //        var syntaxTrees = new[]
            //        {
            //    CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(){
            //    })
            //};
            SyntaxTree?[] syntaxTrees = null;
            var references = new List<PortableExecutableReference>();
            var dotNetCoreDir = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
            //var compiledTasks = DependencyContext.Default.CompileLibraries.Where(p => p.Name.IndexOf("NETStandard") > -1);
            //var netSatandardsLibrary = DependencyContext.Default.RuntimeLibraries.Where(p => p.Name.IndexOf("NETStandard") > -1).Single();
            foreach (var library in DependencyContext.Default.RuntimeLibraries.Where(lib => lib.Name.IndexOf("PTI.Microservices.Library.Generators") > -1))
            {
                var assembly = Assembly.Load(new AssemblyName(library.Name));
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
            references.Add(MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(HttpClient).GetTypeInfo().Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(HttpClientJsonExtensions).GetTypeInfo().Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Uri).GetTypeInfo().Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.Runtime.dll")));
            //references.Add(MetadataReference.CreateFromFile(netSatandardsLibrary.Path));
            //references.Add(MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task<>).GetTypeInfo().Assembly.Location));

            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var compilation = CSharpCompilation.Create(nameof(OpenApiClientServicesGenerator), syntaxTrees, references, options);

            return compilation;
        }
    }
}