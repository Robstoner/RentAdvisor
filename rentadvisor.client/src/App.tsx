import './App.css';
import './index.css';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import { useEffect, useState } from 'react';
import axios from './api/axiosConfig';
import Navbar from './components/Navbar';
import Profile from './pages/Profile';
import CreateProperty from './pages/CreateProperty';
import PropertyDetails from './pages/PropertyDetails'
import PropertyEdit from './pages/PropertyEdit'

import AuthLayout from './layout/AuthLayout';
import MainLayout from './layout/MainLayout';

export interface User {
    id: string;
    userName: string;
    name: string;
    email: string;
    score: number;
    roles?: string[]; 
    createdAt: string;
    lastLogin: string;
}

export type Review = {
    id: string;
    title: string;
    description: string;
    propertyId: string;
    userId: string;
    createdAt: string;
    updatedAt: string;
}

export type Photo = {
    id: string;
    photoPath: string;
    propertyId: string;
    userId: string;
    addedAt: string;
};

export type Property = {
    id: string;
    name: string;
    address: string;
    description: string;
    features: string[];
    photos?: Photo[];
    reviews: Review[];
    createdAt: string;
    updatedAt: string;
};


function App() {
    
    const [user, setUser] = useState<User | null>(null);

    useEffect(() => {
        fetchUser();
    }, []);

    const fetchUser = async () => {
        try {
            const response = await axios.get('/user/current');
            const userData = response.data;
            const mappedUser: User = {
                id: userData.id,
                userName: userData.userName,
                name: userData.name,
                email: userData.email,
                score: userData.score,
                roles: [], // Assuming roles are not provided in the response
                createdAt: userData.createdAt,
                lastLogin: userData.lastLogin
            };
    
            setUser(mappedUser);
            localStorage.setItem('userId', userData.id);
        } catch (error) {
            console.error('Error fetching user', error);
        }
    };

    return (
        <BrowserRouter>
            <Navbar user={user} setUser={setUser} />
            <Routes>
                <Route path="/" element={<MainLayout ><Home /></MainLayout>} />
                <Route path="/login" element={<AuthLayout><Login setUser={setUser}/></AuthLayout>} />
                <Route path="/register" element={<AuthLayout><Register /></AuthLayout>} />
                <Route path="/profile/:userId" element={<MainLayout><Profile /></MainLayout>} />
                <Route path="/createProperty" element={<MainLayout><CreateProperty /></MainLayout>} />
                <Route path="/property/:propertyId" element={<MainLayout><PropertyDetails /></MainLayout>} />
                <Route path="/propertyEdit/:propertyId" element={<MainLayout><PropertyEdit /></MainLayout>} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;
