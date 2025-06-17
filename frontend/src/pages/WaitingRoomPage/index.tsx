import './index.css';
import './adaptive.css';
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from "react-router-dom";
import { Header } from "../../widgets/Header";
import type {User} from "../../entities/User.ts";
import {fetchAuthUserData} from "../../processes/fetchAuthUserData.ts";
import {useSignalR} from "../../app/signalRContext.tsx";
import photoPlaceholder from '../../shared/assets/photo-placeholder.png'
import type {Player} from "../../entities/Player.ts";

export const WaitingRoomPage = () => {
    const connection = useSignalR();
    const { roomId } = useParams();
    const [players, setPlayers] = useState<Player[]>([]);
    const [isGameStarting, setIsGameStarting] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        if (!roomId) return;

        const setupSignalREvents = async () => {
            try {
                const fetchedUser = await fetchAuthUserData();
                const loggedUser: User = fetchedUser as User;

                if (!connection) return;

                connection.on("PlayerJoined", (player: Player) => {
                    setPlayers(prev => {
                        const playerExists = prev.some(p => p.userId === player.userId);
                        return playerExists ? prev : [...prev, player];
                    });
                });

                connection.on("PlayerLeft", (userId: string) => {
                    setPlayers(prev => prev.filter(p => p.userId !== userId));
                });

                connection.on("ReceivePlayersList", (playersList: Player[]) => {
                    setPlayers(playersList);
                });

                connection.on("GameStarted", (gameId: string) => {
                    setIsGameStarting(true);
                    navigate(`/game/${gameId}`);
                });

                connection.on("Error", (errorMessage: string) => {
                    console.error(errorMessage);
                    navigate('/home')
                });

                await connection.invoke("JoinRoom", roomId, loggedUser.userId);

            } catch (err) {
                console.error("Ошибка аутентификации:", err);
            }
        };

        (async () => {
            await setupSignalREvents();
        })();

        return () => {
            if (!connection) return;

            connection.off("PlayerJoined");
            connection.off("PlayerLeft");
            connection.off("ReceivePlayersList");
            connection.off("GameStarted");
            connection.off("Error");
        };
    }, [roomId, connection, navigate]);

    const startGame = async () => {
        if (!connection || !roomId) return;

        try {
            setIsGameStarting(true);
            await connection.invoke("StartGame", roomId);
        } catch (error) {
            console.error("Ошибка при запуске игры:", error);
            setIsGameStarting(false);
        }
    };

    if (!roomId) {
        return <div>Неверный ID комнаты</div>;
    }

    return (
        <>
            <Header />
            <div className="waiting-room-content">
                <h2>Ожидание игроков</h2>
                <h3><strong>Комната</strong> #{roomId}</h3>

                <div className="players-list">
                    <span><strong>Игроков в комнате ({players.length}):</strong></span>
                    <ul>
                        {players.map((player) => (
                            <li key={player.userId}>
                                <img
                                    src={player.userPhoto || photoPlaceholder}
                                    alt={player.username}
                                    width={40}
                                    height={40}
                                />
                                {player.username}
                            </li>
                        ))}
                    </ul>
                </div>

                <button
                    onClick={startGame}
                    disabled={players.length < 2 || isGameStarting || !connection}
                    className="start-game-btn"
                >
                    {isGameStarting ? 'Запуск игры...' : 'Начать игру'}
                </button>
            </div>
        </>
    );
};