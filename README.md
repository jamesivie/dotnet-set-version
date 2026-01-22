# dotnet-set-version

[dotnet-set-version](https://github.com/jamesivie/dotnet-set-version) sets version numbers into project files and AssemblyInfo files.
The first argument is the version number to set and must be of the form nnn.nnn[.nnn[.nnn]], which may or may not be a semver version.
The second optional argument is an alternate version string to set into the AssemblyInformationalVersion attribute.  This alternate version string is generally a Git SHA, but can be any string you want.

## Usage

`dotnet-set-version <version_number> [<alt_version_string>]`

## Global Installation

`dotnet tool install --global dotnet-set-version --version <tool_version>`

## Local Installation

`dotnet new tool-manifest # if you are setting up this repo`
`dotnet tool install --local dotnet-set-version --version <tool_version>`

## Accessing Versions Using Reflection

```csharp
    private readonly DateTime _serverBuildDate = MainProgramAssembly.GetLinkTime();
    private readonly string _serverVersion = MainProgramAssembly.GetName().Version?.ToString() ?? "";
    private readonly string? _serverInformationalVersion = MainProgramAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    /// <summary>
    /// Gets the link time of the specified assembly from the PE header as inserted by the linker.
    /// </summary>
    /// <param name="assembly">The assembly to get the linker timestamp for.</param>
    /// <returns>The <see cref="DateTime"/> when the assembly was linked.</returns>
    public static DateTime GetLinkTime(this Assembly assembly)
    {
        try
        {
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            // get the assembly location
            string filePath = assembly.Location;
            // allocate some space to store the header information
            byte[] b = new byte[2048];
            // open the assembly as a raw file
            using (Stream s = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                // read in the raw PE header bytes
                s.ReadExactly(b);
            }
            // convert the bits from the pe header offset into a time and convert to a standard unix time (seconds since 1970)
            int i = BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(c_PeHeaderOffset));
            int secondsSince1970 = BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(i + c_LinkerTimestampOffset));
            // convert to utc datetime and return
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secondsSince1970);
            return dt;
        }
        catch
        {
            // this assembly may have been created dynamically
            return DateTime.MaxValue;
        }
    }
```

## GitHub Action Sample Usage

```text
      - name: Install dotnet-set-version
        run: dotnet tool install --global dotnet-set-version
      - name: Run dotnet-set-version
        run: dotnet-set-version 0.1.${{ github.run_number }} ${{ github.sha }} -NoPackageOutputPath
```
