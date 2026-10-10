import { useCallback, useEffect, useState, type DependencyList } from 'react';

interface AsyncState<T> { data: T | null; error: unknown; loading: boolean }

/** Small data-loading hook: loading / error / data + reload. `enabled=false` skips the request. */
export function useAsync<T>(fn: () => Promise<T>, deps: DependencyList, enabled = true) {
  const [state, setState] = useState<AsyncState<T>>({ data: null, error: null, loading: enabled });
  const [tick, setTick] = useState(0);

  useEffect(() => {
    if (!enabled) {
      setState({ data: null, error: null, loading: false });
      return;
    }
    let cancelled = false;
    setState((s) => ({ ...s, loading: true, error: null }));
    fn().then(
      (data) => { if (!cancelled) setState({ data, error: null, loading: false }); },
      (error: unknown) => { if (!cancelled) setState({ data: null, error, loading: false }); },
    );
    return () => { cancelled = true; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, enabled, tick]);

  const reload = useCallback(() => setTick((t) => t + 1), []);
  // Enabled but nothing arrived yet (e.g. the render before the effect starts): report loading, not empty.
  const loading = state.loading || (enabled && state.data === null && state.error === null);
  return { ...state, loading, reload };
}
