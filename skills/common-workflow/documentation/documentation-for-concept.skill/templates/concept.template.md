# How Apply this template
1. Decide the glossary root first, per ["# Where the page lives"](../documentation-for-concept.skill.md#where-the-page-lives): `solution-{Name}.skill/glossary/` when the term belongs to a solution skill, or `docs/glossary/` when it belongs to project documentation.
2. Replace `{Term}` with the exact term/library/technology/pattern name, and `{term-slug}` with its kebab-case file name.
3. Fill every section below following its `hint` block. Use the worked example at [examples/webhook.md](../examples/webhook.md) as a model for depth and tone.
4. Add a mermaid diagram (per [mermaid-diagram.skill.md](skills/common-workflow/mermaid-diagram.skill.md)) in "How it works" or "How it is structured" when a flow or structure is easier to see than to read.
5. Remove all `hint` and `example` blocks, and this `# How Apply this template` section, before saving as `{glossary-root}/{term-slug}.md`.

# {Term}
```hint
One or two sentences: what is it, in plain language. A reader who has never heard the term should understand the gist without reading further.
```
```example
A **webhook** is an HTTP callback: instead of your app repeatedly asking a service "did anything happen yet?", the service sends an HTTP POST to a URL you register the moment something happens.
```

## Why it exists
```hint
The problem this term/library/pattern solves, and what people did before or instead. This is the "why should I care" section.
```
```example
Without webhooks, checking for updates means polling — either you hammer the API on a timer and waste requests, or you poll too slowly and miss events. Webhooks let the service push the event to you the instant it happens, so you get near-real-time updates without polling.
```

## How it works
```hint
The mechanics: the sequence of steps, the actors involved, the data that moves between them. Add a mermaid diagram when a sequence or flow is easier to see than to read.
```

## How it is structured
```hint
The parts/components that make it up and how they relate to each other — for example the pieces of a library's API, the phases of a pattern, or the components of an architecture. Add a diagram if it clarifies the relationships.
```

## Example
```hint
A concrete example, preferably from this project's own code. If none exists yet, use a minimal, realistic example instead of abstract pseudo-code.
```

## Related concepts
```hint
Links to other docs/glossary/ pages a reader is likely to need next. Leave this section out if there are none yet.
```

## Sources
```hint
Where this explanation came from: official docs URL, RFC, or a source file in this project. Required whenever the term refers to an external library, technology, or pattern, so the page can be verified or refreshed later.
```
