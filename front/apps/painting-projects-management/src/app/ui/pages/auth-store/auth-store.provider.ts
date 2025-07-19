import { InjectionToken } from '@angular/core';
import { SmzStateStoreBuilder, withAutoRefresh, SmzStore } from '@smz-ui/store';
import { AuthApiService, AuthState } from './auth.api';
import { User } from './user';

interface AuthActions {
  clearAuth(): void;
  updateToken(token: string): void;
  updateUser(user: User): void;
}

interface AuthSelectors {
  isAuthenticated(): boolean;
  hasToken(): boolean;
  getUserName(): string | null;
  getUserEmail(): string | null;
}

export type AuthStore = SmzStore<AuthState, AuthActions, AuthSelectors>;

const builder = new SmzStateStoreBuilder<AuthState, AuthActions, AuthSelectors>()
  .withScopeName('AuthStore')
  .withInitialState({ token: null, currentUser: null })
  .withLoaderFn(async (injector) => injector.get(AuthApiService).fetchAuthData())
  .withPlugin(withAutoRefresh(2 * 60 * 1000))
  .withActions((actions, injector, updateState) => {
    // Clear authentication data
    actions.clearAuth = () => {
      updateState({ token: null, currentUser: null });
    };

    // Update token
    actions.updateToken = (token: string) => {
      updateState({ token });
    };

    // Update user
    actions.updateUser = (user: User) => {
      updateState({ currentUser: user });
    };
  })
  .withSelectors((selectors, injector, getState) => {
    // Check if user is authenticated
    selectors.isAuthenticated = () => {
      const state = getState();
      return state.token !== null && state.currentUser !== null;
    };

    // Check if token exists
    selectors.hasToken = () => {
      return getState().token !== null;
    };

    // Get user name
    selectors.getUserName = () => {
      return getState().currentUser?.name || null;
    };

    // Get user email
    selectors.getUserEmail = () => {
      return getState().currentUser?.email || null;
    };
  });

export const AUTH_STORE_TOKEN = new InjectionToken<AuthStore>('AUTH_STORE_TOKEN');

export const authStoreProvider = builder.buildProvider(AUTH_STORE_TOKEN);