import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-redefinir-senha',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ButtonModule, PasswordModule, MessageModule],
  templateUrl: './redefinir-senha.component.html',
})
export class RedefinirSenhaComponent implements OnInit {
  form: FormGroup;
  carregando = signal(false);
  concluido = signal(false);
  erro = signal<string | null>(null);
  token = signal<string | null>(null);

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group(
      {
        novaSenha: ['', [Validators.required, Validators.minLength(8)]],
        confirmarSenha: ['', Validators.required],
      },
      { validators: this.senhasIguais }
    );
  }

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.erro.set('Link inválido. Solicite um novo link de recuperação.');
    } else {
      this.token.set(token);
    }
  }

  redefinir(): void {
    if (this.form.invalid || !this.token()) {
      this.form.markAllAsTouched();
      return;
    }

    this.carregando.set(true);
    this.erro.set(null);

    const payload = {
      token: this.token()!,
      novaSenha: this.form.value.novaSenha,
      confirmarSenha: this.form.value.confirmarSenha,
    };

    this.auth.redefinirSenha(payload).subscribe({
      next: () => {
        this.concluido.set(true);
        this.carregando.set(false);
        setTimeout(() => this.router.navigate(['/login']), 3000);
      },
      error: (e) => {
        this.erro.set(this.mapearErro(e));
        this.carregando.set(false);
      },
    });
  }

  private senhasIguais(group: FormGroup) {
    const nova = group.get('novaSenha')?.value;
    const confirmar = group.get('confirmarSenha')?.value;
    return nova === confirmar ? null : { senhasDivergem: true };
  }

  private mapearErro(e: any): string {
    if (e?.status === 400) {
      const msg = e?.error?.mensagem || e?.error;
      if (typeof msg === 'string') return msg;
    }
    if (e?.status === 404) return 'Token inválido ou expirado. Solicite um novo link.';
    return 'Erro ao redefinir a senha. Tente novamente.';
  }
}
