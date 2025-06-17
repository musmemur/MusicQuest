import './index.css';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {Header} from "../../widgets/Header";
import {AvailableRoom} from "../../widgets/AvailableRoom";
import {fetchAuthUserData} from "../../processes/fetchAuthUserData.ts";
import type {User} from "../../entities/User.ts";
import {useSignalR} from "../../app/signalRContext.tsx";
import {getActiveRooms} from "../../processes/getActiveRooms.ts";
import type {Room} from "../../entities/Room.ts";
import type {AppDispatch, RootState} from "../../app/store.ts";
import {useDispatch, useSelector} from "react-redux";
import {loadAuthUser} from "../../features/loadAuthUser.ts";

export const HomePage = () => {
    const connection = useSignalR();
    const navigate = useNavigate();
    const [rooms, setRooms] = useState<Room[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [joiningRoomId, setJoiningRoomId] = useState<string | null>(null);
    const dispatch: AppDispatch = useDispatch();
    const authUser = useSelector((state: RootState) => state.loadAuthUser.value);

    useEffect(() => {
        if (!authUser) {
            dispatch(loadAuthUser());
        }
    }, [authUser, dispatch]);

    useEffect(() => {
        const fetchRooms = async () => {
            try {
                const activeRooms = await getActiveRooms();
                setRooms(activeRooms);
            } catch (error) {
                console.error('Ошибка загрузки комнат:', error);
            } finally {
                setIsLoading(false);
            }
        };

        (async () => {
            await fetchRooms();
        })();

        if (connection) {
            connection.on("RoomCreated", (newRoom: Room) => {
                setRooms(prevRooms => [...prevRooms, newRoom]);
            });

            connection.on("RoomClosed", (roomId: string) => {
                setRooms(prevRooms => prevRooms.filter(room => room.id !== roomId));
            });
        }

        return () => {
            if (connection) {
                connection.off("RoomCreated");
                connection.off("RoomClosed");
            }
        };

    }, [connection]);

    const handleJoinRoom = async (roomId: string) => {
        if (!connection) {
            return;
        }

        setJoiningRoomId(roomId);
        try {
            const fetchedUser = await fetchAuthUserData();
            const loggedUser: User = fetchedUser as User;
            await connection.invoke("JoinRoom", roomId, loggedUser.userId);
            navigate(`/waiting-room/${roomId}`);
        } catch (err) {
            console.error("Ошибка присоединения к комнате:", err);
        } finally {
            setJoiningRoomId(null);
        }
    };

    return (
        <div className="home-page">
            <Header />
            <main>
                <div className="home-page-top">
                    <h1>Доступные комнаты</h1>
                    <button onClick={() => navigate("/create-room")} disabled={!authUser}>Создать комнату</button>
                </div>
                <div className="available-rooms">
                    {isLoading ? (
                        <p>Загрузка...</p>
                    ) : rooms.length > 0 ? (
                        rooms.map(room => (
                            <AvailableRoom
                                key={room.id}
                                roomId={room.id}
                                roomName={room.name}
                                genre={room.genre}
                                playersCount={room.playersCount}
                                onJoinRoom={handleJoinRoom}
                                isJoining={joiningRoomId === room.id}
                            />
                        ))
                    ) : (
                        <p>Нет доступных комнат</p>
                    )}
                </div>
            </main>
        </div>
    );
};