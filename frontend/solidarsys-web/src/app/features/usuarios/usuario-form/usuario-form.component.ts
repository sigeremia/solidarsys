import { Component, EventEmitter, inject, Input, OnChanges, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { UsuariosService } from '../usuarios.service';
import { UsuarioListItem } from '../models/usuario.model';
import { NivelAcesso } from '../../../core/models/auth.model';

@Component({
  selector: 'app-usuario-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, DialogModule, ButtonModule,
    InputTextModule, PasswordModule, SelectModule, MessageModule,
  ],
  templateUrl: './usuario-form.component.html',
})
export class UsuarioFormComponent implements OnChanges {
  @Input() aberto = false;
  @Input() usuario: UsuarioListItem | null = null;
  @Output() fechou = new EventEmitter<void>();
  @Output() salvo = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private service = inject(UsuariosService);

  form!: FormGroup;
  carregando = signal(false);
  erro = signal<string | null>(null);

  get modoEdicao(): boolean { return !!this.usuario; }

  niveisAcesso = [
   // { label: 'Administrador', value: 'ADMINISTRADOR' as NivelAcesso },
    { label: 'Gestor',        value: 'GESTOR' as NivelAcesso },
    { label: 'Operador',      value: 'OPERADOR' as NivelAcesso },
    { label: 'Voluntário',    value: 'VOLUNTARIO' as NivelAcesso },
  ];

  ngOnChanges(): void {
    this.erro.set(null);
    this.construirForm();
  }

  private construirForm(): void {
    if (this.modoEdicao) {
      this.form = this.fb.group({
        nome:           [this.usuario!.nome, [Validators.required, Validators.maxLength(150)]],
        dataNascimento: [this.usuario!.dataNascimento, Validators.required],
        nivelAcesso:    [this.usuario!.nivelAcesso, Validators.required],
        idSetor:        [null],
      });
    } else {
      this.form = this.fb.group({
        nome:           ['', [Validators.required, Validators.maxLength(150)]],
        email:          ['', [Validators.required, Validators.email]],
        senha:          ['', [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$/),
        ]],
        cpf:            ['', [Validators.required, Validators.pattern(/^\d{11}$/)]],
        dataNascimento: ['', Validators.required],
        nivelAcesso:    ['OPERADOR' as NivelAcesso, Validators.required],
        idSetor:        [null],
      });
    }
  }

  salvar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.carregando.set(true);
    this.erro.set(null);

    const obs = this.modoEdicao
      ? this.service.atualizar(this.usuario!.id, this.form.value)
      : this.service.criar(this.form.value);

    obs.subscribe({
      next: () => { this.carregando.set(false); this.salvo.emit(); },
      error: e => {
        this.erro.set(this.extrairMensagemErro(e));
        this.carregando.set(false);
      },
    });
  }

  private extrairMensagemErro(e: any): string {
    const body = e?.error;
    if (!body) return 'Erro ao salvar. Verifique os dados.';
    if (typeof body === 'string') return body;
    if (body.mensagem) return body.mensagem;
    if (body.errors) {
      const mensagens = Object.values(body.errors).flat() as string[];
      if (mensagens.length) return mensagens.join(' ');
    }
    if (body.title) return body.title;
    return 'Erro ao salvar. Verifique os dados.';
  }

  fechar(): void { this.fechou.emit(); }
}