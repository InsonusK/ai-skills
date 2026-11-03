@type/api/grpc
Feature: ItemsService
  As a client of the service
  I want the methods of ItemsService to call their commands
  So that I plan, add, and list items over gRPC

  @category/happy
  Scenario: PlanItems becomes the PlanItems command and its result comes back as the response
    Given PlanItems returns the items "buy milk|walk the dog" over gRPC
    When I call ItemsService.PlanItems with the titles "buy milk|walk the dog"
    Then the gRPC response should hold the items "buy milk|walk the dog", none done
    And PlanItems should have received the titles "buy milk|walk the dog" over gRPC

  @category/happy
  Scenario: AddItem becomes the AddItem command with the list and the title
    Given AddItem returns the item "buy milk" over gRPC
    When I call ItemsService.AddItem with the list "home" and the title "buy milk"
    Then the gRPC response should hold the item "buy milk"
    And AddItem should have received the list "home" and the title "buy milk" over gRPC

  @category/negative
  Scenario: A title already in the list is ALREADY_EXISTS
    Given AddItem fails with the kind "conflict" over gRPC
    When I call ItemsService.AddItem with the list "home" and the title "buy milk"
    Then the gRPC call of ItemsService should fail with AlreadyExists

  @category/happy
  Scenario: ListItems becomes the ListItems command with the list
    Given ListItems returns the items "buy milk|walk the dog" over gRPC
    When I call ItemsService.ListItems with the list "home"
    Then the gRPC response should hold the items "buy milk|walk the dog", none done
    And ListItems should have received the list "home" over gRPC

  @category/negative
  Scenario: The deprecated ClearList answers UNIMPLEMENTED with the same message as its HTTP form
    When I call ItemsService.ClearList with the list "home"
    Then the gRPC call of ItemsService should fail with Unimplemented and the message "ClearList is deprecated since 1.1.0"
