// Package mock is the mock of the todo API's client (todoapi): an emulator
// of the API inside the process, which TODO_API_HOST=mock runs the client
// against, so the application runs with no todo service reachable.
package mock

import (
	"encoding/json"
	"net/http"
)

// Emulator answers every operation of todoapi.Operations as the todo API
// does, with stable data and no state. A new operation of the adapter adds
// its route here.
func Emulator() http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("POST /v1/items/plan", func(w http.ResponseWriter, r *http.Request) {
		var request struct {
			Titles []string `json:"titles"`
		}
		if err := json.NewDecoder(r.Body).Decode(&request); err != nil || len(request.Titles) == 0 {
			answer(w, http.StatusBadRequest, map[string]any{"code": 3, "message": "no items to plan", "details": []any{}})
			return
		}
		items := make([]map[string]any, len(request.Titles))
		for i, title := range request.Titles {
			items[i] = map[string]any{"title": title, "done": false}
		}
		answer(w, http.StatusOK, map[string]any{"items": items})
	})
	return mux
}

func answer(w http.ResponseWriter, status int, body any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(body)
}
