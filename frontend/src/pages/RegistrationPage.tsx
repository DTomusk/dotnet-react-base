import { useNavigate } from "react-router-dom";
import AuthForm from "../features/auth/components/AuthForm";
import { useAuth } from "../features/auth/hooks/useAuth";
import { useRegister } from "../features/auth/hooks/useRegister";
import type { LoginSchema } from "../features/auth/schemas/loginSchema";
import { useEffect } from "react";

export default function RegistrationPage() {
    const navigate = useNavigate();
    const mutation = useRegister();
    const { logIn, isAuthenticated } = useAuth();

    useEffect(() => {
        if (isAuthenticated) {
            navigate("/auth/intro", { replace: true });
        }
    }, [isAuthenticated, navigate]);

    const onSubmit = async (formData: LoginSchema) => {
        const response = await mutation.mutateAsync(formData);
        await logIn(response.token);
        navigate("/auth/intro", { replace: true });
    }

    return (
        <AuthForm mode="register" onSubmit={onSubmit} />
    )
}