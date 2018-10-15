# DefaultLanguageAnalyzer
The analyzer determines whether the project components are in the default language

Put this to your project .csproj file:
```xml
<PropertyGroup>
  <AdditionalFileItemNames>$(AdditionalFileItemNames);EmbeddedResource;</AdditionalFileItemNames>
</PropertyGroup>
```

```xml
<!-- Specify a valid path to the library -->
<ItemGroup>
  <Analyzer Include="DefaultLanguageAnalyzer.dll" />
</ItemGroup>
```
