# DefaultLanguageAnalyzer
The analyzer determines whether the project components are in the default language

Put this to your project .csproj file:

<PropertyGroup>
  <AdditionalFileItemNames>$(AdditionalFileItemNames);EmbeddedResource;</AdditionalFileItemNames>
</PropertyGroup>
