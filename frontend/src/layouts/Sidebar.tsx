import { NavLink } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { Icon } from '../components/Icon';
import { NAVIGATION } from '../config/navigation';

export function Sidebar({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { hasPermission } = useAuth();

  const groups = NAVIGATION.map((g) => ({
    ...g,
    items: g.items.filter((i) => !i.permission || hasPermission(i.permission)),
  })).filter((g) => g.items.length > 0);

  return (
    <>
      <div className={`scrim${open ? ' scrim-open' : ''}`} onClick={onClose} aria-hidden="true" />
      <aside className={`sidebar${open ? ' sidebar-open' : ''}`} aria-label="Main navigation">
        <div className="brand">
          <span className="brand-mark" aria-hidden="true">R</span>
          <span>RestaurantSaaS</span>
          <button type="button" className="icon-btn sidebar-close" onClick={onClose} aria-label="Close menu">
            <Icon name="close" />
          </button>
        </div>
        <nav>
          {groups.map((group) => (
            <div key={group.label} className="nav-group">
              <p className="nav-label">{group.label}</p>
              <ul>
                {group.items.map((item) => (
                  <li key={item.key}>
                    {item.implemented ? (
                      <NavLink to={item.path} className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}>
                        <Icon name={item.icon} />
                        <span>{item.label}</span>
                      </NavLink>
                    ) : (
                      <span className="nav-item disabled" aria-disabled="true">
                        <Icon name={item.icon} />
                        <span>{item.label}</span>
                        <span className="soon">Soon</span>
                      </span>
                    )}
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </nav>
      </aside>
    </>
  );
}
