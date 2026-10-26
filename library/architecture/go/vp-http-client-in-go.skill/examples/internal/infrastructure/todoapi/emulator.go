package todoapi

import (
	"encoding/json"
	"net/http"
)

// Emulator answers every operation of Operations as the todo API does, with
// stable data and no state: TODO_API_HOST=mock runs the client against it, so
// the application runs with no todo service reachable. A new operation of
// the adapter adds its route here.
func Emulator() http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("POST /v1/items/plan", func(w http.ResponseWriter, r *http.Request) {
		var request planItemsRequest
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
