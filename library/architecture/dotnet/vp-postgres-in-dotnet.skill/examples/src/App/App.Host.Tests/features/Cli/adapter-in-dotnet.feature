Feature: CLI adapter failures in dotnet
  The input conversion alone defines usage errors; output conversion failures
  are internal errors even when their exception type is FormatException.

  @category/negative
  Scenario Outline: A format exception keeps the meaning of the stage that failed
    When a CLI adapter throws a format exception in "<stage>"
    Then the adapter failure should have kind "<kind>" and message "<message>"
    And the adapter handler should have been called <calls> times
    And the adapter should have logged <crashes> crashes

    Examples:
      | stage   | kind    | message        | calls | crashes |
      | read    | Invalid | adapter broke  | 0     | 0       |
      | handler | none    | adapter broke  | 1     | 0       |
      | encode  | none    | internal error | 1     | 1       |
      | text    | none    | internal error | 1     | 1       |
