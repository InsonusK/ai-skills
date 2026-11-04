# Shared by every stack's external id variation point. The source is
# library/architecture/spec/vp-external-id.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited.
@type/infrastructure/db/postgres
Feature: An identity set by the caller
  As the service over a table whose rows a caller outside it creates
  I want the row saved under the UUID v7 the caller made, and a taken one refused
  So that the caller knows the id before the answer, and two rows never share it

  Background:
    Given a database with the sample table of drafts

  @category/happy
  Scenario: A row is saved under the id the caller gave
    When I insert the draft "a" under the id "0190f5e4-6c2a-7b3d-8e4f-123456789abc"
    Then no failure should be reported
    And the draft "a" should be there under the id "0190f5e4-6c2a-7b3d-8e4f-123456789abc" for any other connection

  @category/negative
  Scenario: An id given twice is a duplicate key, and the first row stays
    Given the draft "a" under the id "0190f5e4-6c2a-7b3d-8e4f-123456789abc"
    When I insert the draft "b" under the id "0190f5e4-6c2a-7b3d-8e4f-123456789abc"
    Then the repository should report a duplicate key
    And the draft "a" should be there under the id "0190f5e4-6c2a-7b3d-8e4f-123456789abc" for any other connection

  @category/happy
  Scenario: A row the caller created answers its version, and saves with it
    When I insert the draft "a" under the id "0190f5e4-6c2a-7b3d-8e4f-123456789abc"
    Then the row should carry a version
    When I change that row to "a2" and save it
    Then no failure should be reported
    And the draft "a2" should be there under the id "0190f5e4-6c2a-7b3d-8e4f-123456789abc" for any other connection

  @category/happy
  Scenario Outline: A UUID v7 in its form of 36 characters is an id the caller may give
    When the caller's id "<id>" is parsed
    Then it should be accepted as "<as>"

    Examples:
      | id                                   | as                                   |
      | 0190f5e4-6c2a-7b3d-8e4f-123456789abc | 0190f5e4-6c2a-7b3d-8e4f-123456789abc |
      | 0190F5E4-6C2A-7B3D-8E4F-123456789ABC | 0190f5e4-6c2a-7b3d-8e4f-123456789abc |

  @category/negative
  Scenario Outline: Anything but a UUID v7 is refused as invalid
    When the caller's id "<id>" is parsed
    Then it should be refused as invalid

    Examples:
      | id                                     | why               |
      | 6f1c2b8e-4d3a-4f5b-9c6d-0e1f2a3b4c5d   | version 4         |
      | 0190f5e4-6c2a-1b3d-8e4f-123456789abc   | version 1         |
      | 0190f5e4-6c2a-7b3d-ce4f-123456789abc   | not the variant   |
      | 00000000-0000-0000-0000-000000000000   | the nil UUID      |
      | 0190f5e46c2a7b3d8e4f123456789abc       | not 36 characters |
      | {0190f5e4-6c2a-7b3d-8e4f-123456789abc} | not 36 characters |
      | not an id                              | no UUID           |
      |                                        | empty             |

  @category/negative
  Scenario: Every row of the application is marked by who sets its identity
    Then every mapped row should be marked as keyed by the service or by the caller
