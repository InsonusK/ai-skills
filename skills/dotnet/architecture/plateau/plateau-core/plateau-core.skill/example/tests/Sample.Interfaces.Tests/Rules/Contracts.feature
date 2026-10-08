@type/contract
Feature: Sample module public contracts

  @category/happy
  Scenario: The greet command is a command returning a result payload
    When a GreetCommand is created with message "World"
    Then it implements ICommand of Result of GreetResult

  @category/happy
  Scenario: The greeted event is a notification
    Then Greeted implements INotificationEvent
