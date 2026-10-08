package linkcheck

import (
	"bufio"
	"encoding/json"
	"errors"
	"fmt"
	"os"
	"sync"
)

var ErrStoreCorrupted = errors.New("store corrupted")

// HistoryStore serializes access to one JSON-lines history file.
type HistoryStore struct {
	path string
	mu   sync.Mutex
}

func NewHistoryStore(path string) *HistoryStore { return &HistoryStore{path: path} }
func (s *HistoryStore) Append(result Result) error {
	s.mu.Lock()
	defer s.mu.Unlock()
	f, err := os.OpenFile(s.path, os.O_APPEND|os.O_CREATE|os.O_WRONLY, 0600)
	if err != nil {
		return err
	}
	defer f.Close()
	return json.NewEncoder(f).Encode(ToRecord(result))
}
func (s *HistoryStore) Load() ([]Result, error) {
	s.mu.Lock()
	defer s.mu.Unlock()
	f, err := os.Open(s.path)
	if errors.Is(err, os.ErrNotExist) {
		return []Result{}, nil
	}
	if err != nil {
		return nil, err
	}
	defer f.Close()
	results := []Result{}
	scan := bufio.NewScanner(f)
	for scan.Scan() {
		var record map[string]any
		if err := json.Unmarshal(scan.Bytes(), &record); err != nil {
			return nil, fmt.Errorf("%w: %v", ErrStoreCorrupted, err)
		}
		result, err := FromRecord(record)
		if err != nil {
			return nil, fmt.Errorf("%w: %v", ErrStoreCorrupted, err)
		}
		results = append(results, result)
	}
	return results, scan.Err()
}
