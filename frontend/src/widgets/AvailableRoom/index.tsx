import './index.css';
import React, {useEffect} from "react";
import type {AppDispatch, RootState} from "../../app/store.ts";
import {useDispatch, useSelector} from "react-redux";
import {loadAuthUser} from "../../features/loadAuthUser.ts";

interface AvailableRoomProps {
    roomId: string;
    roomName: string;
    genre: string;
    playersCount: number;
    onJoinRoom: (roomId: string) => Promise<void>;
    isJoining?: boolean;
}

export const AvailableRoom: React.FC<AvailableRoomProps> = ({roomId, roomName, genre, playersCount, onJoinRoom, isJoining = false}) => {
    const handleJoinClick = async () => {
        await onJoinRoom(roomId);
    };

    const dispatch: AppDispatch = useDispatch();
    const authUser = useSelector((state: RootState) => state.loadAuthUser.value);

    useEffect(() => {
        if (!authUser) {
            dispatch(loadAuthUser());
        }
    }, [authUser, dispatch]);

    return (
        <div className="available-room">
            <h2>{roomName}</h2>
            <h2>Жанр песен: {genre}</h2>
            <h3>Количество игроков: {playersCount}</h3>
            <button onClick={handleJoinClick} disabled={isJoining || !authUser}>
                {isJoining ? 'Присоединение...' : 'Присоединиться'}
            </button>
        </div>
    );
};