@type/tech-check
Feature: Row version mapping in Python
  Classification recognises the system column and engine-generated values.

  @category/happy
  Scenario: A system xmin with engine defaults is recognised
    Given a sample version declaration "valid"
    Then that xmin mapping should be recognised "true"

  @category/negative
  Scenario Outline: An incomplete handwritten declaration is refused
    Given a sample version declaration "<declaration>"
    Then that xmin mapping should be recognised "false"

    Examples:
      | declaration       |
      | wrong column      |
      | wrong type        |
      | migrated          |
      | missing default   |
      | missing onupdate  |
