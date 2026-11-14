# Shared by every stack whose PostgreSQL variation point has the rows form (Go, Python). The source is
# library/architecture/spec/vp-postgres.skill/conformance/postgres-selector/; a stack's copy is
# written by `make conformance-sync` and never edited.
#
# An answer of rows is a list of row objects, never text. A table of the form
# "| title | done |" names the fields each row must have; "one row" is a single
# object, "no row" is the absence of one, "an empty list" is a list with no rows.
@type/infrastructure/db/postgres
Feature: A specification picks rows
  As the service over the database
  I want a named description of a set of rows
  So that it is written once and says nothing of what is done with the rows

  Background:
    Given a database with the sample tables
    And the owner "ann"
    And the owner "bob"
    And the task "a" of "ann" is open
    And the task "b" of "bob" is open

  @category/happy
  Scenario: A specification picks rows along a relation
    When I select the unclosed tasks of "ann"
    Then All should answer the rows:
      | title | done  |
      | a     | false |

  @category/happy
  Scenario: A projection answers a column of a related table
    When I select the titles with their owners of "ann"
    Then All should answer the rows:
      | owner | title |
      | ann   | a     |

  @category/happy
  Scenario: A projection takes an order and a page
    Given the task "c" of "ann" is open
    When I select the titles with their owners of "ann" ordered by title descending
    Then First should answer one row:
      | owner | title |
      | ann   | c     |
    And Take 1 should answer the rows:
      | owner | title |
      | ann   | c     |

  @category/edge
  Scenario: One specification serves several selectors
    Given the task "c" of "ann" is open
    When I give one specification of "ann" to two selectors
    Then the first should count 2
    And the second should delete 2

  @category/negative
  Scenario: A specification with a page is refused
    When I select through a specification that holds a page
    Then the selection should be refused

  @category/negative
  Scenario: A specification with an order is refused
    When I select through a specification that holds an order
    Then the selection should be refused
