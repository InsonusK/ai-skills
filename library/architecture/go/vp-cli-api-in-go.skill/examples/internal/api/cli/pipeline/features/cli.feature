@type/api/cli
Feature: Command line
  As a person or a script that runs the application
  I want `{app} {resource} {action} [flags]` to run one command and say how it went
  So that I read the result on stdout, the error on stderr, and the outcome in the exit code

  @category/happy
  Scenario: An action runs its command and prints the result as one JSON line
    Given the command answers "hello"
    When I run "test echo --text hi"
    Then stdout should be the line '{"text":"hello"}'
    And the command should have received "hi"
    And running should end without an error

  @category/happy
  Scenario: --output text prints the action's text form
    Given the command answers "hello"
    When I run "test echo --text hi --output text"
    Then stdout should be the line 'echo: hello'

  @category/boundary
  Scenario: --output text of an action with no text form prints indented JSON
    Given the command answers "hello"
    When I run "test plain --text hi --output text"
    Then stdout should be indented JSON with "text" = "hello"

  @category/negative
  Scenario: A command rejected by its handler keeps the handler's kind
    Given the command is rejected with the kind "conflict" and "the name is taken"
    When I run "test echo --text hi"
    Then running should fail with the kind "conflict"
    And stderr should mention "the name is taken"
    And stdout should be empty

  @category/error
  Scenario: A failure of the handler keeps no kind
    Given the command fails with "connection refused"
    When I run "test echo --text hi"
    Then running should fail with no kind
    And stderr should mention "connection refused"

  @category/negative
  Scenario Outline: An error of the command line is invalid and never reaches the command
    When I run "<args>"
    Then running should fail with the kind "invalid"
    And stderr should mention "<message>"
    And the command should not have been called

    Examples:
      | args                           | message                        |
      | bogus                          | unknown command               |
      | test bogus                     | see todo test --help          |
      | test echo --nope               | unknown flag: --nope           |
      | test echo --text hi --output x | --output must be json or text  |
      | test echo                      | --text is required             |

  @category/happy
  Scenario Outline: Help lists what can be run, without running anything
    When I run "<args>"
    Then running should end without an error
    And stdout should mention "<shows>"
    And the command should not have been called

    Examples:
      | args             | shows                      |
      | --help           | test                       |
      | test             | echo                       |
      | test echo --help | --text                     |
      |                  | Available Commands         |

  @category/happy
  Scenario: --version prints the version of the binary
    When I run "--version"
    Then stdout should mention "todo version 1.2.3"

  @category/happy
  Scenario: A run is logged when its arguments are parsed, with the flags, and when it ends, with the output
    Given the command answers "hello"
    When I run "test echo --text hi --output text"
    Then the records should be, in order, "cli call received|cli call answered"
    And the "cli call received" record should hold the flag "text" = "hi"
    And one "cli call answered" record should be logged for the action "todo test echo", failed "false"
    And the "cli call answered" record should hold the output "echo: hello"

  @category/negative
  Scenario: A run that fails is logged as failed with the flags given, even when they could not be parsed
    When I run "test echo --nope"
    Then one "cli call answered" record should be logged for the action "todo test echo", failed "true"
    And the records should be, in order, "cli call answered"
