import { Component, EventEmitter, inject, Input, OnChanges, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { VisitasService } from '../visitas.service';
import { BeneficiarioOpcao, VisitaListItem } from '../models/visita.model';

@Component({
  selector: 'app-visita-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, DialogModule, ButtonModule,
    InputTextModule, TextareaModule, SelectModule, MessageModule,
  ],
  templateUrl: './visita-form.component.html',
})
export class VisitaFormComponent implements OnChanges {
  @Input() aberto = false;
  @Input() visita: VisitaListItem | null = null;
  @Output() fechou = new EventEmitter<void>();
  @Output() salvo = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private service = inject(VisitasService);

  form!: FormGroup;
  beneficiarios = signal<BeneficiarioOpcao[]>([]);
  carregando = signal(false);
  erro = signal<string | null>(null);

  get modoEdicao(): boolean { return !!this.visita; }

  ngOnChanges(): void {
    this.erro.set(null);
    this.construirForm();
    if (this.aberto) this.carregarBeneficiarios();
  }

  private construirForm(): void {
    const v = this.visita;
    this.form = this.fb.group({
      idBeneficiario: [v?.idBeneficiario ?? null, Validators.required],
      dataVisita: [v?.dataVisita ?? '', Validators.required],
      responsavel: [v?.responsavel ?? '', [Validators.required, Validators.maxLength(100)]],
      observacoes: [v?.observacoes ?? '', Validators.maxLength(500)],
    });
  }

  private carregarBeneficiarios(): void {
    this.service.listarBeneficiarios().subscribe({
      next: lista => {
        const v = this.visita;
        if (v && !lista.some(b => b.idBeneficiario === v.idBeneficiario)) {
          lista = [...lista, { idBeneficiario: v.idBeneficiario, nome: v.beneficiario }];
        }
        this.beneficiarios.set(lista);
      },
      error: () => this.erro.set('Não foi possível carregar os beneficiários.'),
    });
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.carregando.set(true);
    this.erro.set(null);

    const payload = this.form.value;
    const operacao = this.modoEdicao
      ? this.service.atualizar(this.visita!.idVisita, payload)
      : this.service.criar(payload);

    operacao.subscribe({
      next: () => {
        this.carregando.set(false);
        this.salvo.emit();
      },
      error: e => {
        this.erro.set(this.extrairMensagemErro(e));
        this.carregando.set(false);
      },
    });
  }

  private extrairMensagemErro(e: any): string {
    const body = e?.error;
    if (!body) return 'Erro ao salvar a visita.';
    if (typeof body === 'string') return body;
    if (body.mensagem) return body.mensagem;
    if (body.errors) {
      const mensagens = Object.values(body.errors).flat() as string[];
      if (mensagens.length) return mensagens.join(' ');
    }
    return 'Erro ao salvar a visita.';
  }

  fechar(): void { this.fechou.emit(); }
}
