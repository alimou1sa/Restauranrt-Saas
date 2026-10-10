// .NET serializes DateTime of Kind=Unspecified without a "Z" (e.g. "2026-10-04T08:30:00"),
// which JS would parse as LOCAL time. The backend stores UTC, so force UTC when no zone is present.
export function parseUtc(value: string): Date {
  const hasZone = /([zZ]|[+-]\d{2}:?\d{2})$/.test(value);
  return new Date(hasZone ? value : `${value}Z`);
}

export function isToday(value: string): boolean {
  return parseUtc(value).toDateString() === new Date().toDateString();
}

const numberFmt = new Intl.NumberFormat(undefined, { maximumFractionDigits: 0 });
// The backend has no currency field, so amounts are shown as plain numbers.
const amountFmt = new Intl.NumberFormat(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const dateTimeFmt = new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' });

export const formatCount = (n: number) => numberFmt.format(n);
export const formatAmount = (n: number) => amountFmt.format(n);
export const formatDateTime = (value: string) => dateTimeFmt.format(parseUtc(value));

export function fullName(first: string, last: string | null): string {
  return [first, last].filter(Boolean).join(' ');
}

export function initials(first: string, last: string | null): string {
  return ((first[0] ?? '') + (last?.[0] ?? '')).toUpperCase() || '?';
}
