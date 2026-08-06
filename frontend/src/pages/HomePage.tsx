import { Button, Stack, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { useLanguageStats } from "../features/languagePractice/hooks/useLanguageStats";

export default function HomePage() {
    const navigate = useNavigate();    
    const { data: languageStats } = useLanguageStats();

    return (
        <Stack spacing={5} 
            sx={{
                maxWidth: 600,
                width: "100%",
                textAlign: "center",
            }}>
            {/* TODO: these strings should be internationalized */}
            <Typography variant="h3" component="h1">
                Welcome back {languageStats?.displayName}
            </Typography>
            <Typography variant="body1" color="text.secondary">
                You have practised {languageStats?.uniqueLemmas} unique words in the last {languageStats?.daysPractised} {languageStats?.daysPractised === 1 ? "day" : "days"}.
            </Typography>
            <Button
                variant="contained"
                size="large"
                color="primary"
                sx={{ alignSelf: "center" }}
                onClick={() => navigate("/practice")}
            >
                Start Practicing
            </Button>
        </Stack>
    )
}