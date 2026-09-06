Feature: TodoItem domain rules

  Scenario: A completed item cannot be renamed
    Given a completed item
    When it is renamed
    Then a domain error "Sample.TodoItem.RenameCompleted" is raised

  Scenario: An item cannot be completed twice
    Given a completed item
    When it is completed again
    Then a domain error "Sample.TodoItem.AlreadyDone" is raised
