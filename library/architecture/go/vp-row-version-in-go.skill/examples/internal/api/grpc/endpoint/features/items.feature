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

  @category/happy
  Scenario: AddItem becomes the AddItem command with the list and the title
    When I call ItemsService.AddItem with the list "home" and the title "buy milk"
    Then the added item should be "buy milk"
    And AddItem should have received the list "home" and the title "buy milk"

  @category/negative
  Scenario: A title already in the list is ALREADY_EXISTS
    Given AddItem fails with the kind conflict
    When I call ItemsService.AddItem with the list "home" and the title "buy milk"
    Then the call should fail with AlreadyExists and the message "the title is taken"

  @category/happy
  Scenario: ListItems becomes the ListItems command and its items come back
    Given ListItems holds "buy milk|walk the dog"
    When I call ItemsService.ListItems with the list "home"
    Then the listed items should be "buy milk|walk the dog" for the list "home"

  @category/happy
  Scenario: CompleteItem becomes the CompleteItem command with the version, and answers the new one
    When I call ItemsService.CompleteItem with the list "home", the item 1, and the version "7"
    Then the completed item should be done, with the id 1 and the version "7+1"
    And CompleteItem should have received the list "home", the item 1, and the version "7"

  @category/negative
  Scenario: An item changed since the caller read it is ABORTED
    Given CompleteItem fails with the kind concurrent change
    When I call ItemsService.CompleteItem with the list "home", the item 1, and the version "7"
    Then the call should fail with Aborted and the message "the item was changed since it was read: read it again"

  @category/negative
  Scenario: The deprecated ClearList answers UNIMPLEMENTED
    When I call ItemsService.ClearList with the list "home"
    Then the call should fail with Unimplemented and the message "ClearList is deprecated since 1.1.0"
