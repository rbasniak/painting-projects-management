import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StoreHistoryEvent, STORE_HISTORY_SERVICE, IStoreHistoryService } from '@smz-ui/store';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-store-history',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-4">
      <div class="flex justify-between items-center mb-4">
        <h1 class="text-2xl font-bold">Store History</h1>
        <button
          (click)="loadEvents()"
          class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
        >
          <span class="pi pi-refresh text-sm mr-1"></span>
          Refresh
        </button>
      </div>

      <div class="mb-4">
        <label for="storeSelect" class="block text-sm font-medium text-gray-700 mb-2">
          Select Store
        </label>
        <select
          id="storeSelect"
          [(ngModel)]="selectedStore"
          (ngModelChange)="onStoreChange()"
          class="block w-full px-3 py-2 bg-white border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
        >
          <option value="">All Stores</option>
          <option *ngFor="let store of availableStores" [value]="store">
            {{ store }}
          </option>
        </select>
      </div>

      <div class="overflow-x-auto">
        @if (filteredEvents.length > 0) {
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Store</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Params</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Timestamp</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr *ngFor="let event of filteredEvents">
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{{ event.storeScope }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{{ event.action }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{{ event.params | json }}</td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span
                    [class]="getStatusClass(event.status)"
                    class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                  >
                    {{ event.status }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ event.timestamp | date:'medium' }}
                </td>
              </tr>
            </tbody>
          </table>
        } @else {
          <div class="text-center py-12">
            <div class="mx-auto h-12 w-12 text-gray-400">
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <h3 class="mt-2 text-sm font-medium text-gray-900">No events found</h3>
            <p class="mt-1 text-sm text-gray-500">
              @if (selectedStore) {
                No events found for the selected store.
              } @else {
                No store events have been recorded yet.
              }
            </p>
            <div class="mt-6">
              <button
                (click)="loadEvents()"
                class="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              >
                <span class="pi pi-refresh text-sm mr-1"></span>
                Refresh
              </button>
            </div>
          </div>
        }
      </div>
    </div>
  `,
})
export class StoreHistoryComponent implements OnInit, OnDestroy {
  private storeHistory: IStoreHistoryService = inject(STORE_HISTORY_SERVICE);
  events: StoreHistoryEvent[] = [];
  filteredEvents: StoreHistoryEvent[] = [];
  availableStores: string[] = [];
  selectedStore = '';
  private refreshInterval!: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    this.loadEvents();
    // Auto-refresh a cada 2 segundos
    this.refreshInterval = setInterval(() => this.loadEvents(), 10000);
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
  }

  loadEvents(): void {
    this.events = this.storeHistory.getAllEvents();
    this.availableStores = [...new Set(this.events.map(e => e.storeScope))];
    this.filterEvents();
  }

  onStoreChange(): void {
    this.filterEvents();
  }

  private filterEvents(): void {
    this.filteredEvents = this.selectedStore
      ? this.events.filter(e => e.storeScope === this.selectedStore)
      : this.events;
  }

  getStatusClass(status: string): string {
    const baseClass = 'px-2 inline-flex text-xs leading-5 font-semibold rounded-full';
    switch (status) {
      case 'loading':
        return `${baseClass} bg-yellow-100 text-yellow-800`;
      case 'resolved':
        return `${baseClass} bg-green-100 text-green-800`;
      case 'error':
        return `${baseClass} bg-red-100 text-red-800`;
      default:
        return `${baseClass} bg-gray-100 text-gray-800`;
    }
  }
}