import { Component, EventEmitter, Input, OnChanges, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { MessageModule } from 'primeng/message';
import { SelectButtonModule } from 'primeng/selectbutton';
import { DoadorListItem, DoadorRequest, TipoDoador } from '../models/doador.model';
import { DoadoresService } from '../doadores.service';

function cpfValido(documento: string): boolean {
  if (documento.length !== 11 || /^(\d)\1{10}$/.test(documento)) return false;
  const calcular = (base: string, peso: number): number => {
    const soma = base.split('').reduce((total, digito, indice) => total + Number(digito) * (peso - indice), 0);
    const resto = soma % 11;
    return resto < 2 ? 0 : 11 - resto;
  };
  return calcular(documento.slice(0, 9), 10) === Number(documento[9])
    && calcular(documento.slice(0, 10), 11) === Number(documento[10]);
}

function cnpjValido(documento: string): boolean {
  if (documento.length !== 14 || /^(\d)\1{13}$/.test(documento)) return false;
  const calcular = (base: string, pesos: number[]): number => {
    const soma = base.split('').reduce((total, digito, indice) => total + Number(digito) * pesos[indice], 0);
    const resto = soma % 11;
    return resto < 2 ? 0 : 11 - resto;
  };
  const primeiro = calcular(documento.slice(0, 12), [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
  const segundo = calcular(documento.slice(0, 12) + primeiro, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
  return primeiro === Number(documento[12]) && segundo === Number(documento[13]);
}

@Component({
  selector: 'app-doador-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, DialogModule, ButtonModule,
    InputTextModule, TextareaModule, MessageModule, SelectButtonModule,
  ],
  templateUrl: './doador-form.component.html',
})
export class DoadorFormComponent implements OnChanges {
  @Input() aberto = false;
  @Input() doador: DoadorListItem | null = null;
  @Output() fechou = new EventEmitter<void>();
  @Output() salvo = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private service = inject(DoadoresService);

  form!: FormGroup;
  carregando = signal(false);
  erro = signal<string | null>(null);
  tiposDoador = [
    { label: 'Pessoa Física', value: 'PESSOA_FISICA' as TipoDoador },
    { label: 'Pessoa Jurídica', value: 'PESSOA_JURIDICA' as TipoDoador },
  ];

  get modoEdicao(): boolean { return !!this.doador; }
  get tipo(): TipoDoador { return this.form?.get('tipo')?.value ?? 'PESSOA_FISICA'; }

  ngOnChanges(): void {
    this.erro.set(null);
    this.construirForm();
  }

  onTipoAlterado(): void {
    this.form.get('documento')?.updateValueAndValidity();
  }

  private construirForm(): void {
    const doador = this.doador;
    this.form = this.fb.group({
      tipo: [doador?.tipo ?? 'PESSOA_FISICA', Validators.required],
      nome: [doador?.nome ?? '', [Validators.required, Validators.maxLength(100)]],
      nomeFantasia: [doador?.nomeFantasia ?? '', Validators.maxLength(100)],
      documento: [doador?.documento ?? '', [Validators.required, Validators.maxLength(18), (control: AbstractControl) => this.validarDocumento(control)]],
      telefone: [this.formatarTelefone(doador?.telefone ?? ''), (control: AbstractControl) => this.validarTelefone(control)],
      email: [doador?.email ?? '', [Validators.email, Validators.maxLength(150)]],
      observacao: [doador?.observacao ?? '', Validators.maxLength(2000)],
    });
  }

  private validarDocumento(control: AbstractControl): ValidationErrors | null {
    const original = String(control.value ?? '').trim();
    if (!original) return null;
    if (original.length > 18) return { maxlength: true };

    const digitos = original.replace(/\D/g, '');
    if (this.tipo === 'PESSOA_FISICA' && digitos.length === 11)
      return cpfValido(digitos) ? null : { documento: true };
    if (this.tipo === 'PESSOA_JURIDICA' && digitos.length === 14)
      return cnpjValido(digitos) ? null : { documento: true };
    if ((this.tipo === 'PESSOA_FISICA' && digitos.length === 14)
      || (this.tipo === 'PESSOA_JURIDICA' && digitos.length === 11))
      return { documento: true };
    return null;
  }

  private formatarTelefone(telefone: string): string {
    const digitos = telefone.replace(/\D/g, '');
    if (digitos.length === 11)
      return digitos.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
    if (digitos.length === 10)
      return digitos.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
    return telefone;
  }

  private validarTelefone(control: AbstractControl): ValidationErrors | null {
    const telefone = String(control.value ?? '');
    if (!telefone.trim()) return null;
    const digitos = telefone.replace(/\D/g, '');
    return digitos.length === 10 || digitos.length === 11 ? null : { telefone: true };
  }

  onTelefoneInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digitos = input.value.replace(/\D/g, '').slice(0, 11);
    let formatado = digitos;
    if (digitos.length > 10)
      formatado = digitos.replace(/(\d{2})(\d{5})(\d{0,4})/, '($1) $2-$3');
    else if (digitos.length > 6)
      formatado = digitos.replace(/(\d{2})(\d{4})(\d{0,4})/, '($1) $2-$3');
    else if (digitos.length > 2)
      formatado = digitos.replace(/(\d{2})(\d{0,5})/, '($1) $2');
    input.value = formatado;
    this.form.get('telefone')?.setValue(formatado, { emitEvent: false });
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.carregando.set(true);
    this.erro.set(null);
    const payload = this.form.getRawValue() as DoadorRequest;
    const requisicao = this.modoEdicao
      ? this.service.atualizar(this.doador!.idDoador, payload)
      : this.service.criar(payload);

    requisicao.subscribe({
      next: () => {
        this.carregando.set(false);
        this.salvo.emit();
      },
      error: erro => {
        this.erro.set(this.extrairMensagemErro(erro));
        this.carregando.set(false);
      },
    });
  }

  fechar(): void { this.fechou.emit(); }

  private extrairMensagemErro(erro: any): string {
    const body = erro?.error;
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
}