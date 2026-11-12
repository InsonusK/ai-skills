# Shared by every stack's PostgreSQL variation point. The source is
# library/architecture/spec/vp-postgres.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited.
#
# An answer of rows is a list of row objects, never text. A table of the form
# "| title | done |" names the fields each row must have; "one row" is a single
# object, "no row" is the absence of one, "an empty list" is a list with no rows.
@type/infrastructure/db/postgres
Feature: The selector works on the rows a specification picks
  As the service over the database
  I want to take the rows of one specification and ask what I need
  So that a count, a read, and an update of the same rows are one description

  Background:
    Given a database with the sample tables
    And the owner "ann"
    And the task "a" of "ann" is open
    And the task "b" of "ann" is open
    And the task "c" of "ann" is done

  @category/happy
  Scenario: All answers a list of the picked rows
    When I select the unclosed tasks of "ann"
    Then All should answer the rows:
      | title | done  |
      | a     | false |
      | b     | false |

  @category/happy
  Scenario: Count answers how many rows are picked
    When I select the unclosed tasks of "ann"
    Then Count should answer 2

  @category/happy
  Scenario: Any answers true when a row is picked
    When I select the unclosed tasks of "ann"
    Then Any should answer true

  @category/boundary
  Scenario: A specification that picks nothing is no failure
    When I select the unclosed tasks of "nobody"
    Then All should answer an empty list
    And Count should answer 0
    And Any should answer false

  @category/happy
  Scenario: SelectBy runs no statement
    Given a database that does not answer
    When I select the unclosed tasks of "ann"
    Then no failure should be reported

  @category/happy
  Scenario: Single answers the row of a unique key, with no order
    When I select the task "a" by its id
    Then Single should answer one row:
      | title | done  |
      | a     | false |

  @category/boundary
  Scenario: Single answers no row when nothing is picked
    When I select a task by an id nobody has
    Then Single should answer no row

  @category/negative
  Scenario: Single fails when more than one row is picked
    When I select the unclosed tasks of "ann"
    Then Single should fail with no outcome of the repository

  @category/happy
  Scenario: First answers the first row of the order
    When I select the unclosed tasks of "ann" ordered by title descending
    Then First should answer one row:
      | title | done  |
      | b     | false |

  @category/boundary
  Scenario: First answers no row when nothing is picked
    When I select the unclosed tasks of "nobody" ordered by title
    Then First should answer no row

  @category/happy
  Scenario: Take answers a list of up to n rows
    When I select the unclosed tasks of "ann" ordered by title
    Then Take 1 should answer the rows:
      | title | done  |
      | a     | false |

  @category/boundary
  Scenario: Take answers every row when fewer exist
    When I select the unclosed tasks of "ann" ordered by title
    Then Take 99 should answer the rows:
      | title | done  |
      | a     | false |
      | b     | false |

  @category/negative
  Scenario: Take refuses a count below 1
    When I select the unclosed tasks of "ann" ordered by title
    Then Take 0 should be refused

  @category/happy
  Scenario: Page answers a list of the rows of one page, numbered from 1
    Given the task "d" of "ann" is open
    And the task "e" of "ann" is open
    When I select the unclosed tasks of "ann" ordered by title
    Then Page 1 of size 2 should answer the rows:
      | title | done  |
      | a     | false |
      | b     | false |
    And Page 2 of size 2 should answer the rows:
      | title | done  |
      | d     | false |
      | e     | false |

  @category/boundary
  Scenario: A page past the end is an empty list
    When I select the unclosed tasks of "ann" ordered by title
    Then Page 3 of size 2 should answer an empty list

  @category/negative
  Scenario: Page refuses a number below 1
    When I select the unclosed tasks of "ann" ordered by title
    Then Page 0 of size 2 should be refused

  @category/happy
  Scenario: ThenBy adds a key to the order
    Given the owner "bob"
    And the task "aa" of "bob" is open
    When I select every open task ordered by owner, then by title
    Then All should answer the rows:
      | title | done  |
      | a     | false |
      | b     | false |
      | aa    | false |

  @category/edge
  Scenario: An order gives a new selector and leaves the first one as it is
    When I select the unclosed tasks of "ann"
    And I order that selector by title and, apart, by title descending
    Then First of the first order should answer one row:
      | title | done  |
      | a     | false |
    And First of the second order should answer one row:
      | title | done  |
      | b     | false |

  @category/happy
  Scenario: Update changes every picked row and answers how many
    When I select the unclosed tasks of "ann"
    And I update them as done
    Then the update should answer 2
    And the table "tasks" should hold 3 done rows for any other connection

  @category/edge
  Scenario: An order changes nothing for Count, Update, and Delete
    When I select the unclosed tasks of "ann" ordered by title
    Then Count should answer 2
    When I update them as done
    Then the update should answer 2

  @category/happy
  Scenario: Delete removes every picked row and answers how many
    When I select the unclosed tasks of "ann"
    And I delete them
    Then the delete should answer 2
    And the table "tasks" should hold 1 rows for any other connection

  @category/edge
  Scenario: A selector is no snapshot: the write answers what it touched
    When I select the unclosed tasks of "ann"
    Then Count should answer 2
    When another connection adds the open task "late" of "ann"
    And I update them as done
    Then the update should answer 3

  @category/edge
  Scenario: A selector asks again after its own write
    When I select the unclosed tasks of "ann"
    And I update them as done
    Then Count should answer 0
