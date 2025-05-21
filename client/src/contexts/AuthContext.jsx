import React, { createContext, useContext, useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

const AuthContext = createContext();

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};

export const AuthProvider = ({ children }) => {
    const navigate = useNavigate();
    const [user, setUser] = useState(null);
    const [error, setError] = useState(null);
    const [authLoading, setAuthLoading] = useState(true);

    useEffect(() => {
        setAuthLoading(true);
        const token = localStorage.getItem('token');
        const username = localStorage.getItem('username');
        if (token && username) {
            setUser({ username });
        } else {
            setUser(null);
        }
        setAuthLoading(false);
    }, []);

    const login = async (username, password) => {
        setAuthLoading(true);
        try {
            const response = await fetch('http://localhost:5183/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ username, password }),
            });

            if (!response.ok) {
                const errorData = await response.text();
                throw new Error(errorData || 'Неверное имя пользователя или пароль');
            }

            const data = await response.json();
            localStorage.setItem('token', data.token);
            localStorage.setItem('username', username);
            setUser({ username });
            setError(null);
            setAuthLoading(false);
            navigate('/home');
        } catch (err) {
            setError(err.message);
            setAuthLoading(false);
            throw err;
        }
    };

    const register = async (username, password) => {
        setAuthLoading(true);
        try {
            const response = await fetch('http://localhost:5183/api/auth/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ username, password }),
            });

            if (!response.ok) {
                const errorData = await response.text();
                throw new Error(errorData || 'Ошибка при регистрации');
            }

            const data = await response.json();
            localStorage.setItem('token', data.token);
            localStorage.setItem('username', username);
            setUser({ username });
            setError(null);
            setAuthLoading(false);
            navigate('/home');
        } catch (err) {
            setError(err.message);
            setAuthLoading(false);
            throw err;
        }
    };

    const logout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('username');
        setUser(null);
        setError(null);
        navigate('/auth');
    };

    const clearAuthError = () => {
        setError(null);
    };

    const value = {
        user,
        error,
        authLoading,
        login,
        register,
        logout,
        clearAuthError
    };

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
}; 