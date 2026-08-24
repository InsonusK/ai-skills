# This feature is documentary only — no step definitions bind to it.
# The actual proof is RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat,
# in SampleArchitectureTests.cs, in this same folder.

Feature: Rejection codes are unique and follow the Module.Class.Reason format

  Scenario: RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat
    With rejection codes declared next to each rule, the compile-time guarantee
    that they stay unique and well-formed replaces the old eyeball check against
    a central registry.
