@type/api/grpc
Feature: gRPC contract
  As a client who reads the published proto contract
  I want every service of the contract to be served
  So that no call of a published method answers UNIMPLEMENTED

  @category/happy
  Scenario: The gRPC API serves exactly the services its contract describes
    Then the gRPC API and its proto contract both have exactly the services:
      | service              |
      | todo.v1.ItemsService |
