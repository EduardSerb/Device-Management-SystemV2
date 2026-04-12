import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import {
  CreateDevicePayload,
  DeviceDetail,
  DeviceListItem
} from '../models/device.models';

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/devices`;

  list() {
    return this.http.get<DeviceListItem[]>(this.base);
  }

  search(q: string) {
    const params = new HttpParams().set('q', q);
    return this.http.get<DeviceListItem[]>(`${this.base}/search`, { params });
  }

  getById(id: number) {
    return this.http.get<DeviceDetail>(`${this.base}/${id}`);
  }

  create(payload: CreateDevicePayload) {
    return this.http.post<DeviceDetail>(this.base, payload);
  }

  update(id: number, payload: CreateDevicePayload) {
    return this.http.put<DeviceDetail>(`${this.base}/${id}`, payload);
  }

  delete(id: number) {
    return this.http.delete(`${this.base}/${id}`);
  }

  assign(id: number) {
    return this.http.post(`${this.base}/${id}/assign`, {});
  }

  unassign(id: number) {
    return this.http.post(`${this.base}/${id}/unassign`, {});
  }

  generateDescription(id: number) {
    return this.http.post<{ description: string }>(`${this.base}/${id}/generate-description`, {});
  }
}
