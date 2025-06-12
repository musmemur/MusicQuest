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

export const HomePage = () => {
    const connection = useSignalR();
    const navigate = useNavigate();
    const [rooms, setRooms] = useState<Room[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [joiningRoomId, setJoiningRoomId] = useState<string | null>(null);

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

        const setupSignalREvents = () => {
            if (!connection) return;

            const handlers = {
                RoomUpdated: (updatedRoom: Room) => {
                    setRooms(prev => prev.map(room =>
                        room.id === updatedRoom.id ? updatedRoom : room
                    ));
                },
                PlayerCountChanged: (roomId: string, newCount: number) => {
                    setRooms(prev => prev.map(room =>
                        room.id === roomId ? {...room, playersCount: newCount} : room
                    ));
                },
                RoomCreated: (newRoom: Room) => {
                    setRooms(prev => [...prev, newRoom]);
                },
                RoomClosed: (roomId: string) => {
                    setRooms(prev => prev.filter(room => room.id !== roomId));
                }
            };

            Object.entries(handlers).forEach(([event, handler]) => {
                connection.on(event, handler);
            });

            return () => {
                Object.entries(handlers).forEach(([event, handler]) => {
                    connection.off(event, handler);
                });
            };
        };

        fetchRooms();
        const cleanup = setupSignalREvents();

        return () => {
            cleanup?.();
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
                    <button onClick={() => navigate("/create-room")}>Создать комнату</button>
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