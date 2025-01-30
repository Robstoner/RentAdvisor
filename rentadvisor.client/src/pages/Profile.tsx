import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import '../css/Profile.css';
import axios from '../api/axiosConfig';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faUser } from '@fortawesome/free-solid-svg-icons';

import { User } from '../App.tsx';

const Profile: React.FC = () => {
    const { userId } = useParams<{ userId: string }>();
    const [user, setUser] = useState<User | null>(null);
    const [currentUser, setCurrentUser] = useState<User | null>(null);
    const [availableRoles, setAvailableRoles] = useState<string[]>([]);
    const [selectedRole, setSelectedRole] = useState<string>('');
    const [loading, setLoading] = useState(true);

    const fetchUser = async () => {
        try {
            const response = await axios.get(`/user/${userId}`);
            setUser(response.data);
        } catch (error) {
            console.error('Error fetching user:', error);
        } finally {
            setLoading(false);
        }
    };

    const fetchCurrentUser = async () => {
        try {
            const response = await axios.get('user/current');
            setCurrentUser(response.data);
        } catch (error) {
            console.error('Error fetching current user:', error);
        }
    };

    const fetchAvailableRoles = async () => {
        try {
            const response = await axios.get('/roles');
            const rolesString = response.data.map((role: any) => role.name);
            setAvailableRoles(rolesString);
        } catch (error) {
            console.error('Error fetching roles:', error);
        }
    };

    useEffect(() => {

        if (userId) {
            fetchUser();
            fetchCurrentUser();
            fetchAvailableRoles();
        }
    }, [userId]);

    const handleRemoveRole = async (role: string) => {
        if (!user) return;

        try {
            const response = await axios.delete(`/user/${userId}/role`, {
                headers: { 'Content-Type': 'application/json-patch+json' },
                data: JSON.stringify(role),
            });
            setUser(response.data);
            fetchUser();
        } catch (error) {
            console.error('Error deleting role:', error);
        }
    };

    const handleAddRole = async (role: string) => {
        if (!user || !selectedRole) return;

        try {
            const response = await axios.post(`/user/${userId}/role`,
                JSON.stringify(role),
                { headers: { 'Content-Type': 'application/json-patch+json' } }
            );
            setUser(response.data);
            fetchUser();
        } catch (error) {
            console.error('Error adding role:', error);
        }
    };


    if (loading) {
        return (
            <div className="main-container">
                <p>Loading...</p>
            </div>
        );
    }

    if (!user) {
        return (
            <div className="main-container">
                <h1>User Profile</h1>
                <p>You need to be logged in to view this page.</p>
                <a href='/login'>Login</a>
            </div>
        );
    }

    const isAdmin = currentUser?.roles.includes('Admin');

    return (

        <div className="main-container">
            <h2>User Profile</h2>
            <div className="profile-details">
                <div className='profile-user'>
                    <FontAwesomeIcon icon={faUser} className="profile-icon" />
                    <p className='profile-username'>{user.name}</p>
                </div>
                <p className='profile-score'>Score: {user.score}</p>
                <p className='role-title'>Roles:</p>
                <div className='role-list'>
                    {user.roles.map(role => (
                        <li key={role}>
                            {role} {isAdmin && <button className='role-button' onClick={() => handleRemoveRole(role)}>Remove</button>}
                        </li>
                    ))}
                </div>
            </div>

            {isAdmin && (
                <div className="role-management">
                    <h3>Manage Roles</h3>
                    <div className='role-select-container'>
                        <select className='role-select' value={selectedRole} onChange={e => setSelectedRole(e.target.value)}>
                            <option className='role-option' value="">Select a role</option>
                            {availableRoles.map(role => (
                                <option className='role-option' key={role} value={role}>{role}</option>
                            ))}
                        </select>
                        <button className='role-button' onClick={() => handleAddRole(selectedRole)}>Add Role</button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Profile;