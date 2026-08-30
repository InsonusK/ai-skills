# Hard conflict / soft conflict

A **hard conflict** is two deltas that cannot both be applied at all — they are formally incompatible (e.g. both assign incompatible types to the same field). A **soft conflict** is two deltas that both apply without any technical error, but whose combined result depends on application order or is semantically wrong. Distinction from Lienhardt and Clarke's work on conflict detection in Delta-Oriented Programming (2012).

## Why it exists
The two need different handling. A hard conflict is a modeling mistake to fix (in this repository's classifier, the "two solutions both `.create` the same artifact" case) — never something to paper over with a resolver. A soft conflict is a legitimate case that needs a [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/glossary/conflict-resolving-delta.md|conflict-resolving delta]] (the `TMC`/`FMC`/`FDC` codes).

## How it works
Hard conflicts can, in principle, be caught mechanically by a type system built for the purpose (Lienhardt/Clarke use row polymorphism). Detecting *every* soft conflict automatically is not a tooling gap but a proven undecidable problem in the general case (Mens, 2002) — soft conflicts are found by grouping deltas that share an element and classifying each group by hand, which is exactly what [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]]'s workflow does.

## How it is structured
In this repository's classifier: the un-coded "two `.create`s on one element" case is the hard-conflict case; `TMC`, `FMC`, `FDC` are the soft-conflict cases that need a resolver; every other code is neither — no conflict at all.

## Example
Two solutions both writing `Implementation/{Command}.cs.create.md` for the same class is a hard conflict — fix by turning one into `.extend`. Two solutions each independently `.extend`-ing `{Command}.cs` with a field the other's logic silently invalidates is a soft conflict — build a resolver.

## Related concepts
- [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/glossary/conflict-resolving-delta.md|Conflict-resolving delta]]

## Sources
- Lienhardt, M., Clarke, D. (2012). "Conflict Detection in Delta-Oriented Programming."
- Mens, T. (2002). "A State-of-the-Art Survey on Software Merging." *IEEE TSE* (cited for the undecidability result on general semantic conflict detection).
