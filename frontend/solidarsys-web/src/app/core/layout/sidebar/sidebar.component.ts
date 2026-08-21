import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { NivelAcesso } from '../../models/auth.model';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  roles?: NivelAcesso[];
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard',     icon: 'pi-home',        route: '/dashboard' },
  { label: 'Beneficiários', icon: 'pi-users',       route: '/beneficiarios', roles: ['ADMINISTRADOR', 'GESTOR', 'OPERADOR'] },
  { label: 'Doadores',      icon: 'pi-heart',       route: '/doadores',      roles: ['ADMINISTRADOR', 'GESTOR'] },
  { label: 'Visitas',       icon: 'pi-calendar',    route: '/visitas' },
  { label: 'Comunidades',   icon: 'pi-map',         route: '/comunidades' },
  { label: 'Mapa',          icon: 'pi-map-marker',  route: '/mapa' },
  { label: 'Usuários',      icon: 'pi-user-edit',   route: '/usuarios',      roles: ['ADMINISTRADOR', 'GESTOR'] },
  { label: 'Setores',       icon: 'pi-building',    route: '/setores',       roles: ['ADMINISTRADOR'] },
  { label: 'Relatórios',    icon: 'pi-chart-bar',   route: '/relatorios',    roles: ['ADMINISTRADOR', 'GESTOR'] },
];

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
})
export class SidebarComponent {
  auth = inject(AuthService);
  mobileAberta = signal(false);

  get navItems(): NavItem[] {
    const nivel = this.auth.usuario()?.nivelAcesso;
    if (!nivel) return [];
    return NAV_ITEMS.filter(item => !item.roles || item.roles.includes(nivel));
  }

  toggleMobile(): void {
    this.mobileAberta.update(v => !v);
  }

  fecharMobile(): void {
    this.mobileAberta.set(false);
  }

  sair(): void {
    this.auth.logout().subscribe();
  }
}
