import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { resource } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { delay } from 'rxjs/operators';
import { LoggingService, ScopedLogger } from '@smz-ui/layout';

/**
 * Configuration interface for HTTP Resource options
 */
export interface HttpResourceConfig<T, P = Record<string, unknown>> {
  /** Base URL for the API endpoint */
  baseUrl: string;
  /** Default delay in milliseconds for simulating network latency (development only) */
  defaultDelay?: number;
  /** Function to handle errors */
  errorHandler?: (error: HttpErrorResponse, params: P) => Promise<T>;
  /** Function to transform the response data */
  transformResponse?: (data: unknown) => T | Promise<T>;
  /** Custom loader function */
  loader?: (params: P) => Promise<T>;
}

/**
 * Base service for creating HTTP Resources with common functionality
 * Provides a foundation for building type-safe, error-handled HTTP resources
 */
@Injectable()
export abstract class BaseHttpResourceService {
  protected readonly http = inject(HttpClient);
  protected readonly logger: ScopedLogger;

  constructor(protected readonly config: HttpResourceConfig<never>, loggingService: LoggingService) {
    this.logger = loggingService.scoped(this.constructor.name);
  }

  /**
   * Creates a GET resource with standard error handling and logging
   */
  protected createGetResource<T, P extends Record<string, unknown>>(
    endpoint: string,
    config: Partial<HttpResourceConfig<T, P>> = {}
  ) {
    const fullConfig = { ...this.config, ...config };
    const url = `${fullConfig.baseUrl}${endpoint}`;

    return resource<T, P>({
      params: () => ({} as P),
      loader: async ({ params }) => {
        this.logger.debug(`Loading data from ${url}`, { params });
        
        try {
          if (fullConfig.loader) {
            return fullConfig.loader(params);
          }

          const response = await firstValueFrom(
            this.http
              .get(url, { params: this.serializeParams(params) })
              .pipe(delay(fullConfig.defaultDelay ?? 0))
          );

          const data = fullConfig.transformResponse 
            ? fullConfig.transformResponse(response)
            : response;

          this.logger.debug(`Successfully loaded data from ${url}`, { params });
          return data as T;
        } catch (error) {
          this.logger.error(`Error loading data from ${url}`, { params, error });
          
          if (fullConfig.errorHandler) {
            return fullConfig.errorHandler(error as HttpErrorResponse, params);
          }
          
          throw error;
        }
      }
    });
  }

  /**
   * Creates a POST resource with standard error handling and logging
   */
  protected createPostResource<T, P extends Record<string, unknown>>(
    endpoint: string,
    config: Partial<HttpResourceConfig<T, P>> = {}
  ) {
    const fullConfig = { ...this.config, ...config };
    const url = `${fullConfig.baseUrl}${endpoint}`;

    return resource<T, P>({
      params: () => ({} as P),
      loader: async ({ params }) => {
        this.logger.debug(`Posting data to ${url}`, { params });
        
        try {
          const response = await firstValueFrom(
            this.http
              .post(url, params)
              .pipe(delay(fullConfig.defaultDelay ?? 0))
          );

          const data = fullConfig.transformResponse 
            ? fullConfig.transformResponse(response)
            : response;

          this.logger.debug(`Successfully posted data to ${url}`, { params });
          return data as T;
        }
        catch (error) {
          this.logger.error(`Error posting data to ${url}`, { params, error });
          
          if (fullConfig.errorHandler) {
            return fullConfig.errorHandler(error as HttpErrorResponse, params);
          }
          
          throw error;
        }
      }
    });
  }

  /**
   * Serializes parameters for HTTP requests
   * Converts objects and arrays to string format
   */
  private serializeParams(params: Record<string, unknown>): Record<string, string> {
    return Object.entries(params).reduce((acc, [key, value]) => {
      if (value !== undefined && value !== null) {
        acc[key] = String(value);
      }
      return acc;
    }, {} as Record<string, string>);
  }
} 