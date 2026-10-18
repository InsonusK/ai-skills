@type/tech-check
Feature: Server reflection
  As a developer who calls the service with grpcurl or Postman
  I want the methods listed by server reflection when it is switched on
  So that I call the service without its proto file, and production does not publish it

  @category/happy
  Scenario: With reflection enabled the services are listed
    Given reflection is enabled
    When I list the services by reflection
    Then the listed services should include "test.v1.EchoService"
    And the listed services should include "grpc.health.v1.Health"

  @category/boundary
  Scenario: Reflection is off by default
    When I list the services by reflection
    Then the reflection call should fail with Unimplemented
