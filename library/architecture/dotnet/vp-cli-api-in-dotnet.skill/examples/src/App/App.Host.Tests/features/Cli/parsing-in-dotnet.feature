@type/api/cli
Feature: Command line parsing in dotnet
  The library parser preserves the CLI contract, including repeated flags,
  equal-sign values and rejection before any command is called.

  @category/happy
  Scenario Outline: String values are passed unchanged
    Given the command answers "hello"
    When I run "test echo <flags>"
    Then the command should have received "<value>"
    And stdout should be the line '{"text":"hello"}'
    And running should end without an error

    Examples:
      | flags              | value     |
      | --text=a=b         | a=b       |
      | --text=--literal   | --literal |
      | --text -1          | -1        |
      | --text=            |           |

  @category/boundary
  Scenario: A repeated scalar flag keeps its last value and logs all values
    Given the command answers "hello"
    When I run "test echo --text first --text second"
    Then the command should have received "second"
    And the "cli call received" record should hold the flag "text" = "first,second"
    And running should end without an error

  @category/negative
  Scenario Outline: Invalid syntax never calls the handler
    When I run "<args>"
    Then running should fail with the kind "invalid"
    And the command should not have been called
    And stdout should be empty
    And the records should be, in order, "cli call answered"

    Examples:
      | args                                  |
      | test echo --text                       |
      | test echo --text --nope                |
      | test echo --text hi extra              |
      | test echo --text hi --output           |
      | test echo --text hi --output=bogus      |
      | test echo --text hi -- --nope          |
      | --bogus                               |
      | --help=bogus                          |
      | --version=bogus                       |
      | test echo --text hi --help=bogus       |
      | @missing-response-file                |
      | test echo --text=hi --text             |
      | test echo --text --text=hi             |
      | test echo --text hi --output=json --output |
      | test echo --text hi --output --output=json |
