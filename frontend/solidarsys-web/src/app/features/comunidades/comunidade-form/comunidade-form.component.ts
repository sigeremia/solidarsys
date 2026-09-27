import { Component, EventEmitter, inject, Input, OnChanges, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { ComunidadesService } from '../comunidades.service';
import { ComunidadeListItem } from '../models/comunidade.model';
import { NivelAcesso } from '../../../core/models/auth.model';

@Component({
  selector: 'app-comunidade-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, DialogModule, ButtonModule,
    InputTextModule, PasswordModule, SelectModule, MessageModule,
  ],
  templateUrl: './comunidade-form.component.html',
})
export class ComunidadeFormComponent implements OnChanges {
  @Input() aberto = false;
  @Input() comunidade: ComunidadeListItem | null = null;
  @Output() fechou = new EventEmitter<void>();
  @Output() salvo = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private service = inject(ComunidadesService);

  form!: FormGroup;
  carregando = signal(false);
  erro = signal<string | null>(null);

  get modoEdicao(): boolean { return !!this.comunidade; }


  ngOnChanges(): void {
    this.erro.set(null);
    this.construirForm();
  }

  private construirForm(): void {
    if (this.modoEdicao) {
      this.form = this.fb.group({
        nome:           [this.comunidade!.nome, [Validators.required, Validators.maxLength(150)]],
        regiao: [this.comunidade!.regiao, Validators.required],
        descricao:        [this.comunidade!.descricao, Validators.required],
      });
    } else {
      this.form = this.fb.group({
        nome:           ['', [Validators.required, Validators.maxLength(150)]],
        regiao:           ['', [Validators.required]],
        descricao:        ['', [Validators.required]],
      });
    }
  }

  salvar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.carregando.set(true);
    this.erro.set(null);

    const obs = this.modoEdicao
      ? this.service.atualizar(this.comunidade!.idComunidade, this.form.value)
      : this.service.criar(this.form.value);

    obs.subscribe({
      next: () => { this.carregando.set(false); this.salvo.emit(); },
      error: e => {
        this.erro.set(e?.error?.mensagem ?? e?.error ?? 'Erro ao salvar. Verifique os dados.');
        this.carregando.set(false);
      },
    });
  }

  fechar(): void { this.fechou.emit(); }
}
