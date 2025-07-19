import { Injectable, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { LoggingService } from '@smz-ui/layout';
import { BaseHttpResourceService, HttpResourceConfig } from './base-http-resource.service';
import { firstValueFrom } from 'rxjs';

interface User {
  id: number;
  name: string;
  email: string;
}

interface Post {
  id: number;
  title: string;
  body: string;
  userId: number;
}

interface UserWithPosts {
  user: User;
  posts: Post[];
}

@Injectable({ providedIn: 'root' })
export class UserResourceService extends BaseHttpResourceService {
  /** Current user ID to drive all resources */
  readonly userId = signal<number>(1);

  constructor(loggingService: LoggingService) {
    const config: HttpResourceConfig<unknown> = {
      baseUrl: 'https://jsonplaceholder.typicode.com',
      defaultDelay: 1000, // Simulate network delay in development
      errorHandler: async (error: HttpErrorResponse) => {
        // Default error handling
        if (error.status === 404) {
          return undefined;
        }
        throw error;
      }
    };
    super(config as HttpResourceConfig<never>, loggingService);
  }

  /** Get user by ID */
  readonly userResource = this.createGetResource<User | undefined, { id: number }>('/users/:id', {
    errorHandler: async (error: HttpErrorResponse, params) => {
      if (error.status === 404) {
        this.logger.warn(`User ${params.id} not found`);
        return undefined;
      }
      throw error;
    }
  });

  /** Get posts by user ID */
  readonly postsResource = this.createGetResource<Post[], { userId: number }>('/posts', {
    errorHandler: async (error: HttpErrorResponse, params) => {
      this.logger.warn(`Failed to load posts for user ${params.userId}`, error);
      return [];
    }
  });

  /** Get user with their posts */
  readonly userWithPostsResource = this.createGetResource<UserWithPosts, { id: number }>(
    '/users/:id',
    {
      loader: async (params) => {
        const user = await firstValueFrom(
          this.http.get<User>(`${this.config.baseUrl}/users/${params.id}`)
        );
        const posts = await firstValueFrom(
          this.http.get<Post[]>(`${this.config.baseUrl}/posts`, {
            params: { userId: `${params.id}` }
          })
        );
        return { user, posts };
      }
    }
  );

  /** Update user ID (triggers reload of all resources) */
  updateUserId(id: number) {
    this.logger.debug(`Updating userId signal to ${id}`);
    this.userId.set(id);
  }

  /** Force reload of specific resources */
  reloadUser() {
    this.logger.debug('Manually reloading userResource');
    this.userResource.reload();
  }

  reloadPosts() {
    this.logger.debug('Manually reloading postsResource');
    this.postsResource.reload();
  }

  reloadUserWithPosts() {
    this.logger.debug('Manually reloading userWithPostsResource');
    this.userWithPostsResource.reload();
  }
} 