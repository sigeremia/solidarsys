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
import { UsuariosService } from '../usuarios.service';
import { UsuarioListItem } from '../models/usuario.model';
import { AuthService } from '../../../core/services/auth.service';
import { UsuarioFormComponent } from '../usuario-form/usuario-form.component';
import { NivelAcesso } from '../../../core/models/auth.model';

@Component({
  selector: 'app-usuarios-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule,
    InputTextModule, TagModule, ToastModule, TooltipModule, ConfirmDialogModule,
    UsuarioFormComponent,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './usuarios-list.component.html',
})
export class UsuariosListComponent implements OnInit {
  private service = inject(UsuariosService);
  auth = inject(AuthService);
  private toast = inject(MessageService);
  private confirm = inject(ConfirmationService);

  usuarios = signal<UsuarioListItem[]>([]);
  total = signal(0);
  carregando = signal(false);

  pagina = 1;
  tamanhoPagina = 10;
  busca = '';

  formAberto = signal(false);
  usuarioEmEdicao = signal<UsuarioListItem | null>(null);

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
        this.usuarios.set(res.items);
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
    this.usuarioEmEdicao.set(null);
    this.formAberto.set(true);
  }

  abrirEdicao(usuario: UsuarioListItem): void {
    this.usuarioEmEdicao.set(usuario);
    this.formAberto.set(true);
  }

  onFormSalvo(): void {
    this.formAberto.set(false);
    this.carregar();
    this.toast.add({ severity: 'success', summary: 'Salvo', detail: 'Usuário salvo com sucesso.' });
  }

  confirmarAlternarAtivo(usuario: UsuarioListItem): void {
    const acao = usuario.ativo ? 'inativar' : 'ativar';
    this.confirm.confirm({
      message: `Deseja ${acao} o usuário ${usuario.nome}?`,
      header: 'Confirmação',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.alternarAtivo(usuario),
    });
  }

  private alternarAtivo(usuario: UsuarioListItem): void {
    this.service.alternarAtivo(usuario.id).subscribe({
      next: () => {
        this.carregar();
        this.toast.add({ severity: 'success', summary: 'Atualizado', detail: `Usuário ${usuario.ativo ? 'inativado' : 'ativado'}.` });
      },
    });
  }

  nivelLabel(nivel: NivelAcesso): string {
    const map: Record<NivelAcesso, string> = {
      ADMINISTRADOR: 'Administrador',
      GESTOR: 'Gestor',
      OPERADOR: 'Operador',
      VOLUNTARIO: 'Voluntário',
    };
    return map[nivel] ?? nivel;
  }

  nivelSeverity(nivel: NivelAcesso): 'danger' | 'warn' | 'info' | 'secondary' {
    const map: Record<NivelAcesso, 'danger' | 'warn' | 'info' | 'secondary'> = {
      ADMINISTRADOR: 'danger',
      GESTOR: 'warn',
      OPERADOR: 'info',
      VOLUNTARIO: 'secondary',
    };
    return map[nivel] ?? 'secondary';
  }
}
