import { Link } from 'react-router-dom';

export function RegisterPage(){
 return <div className="center-screen"><section className="card narrow registration-notice">
  <div className="brand brand-dark"><span className="brand-mark">R</span><span>RestaurantSaaS</span></div>
  <span className="platform-eyebrow">GET STARTED</span><h1 className="h2">Create your workspace</h1>
  <p className="muted">Public self-service registration is not enabled by the current API. To keep account creation and organization ownership secure, a platform administrator must provision the workspace first.</p>
  <div className="admin-notice-list"><div><strong>Already have an account?</strong><span>Sign in with your work email.</span></div><div><strong>Starting a new restaurant workspace?</strong><span>Contact the platform administrator to set up your organization and first account.</span></div></div>
  <Link className="btn btn-primary btn-block" to="/login">Back to sign in</Link>
 </section></div>;
}
