import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { resource } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { delay, catchError } from 'rxjs/operators';
import { LOGGING_SERVICE, ScopedLogger } from '@smz-ui/core';

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
export class HttpResourceService {
  private http = inject(HttpClient);
  private logger: ScopedLogger = inject(LOGGING_SERVICE).scoped(HttpResourceService.name);

  /** Current user ID to drive all three resources */
  readonly userId = signal<number>(1);

  /** 1) Basic User Resource */
  readonly userResource = resource<User, { id: number }>({
    params: () => ({ id: this.userId() }),
    loader: async ({ params }) => {
      this.logger.debug(`Loading user with id=${params.id}`);
      const user = await firstValueFrom(
        this.http
          .get<User>(`https://jsonplaceholder.typicode.com/users/${params.id}`)
          .pipe(delay(1500))
          .pipe(
            catchError(err => {
              this.logger.error(`Failed to load user ${params.id}`, err);
              throw err;
            })
          )
      );
      this.logger.debug(`Loaded user ${user.id}: ${user.name}`);
      return user;
    }
  });

  /** 2) Posts Resource with error fallback */
  readonly postsResource = resource<Post[], { userId: number }>({
    params: () => ({ userId: this.userId() }),
    loader: async ({ params }) => {
      this.logger.debug(`Loading posts for userId=${params.userId}`);
      try {
        const posts = await firstValueFrom(
          this.http
            .get<Post[]>(`https://jsonplaceholder.typicode.com/posts`, {
              params: { userId: `${params.userId}` }
            })
            .pipe(delay(2000))
        );
        this.logger.debug(`Loaded ${posts.length} posts for user ${params.userId}`);
        return posts;
      } catch (err) {
        this.logger.error(`Error loading posts for user ${params.userId}`, err);
        return [];
      }
    }
  });

  /** 3) Combined User + Posts Resource */
  readonly userWithPostsResource = resource<UserWithPosts, { id: number }>({
    params: () => ({ id: this.userId() }),
    loader: async ({ params }) => {
      this.logger.debug(`Loading combined data for user ${params.id}`);
      const [user, posts] = await Promise.all([
        firstValueFrom(
          this.http
            .get<User>(`https://jsonplaceholder.typicode.com/users/${params.id}`)
            .pipe(delay(1000))
            .pipe(
              catchError(err => {
                this.logger.error(`Error loading user ${params.id}`, err);
                throw err;
              })
            )
        ),
        firstValueFrom(
          this.http
            .get<Post[]>(`https://jsonplaceholder.typicode.com/posts`, {
              params: { userId: `${params.id}` }
            })
            .pipe(delay(1500))
            .pipe(
              catchError(err => {
                this.logger.error(`Error loading posts for user ${params.id}`, err);
                return Promise.resolve([] as Post[]);
              })
            )
        )
      ]);
      this.logger.debug(`Combined load complete for user ${params.id}`);
      return { user, posts };
    }
  });

  /** Change the user ID (will automatically reload all three) */
  updateUserId(id: number) {
    this.logger.debug(`Updating userId signal to ${id}`);
    this.userId.set(id);
  }

  /** Manually force reload of each resource */
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
