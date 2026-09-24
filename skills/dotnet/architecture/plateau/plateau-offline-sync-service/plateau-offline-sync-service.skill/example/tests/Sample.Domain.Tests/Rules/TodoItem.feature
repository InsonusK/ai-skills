Feature: TodoItem domain rules

  @negative
  Scenario: A completed item cannot be renamed
    Given a completed item
    When it is renamed
    Then a domain error "Sample.TodoItem.RenameCompleted" is raised

  @negative
  Scenario: An item cannot be completed twice
    Given a completed item
    When it is completed again
    Then a domain error "Sample.TodoItem.AlreadyDone" is raised
