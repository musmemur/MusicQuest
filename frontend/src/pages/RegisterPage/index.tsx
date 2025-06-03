import React from "react";
import {getFileData} from "../../shared/utils/getFileData.ts";
import {useNavigate} from "react-router";
import {useFormik} from "formik";
import {registerUser} from "../../processes/registerUser.ts";
import type {UserPhoto} from "../../entities/UserPhoto.ts";
import type {UserRegister} from "../../entities/UserRegister.ts";

interface FormValues {
    username: string;
    password: string;
    photo: File | null;
}

export const RegisterPage = () => {
    const navigate = useNavigate();

    const validate = (values: FormValues) => {
        const errors: { username?: string; password?: string } = {};

        if (!values.username) {
            errors.username = 'Имя пользователя обязательно для заполнения';
        } else if (values.username.length < 5) {
            errors.username = 'Имя пользователя должно состоять минимум из 5 символов';
        } else if (!/[a-zA-Zа-яА-Я]/.test(values.username)) {
            errors.username = 'Имя пользователя должно содержать минимум 1 буквенный символ';
        }

        if (!values.password) {
            errors.password = 'Пароль обязателен для заполнения';
        } else if (values.password.length < 5) {
            errors.password = 'Пароль должен состоять минимум из 5 символов';
        }

        return errors;
    };

    const formik = useFormik<FormValues>({
        initialValues: {
            username: '',
            password: '',
            photo: null as File | null,
        },
        validate,
        onSubmit: async (values) => {
            let userPhoto: UserPhoto | null = null;

            if (values.photo) {
                userPhoto = {
                    fileName: values.photo.name,
                    data: await getFileData(values.photo),
                };
            }

            const user: UserRegister = {
                username: values.username,
                password: values.password,
                userPhoto: userPhoto,
            };

            const response = await registerUser(user);
            localStorage.setItem("token", response.token);
            navigate(`/user/${response.userId}`);
        }
    });

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0] || null;
        formik.setFieldValue("photo", file);
    };

    return (
        <div className="modal">
            <div className="modal-content" onClick={e => e.stopPropagation()}>
                <div className="modal-content-top">
                    <h2>Зарегистрироваться</h2>
                    <button onClick={() => navigate(-1)}>X</button>
                </div>

                <form className="signUp-form" onSubmit={formik.handleSubmit}>
                    <input
                        name="username"
                        type="text"
                        placeholder="Логин"
                        onChange={formik.handleChange}
                        onBlur={formik.handleBlur}
                        value={formik.values.username}
                        autoComplete="new-username"
                        className={formik.errors.username && formik.touched.username ? "error" : ""}
                    />
                    <input
                        name="password"
                        type="password"
                        placeholder="Пароль"
                        onChange={formik.handleChange}
                        onBlur={formik.handleBlur}
                        value={formik.values.password}
                        autoComplete="new-password"
                        className={formik.errors.password && formik.touched.password ? "error" : ""}
                    />
                    <input
                        name="photo"
                        type="file"
                        onChange={handleFileChange}
                    />
                    {formik.errors.username && formik.touched.username && (
                        <div className="signUp-error">{formik.errors.username}</div>
                    )}
                    {formik.errors.password && formik.touched.password && (
                        <div className="signUp-error">{formik.errors.password}</div>
                    )}
                    <button type="submit" disabled={formik.isSubmitting}>
                        {formik.isSubmitting ? "Отправка..." : "Зарегистрироваться"}
                    </button>
                </form>
            </div>
        </div>
    );
};