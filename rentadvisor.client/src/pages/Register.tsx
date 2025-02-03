// Register.tsx
import React, { useState } from 'react';
import axios from '../api/axiosConfig';
import { useNavigate } from 'react-router-dom';
import '../css/Auth.css';

const Register = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();
        if (password !== confirmPassword) {
            setError('Passwords do not match');
            return;
        }
        if (password.length < 8) {
            setError('Password must be at least 8 characters long');
            return;
        }
        if (!/\d/.test(password)) {
            setError('Password must contain at least one digit');
            return;
        }
        if (!/[A-Z]/.test(password)) {
            setError('Password must contain at least one uppercase letter');
            return;
        }
        if (!/\W/.test(password)) {
            setError('Password must contain at least one non-alphanumeric character');
            return;
        }

        try {
            const response = await axios.post('api/register', { email, password });
            localStorage.setItem('token', response.data.accessToken);
            localStorage.setItem('refreshToken', response.data.refreshToken);
            navigate('/login');
        } catch (err) {
            setError('Registration failed');
        }
    };

    return (
        <div className='container'>
            <div className='auth-container'>
                <div className='title-container'>
                    <h2>Register</h2>
                </div>
                <div className='form-container'>
                    <form onSubmit={handleSubmit}>
                        <div className='fields-form'>
                            <label>Email</label>
                            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
                        </div>
                        <div className='fields-form'>
                            <label>Password</label>
                            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
                        </div>
                        <div className='fields-form'>
                            <label>Confirm Password</label>
                            <input type="password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} required />
                        </div>
                        <div className='fields-form'>
                            {error && <p style={{ color: 'red' }}>{error}</p>}
                            <button type="submit">Register</button>
                        </div>
                    </form>
                </div>
                <div className='footer-container'>
                    <p>Already have an account? <a href="/login">Login</a></p>
                </div>
            </div>
        </div>
    );
};

export default Register;  