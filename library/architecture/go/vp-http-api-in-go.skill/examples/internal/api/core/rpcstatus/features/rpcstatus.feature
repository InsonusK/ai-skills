@type/tech-check
Feature: Error kinds answered by one table
  As a client of any entry point of the service
  I want every error answered with the same code for the same kind
  So that HTTP, gRPC, and every service of the system tell the same outcome the same way

  @category/happy
  Scenario Outline: An error of a kind is answered with its code, its HTTP status, and its message
    When a command fails with the kind "<kind>" and the message "the reason"
    Then the code should be <code>
    And the HTTP status should be <http>
    And the message should be "plan items: <kind>: the reason"

    Examples:
      | kind                | code | http |
      | invalid             | 3    | 400  |
      | not found           | 5    | 404  |
      | conflict            | 6    | 409  |
      | concurrent change   | 10   | 409  |
      | failed precondition | 9    | 400  |
      | unauthenticated     | 16   | 401  |
      | permission denied   | 7    | 403  |
      | resource exhausted  | 8    | 429  |
      | unimplemented       | 12   | 501  |
      | unavailable         | 14   | 503  |

  @category/error
  Scenario: An error with no kind is INTERNAL without its message
    When a command fails with the plain error "dial tcp 10.0.0.5:5432: connection refused"
    Then the code should be 13
    And the HTTP status should be 500
    And the message should be "internal error"

  @category/happy
  Scenario Outline: The codes an entry point sets itself have their canonical HTTP status
    Then the code <code> should have the HTTP status <http>

    Examples:
      | code | http | name              |
      | 0    | 200  | OK                |
      | 1    | 499  | CANCELLED         |
      | 2    | 500  | UNKNOWN           |
      | 4    | 504  | DEADLINE_EXCEEDED |
      | 11   | 400  | OUT_OF_RANGE      |
      | 15   | 500  | DATA_LOSS         |
