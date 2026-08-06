import { AppBar, Button, Toolbar } from "@mui/material";
import { useAuth } from "../features/auth/hooks/useAuth";
import { useNavigate } from "react-router-dom";

export default function NavBar() {
    const { isAuthenticated, logOut } = useAuth();
    const navigate = useNavigate();

    return (
        <AppBar position="sticky" sx={{ top: 0 }} color="secondary">
            <Toolbar>
                {isAuthenticated ? (
                    <>
                        <Button color="inherit" onClick={logOut}>Logout</Button>
                    </>
                ) : (
                    <Button onClick={() => navigate("auth/login", { replace: true })}>Login</Button>
                )}
            </Toolbar>
        </AppBar>
    );
}