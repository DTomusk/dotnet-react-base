import AuthLayout from '../layout/AuthLayout'
import LoginPage from '../pages/LoginPage'
import RegistrationPage from '../pages/RegistrationPage'
import HomePage from '../pages/HomePage'
import AppLayout from '../layout/AppLayout'
import { createBrowserRouter } from 'react-router-dom'
import ProtectedLayout from './ProtectedLayout'

export const router = createBrowserRouter([
    {
        path: '/',
        element: <AppLayout />,
        children: [
            {
                element: <ProtectedLayout />,
                children: [
                    {
                        index: true,
                        element: <HomePage />
                    },
                ]
            }
        ]
    },
    {
        path: '/auth',
        element: <AuthLayout />,
        children: [
            {
                path: 'login',
                element: <LoginPage />
            },
            {
                path: 'register',
                element: <RegistrationPage />
            },
        ]
    }
])