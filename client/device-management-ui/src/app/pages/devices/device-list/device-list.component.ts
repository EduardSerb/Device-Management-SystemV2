import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DeviceListItem, DeviceType } from '../../../models/device.models';
import { AuthService } from '../../../services/auth.service';
import { DeviceService } from '../../../services/device.service';

@Component({
  selector: 'app-device-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './device-list.component.html',
  styleUrl: './device-list.component.scss'
})
export class DeviceListComponent implements OnInit {
  private readonly devicesApi = inject(DeviceService);
  private readonly auth = inject(AuthService);

  items: DeviceListItem[] = [];
  searchQuery = '';
  error: string | null = null;
  loading = false;

  readonly DeviceType = DeviceType;

  ngOnInit(): void {
    this.reload();
  }

  reload() {
    this.loading = true;
    this.error = null;
    this.devicesApi.list().subscribe({
      next: (rows) => {
        this.items = rows;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to load devices.';
      }
    });
  }

  runSearch() {
    this.loading = true;
    this.error = null;
    this.devicesApi.search(this.searchQuery).subscribe({
      next: (rows) => {
        this.items = rows;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Search failed.';
      }
    });
  }

  deleteRow(item: DeviceListItem) {
    if (!window.confirm(`Delete ${item.name}?`)) {
      return;
    }
    this.devicesApi.delete(item.id).subscribe({
      next: () => this.reload(),
      error: (err) => {
        const msg = err?.error?.message ?? 'Delete failed.';
        window.alert(msg);
      }
    });
  }

  assign(item: DeviceListItem) {
    this.devicesApi.assign(item.id).subscribe({
      next: () => this.reload(),
      error: (err) => {
        const msg = err?.error?.message ?? 'Assign failed.';
        window.alert(msg);
      }
    });
  }

  unassign(item: DeviceListItem) {
    this.devicesApi.unassign(item.id).subscribe({
      next: () => this.reload(),
      error: (err) => {
        const msg = err?.error?.message ?? 'Unassign failed.';
        window.alert(msg);
      }
    });
  }

  assignedLabel(item: DeviceListItem): string {
    return item.assignedTo?.name ?? '—';
  }

  canUnassign(item: DeviceListItem): boolean {
    const uid = this.auth.userId();
    return !!item.assignedTo && !!uid && item.assignedTo.id === uid;
  }

  canAssign(item: DeviceListItem): boolean {
    return !item.assignedTo;
  }
}
