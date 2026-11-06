@type/tech-check
Feature: Aggregates in memory
  As a handler over the repository's contract
  I want roots kept, picked by a specification, and changed in place, with no database
  So that the handlers run in this example and in their scenarios as they will over a database

  Background:
    Given the notes "b|a|c" are added

  @category/happy
  Scenario: A specification picks a root
    Then the note picked by the title "a" should be "a"

  @category/happy
  Scenario: A root changed by a handler is kept as changed
    When I pick the note "a", rename it "a2", and update it
    Then the note picked by the title "a2" should be "a2"

  @category/happy
  Scenario: A specification's order and page shape a list
    Then the notes ordered by title, page 2 of size 2, should be "c"

  @category/happy
  Scenario: A delete by a specification removes what it picks
    When I delete the notes titled "a"
    Then 2 notes should be left

  @category/negative
  Scenario Outline: A count, an existence check, or a delete refuses a page
    When I <call> through a specification with a page
    Then the call should be refused

    Examples:
      | call   |
      | count  |
      | ask    |
      | delete |
