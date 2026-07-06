export function toDateInputValue(value: string | Date | null | undefined): string {
  if (!value) {
    return '';
  }
  const date = typeof value === 'string' ? new Date(value) : value;
  return date.toISOString().slice(0, 10);
}
