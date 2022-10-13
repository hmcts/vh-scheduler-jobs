{{/* metadata for cronjobs*/}}
{{- define "cronjob.metadata" }}
metadata:
  name: {{ .cron.cronJobName }}
  labels:
    app.kubernetes.io/name: {{ .cron.cronJobName }}
    helm.sh/chart: {{ .root.Chart.Name }}-{{ .root.Chart.Version }}
    app.kubernetes.io/managed-by: {{ .root.Release.Service }}
    app.kubernetes.io/instance: {{ .cron.cronJobName }}
    {{- if .root.Values.aadIdentityName }}
    aadpodidbinding: {{ .root.Values.aadIdentityName }}
    {{- end }}
{{- include "hmcts.annotations.v2" .root | indent 2 -}}
{{- end }}