import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { actionCreators } from '../../store/User';

export default function withUser(wrappedComponent) {
    return connect(
        state => state.user,
        dispatch => bindActionCreators(actionCreators, dispatch)
    )(wrappedComponent);
}