import type {User} from "../entities/User.ts";
import {axiosInstance} from "../app/axiosInstance.ts";

export async function fetchAuthUserData(): Promise<User> {
    const token = localStorage.getItem("token");

    if (!token) {
        throw new Error("No token found");
    }

    try {
        const response = await axiosInstance.get(`/User/me`, {
            headers: {
                Authorization: `Bearer ${token}`,
            },
        });
        return {
            userId: response.data.userId,
            username: response.data.username,
            userPhoto: response.data.userPhoto,
        };
    } catch (error) {
        console.error('Ошибка при выполнении fetchAuthUserData:', error);
        throw error;
    }
}