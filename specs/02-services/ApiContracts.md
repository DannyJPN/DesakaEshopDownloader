# API kontrakty (REST)

## Konvence
- JSON
- versioning (/v1/...)
- request-id a correlation-id

## Typické endpoints
- POST /v1/jobs
- GET  /v1/jobs/{id}
- POST /v1/jobs/{id}/start
- GET  /v1/memory
- POST /v1/exports

## Stavové modely
- Job: Queued → Running → Completed/Failed
- Step: Pending → Active → Done → Error
