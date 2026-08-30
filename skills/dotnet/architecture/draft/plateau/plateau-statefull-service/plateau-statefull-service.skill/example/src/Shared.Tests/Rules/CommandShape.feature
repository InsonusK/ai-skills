Feature: Command shape

  Scenario: A command implements the shared command contract
    Given a command name "World"
    When the command is created
    Then it implements ICommand of Result of string
