@type/tech-check
Feature: gRPC settings
  As the operator who deploys the service
  I want the gRPC entry point's settings read from its own environment variables
  So that the port and the reflection switch change without a rebuild

  @category/boundary
  Scenario: Nothing set means port 9090 and no reflection
    When the gRPC settings are loaded
    Then the gRPC listen port should be 9090
    And gRPC reflection should be disabled

  @category/happy
  Scenario: Port and reflection switch are read
    Given the environment variable "GRPC_LISTEN_PORT" is "9191"
    And the environment variable "GRPC_REFLECTION_ENABLED" is "true"
    When the gRPC settings are loaded
    Then the gRPC listen port should be 9191
    And gRPC reflection should be enabled

  @category/negative
  Scenario Outline: An unusable value is rejected, naming its variable
    Given the environment variable "<name>" is "<value>"
    When the gRPC settings are loaded
    Then loading the gRPC settings should fail mentioning "<name>"

    Examples:
      | name                    | value |
      | GRPC_LISTEN_PORT        | grpc  |
      | GRPC_LISTEN_PORT        | 70000 |
      | GRPC_LISTEN_PORT        | 0     |
      | GRPC_REFLECTION_ENABLED | maybe |
