import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ComunidadesService } from '../comunidades.service';
import { ComunidadeListItem } from '../models/comunidade.model';
import { AuthService } from '../../../core/services/auth.service';
import { ComunidadeFormComponent } from '../comunidade-form/comunidade-form.component';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';

@Component({
  selector: 'app-comunidades-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule,
    InputTextModule, TagModule, ToastModule, TooltipModule, ConfirmDialogModule,
    ComunidadeFormComponent,
    IconFieldModule, InputIconModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './comunidades-list.component.html',
})
export class ComunidadesListComponent implements OnInit {
  private service = inject(ComunidadesService);
  auth = inject(AuthService);
  private toast = inject(MessageService);
  private confirm = inject(ConfirmationService);

  comunidades = signal<ComunidadeListItem[]>([]);
  total = signal(0);
  carregando = signal(false);

  pagina = 1;
  tamanhoPagina = 10;
  busca = '';

  formAberto = signal(false);
  comunidadeEmEdicao = signal<ComunidadeListItem | null>(null);

  get isAdmin(): boolean {
    return this.auth.usuario()?.nivelAcesso === 'ADMINISTRADOR';
  }

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.service.listar(this.pagina, this.tamanhoPagina, this.busca || undefined).subscribe({
      next: res => {
        this.comunidades.set(res.items);
        this.total.set(res.total);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false),
    });
  }

  onBusca(): void {
    this.pagina = 1;
    this.carregar();
  }

  onPage(event: { first: number; rows: number }): void {
    this.pagina = Math.floor(event.first / event.rows) + 1;
    this.tamanhoPagina = event.rows;
    this.carregar();
  }

  abrirNovo(): void {
    this.comunidadeEmEdicao.set(null);
    this.formAberto.set(true);
  }

  abrirEdicao(comunidade: ComunidadeListItem): void {
    this.comunidadeEmEdicao.set(comunidade);
    this.formAberto.set(true);
  }

  onFormSalvo(): void {
    this.formAberto.set(false);
    this.carregar();
    this.toast.add({ severity: 'success', summary: 'Salvo', detail: 'Comunidade salva com sucesso.' });
  }

  confirmarAlternarAtivo(comunidade: ComunidadeListItem): void {
    const acao = comunidade.ativa ? 'inativar' : 'ativar';
    this.confirm.confirm({
      message: `Deseja ${acao} a comunidade ${comunidade.nome}?`,
      header: 'Confirmação',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.alternarAtivo(comunidade),
    });
  }

  private alternarAtivo(comunidade: ComunidadeListItem): void {
    this.service.alternarAtivo(comunidade.idComunidade).subscribe({
      next: () => {
        this.carregar();
        this.toast.add({ severity: 'success', summary: 'Atualizado', detail: `Comunidade ${comunidade.ativa ? 'inativada' : 'ativada'}.` });
      },
    });
  }

}
