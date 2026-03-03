public static partial class Program
{
    private static void IntegrationTestSetup(ICakeContext ctx, BuildData data)
    {
        data.DotNet("new nugetconfig");

        data.DotNet($"nuget add source --name local \"{data.OutputDirectory.FullPath.Replace('/', System.IO.Path.DirectorySeparatorChar)}\"");

        data.DotNet("nuget add source --name cake \"https://pkgs.dev.azure.com/cake-build/Cake/_packaging/cake/nuget/v3/index.json\"");

        var configPath = data.IntegrationTest.NuGetConfig.FullPath;
        var doc = System.Xml.Linq.XDocument.Load(configPath);
        var config = doc.Root!;
        var mapping = new System.Xml.Linq.XElement("packageSourceMapping",
            new System.Xml.Linq.XElement("packageSource",
                new System.Xml.Linq.XAttribute("key", "local"),
                new System.Xml.Linq.XElement("package", new System.Xml.Linq.XAttribute("pattern", "Cake.Sdk")),
                new System.Xml.Linq.XElement("package", new System.Xml.Linq.XAttribute("pattern", "Cake.Generator")),
                new System.Xml.Linq.XElement("package", new System.Xml.Linq.XAttribute("pattern", "Cake.Template"))),
            new System.Xml.Linq.XElement("packageSource",
                new System.Xml.Linq.XAttribute("key", "cake"),
                new System.Xml.Linq.XElement("package", new System.Xml.Linq.XAttribute("pattern", "Cake.Cli")),
                new System.Xml.Linq.XElement("package", new System.Xml.Linq.XAttribute("pattern", "Cake.Common")),
                new System.Xml.Linq.XElement("package", new System.Xml.Linq.XAttribute("pattern", "Cake.Core")),
                new System.Xml.Linq.XElement("package", new System.Xml.Linq.XAttribute("pattern", "Cake.DotNetTool.Module")),
                new System.Xml.Linq.XElement("package", new System.Xml.Linq.XAttribute("pattern", "Cake.NuGet"))),
            new System.Xml.Linq.XElement("packageSource",
                new System.Xml.Linq.XAttribute("key", "nuget"),
                new System.Xml.Linq.XElement("package", new System.Xml.Linq.XAttribute("pattern", "*"))));
        config.Add(mapping);
        doc.Save(configPath);
    }
}