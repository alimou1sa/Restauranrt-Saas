// Lets the HTTP layer tell the auth layer that the session ended, without importing React code.
export type SessionEndReason = 'expired' | 'preauth-expired';
type Listener = (reason: SessionEndReason) => void;

const listeners = new Set<Listener>();

export const authEvents = {
  subscribe(l: Listener): () => void {
    listeners.add(l);
    return () => listeners.delete(l);
  },
  emit(reason: SessionEndReason): void {
    listeners.forEach((l) => l(reason));
  },
};
