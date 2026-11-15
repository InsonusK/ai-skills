@type/api/http
Feature: Proto-JSON form of a proto contract
  As a client of the HTTP form of a gRPC contract
  I want requests and responses in the standard proto-JSON mapping
  So that the HTTP form reads and writes exactly what a grpc-gateway would

  Background:
    Given the HTTP routes read and write proto messages

  @category/happy
  Scenario Outline: A field is accepted by its lowerCamelCase or its proto name
    Given the echo command answers "hello"
    When I POST "/v1/proto" to the HTTP API with the body '<body>'
    Then the HTTP status should be 200
    And the echo command should have received "a/b"

    Examples:
      | body                       |
      | {"requestTypeUrl":"a/b"}   |
      | {"request_type_url":"a/b"} |

  @category/happy
  Scenario: An unknown field is ignored
    Given the echo command answers "hello"
    When I POST "/v1/proto" to the HTTP API with the body '{"requestTypeUrl":"a/b","priority":1}'
    Then the HTTP status should be 200
    And the echo command should have received "a/b"

  @category/happy
  Scenario: The response is proto-JSON: lowerCamelCase keys, a field at its default left out, text as it is
    Given the echo command answers "купить молоко"
    When I POST "/v1/proto" to the HTTP API with the body '{"requestTypeUrl":"a/b"}'
    Then the HTTP response body should be '{"responseTypeUrl":"купить молоко"}'

  @category/negative
  Scenario Outline: A body that is not the contract's message is 400 and never reaches the command
    When I POST "/v1/proto" to the HTTP API with the body '<body>'
    Then the HTTP status should be 400
    And the HTTP error should have code 3 and a message containing "<message>"
    And the echo command should not have been called

    Examples:
      | body                 | message                     |
      | not json             | not valid JSON              |
      | {"requestTypeUrl":7} | does not match the contract |

  @category/negative
  Scenario: A body not declared as JSON is 400 and never reaches the command
    When I POST "/v1/proto" to the HTTP API with the body '{"requestTypeUrl":"a/b"}' as "text/plain"
    Then the HTTP status should be 400
    And the HTTP error should have code 3 and a message containing "Content-Type must be application/json"
    And the echo command should not have been called

  @category/negative
  Scenario: A method deprecated and switched off answers 501 UNIMPLEMENTED
    When I POST "/v1/old" to the HTTP API with the body '{}'
    Then the HTTP status should be 501
    And the HTTP error should have code 12 and the message "Old is deprecated since 1.1.0"
