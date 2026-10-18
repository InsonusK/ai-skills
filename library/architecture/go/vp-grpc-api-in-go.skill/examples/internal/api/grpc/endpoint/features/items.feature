@type/api/grpc
Feature: ItemsService
  As a client of the service
  I want the methods of ItemsService to call their commands
  So that I plan items over gRPC

  @category/happy
  Scenario: PlanItems becomes the PlanItems command and its result comes back as the response
    Given PlanItems returns the items "buy milk|walk the dog"
    When I call ItemsService.PlanItems with the titles "buy milk|walk the dog"
    Then the response should hold the items "buy milk|walk the dog", none done
    And PlanItems should have received the titles "buy milk|walk the dog"
