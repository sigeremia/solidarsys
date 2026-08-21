import { Component, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    MessageModule,
  ],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  form: FormGroup;
  carregando = signal(false);
  erro = signal<string | null>(null);

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      senha: ['', Validators.required],
    });
  }

  entrar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.carregando.set(true);
    this.erro.set(null);

    this.auth.login(this.form.value).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (e) => {
        this.erro.set(this.mapearErro(e));
        this.carregando.set(false);
      },
    });
  }

  private mapearErro(e: any): string {
    if (e?.status === 401) return 'E-mail ou senha incorretos.';
    if (e?.status === 403) {
      const mins = e?.error?.minutosRestantes ?? 15;
      return `Conta bloqueada. Tente novamente em ${mins} minuto(s).`;
    }
    if (e?.status === 429) return 'Muitas tentativas. Aguarde alguns minutos.';
    return 'Erro ao conectar. Verifique sua conexão e tente novamente.';
  }
}
