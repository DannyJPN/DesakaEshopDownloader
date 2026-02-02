# Non-Functional Requirements (navrh)

## Dostupnost
- Cíl: 24/7 dostupnost (ideal).
- Implementace HA je dle moznosti prostredi; kratke vypadky jsou akceptovatelne, ale system ma bezet prubezne.

## Vykon (soft limity)
- Full scrape per eshop: cil do 3 hodin (soft limit).
- Unifier: cas do stavu "vse zpracovano bez potvrzeni" do 24 hodin (soft limit).
- Unifier nema hard timeout (blokuje jej approval workflow).

## Retence logu/historie
- Historie produktu: default 1 rok (konfigurovatelne).
- Logy: default 1 rok (konfigurovatelne).

## Skálování
- Horizontalni skálování sluzeb (oddeleny beh na vice strojich).
- DB je jedina centralni zavislost (provider vymenitelny).

## Observability
- Health endpointy pro vsechny sluzby.
- Structured logs + korelace (correlation_id).
