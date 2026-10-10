import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <div className="card narrow">
      <h1 className="h2">Page not found</h1>
      <p className="muted">The page you are looking for does not exist.</p>
      <Link to="/dashboard" className="btn btn-primary">Back to dashboard</Link>
    </div>
  );
}
