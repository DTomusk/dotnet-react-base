import { createContext, useCallback, useEffect, useState, type ReactNode } from "react";
import { clearToken, getToken, setToken } from "../lib/auth/token";
import { registerUnauthorizedHandler } from "../lib/auth/session";
import { useQueryClient } from "@tanstack/react-query";

type AuthContextValue = {
    isAuthenticated: boolean;
    logIn: (token: string) => void;
    logOut: () => void;
};

export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [isAuthenticated, setIsAuthenticated] = useState(() => Boolean(getToken()));
    const queryClient = useQueryClient();

    const logIn = useCallback((token: string) => {
        setToken(token);
        setIsAuthenticated(true);
    }, []);

    const logOut = useCallback(() => {
        clearToken();
        queryClient.clear();
        setIsAuthenticated(false);
    }, []);

    useEffect(() => {
        registerUnauthorizedHandler(logOut);

        return () => {
            registerUnauthorizedHandler(null);
        };
    }, [logOut]);

    return (
        <AuthContext.Provider value={{ isAuthenticated, logIn, logOut }}>
            {children}
        </AuthContext.Provider>
    );
} 