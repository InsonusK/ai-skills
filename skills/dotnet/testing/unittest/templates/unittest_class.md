# Template of unit test class
Use this [template](#tempalte) to create unit test

## Tempalte
```csharp
using Microsoft.Extensions.Logging;
using Divergic.Logging.Xunit;
using Xunit.Abstractions;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class {TestedClassName}_Test : LoggingTestsBase<{TestedClassName}_Test>
{

    public {TestedClassName}_Test(ITestOutputHelper output, LogLevel logLevel = LogLevel.Debug) : base(output, logLevel)
    {

    }
    /// <summary>
    /// description: ...
    /// input: ...
    /// output: ...
    /// expected_result: ...
    /// </summary>
    /// <returns></returns>
    [Fact]
    public void test_{group}_WHEN_{condition}__THEN_{result}()
    {
        #region Array
        Logger.LogDebug("Test ARRAY");

        {define used variables}

        #endregion


        #region Act
        Logger.LogDebug("Test ACT");



        #endregion


        #region Assert
        Logger.LogDebug("Test ASSERT");

        #endregion
    }
}
```

## Method naming
- `{group}` - logical grouping of tests
    - examples: 
        - `test_Name_WHEN_is_empty_THEN_error`, `test_Name_WHEN_is_valid_THEN_success`
        - `test_Select_WHEN_id_exist_THEN_return_entity`, `test_Select_WHEN_id_not_exist_THEN_return_null`
- `{condition}` - the input state or scenario
- `{result}` - the expected outcome

## Fields naming
- `expected_{field_name}` - variable which contain expected values
- `asserted_{field_name}` - variable which must be checked and be equal expected_filed

## Storage
Unit test must store in separate csproj but in same folder as tested class
Example:
- src/Project/Project.csproj
    - services
        - Service.cs
- src/Project/Project.Test.csproj
    - services
        - Service_Test.cs

## Namespace
Unit test has same namespace as tested class but with "Test" prefix
Example:
- `{CS_Project}.{Folder}.{TestedClassName}`
- `{CS_Project}.Test.{Folder}.{TestedClassName}`

## Class naming
Unit test class has same name as tested class but with "_Test" suffix
Example:
- `{TestedClassName}_Test`

If there are too mush test cases for one class, create separate class for each group of test cases
`{TestedClassName}_{Group}_Test`

Example:
- `{TestedClassName}_Handle_Test`
- `{TestedClassName}_Construct_Test`

Group like you see in [Method naming](#method-naming)
