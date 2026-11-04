# Shared by every stack's row version variation point. The source is
# library/architecture/spec/vp-row-version.skill/conformance/; a stack's copy is
# written by `make conformance-sync` and never edited.
@type/infrastructure/db/postgres
Feature: The version of a row
  As the service over a table whose rows change
  I want every write to compare the version the caller read
  So that of two writers the first is kept and the second is told

  Background:
    Given a database with the sample table of notes
    And the note "a"
    And the note "b"

  @category/happy
  Scenario: A row read answers its version
    When I read the note "a"
    Then the row should carry a version

  @category/happy
  Scenario: A new row carries a version, and saves with it
    When I insert the note "c"
    And I change that row to "c2" and save it
    Then no failure should be reported
    And the note "c2" should be there for any other connection

  @category/happy
  Scenario: A save with the version read is kept and moves the version
    When I read the note "a"
    And I change that row to "a2" and save it
    Then no failure should be reported
    And the row should carry another version than it was read with

  @category/happy
  Scenario: The same row saves twice in a row
    When I read the note "a"
    And I change that row to "a2" and save it
    And I change that row to "a3" and save it
    Then no failure should be reported
    And the note "a3" should be there for any other connection

  @category/negative
  Scenario: The second of two saves from one read is a changed row
    When two callers read the note "a"
    And the first caller saves "first"
    And the second caller saves "second"
    Then the repository should report a changed row
    And the note "first" should be there for any other connection

  @category/negative
  Scenario: A version a caller kept is compared when it comes back
    Given a caller keeps the version of the note "a"
    And another connection changes the note "a" to "other"
    When the caller saves "mine" with the version it kept
    Then the repository should report a changed row
    And the note "other" should be there for any other connection

  @category/happy
  Scenario: A version a caller kept and nobody moved is accepted
    Given a caller keeps the version of the note "a"
    When the caller saves "mine" with the version it kept
    Then no failure should be reported
    And the note "mine" should be there for any other connection

  @category/happy
  Scenario: A delete with a version a caller kept and nobody moved deletes the row
    Given a caller keeps the version of the note "a"
    When the caller deletes with the version it kept
    Then no failure should be reported
    And the note "a" should be gone for any other connection

  @category/negative
  Scenario: A delete with a moved version is a changed row
    Given a caller keeps the version of the note "a"
    And another connection changes the note "a" to "other"
    When the caller deletes with the version it kept
    Then the repository should report a changed row
    And the note "other" should be there for any other connection

  @category/edge
  Scenario: A statement that knows nothing of the version moves it
    When I read the note "a"
    And another connection changes the note "a" to "other"
    And I change that row to "mine" and save it
    Then the repository should report a changed row

  @category/edge
  Scenario: Update of a selector fails as a whole when one of its rows moved
    When I update every note while another connection changes the note "b" to "other"
    Then the repository should report a changed row
    And the note "a" should be there for any other connection
    And the note "other" should be there for any other connection

  @category/edge
  Scenario: Of two saves at the same moment exactly one is kept
    When two callers save one row at the same moment, 50 times
    Then every time exactly one of them should have been told a changed row

  @category/negative
  Scenario: Every row of the application is marked
    Then every mapped row should be marked as changing or as never changing
