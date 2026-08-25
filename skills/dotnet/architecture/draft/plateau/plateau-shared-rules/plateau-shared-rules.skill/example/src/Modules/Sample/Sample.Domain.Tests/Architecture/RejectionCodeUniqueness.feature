Feature: Rejection code uniqueness and format

  Every rejection code declared by a centralized rule uses the {ModuleName}.{Class}.{Reason}
  format, is unique across the Sample.Domain.Rules assembly, and is discoverable as a
  public static string field ending with "Code".

  Scenario: RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat
    Given the compiled Sample.Domain.Rules assembly is loaded
    When every public static string field ending with Code is collected
    Then every value matches Module.Class.Reason
    And no value is duplicated
