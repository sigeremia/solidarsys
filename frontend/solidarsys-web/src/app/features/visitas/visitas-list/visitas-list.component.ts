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
import { AuthService } from '../../../core/services/auth.service';
import { VisitasService } from '../visitas.service';
import { VisitaListItem } from '../models/visita.model';
import { VisitaFormComponent } from '../visita-form/visita-form.component';

@Component({
  selector: 'app-visitas-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule,
    InputTextModule, IconFieldModule, InputIconModule,
    ToastModule, TooltipModule, ConfirmDialogModule,
    VisitaFormComponent,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './visitas-list.component.html',
})
export class VisitasListComponent implements OnInit {
  private service = inject(VisitasService);
  auth = inject(AuthService);
  private toast = inject(MessageService);
  private confirm = inject(ConfirmationService);

  visitas = signal<VisitaListItem[]>([]);
  total = signal(0);
  carregando = signal(false);

  pagina = 1;
  tamanhoPagina = 10;
  busca = '';

  formAberto = signal(false);
  visitaEmEdicao = signal<VisitaListItem | null>(null);

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
    return nivel === 'ADMINISTRADOR' || nivel === 'GESTOR' || nivel === 'OPERADOR';
  }

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
        this.visitas.set(res.items);
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
    this.visitaEmEdicao.set(null);
    this.formAberto.set(true);
  }

  abrirEdicao(visita: VisitaListItem): void {
    this.visitaEmEdicao.set(visita);
    this.formAberto.set(true);
  }

  onFormSalvo(): void {
    this.formAberto.set(false);
    this.carregar();
    this.toast.add({ severity: 'success', summary: 'Salvo', detail: 'Visita salva com sucesso.' });
  }

  confirmarRemocao(visita: VisitaListItem): void {
    this.confirm.confirm({
      message: `Deseja remover a visita de ${visita.beneficiario}?`,
      header: 'Confirmação',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Remover',
      rejectLabel: 'Cancelar',
      accept: () => this.remover(visita),
    });
  }

  private remover(visita: VisitaListItem): void {
    this.service.excluir(visita.idVisita).subscribe({
      next: () => {
        if (this.visitas().length === 1 && this.pagina > 1) this.pagina--;
        this.carregar();
        this.toast.add({ severity: 'success', summary: 'Removida', detail: 'Visita removida.' });
      },
      error: e => this.toast.add({
        severity: 'error',
        summary: 'Erro',
        detail: e?.error?.mensagem ?? 'Não foi possível remover a visita.',
      }),
    });
  }

  formatarData(data: string): string {
    if (!data) return '—';
    const valor = new Date(`${data}T00:00:00Z`);
    return new Intl.DateTimeFormat('pt-BR', { timeZone: 'UTC' }).format(valor);
  }
}
