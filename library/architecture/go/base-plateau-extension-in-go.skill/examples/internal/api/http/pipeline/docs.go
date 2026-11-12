// Source: skill vp-http-api-in-go

package pipeline

import (
	"encoding/json"
	"net/http"

	"github.com/swaggest/swgui"
	"github.com/swaggest/swgui/v5emb"

	"github.com/example/todo/docs/integration/api/todo"
)

// registerDocs adds the spec and Swagger UI routes. They are mounted at "/"
// like every route; each URL they hand to a browser carries basePath.
func registerDocs(mux *http.ServeMux, basePath string) {
	spec := publishedSpec(todo.Spec, basePath)
	mux.HandleFunc("GET /openapi.json", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write(spec)
	})

	ui := v5emb.NewHandlerWithConfig(swgui.Config{
		Title:            "Todo",
		SwaggerJSON:      basePath + "/openapi.json",
		BasePath:         basePath + "/swagger/",
		InternalBasePath: "/swagger/",
	})
	mux.Handle("GET /swagger/", ui)
	// Registered by hand: the redirect ServeMux would add for "/swagger" points
	// at "/swagger/" and loses the prefix.
	mux.HandleFunc("GET /swagger", func(w http.ResponseWriter, r *http.Request) {
		http.Redirect(w, r, basePath+"/swagger/", http.StatusMovedPermanently)
	})
}

// publishedSpec is the embedded contract with the path the service is
// published under set in it, so Swagger UI's "Try it out" calls this service
// and not the ingress root: servers[0].url in a hand-written OpenAPI 3 spec,
// basePath in an OpenAPI 2 spec generated from a proto file. The committed
// file names neither.
func publishedSpec(embedded []byte, basePath string) []byte {
	if basePath == "" {
		return embedded
	}
	var doc map[string]any
	if err := json.Unmarshal(embedded, &doc); err != nil {
		panic("the embedded OpenAPI contract is not JSON: " + err.Error())
	}
	if _, v2 := doc["swagger"]; v2 {
		doc["basePath"] = basePath
	} else {
		doc["servers"] = []map[string]string{{"url": basePath}}
	}
	published, err := json.Marshal(doc)
	if err != nil {
		panic(err)
	}
	return published
}
