import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { AUTH_STORE_TOKEN, AuthStore } from './auth-store.provider';

@Component({
  selector: 'app-auth-store-demo',
  standalone: true,
  imports: [CommonModule, ButtonModule, CardModule],
  host: { class: 'flex flex-col gap-4' },
  template: `
    <div class="text-3xl font-bold">Auth Store Demo</div>

    <p-card header="Actions" styleClass="w-full">
      <div class="flex gap-2">
        <button pButton type="button" label="Reload" icon="pi pi-refresh" (click)="store.actions.reload()" [disabled]="store.status.isLoading()"></button>
        <button pButton type="button" label="Force Reload" icon="pi pi-refresh" severity="warn" (click)="store.actions.forceReload()" [disabled]="store.status.isLoading()"></button>
        <button pButton type="button" label="Clear Auth" icon="pi pi-times" severity="danger" (click)="store.actions.clearAuth()" [disabled]="store.status.isLoading()"></button>
      </div>
    </p-card>

    <p-card header="Status" styleClass="w-full">
      <div class="flex items-center gap-3 p-3 rounded-lg"
           [ngClass]="{
             'bg-blue-50 border border-blue-200': store.status.isLoading(),
             'bg-green-50 border border-green-200': store.status.isLoaded(),
             'bg-red-50 border border-red-200': store.status.isError()
           }">
        <i class="pi text-xl"
           [ngClass]="{
             'pi-spin pi-spinner text-blue-500': store.status.isLoading(),
             'pi-check-circle text-green-500': store.status.isLoaded(),
             'pi-exclamation-circle text-red-500': store.status.isError()
           }"></i>
        <div class="flex flex-col">
          <span class="font-medium">Status:</span>
          <span class="text-lg font-semibold">{{ store.status.status().toUpperCase() }}</span>
        </div>
      </div>
    </p-card>

    @if (store.state.state()) {
      <p-card header="Auth Data" styleClass="w-full">
        <div class="flex flex-col gap-4">
          <div class="flex flex-col gap-2 p-4 bg-gray-50 rounded-lg border border-gray-200">
            <div class="flex items-center gap-2 text-gray-600">
              <i class="pi pi-key"></i>
              <span class="font-medium">Token</span>
            </div>
            <div class="text-sm font-mono bg-white p-2 rounded border">
              {{ store.state.state().token || 'No token' }}
            </div>
          </div>

          <div class="flex flex-col gap-2 p-4 bg-gray-50 rounded-lg border border-gray-200">
            <div class="flex items-center gap-2 text-gray-600">
              <i class="pi pi-user"></i>
              <span class="font-medium">User Information</span>
            </div>
            <div class="text-sm">
              <div><strong>Name:</strong> {{ store.state.state().currentUser?.name || 'No user' }}</div>
              <div><strong>Email:</strong> {{ store.state.state().currentUser?.email || 'No email' }}</div>
            </div>
          </div>
        </div>
      </p-card>

      <p-card header="Selectors" styleClass="w-full">
        <div class="flex flex-col gap-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="flex flex-col gap-2">
              <div class="font-bold">Auth Checks:</div>
              <div class="flex items-center gap-2">
                <i class="pi" [class.pi-check-circle]="store.selectors.isAuthenticated()" [class.pi-times-circle]="!store.selectors.isAuthenticated()"
                   [class.text-green-500]="store.selectors.isAuthenticated()" [class.text-red-500]="!store.selectors.isAuthenticated()"></i>
                <span>Is Authenticated</span>
              </div>
              <div class="flex items-center gap-2">
                <i class="pi" [class.pi-check-circle]="store.selectors.hasToken()" [class.pi-times-circle]="!store.selectors.hasToken()"
                   [class.text-green-500]="store.selectors.hasToken()" [class.text-red-500]="!store.selectors.hasToken()"></i>
                <span>Has Token</span>
              </div>
            </div>

            <div class="flex flex-col gap-2">
              <div class="font-bold">User Info:</div>
              <div class="flex items-center gap-2">
                <i class="pi pi-user text-blue-500"></i>
                <span>Name: {{ store.selectors.getUserName() || 'N/A' }}</span>
              </div>
              <div class="flex items-center gap-2">
                <i class="pi pi-envelope text-blue-500"></i>
                <span>Email: {{ store.selectors.getUserEmail() || 'N/A' }}</span>
              </div>
            </div>
          </div>
        </div>
      </p-card>
    }

    @if (store.status.isError()) {
      <div class="flex flex-col gap-2 p-4 border border-red-200 rounded-lg bg-red-50">
        <div class="flex items-center gap-2">
          <i class="pi pi-exclamation-triangle text-red-500"></i>
          <span class="font-semibold text-red-700">Error occurred</span>
        </div>
        <div class="text-red-600">{{ store.error.error()?.message }}</div>
        @if (store.error.error()?.storeName) {
          <div class="text-sm text-red-500">Store: {{ store.error.error()?.storeName }}</div>
        }
        @if (store.error.error()?.timestamp) {
          <div class="text-sm text-red-500">Time: {{ store.error.error()?.timestamp | date:'medium' }}</div>
        }
      </div>
    }
  `
})
export class AuthStoreDemoComponent implements OnInit, OnDestroy {
  readonly store: AuthStore = inject(AUTH_STORE_TOKEN);

  ngOnInit(): void {
    this.store.controls.wakeUp();
  }

  ngOnDestroy(): void {
    this.store.controls.sleep();
  }
}
