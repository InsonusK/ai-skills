Feature: Greet command

  Scenario: Valid greet command returns a greeting
    Given the name "World"
    When the greet command is handled
    Then the result value is "Hello, World!"
