using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace CraftingInterpreters.Generators
{
    [Generator]
    public class CraftingInterpretersGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // find anything that matches our files
            var myFiles = context.AdditionalFiles.Where(at => at.Path.EndsWith("SourceGenInput.txt"));
            foreach (var file in myFiles)
            {
                var content = file.GetText(context.CancellationToken);

                var classDefs = ParseClasses(content).GroupBy(x => x.BaseName);
                foreach (var classDef in classDefs)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("#nullable enable");
                    sb.Append("namespace CraftingInterpreters.Lox.").Append(classDef.Key).AppendLine(";");
                    sb.AppendLine();
                    sb.Append("public abstract record ").AppendLine(classDef.Key);
                    sb.AppendLine("{");
                    sb.AppendLine("    public abstract T Accept<T>(IVisitor<T> visitor);");
                    sb.AppendLine("}");
                    sb.AppendLine();
                    foreach (var entry in classDef)
                    {
                        sb.Append("public record ").Append(entry.ClassName).Append("(").Append(entry.Parameters)
                            .Append(") : ").AppendLine(entry.BaseName);
                        sb.AppendLine("{");
                        sb.AppendLine("    public override T Accept<T>(IVisitor<T> visitor) =>");
                        sb.Append("        visitor.Visit").Append(entry.ClassName).Append(entry.BaseName)
                            .AppendLine("(this);");
                        sb.AppendLine("}");
                        sb.AppendLine();
                    }

                    sb
                        .AppendLine("public interface IVisitor<out T>")
                        .AppendLine("{");
                    foreach (var entry in classDef)
                    {
                        sb.Append("    T Visit")
                            .Append(entry.ClassName)
                            .Append(entry.BaseName)
                            .Append("(")
                            .Append(entry.ClassName)
                            .Append(" ")
                            .Append(entry.BaseName.ToLower())
                            .AppendLine(");");
                    }
                    sb.AppendLine("}");
                    sb.AppendLine("#nullable restore");

                    var sourceText = SourceText.From(sb.ToString(), Encoding.UTF8);

                    context.AddSource($"{classDef.Key}.generated.cs", sourceText);
                }
            }
        }

        private static readonly string Space = " ";

        private IEnumerable<ClassDef> ParseClasses(SourceText content)
        {
            var baseName = "";
            foreach (var line in content.Lines)
            {
                var lineAsString = content.ToString(line.Span);

                if (lineAsString.StartsWith(Space))
                {
                    if (string.IsNullOrWhiteSpace(baseName)) continue;

                    var parts = lineAsString.Trim().Split(':').Select(x => x.Trim()).ToArray();
                    if (parts.Length != 2) continue;
                    yield return new ClassDef
                    {
                        BaseName = baseName,
                        ClassName = parts[0],
                        Parameters = parts[1]
                    };
                    continue;
                }

                baseName = lineAsString.Trim();
            }
        }
    }

    public struct ClassDef
    {
        public string BaseName;
        public string ClassName;
        public string Parameters;
    }
}