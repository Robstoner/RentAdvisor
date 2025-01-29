// axiosConfig.ts
import axios from 'axios';

const target = import.meta.env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${import.meta.env.ASPNETCORE_HTTPS_PORT}` :
    import.meta.env.ASPNETCORE_URLS ? import.meta.env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:8080';

// Make targer backend:8080 if NODE_ENV is production
//const target = __NODE_ENV__ === "Production" ? 'backend:8080' : 'http:localhost:8080';

const axiosInstance = axios.create({
    baseURL: target,
});

axiosInstance.interceptors.request.use(
    config => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    error => {
        return Promise.reject(error);
    }
);

axiosInstance.interceptors.response.use(
    response => response,
    async error => {
        const originalRequest = error.config;
        if (error.response.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;
            try {
                const refreshToken = localStorage.getItem('refreshToken');
                if (refreshToken) {
                    const response = await axiosInstance.post('/refresh', { token: refreshToken });
                    localStorage.setItem('token', response.data.accessToken);
                    originalRequest.headers.Authorization = `Bearer ${response.data.accessToken}`;
                    return axiosInstance(originalRequest);
                }
            } catch (refreshError) {
                console.error('Error refreshing token', refreshError);
                localStorage.removeItem('token');
                localStorage.removeItem('refreshToken');
                window.location.href = '/login';
            }
        }
        return Promise.reject(error);
    }
);

export default axiosInstance;