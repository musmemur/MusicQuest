import './index.css';
import {Header} from "../../widgets/Header";
import {useEffect, useState} from "react";
import {useNavigate} from "react-router";
import {useSignalR} from "../../app/signalRContext.tsx";
import {DeezerGenres} from "../../entities/DeezerGenres.ts";
import type {CreateRoomDto} from "../../entities/CreateRoomDto.ts";
import {createRoom} from "../../processes/createRoom.ts";
import type {AppDispatch, RootState} from "../../app/store.ts";
import {useDispatch, useSelector} from "react-redux";
import {loadAuthUser} from "../../features/loadAuthUser.ts";

const QUESTION_COUNTS = [3, 4, 5];

export const CreateRoomPage = () => {
    const navigate = useNavigate();
    const connection = useSignalR();
    const [selectedGenre, setSelectedGenre] = useState<number | null>(null);
    const [questionCount, setQuestionCount] = useState<number>(0);
    const [isCreating, setIsCreating] = useState(false);
    const dispatch: AppDispatch = useDispatch();
    const authUser = useSelector((state: RootState) => state.loadAuthUser.value);

    useEffect(() => {
        if (!authUser) {
            dispatch(loadAuthUser());
        }
    }, [authUser, dispatch]);

    const handleCreateRoom = async () => {
        if (!selectedGenre) {
            console.warn('Выберите жанр!');
            return;
        }

        setIsCreating(true);

        try {
            const genreName = DeezerGenres.find(g => g.id === selectedGenre)?.name || '';
            if (authUser) {
                const createRoomDto: CreateRoomDto = {
                    genre: genreName,
                    questionCount,
                    userHostId: authUser.userId
                }
                const createdRoom = await createRoom(createRoomDto);
                if (connection) {
                    navigate(`/waiting-room/${createdRoom}`);
                }
            }
        } catch (error) {
            console.error('Ошибка создания комнаты:', error);
        } finally {
            setIsCreating(false);
        }
    };

    return (
        <>
            <Header />
            <div className="create-room-container">
                <h1>Создать комнату</h1>

                <div className="form-group">
                    <label htmlFor="genre-select">Выберите жанр музыки</label>
                    <select
                        id="genre-select"
                        value={selectedGenre || ''}
                        onChange={(e) => setSelectedGenre(Number(e.target.value))}
                        className="form-control"
                    >
                        <option value="" disabled>-- Выбрать жанр --</option>
                        {DeezerGenres.map(genre => (
                            <option key={genre.id} value={genre.id}>
                                {genre.name}
                            </option>
                        ))}
                    </select>
                </div>

                <div className="form-group">
                    <label>Выберите количество вопросов</label>
                    <div className="question-count-buttons">
                        {QUESTION_COUNTS.map(count => (
                            <button
                                key={count}
                                className={`count-btn ${questionCount === count ? 'active' : ''}`}
                                onClick={() => setQuestionCount(count)}
                            >
                                {count}
                            </button>
                        ))}
                    </div>
                </div>

                <button
                    onClick={handleCreateRoom}
                    disabled={isCreating || !selectedGenre || !questionCount}
                    className="create-btn"
                >
                    {isCreating ? 'Создание...' : 'Создать комнату'}
                </button>
            </div>
        </>
    );
};