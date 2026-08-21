import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6">
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-slate-800">
          Olá, {{ (auth.usuario()?.nome?.split(' ') ?? ['Usuário'])[0] }} 👋
        </h1>
        <p class="text-slate-500 text-sm mt-0.5">Bem-vindo ao SolidarSYS</p>
      </div>

      <!-- Cards de estatísticas — placeholder para Passo 7 (gráficos) -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
        @for (card of cards; track card.label) {
          <div class="bg-white rounded-xl border border-slate-200 shadow-sm p-5">
            <div class="flex items-center justify-between mb-3">
              <span class="text-slate-500 text-sm">{{ card.label }}</span>
              <div [class]="'w-9 h-9 rounded-lg flex items-center justify-center ' + card.bg">
                <i [class]="'pi ' + card.icon + ' ' + card.color + ' text-base'"></i>
              </div>
            </div>
            <p class="text-2xl font-bold text-slate-800">—</p>
            <p class="text-xs text-slate-400 mt-1">Disponível no Passo 7</p>
          </div>
        }
      </div>

      <div class="bg-white rounded-xl border border-slate-200 shadow-sm p-6 flex items-center justify-center h-48 text-slate-400">
        <div class="text-center">
          <i class="pi pi-chart-bar text-4xl mb-3 block"></i>
          <p class="font-medium">Gráficos em construção</p>
          <p class="text-sm">Serão implementados no Passo 7 — Relatórios e Dashboard</p>
        </div>
      </div>
    </div>
  `,
})
export class DashboardComponent {
  auth = inject(AuthService);

  cards = [
    { label: 'Beneficiários', icon: 'pi-users',      bg: 'bg-blue-50',   color: 'text-blue-600' },
    { label: 'Doadores',      icon: 'pi-heart',      bg: 'bg-pink-50',   color: 'text-pink-600' },
    { label: 'Visitas',       icon: 'pi-calendar',   bg: 'bg-amber-50',  color: 'text-amber-600' },
    { label: 'Usuários',      icon: 'pi-user-edit',  bg: 'bg-teal-50',   color: 'text-teal-600' },
  ];
}
