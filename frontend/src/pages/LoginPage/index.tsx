import {type FC, useState} from "react";
import {useNavigate} from "react-router";
import {useFormik} from "formik";
import {loginUser} from "../../processes/loginUser.ts";
import type {UserLogin} from "../../entities/UserLogin.ts";

export const LoginPage: FC = () => {
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const formik = useFormik({
        initialValues: {
            login: '',
            password: '',
        },
        onSubmit: async (values) => {
            const user: UserLogin = {
                username: values.login,
                password: values.password,
            };

            try {
                const authData = await loginUser(user);
                localStorage.setItem("token", authData.token);
                navigate(`/user/${authData.userId}`);
            } catch (error) {
                if (error instanceof Error) {
                    setError(error.message);
                } else {
                    setError("Произошла неизвестная ошибка");
                }
            }
        }
    });

    return(
        <div>
            <div className="modal">
                <div className="modal-content" onClick={e => e.stopPropagation()}>
                    <div className="modal-content-top">
                        <h2>Войти</h2>
                        <button onClick={() => navigate(-1)}>X</button>
                    </div>
                    <form className="signUp-form" onSubmit={formik.handleSubmit}>
                        <input
                            name="login"
                            type="text"
                            placeholder="Логин"
                            autoComplete="username"
                            onChange={formik.handleChange}
                            value={formik.values.login}
                        />
                        <input
                            name="password"
                            type="password"
                            placeholder="Пароль"
                            autoComplete="current-password"
                            onChange={formik.handleChange}
                            value={formik.values.password}
                        />
                        {error && (
                            <span className="signUp-error">{error}</span>
                        )}
                        <button type="submit" disabled={formik.isSubmitting}>
                            {formik.isSubmitting ? "Отправка..." : "Войти"}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    )
}