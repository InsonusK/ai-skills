Feature: Task schedule window must not be inverted

  @format
  Scenario: Start date after due date is rejected
    Given a schedule from "2026-08-01T00:00:00Z" to "2026-07-01T00:00:00Z"
    When ScheduleRules validates it
    Then the result is invalid with error code "Sample.Schedule.WindowInverted"

  @format
  Scenario: Start date equal to due date is accepted
    Given a schedule from "2026-08-01T00:00:00Z" to "2026-08-01T00:00:00Z"
    When ScheduleRules validates it
    Then the result is valid

  @format
  Scenario: Missing due date is accepted
    Given a schedule from "2026-08-01T00:00:00Z" to ""
    When ScheduleRules validates it
    Then the result is valid

  @semantic
  Scenario: Create task command with inverted dates is rejected
    Given a command with start date "2026-08-15T00:00:00Z" and due date "2026-08-10T00:00:00Z"
    When ScheduleRules validates it
    Then the result is invalid with error code "Sample.Schedule.WindowInverted"

  @semantic
  Scenario: Create task command with valid dates is accepted
    Given a command with start date "2026-08-10T00:00:00Z" and due date "2026-08-15T00:00:00Z"
    When ScheduleRules validates it
    Then the result is valid
