import React from 'react';
import { Navigate } from 'react-router-dom';

interface PrivateRouteProps {
    element: React.ComponentType;
}

const PrivateRoute: React.FC<PrivateRouteProps> = ({ element: Component, ...rest }) => {
    const token = localStorage.getItem('token');
    return token ? <Component {...rest} /> : <Navigate to="/login" />;
};

export default PrivateRoute;