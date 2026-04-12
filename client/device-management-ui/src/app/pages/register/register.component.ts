import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    fullName: ['', [Validators.required]],
    roleName: ['', [Validators.required]],
    location: ['', [Validators.required]]
  });

  errorMessage: string | null = null;
  loading = false;

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.errorMessage = null;
    this.auth.register(this.form.getRawValue()).subscribe({
      next: (res) => {
        this.auth.persistSession(res);
        void this.router.navigateByUrl('/devices');
      },
      error: (err) => {
        this.loading = false;
        const msg = err?.error?.message ?? err?.error?.errors?.[0];
        this.errorMessage = typeof msg === 'string' ? msg : 'Registration failed.';
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
}
