import {axiosInstance} from "../app/axiosInstance.ts";
import axios from "axios";
import type {UserLogin} from "../entities/UserLogin.ts";
import type {AuthResponse} from "../entities/AuthResponse.ts";

export async function loginUser(user: UserLogin): Promise<AuthResponse> {
    try {
        const response = await axiosInstance.post("/User/login", user, {
            headers: { "Content-Type": "application/json" }
        });

        return {
            token: response.data.token,
            userId: response.data.userId,
            username: response.data.username,
            userPhoto: response.data.userPhoto
        };
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const errorMessage = error.response?.data?.message || error.response?.data || "Произошла ошибка авторизации";
            throw new Error(typeof errorMessage === 'string' ? errorMessage : JSON.stringify(errorMessage));
        } else {
            console.error("Неожиданная ошибка:", error);
            throw new Error("Произошла неизвестная ошибка");
        }
    }
}