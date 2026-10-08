const linkPattern = /https?:\/\/[^\s<>"']+/gi;

/** Every http(s) link of the text, in order of first appearance, each one once. */
export function extractLinks(text: string): string[] {
  const links: string[] = [];
  for (const match of text.match(linkPattern) ?? []) {
    const link = match.replace(/[.,;:!?)]+$/, "");
    if (!links.includes(link)) {
      links.push(link);
    }
  }
  return links;
}
