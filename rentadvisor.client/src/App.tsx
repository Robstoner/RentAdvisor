import './App.css';
import './index.css';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import { useEffect, useState } from 'react';
import axios from './api/axiosConfig';
import Navbar from './components/Navbar';
// import PrivateRoute from './components/PrivateRoute';

import AuthLayout from './layout/AuthLayout';
import MainLayout from './layout/MainLayout';
export interface User {
    id: string;
    name: string;
    roles: string[];
}

function App() {
    
    const [user, setUser] = useState<User | null>(null);

    useEffect(() => {
        fetchUser();
    }, []);

    const fetchUser = async () => {
        try {
            const response = await axios.get('/user/current');
            setUser(response.data);
            console.log("sadas", response)
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
            </Routes>
        </BrowserRouter>
    );
}

export default App;