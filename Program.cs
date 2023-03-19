using System.Text.RegularExpressions;

Regex VersionNumberValidate = new(@"^[0-9]+(\.[0-9]+){1,3}$", RegexOptions.Compiled);
Regex ProjectFileVersionReplace = new(@"(?<=<(?:Assembly|File|Package|)Version>)[^<]*", RegexOptions.Compiled);
Regex PackageOutputPathReplace = new(@"<PackageOutputPath>[^<]*</PackageOutputPath>", RegexOptions.Compiled);
Regex AssemblyInfoVersionReplace = new(@"(?<=\[.*Assembly.*Version ?\(\"")([^\""]*)", RegexOptions.Compiled);

if (args.Length < 1)
{
    Console.WriteLine("Usage: ");
    Console.WriteLine("\tdotnet-set-version <version_number>");
    Console.WriteLine("");
    return;
}

string? version = null;
bool noPackageOutputPath = false;
foreach (string arg in args)
{
    if (String.Equals(arg, "-NoPackageOutputPath") || String.Equals(arg, "/NoPackageOutputPath")) noPackageOutputPath = true;
    // check for a version number
    if (VersionNumberValidate.IsMatch(arg)) version = arg;
}
if (version == null)
{
    Console.WriteLine("The specified version must be of the form nnn.nnn[.nnn[.nnn]]");
    Console.WriteLine("");
    return;
}
foreach (string targetFile in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.AllDirectories))
{
    UpdateProjectFile(targetFile, args[0], noPackageOutputPath);
}
foreach (string targetFile in Directory.GetFiles(Directory.GetCurrentDirectory(), "AssemblyInfo.cs", SearchOption.AllDirectories))
{
    UpdateCodeFile(targetFile, args[0]);
}

void UpdateProjectFile(string targetFile, string newVersion, bool noPackageOutputPath)
{
    string targetFileContents = File.ReadAllText(targetFile);
    targetFileContents = ProjectFileVersionReplace.Replace(targetFileContents, newVersion);
    if (noPackageOutputPath) targetFileContents = PackageOutputPathReplace.Replace(targetFileContents, "");
    File.WriteAllText(targetFile, targetFileContents);
    Console.WriteLine($"Updated versions in {targetFile} to {newVersion}.");
}

void UpdateCodeFile(string targetFile, string newVersion)
{
    string targetFileContents = File.ReadAllText(targetFile);
    targetFileContents = AssemblyInfoVersionReplace.Replace(targetFileContents, newVersion);
    File.WriteAllText(targetFile, targetFileContents);
    Console.WriteLine($"Updated versions in {targetFile} to {newVersion}.");
}
