{{/*
Standard chart name + common labels, reused by every template in this
chart so a `kubectl get all -l app.kubernetes.io/instance={service-name}`
finds every resource this release owns, including the migrate Job.
*/}}
{{- define "chart.name" -}}
{{- .Chart.Name -}}
{{- end -}}

{{- define "chart.labels" -}}
app.kubernetes.io/name: {{ include "chart.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end -}}

{{- define "chart.selectorLabels" -}}
app.kubernetes.io/name: {{ include "chart.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}
