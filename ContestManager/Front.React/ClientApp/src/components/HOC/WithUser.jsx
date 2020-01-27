import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { actionCreators } from '../../store/User';
import { getDisplayName } from '../../utils';

export default function WithUser(wrappedComponent) {
    return connect(
        state => ({ ...state.user, fetchingUser: state.user.fetching }),
        dispatch => bindActionCreators(actionCreators, dispatch)
    )(wrappedComponent);
}
