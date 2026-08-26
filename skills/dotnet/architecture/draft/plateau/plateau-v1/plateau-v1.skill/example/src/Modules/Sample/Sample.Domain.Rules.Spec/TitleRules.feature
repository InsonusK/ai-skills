Feature: Task title must be present and within length limits

  @format
  Scenario: Empty title is rejected
    Given a title value of ""
    When TitleRules validates it
    Then the result is invalid with error code "Sample.Title.Required"

  @format
  Scenario: Whitespace-only title is rejected
    Given a title value of "   "
    When TitleRules validates it
    Then the result is invalid with error code "Sample.Title.Required"

  @format
  Scenario: Title exceeding 200 characters is rejected
    Given a title value of 201 characters
    When TitleRules validates it
    Then the result is invalid with error code "Sample.Title.MaxLength"

  @format
  Scenario: Title at exactly 200 characters is accepted
    Given a title value of 200 characters
    When TitleRules validates it
    Then the result is valid

  @semantic
  Scenario: Create task command with empty title is rejected
    Given a task title of ""
    When the command validator checks the title
    Then the result is invalid with error code "Sample.Title.Required"

  @semantic
  Scenario: Create task command with too long title is rejected
    Given a task title of 201 characters
    When the command validator checks the title
    Then the result is invalid with error code "Sample.Title.MaxLength"

  @semantic
  Scenario: Create task command with valid title is accepted
    Given a task title of "Review shared-rules example"
    When the command validator checks the title
    Then the result is valid
