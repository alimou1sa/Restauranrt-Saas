import { useState, type FormEvent } from 'react';
import { useAuth } from '../auth/AuthContext';
import { Spinner } from '../components/Feedback';
import { getErrorMessage } from '../utils/errors';

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function LoginPage() {
  const { login, notice } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [touched, setTouched] = useState({ email: false, password: false });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const emailError = !email.trim() ? 'Email is required.' : !EMAIL_RE.test(email.trim()) ? 'Enter a valid email address.' : null;
  const passwordError = !password ? 'Password is required.' : null;

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setTouched({ email: true, password: true });
    setError(null);
    if (emailError || passwordError) return;

    setSubmitting(true);
    try {
      await login(email.trim(), password);
      // AuthContext moves to "preauth"; the route guard then redirects to organization selection.
    } catch (err) {
      setError(getErrorMessage(err, 'Sign in failed.'));
      setSubmitting(false);
    }
  }

  return (
    <div className="center-screen">
      <div className="card narrow">
        <div className="brand brand-dark">
          <span className="brand-mark" aria-hidden="true">R</span>
          <span>RestaurantSaaS</span>
        </div>
        <h1 className="h2">Sign in</h1>
        <p className="muted">Use your work email to access your restaurant workspace.</p>

        {notice && <div className="banner banner-info" role="status">{notice}</div>}
        {error && <div className="banner banner-error" role="alert">{error}</div>}

        <form onSubmit={handleSubmit} noValidate>
          <div className="field">
            <label htmlFor="email">Email</label>
            <input id="email" type="email" autoComplete="username" value={email}
              onChange={(e) => setEmail(e.target.value)} onBlur={() => setTouched((t) => ({ ...t, email: true }))}
              aria-invalid={touched.email && !!emailError} aria-describedby="email-err" />
            <p id="email-err" className="field-error">{touched.email ? emailError : null}</p>
          </div>
          <div className="field">
            <label htmlFor="password">Password</label>
            <input id="password" type="password" autoComplete="current-password" value={password}
              onChange={(e) => setPassword(e.target.value)} onBlur={() => setTouched((t) => ({ ...t, password: true }))}
              aria-invalid={touched.password && !!passwordError} aria-describedby="password-err" />
            <p id="password-err" className="field-error">{touched.password ? passwordError : null}</p>
          </div>
          <button type="submit" className="btn btn-primary btn-block" disabled={submitting}>
            {submitting ? <Spinner label="Signing in" /> : null}
            {submitting ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
      </div>
    </div>
  );
}
