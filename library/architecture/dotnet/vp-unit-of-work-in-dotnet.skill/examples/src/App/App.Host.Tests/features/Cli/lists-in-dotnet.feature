@type/api/cli
Feature: lists commands in dotnet
  As a caller of the command line
  I want list flags translated into commands and their results into CLI output

  @category/happy
  Scenario: Add maps flags and prints a JSON item
    Given the list command answers "buy-milk"
    When I run the lists command "lists add --list home --title buy-milk"
    Then AddItem should have received list "home" and title "buy-milk"
    And stdout should be the line '{"item":{"title":"buy-milk","done":false}}'
    And running should end without an error

  @category/happy
  Scenario: Show maps the list and prints JSON items
    Given the list command answers "buy-milk"
    When I run the lists command "lists show --list home"
    Then ListItems should have received list "home"
    And stdout should be the line '{"items":[{"title":"buy-milk","done":false}]}'

  @category/happy
  Scenario Outline: A list action offers a text form
    Given the list command answers "buy-milk"
    When I run the lists command "<args> --output text"
    Then stdout should be the line "[ ] buy-milk"

    Examples:
      | args                                         |
      | lists add --list home --title buy-milk        |
      | lists show --list home                       |

  @category/negative
  Scenario Outline: A list action refuses missing flags before calling a handler
    When I run the lists command "<args>"
    Then running should fail with the kind "invalid"
    And neither list handler should have been called
    And stdout should be empty

    Examples:
      | args                               |
      | lists add --list home               |
      | lists add --title buy-milk          |
      | lists show                         |
