import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faUser } from '@fortawesome/free-solid-svg-icons';
import { faFaceKissBeam } from '@fortawesome/free-solid-svg-icons';
import '../css/Navbar.css';
import axios from '../api/axiosConfig';
import { faCircleUser } from '@fortawesome/free-solid-svg-icons';


import { User } from '../App.tsx';

interface NavbarProps {
  user: User | null;
  setUser: (user: User | null) => void;
}

const Navbar: React.FC<NavbarProps> = ({ user, setUser }) => {
  const navigate = useNavigate();
  const [dropdownVisible, setDropdownVisible] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setDropdownVisible(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  const handleLogoClick = () => {
    navigate('/');
  };

  const handleUserClick = () => {
    setDropdownVisible(!dropdownVisible);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    setUser(null);
    navigate('/login');
  };

  const handleProfileClick = () => {
    if (user)
      navigate(`/profile/${user.id}`);
    setDropdownVisible(false);
  };

  useEffect(() => {
    const fetchUser = async () => {
      const token = localStorage.getItem('token');
      if (token) {
        try {
          const response = await axios.get('/user/current', {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          });
          setUser(response.data);
        } catch (error) {
          console.error('Error fetching user:', error);
          localStorage.removeItem('token');
          setUser(null);
        }
      }
    };

    if (!user) {
      fetchUser();
    }
  }, [user, setUser]);

  return (
    <div className="navbar">
      <div className="navbar-logo" onClick={handleLogoClick}>
        <FontAwesomeIcon icon={faFaceKissBeam} className="icon" />
        <p>Homepage</p>
      </div>
      <div className="navbar-user" onClick={handleUserClick} ref={dropdownRef}>
        <FontAwesomeIcon icon={faUser} className="icon" />
        {dropdownVisible && (
          <div >
            {user ? (
              <div className="dropdown-menu">
                <div className='dropdown-item' onClick={handleProfileClick}>
                  <p className='dropdown-icon-user'><FontAwesomeIcon icon={faCircleUser} /></p>
                  <div className='dropdown-user'>
                    <p className=''>{user.name}</p>
                    <p className='dropdown-role'>{(user.roles && user.roles.length > 0) ? user.roles.join(', ') : 'User'}</p>
                  </div>
                </div>
                <div className='dropdown-item'>
                  <p>Score: {user.score}</p>
                </div>
                  <p className="dropdown-item" onClick={handleLogout}>Logout</p>
              </div>
            ) : (
              <div className="dropdown-menu">
                <div className="dropdown-item" onClick={() => navigate('/login')}>Login</div>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default Navbar;