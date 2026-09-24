Feature: Sample module public contracts

  @happy
  Scenario: The greet command is a command returning a result payload
    When a GreetCommand is created with message "World"
    Then it implements ICommand of Result of GreetResult

  @happy
  Scenario: The greeted event is a notification
    Then Greeted implements INotificationEvent
