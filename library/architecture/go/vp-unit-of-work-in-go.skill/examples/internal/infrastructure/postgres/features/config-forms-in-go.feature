@type/tech-check
Feature: The forms of DATABASE_DSN in Go
  As the operator of the service
  I want a DSN in either form pgx reads taken as it is
  So that the URL and the key=value form both work

  @category/happy
  Scenario Outline: The DSN is read as it is
    Given the environment variable "DATABASE_DSN" is "<dsn>"
    When the settings are loaded
    Then the DSN should be "<dsn>"

    Examples:
      | dsn                                                  |
      | postgres://todo:secret@db:5432/todo?sslmode=disable  |
      | host=db port=5432 user=todo password=secret dbname=todo |
