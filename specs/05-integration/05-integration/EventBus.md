# Event Bus (navrh)

## Ucel
- Povinny pro notifikace (UI).
- Orchestrace: zatim ne (decentralizovane schedulery + runtime_lock).

## Typy udalosti (min)
- NotificationRequested
- WebScrapeStarted / WebScrapeFinished / WebScrapeFailed
- AutopollRunStarted / AutopollRunFinished / AutopollRunFailed
- AutopollBatchReady / AutopollBatchCommitted
- UnifierRunStarted / UnifierRunFinished / UnifierRunFailed
- ApprovalNeeded
- ExportCompleted

## Garance doruceni
- At-least-once (odberatele musi byt idempotentni).

## Implementace
- RabbitMQ (self-hosted, zdarma).
- Durable queues + topic exchanges + subscriptions.

## Poznamky
- REST zůstává hlavní synchronní kanál.
