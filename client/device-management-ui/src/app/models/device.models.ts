export enum DeviceType {
  Phone = 0,
  Tablet = 1
}

export interface UserSummary {
  id: string;
  name: string;
  role: string;
  location: string;
}

export interface DeviceListItem {
  id: number;
  name: string;
  manufacturer: string;
  type: DeviceType;
  os: string;
  assignedTo: UserSummary | null;
}

export interface DeviceDetail {
  id: number;
  name: string;
  manufacturer: string;
  type: DeviceType;
  os: string;
  osVersion: string;
  processor: string;
  ramAmount: string;
  description: string;
  assignedTo: UserSummary | null;
}

export interface CreateDevicePayload {
  name: string;
  manufacturer: string;
  type: DeviceType;
  os: string;
  osVersion: string;
  processor: string;
  ramAmount: string;
  description: string;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
}
