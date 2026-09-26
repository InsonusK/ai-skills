Feature: Sample module public contracts

  @happy
  Scenario: The add-item command carries a result payload and a timestamp
    When an AddItemCommand is created
    Then it implements ICommand of Result of AddItemResult
    And it implements ICommandWithTimestamp

  @concurrency
  Scenario: The rename command carries versions for the concurrency guard
    When a RenameItemCommand is created with expected version 4
    Then its Versions map holds TodoItem id-to-version 4
