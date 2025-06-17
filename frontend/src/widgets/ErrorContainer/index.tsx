import './index.css';
import {ErrorIcon} from "../../shared/assets/svg/ErrorIcon.tsx";

export const ErrorContainer = () => {
    return (
        <div className="error-container">
            <ErrorIcon/>
            <div>Пользователь не найден</div>
        </div>
    )
}