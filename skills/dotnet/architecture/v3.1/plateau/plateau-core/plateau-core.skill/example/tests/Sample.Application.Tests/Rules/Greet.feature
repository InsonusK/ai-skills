Feature: Greet feature

  Scenario: Handling a greet command renders and stores the greeting
    Given the greeting message "World"
    When the greet command is handled
    Then the rendered result is "Hello, World!"
    And the stored last greeting is "Hello, World!"

  Scenario: The greeting property validator rejects an empty message
    Given the greeting message ""
    When the greeting property validator runs
    Then validation fails with error code "Sample.Greeting.Required"
