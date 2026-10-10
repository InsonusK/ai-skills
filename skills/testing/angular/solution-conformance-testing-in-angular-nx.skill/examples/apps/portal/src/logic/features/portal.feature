@type/service
Feature: Portal review heading
  @category/happy
  Scenario: A single link review
    When the portal reviews 1 link
    Then the portal heading is "Review 1 link"
  @category/boundary
  Scenario: An empty review
    When the portal reviews 0 links
    Then the portal heading is "Review 0 links"
