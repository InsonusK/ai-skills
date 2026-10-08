export function extractLinks(text: string): string[] {
  const links = (text.match(/https?:\/\/[^\s<>"']+/gi) || []).map(link => link.replace(/[.,;:!?)]+$/, ''));
  return [...new Set(links)];
}
