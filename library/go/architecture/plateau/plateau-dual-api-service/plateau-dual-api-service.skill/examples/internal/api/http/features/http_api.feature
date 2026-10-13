@type/api/http
Feature: HTTP API
  As a caller without a grpc stack
  I want the service's capability and its description over HTTP
  So that a browser, curl or a simple client can use it and see what it offers

  @category/happy
  Scenario: the HTTP API serves exactly the operations its contract describes
    Then the HTTP API and its OpenAPI contract both have exactly the operations:
      | method | path            |
      | POST   | /v1/links/check |

  Scenario Outline: a request is answered with a status and a JSON body
    When I send "<method>" "<path>" with the body:
      """
      <body>
      """
    Then the response status should be <status>
    And the response body should have "<field>" equal to "<value>"

    @category/happy
    Examples: the check succeeds
      | method | path            | body                                           | status | field      | value                    |
      | POST   | /v1/links/check | {"url": "https://Example.com/Path"}            | 200    | normalized | https://example.com/Path |
      | POST   | /v1/links/check | {"url": "https://example.com", "later": true}  | 200    | url        | https://example.com      |

    @category/negative
    Examples: the request is refused with a google.rpc.Status body
      | method | path            | body                  | status | field | value |
      | POST   | /v1/links/check | {"url": "not-a-url"}  | 400    | code  | 3     |
      | POST   | /v1/links/check | {"url":               | 400    | code  | 3     |

  @category/happy
  Scenario: the health endpoint reports the running version
    When I send "GET" "/health"
    Then the response status should be 200
    And the response body should have "version" equal to the binary's version

  Scenario Outline: the docs routes follow HTTP_API_DOCS_ENABLED and HTTP_BASE_PATH
    Given the service is started with docs "<docs>" under the base path "<base path>"
    When I send "GET" "<path>"
    Then the response status should be <status>
    And the response should mention "<mentions>"

    @category/happy
    Examples: docs on
      | docs | base path | path          | status | mentions              |
      | on   |           | /openapi.json | 200    | /v1/links/check       |
      | on   |           | /swagger/     | 200    | /openapi.json         |
      | on   | /prefix   | /openapi.json | 200    | "basePath":"/prefix"  |
      | on   | /prefix   | /swagger/     | 200    | /prefix/openapi.json  |
      | on   | /prefix   | /swagger      | 301    | /prefix/swagger/      |

    @category/negative
    Examples: docs off - the routes do not exist
      | docs | base path | path          | status | mentions |
      | off  |           | /openapi.json | 404    |          |
      | off  |           | /swagger/     | 404    |          |
