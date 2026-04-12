import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { DeviceDetailComponent } from './pages/devices/device-detail/device-detail.component';
import { DeviceFormComponent } from './pages/devices/device-form/device-form.component';
import { DeviceListComponent } from './pages/devices/device-list/device-list.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'devices',
    canActivate: [authGuard],
    children: [
      { path: '', component: DeviceListComponent },
      { path: 'new', component: DeviceFormComponent },
      { path: ':id/edit', component: DeviceFormComponent },
      { path: ':id', component: DeviceDetailComponent }
    ]
  },
  { path: '', redirectTo: 'devices', pathMatch: 'full' }
];
