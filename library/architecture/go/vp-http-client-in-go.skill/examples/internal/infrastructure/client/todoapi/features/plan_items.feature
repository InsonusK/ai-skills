@type/tech-check
Feature: The todo API's PlanItems
  As the service that plans with the todo service
  I want PlanItems sent as the contract says and its answer mapped to items
  So that the service sees items, never the contract's shapes

  @category/happy
  Scenario: The titles are sent and the planned items come back
    Given the todo API answers '{"items":[{"title":"buy milk","done":false},{"title":"walk the dog","done":true}]}'
    When I call PlanItems with the titles "buy milk|walk the dog"
    Then the todo API should have received POST "/v1/items/plan" with '{"titles":["buy milk","walk the dog"]}'
    And the items should be "buy milk:open|walk the dog:done"

  @category/happy
  Scenario: With the host mock, the emulator plans one open item per title
    Given the todo API is mocked
    When I call PlanItems with the titles "buy milk|walk the dog"
    Then the items should be "buy milk:open|walk the dog:open"
