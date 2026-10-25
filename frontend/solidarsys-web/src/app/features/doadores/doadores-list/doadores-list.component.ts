import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime } from 'rxjs';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { AuthService } from '../../../core/services/auth.service';
import { DoadorListItem } from '../models/doador.model';
import { DoadoresService } from '../doadores.service';
import { DoadorFormComponent } from '../doador-form/doador-form.component';

@Component({
  selector: 'app-doadores-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule,
    InputTextModule, IconFieldModule, InputIconModule, TagModule,
    ToastModule, TooltipModule, ConfirmDialogModule, DoadorFormComponent,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './doadores-list.component.html',
})
export class DoadoresListComponent implements OnInit {
  private service = inject(DoadoresService);
  auth = inject(AuthService);
  private toast = inject(MessageService);
  private confirm = inject(ConfirmationService);

  doadores = signal<DoadorListItem[]>([]);
  total = signal(0);
  carregando = signal(false);
  formAberto = signal(false);
  doadorEmEdicao = signal<DoadorListItem | null>(null);

  pagina = 1;
  tamanhoPagina = 10;
  busca = '';
  private busca$ = new Subject<void>();

  constructor() {
    this.busca$
      .pipe(debounceTime(300), takeUntilDestroyed())
      .subscribe(() => {
        this.pagina = 1;
        this.carregar();
      });
  }

  get podeEditar(): boolean {
    const nivel = this.auth.usuario()?.nivelAcesso;
    return nivel === 'ADMINISTRADOR' || nivel === 'GESTOR';
  }

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.service.listar(this.pagina, this.tamanhoPagina, this.busca.trim() || undefined).subscribe({
      next: resultado => {
        this.doadores.set(resultado.items);
        this.total.set(resultado.total);
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
    this.doadorEmEdicao.set(null);
    this.formAberto.set(true);
  }

  abrirEdicao(doador: DoadorListItem): void {
    this.doadorEmEdicao.set(doador);
    this.formAberto.set(true);
  }

  onFormSalvo(): void {
    this.formAberto.set(false);
    this.carregar();
    this.toast.add({ severity: 'success', summary: 'Salvo', detail: 'Doador salvo com sucesso.' });
  }

  confirmarRemocao(doador: DoadorListItem): void {
    this.confirm.confirm({
      message: `Deseja remover o doador ${doador.nome}?`,
      header: 'Confirmação',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Remover',
      rejectLabel: 'Cancelar',
      accept: () => this.remover(doador),
    });
  }

  formatarDocumento(documento: string): string {
    const digitos = (documento ?? '').replace(/\D/g, '');
    if (digitos.length === 11)
      return digitos.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    if (digitos.length === 14)
      return digitos.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
    return documento;
  }

  formatarTelefone(telefone: string | null): string {
    const digitos = (telefone ?? '').replace(/\D/g, '');
    if (digitos.length === 11)
      return digitos.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
    if (digitos.length === 10)
      return digitos.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
    return telefone || '—';
  }

  formatarData(data: string | null): string {
    if (!data) return '—';
    const [ano, mes, dia] = data.slice(0, 10).split('-');
    return `${dia}/${mes}/${ano}`;
  }

  private remover(doador: DoadorListItem): void {
    this.service.inativar(doador.idDoador).subscribe({
      next: () => {
        if (this.doadores().length === 1 && this.pagina > 1) this.pagina--;
        this.carregar();
        this.toast.add({ severity: 'success', summary: 'Removido', detail: 'Doador removido.' });
      },
      error: e => this.toast.add({
        severity: 'error',
        summary: 'Erro',
        detail: e?.error?.mensagem ?? 'Não foi possível remover o doador.',
      }),
    });
  }
}