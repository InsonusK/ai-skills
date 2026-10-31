@type/tech-check
Feature: One scope per command
  As a variation point with something that lives per command, such as a database context
  I want every command an entry point sends to run in a scope of its own, its nested commands inside it
  So that one operation shares one context, and no two operations share one

  Background:
    Given an application with the command scope

  @category/happy
  Scenario: Commands sent from outside run in scopes of their own
    When a command is sent
    And a command is sent
    Then each command should have run in a scope of its own
    And 2 scopes should have been disposed

  @category/happy
  Scenario: A nested command runs in its parent's scope
    When a command that sends a nested command is sent
    Then the nested command should have run in its parent's scope
    And 1 scope should have been disposed

  @category/edge
  Scenario: Commands sent at once do not share a scope
    When two commands are sent at once
    Then each command should have run in a scope of its own

  @category/error
  Scenario: A command that fails still ends its scope
    When a command that fails is sent
    Then the failure should reach the sender
    And 1 scope should have been disposed

  @category/error
  Scenario: The next command after a failure gets a new scope
    When a command that fails is sent
    And a command is sent
    Then 2 scopes should have been disposed

  @category/negative
  Scenario: A stream has no command scope
    When a stream is asked for
    Then the stream should be refused
