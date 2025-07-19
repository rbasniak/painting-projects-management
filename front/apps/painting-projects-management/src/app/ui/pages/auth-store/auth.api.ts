import { Injectable } from '@angular/core';
import { User } from './user';

export interface AuthState {
  token: string | null;
  currentUser: User | null;
}

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  async fetchAuthData(): Promise<Partial<AuthState>> {
    await new Promise((resolve) => setTimeout(resolve, 300));
    return {
      token: 'token-' + Math.random().toString(36).substring(2, 8),
      currentUser: { id: 1, name: 'Demo User', email: 'demo@user.com' },
    };
  }
}
