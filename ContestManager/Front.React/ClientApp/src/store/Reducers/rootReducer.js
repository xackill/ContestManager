import { connectRouter } from "connected-react-router";
import { combineReducers } from 'redux';
import contests from './Contest';
import error from './Error';
import news from './News';
import participants from './Participants';
import results from './Results';
import user from './User';

export function createRootReducer(history) {
    const reducers = {
        error,
        user,
        contests,
        news,
        results,
        participants
    };

    return combineReducers({
        router: connectRouter(history),
        ...reducers
    });
}

