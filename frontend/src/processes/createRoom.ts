import {axiosInstance} from "../app/axiosInstance.ts";
import type {CreateRoomDto} from "../entities/CreateRoomDto.ts";

export async function createRoom(createRoomDto: CreateRoomDto): Promise<string> {
    try {
        const token = localStorage.getItem('token');
        const response = await axiosInstance.post(`/api/rooms`, createRoomDto, {
            headers : {
                'Content-Type': 'application/json',
                "Authorization": `Bearer ${token}`,
            }
        });
        return response.data.roomId;
    } catch (error) {
        console.error('Ошибка при выполнении createRoom:', error);
        throw error;
    }
}