package test

import (
	"fmt"
	"github.com/cucumber/godog"
	"github.com/example/todo/internal/infrastructure/postgres"
)

func registerMappingSteps(sc *godog.ScenarioContext) {
	var row any
	sc.Step(`^a sample version declaration "([^"]*)"$`, func(declaration string) error {
		switch declaration {
		case "valid":
			row = struct {
				Version postgres.Version `gorm:"column:xmin;->;-:migration"`
			}{}
		case "ignored":
			row = struct {
				Version postgres.Version `gorm:"-"`
			}{}
		case "wrong column":
			row = struct {
				Version postgres.Version `gorm:"column:version;->;-:migration"`
			}{}
		case "writable":
			row = struct {
				Version postgres.Version `gorm:"column:xmin;-:migration"`
			}{}
		case "migrated":
			row = struct {
				Version postgres.Version `gorm:"column:xmin;->"`
			}{}
		case "wrong type":
			row = struct {
				Version string `gorm:"column:xmin;->;-:migration"`
			}{}
		default:
			return fmt.Errorf("unknown declaration %s", declaration)
		}
		return nil
	})
	sc.Step(`^that xmin mapping should be recognised "([^"]*)"$`, func(want string) error {
		if got := declaresVersion(row); got != (want == "true") {
			return fmt.Errorf("mapping recognised=%v, want %s", got, want)
		}
		return nil
	})
}
