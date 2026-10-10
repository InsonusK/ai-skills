package linkcheck

import "errors"

var ErrMissingField = errors.New("MISSING_FIELD")

// ToRecord maps the stable public result fields to a storage record.
func ToRecord(result Result) map[string]any {
	return map[string]any{"is_valid": result.IsValid, "normalized": result.Normalized, "error_code": result.ErrorCode}
}

// FromRecord reconstructs a result and rejects records without a validity flag.
func FromRecord(record map[string]any) (Result, error) {
	valid, exists := record["is_valid"]
	if !exists {
		return Result{}, ErrMissingField
	}
	isValid, _ := valid.(bool)
	normalized, _ := record["normalized"].(string)
	code, _ := record["error_code"].(string)
	return Result{IsValid: isValid, Normalized: normalized, ErrorCode: code}, nil
}
