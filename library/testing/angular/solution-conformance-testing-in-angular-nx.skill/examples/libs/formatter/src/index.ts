export function formatHeading(count: number): string {
  if (!Number.isInteger(count) || count < 0) throw new Error("invalid count");
  return `${count} ${count === 1 ? "link" : "links"}`;
}
