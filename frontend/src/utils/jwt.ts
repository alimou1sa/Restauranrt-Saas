// Reads only the `exp` claim to decide whether to refresh. This is NOT validation - the backend validates.
export function getTokenExpiryMs(token: string): number | null {
  try {
    const payload = token.split('.')[1];
    const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    const exp = (JSON.parse(json) as { exp?: number }).exp;
    return typeof exp === 'number' ? exp * 1000 : null;
  } catch {
    return null;
  }
}

export function isTokenExpired(token: string, skewMs = 15_000): boolean {
  const exp = getTokenExpiryMs(token);
  return exp === null || exp - skewMs <= Date.now();
}
