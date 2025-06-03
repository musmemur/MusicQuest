import './index.css';
import React from "react";

interface AvailableRoomProps {
    roomId: string;
    roomName: string;
    genre: string;
    playersCount: number;
    onJoinRoom: (roomId: string) => Promise<void>;
    isJoining?: boolean;
}

export const AvailableRoom: React.FC<AvailableRoomProps> = ({
                                  roomId,
                                  roomName,
                                  genre,
                                  playersCount,
                                  onJoinRoom,
                                  isJoining = false
                              }) => {
    const handleJoinClick = async () => {
        await onJoinRoom(roomId);
    };

    return (
        <div className="available-room">
            <h2>Название комнаты: {roomName}</h2>
            <h2>Жанр песен: {genre}</h2>
            <h3>Количество игроков: {playersCount}</h3>
            <button onClick={handleJoinClick} disabled={isJoining}>
                {isJoining ? 'Присоединение...' : 'Присоединиться'}
            </button>
        </div>
    );
};