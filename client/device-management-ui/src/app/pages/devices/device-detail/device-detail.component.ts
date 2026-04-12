import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DeviceDetail, DeviceType } from '../../../models/device.models';
import { DeviceService } from '../../../services/device.service';

@Component({
  selector: 'app-device-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './device-detail.component.html',
  styleUrl: './device-detail.component.scss'
})
export class DeviceDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(DeviceService);

  device: DeviceDetail | null = null;
  error: string | null = null;
  loading = false;
  aiMessage: string | null = null;

  readonly DeviceType = DeviceType;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isFinite(id)) {
      this.error = 'Invalid id.';
      return;
    }
    this.loading = true;
    this.api.getById(id).subscribe({
      next: (d) => {
        this.device = d;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Device not found.';
      }
    });
  }

  generate() {
    if (!this.device) {
      return;
    }
    this.aiMessage = null;
    this.api.generateDescription(this.device.id).subscribe({
      next: (res) => {
        this.aiMessage = 'Description updated.';
        this.device!.description = res.description;
      },
      error: (err) => {
        this.aiMessage = err?.error?.message ?? 'Generation failed. Configure OpenAI:ApiKey on the API.';
      }
    });
  }
}
