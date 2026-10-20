import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime } from 'rxjs';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { BeneficiariosService } from '../beneficiarios.service';
import { BeneficiarioListItem } from '../models/beneficiario.model';
import { AuthService } from '../../../core/services/auth.service';
import { BeneficiarioFormComponent } from '../beneficiario-form/beneficiario-form.component';

@Component({
  selector: 'app-beneficiarios-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule,
    InputTextModule, IconFieldModule, InputIconModule,
    ToastModule, TooltipModule, ConfirmDialogModule,
    BeneficiarioFormComponent,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './beneficiarios-list.component.html',
})
export class BeneficiariosListComponent implements OnInit {
  private service = inject(BeneficiariosService);
  auth = inject(AuthService);
  private toast = inject(MessageService);
  private confirm = inject(ConfirmationService);

  beneficiarios = signal<BeneficiarioListItem[]>([]);
  total = signal(0);
  carregando = signal(false);

  pagina = 1;
  tamanhoPagina = 10;
  busca = '';

  formAberto = signal(false);
  beneficiarioEmEdicao = signal<BeneficiarioListItem | null>(null);

  private busca$ = new Subject<void>();

  constructor() {
    this.busca$
      .pipe(debounceTime(300), takeUntilDestroyed())
      .subscribe(() => {
        this.pagina = 1;
        this.carregar();
      });
  }

  /** Administrador, Gestor e Operador podem cadastrar/editar. */
  get podeEditar(): boolean {
    const nivel = this.auth.usuario()?.nivelAcesso;
    return nivel === 'ADMINISTRADOR' || nivel === 'GESTOR' || nivel === 'OPERADOR';
  }

  /** Só Administrador e Gestor podem remover. */
  get podeRemover(): boolean {
    const nivel = this.auth.usuario()?.nivelAcesso;
    return nivel === 'ADMINISTRADOR' || nivel === 'GESTOR';
  }

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.service.listar(this.pagina, this.tamanhoPagina, this.busca.trim() || undefined).subscribe({
      next: res => {
        this.beneficiarios.set(res.items);
        this.total.set(res.total);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false),
    });
  }

  onBusca(): void {
    this.busca$.next();
  }

  onPage(event: { first: number; rows: number }): void {
    this.pagina = Math.floor(event.first / event.rows) + 1;
    this.tamanhoPagina = event.rows;
    this.carregar();
  }

  abrirNovo(): void {
    this.beneficiarioEmEdicao.set(null);
    this.formAberto.set(true);
  }

  abrirEdicao(beneficiario: BeneficiarioListItem): void {
    this.beneficiarioEmEdicao.set(beneficiario);
    this.formAberto.set(true);
  }

  onFormSalvo(): void {
    this.formAberto.set(false);
    this.carregar();
    this.toast.add({ severity: 'success', summary: 'Salvo', detail: 'Beneficiário salvo com sucesso.' });
  }

  confirmarRemocao(beneficiario: BeneficiarioListItem): void {
    this.confirm.confirm({
      message: `Deseja remover o beneficiário ${beneficiario.nome}?`,
      header: 'Confirmação',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Remover',
      rejectLabel: 'Cancelar',
      accept: () => this.remover(beneficiario),
    });
  }

  private remover(beneficiario: BeneficiarioListItem): void {
    this.service.inativar(beneficiario.idBeneficiario).subscribe({
      next: () => {
        // Se removeu o último item da página, volta uma página.
        if (this.beneficiarios().length === 1 && this.pagina > 1) this.pagina--;
        this.carregar();
        this.toast.add({ severity: 'success', summary: 'Removido', detail: 'Beneficiário removido.' });
      },
      error: e => this.toast.add({
        severity: 'error',
        summary: 'Erro',
        detail: e?.error?.mensagem ?? 'Não foi possível remover o beneficiário.',
      }),
    });
  }

  formatarCpf(cpf: string): string {
    const d = (cpf ?? '').replace(/\D/g, '');
    return d.length === 11 ? d.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4') : cpf;
  }

  rotuloMembros(qtd: number): string {
    return `${qtd} ${qtd === 1 ? 'membro' : 'membros'}`;
  }
}