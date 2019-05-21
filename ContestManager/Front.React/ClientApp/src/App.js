import React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import Home from './components/ContestList';
import Login from './components/Login';
import UserPage from './components/UserPage';
import Register from './components/Register';

export default () => (
    <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/login' component={Login} />
        <Route path='/register' component={Register} />
        <Route path='/user' component={UserPage} />
    </Layout>
);
