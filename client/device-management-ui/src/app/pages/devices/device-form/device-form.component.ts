import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DeviceType } from '../../../models/device.models';
import { DeviceService } from '../../../services/device.service';

@Component({
  selector: 'app-device-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './device-form.component.html',
  styleUrl: './device-form.component.scss'
})
export class DeviceFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(DeviceService);

  readonly DeviceType = DeviceType;

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    manufacturer: ['', [Validators.required]],
    type: [DeviceType.Phone, [Validators.required]],
    os: ['', [Validators.required]],
    osVersion: ['', [Validators.required]],
    processor: ['', [Validators.required]],
    ramAmount: ['', [Validators.required]],
    description: ['', [Validators.required]]
  });

  errorMessage: string | null = null;
  loading = false;
  private deviceId: number | null = null;

  get isEdit(): boolean {
    return this.deviceId !== null;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }
    const parsed = Number(id);
    if (!Number.isFinite(parsed)) {
      this.errorMessage = 'Invalid device id.';
      return;
    }
    this.deviceId = parsed;
    this.loading = true;
    this.api.getById(parsed).subscribe({
      next: (d) => {
        this.form.patchValue({
          name: d.name,
          manufacturer: d.manufacturer,
          type: d.type,
          os: d.os,
          osVersion: d.osVersion,
          processor: d.processor,
          ramAmount: d.ramAmount,
          description: d.description
        });
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Failed to load device.';
      }
    });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.errorMessage = null;
    const payload = this.form.getRawValue();

    if (this.isEdit && this.deviceId !== null) {
      this.api.update(this.deviceId, payload).subscribe({
        next: (d) => {
          void this.router.navigate(['/devices', d.id]);
        },
        error: (err) => this.handleError(err)
      });
    } else {
      this.api.create(payload).subscribe({
        next: (d) => {
          void this.router.navigate(['/devices', d.id]);
        },
        error: (err) => this.handleError(err)
      });
    }
  }

  private handleError(err: unknown) {
    this.loading = false;
    const anyErr = err as { status?: number; error?: { message?: string } };
    if (anyErr?.status === 409) {
      this.errorMessage = anyErr.error?.message ?? 'Device already exists (same name and manufacturer).';
      return;
    }
    this.errorMessage = anyErr.error?.message ?? 'Save failed.';
  }
}
