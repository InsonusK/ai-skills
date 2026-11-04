@type/tech-check
Feature: The forms of DATABASE_DSN in Python
  As the operator of the service
  I want a DSN in the URL form asyncpg reads, and a clear refusal of another form
  So that a DSN copied from Go's key=value form fails at start, not at the first request

  @category/happy
  Scenario Outline: A URL is read as it is
    Given the environment variable "DATABASE_DSN" is "<dsn>"
    When the settings are loaded
    Then the database should be "<database>" on "<host>"

    Examples:
      | dsn                                       | database | host |
      | postgres://todo:secret@db:5432/todo       | todo     | db   |
      | postgresql://todo:secret@db/todo?ssl=true | todo     | db   |

  @category/negative
  Scenario: The key=value form is refused, asking for a URL
    Given the environment variable "DATABASE_DSN" is "host=db user=todo password=secret dbname=todo"
    When the settings are loaded
    Then loading should fail mentioning "a URL"
    And the failure should not mention "secret"

  @category/negative
  Scenario: A URL of another database is refused
    Given the environment variable "DATABASE_DSN" is "mysql://todo:secret@db/todo"
    When the settings are loaded
    Then loading should fail mentioning "a URL postgresql://"
    And the failure should not mention "secret"
