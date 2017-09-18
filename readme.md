## RefSwitch
RefSwitch is a tool for switching between package & project references within a project and/or solution.

The csproj-file must first be modified in order to use RefSwitch.  
```
<ItemGroup>
    <!-- PackageReferences BEGIN -->
    <!-- <PackageReference Include="MyCustomPackage" Version="1.2.3" /> -->
    <!-- PackageReferences END -->
</ItemGroup>

<ItemGroup>
    <!-- ProjectReferences BEGIN -->
    <ProjectReference Include="..\MyCustomPackage\MyCustomPackage.csproj" />
    <!-- ProjectReferences END -->
</ItemGroup>
```

The syntax for switching reference types is
```
RefSwitch.exe <type> [target]
    type    'package' or 'project'
    target  specify a file or folder
```

Example:
```
RefSwitch.exe package c:\dev\sample
```