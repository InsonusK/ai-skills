Feature: Check a URL
  As a caller of the link-check service
  I want to validate and normalize a URL
  So that I can be sure it is well-formed before using it further

  Scenario Outline: Check a URL
    Given the URL "<input>"
    When I check the URL
    Then the check should be "<outcome>"
    And the normalized URL should be "<normalized>"

    Examples:
      | input                     | outcome | normalized                |
      | https://Example.com/Path  | valid   | https://example.com/Path  |
      | HTTP://EXAMPLE.COM        | valid   | http://example.com        |
      | not-a-url                 | invalid |                           |
      | ftp://example.com/file    | invalid |                           |
      |                           | invalid |                           |
