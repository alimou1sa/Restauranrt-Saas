import type { ReactNode } from 'react';
import { useAuth } from './AuthContext';

interface CanProps {
  permission?: string;
  anyOf?: readonly string[];
  fallback?: ReactNode;
  children: ReactNode;
}

/** Hides UI the user has no permission for. UX only - the API enforces the real rule. */
export function Can({ permission, anyOf, fallback = null, children }: CanProps) {
  const { hasPermission, hasAnyPermission } = useAuth();
  const allowed = permission ? hasPermission(permission) : anyOf ? hasAnyPermission(anyOf) : true;
  return <>{allowed ? children : fallback}</>;
}
