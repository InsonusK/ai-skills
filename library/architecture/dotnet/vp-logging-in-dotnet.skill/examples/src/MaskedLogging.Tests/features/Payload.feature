@type/tech-check
Feature: Payloads only Microsoft.Extensions.Logging gives
  As an operator of a dotnet service with the logging variation point
  I want a payload handed over in any way ILogger allows masked or never written
  So that no way of passing a payload in dotnet escapes the mask

  # The paths only dotnet has (H06 of leak-catalog.md); the shared scenarios
  # are masking.feature and payload-policy.feature.

  @category/negative
  Scenario: [H06] A payload given through a message template is masked, and the template written in place of its text
    When a record is logged with the template "called with {payload}" and the payload password "s3cret" and title "buy milk"
    Then the record should have "msg" = "called with {payload}"
    And the record should mention "buy milk"
    And the log should not mention "s3cret"

  @category/negative
  Scenario: [H06] A payload given to a scope is never written
    When a record is logged inside a scope holding the payload password "s3cret"
    Then the log should not mention "s3cret"

  @category/happy
  Scenario: A message template without a payload is written as its text
    When a record is logged with the template "listening on {address}" and the address "http://0.0.0.0:8080"
    Then the record should have "msg" = "listening on http://0.0.0.0:8080"

  @category/boundary
  Scenario: A record whose state is not a list of attributes is written with its text
    When a record is logged with the state "plain text", which is not a list of attributes
    Then the record should have "msg" = "plain text"

  @category/happy
  Scenario: [L15] Headers given as pairs, without IDictionary, are masked by their names
    When the payload is headers with "Authorization" = "Bearer s3cret" and "Accept" = "text/plain"
    Then the masked payload should be '{"Accept":["text/plain"],"Authorization":"***"}'

  @category/negative
  Scenario: A value that is code, not data - a delegate, a stream, a type - is written as its type only
    When the payload is a map with "callback" holding a delegate
    Then the masked payload should be '{"callback":"<unloggable Func`1>"}'

  @category/happy
  Scenario: A value that writes itself as one JSON value, like a time, is written as that value
    When the payload is the time "2026-10-22T10:00:00+00:00"
    Then the masked payload should be '"2026-10-22T10:00:00+00:00"'

  @category/happy
  Scenario: [L04] A non-generic dictionary, like the Data of an exception, is masked by its keys
    When the payload is a non-generic dictionary with "password" = "s3cret" and "title" = "buy milk"
    Then the masked payload should be '{"password":"***","title":"buy milk"}'

  @category/boundary
  Scenario: A property whose getter throws is written unreadable, and an indexer not at all
    When the payload is an object "x" whose other properties cannot be read
    Then the masked payload should be '{"name":"x","fails":"<unreadable>"}'
