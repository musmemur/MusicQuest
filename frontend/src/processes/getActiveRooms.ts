import {axiosInstance} from "../app/axiosInstance.ts";
import type {Room} from "../entities/Room.ts";

export async function getActiveRooms(): Promise<Room[]> {
    try {
        const response = await axiosInstance.get(`/api/rooms`);
        return response.data.map((room: Room) => ({
            id: room.id,
            name: room.name,
            genre: room.genre,
            playersCount: room.playersCount
        }));
    } catch (error) {
        console.error('Ошибка при выполнении getActiveRooms:', error);
        throw error;
    }
}