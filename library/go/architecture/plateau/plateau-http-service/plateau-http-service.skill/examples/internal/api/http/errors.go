package http

import (
	"encoding/json"
	"net/http"
)

// The grpc status codes this adapter answers with. The numbers are those of
// google.rpc.Code, so the error body reads the same as in a service that has a
// grpc contract.
const (
	codeInvalidArgument = 3
	codeInternal        = 13
)

// httpStatus is the canonical gRPC-to-HTTP mapping for the codes above; a code
// not listed is a server fault.
var httpStatus = map[int]int{
	codeInvalidArgument: http.StatusBadRequest,
	codeInternal:        http.StatusInternalServerError,
}

// statusBody is google.rpc.Status in proto-JSON.
type statusBody struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
	Details []any  `json:"details"`
}

func writeError(w http.ResponseWriter, code int, err error) {
	status, ok := httpStatus[code]
	if !ok {
		status = http.StatusInternalServerError
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(statusBody{Code: code, Message: err.Error(), Details: []any{}})
}
