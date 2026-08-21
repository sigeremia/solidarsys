import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-esqueceu-senha',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ButtonModule, InputTextModule, MessageModule],
  templateUrl: './esqueceu-senha.component.html',
})
export class EsqueceuSenhaComponent {
  form: FormGroup;
  carregando = signal(false);
  enviado = signal(false);
  erro = signal<string | null>(null);

  constructor(private fb: FormBuilder, private auth: AuthService) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  enviar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.carregando.set(true);
    this.erro.set(null);

    this.auth.esqueceuSenha(this.form.value).subscribe({
      next: () => {
        this.enviado.set(true);
        this.carregando.set(false);
      },
      error: () => {
        // Sempre mostra a mensagem genérica (mesmo em erro de servidor) para não revelar se o e-mail existe
        this.enviado.set(true);
        this.carregando.set(false);
      },
    });
  }
}
