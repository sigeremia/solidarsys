import { Component, EventEmitter, inject, Input, OnChanges, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { BeneficiariosService } from '../beneficiarios.service';
import { BeneficiarioListItem, ComunidadeOpcao } from '../models/beneficiario.model';

/** Valida os dígitos verificadores do CPF (mesma regra do backend). */
function cpfValidator(control: AbstractControl): ValidationErrors | null {
  const cpf = String(control.value ?? '').replace(/\D/g, '');
  if (!cpf) return null; // "required" cuida do vazio
  if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) return { cpf: true };

  const calc = (base: string, pesoInicial: number): number => {
    const soma = base.split('').reduce((acc, d, i) => acc + Number(d) * (pesoInicial - i), 0);
    const resto = soma % 11;
    return resto < 2 ? 0 : 11 - resto;
  };

  const d1 = calc(cpf.slice(0, 9), 10);
  const d2 = calc(cpf.slice(0, 10), 11);
  return d1 === Number(cpf[9]) && d2 === Number(cpf[10]) ? null : { cpf: true };
}

@Component({
  selector: 'app-beneficiario-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, DialogModule, ButtonModule,
    InputTextModule, SelectModule, MessageModule,
  ],
  templateUrl: './beneficiario-form.component.html',
})
export class BeneficiarioFormComponent implements OnChanges {
  @Input() aberto = false;
  @Input() beneficiario: BeneficiarioListItem | null = null;
  @Output() fechou = new EventEmitter<void>();
  @Output() salvo = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private service = inject(BeneficiariosService);

  form!: FormGroup;
  comunidades = signal<ComunidadeOpcao[]>([]);
  carregando = signal(false);
  erro = signal<string | null>(null);

  hoje = new Date().toISOString().split('T')[0];

  get modoEdicao(): boolean { return !!this.beneficiario; }

  ngOnChanges(): void {
    this.erro.set(null);
    this.construirForm();
    if (this.aberto) this.carregarComunidades();
  }

  private construirForm(): void {
    const b = this.beneficiario;
    this.form = this.fb.group({
      nome:           [b?.nome ?? '', [Validators.required, Validators.maxLength(150)]],
      cpf:            [b?.cpf ?? '', [Validators.required, cpfValidator]],
      dataNascimento: [b?.dataNascimento ?? '', Validators.required],
      endereco:       [b?.endereco ?? '', [Validators.required, Validators.maxLength(255)]],
      idComunidade:   [b?.idComunidade ?? null, Validators.required],
    });
  }

  private carregarComunidades(): void {
    this.service.listarComunidades().subscribe({
      next: lista => {
        const b = this.beneficiario;
        // Mantém visível a comunidade atual mesmo que ela tenha sido inativada depois.
        if (b && !lista.some(c => c.idComunidade === b.idComunidade)) {
          lista = [...lista, { idComunidade: b.idComunidade, nome: b.comunidade }];
        }
        this.comunidades.set(lista);
      },
      error: () => this.erro.set('Não foi possível carregar as comunidades.'),
    });
  }

  /** Aceita só dígitos no CPF (máx. 11). */
  onCpfInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const limpo = input.value.replace(/\D/g, '').slice(0, 11);
    if (input.value !== limpo) input.value = limpo;
    this.form.get('cpf')!.setValue(limpo, { emitEvent: false });
  }

  salvar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.carregando.set(true);
    this.erro.set(null);

    const payload = this.form.value;

    const obs = this.modoEdicao
      ? this.service.atualizar(this.beneficiario!.idBeneficiario, payload)
      : this.service.criar(payload);

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