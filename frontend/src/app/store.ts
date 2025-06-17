import {configureStore, type ThunkDispatch, type UnknownAction} from '@reduxjs/toolkit';
import loadAuthUserReducer from '../features/loadAuthUser.ts';

const store = configureStore({
    reducer: {
        loadAuthUser: loadAuthUserReducer,
    }
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = ThunkDispatch<RootState, unknown, UnknownAction>;

export default store;