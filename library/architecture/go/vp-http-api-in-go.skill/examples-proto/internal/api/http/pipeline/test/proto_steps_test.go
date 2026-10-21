package test

import (
	"net/http"

	"google.golang.org/protobuf/types/known/apipb"

	httppipeline "github.com/example/todo/internal/api/http/pipeline"
)

// The proto routes use apipb.Method, a well-known message with a snake_case
// field (request_type_url), so these tests need no proto file of their own.
func init() {
	moreRoutes = func(w *World) []httppipeline.Route {
		decode := func(r *http.Request) (echoCommand, error) {
			msg := &apipb.Method{}
			if err := httppipeline.DecodeProto(r, msg); err != nil {
				return echoCommand{}, err
			}
			return echoCommand{Text: msg.GetRequestTypeUrl()}, nil
		}
		encode := func(result echoResult) any {
			return httppipeline.ProtoJSON(&apipb.Method{ResponseTypeUrl: result.Text})
		}
		return []httppipeline.Route{
			{Pattern: "POST /v1/proto", Handler: httppipeline.Endpoint(w.echoHandler(), decode, encode, http.StatusOK)},
			{Pattern: "POST /v1/old", Handler: httppipeline.Unimplemented("Old is deprecated since 1.1.0")},
		}
	}
}
