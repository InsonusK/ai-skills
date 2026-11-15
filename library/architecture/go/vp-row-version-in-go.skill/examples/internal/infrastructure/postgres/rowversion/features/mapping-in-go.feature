@type/tech-check
Feature: Row version mapping in Go
  The row classification checks the mapped, read-only, non-migrated xmin.

  @category/happy
  Scenario: A mapped read-only xmin is recognised
    Given a sample version declaration "valid"
    Then that xmin mapping should be recognised "true"

  @category/negative
  Scenario Outline: A field type alone does not establish the mapping
    Given a sample version declaration "<declaration>"
    Then that xmin mapping should be recognised "<recognised>"

    Examples:
      | declaration | recognised |
      | ignored     | false      |
      | wrong column | false     |
      | writable    | false      |
      | migrated    | false      |
      | wrong type  | false      |
