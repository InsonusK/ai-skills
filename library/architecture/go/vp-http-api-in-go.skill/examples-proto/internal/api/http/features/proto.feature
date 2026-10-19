@type/api/http
Feature: Proto-JSON form of a proto contract
  As a client of the HTTP form of a gRPC contract
  I want requests and responses in the standard proto-JSON mapping
  So that the HTTP form reads and writes exactly what a grpc-gateway would

  @category/happy
  Scenario Outline: A field is accepted by its lowerCamelCase or its proto name
    Given the command answers "hello"
    When I POST "/v1/proto" with the body '<body>'
    Then the response status should be 200
    And the command should have received "a/b"

    Examples:
      | body                         |
      | {"requestTypeUrl":"a/b"}     |
      | {"request_type_url":"a/b"}   |

  @category/happy
  Scenario: An unknown field is ignored
    Given the command answers "hello"
    When I POST "/v1/proto" with the body '{"requestTypeUrl":"a/b","priority":1}'
    Then the response status should be 200
    And the command should have received "a/b"

  @category/happy
  Scenario: The response is proto-JSON: lowerCamelCase keys, a field at its default left out
    Given the command answers "hello"
    When I POST "/v1/proto" with the body '{"requestTypeUrl":"a/b"}'
    Then the response body should be '{"responseTypeUrl":"hello"}'

  @category/negative
  Scenario Outline: A body that is not the contract's message is 400 and never reaches the command
    When I POST "/v1/proto" with the body '<body>'
    Then the response status should be 400
    And the error body should have code 3 and a message containing "<message>"
    And the command should not have been called

    Examples:
      | body                         | message                            |
      | not json                     | not valid JSON                     |
      | {"requestTypeUrl":7}         | does not match the contract        |

  @category/negative
  Scenario: A method deprecated and switched off answers 501 UNIMPLEMENTED
    When I POST "/v1/old" with the body '{}'
    Then the response status should be 501
    And the error body should have code 12 and the message "Old is deprecated since 1.1.0"
