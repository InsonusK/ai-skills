@type/tech-check
Feature: The package describes itself
  Tooling reads the version from the package; it must be the one the distribution declares.

  @category/happy
  Scenario: The package reports the version of its distribution
    Then the package version equals the installed distribution's version
