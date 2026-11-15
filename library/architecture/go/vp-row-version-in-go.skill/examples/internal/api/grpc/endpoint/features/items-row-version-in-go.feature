@type/api/grpc
Feature: Row version of ItemsService
  As a client of the service
  I want the methods of ItemsService to call their commands
  So that I plan items over gRPC


  @category/happy
  Scenario: CompleteItem becomes the CompleteItem command with the version, and answers the new one
    When I call ItemsService.CompleteItem with the list "home", the item 1, and the version "7"
    Then the completed item should be done, with the id 1 and the version "7+1"
    And CompleteItem should have received the list "home", the item 1, and the version "7"

  @category/negative
  Scenario: An item changed since the caller read it is ABORTED
    Given CompleteItem fails with the kind concurrent change
    When I call ItemsService.CompleteItem with the list "home", the item 1, and the version "7"
    Then the gRPC call of ItemsService should fail with ABORTED and the message "the item was changed since it was read: read it again"
