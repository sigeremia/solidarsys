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
import { SetoresService } from '../setores.service';
import { SetoresListItem } from '../models/setores.model';
import { AuthService } from '../../../core/services/auth.service';
import { SetoresFormComponent } from '../setores-form/setores-form.component';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';

@Component({
  selector: 'app-setores-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule,
    InputTextModule, TagModule, ToastModule, TooltipModule, ConfirmDialogModule,
    SetoresFormComponent,
    IconFieldModule, InputIconModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './setores-list.component.html',
})
export class SetoresListComponent implements OnInit {
  private service = inject(SetoresService);
  auth = inject(AuthService);
  private toast = inject(MessageService);
  private confirm = inject(ConfirmationService);

  setores = signal<SetoresListItem[]>([]);
  total = signal(0);
  carregando = signal(false);

  pagina = 1;
  tamanhoPagina = 10;
  busca = '';

  formAberto = signal(false);
  setorEmEdicao = signal<SetoresListItem | null>(null);

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
        this.setores.set(res.items);
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
    this.setorEmEdicao.set(null);
    this.formAberto.set(true);
  }

  abrirEdicao(setor: SetoresListItem): void {
    this.setorEmEdicao.set(setor);
    this.formAberto.set(true);
  }

  onFormSalvo(): void {
    this.formAberto.set(false);
    this.carregar();
    this.toast.add({ severity: 'success', summary: 'Salvo', detail: 'Comunidade salva com sucesso.' });
  }

  confirmarAlternarAtivo(setor: SetoresListItem): void {
    const acao = setor.ativo ? 'inativar' : 'ativar';
    this.confirm.confirm({
      message: `Deseja ${acao} a comunidade ${setor.nome}?`,
      header: 'Confirmação',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.alternarAtivo(setor),
    });
  }

  private alternarAtivo(setor: SetoresListItem): void {
    this.service.alternarAtivo(setor.idSetores).subscribe({
      next: () => {
        this.carregar();
        this.toast.add({ severity: 'success', summary: 'Atualizado', detail: `Comunidade ${setor.ativo ? 'inativada' : 'ativada'}.` });
      },
    });
  }

}
