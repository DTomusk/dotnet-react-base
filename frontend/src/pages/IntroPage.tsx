import LanguageSelector from "../features/languagePractice/components/LanguageSelector";
import { useLanguageSelection } from "../features/languagePractice/hooks/useLanguageSelection";
import Spinner from "../components/Spinner";
import Alert from "@mui/material/Alert";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { useEffect } from "react";

export default function IntroPage() {
    const navigate = useNavigate();
    const { t } = useTranslation(["common"]);
    const {
        activeLanguage,
        error,
        languageItems,
        selectedLanguage,
        setSelectedLanguage,
        confirmLanguage,
        isLoading,
        isLoadingLanguages,
        isSubmitting,
    } = useLanguageSelection();

    useEffect(() => {
        if (!isLoading && activeLanguage) {
            navigate("/");
        }
    }, [isLoading, activeLanguage, navigate]);

    const onConfirmLanguage = async () => {
        await confirmLanguage();
        navigate("/");
    }

    if (isLoading) {
        return <Spinner aria-label={t("common:loading")} />;
    }

    if (error) {
        return <Alert severity="error">{t("common:error")}: {error.message}</Alert>;
    }

    return (
        <LanguageSelector
            items={languageItems}
            isLoading={isLoadingLanguages}
            isSubmitting={isSubmitting}
            selectedLanguage={selectedLanguage}
            onLanguageChange={setSelectedLanguage}
            onConfirm={onConfirmLanguage}
        />
    );
}