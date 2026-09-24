Feature: ItemTitle rule

  The centralized title condition — one Gherkin source, re-proven by every layer
  that redirects to it. Every scenario here is @format: the wrapper is a single
  SoftItemTitle checked in isolation, so it is proven at the rule itself
  (Sample.Domain.Rules.Tests) and again through the ItemTitle constructor
  (Sample.Domain.Tests). Text is domain language only, so the same file could be
  copied into a client repo in another language.

  @format @negative
  Scenario: A blank title fails the required rule
    When the title "" is checked
    Then the check fails with error code "Sample.ItemTitle.Required"

  @format @boundary
  Scenario: An over-long title fails the length rule
    When a 101-character title is checked
    Then the check fails with error code "Sample.ItemTitle.TooLong"

  @format @happy
  Scenario: A normal title passes
    When the title "buy milk" is checked
    Then the check passes
