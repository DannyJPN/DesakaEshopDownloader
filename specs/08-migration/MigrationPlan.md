# Migracni plan (navrh)

## Import dat
- Primarne jednorazovy import z CSV/XLS do nove DB.
- Musi byt mozne import zopakovat (v pripade nezdaru/testovani).

## Paralelni provoz
- Stary a novy system mohou bezet nezavisle (bez vzajemne zavislosti).

## Cutover
- Neni vyzadovan strictni cutover plan; provoz je oddeleny.
