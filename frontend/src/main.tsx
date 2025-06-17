import {BrowserRouter} from "react-router";
import { createRoot } from 'react-dom/client'
import App from './app/App.tsx';
import store from "./app/store.ts";
import {Provider} from "react-redux";

createRoot(document.getElementById('root')!).render(
    <Provider store={store}>
        <BrowserRouter>
            <App />
        </BrowserRouter>
    </Provider>
)
