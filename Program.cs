using System.Text.RegularExpressions;

Regex VersionNumberValidate = new(@"^[0-9]+(\.[0-9]+){1,3}$", RegexOptions.Compiled);
Regex ProjectFileVersionReplace = new(@"(?<=<(?:Assembly|File|Package|)Version>)[^<]*", RegexOptions.Compiled);
Regex ProjectFileInformationalVersionReplace = new Regex(@"(?<=<InformationalVersion>)[^<]*", RegexOptions.Compiled);
Regex PackageOutputPathReplace = new(@"<PackageOutputPath>[^<]*</PackageOutputPath>", RegexOptions.Compiled);
Regex AssemblyInfoVersionReplace = new(@"(?<=\[.*Assembly.*Version ?\(\"")([^\""]*)", RegexOptions.Compiled);
Regex AssemblyInformationalVersion = new Regex(@"(?<=\[.*Assembly.*Informational.*Version ?\(\"")([^\""]*)", RegexOptions.Compiled);

if (args.Length < 1)
{
    Console.WriteLine("Usage: ");
    Console.WriteLine("\tdotnet-set-version <version_number> [<alternate_version_string>]");
    Console.WriteLine("");
    return;
}

string? versionNumber = null;
string? alternateVersionString = null;
bool noPackageOutputPath = false;
foreach (string arg in args)
{
    if (string.Equals(arg, "-NoPackageOutputPath") || string.Equals(arg, "/NoPackageOutputPath")) noPackageOutputPath = true;
    // check for a version number
    if (VersionNumberValidate.IsMatch(arg)) versionNumber = arg;
    else if (versionNumber != null && alternateVersionString == null) alternateVersionString = arg;
}
if (versionNumber == null)
{
    Console.WriteLine("The specified version must be of the form nnn.nnn[.nnn[.nnn]]");
    Console.WriteLine("");
    return;
}
foreach (string targetFile in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.AllDirectories))
{
    UpdateProjectFile(targetFile, versionNumber, alternateVersionString, noPackageOutputPath);
}
foreach (string targetFile in Directory.GetFiles(Directory.GetCurrentDirectory(), "AssemblyInfo.cs", SearchOption.AllDirectories))
{
    UpdateCodeFile(targetFile, versionNumber, alternateVersionString);
}

void UpdateProjectFile(string targetFile, string newVersion, string? alternateVersionString, bool noPackageOutputPath)
{
    string targetFileContents = File.ReadAllText(targetFile);
    if (alternateVersionString != null)
    {
        targetFileContents = ProjectFileInformationalVersionReplace.Replace(targetFileContents, alternateVersionString);
    }
    targetFileContents = ProjectFileVersionReplace.Replace(targetFileContents, newVersion);
    if (noPackageOutputPath) targetFileContents = PackageOutputPathReplace.Replace(targetFileContents, "");
    File.WriteAllText(targetFile, targetFileContents);
    string outputAlternateVersionSuffix = alternateVersionString != null ? $" and informational version {alternateVersionString}" : "";
    Console.WriteLine($"Updated versions in {targetFile} to {newVersion}{outputAlternateVersionSuffix}.");
}

void UpdateCodeFile(string targetFile, string newVersion, string? alternateVersionString)
{
    string targetFileContents = File.ReadAllText(targetFile);
    targetFileContents = AssemblyInfoVersionReplace.Replace(targetFileContents, newVersion);
    if (alternateVersionString != null)
    {
        targetFileContents = AssemblyInformationalVersion.Replace(targetFileContents, alternateVersionString);
    }
    File.WriteAllText(targetFile, targetFileContents);
    string outputAlternateVersionSuffix = alternateVersionString != null ? $" and informational version {alternateVersionString}" : "";
    Console.WriteLine($"Updated versions in {targetFile} to {newVersion}{outputAlternateVersionSuffix}.");
}
